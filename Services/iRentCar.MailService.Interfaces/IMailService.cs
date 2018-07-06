using Microsoft.ServiceFabric.Services.Remoting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.MailService.Interfaces
{
    public interface IMailService : IService
    {
        Task<MailServiceError> SendMailAsync(MailInfo mail, CallBackInfo callback,   CancellationToken cancellationToken);

    }
}