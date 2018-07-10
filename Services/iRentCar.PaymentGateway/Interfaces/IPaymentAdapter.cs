using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iRentCar.PaymentGateway.Interfaces
{
    public interface IPaymentAdapter
    {
        Task<PaymentData> ParseAsync(string jsonPayload);
    }
}
