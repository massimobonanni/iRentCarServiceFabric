using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CoreInterfaces=iRentCar.Core.Interfaces;
using iRentCar.UserActor.Interfaces;

namespace iRentCar.UserActor
{
    internal class InMemoryUserRepository : CoreInterfaces.IUsersRepository
    {
        private static IDictionary<string, CoreInterfaces.UserInfo> users = new Dictionary<string, CoreInterfaces.UserInfo>
        {
            {
                "massimo.bonanni",
                new CoreInterfaces.UserInfo()
                {
                    Email = "mabonann@microsoft.com",
                    FirstName = "Massimo",
                    LastName="Bonanni",
                    IsEnabled=true
                }
            }
        };
        
        public Task<CoreInterfaces.UserInfo> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            CoreInterfaces.UserInfo user = null;
            if (users.ContainsKey(username))
                user = users[username];
            return Task.FromResult(user);
        }
    }
}