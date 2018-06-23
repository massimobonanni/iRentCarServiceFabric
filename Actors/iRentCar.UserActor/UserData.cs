using System.Runtime.Serialization;

namespace iRentCar.UserActor
{
    [DataContract]
    internal class UserData
    {
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public bool IsEnabled { get; set; }

    }
}
