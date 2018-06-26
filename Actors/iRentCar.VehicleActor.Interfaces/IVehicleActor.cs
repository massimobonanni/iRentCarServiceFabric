using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace iRentCar.VehicleActor.Interfaces
{

    public interface IVehicleActor : IActor
    {
        Task<bool> ReserveAsync(string user, DateTime startReservation, DateTime endReservation, CancellationToken cancellationToken);

        Task<bool> UnreserveAsync(CancellationToken cancellationToken);

        Task<VehicleInfo> GetInfoAsync(CancellationToken cancellationToken);
    }
}
