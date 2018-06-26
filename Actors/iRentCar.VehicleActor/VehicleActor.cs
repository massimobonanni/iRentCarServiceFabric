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
using VehicleInfo = iRentCar.VehicleActor.Interfaces.VehicleInfo;
using VehicleActorInterface = iRentCar.VehicleActor.Interfaces;

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
    internal class VehicleActor : Core.Implementations.ActorBase, IVehicleActor
    {
        /// <summary>
        /// Initializes a new instance of VehicleActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public VehicleActor(ActorService actorService, ActorId actorId, IVehiclesServiceProxy vehiclesServiceProxy)
            : this(actorService, actorId, new ReliableFactory(), new ReliableFactory(), vehiclesServiceProxy)
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

        private async Task<RentInfo> GetCurrentRentInfoFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var infoData = await this.StateManager.TryGetStateAsync<RentInfo>(CurrentRentInfoKeyName, cancellationToken);
            return infoData.HasValue ? infoData.Value : null;
        }
        private Task SetCurrentRentInfoIntoStateAsync(RentInfo info, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<RentInfo>(CurrentRentInfoKeyName, info, cancellationToken);
        }
        #endregion [ StateManager accessor ]

        #region [ IVehicleActor interface ]
        public Task<bool> ReserveAsync(string user, DateTime startReservation, DateTime endReservation, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UnreserveAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<VehicleActorInterface.VehicleInfo> GetInfoAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion [ IVehicleActor interface ]
    }
}
