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
        /// <summary>
        /// Initializes a new instance of InvoiceActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
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

        
    }
}
