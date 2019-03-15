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
    public class ReserveVehicleCommand : CommandBase
    {
        private string plate;
        private string username;
        private DateTime startDate;
        private DateTime endDate;

        public override Task<bool> ParseArgumentsAsync(IEnumerable<string> args)
        {
            var result = true;
            if (!GetValue<string>(args, "plate", ref plate))
            {

                this.WriteError("The argument '-plate' is mandatory");
                result = false;
            }

            if (!GetValue<string>(args, "user", ref username))
            {
                this.WriteError("The argument '-user' is mandatory");
                result = false;
            }

            string strDate = null;
            if (!GetValue<string>(args, "startDate", ref strDate))
            {
                this.WriteError("The argument '-startDate' is mandatory");
                result = false;
            }
            else
            {
                if (!DateTime.TryParse(strDate, out startDate))
                {
                    this.WriteError("The argument '-startDate' is not valid");
                    result = false;
                }
            }

            if (!GetValue<string>(args, "endDate", ref strDate))
            {
                this.WriteError("The argument '-endDate' is mandatory");
                result = false;
            }
            else
            {
                if (!DateTime.TryParse(strDate, out endDate))
                {
                    this.WriteError("The argument '-endDate' is not valid");
                    result = false;
                }
            }

            return Task.FromResult(result);
        }


        public override async Task ExecuteAsync(IEnumerable<string> args)
        {
            var actorProxy = ActorProxy.Create<IVehicleActor>(new Microsoft.ServiceFabric.Actors.ActorId(this.plate),
                new Uri(UriConstants.VehicleActorUri));

            var response = await actorProxy.ReserveAsync(this.username, this.startDate, this.endDate, default(CancellationToken));

            Console.WriteLine($"ReserveAsync --> {response}");
            WriteMessage(null);
        }

        public override void ShowArguments()
        {
            WriteMessage("Reserves a vehicle for a customer.");
            WriteMessage(null);
            Console.WriteLine("-plate = vehicle plate to reserve");
            Console.WriteLine("-user = user name reserve to");
            Console.WriteLine("-startDate = start date of reservation (yyyy-MM-dd HH:mm:ss)");
            Console.WriteLine("-endDate = end date of reservation (yyyy-MM-dd HH:mm:ss)");
            Console.WriteLine();

        }
    }
}
