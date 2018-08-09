using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using iRentCar.Core.Interfaces;
using iRentCar.InvoiceActor.Interfaces;
using iRentCar.InvoicesService.Interfaces;
using Microsoft.ServiceFabric.Actors;
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
    internal sealed class InvoicesService : Core.Implementations.StatefulServiceBase, IInvoicesService
    {
        public InvoicesService(StatefulServiceContext context,
            IActorFactory actorFactory = null, IServiceFactory serviceFactory = null)
            : base(context, actorFactory, serviceFactory)
        { }

        public InvoicesService(StatefulServiceContext context, IReliableStateManagerReplica stateManager,
                        IActorFactory actorFactory = null, IServiceFactory serviceFactory = null)
            : base(context, stateManager, actorFactory, serviceFactory)
        { }

        private IReliableDictionary<string, uint> invoiceNumbersDictionary;

        internal const string InvoiceNumbersDictionaryKeyName = "InvoiceNumbersDictionaryKeyName";

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
            await InitializeInvoiceNumbersDictionaryAsync();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private async Task InitializeInvoiceNumbersDictionaryAsync()
        {
            if (this.invoiceNumbersDictionary == null)
                invoiceNumbersDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, uint>>(InvoiceNumbersDictionaryKeyName);
        }

        #region [ IInvoicesService interface ]
        public async Task<Interfaces.InvoiceInfo> GenerateInvoiceAsync(string customerId, decimal amount,
            DateTime releaseDate, string callbackUri, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new ArgumentException(nameof(customerId));
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            await InitializeInvoiceNumbersDictionaryAsync();

            Interfaces.InvoiceInfo invoice = null;

            uint invoiceNumber = 0;
            string yearKey = releaseDate.Year.ToString();
            using (var tx = this.StateManager.CreateTransaction())
            {
                invoiceNumber = await this.invoiceNumbersDictionary.GetOrAddAsync(tx,yearKey, 0);
                InvoiceActorError creationResult = InvoiceActorError.Ok;
                string invoiceNumberComplete;
                do
                {
                    invoiceNumber++;

                    invoiceNumberComplete = $"{yearKey}/{invoiceNumber}";

                    var invoiceActor = this.actorFactory.Create<IInvoiceActor>(new ActorId(invoiceNumberComplete),
                        new Uri(UriConstants.InvoiceActorUri));

                    creationResult = await invoiceActor.CreateAsync(customerId, amount, releaseDate,
                        callbackUri, cancellationToken);

                } while ((creationResult == InvoiceActorError.InvoiceAlreadyExists ||
                         creationResult == InvoiceActorError.InvoiceAlreadyPaid ||
                         creationResult == InvoiceActorError.InvoiceNotValid) && ! cancellationToken.IsCancellationRequested\);

                if (creationResult == InvoiceActorError.Ok)
                {
                    await this.invoiceNumbersDictionary.SetAsync(tx, yearKey, invoiceNumber);

                    invoice = new Interfaces.InvoiceInfo()
                    {
                        Amount = amount,
                        Customer = customerId,
                        InvoiceNumber = invoiceNumberComplete,
                        ReleaseDate = releaseDate,
                        State = Interfaces.InvoiceState.NotPaid
                    };
                    await tx.CommitAsync();
                }
            }
            return invoice;
        }
        #endregion [ IInvoicesService interface ]
    }
}
