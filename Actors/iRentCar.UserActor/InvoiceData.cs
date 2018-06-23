using System;
using System.Runtime.Serialization;

namespace iRentCar.UserActor
{
    [DataContract]
    internal class InvoiceData
    {
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public Decimal Amount { get; set; }
        [DataMember]
        public DateTime ReleaseDate { get; set; }
        [DataMember]
        public InvoiceState State { get; set; }
        [DataMember]
        public DateTime PaymentDate { get; set; }


    }
}
