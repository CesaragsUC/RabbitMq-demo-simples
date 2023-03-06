using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using Rabbit.Subscription.Domain;
using Rabbit.Subscription.Interfaces;
using RabbitAppContext;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Rabbit.Exchanges.RabbitMQConfig.Basic
{
    public class RabbitManager : IRabbitManager
    {
        private readonly DefaultObjectPool<IModel> _objectPool;
        private readonly IServiceProvider _serviceProvider;

        public RabbitManager(IPooledObjectPolicy<IModel> objectPolicy,
            IServiceProvider serviceProvider)
        {
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
            _serviceProvider = serviceProvider;
        }


        public void SendMessage<T>(T message, string exchangeName, string exchangeType, string queueName, string routeKey) where T : class
        {
            if (message == null)
                return;

            var channel = _objectPool.Get();

            try
            {
                channel.ExchangeDeclare(exchangeName, exchangeType, true, false, null);

                var sendBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchangeName, routeKey, properties, sendBytes);

                channel.QueueDeclare(queue: queueName,
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

                channel.QueueBind(queue: queueName,
                        exchange: exchangeName,
                        routingKey: routeKey);


            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _objectPool.Return(channel);
            }
        }

        public void ReceivingMessage(string queueName, bool autoAck = true)
        {
            var channel = _objectPool.Get();
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                var cliente = JsonConvert.DeserializeObject<Cliente>(message);
                cliente.Ativo = true;
                cliente.UltimaAtualizacao = DateTime.Now;

                using (var scope = _serviceProvider.CreateScope())
                {
                    var clienteRepository = scope.ServiceProvider.GetRequiredService<IClienteRepository>();
                    clienteRepository.Update(cliente);
                }


                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("[+] Mensagem Recebida");
                Console.WriteLine($"Resut: {message}");
                Console.ResetColor();

            };

            channel.BasicConsume(queue: queueName,
                     autoAck: autoAck,
                     consumer: consumer);
        }

        public void BindFila(string queueName, string exchangeType, string exchangeName, string routeKey)
        {
            var channel = _objectPool.Get();

            try
            {
                //cria Exchange
                channel.ExchangeDeclare(exchangeName, exchangeType, true, false, null);

                //Cria uma Fila
                channel.QueueDeclare(queue: queueName,
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                //Faz o Bind da Fila com a RoutingKey na Exchange criada
                channel.QueueBind(
                        queue: queueName,
                        exchange: exchangeName,
                        routingKey: routeKey);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void RemoverFila(string queueName)
        {
            var channel = _objectPool.Get();

            try
            {

                channel.QueueDelete(queueName);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void RemoverExchange(string exchangeName)
        {
            var channel = _objectPool.Get();

            try
            {
                channel.ExchangeDelete(exchangeName);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
