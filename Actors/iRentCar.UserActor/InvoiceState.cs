using System.Runtime.Serialization;

namespace iRentCar.UserActor
{
    [DataContract]
    internal enum InvoiceState
    {
        [EnumMember]
        NotPaid,
        [EnumMember]
        Paid
    }
}