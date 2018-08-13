using iRentCar.Core;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.InvoicesService.Interfaces
{
    public sealed class InvoicesServiceProxy : IInvoicesServiceProxy
    {
        private static Uri serviceUri;
        private static InvoicesServiceProxy instance = null;
        private static List<Int64RangePartitionInformation> partitionInfoList = null;

        private static readonly object singletonLock = new object();

        private InvoicesServiceProxy()
        {

        }

        static InvoicesServiceProxy()
        {
            serviceUri = new Uri(UriConstants.InvoicesServiceUri);
        }


        public static InvoicesServiceProxy Instance
        {
            get
            {
                lock (singletonLock)
                {
                    return instance ?? (instance = new InvoicesServiceProxy());
                }
            }
        }

        private async Task EnsurePartitionCount()
        {
            if (partitionInfoList == null || !partitionInfoList.Any())
            {
                using (var client = new FabricClient())
                {
                    var partitionList = await client.QueryManager.GetPartitionListAsync(serviceUri);

                    partitionInfoList = partitionList.Select(p => p.PartitionInformation)
                        .OfType<Int64RangePartitionInformation>().ToList();
                }
            }
        }

        private IInvoicesService CreateServiceProxy()
        {
            var partitionKey = new ServicePartitionKey(0);
            return ServiceProxy.Create<IInvoicesService>(serviceUri, partitionKey);
        }


        public async Task<InvoiceInfo> GenerateInvoiceAsync(string customerId, decimal amount,
            DateTime releaseDate, string callbackUri, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException(nameof(customerId));
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            await EnsurePartitionCount();
            InvoiceInfo invoice = null;
            try
            {
                var proxy = CreateServiceProxy();
                invoice = await proxy.GenerateInvoiceAsync(customerId, amount, releaseDate, callbackUri,cancellationToken);
            }
            catch (Exception)
            {
                throw;
            }
            return invoice;
        }
    }
}
