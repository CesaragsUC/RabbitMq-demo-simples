using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;

namespace Rabbit.EventBus.Infra.Connection
{
    public class RabbitMQPersistentConnection : IPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly TimeSpan _intervaloAntesDeReconectar;
        private readonly ILogger<RabbitMQPersistentConnection> _logger;
        private IConnection _connection;
        private bool _disposed;
        private readonly object _locker = new object();
        private bool _connectionFailed = false;

        public RabbitMQPersistentConnection(IConnectionFactory connectionFactory,
            ILogger<RabbitMQPersistentConnection> logger,
            int intervaloAntesDeReconectar = 15)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _logger = logger;
            _intervaloAntesDeReconectar = TimeSpan.FromSeconds(intervaloAntesDeReconectar);
        }


        public bool IsConnected
        {
            get
            {
                return (_connection != null) && (_connection.IsOpen) && (!_disposed); 
            }
        }

        public event EventHandler OnReconectadoAposFalhaConexao;

        public IModel CreateModel()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Nenhuma conexão RabbitMQ está disponível para executar esta ação.");
            
            return _connection.CreateModel();
        }

        public bool TryConnect()
        {
             _logger.LogInformation("Tentando se conectar ao RabbitMQ...");

            //sobre lock: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock
            lock (_locker)
            {
                //Cria uma política para tentar se conectar novamente ao agente de mensagens até que seja bem-sucedido.
                var policy = Policy.Handle<SocketException>()
                             .Or<BrokerUnreachableException>()
                             .WaitAndRetryForever((duracao) => _intervaloAntesDeReconectar, (ex, time) =>
                             {
                                 _logger.LogWarning("O cliente RabbitMQ não pôde se conectar após {TimeOut} segundos ({ExceptionMessage}). Esperando para tentar novamente...", $"{(int)time.TotalSeconds}", ex.Message);
                             });

                policy.Execute(() => {
                    _connection = _connectionFactory.CreateConnection();
                });

                if(!IsConnected)
                {
                    _logger.LogCritical("ERROR: não foi possivel se conectar ao RabbitMQ.");
                    _connectionFailed = true;
                    return false;
                }

                //Esses manipuladores de eventos lidam com situações em que a conexão é perdida por qualquer motivo. Eles tentam reconectar o cliente.
                _connection.ConnectionShutdown += OnConexaoEncerrada;
                _connection.CallbackException += OnCallbackException;
                _connection.ConnectionBlocked += OnConexaoBloqueada;
                _connection.ConnectionUnblocked += OnConexaoDesbloqueada;

                _logger.LogInformation("O cliente RabbitMQ adquiriu uma conexão persistente com '{HostName}' e está inscrito em eventos de falha", _connection.Endpoint.HostName);

                // Se a conexão falhou anteriormente devido a um desligamento do RabbitMQ ou algo semelhante, precisamos garantir que a Exchange e as Filas existam novamente.
                // Também é necessário religar todos os manipuladores(handles) de eventos do aplicativo. Usamos este manipulador de eventos abaixo para fazer isso.

                if(_connectionFailed)
                {
                    OnReconectadoAposFalhaConexao?.Invoke(this, null);
                    _connectionFailed = false;
                }

                return true;
            }
        }

        public void Dispose() 
        { 
            if(_disposed)
                return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
               _logger.LogCritical(ex.ToString());
            }
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs args)
        {
            _connectionFailed = true;

            _logger.LogWarning("Uma exceção lançada de conexão RabbitMQ. Tentando reconectar...");
            TryConnectIfNotDisposed();
        }

        private void OnConexaoEncerrada(object sender, ShutdownEventArgs args)
        {
            _connectionFailed = true;

            _logger.LogWarning("Uma conexão RabbitMQ está desligada. Tentando reconectar...");
            TryConnectIfNotDisposed();
        }

        private void OnConexaoBloqueada(object sender, ConnectionBlockedEventArgs args)
        {
            _connectionFailed = true;

            _logger.LogWarning("Uma conexão RabbitMQ está bloqueada. Tentando reconectar...");
            TryConnectIfNotDisposed();
        }

        private void OnConexaoDesbloqueada(object sender, EventArgs args)
        {
            _connectionFailed = true;

            _logger.LogWarning("Uma conexão RabbitMQ está desbloqueada. Tentando reconectar...");
            TryConnectIfNotDisposed();
        }

        private void TryConnectIfNotDisposed()
        {
            if (_disposed)
            {
                _logger.LogInformation("O cliente RabbitMQ está descartado. Nenhuma ação será tomada.");
                return;
            }

            TryConnect();
        }
    }
}
