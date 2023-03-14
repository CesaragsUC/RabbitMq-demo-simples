using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.AutoMock;
using Rabbit.EventBus.Infra.Connection;
using Rabbit.Subscription.Demo;
using RabbitMQ.Client;
using System.Configuration;
using Xunit;

namespace Rabbit.Test
{
    [TestClass]
    public class RabbitMQConnectionTest
    {
        private Mock<ILogger<RabbitMQPersistentConnection>> _logger;
        private string _config;

        private void IniciarMocks()
        {
            _logger = new Mock<ILogger<RabbitMQPersistentConnection>>();
            _config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("RabbitMQ")["ConnectionUrl"];
        }

        public RabbitMQConnectionTest()
        {
            IniciarMocks();
        }

        [Fact(DisplayName = "Realizar Conexao com sucesso RabbitMQ")]
        [Trait("RabbitMQ","Teste DeConexao")]
        public async void Connectar_Ao_RabbitMQ_Deve_Retornar_OK()
        {
          
            var connectionFactory = new ConnectionFactory
            {
                Uri = new Uri(_config),
                DispatchConsumersAsync = true,
            };

            var _rabbit = new RabbitMQPersistentConnection(connectionFactory, _logger.Object);
            var connected = _rabbit.TryConnect();
            Xunit.Assert.True(connected); 

        }

    }
}