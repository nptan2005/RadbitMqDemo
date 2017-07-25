using System;
using RadbitMqDemo.Enum;
using RadbitMqDemo.Global;
using RadbitMqDemo.Service;
using ServiceStack;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;

namespace RadbitMqDemo
{
    class Program
    {
        private const string TestMessage = "Hello Work!!!";
        private static int i = 0;
        //static void Main(string[] args)
        //{
        //    StartSend:
        //    i++;
        //    RabbitMqServiceTaskSenderBase.SendMessage($"{TestMessage} - {i}");
        //    Console.ResetColor();
        //    Console.WriteLine(" -----------------------------");
        //    Console.WriteLine(" [x] Press Enter to continue ! ");
        //    Console.WriteLine(" [x] Press Tab Send 10,000 Msg!");
        //    Console.WriteLine(" -----------------------------");
        //    if (Console.ReadKey().Key == ConsoleKey.Enter)
        //    {
        //        goto StartSend;
        //    }
        //}

        static void Main(string[] args)
        {


            //while (true)
            //{
            //    RabbitMqSenderBase.SendMessage(TestMessage);
            //}


            RabbitMqReceiveBase receive = new RabbitMqReceiveBase();
            StartSend:
            //RabbitMqSenderBase.SendMessage(TestMessage);
            RabbitMqServiceTaskSenderBase.SendMessage(TestMessage);
            Console.ResetColor();
            Console.WriteLine(" -----------------------------");
            Console.WriteLine(" [x] Press Enter to continue ! ");
            Console.WriteLine(" [x] Press Tab Send 10,000 Msg!");
            Console.WriteLine(" -----------------------------");
            if (Console.ReadKey().Key == ConsoleKey.Enter)
            {
                goto StartSend;
            }
            if (Console.ReadKey().Key == ConsoleKey.Tab)
            {
                for (int i = 0; i < 10000; i++)
                {
                    //RabbitMqSenderBase.SendMessage(TestMessage);
                    RabbitMqServiceTaskSenderBase.SendMessage(TestMessage);
                }
            }

            RabbitMqReceiveBase.TryStop();
        }

    }
}
