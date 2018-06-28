using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreInterfaces = iRentCar.Core.Interfaces;
using iRentCar.FrontEnd.Models;
using VehiclesServiceInterfaces = iRentCar.VehiclesService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iRentCar.VehicleActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using iRentCar.Core;

namespace iRentCar.FrontEnd.Controllers
{
    public class VehiclesController : Controller
    {
        public VehiclesController(VehiclesServiceInterfaces.IVehiclesServiceProxy vehiclesServiceProxy, CoreInterfaces.IActorFactory actorFactory)
        {
            if (vehiclesServiceProxy == null)
                throw new ArgumentNullException(nameof(vehiclesServiceProxy));
            if (actorFactory == null)
                throw new ArgumentNullException(nameof(actorFactory));

            this.vehiclesServiceProxy = vehiclesServiceProxy;
            this.actorFactory = actorFactory;
        }

        private readonly VehiclesServiceInterfaces.IVehiclesServiceProxy vehiclesServiceProxy;
        private readonly CoreInterfaces.IActorFactory actorFactory;

        // GET: Vehicles
        public async Task<ActionResult> Index(string brand, string model, string plate, VehiclesServiceInterfaces.VehicleState? state, int pageIndex = 0, int pageSize = 10)
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
        public async Task<ActionResult> Details(string plate)
        {
            var actorProxy = this.actorFactory.Create<IVehicleActor>(new ActorId(plate),
                new Uri(UriConstants.VehicleActorUri));

            var vehicleInfo = await actorProxy.GetInfoAsync(default(CancellationToken));

            if (vehicleInfo == null || !vehicleInfo.IsValid())
                return NotFound();

            return View(vehicleInfo);
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