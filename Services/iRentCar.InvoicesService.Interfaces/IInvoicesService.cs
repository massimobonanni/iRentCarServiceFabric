﻿using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.InvoicesService.Interfaces
{
    public interface IInvoicesService : IService
    {
        Task<InvoiceInfo> GenerateInvoiceAsync(string customerId, decimal amount,
            DateTime releaseDate, string callbackUri, CancellationToken cancellationToken);
    }
}
