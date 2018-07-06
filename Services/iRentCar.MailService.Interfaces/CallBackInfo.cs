using System.Runtime.Serialization;

namespace iRentCar.MailService.Interfaces
{
    [DataContract]
    public class CallBackInfo
    {
        [DataMember]
        public string ActorId { get; set; }
        [DataMember]
        public string ServiceUri { get; set; }
    }
}