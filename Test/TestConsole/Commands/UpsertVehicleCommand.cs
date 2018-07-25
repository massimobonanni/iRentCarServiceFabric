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

namespace TestConsole.Commands
{
    public class UpsertVehicleCommand : CommandBase
    {
        private string plate;
        private string brand;
        private string model;
        private decimal dailyCost;
        private iRentCar.VehiclesService.Interfaces.VehicleState state;

        public override Task<bool> ParseArgumentsAsync(IEnumerable<string> args)
        {
            bool result = true;
            if (!GetValue<string>(args, "plate", ref plate))
            {

                this.WriteError("The argument '-plate' is mandatory");
                result = false;
            }

            if (!GetValue<string>(args, "brand", ref brand))
            {
                this.WriteError("The argument '-brand' is mandatory");
                result = false;
            }

            if (!GetValue<string>(args, "model", ref model))
            {
                this.WriteError("The argument '-model' is mandatory");
                result = false;
            }

            string strCost = null;
            if (!GetValue<string>(args, "dailyCost", ref strCost))
            {
                this.WriteError("The argument '-dailyCost' is mandatory");
                result = false;
            }
            else
            {
                if (!decimal.TryParse(strCost, out dailyCost))
                {
                    this.WriteError("The argument '-dailyCost' is not valid");
                    result = false;
                }
            }


            string strState = null;
            if (!GetValue<string>(args, "state", ref strState))
            {
                this.WriteError("The argument '-state' is mandatory");
                result = false;
            }
            else
            {
                if (!Enum.TryParse(strState, out state))
                {
                    this.WriteError("The argument '-state' is not valid");
                    result = false;
                }
            }

            return Task.FromResult(result);
        }


        public override async Task ExecuteAsync(IEnumerable<string> args)
        {
            var vehicle = new iRentCar.VehiclesService.Interfaces.VehicleInfo()
            {
                Brand = this.brand,
                DailyCost = this.dailyCost,
                Model = this.model,
                Plate = this.plate,
                State = this.state
            };
            var response = await VehiclesServiceProxy.Instance.AddOrUpdateVehicleAsync(vehicle, default(CancellationToken));

            Console.WriteLine($"AddOrUpdateVehicleAsync --> {response}");
            WriteMessage(null);
        }

        public override void ShowArguments()
        {
            WriteMessage("Updates or adds a vehicle to the vehicles service.");
            WriteMessage(null);
            Console.WriteLine("-plate = plate of the vehicle");
            Console.WriteLine("-brand = brand of the vehicle");
            Console.WriteLine("-model = model of the vehicle");
            Console.WriteLine("-dailyCost = daily cost of the vehicle");
            Console.WriteLine("-state = state of the vehicle (Free, Busy, NotAvailable)");
            Console.WriteLine();

        }
    }
}
