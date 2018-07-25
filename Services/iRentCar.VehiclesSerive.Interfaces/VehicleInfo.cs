using System;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace iRentCar.VehiclesService.Interfaces
{
    [DataContract]
    public class VehicleInfo
    {
        public VehicleInfo()
        {

        }

        public VehicleInfo(VehicleInfo source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            
            this.Brand = source.Brand;
            this.DailyCost = source.DailyCost;
            this.Model = source.Model;
            this.Plate = source.Plate;
            this.State = source.State;
        }

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
                if (Plate == null)
                    return 0;
                return Plate.GetExtendedHash();
            }
        }

    }
}
