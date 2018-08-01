using System.Collections.Generic;
using System.Runtime.Serialization;

namespace iRentCar.UsersService.Interfaces
{
    [DataContract]
    public class UserInfo
    {
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Email { get; set; }
    }
}
