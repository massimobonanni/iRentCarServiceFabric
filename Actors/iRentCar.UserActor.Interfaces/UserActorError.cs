using System.Runtime.Serialization;

namespace iRentCar.UserActor.Interfaces
{
    [DataContract]
    public enum UserActorError
    {
        [EnumMember]
        Ok = 0,
        [EnumMember]
        UserNotValid = 2,
        [EnumMember]
        VehicleAlreadyRented = 10,
        [EnumMember]
        VehicleNotRented = 11,
        [EnumMember]
        GenericError = 999

    }
}