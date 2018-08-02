using System;
using System.Collections.Generic;
using System.Text;

namespace iRentCar.UsersService.Interfaces
{
    internal static class UserInfoExtensions
    {
        public static iRentCar.UserActor.Interfaces.UserInfo ToUserActorUserInfo(this UserInfo info)
        {
            if (info == null)
                throw new NullReferenceException(nameof(info));

            return new iRentCar.UserActor.Interfaces.UserInfo()
            {
                CurrentRent = null,
                Email = info.Email,
                FirstName = info.FirstName,
                IsEnabled = info.IsEnabled,
                LastName=info.LastName
            };

        }

        public static bool VerifyFilters(this UserInfo info, string username, string firstName, string lastName, string mail)
        {
            if (info == null)
                throw new NullReferenceException(nameof(info));

            var inResult = true;
            if (!string.IsNullOrWhiteSpace(username))
                if (info.Username != username)
                    inResult = false;

            if (!string.IsNullOrWhiteSpace(firstName) && inResult)
                if (!info.FirstName.Contains(firstName))
                    inResult = false;

            if (!string.IsNullOrWhiteSpace(lastName) && inResult)
                if (!info.LastName.Contains(lastName))
                    inResult = false;

            if (!string.IsNullOrWhiteSpace(mail) && inResult)
                if (!info.Email.Contains(mail))
                    inResult = false;

            return inResult;
        }
    }
}
