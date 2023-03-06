using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabtimq_HelloWorld
{
    public static class Exchange
    {
        /// <summary>
        /// Exemplo de como criar uma Exchange com nome e fazer bing com uma fila atraves de uma routing key
        /// </summary>
        public static void BingingComExchange()
        {
            const string exchaneName = "logs";

            var channel = RabitConnection.Channel;

            channel.ExchangeDeclare(exchange: exchaneName, type: ExchangeType.Fanout);

            var queueName = channel.QueueDeclare().QueueName;

            channel.QueueBind(queue:queueName,
                            exchange: exchaneName,
                            routingKey:string.Empty);

            Console.WriteLine("[*] Esperando pelos Logs...");

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] {0}", message);
            };

            channel.BasicConsume(queue: queueName,
                     autoAck: true,
                     consumer: consumer);
        }
    }
}
