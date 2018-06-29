using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using iRentCar.InvoiceActor.Interfaces;
using iRentCar.Core.Interfaces;
using iRentCar.Core.Implementations;
using System.Runtime.Serialization;
using ActorInterfaces = iRentCar.InvoiceActor.Interfaces;

namespace iRentCar.InvoiceActor
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    [ActorService(Name = "InvoiceActor")]
    internal class InvoiceActor : iRentCar.Core.Implementations.ActorBase, IInvoiceActor
    {
        public InvoiceActor(ActorService actorService, ActorId actorId)
            : this(actorService, actorId, new ReliableFactory(), new ReliableFactory())
        {
        }

        public InvoiceActor(ActorService actorService, ActorId actorId, IActorFactory actorFactory,
            IServiceFactory serviceFactory)
            : base(actorService, actorId, actorFactory, serviceFactory)
        {
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            return Task.CompletedTask;
        }

        private const string InvoiceDataKeyName = "InvoiceData";

        public SerializationInfo UriConstants { get; private set; }

        #region [ StateManager accessors ]
        private async Task<InvoiceData> GetInvoiceDataFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var data = await this.StateManager.TryGetStateAsync<InvoiceData>(InvoiceDataKeyName, cancellationToken);
            return data.HasValue ? data.Value : null;
        }
        private Task SetInvoiceDataIntoStateAsync(InvoiceData data, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<InvoiceData>(InvoiceDataKeyName, data, cancellationToken);
        }
        #endregion [ StateManager accessors ]

        #region [ IInvoiceActor interfaces ]
        public async Task<InvoiceActorError> CreateAsync(string customer, decimal amount, DateTime creationDate, string callbackUri, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(customer))
                throw new ArgumentException(nameof(customer));
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount));

            var invoiceData = await this.GetInvoiceDataFromStateAsync(cancellationToken);

            if (invoiceData != null)
                return InvoiceActorError.InvoiceAlreadyExists;

            var invoicedata = new InvoiceData()
            {
                Amount = amount,
                CreationDate = creationDate,
                Customer = customer,
                State = InvoiceState.NotPaid,
                CallbackUri = callbackUri
            };
            await this.SetInvoiceDataIntoStateAsync(invoicedata, cancellationToken);
            return InvoiceActorError.Ok;
        }

        public async Task<ActorInterfaces.InvoiceInfo> GetInfoAsync(CancellationToken cancellationToken)
        {
            var invoiceData = await this.GetInvoiceDataFromStateAsync(cancellationToken);

            if (invoiceData != null)
                return null;

            return invoiceData.ToInvoiceInfo();
        }

        public async Task<InvoiceActorError> PaidAsync(DateTime payDate, CancellationToken cancellationToken)
        {
            var invoiceData = await this.GetInvoiceDataFromStateAsync(cancellationToken);

            if (invoiceData == null)
                return InvoiceActorError.InvoiceNotValid;

            if (invoiceData.State == InvoiceState.Paid)
                return InvoiceActorError.InvoiceAlreadyPaid;

            if (invoiceData.CreationDate > payDate)
                return InvoiceActorError.PaymentDateNotCorrect;

            invoiceData.PaymentDate = payDate;
            invoiceData.State = InvoiceState.Paid;

            if (!string.IsNullOrWhiteSpace(invoiceData.CallbackUri))
            {
                try
                {
                    var callbackProxy = this.actorFactory.Create<IInvoiceCallbackActor>(new ActorId(invoiceData.Customer),
                            new Uri(invoiceData.CallbackUri));

                    await callbackProxy.InvoicePaidAsync(this.Id.ToString(), payDate, cancellationToken);
                }
                catch { }
            }

            return InvoiceActorError.Ok;
        }

        #endregion [ IInvoiceActor interfaces ]
    }
}
