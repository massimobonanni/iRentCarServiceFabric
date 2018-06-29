using System;
using System.Runtime.Serialization;

namespace iRentCar.InvoiceActor.Interfaces
{
    [DataContract]
    public class InvoiceInfo
    {
        [DataMember]
        public string Number { get; set; }

        [DataMember]
        public string Customer { get; set; }

        [DataMember]
        public DateTime CreationDate { get; set; }

        [DataMember]
        public DateTime? PaymentDate { get; set; }

        [DataMember]
        public decimal Amount { get; set; }

        [DataMember]
        public InvoiceState State { get; set; }
    }
}