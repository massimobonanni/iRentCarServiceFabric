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
        public DateTime? EndRent { get; set; }


        public decimal? CalculateCost()
        {
            if (!EndRent.HasValue)
                return null;

            var dayDuration = (int)Math.Floor((EndRent.Value - StartRent).TotalDays + 1);

            return dayDuration * VehicleDailyCost;
        }


    }
}
