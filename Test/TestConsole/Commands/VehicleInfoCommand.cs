using iRentCar.Core;
using iRentCar.VehicleActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestConsole.Commands
{
    public class VehicleInfoCommand : CommandBase
    {
        private string plate;

        public override Task<bool> ParseArgumentsAsync(IEnumerable<string> args)
        {
            bool result = true;
            if (!GetValue<string>(args, "plate", ref plate))
            {
                
               this.WriteError("The argument '-plate' is mandatory");
                result = false;
            }

            return Task.FromResult(result);
        }

        
        public override async Task ExecuteAsync(IEnumerable<string> args)
        {
            var actorProxy = ActorProxy.Create<IVehicleActor>(new Microsoft.ServiceFabric.Actors.ActorId(this.plate),
                new Uri(UriConstants.VehicleActorUri));

            var response = await actorProxy.GetInfoAsync(default(CancellationToken));

            Console.WriteLine($"GetInfo -->");
            WriteJson(response);
            WriteMessage(null);
        }

        public override void ShowArguments()
        {
            WriteMessage("Shows the vehicle information.");
            WriteMessage(null);
            Console.WriteLine("-plate = vehicle plate");Console.WriteLine();
            WriteMessage(null);
        }
    }
}
