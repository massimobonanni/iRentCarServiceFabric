using iRentCar.MailService.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace iRentCar.MailService
{
    internal static class Extensions
    {
        public static MailSendResult ToMailSendResult(this MailAdapterResult result)
        {
            switch (result)
            {
                case MailAdapterResult.Ok:
                    return MailSendResult.Sended;
                case MailAdapterResult.InfrastructuralError:
                case MailAdapterResult.NotInfrastructuralError:
                default:
                    return MailSendResult.NotSended;
            }

        }
    }
}
