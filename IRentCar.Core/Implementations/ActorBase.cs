using System;
using iRentCar.Core.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace iRentCar.Core.Implementations
{
    public abstract class ActorBase : Actor
    {
        protected readonly IActorFactory actorFactory;
        protected readonly IServiceFactory serviceFactory;

        protected ActorBase(ActorService actorService, ActorId actorId, IActorFactory actorFactory,
            IServiceFactory serviceFactory)
            : base(actorService, actorId)
        {
            if (actorFactory == null)
                throw new ArgumentNullException(nameof(actorFactory));
            if (serviceFactory == null)
                throw new ArgumentNullException(nameof(serviceFactory));

            this.actorFactory = actorFactory;
            this.serviceFactory = serviceFactory;
        }
    }
}