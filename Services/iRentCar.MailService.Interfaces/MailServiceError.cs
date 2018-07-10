using System.Runtime.Serialization;

namespace iRentCar.MailService.Interfaces
{
    [DataContract]
    public enum MailServiceError
    {
        [EnumMember]
        Ok=0,
        [EnumMember]
        SubjectNotValid = 1,
        [EnumMember]
        TOAddressesEmpty = 2,
        [EnumMember]
        TOAddressNotValid = 3,
        [EnumMember]
        CCAddressNotValid = 4,
        [EnumMember]
        BccAddressNotValid = 5,

        [EnumMember]
        GenericError=999
    }
}