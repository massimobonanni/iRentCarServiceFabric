using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.FrontEnd.Models;
using iRentCar.UsersService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace iRentCar.FrontEnd.Controllers
{
    public class UsersController : Controller
    {
        public UsersController(IUsersServiceProxy usersServiceProxy)
        {
            if (usersServiceProxy == null)
                throw new ArgumentNullException(nameof(usersServiceProxy));

            this.usersServiceProxy = usersServiceProxy;
        }

        private readonly IUsersServiceProxy usersServiceProxy;

        [Route("Users")]
        [Route("Users/Index")]
        public async Task<ActionResult> Index(string username, string firstName, string lastName, string mail, int pageIndex = 0, int pageSize = 10)
        {
            var users =
                await this.usersServiceProxy.SearchUsersAsync(username, firstName, lastName, mail, default(CancellationToken));

            var count = users.Count();
            users = users.OrderBy(v => v.Username);
            if (pageIndex >= 0 && pageSize > 0)
            {
                users = users.Skip(pageIndex * pageSize).Take(pageSize);
            }

            var result = new UserSearchResult(users, count, pageIndex, pageSize)
            {
                UsernameFilter = username,
                LastNameFilter = lastName,
                FirstNameFilter = firstName,
                MailFilter = mail
            };

            return View(result);
        }

        // GET: Users/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
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

        // GET: Users/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Users/Edit/5
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

        // GET: Users/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Users/Delete/5
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