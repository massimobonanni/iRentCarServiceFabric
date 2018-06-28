using System.Runtime.Serialization;

namespace iRentCar.InvoicesService.Interfaces
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