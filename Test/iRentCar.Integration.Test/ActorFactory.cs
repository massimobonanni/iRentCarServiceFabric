using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using iRentCar.Core.Interfaces;
using iRentCar.InvoicesService.Interfaces;
using iRentCar.UsersService.Interfaces;
using iRentCar.VehiclesService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Moq;
using ServiceFabric.Mocks;

namespace iRentCar.Integration.Test
{
    internal static class ActorFactory
    {
        internal static async Task<VehicleActor.VehicleActor> CreateVehicleActorAsync(ActorId id, IActorFactory actorFactory = null,
            IServiceFactory serviceFactory = null, IVehiclesServiceProxy vehiclesServiceProxy = null,
            bool invokeOnActivate = true)
        {
            if (actorFactory == null)
                actorFactory = new Mock<IActorFactory>().Object;
            if (serviceFactory == null)
                serviceFactory = new Mock<IServiceFactory>().Object;
            if (vehiclesServiceProxy == null)
                vehiclesServiceProxy = new Mock<IVehiclesServiceProxy>().Object;

            Func<ActorService, ActorId, ActorBase> factory =
                (service, actorId) => new VehicleActor.VehicleActor(service, id, actorFactory, serviceFactory, vehiclesServiceProxy);

            var svc = MockActorServiceFactory.CreateActorServiceForActor<VehicleActor.VehicleActor>(factory);

            var actor = svc.Activate(id);

            if (invokeOnActivate)
                await actor.InvokeOnActivateAsync();

            return actor;
        }

        internal static async Task<UserActor.UserActor> CreateUserActorAsync(ActorId id, IActorFactory actorFactory = null,
            IServiceFactory serviceFactory = null, IUsersServiceProxy usersServiceProxy = null,
            IInvoicesServiceProxy invoicesServiceProxy = null, bool invokeOnActivate = true)
        {
            if (actorFactory == null)
                actorFactory = new Mock<IActorFactory>().Object;
            if (serviceFactory == null)
                serviceFactory = new Mock<IServiceFactory>().Object;
            if (usersServiceProxy == null)
                usersServiceProxy = new Mock<IUsersServiceProxy>().Object;
            if (invoicesServiceProxy == null)
                invoicesServiceProxy = new Mock<IInvoicesServiceProxy>().Object;

            Func<ActorService, ActorId, ActorBase> factory =
                (service, actorId) => new UserActor.UserActor(service, id, actorFactory, serviceFactory, usersServiceProxy, invoicesServiceProxy);

            var svc = MockActorServiceFactory.CreateActorServiceForActor<UserActor.UserActor>(factory);

            var actor = svc.Activate(id);

            if (invokeOnActivate)
                await actor.InvokeOnActivateAsync();

            return actor;
        }

    }
}
