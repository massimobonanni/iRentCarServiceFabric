using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iRentCar.UserActor.Interfaces;

namespace iRentCar.FrontEnd.Models.Dto
{
    public class UserInfoForDetails
    {
        public UserInfoForDetails()
        {

        }
        public UserInfoForDetails(string username, UserInfo info)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(nameof(username));

            if (info == null)
                throw new ArgumentNullException(nameof(info));

            this.Username = username;
            this.UserInfo = info;
        }
        public string Username { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}
