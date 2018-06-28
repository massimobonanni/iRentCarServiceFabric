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
        public decimal VehicleDailyCost { get; set; }
        [DataMember]
        public DateTime StartRent { get; set; }
        [DataMember]
        public DateTime EndRent { get; set; }

        public bool IsRentTimeExpired(DateTime returnDate)
        {
            return returnDate > EndRent;
        }

        public decimal? CalculateCost(DateTime returnDate)
        {
            var dayDuration = (int)Math.Floor((returnDate - StartRent).TotalDays + 1);

            return dayDuration * VehicleDailyCost;
        }


    }
}
