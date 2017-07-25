using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RadbitMqDemo.DataTransfer;
using RadbitMqDemo.Enum;

namespace RadbitMqDemo.Global
{
    class RabbitMqReceiveBase
    {
        private static readonly string QueueName = FunctionBase.GetConfiguration(QueueEnum.QueueName);
        private static readonly IConnection Connection;
        private static readonly IModel Model;
  
        static RabbitMqReceiveBase()
        {
            ConnectionFactory receiveFactory = new ConnectionFactory
            {
                HostName = FunctionBase.GetConfiguration(QueueEnum.QueueHost),
                UserName = FunctionBase.GetConfiguration(QueueEnum.QueueUserName),
                Password = FunctionBase.GetConfiguration(QueueEnum.QueuePassword),
                Port = int.Parse(FunctionBase.GetConfiguration(QueueEnum.QueuePort))
            };
            Connection = receiveFactory.CreateConnection("DemoReceiveRabbitMq");

            Model = Connection.CreateModel();
            Model.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false,
                arguments: null);

            EventingBasicConsumer consumer = new EventingBasicConsumer(Model);
            consumer.ConsumerTag = "DemoReceiveRabbitMqConsumerTag";
            consumer.Received += AutoReceive;
            Model.BasicConsume(queue: QueueName, noAck: true, consumer: consumer);
        }
       

        private static string GetTextBody(byte[] body, IBasicProperties properties)
        {
            if (properties.ContentType == ContentEnum.Json || properties.ContentType == ContentEnum.Xml)
            {
                string bodyJson = Encoding.UTF8.GetString(body);
                MessageData data = FunctionBase.Deserialize<MessageData>(bodyJson, properties.ContentType);
                return data.Message;
            }
            return Encoding.UTF8.GetString(body);
        }
        private static void AutoReceive(object model, BasicDeliverEventArgs ea)
        {
            try
            {
                ConsoleBase.PrintReceiveMessage(GetTextBody(ea.Body,ea.BasicProperties));
                //Thread.Sleep(5000);
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine(e);
                throw;
            }
        }

        public static bool TryStop()
        {
            Model?.Close();
            Model?.Dispose();
            Connection?.Close();
            Connection?.Dispose();

            return true;
        }
    }
}
