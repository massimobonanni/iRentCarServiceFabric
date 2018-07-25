using System;

namespace iRentCar.Core.Interfaces
{
    public class VehicleInfo
    {
        public string Plate { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public Decimal DailyCost { get; set; }
        public VehicleState State { get; set; }

        public long PartitionKey
        {
            get
            {
                return Plate.GetExtendedHash();
            }
        }

    }
}
