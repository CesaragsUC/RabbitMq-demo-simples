using Microsoft.Extensions.Logging;
using Polly;
using Rabbit.EventBus.Config.Bus;
using Rabbit.EventBus.Config.Eventos;
using Rabbit.EventBus.Config.Subscription;
using Rabbit.EventBus.Infra.Connection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using System.Text.Json;
using System.Text;
using System.Reflection;

namespace Rabbit.EventBus.Infra.Bus
{
    /// <summary>
    /// Implementação de barramento de eventos que usa RabbitMQ como intermediário de mensagem.
    /// A implementação é baseada em eShopOnContainers (tutorial da Microsoft sobre microsserviços no .NET Core), mas implementa alguns recursos que descobri que são baseados em diferentes bibliotecas.
    /// 
    /// References:
    /// - https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications
    /// - https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/rabbitmq-event-bus-development-test-environment
    /// - https://github.com/ojdev/RabbitMQ.EventBus.AspNetCore
    /// </summary>
    public class RabbitMQEventBus : IEventBus
    {
        private readonly string _exchangeName;
        private readonly string _queueName;
        private readonly int _contatorTentativasPublicacao = 5;
        private readonly TimeSpan _subscribeRetryTime = TimeSpan.FromSeconds(5);

        private readonly IPersistentConnection _persistentConnection;
        private readonly IEventBusSubscriptionManager _subscriptionsManager;
        private readonly IServiceProvider _serviceProvider;

        private readonly ILogger<RabbitMQEventBus> _logger;

        private IModel _consumerChannel;

        public RabbitMQEventBus(
            IPersistentConnection persistentConnection,
            IEventBusSubscriptionManager subscriptionsManager,
            IServiceProvider serviceProvider,
            ILogger<RabbitMQEventBus> logger,
            string brokerName,
            string queueName)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _subscriptionsManager = subscriptionsManager ?? throw new ArgumentNullException(nameof(subscriptionsManager));
            _serviceProvider = serviceProvider;
            _logger = logger;
            _exchangeName = brokerName ?? throw new ArgumentNullException(nameof(brokerName));
            _queueName = queueName ?? throw new ArgumentNullException(nameof(queueName));

            ConfigureMessageBroker();
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : Event
        {
            //Se nao esta connectado, tenteo conectar
            if(_persistentConnection.IsConnected)
                _persistentConnection.TryConnect();

            var policy = Policy
                    .Handle<BrokerUnreachableException>()
                    .Or<SocketException>()
                    .WaitAndRetry(_contatorTentativasPublicacao, tentarNovamente => TimeSpan.FromSeconds(Math.Pow(2, tentarNovamente)), (exception, timeSpan) =>
                    {
                        _logger.LogWarning(exception, "Não foi possível publicar o evento #{EventId} após {Timeout} segundos: {ExceptionMessage}", @event.Id, $"{timeSpan.TotalSeconds:n1}", exception.Message);
                    });

            //pega nome do evento
            var eventName = @event.GetType().Name;

            _logger.LogTrace("Criando canal RabbitMQ para publicar o evento #{EventId} ({EventName})...", @event.Id, eventName);

            using (var channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace("Declarando a troca RabbitMQ para publicar o evento #{EventId}...", @event.Id);

                channel.ExchangeDeclare(exchange: _exchangeName, type: "direct");

                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = (byte)DeliveryMode.Persistent;

                    _logger.LogTrace("Publicando evento no RabbitMQ com ID #{EventId}...", @event.Id);

                    channel.BasicPublish(
                        exchange: _exchangeName,
                        routingKey: eventName,
                        mandatory: true,
                        basicProperties: properties,
                        body: body);

                    _logger.LogTrace("Evento publicado com ID #{EventId}.", @event.Id);
                });
            }
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = _subscriptionsManager.GetEventIdentifier<TEvent>();
            var eventHandlerName = typeof(TEventHandler).Name;

            AddQueueBindForEventSubscription(eventName);

            _logger.LogInformation("Inscrevendo-se no evento {EventName} com {EventHandler}...", eventName, eventHandlerName);

            _subscriptionsManager.AddSubscription<TEvent, TEventHandler>();
            StartBasicConsume();

            _logger.LogInformation("Inscrito no evento {EventName} com {EvenHandler}.", eventName, eventHandlerName);
        }

        public void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : Event
            where TEventHandler : IEventHandler<TEvent>
        {
            var eventName = _subscriptionsManager.GetEventIdentifier<TEvent>();

            _logger.LogInformation("Cancelando inscrição no evento {EventName}...", eventName);

            _subscriptionsManager.RemoveSubscription<TEvent, TEventHandler>();

            _logger.LogInformation("Inscrição cancelada no evento {EventName}.", eventName);
        }

        private void ConfigureMessageBroker()
        {
            _consumerChannel = CreateConsumerChannel();
            _subscriptionsManager.OnEventRemoved += SubscriptionManager_OnEventRemoved;
            _persistentConnection.OnReconectadoAposFalhaConexao += PersistentConnection_OnReconectadoAposFalhaConexao;
        }
        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _logger.LogTrace("Criando canal do consumidor RabbitMQ...");


            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(exchange: _exchangeName, type: "direct");
            channel.QueueDeclare
            (
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recriando o canal do consumidor RabbitMQ...");
                DoCreateConsumerChannel();
            };

            _logger.LogTrace("Criação do canal de consumo RabbitMQ.");


            return channel;
        }
        private void StartBasicConsume()
        {
            _logger.LogTrace("Iniciando o consumo básico do RabbitMQ..");

            if (_consumerChannel == null)
            {
                _logger.LogError("Não foi possível iniciar o consumo básico porque o canal do consumidor é nulo.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            consumer.Received += Consumer_Received;

            _consumerChannel.BasicConsume
            (
                queue: _queueName,
                autoAck: false,
                consumer: consumer
            );

            _logger.LogTrace("Consumo básico do RabbitMQ iniciado.");
        }
        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

            bool isAcknowledged = false;

            try
            {
                await ProcessarEvento(eventName, message);

                _consumerChannel.BasicAck(eventArgs.DeliveryTag, multiple: false);
                isAcknowledged = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao processar a seguinte mensagem: {Message}.", message);
            }
            finally
            {
                if (!isAcknowledged)
                {
                    await TryEnqueueMessageAgainAsync(eventArgs);
                }
            }
        }
        private async Task TryEnqueueMessageAgainAsync(BasicDeliverEventArgs eventArgs)
        {
            try
            {
                _logger.LogWarning("Adicionando mensagem à fila novamente com {Tempo} segundos de atraso...", $"{_subscribeRetryTime.TotalSeconds:n1}");

                await Task.Delay(_subscribeRetryTime);
                _consumerChannel.BasicNack(eventArgs.DeliveryTag, false, true);

                _logger.LogTrace("Mensagem adicionada à fila novamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Não foi possível colocar a mensagem na fila novamente: {Error}", ex.Message);
            }
        }

        private async Task ProcessarEvento(string eventName, string message)
        {
            _logger.LogTrace("Processando evento RabbitMQ: {EventName}...", eventName);

            if (!_subscriptionsManager.HasSubscriptionsForEvent(eventName))
            {
                _logger.LogTrace("Não há inscrições para este evento.");
                return;
            }

            var subscriptions = _subscriptionsManager.GetHandlersForEvent(eventName);
            foreach (var subscription in subscriptions)
            {
                var handler = _serviceProvider.GetService(subscription.HandlerType);
                if (handler == null)
                {
                    _logger.LogWarning("Não há manipuladores para o seguinte evento: {EventName}", eventName);
                    continue;
                }

                var eventType = _subscriptionsManager.GetEventTypeByName(eventName);

                var @event = JsonSerializer.Deserialize(message, eventType);
                var eventHandlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

                // Faz com que o método retorne imediatamente
                await Task.Yield();

                // Este código vai ser executado no futuro
                await (Task)eventHandlerType.GetMethod(nameof(IEventHandler<Event>.HandleAsync)).Invoke(handler, new object[] { @event });
            }

            _logger.LogTrace("Evento processado {EventName}.", eventName);
        }

        private void SubscriptionManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(queue: _queueName, exchange: _exchangeName, routingKey: eventName);

                if (_subscriptionsManager.IsEmpty)
                {
                    _consumerChannel.Close();
                }
            }
        }

        private void AddQueueBindForEventSubscription(string eventName)
        {
            var containsKey = _subscriptionsManager.HasSubscriptionsForEvent(eventName);
            if (containsKey)
            {
                return;
            }

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueBind(queue: _queueName, exchange: _exchangeName, routingKey: eventName);
            }
        }

        private void PersistentConnection_OnReconectadoAposFalhaConexao(object sender, EventArgs e)
        {
            DoCreateConsumerChannel();
            RecriarSubscricao();
        }
        private void DoCreateConsumerChannel()
        {
            _consumerChannel.Dispose();
            _consumerChannel = CreateConsumerChannel();
            StartBasicConsume();
        }
        private void RecriarSubscricao()
        {
            var subscriptions = _subscriptionsManager.GetAllSubscriptions();
            _subscriptionsManager.Clear();

            Type eventBusType = this.GetType();
            MethodInfo genericSubscribe;

            foreach (var entry in subscriptions)
            {
                foreach (var subscription in entry.Value)
                {
                    genericSubscribe = eventBusType.GetMethod("Subscribe").MakeGenericMethod(subscription.EventType, subscription.HandlerType);
                    genericSubscribe.Invoke(this, null);
                }
            }
        }
    }
}