using System.Runtime.Serialization;

namespace iRentCar.UserActor.Interfaces
{
    [DataContract]
    public enum UserActorError
    {
        [EnumMember]
        Ok = 0,
        [EnumMember]
        VehicleAlreadyRented = 1,
        [EnumMember]
        InternalActorError = 999

    }
}