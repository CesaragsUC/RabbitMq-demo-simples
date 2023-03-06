using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabtimq_HelloWorld
{
    public static class Consumer
    {
        public static void ReceiveMessage()
        {
         
            var channel =  RabitConnection.Channel;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Esperando por mensagens...");

            var consumidor = new EventingBasicConsumer(channel);

            consumidor.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);


               
                Console.WriteLine("[+] Mensagem Recebida");
                Console.WriteLine("Pressione qualquer tecla para sair.");
                Console.ReadLine();
                Console.ResetColor();

            };


            channel.BasicConsume(
                    queue:"hello",
                    autoAck:true,
                    consumerTag: string.Empty,
                    noLocal: false,
                    exclusive:false,
                    arguments:null,
                    consumer:consumidor);


        }
    }
}
