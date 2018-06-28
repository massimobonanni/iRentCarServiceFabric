using iRentCar.UserActor.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace iRentCar.UserActor
{
    internal static class Extensions
    {

        internal static UserInfo ToUserInfo(this UserData userData)
        {
            if (userData == null)
                throw new NullReferenceException(nameof(userData));

            return new UserInfo()
            {
                Email = userData.Email,
                FirstName = userData.FirstName,
                IsEnabled = userData.IsEnabled,
                LastName = userData.LastName,
                Invoices = new List<InvoiceInfo>(),
            };
        }

        internal static RentInfo ToRentInfo(this RentData data)
        {
            if (data == null)
                throw new NullReferenceException(nameof(data));

            return new RentInfo()
            {
                DailyCost = data.VehicleDailyCost,
                Plate = data.VehiclePlate,
                StartRent = data.StartRent
            };
        }

        internal static InvoiceInfo ToInvoiceInfo(this InvoiceData data)
        {
            if (data == null)
                throw new NullReferenceException(nameof(data));

            return new InvoiceInfo()
            {
                Amount = data.Amount,
                Number = data.Number,
                ReleaseDate = data.ReleaseDate
            };
        }
    }
}
