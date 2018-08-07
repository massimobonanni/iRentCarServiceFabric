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
using iRentCar.UserActor.Interfaces;
using iRentCar.FrontEnd.Models.Dto;

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

        [Route("Vehicles")]
        [Route("Vehicles/Index")]
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

        [HttpGet("Vehicles/{plate}")]
        public async Task<ActionResult> Details(string plate)
        {
            var actorProxy = this.actorFactory.Create<IVehicleActor>(new ActorId(plate),
                    new Uri(UriConstants.VehicleActorUri));

            var vehicleInfo = await actorProxy.GetInfoAsync(default(CancellationToken));

            if (vehicleInfo == null || !vehicleInfo.IsValid())
                return NotFound();

            return View(vehicleInfo);
        }

        [HttpGet("Vehicles/reserve/{plate}")]

        public async Task<ActionResult> Reserve(string plate)
        {
            if (string.IsNullOrWhiteSpace(plate))
                return BadRequest();
            
            var actorProxy = this.actorFactory.Create<IVehicleActor>(new ActorId(plate),
                    new Uri(UriConstants.VehicleActorUri));

            var vehicleInfo = await actorProxy.GetInfoAsync(default(CancellationToken));

            if (vehicleInfo == null || !vehicleInfo.IsValid())
                return NotFound();

            var modelForReserve = new VehicleInfoForReserveDto(vehicleInfo);

            return View(modelForReserve);
        }

        // POST: Vehicles/Create
        [HttpPost("Vehicles/reserve/{plate}")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reserve(string plate, [FromForm] VehicleInfoForReserveDto reservation)
        {
            if (!ModelState.IsValid)
                return View(reservation);

            if (reservation.StartReservation > reservation.EndReservation)
            {
                ModelState.AddModelError("", "The start reservation date must be less then the end reservation date.");
                return View(reservation);
            }

            var vehicleProxy = this.actorFactory.Create<IVehicleActor>(new ActorId(plate),
                           new Uri(UriConstants.VehicleActorUri));

            var vehicleInfo = await vehicleProxy.GetInfoAsync(default(CancellationToken));

            if (vehicleInfo == null || !vehicleInfo.IsValid())
                return NotFound();

            if (vehicleInfo.State != VehicleState.Free)
            {
                ModelState.AddModelError("", "The vehicle is not available.");
                return View(reservation);
            }


            var result= await vehicleProxy.ReserveAsync(reservation.Customer, reservation.StartReservation, 
                reservation.EndReservation, default(CancellationToken));
            
            if (result == VehicleActorError.Ok)
                return RedirectToAction(nameof(Index));

            switch (result)
            {
                case VehicleActorError.Ok:
                    break;
                case VehicleActorError.ReservationDatesWrong:
                    ModelState.AddModelError("", "The reservation dates are wrong");
                    break;
                case VehicleActorError.VehicleBusy:
                    ModelState.AddModelError("", "The vehicle cannot be reserved");
                    break;
                case VehicleActorError.VehicleNotAvailable:
                    ModelState.AddModelError("", "The vehicle is not available");
                    break;
                case VehicleActorError.VehicleNotExists:
                    ModelState.AddModelError("", "The vehicle doesn't exist");
                    break;
                case VehicleActorError.GenericError:
                default:
                    ModelState.AddModelError("", "An error occurs during reservation procedure");
                    break;
            }
            return View(reservation);
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