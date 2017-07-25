using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using RadbitMqDemo.DataTransfer;
using RadbitMqDemo.Enum;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;

namespace RadbitMqDemo.Global
{
    public class RabbitMqServiceTaskSenderBase
    {
        //public static void SendMessage(string message)
        //{
        //    ConnectionFactory connf = new ConnectionFactory();
        //    connf.HostName = (FunctionBase.GetConfiguration(QueueEnum.QueueHost));
        //    connf.UserName = FunctionBase.GetConfiguration(QueueEnum.QueueUserName);
        //    connf.Password = FunctionBase.GetConfiguration(QueueEnum.QueuePassword);
        //    //connf.
        //    using (RabbitMqMessageFactory f = new RabbitMqMessageFactory(connf))
        //        //using (var mqClient = rabbitService.MqServer.CreateMessageQueueClient())
        //    using (var mqClient = f.CreateMessageProducer())
        //    {
        //        //IMessage messageObj = new Message();
        //        //messageObj.Body = new RabbitServiceTaskMessageData
        //        //{
        //        //    Message = message
        //        //};
        //        //mqClient.Publish(FunctionBase.GetConfiguration(QueueEnum.QueueName), messageObj);

        //        var t = new RabbitServiceTaskMessageData
        //        {
        //            Message = message
        //        };
                
        //        mqClient.Publish<RabbitServiceTaskMessageData>(t);

        //        ConsoleBase.PrintSendMessage(message);
        //    }
            
        //}
        private static int Couter;
        public static void SendMessage(string messageText)
        {
            Couter++;
            ConnectionFactory factory = new ConnectionFactory
            {
                HostName = FunctionBase.GetConfiguration(QueueEnum.QueueHost),
                UserName = FunctionBase.GetConfiguration(QueueEnum.QueueUserName),
                Password = FunctionBase.GetConfiguration(QueueEnum.QueuePassword),
                Port = int.Parse(FunctionBase.GetConfiguration(QueueEnum.QueuePort)),
                VirtualHost = FunctionBase.GetConfiguration(QueueEnum.QueueVirtualHost)
            };
            using (RabbitMqMessageFactory rabbitMqServer = new RabbitMqMessageFactory(factory))
            {
                IMessageProducer messageProducer = rabbitMqServer.CreateMessageProducer();
                string t = $"{messageText} - {Couter}";
                int p = Couter;
                IMessage message = new Message
                {
                    Body = new RequestData { Function = "Authenticate", Data = t }
                };
                message.Priority = p;
                t += " Priority: " + p;
                ConsoleBase.PrintSendMessage(t);
                messageProducer.Publish(message);
            }

            
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
                    ContentType = ContentEnum.Xml,
                    ContentEncoding = "UTF-8",
                    AppId = Guid.NewGuid().ToString(),
                    MessageId = Guid.NewGuid().ToString(),
                    Expiration = "36000000"
                };
            }
        }
    }
}
