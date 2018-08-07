using System.Runtime.Serialization;

namespace iRentCar.InvoiceActor.Interfaces
{
    [DataContract]
    public enum InvoiceActorError
    {
        [EnumMember]
        Ok = 0,
        [EnumMember]
        InvoiceAlreadyExists = 1,
        [EnumMember]
        PaymentDateNotCorrect = 2,
        [EnumMember]
        InvoiceNotValid = 3,
        [EnumMember]
        InvoiceAlreadyPaid = 4,
        [EnumMember]
        GenericError = 999
    }
}