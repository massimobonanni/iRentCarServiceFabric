using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.MailService.Interfaces
{
    public interface IMailServiceProxy
    {
        Task<MailServiceError> SendMailAsync(MailInfo mail, CallBackInfo callback, CancellationToken cancellationToken);
    }
}