using System;
using System.Runtime.Serialization;
using iRentCar.UserActor.Interfaces;

namespace iRentCar.UserActor
{
    [DataContract]
    internal class UserData
    {
        public UserData()
        {

        }

        public UserData(UserInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            this.Email = info.Email;
            this.FirstName = info.FirstName;
            this.LastName = info.LastName;
            this.IsEnabled = info.IsEnabled;
        }

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
