using System;

namespace RadbitMqDemo.Global
{
    public class ConsoleBase
    {
        public static void PrintSendMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(" [-->] Sent:     '{0}'", message);
        }
        public static void PrintReceiveMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(" [<--] Received: '{0}'", message);
        }
    }
}
