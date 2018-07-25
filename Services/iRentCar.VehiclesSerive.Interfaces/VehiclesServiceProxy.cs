using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace iRentCar.VehiclesService.Interfaces
{
    public sealed class VehiclesServiceProxy : IVehiclesServiceProxy
    {
        private static Uri serviceUri;
        private static VehiclesServiceProxy instance = null;
        private static List<Int64RangePartitionInformation> partitionInfoList = null;

        private static readonly object singletonLock = new object();

        private VehiclesServiceProxy()
        {

        }

        static VehiclesServiceProxy()
        {
            serviceUri = new Uri(UriConstants.VehiclesServiceUri);
        }


        public static VehiclesServiceProxy Instance
        {
            get
            {
                lock (singletonLock)
                {
                    return instance ?? (instance = new VehiclesServiceProxy());
                }
            }
        }

        private async Task EnsurePartitionCount()
        {
            if (partitionInfoList == null || !partitionInfoList.Any())
            {
                using (var client = new FabricClient())
                {
                    var partitionList = await client.QueryManager.GetPartitionListAsync(serviceUri);

                    partitionInfoList = partitionList.Select(p => p.PartitionInformation)
                        .OfType<Int64RangePartitionInformation>().ToList();
                }
            }
        }

        private IVehiclesService CreateServiceProxy(VehicleInfo vehicle)
        {
            var partitionKey = new ServicePartitionKey(vehicle.PartitionKey);
            return ServiceProxy.Create<IVehiclesService>(serviceUri, partitionKey);
        }

        public async Task<IEnumerable<VehicleInfo>> SearchVehiclesAsync(string plate, string model, string brand,
            VehicleState? state, CancellationToken cancellationToken)
        {
            await EnsurePartitionCount();
            IEnumerable<VehicleInfo> result = null;
            var taskList = new List<Task<List<VehicleInfo>>>();
            try
            {
                foreach (var partition in partitionInfoList)
                {
                    var srvPartitionKey = new ServicePartitionKey(partition.LowKey);
                    var proxy = ServiceProxy.Create<IVehiclesService>(serviceUri, srvPartitionKey);
                    taskList.Add(proxy.SearchVehiclesAsync(plate, model, brand, state, cancellationToken));
                }
                var resultLists=await Task.WhenAll(taskList);
                result = resultLists.SelectMany(t => t).ToList();
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }

        public async Task<VehicleInfo> GetVehicleByPlateAsync(string plate, CancellationToken cancellationToken)
        {
            await EnsurePartitionCount();
            VehicleInfo result = null;
            try
            {
                var vehicle = new VehicleInfo() { Plate = plate };
                var srvPartitionKey = new ServicePartitionKey(vehicle.PartitionKey);
                var proxy = ServiceProxy.Create<IVehiclesService>(serviceUri, srvPartitionKey);
                result = await proxy.GetVehicleByPlateAsync(plate, cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }

        public async Task<bool> UpdateVehicleStateAsync(string plate, VehicleState newState, CancellationToken cancellationToken)
        {
            await EnsurePartitionCount();
            bool result = false;
            try
            {
                var vehicle = new VehicleInfo() { Plate = plate };
                var srvPartitionKey = new ServicePartitionKey(vehicle.PartitionKey);
                var proxy = ServiceProxy.Create<IVehiclesService>(serviceUri, srvPartitionKey);
                result = await proxy.UpdateVehicleStateAsync(plate, newState, cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }

        public async Task<bool> AddOrUpdateVehicleAsync(VehicleInfo vehicle, CancellationToken cancellationToken)
        {
            await EnsurePartitionCount();
            bool result = false;
            try
            {
                var srvPartitionKey = new ServicePartitionKey(vehicle.PartitionKey);
                var proxy = ServiceProxy.Create<IVehiclesService>(serviceUri, srvPartitionKey);
                result = await proxy.AddOrUpdateVehicleAsync(vehicle, cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }

            return result;
        }
    }
}
