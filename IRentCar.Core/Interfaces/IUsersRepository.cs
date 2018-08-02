using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.Core.Interfaces
{
    public interface IUsersRepository
    {
        void SetServiceHost(ServiceContext hostContext);
        Task<IQueryable<UserInfo>> GetAllUsersAsync(long lowPartitionKey, long highPartitionKey,CancellationToken token);
    }
}
