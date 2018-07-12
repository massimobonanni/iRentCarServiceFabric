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
            bool result = true;
            if (!GetValue<string>(args, "plate", ref plate))
            {
                Console.WriteLine("L'argomento -plate è obbligatorio");
                result = false;
            }

            if (!GetValue<string>(args, "user", ref username ))
            {
                Console.WriteLine("L'argomento -user è obbligatorio");
                result = false;
            }

            string strDate=null;
            if (!GetValue<string>(args, "startDate", ref strDate))
            {
                Console.WriteLine("L'argomento -startDate è obbligatorio");
                result = false;
            }
            else
            {
                if (!DateTime.TryParse(strDate, out startDate))
                {
                    Console.WriteLine("L'argomento -startDate non è valido");
                    result = false;
                }
            }

            if (!GetValue<string>(args, "endDate", ref strDate))
            {
                Console.WriteLine("L'argomento -endDate è obbligatorio");
                result = false;
            }
            else
            {
                if (!DateTime.TryParse(strDate, out endDate))
                {
                    Console.WriteLine("L'argomento -endDate non è valido");
                    result = false;
                }
            }

            return Task.FromResult(result);
        }

        
        public override async Task ExecuteAsync(IEnumerable<string> args)
        {
            var actorProxy = ActorProxy.Create<IVehicleActor>(new Microsoft.ServiceFabric.Actors.ActorId(this.plate),
                new Uri(UriConstants.VehicleActorUri));

            var response = await actorProxy.ReserveAsync(this.username,this.startDate,this.endDate,default(CancellationToken));

            Console.WriteLine($"ReserveAsync --> {response}");
        }

        public override void ShowArguments()
        {
            Console.WriteLine("-plate = vehicle plate to reserve");
            Console.WriteLine("-user = user name reserve to");
            Console.WriteLine("-startDate = start date of reservation (yyyymmddHHmm)");
            Console.WriteLine("-endDate = end date of reservation (yyyymmddHHmm)");
            Console.WriteLine();

        }
    }
}
