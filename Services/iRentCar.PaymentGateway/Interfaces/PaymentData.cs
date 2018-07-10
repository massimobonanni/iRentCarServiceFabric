using System;
using System.Collections.Generic;

namespace iRentCar.PaymentGateway.Interfaces
{
    public class PaymentData
    {
        public string InvoiceNumber { get; set; }
        public PaymentResult PaymentResult { get; set; }
        public DateTime PaymentDate { get; set; }

        public Dictionary<string, object> Properties { get; set; }
    }
}