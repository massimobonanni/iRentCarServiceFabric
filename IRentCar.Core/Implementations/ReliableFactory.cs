using System;
using iRentCar.Core.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace iRentCar.Core.Implementations
{
    public class ReliableFactory : IActorFactory, IServiceFactory
    {
        TActorInterface IActorFactory.Create<TActorInterface>(string actorId, Uri serviceUri, string listenerName)
        {
            return ActorProxy.Create<TActorInterface>(new ActorId(actorId), serviceUri, listenerName);
        }

        TActorInterface IActorFactory.Create<TActorInterface>(ActorId actorId, Uri serviceUri, string listenerName)
        {
            return ActorProxy.Create<TActorInterface>(actorId, serviceUri, listenerName);
        }

        TActorInterface IActorFactory.Create<TActorInterface>(ActorId actorId, string applicationName = null, string serviceName = null, string listenerName = null)
        {
            return ActorProxy.Create<TActorInterface>(actorId, applicationName, serviceName, listenerName);
        }

        TServiceInterface IServiceFactory.Create<TServiceInterface>(Uri serviceUri)
        {
            return ServiceProxy.Create<TServiceInterface>(serviceUri);
        }

        TServiceInterface IServiceFactory.Create<TServiceInterface>(Uri serviceUri,
            ServicePartitionKey partitionKey)
        {
            return ServiceProxy.Create<TServiceInterface>(serviceUri, partitionKey);
        }

        TServiceInterface IServiceFactory.Create<TServiceInterface>(Uri serviceUri,
            ServicePartitionKey partitionKey, TargetReplicaSelector targetReplicaSelector)
        {
            return ServiceProxy.Create<TServiceInterface>(serviceUri, partitionKey, targetReplicaSelector);
        }

        TServiceInterface IServiceFactory.Create<TServiceInterface>(Uri serviceUri,
            ServicePartitionKey partitionKey, TargetReplicaSelector targetReplicaSelector,
            string listenerName)
        {
            return ServiceProxy.Create<TServiceInterface>(serviceUri, partitionKey, targetReplicaSelector, listenerName);
        }
    }
}