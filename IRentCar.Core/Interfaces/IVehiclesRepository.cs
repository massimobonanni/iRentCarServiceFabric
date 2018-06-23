using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.Core.Interfaces
{
    public interface IVehiclesRepository
    {
        Task<IQueryable<VehicleInfo>> GetAllVehiclesAsync(long lowPartitionKey, long highPartitionKey);
    }
}
