using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace iRentCar.MailService.Interfaces
{
    public sealed class MailServiceProxy : IMailServiceProxy
    {
        private static Uri serviceUri;
        private static MailServiceProxy instance = null;
        private static List<Int64RangePartitionInformation> partitionInfoList = null;
        private static int currentPartitionIndex = 0;

        private static readonly object singletonLock = new object();

        private MailServiceProxy()
        {

        }

        static MailServiceProxy()
        {
            serviceUri = new Uri(UriConstants.MailServiceUri);
        }


        public static MailServiceProxy Instance
        {
            get
            {
                lock (singletonLock)
                {
                    return instance ?? (instance = new MailServiceProxy());
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
                    currentPartitionIndex = 0;
                }
            }
        }

        private IMailService CreateServiceProxy(MailInfo mail)
        {
            var partitionKey = new ServicePartitionKey(partitionInfoList[currentPartitionIndex].LowKey);

            currentPartitionIndex++;
            if (currentPartitionIndex >= partitionInfoList.Count)
            {
                currentPartitionIndex = 0;
            }

            return ServiceProxy.Create<IMailService>(serviceUri, partitionKey);
        }


        public async Task<MailServiceError> SendMailAsync(MailInfo mail, CallBackInfo callback, CancellationToken cancellationToken)
        {
            await EnsurePartitionCount();
            try
            {
                var proxy = CreateServiceProxy(mail);
                return await proxy.SendMailAsync(mail, callback, cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
