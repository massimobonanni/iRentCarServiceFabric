using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using iRentCar.VehicleActor.Interfaces;

namespace iRentCar.FrontEnd.Models.Dto
{
    public class VehicleInfoForReserveDto
    {
        public VehicleInfoForReserveDto()
        {

        }

        public VehicleInfoForReserveDto(VehicleInfo vehicleInfo)
        {
            if (vehicleInfo == null)
                throw new ArgumentNullException(nameof(vehicleInfo));

            this.Brand = vehicleInfo.Brand;
            this.DailyCost = vehicleInfo.DailyCost;
            this.Model = vehicleInfo.Model;
            this.Plate = vehicleInfo.Plate;
        }

        public string Plate { get; set; }
        public string Model { get; set; }
        public string Brand { get; set; }
        public decimal DailyCost { get; set; }

        [Required]
        public string Customer { get; set; }
        [Required]
        public DateTime StartReservation { get; set; }
        [Required]
        public DateTime EndReservation { get; set; }
    }
}
