using iRentCar.InvoiceActor.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using ActorInterface = iRentCar.InvoiceActor.Interfaces;
using Actor = iRentCar.InvoiceActor;

namespace iRentCar.InvoiceActor
{
    internal static class Extensions
    {
        public static ActorInterface.InvoiceInfo ToInvoiceInfo(this InvoiceData data)
        {
            if (data == null)
                throw new NullReferenceException(nameof(data));

            return new ActorInterface.InvoiceInfo()
            {
                Amount = data.Amount,
                CreationDate = data.CreationDate,
                Customer = data.CustomerId,
                PaymentDate = data.PaymentDate,
                State = data.State
            };

        }
    }
}
