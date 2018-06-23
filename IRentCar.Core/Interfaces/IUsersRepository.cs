using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.Core.Interfaces
{
    public interface IUsersRepository
    {
        Task<UserInfo> GetUserByUsernameAsync(string username, CancellationToken cancellationToken);
    }
}
