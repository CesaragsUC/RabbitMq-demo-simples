// See https://aka.ms/new-console-template for more information
using Rabtimq_HelloWorld;

RabitConnection.Connect();

//Producer.CreateMessage();
//Consumer.ReceiveMessage();

Exchange.BingingComExchange();

Console.ReadKey();
