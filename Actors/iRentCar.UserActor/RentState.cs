using System.Runtime.Serialization;

namespace iRentCar.UserActor
{
    [DataContract]
    internal enum RentState
    {
        [EnumMember]
        Active,
        [EnumMember]
        Closed
    }
}