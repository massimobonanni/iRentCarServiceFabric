using iRentCar.Core;
using iRentCar.Core.Interfaces;
using iRentCar.InvoiceActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceFabric.Mocks;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace iRentCar.InvoicesService.Test
{
    [TestClass]
    public class InvoicesServiceTest
    {
        private async Task<uint?> GetInvoiceNumberInDictionaryAsync(IReliableStateManagerReplica stateManager, string yearKey)
        {
            var invoiceNumbersDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<string, uint>>(InvoicesService.InvoiceNumbersDictionaryKeyName);
            uint newInvoiceNumber = 0;
            using (var tx = stateManager.CreateTransaction())
            {
                var value = await invoiceNumbersDictionary.TryGetValueAsync(tx, yearKey);
                if (!value.HasValue)
                    return null;
                newInvoiceNumber = value.Value;
            }
            return newInvoiceNumber;
        }

        private async Task SetInvoiceNumberInDictionaryAsync(IReliableStateManagerReplica stateManager, string yearKey, uint invoiceNumber)
        {
            var invoiceNumbersDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<string, uint>>(InvoicesService.InvoiceNumbersDictionaryKeyName);
            using (var tx = stateManager.CreateTransaction())
            {
                await invoiceNumbersDictionary.AddAsync(tx, yearKey, invoiceNumber);
                await tx.CommitAsync();
            }
        }

        #region [ GenerateInvoiceAsync ]

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task GenerateInvoiceAsync_CustomerNull_ThrowException()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();

            var service = new InvoicesService(context, stateManager);

            string customer = null;
            decimal amount = 100M;
            DateTime releaseDate = DateTime.Now;

            var actual = await service.GenerateInvoiceAsync(customer, amount, releaseDate, default(CancellationToken));

        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public async Task GenerateInvoiceAsync_AmountNegative_ThrowException()
        {
            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();

            var service = new InvoicesService(context, stateManager);

            string customer = "testUser";
            decimal amount = -1M;
            DateTime releaseDate = DateTime.Now;

            var actual = await service.GenerateInvoiceAsync(customer, amount, releaseDate, default(CancellationToken));

        }

        [TestMethod]
        public async Task GenerateInvoiceAsync_InvoiceExists_ReturnNull()
        {
            string customer = "testUser";
            decimal amount = 100M;
            DateTime releaseDate = DateTime.Now;

            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var actorFactory = new Mock<IActorFactory>();
            var invoiceActor = new Mock<IInvoiceActor>();

            actorFactory.Setup(f => f.Create<IInvoiceActor>(It.IsAny<ActorId>(), It.Is<Uri>(u => u.AbsoluteUri == UriConstants.InvoiceActorUri), null))
                .Returns(invoiceActor.Object);

            invoiceActor.Setup(i => i.CreateAsync(customer, amount, releaseDate, UriConstants.UserActorUri, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(InvoiceActorError.InvoiceAlreadyExists));


            var yearKey = releaseDate.Year.ToString();
            uint currentInvoiceNumber = 10;
            await SetInvoiceNumberInDictionaryAsync(stateManager, yearKey, currentInvoiceNumber);

            var service = new InvoicesService(context, stateManager, actorFactory.Object, null);

            var actual = await service.GenerateInvoiceAsync(customer, amount, releaseDate, default(CancellationToken));

            Assert.IsNull(actual);
            uint? newInvoiceNumber = await GetInvoiceNumberInDictionaryAsync(stateManager, yearKey);
            Assert.AreEqual(currentInvoiceNumber, newInvoiceNumber.Value);
        }

        [TestMethod]
        public async Task GenerateInvoiceAsync_InvoiceNotExist_ReturnInvoice()
        {
            string customer = "testUser";
            decimal amount = 100M;
            DateTime releaseDate = DateTime.Now;

            var context = MockStatefulServiceContextFactory.Default;
            var stateManager = new MockReliableStateManager();
            var actorFactory = new Mock<IActorFactory>();
            var invoiceActor = new Mock<IInvoiceActor>();

            actorFactory.Setup(f => f.Create<IInvoiceActor>(It.IsAny<ActorId>(), It.Is<Uri>(u => u.AbsoluteUri == UriConstants.InvoiceActorUri), null))
                .Returns(invoiceActor.Object);

            invoiceActor.Setup(i => i.CreateAsync(customer, amount, releaseDate, UriConstants.UserActorUri, It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(InvoiceActorError.Ok));


            var yearKey = releaseDate.Year.ToString();
            uint currentInvoiceNumber = 10;
            await SetInvoiceNumberInDictionaryAsync(stateManager, yearKey, currentInvoiceNumber);

            var service = new InvoicesService(context, stateManager, actorFactory.Object, null);

            var actual = await service.GenerateInvoiceAsync(customer, amount, releaseDate, default(CancellationToken));

            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.Customer, customer);
            Assert.AreEqual(actual.Amount , amount);

            uint? newInvoiceNumber = await GetInvoiceNumberInDictionaryAsync(stateManager, yearKey);
            Assert.AreEqual(currentInvoiceNumber+1, newInvoiceNumber.Value);

        }
        #endregion [ GenerateInvoiceAsync ]
    }
}
