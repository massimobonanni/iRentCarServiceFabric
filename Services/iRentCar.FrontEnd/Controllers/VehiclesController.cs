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


        [HttpGet("{plate}")]
        public async Task<ActionResult> Details(string plate)
        {
            var actorProxy = this.actorFactory.Create<IVehicleActor>(new ActorId(plate),
                    new Uri(UriConstants.VehicleActorUri));

            var vehicleInfo = await actorProxy.GetInfoAsync(default(CancellationToken));

            if (vehicleInfo == null || !vehicleInfo.IsValid())
                return NotFound();

            return View(vehicleInfo);
        }

        public async Task<ActionResult> Reserve(string plate)
        {
            var actorProxy = this.actorFactory.Create<IVehicleActor>(new ActorId(plate),
                    new Uri(UriConstants.VehicleActorUri));

            var vehicleInfo = await actorProxy.GetInfoAsync(default(CancellationToken));

            if (vehicleInfo == null || !vehicleInfo.IsValid())
                return NotFound();

            var modelForReserve = new VehicleInfoForReserveDto(vehicleInfo);

            return View(modelForReserve);
        }

        // POST: Vehicles/Create
        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Reserve(string plate, [FromBody] VehicleInfoForReserveDto reservation)
        {
            if (!ModelState.IsValid)
                return View();

            if (reservation.StartReservation > reservation.EndReservation)
            {
                ModelState.AddModelError("", "La data di inizio noleggio deve essere inferiore alla data di fine noleggio");
                return View();
            }

            var vehicleProxy = this.actorFactory.Create<IVehicleActor>(new ActorId(plate),
                           new Uri(UriConstants.VehicleActorUri));

            var vehicleInfo = await vehicleProxy.GetInfoAsync(default(CancellationToken));

            if (vehicleInfo == null || !vehicleInfo.IsValid())
                return NotFound();

            if (vehicleInfo.State != VehicleState.Free)
            {
                ModelState.AddModelError("", "L'autovettura non e' piu' disponibile per il noleggio.");
                return View();
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
                    ModelState.AddModelError("", "Le date di prenotazione sono errate");
                    break;
                case VehicleActorError.VehicleBusy:
                    ModelState.AddModelError("", "L'autovettura non può essere assegnata");
                    break;
                case VehicleActorError.VehicleNotAvailable:
                    ModelState.AddModelError("", "Il veicolo non è prenotabile");
                    break;
                case VehicleActorError.GenericError:
                    ModelState.AddModelError("", "Si è verificato un errore nella procedura di prenotazione");
                    break;
                default:
                    break;
            }
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