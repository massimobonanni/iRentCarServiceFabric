using System;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting;

namespace iRentCar.Core.Interfaces
{
    public interface IServiceFactory
    {
        TServiceInterface Create<TServiceInterface>(Uri serviceUri, ServicePartitionKey partitionKey, TargetReplicaSelector targetReplicaSelector, string listenerName) where TServiceInterface : IService;

        TServiceInterface Create<TServiceInterface>(Uri serviceUri) where TServiceInterface : IService;

        TServiceInterface Create<TServiceInterface>(Uri serviceUri, ServicePartitionKey partitionKey) where TServiceInterface : IService;

        TServiceInterface Create<TServiceInterface>(Uri serviceUri, ServicePartitionKey partitionKey, TargetReplicaSelector targetReplicaSelector) where TServiceInterface : IService;
    }
}
