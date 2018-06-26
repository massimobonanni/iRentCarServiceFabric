using System;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core.Implementations;
using iRentCar.Core.Interfaces;
using iRentCar.UserActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace iRentCar.UserActor
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
    [ActorService(Name = "UserActor")]
    internal class UserActor : Core.Implementations.ActorBase, IUserActor
    {
        /// <summary>
        /// Initializes a new instance of UserActor
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public UserActor(ActorService actorService, ActorId actorId, IUsersRepository userRepository)
            : this(actorService, actorId, new ReliableFactory(), new ReliableFactory(), userRepository)
        {

        }

        public UserActor(ActorService actorService, ActorId actorId, IActorFactory actorFactory,
            IServiceFactory serviceFactory, IUsersRepository userRepository)
            : base(actorService, actorId, actorFactory, serviceFactory)
        {
            if (userRepository == null)
                throw new ArgumentNullException(nameof(userRepository));

            this.userRepository = userRepository;
        }

        private readonly IUsersRepository userRepository;

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");



            return Task.CompletedTask;
        }

        private const string CurrentRentedCarKeyName = "CurrentRentedCar";
        private const string InvoikeKeyNamePrefix = "Invoice_";
        private const string UserDataKeyName = "UserInfo";


        public async Task<UserActorError> RentVehicleAsync(RentInfo rentInfo, CancellationToken cancellationToken)
        {
            if (rentInfo == null)
                throw new ArgumentNullException(nameof(rentInfo));

            UserActorError result = UserActorError.Ok;

            var currentRentVehicle = await GetCurrentRentVehicleAsync(cancellationToken);

            if (currentRentVehicle == null)
            {
                currentRentVehicle = new RentData()
                {
                    StartRent = DateTime.Now,
                    VehicleDailyCost = rentInfo.DailyCost,
                    VehiclePlate = rentInfo.Plate,
                    State = RentState.Active
                };
                await SetCurrentRentVehicleAsync(currentRentVehicle, cancellationToken);
            }
            else
            {
                result = UserActorError.VehicleAlreadyRented;
            }

            return result;

        }

        public Task<UserActorError> ReleaseVehicleAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsValidAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<InvoiceInfo> GetActiveInvoiceAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        #region [ Interazione con lo stato ]

        private async Task<RentData> GetCurrentRentVehicleAsync(CancellationToken cancellationToken)
        {
            var rentData =
                await this.StateManager.TryGetStateAsync<RentData>(CurrentRentedCarKeyName, cancellationToken);

            return rentData.HasValue ? rentData.Value : null;
        }

        private async Task SetCurrentRentVehicleAsync(RentData currentRentVehicle, CancellationToken cancellationToken)
        {
            await this.StateManager.SetStateAsync(CurrentRentedCarKeyName, currentRentVehicle, cancellationToken);

        }
        #endregion [ Interazione con lo stato ]
    }
}
