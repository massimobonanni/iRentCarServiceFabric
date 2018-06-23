using System;
using System.Runtime.Serialization;

namespace iRentCar.VehiclesService.Interfaces
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
        public decimal DailyCost { get; set; }
        [DataMember]
        public VehicleState State { get; set; }

        [IgnoreDataMember]
        public long PartitionKey
        {
            get
            {
                var fullkey = $"{Brand}-{Model}";
                return fullkey.GetExtendedHash();
            }
        }

    }
}
