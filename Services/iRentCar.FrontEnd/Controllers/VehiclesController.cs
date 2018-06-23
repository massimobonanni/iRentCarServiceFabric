using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.FrontEnd.Models;
using iRentCar.VehiclesService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace iRentCar.FrontEnd.Controllers
{
    public class VehiclesController : Controller
    {
        public VehiclesController(IVehiclesServiceProxy vehiclesServiceProxy)
        {
            if (vehiclesServiceProxy == null)
                throw new ArgumentNullException(nameof(vehiclesServiceProxy));

            this.vehiclesServiceProxy = vehiclesServiceProxy;
        }

        private readonly IVehiclesServiceProxy vehiclesServiceProxy;

        // GET: Vehicles
        public async Task<ActionResult> Index(string brand, string model, string plate, VehicleState? state, int pageIndex = 0, int pageSize = 10)
        {
            var vehicles =
                await this.vehiclesServiceProxy.SearchVehiclesAsync(plate, model, brand, state, default(CancellationToken));
            var count = vehicles.Count();
            vehicles = vehicles.OrderBy(v => v.Plate);
            if (pageIndex >= 0 && pageSize > 0)
            {
                vehicles = vehicles.Skip(pageIndex * pageSize).Take(pageSize);
            }

            var result = new VehicleSearchResult(vehicles, count, pageIndex, pageSize)
            {
                BrandFilter = brand,
                ModelFilter = model,
                PlateFilter = plate,
                StateFilter = state
            };

            return View(result);
        }

        // GET: Vehicles/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Vehicles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Vehicles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Vehicles/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Vehicles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Vehicles/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Vehicles/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}