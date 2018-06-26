using System;
using System.Runtime.Serialization;

namespace iRentCar.VehicleActor.Interfaces
{
    [DataContract]
    public class RentInfo
    {
        [DataMember]
        public string User { get; set; }
        [DataMember]
        public DateTime StartDate { get; set; }
        [DataMember]
        public DateTime EndDate { get; set; }

    }
}