using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.MailService.Interfaces
{
    public interface ICallBackMailService : IActor
    {
        Task MailSendCompletedAsync(Guid messageId, MailSendResult result, CancellationToken cancellationToken);

    }
}