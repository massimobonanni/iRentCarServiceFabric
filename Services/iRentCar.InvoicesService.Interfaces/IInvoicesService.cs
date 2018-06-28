using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.InvoicesService.Interfaces
{
    public interface IInvoicesService : IService
    {
        Task<InvoiceInfo> GenerateInvoiceAsync(string customer, decimal amount, 
            DateTime releaseDate, CancellationToken cancellationToken);
    }
}
