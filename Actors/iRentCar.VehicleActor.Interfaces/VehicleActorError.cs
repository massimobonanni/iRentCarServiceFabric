using System.Runtime.Serialization;

namespace iRentCar.VehicleActor.Interfaces
{
    [DataContract]
    public enum VehicleActorError
    {
        [EnumMember]
        Ok = 0,
        [EnumMember]
        ReservationDatesWrong = 1,
        [EnumMember]
        VehicleBusy = 2,
        [EnumMember]
        VehicleNotAvailable = 3,
        [EnumMember]
        VehicleFree = 4,
        [EnumMember]
        VehicleNotExists =5,
        [EnumMember]
        GenericError = 999
       
    }
}