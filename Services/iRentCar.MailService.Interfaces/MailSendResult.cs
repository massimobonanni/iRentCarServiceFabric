using System.Runtime.Serialization;

namespace iRentCar.MailService.Interfaces
{
    [DataContract]
    public enum MailSendResult
    {
        [EnumMember]
        Sended=0,
        [EnumMember]
        NotSended=999
    }
}