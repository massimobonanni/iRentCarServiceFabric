using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.UsersService.Interfaces
{
    public interface IUsersService : IService
    {
        Task<List<UserInfo>> SearchUsersAsync(string username, string firstName, string lastName, string mail,
            CancellationToken cancellationToken);

        Task<UserInfo> GetUserByUserNameAsync(string userName, CancellationToken cancellationToken);

        Task<bool> AddOrUpdateUserAsync(UserInfo user, CancellationToken cancellationToken);
    }
}