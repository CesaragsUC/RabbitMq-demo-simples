using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabbit.EventBus.Config.Bus;
using Rabbit.EventBus.Config.Subscription;
using Rabbit.EventBus.Infra.Bus;
using Rabbit.EventBus.Infra.Connection;
using Rabbit.EventBus.Infra.Extensions;
using RabbitMQ.Client;

namespace Rabbit.EventBus.Infra.Config
{
    public static class RabbitEventBusConfig
    {
        /// <summary>
        /// Adds an event bus that uses RabbitMQ to deliver messages.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionUrl">URL para se conectar ao RabbitMQ.</param>
        /// <param name="brokerName">Broker name. Isso representa o nome da Exchange.</param>
        /// <param name="queueName">Messa queue name, to track on RabbitMQ.</param>
        /// <param name="timeoutBeforeReconnecting">A quantidade de tempo em segundos que o aplicativo aguardará após tentar se reconectar ao RabbitMQ.</param>
        public static void AddRabbitMQEventBus(this IServiceCollection services, string connectionUrl, string brokerName, string queueName, int timeoutBeforeReconnecting = 15)
        {
            services.AddSingleton<IEventBusSubscriptionManager, InMemoryEventBusSubscriptionManager>();
            services.AddSingleton<IPersistentConnection, RabbitMQPersistentConnection>(factory =>
            {
                var connectionFactory = new ConnectionFactory
                {
                    Uri = new Uri(connectionUrl),
                    DispatchConsumersAsync = true,
                };

                var logger = factory.GetService<ILogger<RabbitMQPersistentConnection>>();
                return new RabbitMQPersistentConnection(connectionFactory, logger, timeoutBeforeReconnecting);
            });

            services.AddSingleton<IEventBus, RabbitMQEventBus>(factory =>
            {
                var persistentConnection = factory.GetService<IPersistentConnection>();
                var subscriptionManager = factory.GetService<IEventBusSubscriptionManager>();
                var logger = factory.GetService<ILogger<RabbitMQEventBus>>();

                return new RabbitMQEventBus(persistentConnection, subscriptionManager, factory, logger, brokerName, queueName);
            });
        }
    }
}
