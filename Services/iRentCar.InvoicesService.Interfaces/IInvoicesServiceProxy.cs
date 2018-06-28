using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.InvoicesService.Interfaces
{
    public interface IInvoicesServiceProxy
    {
        Task<InvoiceInfo> GenerateInvoiceAsync(string customer, decimal amount, 
            DateTime releaseDate,CancellationToken cancellationToken);
    }
}