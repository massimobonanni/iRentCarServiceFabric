using iRentCar.MailService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public static MailServiceError Check(this MailInfo mail)
        {
            if (mail == null)
                throw new NullReferenceException(nameof(mail));

            if (string.IsNullOrWhiteSpace(mail.Subject))
            {
                return MailServiceError.SubjectNotValid;
            }
            if (mail.TOAddresses == null || !mail.TOAddresses.Any())
            {
                return MailServiceError.TOAddressesEmpty;
            }
            if (mail.TOAddresses.Any(a => string.IsNullOrWhiteSpace(a) || !a.IsValidEmail()))
            {
                return MailServiceError.TOAddressNotValid;

            }
            if (mail.CCAddresses != null && mail.CCAddresses.Any())
            {
                if (mail.CCAddresses.Any(a => string.IsNullOrWhiteSpace(a) || !a.IsValidEmail()))
                {
                    return MailServiceError.CCAddressNotValid;
                }
            }
            if (mail.BccAddresses != null && mail.BccAddresses.Any())
            {
                if (mail.BccAddresses.Any(a => string.IsNullOrWhiteSpace(a) || !a.IsValidEmail()))
                {
                    return MailServiceError.BccAddressNotValid;
                }
            }
            return MailServiceError.Ok;
        }
    }
}
