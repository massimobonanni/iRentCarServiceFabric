using System;
using System.Runtime.Serialization;

namespace iRentCar.Core.Interfaces
{
    public class UserInfo
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsEnabled { get; set; }

        public long PartitionKey
        {
            get
            {
                return Username.GetExtendedHash();
            }
        }
    }
}
