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
    public class JsonUsersRepository : IUsersRepository
    {
        private ServiceContext hostContext;

        private static IEnumerable<UserInfo> users;
        private readonly object usersSyncObject=new object();


       private async Task LoadUsersFromFileAsync( CancellationToken token)
        {
            var dataPkg = hostContext.CodePackageActivationContext.GetDataPackageObject("Data");

            var customDataFilePath = $@"{dataPkg.Path}\users.json";

            string fileContent;
            using (var reader = File.OpenText(customDataFilePath))
            {
                fileContent = await reader.ReadToEndAsync();
            }

            if (!string.IsNullOrWhiteSpace(fileContent))
            {
                lock (usersSyncObject)
                {
                    users = JsonConvert.DeserializeObject<List<UserInfo>>(fileContent);
                }
            }

        }

        public void SetServiceHost(ServiceContext hostContext)
        {
            if (hostContext == null)
                throw new ArgumentNullException(nameof(hostContext));

            this.hostContext = hostContext;
        }

        public async Task<IQueryable<UserInfo>> GetAllUsersAsync(long lowPartitionKey, long highPartitionKey, CancellationToken token)
        {
            if (users == null)
            {
                await LoadUsersFromFileAsync(token);
            }

            var query = users.Where(a => a.PartitionKey >= lowPartitionKey)
                .Where(a => a.PartitionKey <= highPartitionKey);

            return query.AsQueryable();
        }
    }
}
