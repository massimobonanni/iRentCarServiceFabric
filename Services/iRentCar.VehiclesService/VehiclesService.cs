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

namespace iRentCar.VehiclesService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class VehiclesService : StatefulService, IVehiclesService
    {
        public VehiclesService(StatefulServiceContext context, IVehiclesRepository vehiclesRepository)
            : base(context)
        {
            if (vehiclesRepository == null)
                throw new ArgumentNullException(nameof(vehiclesRepository));

            this.vehiclesRepository = vehiclesRepository;
        }

        private readonly IVehiclesRepository vehiclesRepository;

        private IReliableDictionary<string, VehicleInfo> vehiclesDictionary;

        private const string VehiclesDictionaryKeyName = "VehiclesDictionaryKeyName";

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
            await LoadVehiclesDictionaryAsync(cancellationToken);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private async Task LoadVehiclesDictionaryAsync(CancellationToken cancellationToken)
        {
            var dictionary = await this.StateManager.TryGetAsync<IReliableDictionary<string, VehicleInfo>>(VehiclesDictionaryKeyName);
            using (var trx = this.StateManager.CreateTransaction())
            {
                if (!dictionary.HasValue || await dictionary.Value.GetCountAsync(trx) == 0)
                {
                    this.vehiclesDictionary = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, VehicleInfo>>(VehiclesDictionaryKeyName);
                    var partition = (Int64RangePartitionInformation)this.Partition.PartitionInfo;
                    var vehicles = await this.vehiclesRepository.GetAllVehiclesAsync(partition.LowKey, partition.HighKey);

                    foreach (var vehicleInfo in vehicles)
                    {
                        await this.vehiclesDictionary.AddAsync(trx, vehicleInfo.Plate, new VehicleInfo()
                        {
                            Brand = vehicleInfo.Brand,
                            DailyCost = vehicleInfo.DailyCost,
                            Model = vehicleInfo.Model,
                            Plate = vehicleInfo.Plate,
                            State = VehicleState.Free
                        }, TimeSpan.FromSeconds(4), cancellationToken);
                    }
                }
                else
                    this.vehiclesDictionary = dictionary.Value;

                await trx.CommitAsync();
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

                    var vehicleInResult = true;
                    if (!string.IsNullOrWhiteSpace(plate))
                        if (vehicle.Plate != plate)
                            vehicleInResult = false;

                    if (!string.IsNullOrWhiteSpace(model) && vehicleInResult)
                        if (vehicle.Model != model)
                            vehicleInResult = false;

                    if (!string.IsNullOrWhiteSpace(brand) && vehicleInResult)
                        if (vehicle.Brand != brand)
                            vehicleInResult = false;

                    if (state.HasValue && vehicleInResult)
                        if (vehicle.State != state.Value)
                            vehicleInResult = false;

                    if (vehicleInResult)
                        resultList.Add(vehicle);
                }
                await tx.CommitAsync();
            }
            return resultList;
        }

        #endregion [ Interfaccia IVehiclesService ]
    }
}
