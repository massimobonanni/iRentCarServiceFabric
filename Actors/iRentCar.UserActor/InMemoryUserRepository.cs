using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core.Interfaces;
using iRentCar.UserActor.Interfaces;

namespace iRentCar.UserActor
{
    internal class InMemoryUserRepository : IUsersRepository
    {
        private static IDictionary<string, UserInfo> users = new Dictionary<string, UserInfo>
        {
            {
                "massimo.bonanni",
                new UserInfo()
                {
                    Email = "mabonann@microsoft.com",
                    FirstName = "Massimo",
                    LastName="Bonanni",
                    IsEnabled=true
                }
            }
        };
        
        public Task<UserInfo> GetUserByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            UserInfo user = null;
            if (users.ContainsKey(username))
                user = users[username];
            return Task.FromResult(user);
        }
    }
}