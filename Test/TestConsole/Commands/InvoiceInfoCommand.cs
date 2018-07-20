using iRentCar.Core;
using iRentCar.InvoiceActor.Interfaces;
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
    public class InvoiceInfoCommand : CommandBase
    {
        private string invoiceNumber;

        public override Task<bool> ParseArgumentsAsync(IEnumerable<string> args)
        {
            bool result = true;
            if (!GetValue<string>(args, "number", ref invoiceNumber))
            {
                
               this.WriteError("The argument '-number' is mandatory");
                result = false;
            }

            return Task.FromResult(result);
        }

        
        public override async Task ExecuteAsync(IEnumerable<string> args)
        {
            var actorProxy = ActorProxy.Create<IInvoiceActor>(new Microsoft.ServiceFabric.Actors.ActorId(this.invoiceNumber),
                new Uri(UriConstants.InvoiceActorUri));

            var response = await actorProxy.GetInfoAsync(default(CancellationToken));

            Console.WriteLine($"GetInfo -->");
            WriteJson(response);
            WriteMessage(null);
        }

        public override void ShowArguments()
        {
            WriteMessage("Shows the invoice information.");
            WriteMessage(null);
            Console.WriteLine("-number = invoice number");Console.WriteLine();
            WriteMessage(null);
        }
    }
}
