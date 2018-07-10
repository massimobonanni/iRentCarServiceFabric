using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using iRentCar.Core.Interfaces;
using iRentCar.InvoiceActor.Interfaces;
using iRentCar.PaymentGateway.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;

namespace iRentCar.PaymentGateway.Controllers
{
    [Route("api/[controller]")]
    public class PaymentController : Controller
    {
        private readonly IActorFactory actorFactory;
        private readonly IPaymentAdapter paymentAdapter;

        public PaymentController(IActorFactory actorFactory, IPaymentAdapter paymentAdapter)
        {
            if (actorFactory == null)
                throw new ArgumentNullException(nameof(actorFactory));
            if (paymentAdapter == null)
                throw new ArgumentNullException(nameof(paymentAdapter));

            this.actorFactory = actorFactory;
            this.paymentAdapter = paymentAdapter;
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var body = await this.Request.GetRawBodyStringAsync();

            try
            {
                var paymentData = await this.paymentAdapter.ParseAsync(body);
                if (paymentData == null)
                    return BadRequest();
                if (paymentData.PaymentResult == PaymentResult.Paid)
                {
                    var invoiceProxy = this.actorFactory.Create<IInvoiceActor>(new ActorId(paymentData.InvoiceNumber),
                        new Uri(UriConstants.InvoiceActorUri));
                    await invoiceProxy.PaidAsync(paymentData.PaymentDate, default(CancellationToken));
                }
            }
            catch (Exception)
            {
                return BadRequest();
            }


            return Ok();
        }


    }
}
