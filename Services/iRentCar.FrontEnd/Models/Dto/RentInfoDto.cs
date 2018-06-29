using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace iRentCar.FrontEnd.Models.Dto
{
    public class RentInfoDto
    {
        [Required(ErrorMessage = "La targa è obbligatoria")]

        public string Plate { get; set; }

        [Required(ErrorMessage ="Il customer è obbligatorio")]
        public string Customer { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
