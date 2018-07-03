using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.InvoicesService.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace iRentCar.InvoicesService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class InvoicesService : StatefulService, IInvoicesService
    {
        public InvoicesService(StatefulServiceContext context)
            : base(context)
        { }

        public InvoicesService(StatefulServiceContext context, IReliableStateManagerReplica stateManager)
            : base(context, stateManager)
        { }

        private IReliableDictionary<string, uint> invoiceNumbersDictionary;

        private const string InvoiceNumbersDictionaryKeyName = "InvoiceNumbersDictionaryKeyName";

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {

            invoiceNumbersDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, uint>>(InvoiceNumbersDictionaryKeyName);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        #region [ IInvoicesService interface ]
        public async Task<InvoiceInfo> GenerateInvoiceAsync(string customer, decimal amount,
            DateTime releaseDate, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(customer))
                throw new ArgumentException(nameof(customer));
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));


            uint invoiceNumber = 0;
            string yearKey = releaseDate.Year.ToString();
            using (var tx = this.StateManager.CreateTransaction())
            {
                invoiceNumber = await this.invoiceNumbersDictionary.GetOrAddAsync(tx,
                    yearKey, 0, TimeSpan.FromSeconds(5), cancellationToken);
                invoiceNumber++;
                await this.invoiceNumbersDictionary.SetAsync(tx, yearKey, invoiceNumber);
                await tx.CommitAsync();
            }

            // TODO: creazione della fattura come attore e inizializzazione!!!
            var invoiceNumberComplete = $"{yearKey}/{invoiceNumber}";

            return new InvoiceInfo()
            {
                Amount = amount,
                Customer = customer,
                InvoiceNumber = invoiceNumberComplete,
                ReleaseDate = releaseDate,
                State = InvoiceState.NotPaid
            };
        }
        #endregion [ IInvoicesService interface ]
    }
}
