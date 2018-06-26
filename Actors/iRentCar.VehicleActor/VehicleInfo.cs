using System;
using System.Runtime.Serialization;
using VehiclesServiceInterfaces=iRentCar.VehiclesService.Interfaces;
using VehicleActorInterfaces = iRentCar.VehicleActor.Interfaces;

namespace iRentCar.VehicleActor
{
    [DataContract]
    internal class VehicleInfo
    {
        [DataMember]
        public string Model { get; set; }
        [DataMember]
        public string Brand { get; set; }
        [DataMember]
        public decimal DailyCost { get; set; }

        public Interfaces.VehicleInfo ToInterfacesInfo()
        {
            return new Interfaces.VehicleInfo()
            {
                Brand = this.Brand,
                DailyCost = this.DailyCost,
                Model = this.Model
            };
        }

        public static VehicleInfo FromServiceInterfacesInfo(VehiclesServiceInterfaces.VehicleInfo vehicleInfo)
        {
            if (vehicleInfo == null)
                throw new ArgumentNullException(nameof(vehicleInfo));
            return new VehicleInfo()
            {
                Brand = vehicleInfo.Brand,
                DailyCost = vehicleInfo.DailyCost,
                Model = vehicleInfo.Model,
            };
        }

        public static VehicleInfo FromActorInterfacesInfo(VehicleActorInterfaces.VehicleInfo vehicleInfo)
        {
            if (vehicleInfo == null)
                throw new ArgumentNullException(nameof(vehicleInfo));
            return new VehicleInfo()
            {
                Brand = vehicleInfo.Brand,
                DailyCost = vehicleInfo.DailyCost,
                Model = vehicleInfo.Model,
            };
        }
    }
}
