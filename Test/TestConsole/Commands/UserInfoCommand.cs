using iRentCar.Core;
using iRentCar.UserActor.Interfaces;
using iRentCar.VehicleActor.Interfaces;
using iRentCar.VehiclesService.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsole.Commands
{
    public class UserInfoCommand : CommandBase
    {
        private string user;


        public override Task<bool> ParseArgumentsAsync(IEnumerable<string> args)
        {
            bool result = true;
            if (!GetValue<string>(args, "user", ref user))
            {

                this.WriteError("The argument '-user' is mandatory");
                result = false;
            }
            return Task.FromResult(result);
        }


        public override async Task ExecuteAsync(IEnumerable<string> args)
        {
            var actorProxy = ActorProxy.Create<IUserActor>(new ActorId(this.user),
               new Uri(UriConstants.UserActorUri));
            var response = await actorProxy.GetInfoAsync(default(CancellationToken));
            WriteSuccess($"Search result:");
            WriteJson(response);
            WriteMessage(null);
        }

        public override void ShowArguments()
        {
            WriteMessage("Retrieves the user information for a specific user");
            WriteMessage(null);
            WriteMessage("-user = the user name");
            WriteMessage(null);

        }
    }
}
