using System.Runtime.Serialization;

namespace iRentCar.MailService.Interfaces
{
    [DataContract]
    public enum MailServiceError
    {
        [EnumMember]
        Ok=0,
        [EnumMember]
        GenericError=999
    }
}