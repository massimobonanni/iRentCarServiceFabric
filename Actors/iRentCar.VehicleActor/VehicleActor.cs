﻿using System;
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
            : this(actorService, actorId, new ReliableFactory(), new ReliableFactory(), null)
        {

        }

        public VehicleActor(ActorService actorService, ActorId actorId, IActorFactory actorFactory,
            IServiceFactory serviceFactory, IVehiclesServiceProxy vehiclesServiceProxy)
            : base(actorService, actorId, actorFactory, serviceFactory)
        {
            if (vehiclesServiceProxy == null)
                this.vehiclesServiceProxy = VehiclesServiceProxy.Instance;
            else
                this.vehiclesServiceProxy = vehiclesServiceProxy;
        }

        private readonly IVehiclesServiceProxy vehiclesServiceProxy;


        internal const string InfoKeyName = "Info";
        internal const string StateKeyName = "State";
        internal const string CurrentRentInfoKeyName = "CurrentRent";

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            var stateInfo = await GetVehicleDataFromStateAsync();
            if (stateInfo == null)
            {
                var info = await this.vehiclesServiceProxy.GetVehicleByPlateAsync(this.Id.GetStringId(),
                    default(CancellationToken));

                VehicleData vehicleInfo = null;
                VehicleActorInterface.VehicleState vehicleState = VehicleActorInterface.VehicleState.NotAvailable;
                if (info != null)
                {
                    vehicleInfo = VehicleData.FromServiceInterfacesInfo(info);
                    vehicleState = info.State.ToActorInterfaceState();
                }
                await SetVehicleDataIntoStateAsync(vehicleInfo);
                await SetVehicleStateIntoStateAsync(vehicleState);
            }
        }

        #region [ StateManager accessor ]
        private async Task<VehicleData> GetVehicleDataFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var infoData = await this.StateManager.TryGetStateAsync<VehicleData>(InfoKeyName, cancellationToken);
            return infoData.HasValue ? infoData.Value : null;
        }
        private Task SetVehicleDataIntoStateAsync(VehicleData data, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync<VehicleData>(InfoKeyName, data, cancellationToken);
        }

        private async Task<VehicleActorInterface.VehicleState> GetVehicleStateFromStateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var stateData = await this.StateManager.TryGetStateAsync<VehicleActorInterface.VehicleState>(StateKeyName, cancellationToken);
            return stateData.HasValue ? stateData.Value : VehicleActorInterface.VehicleState.NotAvailable;
        }
        private Task SetVehicleStateIntoStateAsync(VehicleActorInterface.VehicleState state, CancellationToken cancellationToken = default(CancellationToken))
        {
            return this.StateManager.SetStateAsync(StateKeyName, state, cancellationToken);
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
        public async Task<VehicleActorError> ReserveAsync(string user, DateTime startReservation, DateTime endReservation, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(user))
                throw new ArgumentException(nameof(user));

            if (startReservation >= endReservation)
                return VehicleActorError.ReservationDatesWrong;

            var info = await GetVehicleDataFromStateAsync(cancellationToken);
            if (info == null)
                return VehicleActorError.VehicleNotExists;

            var currentState = await GetVehicleStateFromStateAsync(cancellationToken);

            if (currentState == VehicleActorInterface.VehicleState.Busy)
                return VehicleActorError.VehicleBusy;
            if (currentState == VehicleActorInterface.VehicleState.NotAvailable)
                return VehicleActorError.VehicleNotAvailable;

            var actorProxy = this.actorFactory.Create<IUserActor>(new ActorId(user),
                new Uri(UriConstants.UserActorUri));

            var rentinfo = new UserActorInterfaces.RentInfo()
            {
                DailyCost = info.DailyCost,
                Plate = this.Id.ToString(),
                StartRent = startReservation,
                EndRent = endReservation
            };

            //var response = await actorProxy.RentVehicleAsync(rentinfo, cancellationToken);

            var response = await actorProxy.CallWithPolicyForTimeoutAsync<IUserActor, UserActorError>(c => actorProxy.RentVehicleAsync(rentinfo, c),
                5, i => TimeSpan.FromMilliseconds(100 * i), cancellationToken);

            if (response == UserActorError.UserNotValid)
                return VehicleActorError.UserNotValid;

            if (response != UserActorError.Ok)
                return VehicleActorError.GenericError;

            VehicleActorInterface.RentInfo rentInfo = new VehicleActorInterface.RentInfo() { User = user, StartDate = startReservation, EndDate = endReservation };
            await this.SetCurrentRentInfoIntoStateAsync(rentInfo, cancellationToken);
            await this.SetVehicleStateIntoStateAsync(VehicleActorInterface.VehicleState.Busy, cancellationToken);
            var result = await this.vehiclesServiceProxy.UpdateVehicleStateAsync(this.Id.ToString(),
                VehiclesService.Interfaces.VehicleState.Busy, cancellationToken);

            return VehicleActorError.Ok;
        }

        public async Task<VehicleActorError> UnreserveAsync(CancellationToken cancellationToken)
        {
            var currentRentInfo = await GetCurrentRentInfoFromStateAsync(cancellationToken);
            if (currentRentInfo == null)
                return VehicleActorError.VehicleNotExists;

            var currentState = await GetVehicleStateFromStateAsync(cancellationToken);

            if (currentState == VehicleActorInterface.VehicleState.Free)
                return VehicleActorError.VehicleFree;
            if (currentState == VehicleActorInterface.VehicleState.NotAvailable)
                return VehicleActorError.VehicleNotAvailable;

            var actorProxy = this.actorFactory.Create<IUserActor>(new ActorId(currentRentInfo.User),
                        new Uri(UriConstants.UserActorUri));

            var response = await actorProxy.ReleaseVehicleAsync(cancellationToken);

            if (response != UserActorError.Ok)
                return VehicleActorError.GenericError;

            await this.SetCurrentRentInfoIntoStateAsync(null, cancellationToken);
            await this.SetVehicleStateIntoStateAsync(VehicleActorInterface.VehicleState.Free, cancellationToken);
            var result = await this.vehiclesServiceProxy.UpdateVehicleStateAsync(this.Id.ToString(),
                VehiclesService.Interfaces.VehicleState.Free, cancellationToken);

            return VehicleActorError.Ok;
        }

        public async Task<VehicleActorInterface.VehicleInfo> GetInfoAsync(CancellationToken cancellationToken)
        {
            var info = await GetVehicleDataFromStateAsync(cancellationToken);
            if (info == null)
                return null;

            var state = await GetVehicleStateFromStateAsync(cancellationToken);
            var rentInfo = await GetCurrentRentInfoFromStateAsync(cancellationToken);

            VehicleActorInterface.VehicleInfo response = info.ToInterfacesInfo();
            response.Plate = this.Id.ToString();
            response.State = state;
            response.CurrentRent = rentInfo;

            return response;
        }

        public async Task<VehicleActorError> UpdateVehicleInfoAsync(VehicleActorInterface.VehicleInfo info,
            CancellationToken cancellationToken)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(InfoKeyName));

            var vehicleData = VehicleData.FromActorInterfacesInfo(info);
            await SetVehicleDataIntoStateAsync(vehicleData, cancellationToken);

            await SetVehicleStateIntoStateAsync(info.State, cancellationToken);

            return VehicleActorError.Ok;
        }
        #endregion [ IVehicleActor interface ]
    }
}
