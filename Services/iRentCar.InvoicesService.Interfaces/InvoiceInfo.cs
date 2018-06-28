using System;
using System.Runtime.Serialization;

namespace iRentCar.InvoicesService.Interfaces
{
    [DataContract]
    public class InvoiceInfo
    {
        [DataMember]
        public string InvoiceNumber { get; set; }
        [DataMember]
        public DateTime ReleaseDate { get; set; }
        [DataMember]
        public string Customer { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public InvoiceState State { get; set; }

    }
}
