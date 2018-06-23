using System;
using Microsoft.ServiceFabric.Actors;

namespace iRentCar.Core.Interfaces
{
    public interface IActorFactory
    {
        TActorInterface Create<TActorInterface>(string actorId, Uri serviceUri, string listenerName = null) where TActorInterface : IActor;

        TActorInterface Create<TActorInterface>(ActorId actorId, Uri serviceUri, string listenerName = null) where TActorInterface : IActor;

        TActorInterface Create<TActorInterface>(ActorId actorId, string applicationName = null, string serviceName = null, string listenerName = null) where TActorInterface : IActor;
    }
}
