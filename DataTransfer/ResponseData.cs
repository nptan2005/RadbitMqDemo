using System.Runtime.Serialization;

namespace RadbitMqDemo.DataTransfer
{
    [DataContract]
    public sealed class ResponseData
    {
        [DataMember]
        public string Data { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string RequestID { get; set; }

        [DataMember]
        public string ResponseCode { get; set; }
    }
}