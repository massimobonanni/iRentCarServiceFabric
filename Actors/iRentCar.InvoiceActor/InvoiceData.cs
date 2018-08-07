using iRentCar.InvoiceActor.Interfaces;
using System;
using System.Runtime.Serialization;

namespace iRentCar.InvoiceActor
{
    [DataContract]
    internal class InvoiceData
    {
        [DataMember]
        public string CallbackUri { get; set; }

        [DataMember]
        public string CustomerId { get; set; }

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