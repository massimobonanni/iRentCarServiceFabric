using System.Runtime.Serialization;

namespace iRentCar.VehiclesService.Interfaces
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