using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;
using Rabbit.Subscription.Domain;
using Rabbit.Subscription.Interfaces;
using RabbitAppContext;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Rabbit.Exchanges.Integration
{

    public class ClienteIntegration : BackgroundService
    {
        private readonly DefaultObjectPool<IModel> _objectPool;
        private readonly IServiceProvider _serviceProvider;

        public ClienteIntegration(IPooledObjectPolicy<IModel> objectPolicy,
            IServiceProvider serviceProvider)
        {
            _objectPool = new DefaultObjectPool<IModel>(objectPolicy, Environment.ProcessorCount * 2);
            _serviceProvider = serviceProvider;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                ClienteUpdateIntegration();
               
            }
        }
        /// <summary>
        /// Quando cadastramos um cliente, por padrao fica Ativo =  false.
        /// Aqui fazemos um update simples, Ativo = true e UltimaAtualizacao = UltimaAtualizacao = DateTime.Now.
        /// somente para didatica.
        /// </summary>
        public void ClienteUpdateIntegration()
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

            channel.BasicConsume(queue: "FilaCliente",
                     autoAck: true,
                     consumer: consumer);
        }
    }
}
