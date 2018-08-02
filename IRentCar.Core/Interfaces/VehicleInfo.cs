using System;
using Newtonsoft.Json;

namespace iRentCar.Core.Interfaces
{
    public class VehicleInfo
    {
        public string Plate { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public Decimal DailyCost { get; set; }
        public VehicleState State { get; set; }

        [JsonIgnore]
        public long PartitionKey => Plate.GetExtendedHash();
    }
}
