using RabbitMQ.Client;

namespace Rabbit.EventBus.Infra.Connection
{
    public interface IPersistentConnection
    {
        event EventHandler OnReconectadoAposFalhaConexao;
        bool IsConnected { get; }

        bool TryConnect();
        IModel CreateModel();
    }
}
