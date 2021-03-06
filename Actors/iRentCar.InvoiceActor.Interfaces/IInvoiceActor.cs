﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

//[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace iRentCar.InvoiceActor.Interfaces
{
    /// <summary>
    /// This interface defines the methods exposed by an actor.
    /// Clients use this interface to interact with the actor that implements it.
    /// </summary>
    public interface IInvoiceActor : IActor
    {
        Task<InvoiceActorError> CreateAsync(string customerId, decimal amount, DateTime creationDate, string callbackUri, CancellationToken cancellationToken);

        Task<InvoiceActorError> PaidAsync(DateTime payDate, CancellationToken cancellationToken);

        Task<InvoiceInfo> GetInfoAsync(CancellationToken cancellationToken);

    }
}
