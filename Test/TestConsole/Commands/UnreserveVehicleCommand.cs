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
    public class UnreserveVehicleCommand : CommandBase
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

            var response = await actorProxy.UnreserveAsync(default(CancellationToken));

            Console.WriteLine($"UnreserveAsync --> {response}");
        }

        public override void ShowArguments()
        {
            WriteMessage("Unreserves a vehicle.");
            WriteMessage(null);
            Console.WriteLine("-plate = vehicle plate to unreserve");Console.WriteLine();
            WriteMessage(null);
        }
    }
}
