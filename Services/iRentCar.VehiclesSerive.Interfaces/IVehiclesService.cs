using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.VehiclesService.Interfaces
{
    public interface IVehiclesService : IService
    {
        Task<List<VehicleInfo>> SearchVehiclesAsync(string plate, string model, string brand, VehicleState? state,
            CancellationToken cancellationToken);
    }
}