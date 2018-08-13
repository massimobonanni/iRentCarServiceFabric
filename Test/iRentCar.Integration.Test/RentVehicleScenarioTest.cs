using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using iRentCar.Core.Interfaces;
using iRentCar.InvoicesService.Interfaces;
using iRentCar.UserActor;
using iRentCar.UserActor.Interfaces;
using iRentCar.UsersService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;
using iRentCar.VehicleActor;
using iRentCar.VehiclesService.Interfaces;
using Microsoft.ServiceFabric.Data.Collections;
using VehicleInfo = iRentCar.VehicleActor.Interfaces.VehicleInfo;

#pragma warning disable 4014 // Warning disabled only in these tests because in some scenarios I want to call awaitable functions in fire and forget mode!

namespace iRentCar.Integration.Test
{
    [TestClass]
    public class RentVehicleScenarioTest
    {
        private class Mocks
        {
            public Mocks()
            {
                VehiclesRepository = new Mock<IVehiclesRepository>();
                UsersRepository = new Mock<IUsersRepository>();
                ActorFactory = new Mock<IActorFactory>();
                ServiceFactory = new Mock<IServiceFactory>();
                VehiclesServiceProxy = new Mock<IVehiclesServiceProxy>();
                UsersServiceProxy = new Mock<IUsersServiceProxy>();
                InvoicesServiceProxy = new Mock<IInvoicesServiceProxy>();
            }

            public Mock<IVehiclesRepository> VehiclesRepository { get; private set; }
            public Mock<IUsersRepository> UsersRepository { get; private set; }
            public Mock<IActorFactory> ActorFactory { get; private set; }
            public Mock<IServiceFactory> ServiceFactory { get; private set; }
            public Mock<IVehiclesServiceProxy> VehiclesServiceProxy { get; private set; }
            public Mock<IUsersServiceProxy> UsersServiceProxy { get; private set; }
            public Mock<IInvoicesServiceProxy> InvoicesServiceProxy { get; private set; }
        }



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

            var mocks = new Mocks();

            var vehiclesService = await ServiceFactory.CreateVehiclesService(vehiclesServiceState,
                mocks.VehiclesRepository.Object, mocks.ActorFactory.Object, mocks.ServiceFactory.Object, false);
            var usersService = await ServiceFactory.CreateUsersService(usersServiceState, mocks.UsersRepository.Object,
                mocks.ActorFactory.Object, mocks.ServiceFactory.Object, false);
            var vehicleActor = await ActorFactory.CreateVehicleActorAsync(vehicleActorId, mocks.ActorFactory.Object,
                mocks.ServiceFactory.Object, mocks.VehiclesServiceProxy.Object, false);
            var userActor = await ActorFactory.CreateUserActorAsync(userActorId, mocks.ActorFactory.Object,
                mocks.ServiceFactory.Object, mocks.UsersServiceProxy.Object, mocks.InvoicesServiceProxy.Object, false);

            mocks.UsersRepository.Setup(r =>
                    r.GetAllUsersAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new Core.Interfaces.UserInfo()
                    {
                        Username = user,
                        FirstName = "Massimo",
                        LastName = "Bonanni",
                        Email = "a@a.it",
                        IsEnabled = true
                    }
                }.AsQueryable());

            mocks.VehiclesRepository.Setup(r =>
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

            mocks.VehiclesServiceProxy.Setup(s => s.GetVehicleByPlateAsync(vehiclePlate, It.IsAny<CancellationToken>()))
                .Returns(() => vehiclesService.GetVehicleByPlateAsync(vehiclePlate, default(CancellationToken)));

            mocks.UsersServiceProxy.Setup(s => s.GetUserByUserNameAsync(user, It.IsAny<CancellationToken>()))
                .Returns(() => usersService.GetUserByUserNameAsync(user, default(CancellationToken)));

            mocks.ActorFactory.Setup(s => s.Create<IUserActor>(userActorId, It.Is<Uri>(u => u.AbsoluteUri == UriConstants.UserActorUri), null))
                .Returns(userActor);

            usersService.InvokeRunAsync();
            vehiclesService.InvokeRunAsync();

            vehicleActor.InvokeOnActivateAsync();
            userActor.InvokeOnActivateAsync();

            var response = await vehicleActor.ReserveAsync(user, startDate, endDate, default(CancellationToken));

            Assert.AreEqual(response, VehicleActor.Interfaces.VehicleActorError.Ok);

            var vehicleData = await vehicleActor.StateManager.TryGetStateAsync<VehicleData>(VehicleActor.VehicleActor.InfoKeyName);
            Assert.IsTrue(vehicleData.HasValue);
            var vehicleState = await vehicleActor.StateManager.TryGetStateAsync<VehicleActor.Interfaces.VehicleState>(VehicleActor.VehicleActor.StateKeyName);
            Assert.IsTrue(vehicleState.HasValue);
            Assert.AreEqual(vehicleState.Value, VehicleActor.Interfaces.VehicleState.Busy);
            var vehicleRentInfo =
                await vehicleActor.StateManager.TryGetStateAsync<VehicleActor.Interfaces.RentInfo>(VehicleActor
                    .VehicleActor.CurrentRentInfoKeyName);
            Assert.IsTrue(vehicleRentInfo.HasValue);
            Assert.AreEqual(vehicleRentInfo.Value.User, user);
            Assert.AreEqual(vehicleRentInfo.Value.StartDate, startDate);
            Assert.AreEqual(vehicleRentInfo.Value.EndDate, endDate);

            var userData = await userActor.StateManager.TryGetStateAsync<UserData>(UserActor.UserActor.UserDataKeyName);
            Assert.IsTrue(userData.HasValue);
            Assert.AreEqual(userData.Value.FirstName, "Massimo");
            Assert.AreEqual(userData.Value.LastName, "Bonanni");
            var userRentData =
                await userActor.StateManager.TryGetStateAsync<RentData>(UserActor.UserActor.CurrentRentedCarKeyName);
            Assert.IsTrue(userRentData.HasValue);
            Assert.AreEqual(userRentData.Value.VehiclePlate, vehiclePlate);
            Assert.AreEqual(userRentData.Value.StartRent, startDate);
            Assert.AreEqual(userRentData.Value.EndRent, endDate);
        }

        [TestMethod]
        public async Task RentVehicle_UserNotExists_RentKO()
        {
            var vehiclePlate = "FE251JX";
            var vehicleActorId = new ActorId(vehiclePlate);
            var user = "massimo.bonanni";
            var userActorId = new ActorId(user);
            var startDate = DateTime.Now;
            var endDate = DateTime.Now.AddDays(1);

            var vehiclesServiceState = new MockReliableStateManager();
            var usersServiceState = new MockReliableStateManager();

            var mocks = new Mocks();

            var vehiclesService = await ServiceFactory.CreateVehiclesService(vehiclesServiceState,
                mocks.VehiclesRepository.Object, mocks.ActorFactory.Object, mocks.ServiceFactory.Object, false);
            var usersService = await ServiceFactory.CreateUsersService(usersServiceState, mocks.UsersRepository.Object,
                mocks.ActorFactory.Object, mocks.ServiceFactory.Object, false);
            var vehicleActor = await ActorFactory.CreateVehicleActorAsync(vehicleActorId, mocks.ActorFactory.Object,
                mocks.ServiceFactory.Object, mocks.VehiclesServiceProxy.Object, false);
            var userActor = await ActorFactory.CreateUserActorAsync(userActorId, mocks.ActorFactory.Object,
                mocks.ServiceFactory.Object, mocks.UsersServiceProxy.Object, mocks.InvoicesServiceProxy.Object, false);

            mocks.UsersRepository.Setup(r =>
                    r.GetAllUsersAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new Core.Interfaces.UserInfo()
                    {
                        Username = user+"1",
                        FirstName = "Massimo",
                        LastName = "Bonanni",
                        Email = "a@a.it",
                        IsEnabled = true
                    }
                }.AsQueryable());

            mocks.VehiclesRepository.Setup(r =>
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

            mocks.VehiclesServiceProxy.Setup(s => s.GetVehicleByPlateAsync(vehiclePlate, It.IsAny<CancellationToken>()))
                .Returns(() => vehiclesService.GetVehicleByPlateAsync(vehiclePlate, default(CancellationToken)));

            mocks.UsersServiceProxy.Setup(s => s.GetUserByUserNameAsync(user, It.IsAny<CancellationToken>()))
                .Returns(() => usersService.GetUserByUserNameAsync(user, default(CancellationToken)));

            mocks.ActorFactory.Setup(s => s.Create<IUserActor>(userActorId, It.Is<Uri>(u => u.AbsoluteUri == UriConstants.UserActorUri), null))
                .Returns(userActor);

            usersService.InvokeRunAsync();
            vehiclesService.InvokeRunAsync();

            vehicleActor.InvokeOnActivateAsync();
            userActor.InvokeOnActivateAsync();

            var response = await vehicleActor.ReserveAsync(user, startDate, endDate, default(CancellationToken));

            Assert.AreEqual(response, VehicleActor.Interfaces.VehicleActorError.GenericError);

            var vehicleData = await vehicleActor.StateManager.TryGetStateAsync<VehicleData>(VehicleActor.VehicleActor.InfoKeyName);
            Assert.IsTrue(vehicleData.HasValue);
            var vehicleState = await vehicleActor.StateManager.TryGetStateAsync<VehicleActor.Interfaces.VehicleState>(VehicleActor.VehicleActor.StateKeyName);
            Assert.IsTrue(vehicleState.HasValue);
            Assert.AreEqual(vehicleState.Value, VehicleActor.Interfaces.VehicleState.Free);
            var vehicleRentInfo =
                await vehicleActor.StateManager.TryGetStateAsync<VehicleActor.Interfaces.RentInfo>(VehicleActor
                    .VehicleActor.CurrentRentInfoKeyName);
            Assert.IsFalse(vehicleRentInfo.HasValue);

            var userData = await userActor.StateManager.TryGetStateAsync<UserData>(UserActor.UserActor.UserDataKeyName);
            Assert.IsFalse(userData.HasValue);
        }
    }
}
