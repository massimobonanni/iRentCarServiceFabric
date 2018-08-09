using System;
using System.Collections.Generic;
using System.Text;

namespace iRentCar.VehiclesService.Interfaces
{
    internal static class VehicleInfoExtensions
    {
        public static iRentCar.VehicleActor.Interfaces.VehicleInfo ToVehicleActorVehicleInfo(this VehicleInfo info)
        {
            if (info == null)
                throw new NullReferenceException(nameof(info));

            return new iRentCar.VehicleActor.Interfaces.VehicleInfo()
            {
                Brand = info.Brand,
                CurrentRent = null,
                DailyCost = info.DailyCost,
                Model = info.Model,
                Plate = info.Plate,
                State = info.State.ToVehicleActorVehicleState()
            };

        }

        public static iRentCar.VehicleActor.Interfaces.VehicleState ToVehicleActorVehicleState(this VehicleState state)
        {
            switch (state)
            {
                case VehicleState.Free:
                    return iRentCar.VehicleActor.Interfaces.VehicleState.Free;
                case VehicleState.Busy:
                    return iRentCar.VehicleActor.Interfaces.VehicleState.Busy;
                case VehicleState.NotAvailable:
                    return iRentCar.VehicleActor.Interfaces.VehicleState.NotAvailable;
                default:
                    return iRentCar.VehicleActor.Interfaces.VehicleState.NotAvailable;
            }
        }

        public static bool VerifyFilters(this VehicleInfo info, string plate, string model, string brand,
            VehicleState? state)
        {
            if (info == null)
                throw new NullReferenceException(nameof(info));

            var inResult = true;
            if (!string.IsNullOrWhiteSpace(plate))
                if (!String.Equals(info.Plate, plate, StringComparison.CurrentCultureIgnoreCase))
                    inResult = false;

            if (!string.IsNullOrWhiteSpace(model) && inResult)
                if (!String.Equals(info.Model, model, StringComparison.CurrentCultureIgnoreCase))
                    inResult = false;

            if (!string.IsNullOrWhiteSpace(brand) && inResult)
                if (!String.Equals(info.Brand, brand, StringComparison.CurrentCultureIgnoreCase))
                    inResult = false;

            if (state.HasValue && inResult)
                if (info.State != state.Value)
                    inResult = false;

            return inResult;
        }

    }
}
