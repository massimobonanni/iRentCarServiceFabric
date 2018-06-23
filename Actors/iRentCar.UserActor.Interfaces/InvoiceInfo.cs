using System;
using System.Runtime.Serialization;

namespace iRentCar.UserActor.Interfaces
{
    [DataContract]
    public class InvoiceInfo
    {
        [DataMember]
        public string Number { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public DateTime ReleaseDate { get; set; }
    }
}