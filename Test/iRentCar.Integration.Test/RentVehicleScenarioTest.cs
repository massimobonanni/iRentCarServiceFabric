using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using iRentCar.Core.Interfaces;
using iRentCar.InvoicesService.Interfaces;
using iRentCar.UserActor.Interfaces;
using iRentCar.UsersService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;
using iRentCar.VehicleActor;
using iRentCar.VehiclesService.Interfaces;

namespace iRentCar.Integration.Test
{
    [TestClass]
    public class RentVehicleScenarioTest
    {


        [TestMethod]
        public async Task RentVehicle_RentOk()
        {
            var vehiclePlate = "FE251JX";
            var vehicleActorId = new ActorId(vehiclePlate);
            var user = "massimo.bonanni";
            var userActorId = new ActorId(user);
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(1);

            var vehiclesServiceState = new MockReliableStateManager();
            var usersServiceState = new MockReliableStateManager();

            var vehiclesRepositoryMock = new Mock<IVehiclesRepository>();
            var usersRepositoryMock = new Mock<IUsersRepository>();
            var actorFactoryMock = new Mock<IActorFactory>();
            var serviceFactoryMock = new Mock<IServiceFactory>();
            var vehiclesServiceProxyMock = new Mock<IVehiclesServiceProxy>();
            var usersServiceProxyMock = new Mock<IUsersServiceProxy>();
            var invoicesServiceProxyMock = new Mock<IInvoicesServiceProxy>();

            var vehiclesService = await ServiceFactory.CreateVehiclesService(vehiclesServiceState,
                vehiclesRepositoryMock.Object, actorFactoryMock.Object, serviceFactoryMock.Object);
            var usersService = await ServiceFactory.CreateUsersService(usersServiceState, usersRepositoryMock.Object,
                actorFactoryMock.Object, serviceFactoryMock.Object);
            var vehicleActor = await ActorFactory.CreateVehicleActorAsync(vehicleActorId, actorFactoryMock.Object,
                serviceFactoryMock.Object, vehiclesServiceProxyMock.Object, false);
            var userActor = await ActorFactory.CreateUserActorAsync(userActorId, actorFactoryMock.Object,
                serviceFactoryMock.Object, usersServiceProxyMock.Object, invoicesServiceProxyMock.Object, false);

            usersRepositoryMock.Setup(r =>
                    r.GetAllUsersAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new Core.Interfaces.UserInfo()
                    {
                        Username = user,
                        FirstName = "Massimo",
                        LastName = "bonanni",
                        Email = "a@a.it",
                        IsEnabled = true
                    }
                }.AsQueryable());

            vehiclesRepositoryMock.Setup(r =>
                    r.GetAllVehiclesAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] {
                    new Core.Interfaces.VehicleInfo()
                    {
                        Plate = vehiclePlate,
                        Brand = "FIAT",
                        Model = "Panda",
                        DailyCost = 50,
                        State = Core.Interfaces.VehicleState.Free
                    }}.AsQueryable());

            vehiclesServiceProxyMock.Setup(s => s.GetVehicleByPlateAsync(vehiclePlate, It.IsAny<CancellationToken>()))
                .Returns(() => vehiclesService.GetVehicleByPlateAsync(vehiclePlate, default(CancellationToken)));

            usersServiceProxyMock.Setup(s => s.GetUserByUserNameAsync(user, It.IsAny<CancellationToken>()))
                .Returns(() => usersService.GetUserByUserNameAsync(user, default(CancellationToken)));

            actorFactoryMock.Setup(s => s.Create<IUserActor>(userActorId, It.Is<Uri>(u => u.AbsoluteUri == UriConstants.UserActorUri), null))
                .Returns(userActor);

            await usersService.InvokeRunAsync();
            await vehiclesService.InvokeRunAsync();

            await vehicleActor.InvokeOnActivateAsync();
            await userActor.InvokeOnActivateAsync();

            var response = await vehicleActor.ReserveAsync(user, startDate, endDate, default(CancellationToken));

            Assert.AreEqual(response, VehicleActor.Interfaces.VehicleActorError.Ok);
        }
    }
}
