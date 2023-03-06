using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Rabtimq_HelloWorld
{
    public static class RabitConnection
    {
        public static IConnection Connection { get; set; }
        public static IModel Channel { get; set; }

        public static IModel Connect()
        {
            Console.ForegroundColor= ConsoleColor.Green;
            Console.WriteLine("Crating a connection with Rabitmq...");
            try
            {
                var factory = new ConnectionFactory { HostName = "localhost" };
                var connection = factory.CreateConnection("Helo-World"); //cria uma conexao
                var channel = connection.CreateModel(); //cria um canal
                Connection = connection;
                Channel = channel;

                channel.QueueDeclare(
                        queue: "hello",
                        durable: false, 
                        exclusive: false, 
                        autoDelete: false, 
                        arguments: null);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Connection Created!");
                Console.ResetColor();

                return channel;
            }
            catch (Exception ex )
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A error occour while create a connection!");
                Console.ResetColor();
                throw;
            }



        }
    }
}
