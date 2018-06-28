using System.Collections.Generic;
using System.Runtime.Serialization;

namespace iRentCar.UserActor.Interfaces
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
        [DataMember]
        public bool IsEnabled { get; set; }
        [DataMember]
        public RentInfo CurrentRent { get; set; }
        [DataMember]
        public List<InvoiceInfo> Invoices { get; set; }

    }
}
