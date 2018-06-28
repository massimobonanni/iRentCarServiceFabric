using System;
using System.Runtime.Serialization;

namespace iRentCar.VehicleActor.Interfaces
{
    [DataContract]
    public class VehicleInfo
    {
        [DataMember]
        public string Plate { get; set; }
        [DataMember]
        public string Model { get; set; }
        [DataMember]
        public string Brand { get; set; }
        [DataMember]
        public Decimal DailyCost { get; set; }
        [DataMember]
        public VehicleState State { get; set; }
        [DataMember]
        public RentInfo CurrentRent { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Model) && !string.IsNullOrWhiteSpace(Brand);
        }

    }
}
