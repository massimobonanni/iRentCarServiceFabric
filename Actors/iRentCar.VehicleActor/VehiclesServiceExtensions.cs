using System;
using System.Collections.Generic;
using System.Text;
using VehiclesServiceInterfaces = iRentCar.VehiclesService.Interfaces;
using VehicleActorInterfaces = iRentCar.VehicleActor.Interfaces;

namespace iRentCar.VehiclesService.Interfaces
{
    internal static class VehiclesServiceExtensions
    {

        internal static VehicleActorInterfaces.VehicleState ToActorInterfaceState(this VehicleState state)
        {
            switch (state)
            {
                case VehicleState.Busy:
                    return VehicleActorInterfaces.VehicleState.Busy;
                case VehicleState.Free:
                    return VehicleActorInterfaces.VehicleState.Free;
                case VehicleState.NotAvailable:
                default:
                    return VehicleActorInterfaces.VehicleState.NotAvailable;
            }
        }
    }
}
