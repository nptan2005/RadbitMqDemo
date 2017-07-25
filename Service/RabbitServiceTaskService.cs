using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using MongoDB.Driver.Core.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using RadbitMqDemo.DataTransfer;
using RadbitMqDemo.Enum;
using RadbitMqDemo.Global;
using ServiceStack;
using ServiceStack.Messaging;
using ServiceStack.RabbitMq;

namespace RadbitMqDemo.Service
{
    public class RabbitServiceTaskService
    {
        public RabbitMqServer MqServer { private set; get; }
        
        public RabbitServiceTaskService()
        {
            MqServer = new RabbitMqServer(FunctionBase.GetConfiguration(QueueEnum.QueueHost),
                FunctionBase.GetConfiguration(QueueEnum.QueueUserName),
                FunctionBase.GetConfiguration(QueueEnum.QueuePassword))
            {
                PublishMessageFilter = (queueName, properties, msg) => {
                    properties.AppId = "app:{0}".Fmt(queueName);
                    //ConsoleBase.PrintReceiveMessage(msg.Body.ToString());
                },
                //GetMessageFilter = (queueName, basicMsg) => {
                //    IBasicProperties props = basicMsg.BasicProperties;
                //    receivedMsgType = props.Type; //automatically added by RabbitMqProducer
                //    receivedMsgApp = props.AppId;
                //}
            };
            
            

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
                    ContentType = ContentEnum.Json,
                    ContentEncoding = "UTF-8",
                    AppId = Guid.NewGuid().ToString(),
                    MessageId = Guid.NewGuid().ToString(),
                    Expiration = "36000000"
                };
            }
        }
        private static void AutoReceive()
        {
            
        }
    }
}
