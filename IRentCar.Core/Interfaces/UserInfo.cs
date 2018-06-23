using System.Runtime.Serialization;

namespace iRentCar.Core.Interfaces
{
    public class UserInfo
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool IsEnabled { get; set; }

    }
}
