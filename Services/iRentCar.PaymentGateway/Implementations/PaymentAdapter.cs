using iRentCar.PaymentGateway.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iRentCar.PaymentGateway.Implementations
{
    public class PaymentAdapter : IPaymentAdapter
    {
        public Task<PaymentData> ParseAsync(string jsonPayload)
        {
            PaymentData result = null;
            JObject payload = JObject.Parse(jsonPayload);
            var invoiceNumberProperty = payload.Property("requestId");
            var resultProperty = payload.Property("result");
            var dateProperty = payload.Property("date");

            if (invoiceNumberProperty != null && resultProperty != null && dateProperty != null)
            {
                result = new PaymentData()
                {
                    InvoiceNumber = invoiceNumberProperty.Value.ToString(),
                    PaymentResult = resultProperty.Value.ToString() == "OK" ? PaymentResult.Paid : PaymentResult.NotPaid,
                    PaymentDate = DateTime.Parse(dateProperty.Value.ToString())
                };
            }

            return Task.FromResult(result);
        }
    }
}
