using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabtimq_HelloWorld
{
    public static class Producer
    {
        public static void CreateMessage()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Criando mensagens");

            var channel = RabitConnection.Channel;
            const string message = "Estou aprendendo RabbitMq";

            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(
                    exchange: string.Empty,
                    mandatory:false, 
                    routingKey:"hello",
                    basicProperties:null,
                    body:body);

            
            Console.WriteLine("Mensagem enviada!");
            Console.WriteLine("Pressione qualquer tecla para continuar.");
            Console.ReadLine();

            Console.ResetColor();

        }


    }
}
