using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreInterfaces = iRentCar.Core.Interfaces;

namespace iRentCar.Core.Implementations
{
    public class InMemoryUserRepository : CoreInterfaces.IUsersRepository
    {
        private static IEnumerable<CoreInterfaces.UserInfo> users = new List<CoreInterfaces.UserInfo>
        {
            {
                new CoreInterfaces.UserInfo()
                {
                    Username="massimo.bonanni",
                    Email = "mabonann@microsoft.com",
                    FirstName = "Massimo",
                    LastName="Bonanni",
                    IsEnabled=true
                }
            }
        };

        public Task<IQueryable<CoreInterfaces.UserInfo>> GetAllUsersAsync(long lowPartitionKey, long highPartitionKey, CancellationToken token)
        {
            var query = users
                .Where(a => a.PartitionKey >= lowPartitionKey)
                .Where(a => a.PartitionKey <= highPartitionKey);

            return Task.FromResult(query.ToList().AsQueryable()) ;
        }

        public void SetServiceHost(ServiceContext hostContext)
        {

        }
    }
}