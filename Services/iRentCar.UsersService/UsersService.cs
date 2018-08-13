using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using iRentCar.UsersService.Interfaces;
using iRentCar.Core.Interfaces;
using Microsoft.ServiceFabric.Actors;
using UserInfo = iRentCar.UsersService.Interfaces.UserInfo;
using Microsoft.ServiceFabric.Data;
using iRentCar.UserActor.Interfaces;

namespace iRentCar.UsersService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class UsersService : iRentCar.Core.Implementations.StatefulServiceBase, IUsersService
    {
        public UsersService(StatefulServiceContext context)
            : base(context)
        { }
        public UsersService(StatefulServiceContext context, IUsersRepository usersRepository,
            IActorFactory actorFactory = null, IServiceFactory serviceFactory = null)
            : base(context, actorFactory, serviceFactory)
        {
            if (usersRepository == null)
                throw new ArgumentNullException(nameof(usersRepository));

            this.usersRepository = usersRepository;
        }

        public UsersService(StatefulServiceContext context, IReliableStateManagerReplica stateManager, IUsersRepository usersRepository,
            IActorFactory actorFactory = null, IServiceFactory serviceFactory = null)
          : base(context, stateManager, actorFactory, serviceFactory)
        {
            if (usersRepository == null)
                throw new ArgumentNullException(nameof(usersRepository));

            this.usersRepository = usersRepository;
        }


        private readonly IUsersRepository usersRepository;

        private IReliableDictionary<string, UserInfo> usersDictionary;

        private const string UsersDictionaryKeyName = "UsersDictionaryKeyName";


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
            try
            {
                await LoadUsersDictionaryAsync(cancellationToken);
            }
            catch (Exception)
            {

            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        private async Task LoadUsersDictionaryAsync(CancellationToken cancellationToken)
        {
            this.usersRepository.SetServiceHost(this.Context);
            this.usersDictionary =
                await this.StateManager.GetOrAddAsync<IReliableDictionary<string, UserInfo>>(
                    UsersDictionaryKeyName);

            var fillDictionary = false;
            using (var trx = this.StateManager.CreateTransaction())
            {
                fillDictionary = await this.usersDictionary.GetCountAsync(trx) == 0;
            }

            if (fillDictionary)
            {
                this.Partition.GetPartitionRange(out var partitionLowKey, out var partitionHighKey);

                var users =
                    await this.usersRepository.GetAllUsersAsync(partitionLowKey, partitionHighKey, cancellationToken);

                using (var trx = this.StateManager.CreateTransaction())
                {
                    foreach (var userInfo in users)
                    {
                        await this.usersDictionary.SetAsync(trx, userInfo.Username, new UserInfo()
                        {
                            Username = userInfo.Username,
                            Email = userInfo.Email,
                            FirstName = userInfo.FirstName,
                            LastName = userInfo.LastName,
                            IsEnabled = userInfo.IsEnabled
                        });
                    }
                    await trx.CommitAsync();
                }
            }
        }

        #region [ Interfaccia IUsersService ]
        public async Task<bool> AddOrUpdateUserAsync(UserInfo user, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            bool result = false;
            if (!string.IsNullOrWhiteSpace(user.Username))
            {
                using (var tx = this.StateManager.CreateTransaction())
                {
                    await this.usersDictionary.SetAsync(tx, user.Username, user,
                        TimeSpan.FromSeconds(5), cancellationToken);
                    await tx.CommitAsync();
                    result = true;
                }

                try
                {
                    var userProxy = this.actorFactory.Create<IUserActor>(new ActorId(user.Username),
                        new Uri(UriConstants.UserActorUri));

                    await userProxy.UpdateUserInfoAsync(user.ToUserActorUserInfo(), cancellationToken);
                }
                catch { }

            }

            return result;
        }

        public async Task<UserInfo> GetUserByUserNameAsync(string username, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(nameof(username));

            UserInfo user = null;
            using (var tx = this.StateManager.CreateTransaction())
            {
                var tryUser = await this.usersDictionary.TryGetValueAsync(tx, username, TimeSpan.FromSeconds(5), cancellationToken);
                if (tryUser.HasValue)
                    user = tryUser.Value;
            }

            return user;
        }

        public async Task<List<UserInfo>> SearchUsersAsync(string username, string firstName, string lastName, string mail, CancellationToken cancellationToken)
        {
            List<UserInfo> resultList = new List<UserInfo>();
            using (var tx = this.StateManager.CreateTransaction())
            {
                var users = await this.usersDictionary.CreateEnumerableAsync(tx);
                var usersEnumerator = users.GetAsyncEnumerator();

                while (await usersEnumerator.MoveNextAsync(cancellationToken))
                {
                    var user = usersEnumerator.Current.Value;

                    if (user.VerifyFilters(username, firstName, lastName, mail))
                        resultList.Add(user);
                }
                await tx.CommitAsync();
            }
            return resultList;
        }
        #endregion [ Interfaccia IUsersService ]
    }
}
