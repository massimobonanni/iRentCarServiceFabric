using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using iRentCar.Core;
using iRentCar.Core.Interfaces;
using Microsoft.ServiceFabric.Data;
using Moq;
using ServiceFabric.Mocks;

namespace iRentCar.Integration.Test
{
    internal static class ServiceFactory
    {
        internal static async Task<VehiclesService.VehiclesService> CreateVehiclesService(IReliableStateManagerReplica stateManager = null,
            IVehiclesRepository vehiclesRepository = null, IActorFactory actorFactory = null, IServiceFactory serviceFactory = null,
            bool invokeRunAsync = true)
        {
            var context = MockStatefulServiceContextFactory.Default;

            if (stateManager == null)
                stateManager = new MockReliableStateManager();
            if (vehiclesRepository == null)
                vehiclesRepository = new Mock<IVehiclesRepository>().Object;
            if (actorFactory == null)
                actorFactory = new Mock<IActorFactory>().Object;
            if (serviceFactory == null)
                serviceFactory = new Mock<IServiceFactory>().Object;

            var service = new VehiclesService.VehiclesService(context, stateManager, vehiclesRepository, actorFactory, serviceFactory);

            if (invokeRunAsync)
                await service.InvokeRunAsync();

            return service;
        }

        internal static async Task<UsersService.UsersService> CreateUsersService(IReliableStateManagerReplica stateManager = null,
            IUsersRepository usersRepository = null, IActorFactory actorFactory = null, IServiceFactory serviceFactory = null,
            bool invokeRunAsync = true)
        {
            var context = MockStatefulServiceContextFactory.Create(MockCodePackageActivationContext.Default,
                "UsersServiceType", new Uri(UriConstants.UsersServiceUri), Guid.NewGuid(), DateTime.Now.Ticks);

            if (stateManager == null)
                stateManager = new MockReliableStateManager();
            if (usersRepository == null)
                usersRepository = new Mock<IUsersRepository>().Object;
            if (actorFactory == null)
                actorFactory = new Mock<IActorFactory>().Object;
            if (serviceFactory == null)
                serviceFactory = new Mock<IServiceFactory>().Object;

            var service = new UsersService.UsersService(context, stateManager, usersRepository, actorFactory, serviceFactory);
            
            if (invokeRunAsync)
                await service.InvokeRunAsync();

            return service;
        }
    }
}
