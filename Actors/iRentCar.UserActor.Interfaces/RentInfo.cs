using System;
using System.Runtime.Serialization;

namespace iRentCar.UserActor.Interfaces
{
    [DataContract]
    public class RentInfo
    {
        [DataMember]
        public string Plate { get; set; }
        [DataMember]
        public decimal DailyCost { get; set; }
        [DataMember]
        public DateTime StartRent { get; set; } = DateTime.Now;
    }

}