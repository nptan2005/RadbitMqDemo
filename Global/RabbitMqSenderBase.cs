using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using RadbitMqDemo.DataTransfer;
using RadbitMqDemo.Enum;

namespace RadbitMqDemo.Global
{
    class RabbitMqSenderBase
    {
        private static int _couter;
        private static int _errCounter = 0;

        private static readonly string QueueName = FunctionBase.GetConfiguration(QueueEnum.QueueName);
        private static readonly ConnectionFactory Factory;

        private static readonly IConnection Tconnection;
        private static readonly IModel Tchannel;
        static RabbitMqSenderBase()
        {
            Factory = new ConnectionFactory
            {
                HostName = FunctionBase.GetConfiguration(QueueEnum.QueueHost),
                UserName = FunctionBase.GetConfiguration(QueueEnum.QueueUserName),
                Password = FunctionBase.GetConfiguration(QueueEnum.QueuePassword),
                Port = int.Parse(FunctionBase.GetConfiguration(QueueEnum.QueuePort)),
                VirtualHost = FunctionBase.GetConfiguration(QueueEnum.QueueVirtualHost)
            };
            
            Tconnection = Factory.CreateConnection("DemoSendRabbitMq");
            
            Tchannel = Tconnection.CreateModel();
            
            Tchannel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        private static IBasicProperties Properties
        {
            get
            {
                //Header
                var authInfo = $"{FunctionBase.GetConfiguration(QueueEnum.QueueUserName)}:" +
                               $"{FunctionBase.GetConfiguration(QueueEnum.QueuePassword)}";
                authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

                IDictionary<string, object> header = new ConcurrentDictionary<string, object>();
                header.Add("Authorization", "Basic " + authInfo);

                return new BasicProperties
                {
                    Headers = header,
                    ContentType = GetContentType,
                    ContentEncoding = "UTF-8",
                    AppId = Guid.NewGuid().ToString(),
                    MessageId = Guid.NewGuid().ToString(),
                    Expiration = "36000000"
                };
            }
        }

        private static byte[] MessageBody(string message)
        {
            string messageText = $"{message} - {_couter} - {GetContentType} | ErrCounter: {_errCounter}";
            MessageData msgObj = new MessageData { Message = messageText };
            ConsoleBase.PrintSendMessage(messageText);
            return Encoding.UTF8.GetBytes(FunctionBase.Serialize(msgObj, GetContentType));
        }

        private static string GetContentType =>  _couter % 2 == 0 ? ContentEnum.Json : ContentEnum.Xml;
        public static void SendMessage(string message)
        {
            Tchannel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: Properties, body: MessageBody(message));
            /*
            _couter++;
            IConnection connection = null;
            IModel channel = null;
            
            try
            {
                connection = Factory.CreateConnection("DemoSendRabbitMq");
                channel = connection.CreateModel();

                channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicPublish(exchange: "", routingKey: QueueName, basicProperties: Properties, body: MessageBody(message));

            }
            catch (Exception e)
            {
                _errCounter++;
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("-------------------------");
                Console.WriteLine(e);
                Console.WriteLine("-------------------------");
            }
            finally
            {
                channel?.Close();
                channel?.Dispose();

                connection?.Close();
                connection?.Dispose();
            }
            */
        }
    }
}
