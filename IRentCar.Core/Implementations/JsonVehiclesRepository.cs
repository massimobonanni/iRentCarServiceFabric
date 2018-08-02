using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core.Interfaces;
using Newtonsoft.Json;

namespace iRentCar.Core.Implementations
{
    public class JsonVehiclesRepository : IVehiclesRepository
    {
        private ServiceContext hostContext;

        private static IEnumerable<VehicleInfo> vehicles;
        private object vehiclesSyncObject=new object();


        public async Task<IQueryable<VehicleInfo>> GetAllVehiclesAsync(long lowPartitionKey, long highPartitionKey, CancellationToken token)
        {
            if (vehicles == null)
            {
                await LoadVehiclesFromFileAsync(token);
            }

            var query = vehicles.Where(a => a.PartitionKey >= lowPartitionKey)
                .Where(a => a.PartitionKey <= highPartitionKey);

            return query.AsQueryable();
        }

        private async Task LoadVehiclesFromFileAsync( CancellationToken token)
        {
            var dataPkg = hostContext.CodePackageActivationContext.GetDataPackageObject("Data");

            var customDataFilePath = $@"{dataPkg.Path}\vehicles.json";

            string fileContent;
            using (var reader = File.OpenText(customDataFilePath))
            {
                fileContent = await reader.ReadToEndAsync();
            }

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                lock (vehiclesSyncObject)
                {
                    vehicles = JsonConvert.DeserializeObject<List<VehicleInfo>>(fileContent);
                }
            }

        }

        public void SetServiceHost(ServiceContext hostContext)
        {
            if (hostContext == null)
                throw new ArgumentNullException(nameof(hostContext));

            this.hostContext = hostContext;
        }
    }
}
