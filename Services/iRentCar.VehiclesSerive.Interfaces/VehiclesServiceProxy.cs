﻿using System;
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
    public interface IVehiclesServiceProxy
    {
        Task<IEnumerable<VehicleInfo>> SearchVehiclesAsync(string plate, string model, string brand,
            VehicleState? state, CancellationToken cancellationToken);
    }

    public sealed class VehiclesServiceProxy : IVehiclesServiceProxy
    {
        private static Uri serviceUri;
        private static VehiclesServiceProxy instance = null;
        private static List<Int64RangePartitionInformation> partitionInfoList = null;

        private static readonly object singletonLock = new object();

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
            var taskList = new List<Task<List<VehicleInfo>>>();
            try
            {
                foreach (var partition in partitionInfoList)
                {
                    var srvPartitionKey = new ServicePartitionKey(partition.LowKey);
                    var proxy = ServiceProxy.Create<IVehiclesService>(serviceUri, srvPartitionKey);
                    taskList.Add(proxy.SearchVehiclesAsync(plate, model, brand, state, cancellationToken));
                }
                await Task.WhenAll(taskList);
            }
            catch (Exception ex)
            {
                var a = 1;
            }

            var result = taskList.SelectMany(t => t.Result).ToList();
            return result;
        }
    }
}
