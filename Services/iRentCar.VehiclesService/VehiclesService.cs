using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core.Interfaces;
using iRentCar.VehiclesService.Interfaces;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using VehicleInfo = iRentCar.VehiclesService.Interfaces.VehicleInfo;
using VehicleState = iRentCar.VehiclesService.Interfaces.VehicleState;
using Microsoft.ServiceFabric.Data;
using System.Fabric.Description;
using iRentCar.VehicleActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using iRentCar.Core;

namespace iRentCar.VehiclesService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class VehiclesService : iRentCar.Core.Implementations.StatefulServiceBase, IVehiclesService
    {
        public VehiclesService(StatefulServiceContext context, IVehiclesRepository vehiclesRepository,
            IActorFactory actorFactory = null, IServiceFactory serviceFactory = null)
            : base(context, actorFactory, serviceFactory)
        {
            if (vehiclesRepository == null)
                throw new ArgumentNullException(nameof(vehiclesRepository));

            this.vehiclesRepository = vehiclesRepository;
        }

        public VehiclesService(StatefulServiceContext context, IReliableStateManagerReplica stateManager, IVehiclesRepository vehiclesRepository,
            IActorFactory actorFactory = null, IServiceFactory serviceFactory = null)
          : base(context, stateManager, actorFactory, serviceFactory)
        {
            if (vehiclesRepository == null)
                throw new ArgumentNullException(nameof(vehiclesRepository));

            this.vehiclesRepository = vehiclesRepository;
        }


        private readonly IVehiclesRepository vehiclesRepository;

        private IReliableDictionary<string, VehicleInfo> vehiclesDictionary;

        internal const string VehiclesDictionaryKeyName = "VehiclesDictionaryKeyName";

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return this.CreateServiceRemotingReplicaListeners();
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.Message($"{this.GetType().Name}.RunAsync");
            await LoadVehiclesDictionaryAsync(cancellationToken);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private async Task LoadVehiclesDictionaryAsync(CancellationToken cancellationToken)
        {
            this.vehiclesRepository.SetServiceHost(this.Context);
            this.vehiclesDictionary =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, VehicleInfo>>(
                    VehiclesDictionaryKeyName);

            var fillDictionary = false;
            using (var trx = this.StateManager.CreateTransaction())
            {
                fillDictionary = await this.vehiclesDictionary.GetCountAsync(trx) == 0;
            }

            if (fillDictionary)
            {
                this.Partition.GetPartitionRange(out var partitionLowKey, out var partitionHighKey);
                
                var vehicles =
                    await this.vehiclesRepository.GetAllVehiclesAsync(partitionLowKey, partitionHighKey,
                        cancellationToken);
                using (var trx = this.StateManager.CreateTransaction())
                {
                    foreach (var vehicleInfo in vehicles)
                    {
                        await this.vehiclesDictionary.SetAsync(trx, vehicleInfo.Plate, new VehicleInfo()
                        {
                            Brand = vehicleInfo.Brand,
                            DailyCost = vehicleInfo.DailyCost,
                            Model = vehicleInfo.Model,
                            Plate = vehicleInfo.Plate,
                            State = VehicleState.Free
                        });
                    }
                    await trx.CommitAsync();
                }
            }
        }

        #region [ Interfaccia IVehiclesService ]

        public async Task<List<VehicleInfo>> SearchVehiclesAsync(string plate, string model, string brand, VehicleState? state, CancellationToken cancellationToken)
        {
            List<VehicleInfo> resultList = new List<VehicleInfo>();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var vehicles = await this.vehiclesDictionary.CreateEnumerableAsync(tx);
                var vehiclesEnumerator = vehicles.GetAsyncEnumerator();

                while (await vehiclesEnumerator.MoveNextAsync(cancellationToken))
                {
                    var vehicle = vehiclesEnumerator.Current.Value;

                    if (vehicle.VerifyFilters(plate, model, brand, state))
                        resultList.Add(vehicle);
                }
                await tx.CommitAsync();
            }
            return resultList;
        }

        public async Task<VehicleInfo> GetVehicleByPlateAsync(string plate, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(plate))
                throw new ArgumentException(nameof(plate));

            VehicleInfo vehicle = null;
            using (var tx = this.StateManager.CreateTransaction())
            {
                var tryVehicle = await this.vehiclesDictionary.TryGetValueAsync(tx, plate, TimeSpan.FromSeconds(5), cancellationToken);
                if (tryVehicle.HasValue)
                    vehicle = tryVehicle.Value;
            }

            return vehicle;
        }

        public async Task<bool> UpdateVehicleStateAsync(string plate, VehicleState newState, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(plate))
                throw new ArgumentException(nameof(plate));

            bool result = false;
            using (var tx = this.StateManager.CreateTransaction())
            {
                var tryVehicle = await this.vehiclesDictionary.TryGetValueAsync(tx, plate, TimeSpan.FromSeconds(5), cancellationToken);
                if (tryVehicle.HasValue)
                {
                    var newVehicle = new VehicleInfo(tryVehicle.Value);
                    newVehicle.State = newState;
                    await this.vehiclesDictionary.SetAsync(tx, plate, newVehicle,
                        TimeSpan.FromSeconds(5), cancellationToken);
                    await tx.CommitAsync();
                    result = true;
                }
            }

            return result;
        }

        public async Task<bool> AddOrUpdateVehicleAsync(VehicleInfo vehicle, CancellationToken cancellationToken)
        {
            if (vehicle == null)
                throw new ArgumentNullException(nameof(vehicle));

            bool result = false;
            if (!string.IsNullOrWhiteSpace(vehicle.Plate))
            {
                using (var tx = this.StateManager.CreateTransaction())
                {
                    await this.vehiclesDictionary.SetAsync(tx, vehicle.Plate, vehicle,
                        TimeSpan.FromSeconds(5), cancellationToken);
                    await tx.CommitAsync();
                    result = true;
                }

                try
                {
                    var vehicleProxy = this.actorFactory.Create<IVehicleActor>(new ActorId(vehicle.Plate),
                        new Uri(UriConstants.VehicleActorUri));

                    await vehicleProxy.UpdateVehicleInfoAsync(vehicle.ToVehicleActorVehicleInfo(), cancellationToken);
                }
                catch {}

            }

            return result;
        }

        #endregion [ Interfaccia IVehiclesService ]
    }
}
