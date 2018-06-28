using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.VehiclesService.Interfaces
{
    public interface IVehiclesServiceProxy
    {
        Task<IEnumerable<VehicleInfo>> SearchVehiclesAsync(string plate, string model, string brand,
            VehicleState? state, CancellationToken cancellationToken);

        Task<VehicleInfo> GetVehicleByPlateAsync(string plate, CancellationToken cancellationToken);

        Task<bool> UpdateVehicleStateAsync(string plate, VehicleState newState, CancellationToken cancellationToken);
    }
}