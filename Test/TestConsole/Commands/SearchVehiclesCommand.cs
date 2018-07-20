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
using Newtonsoft.Json.Linq;

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
            var totalItems = response.Count();
            var vehicles = response.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            var result = new VehicleSearchResult(vehicles, totalItems, this.pageNumber, pageSize)
            {
                BrandFilter = this.brandFilter,
                ModelFilter = this.modelFilter,
                PlateFilter = this.plateFilter,
                StateFilter = null
            };
            WriteSuccess($"Search result:");
            WriteJson(result);
            WriteMessage(null);
        }

        public override void ShowArguments()
        {
            WriteMessage("Execute a vehicles search using filters.");
            WriteMessage(null);
            WriteMessage("-plate = vehicle plate filter");
            WriteMessage("-model = model filter");
            WriteMessage("-brand = brand filter");
            WriteMessage("-pageSize = page size (default 20)");
            WriteMessage("-pageNumber = page number (default 1)");
            WriteMessage(null);

        }
    }

    public class VehicleSearchResult 
    {
        public VehicleSearchResult(IEnumerable<iRentCar.VehiclesService.Interfaces.VehicleInfo> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            TotalItems = count;
            if (pageSize == 0)
                TotalPages = 1;
            else
                TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.Vehicles.AddRange(items);
        }

        public List<iRentCar.VehiclesService.Interfaces.VehicleInfo> Vehicles { get; set; } =
            new List<iRentCar.VehiclesService.Interfaces.VehicleInfo>();

        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }

        public int TotalItems { get; private set; }

        public bool HasPreviousPage => (PageIndex >= 1);

        public bool HasNextPage => (PageIndex < TotalPages - 1);

        public string BrandFilter { get; set; }
        public string ModelFilter { get; set; }
        public string PlateFilter { get; set; }
        public iRentCar.VehiclesService.Interfaces.VehicleState? StateFilter { get; set; }

    }
}
