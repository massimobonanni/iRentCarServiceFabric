using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using iRentCar.UserActor.Interfaces;

namespace iRentCar.FrontEnd.Models.Dto
{
    public class UserInfoForCreate
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email must be a valid email address")]
        public string Email { get; set; }

        public bool IsEnabled { get; set; } = true;
    }
}
