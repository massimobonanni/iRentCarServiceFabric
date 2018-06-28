using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core.Implementations;
using iRentCar.Core.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using iRentCar.VehicleActor.Interfaces;
using iRentCar.VehiclesService.Interfaces;
using VehicleActorInterface = iRentCar.VehicleActor.Interfaces;
using iRentCar.UserActor.Interfaces;
using iRentCar.Core;
using UserActorInterfaces = iRentCar.UserActor.Interfaces;


namespace iRentCar.VehicleActor
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
    [ActorService(Name = "VehicleActor")]
    internal class VehicleActor : Core.Implementations.ActorBase, IVehicleActor
    {
        public VehicleActor(ActorService actorService, ActorId actorId)
            : this(actorService, actorId, new ReliableFactory(), new ReliableFactory(), new VehiclesServiceProxy())
        {

        }

        public VehicleActor(ActorService actorService, ActorId actorId, IActorFactory actorFactory,
            IServiceFactory serviceFactory, IVehiclesServiceProxy vehiclesServiceProxy)
            : base(actorService, actorId, actorFactory, serviceFactory)
        {
            if (vehiclesServiceProxy == null)
                throw new ArgumentNullException(nameof(vehiclesServiceProxy));

            this.vehiclesServiceProxy = vehiclesServiceProxy;
        }

        private readonly IVehiclesServiceProxy vehiclesServiceProxy;


        private const string InfoKeyName = "Info";
        private const string StateKeyName = "State";
        private const string CurrentRentInfoKeyName = "CurrentRent";

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            var stateInfo = await GetVehicleInfoFromStateAsync();
            if (stateInfo == null)
            {
                var info = await this.vehiclesServiceProxy.GetVehicleByPlateAsync(this.Id.GetStringId(),
                    default(CancellationToken));

                VehicleInfo vehicleInfo = null;
                VehicleActorInterface.VehicleState vehicleState = VehicleActorInterface.VehicleState.NotAvailable;
                if (info != null)
                {
                    vehicleInfo = VehicleInfo.FromServiceInterfacesInfo(info);
                    vehicleState = info.State.ToActorInterfaceState();
                }
                await SetVehicleInfoIntoStateAsync(vehicleInfo);
                await SetVehicleStateIntoStateAsync(vehicleState);
            }
        }

        #region [ StateManager accessor ]
        private async Task<VehicleInfo> GetVehicleInfoFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var infoData = await this.StateManager.TryGetStateAsync<VehicleInfo>(InfoKeyName, cancellationToken);
            return infoData.HasValue ? infoData.Value : null;
        }
        private Task SetVehicleInfoIntoStateAsync(VehicleInfo info, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<VehicleInfo>(InfoKeyName, info, cancellationToken);
        }

        private async Task<VehicleActorInterface.VehicleState> GetVehicleStateFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var stateData = await this.StateManager.TryGetStateAsync<VehicleActorInterface.VehicleState>(StateKeyName, cancellationToken);
            return stateData.HasValue ? stateData.Value : VehicleActorInterface.VehicleState.NotAvailable;
        }
        private Task SetVehicleStateIntoStateAsync(VehicleActorInterface.VehicleState state, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<VehicleActorInterface.VehicleState>(StateKeyName, state, cancellationToken);
        }

        private async Task<VehicleActorInterface.RentInfo> GetCurrentRentInfoFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var infoData = await this.StateManager.TryGetStateAsync<VehicleActorInterface.RentInfo>(CurrentRentInfoKeyName, cancellationToken);
            return infoData.HasValue ? infoData.Value : null;
        }
        private Task SetCurrentRentInfoIntoStateAsync(VehicleActorInterface.RentInfo info, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<VehicleActorInterface.RentInfo>(CurrentRentInfoKeyName, info, cancellationToken);
        }
        #endregion [ StateManager accessor ]

        #region [ IVehicleActor interface ]
        public async Task<bool> ReserveAsync(string user, DateTime startReservation, DateTime endReservation, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(user))
                throw new ArgumentException(nameof(user));

            if (startReservation >= endReservation)
                return false;

            var currentState = await GetVehicleStateFromStateAsync(cancellationToken);

            if (currentState != VehicleActorInterface.VehicleState.Free)
                return false;

            var info = await GetVehicleInfoFromStateAsync(cancellationToken);

            var actorProxy = this.actorFactory.Create<IUserActor>(new ActorId(user),
                UriConstants.UserActorUri);

            var response = await actorProxy.RentVehicleAsync(new UserActorInterfaces.RentInfo()
            {
                DailyCost = info.DailyCost,
                Plate = this.Id.ToString()
            }, cancellationToken);

            if (response != UserActorError.Ok)
                return false;

            VehicleActorInterface.RentInfo rentInfo = new VehicleActorInterface.RentInfo() { User = user, StartDate = startReservation, EndDate = endReservation };
            await this.SetCurrentRentInfoIntoStateAsync(rentInfo, cancellationToken);
            await this.SetVehicleStateIntoStateAsync(VehicleActorInterface.VehicleState.Busy);
            var result = await this.vehiclesServiceProxy.UpdateVehicleStateAsync(this.Id.ToString(),
                VehiclesService.Interfaces.VehicleState.Busy, cancellationToken);

            return true;
        }

        public async Task<bool> UnreserveAsync(CancellationToken cancellationToken)
        {
            var currentState = await GetVehicleStateFromStateAsync(cancellationToken);
            if (currentState != VehicleActorInterface.VehicleState.Busy)
                return false;

            var currentRentInfo = await GetCurrentRentInfoFromStateAsync(cancellationToken);
            if (currentRentInfo == null)
                return false;

            var actorProxy = this.actorFactory.Create<IUserActor>(new ActorId(currentRentInfo.User),
                UriConstants.UserActorUri);

            var response = await actorProxy.ReleaseVehicleAsync(cancellationToken);

            if (response != UserActorError.Ok)
                return false;

            await this.SetCurrentRentInfoIntoStateAsync(null, cancellationToken);
            await this.SetVehicleStateIntoStateAsync(VehicleActorInterface.VehicleState.Free);
            var result = await this.vehiclesServiceProxy.UpdateVehicleStateAsync(this.Id.ToString(),
                VehiclesService.Interfaces.VehicleState.Free, cancellationToken);

            return true;
        }

        public async Task<VehicleActorInterface.VehicleInfo> GetInfoAsync(CancellationToken cancellationToken)
        {
            try
            {
                var info = await GetVehicleInfoFromStateAsync(cancellationToken);
                var state = await GetVehicleStateFromStateAsync(cancellationToken);
                var rentInfo = await GetCurrentRentInfoFromStateAsync(cancellationToken);

                var response = info.ToInterfacesInfo();
                response.State = state;
                response.CurrentRent = rentInfo;

                return response;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        #endregion [ IVehicleActor interface ]
    }
}
