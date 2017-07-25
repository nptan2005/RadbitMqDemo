using System;
using System.Runtime.Serialization;

namespace RadbitMqDemo.DataTransfer
{
    [DataContract]
    public class RabbitServiceTaskMessageData
    {
        [DataMember]
        public string QueueName { get; set; }
        [DataMember]
        public string Message { get; set; } = "Hello Moto";
        [DataMember]
        public string RequestID { get; set; } = Guid.NewGuid().ToString();
        [DataMember]
        public string RequestDateTime { get; set; } = DateTime.UtcNow.ToString("s") + "Z";
    }
}
