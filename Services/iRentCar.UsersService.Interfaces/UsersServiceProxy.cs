using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using iRentCar.Core.Interfaces;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace iRentCar.UsersService.Interfaces
{
    public sealed class UsersServiceProxy : IUsersServiceProxy
    {
        private static Uri serviceUri;
        private static UsersServiceProxy instance = null;
        private static List<Int64RangePartitionInformation> partitionInfoList = null;

        private static readonly object singletonLock = new object();

        private UsersServiceProxy()
        {

        }

        static UsersServiceProxy()
        {
            serviceUri = new Uri(UriConstants.UsersServiceUri);
        }


        public static UsersServiceProxy Instance
        {
            get
            {
                lock (singletonLock)
                {
                    return instance ?? (instance = new UsersServiceProxy());
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

        private IUsersService CreateServiceProxy(UserInfo user)
        {
            var partitionKey = new ServicePartitionKey(user.PartitionKey);
            return ServiceProxy.Create<IUsersService>(serviceUri, partitionKey);
        }


        public async Task<IEnumerable<UserInfo>> SearchUsersAsync(string username, string firstName, string lastName, string mail, CancellationToken cancellationToken)
        {
            await EnsurePartitionCount();
            IEnumerable<UserInfo> result = null;
            var taskList = new List<Task<List<UserInfo>>>();
            foreach (var partition in partitionInfoList)
            {
                var srvPartitionKey = new ServicePartitionKey(partition.LowKey);
                var proxy = ServiceProxy.Create<IUsersService>(serviceUri, srvPartitionKey);
                taskList.Add(proxy.SearchUsersAsync(username, firstName, lastName, mail, cancellationToken));
            }
            var resultLists = await Task.WhenAll(taskList);
            result = resultLists.SelectMany(t => t).ToList();
            return result;
        }

        public async Task<UserInfo> GetUserByUserNameAsync(string username, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(nameof(username));

            await EnsurePartitionCount();
            UserInfo user = new UserInfo() { Username = username };
            var srvPartitionKey = new ServicePartitionKey(user.PartitionKey);
            var proxy = ServiceProxy.Create<IUsersService>(serviceUri, srvPartitionKey);
            user = await proxy.GetUserByUserNameAsync(username, cancellationToken);

            return user;
        }

        public async Task<bool> AddOrUpdateUserAsync(UserInfo user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await EnsurePartitionCount();
            bool result = false;
            var srvPartitionKey = new ServicePartitionKey(user.PartitionKey);
            var proxy = ServiceProxy.Create<IUsersService>(serviceUri, srvPartitionKey);
            result = await proxy.AddOrUpdateUserAsync(user, cancellationToken);

            return result;
        }
    }
}
