using iRentCar.Core;
using iRentCar.VehicleActor.Interfaces;
using iRentCar.VehiclesService.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.UsersService.Interfaces;
using Newtonsoft.Json.Linq;

namespace TestConsole.Commands
{
    public class SearchUsersCommand : CommandBase
    {
        private string usernameFilter;
        private string firstNameFilter;
        private string lastNameFilter;
        private string mailFilter;
        private int pageSize;
        private int pageNumber;


        public override Task<bool> ParseArgumentsAsync(IEnumerable<string> args)
        {
            bool result = true;
            GetValue<string>(args, "username", ref usernameFilter);
            GetValue<string>(args, "firstName", ref firstNameFilter);
            GetValue<string>(args, "lastName", ref lastNameFilter);
            GetValue<string>(args, "mail", ref mailFilter);
            if (!GetValue<int>(args, "pageSize", ref pageSize))
            {
                pageSize = 20;
            }
            if (!GetValue<int>(args, "pageNumber", ref pageNumber))
            {
                pageNumber = 1;
            }
            return Task.FromResult(result);
        }


        public override async Task ExecuteAsync(IEnumerable<string> args)
        {
            var response = await UsersServiceProxy.Instance.SearchUsersAsync(usernameFilter, firstNameFilter, lastNameFilter, mailFilter, default(CancellationToken));
            var totalItems = response.Count();
            var users = response.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var result = new UserSearchResult(users, totalItems, this.pageNumber, pageSize)
            {
                LastNameFilter = this.lastNameFilter,
                FirstNameFilter = this.firstNameFilter,
                UsernameFilter = this.usernameFilter,
                MailFilter = this.mailFilter
            };
            WriteSuccess($"Search result:");
            WriteJson(result);
            WriteMessage(null);
        }

        public override void ShowArguments()
        {
            WriteMessage("Execute users search using filters.");
            WriteMessage(null);
            WriteMessage("-username = username");
            WriteMessage("-firstName = user first name (LIKE)");
            WriteMessage("-lastName = user last name (LIKE)");
            WriteMessage("-mail = user mail (LIKE)");
            WriteMessage("-pageSize = page size (default 20)");
            WriteMessage("-pageNumber = page number (default 1)");
            WriteMessage(null);

        }
    }

    public class UserSearchResult
    {
        public UserSearchResult(IEnumerable<iRentCar.UsersService.Interfaces.UserInfo> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalItems = count;
            if (pageSize == 0)
                TotalPages = 1;
            else
                TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.Users.AddRange(items);
        }

        public List<iRentCar.UsersService.Interfaces.UserInfo> Users { get; set; } =
            new List<iRentCar.UsersService.Interfaces.UserInfo>();

        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public int TotalItems { get; private set; }

        public bool HasPreviousPage => (PageIndex >= 1);

        public bool HasNextPage => (PageIndex < TotalPages - 1);


        public string UsernameFilter { get; set; }
        public string FirstNameFilter { get; set; }
        public string LastNameFilter { get; set; }
        public string MailFilter { get; set; }

    }
}
