using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.UsersService.Interfaces
{
    public interface IUsersServiceProxy
    {
        Task<IEnumerable<UserInfo>> SearchUsersAsync(string username, string firstName, string lastName, string mail,
            CancellationToken cancellationToken);

        Task<UserInfo> GetUserByUserNameAsync(string username, CancellationToken cancellationToken);

        Task<bool> AddOrUpdateUserAsync(UserInfo user, CancellationToken cancellationToken);
    }
}