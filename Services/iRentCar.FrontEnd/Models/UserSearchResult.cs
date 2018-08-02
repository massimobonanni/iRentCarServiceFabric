using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iRentCar.UsersService.Interfaces;
using iRentCar.VehiclesService.Interfaces;

namespace iRentCar.FrontEnd.Models
{
    public class UserSearchResult : SearchResultModel<UserInfo>
    {
        public UserSearchResult(IEnumerable<UserInfo> items, int count, int pageIndex, int pageSize) : base(items,
            count, pageIndex, pageSize)
        {
        }

        public string UsernameFilter { get; set; }
        public string FirstNameFilter { get; set; }
        public string LastNameFilter { get; set; }
        public string MailFilter { get; set; }
    }
}
