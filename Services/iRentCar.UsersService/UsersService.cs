using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using iRentCar.UsersService.Interfaces;
using iRentCar.Core.Interfaces;
using UserInfo = iRentCar.UsersService.Interfaces.UserInfo;
using Microsoft.ServiceFabric.Data;

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

        private IReliableDictionary<string, UserInfo> UsersDictionary;

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

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        #region [ Interfaccia IUsersService ]
        public Task<bool> AddOrUpdateUserAsync(UserInfo user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserInfo> GetUserByUserNameAsync(string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<UserInfo>> SearchUsersAsync(string username, string firstName, string lastName, string mail, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        #endregion [ Interfaccia IUsersService ]
    }
}
