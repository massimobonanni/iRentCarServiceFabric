using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Runtime;

namespace iRentCar.MailService
{
    internal class NeutralMailAdapter : MailAdapterBase
    {
        public override async Task<MailAdapterResult> SendMailAsync(MailData mail, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            return MailAdapterResult.Ok;
        }
    }
}
