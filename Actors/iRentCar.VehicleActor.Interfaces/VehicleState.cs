using System.Runtime.Serialization;

namespace iRentCar.VehicleActor.Interfaces
{
    [DataContract]
    public enum VehicleState
    {
        [EnumMember]
        Free,
        [EnumMember]
        Busy,
        [EnumMember]
        NotAvailable
    }
}