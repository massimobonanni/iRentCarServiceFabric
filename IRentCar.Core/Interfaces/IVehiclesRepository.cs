using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.Core.Interfaces
{
    public interface IVehiclesRepository
    {
        void SetServiceHost(ServiceContext hostContext);
        Task<IQueryable<VehicleInfo>> GetAllVehiclesAsync(long lowPartitionKey, long highPartitionKey, CancellationToken token);
    }
}
