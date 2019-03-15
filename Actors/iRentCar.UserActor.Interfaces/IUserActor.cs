using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

//[assembly: FabricTransportActorRemotingProvider(RemotingListener = RemotingListener.V2Listener, RemotingClient = RemotingClient.V2Client)]
namespace iRentCar.UserActor.Interfaces
{

    public interface IUserActor : IActor
    {
        Task<UserActorError> RentVehicleAsync(RentInfo rentInfo, CancellationToken cancellationToken);
        Task<UserActorError> ReleaseVehicleAsync(CancellationToken cancellationToken);
        Task<bool> IsValidAsync(CancellationToken cancellationToken);
        Task<InvoiceInfo> GetActiveInvoiceAsync(CancellationToken cancellationToken);
        Task<UserInfo> GetInfoAsync(CancellationToken cancellationToken);
        Task<UserActorError> UpdateUserInfoAsync(UserInfo info, CancellationToken cancellationToken);
    }
}
