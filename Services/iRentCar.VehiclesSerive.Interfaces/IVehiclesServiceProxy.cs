using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.VehiclesService.Interfaces
{
    public interface IVehiclesServiceProxy
    {
        Task<IEnumerable<VehicleInfo>> SearchVehiclesAsync(string plate, string model, string brand,
            VehicleState? state, CancellationToken cancellationToken);
    }
}