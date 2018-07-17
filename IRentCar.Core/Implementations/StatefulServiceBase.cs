using iRentCar.Core.Interfaces;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Services.Runtime;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text;

namespace iRentCar.Core.Implementations
{
    public abstract class StatefulServiceBase : StatefulService
    {
        protected StatefulServiceBase(StatefulServiceContext context, IActorFactory actorFactory = null,
            IServiceFactory serviceFactory = null)
            : base(context)
        {
            if (actorFactory == null)
                this.actorFactory = new ReliableFactory();
            else
                this.actorFactory = actorFactory;

            if (serviceFactory == null)
                this.serviceFactory = new ReliableFactory();
            else
                this.serviceFactory = serviceFactory;
        }

        protected StatefulServiceBase(StatefulServiceContext context, IReliableStateManagerReplica stateManager,
            IActorFactory actorFactory = null, IServiceFactory serviceFactory = null)
            : base(context, stateManager)
        {
            if (actorFactory == null)
                this.actorFactory = new ReliableFactory();
            else
                this.actorFactory = actorFactory;


            if (serviceFactory == null)
                this.serviceFactory = new ReliableFactory();
            else
                this.serviceFactory = serviceFactory;
        }

        protected readonly IActorFactory actorFactory;
        protected readonly IServiceFactory serviceFactory;


    }
}
