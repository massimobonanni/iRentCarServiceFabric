using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace iRentCar.Core.Interfaces
{
    public class UserInfo
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsEnabled { get; set; }

        [JsonIgnore]
        public long PartitionKey => Username.GetExtendedHash();
    }
}
