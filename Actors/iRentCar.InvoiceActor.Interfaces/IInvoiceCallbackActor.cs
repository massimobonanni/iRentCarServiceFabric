using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

namespace iRentCar.InvoiceActor.Interfaces
{
    public interface IInvoiceCallbackActor : IActor
    {
        Task InvoicePaidAsync(string invoiceNumber, DateTime paymentDate, CancellationToken cancellationToken);
    }
}
