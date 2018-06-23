using System;
using System.Runtime.Serialization;

namespace iRentCar.UserActor
{
    [DataContract]
    internal class RentData
    {
        [DataMember]
        public string VehiclePlate { get; set; }
        [DataMember]
        public Decimal VehicleDailyCost { get; set; }
        [DataMember]
        public string InvoiceNumber { get; set; }
        [DataMember]
        public RentState State { get; set; }
        [DataMember]
        public DateTime StartRent { get; set; }
        [DataMember]
        public DateTime? EndRent { get; set; }


    }
}
