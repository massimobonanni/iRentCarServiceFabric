using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iRentCar.VehiclesService.Interfaces;

namespace iRentCar.FrontEnd.Models
{
    public class VehicleSearchResult : SearchResultModel<VehicleInfo>
    {
        public VehicleSearchResult(IEnumerable<VehicleInfo> items, int count, int pageIndex, int pageSize) : base(items,
            count, pageIndex, pageSize)
        {
        }

        public string BrandFilter { get; set; }
        public string ModelFilter { get; set; }
        public string PlateFilter { get; set; }
        public VehicleState? StateFilter { get; set; }

    }
}
