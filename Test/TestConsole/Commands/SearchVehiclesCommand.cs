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

namespace TestConsole.Commands
{
    public class SearchVeiclesCommand : CommandBase
    {
        private string plateFilter;
        private string modelFilter;
        private string brandFilter;
        private int pageSize;
        private int pageNumber;


        public override Task<bool> ParseArgumentsAsync(IEnumerable<string> args)
        {
            bool result = true;
            GetValue<string>(args, "plate", ref plateFilter);
            GetValue<string>(args, "model", ref modelFilter);
            GetValue<string>(args, "brand", ref brandFilter);
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
            var response = await VehiclesServiceProxy.Instance.SearchVehiclesAsync(plateFilter, modelFilter, brandFilter, null, default(CancellationToken));
            var vehicles = response.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            WriteSuccess($"Search result:");
            WriteJson(vehicles);
            WriteMessage(null);
        }

        public override void ShowArguments()
        {
            WriteMessage("Execute a vehicles search using filters");
            WriteMessage(null);
            WriteMessage("-plate = vehicle plate filter");
            WriteMessage("-model = model filter");
            WriteMessage("-brand = brand filter");
            WriteMessage("-pageSize = page size (default 20)");
            WriteMessage("-pageNumber = page number (default 1)");
            WriteMessage(null);

        }
    }
}
