using iRentCar.Core.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.InvoiceActor.Test
{
    [TestClass]
    public class InvoiceActorTest
    {
        internal static InvoiceActor CreateActor(ActorId id, IActorFactory actorFactory = null, IServiceFactory serviceFactory = null)
        {
            if (actorFactory == null)
            {
                actorFactory = new Mock<IActorFactory>().Object;
            }
            if (serviceFactory == null)
            {
                serviceFactory = new Mock<IServiceFactory>().Object;
            }
           
            Func<ActorService, ActorId, ActorBase> factory = (service, actorId) => new InvoiceActor(service, id, actorFactory, serviceFactory);
            var svc = MockActorServiceFactory.CreateActorServiceForActor<InvoiceActor>(factory);
            var actor = svc.Activate(id);
            return actor;
        }

        #region [ CreateAsync ]

        [TestMethod]
        public async Task CreateAsync_InputDataOk_ResultOk()
        {
            var actorGuid = Guid.NewGuid();
            var id = new ActorId(actorGuid);

            var actor = CreateActor(id, null, null);
            var stateManager = (MockActorStateManager)actor.StateManager;

            var customer = "massimo.bonanni";
            var amount = 100.0M;
            var creationDate = DateTime.Now;

            var actual = await actor.CreateAsync(customer, amount, creationDate, null, default(CancellationToken));

            Assert.AreEqual(actual, Interfaces.InvoiceActorError.Ok);
            var invoiceData = await stateManager.GetStateAsync<InvoiceData>(InvoiceActor.InvoiceDataKeyName);
            Assert.IsNotNull(invoiceData);
            Assert.AreEqual(invoiceData.Customer, customer);
            Assert.AreEqual(invoiceData.Amount, amount);
            Assert.AreEqual(invoiceData.CreationDate, creationDate);
            Assert.IsFalse(invoiceData.PaymentDate.HasValue);
            Assert.AreEqual(invoiceData.State, Interfaces.InvoiceState.NotPaid);
            Assert.IsNull(invoiceData.CallbackUri);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task CreateAsync_CustomerNull_ThrowException()
        {
            var actorGuid = Guid.NewGuid();
            var id = new ActorId(actorGuid);

            var actor = CreateActor(id, null, null);
            var stateManager = (MockActorStateManager)actor.StateManager;

            string customer = null;
            var amount = 100.0M;
            var creationDate = DateTime.Now;

            var actual = await actor.CreateAsync(customer, amount, creationDate, null, default(CancellationToken));

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task CreateAsync_AmountLessZero_ThrowException()
        {
            var actorGuid = Guid.NewGuid();
            var id = new ActorId(actorGuid);

            var actor = CreateActor(id, null, null);
            var stateManager = (MockActorStateManager)actor.StateManager;

            string customer = "massimo.bonanni";
            var amount = -1;
            var creationDate = DateTime.Now;

            var actual = await actor.CreateAsync(customer, amount, creationDate, null, default(CancellationToken));

        }
        #endregion [ CreateAsync ]
    }
}
