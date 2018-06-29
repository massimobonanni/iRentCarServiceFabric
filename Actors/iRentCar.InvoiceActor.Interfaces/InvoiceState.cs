using System.Runtime.Serialization;

namespace iRentCar.InvoiceActor.Interfaces
{
    [DataContract]
    public enum InvoiceState
    {
        [EnumMember]
        NotPaid,
        [EnumMember]
        Paid
    }
}