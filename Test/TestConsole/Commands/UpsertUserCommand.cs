using iRentCar.Core;
using iRentCar.VehicleActor.Interfaces;
using iRentCar.VehiclesService.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.UsersService.Interfaces;

namespace TestConsole.Commands
{
    public class UpsertUserCommand : CommandBase
    {
        private string username;
        private string firstName;
        private string lastName;
        private string mail;
        private bool isEnabled;

        public override Task<bool> ParseArgumentsAsync(IEnumerable<string> args)
        {
            bool result = true;
            if (!GetValue<string>(args, "username", ref username))
            {

                this.WriteError("The argument '-username' is mandatory");
                result = false;
            }

            if (!GetValue<string>(args, "firstName", ref firstName))
            {
                this.WriteError("The argument '-firstName' is mandatory");
                result = false;
            }

            if (!GetValue<string>(args, "lastName", ref lastName))
            {
                this.WriteError("The argument '-lastName' is mandatory");
                result = false;
            }

            if (!GetValue<string>(args, "mail", ref mail))
            {
                this.WriteError("The argument '-mail' is mandatory");
                result = false;
            }

            string strEnabled = null;
            if (GetValue<string>(args, "isEnabled", ref strEnabled))
            {
                if (!bool.TryParse(strEnabled, out isEnabled))
                {
                    this.WriteError("The argument '-isEnabled' is not valid");
                    result = false;
                }
            }

            return Task.FromResult(result);
        }


        public override async Task ExecuteAsync(IEnumerable<string> args)
        {
            var user = new iRentCar.UsersService.Interfaces.UserInfo()
            {
                FirstName = this.firstName,
                LastName = this.lastName,
                Email = this.mail,
                Username = this.username,
                IsEnabled = this.isEnabled
            };
            var response = await UsersServiceProxy.Instance.AddOrUpdateUserAsync(user, default(CancellationToken));

            Console.WriteLine($"AddOrUpdateUserAsync --> {response}");
            WriteMessage(null);
        }

        public override void ShowArguments()
        {
            WriteMessage("Updates or adds a user to the users service.");
            WriteMessage(null);
            Console.WriteLine("-username = username of the user");
            Console.WriteLine("-firstName = first name of the user");
            Console.WriteLine("-lastName = last name of the user");
            Console.WriteLine("-mail = mail of the user");
            Console.WriteLine("-isEnabled = 'true' if the user is anabled, 'false' otherwise");
            Console.WriteLine();

        }
    }
}
