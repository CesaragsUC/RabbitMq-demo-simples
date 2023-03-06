using RabbitMQ.Client;

namespace Rabbit.Exchanges.RabbitMQConfig.Basic
{
    public interface IRabbitManager
    {
        void SendMessage<T>(T message, string exchangeName, string exchangeType, string queueName, string routeKey) where T : class;
        void ReceivingMessage(string queueName, bool autoAck = true);
        void BindFila(string queueName, string exchangeType, string exchangeName, string routeKey);
        public void RemoverFila(string queueName);
        public void RemoverExchange(string exchangeName);
    }

}
