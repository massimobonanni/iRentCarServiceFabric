using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using iRentCar.Core.Interfaces;
using iRentCar.FrontEnd.Models;
using iRentCar.FrontEnd.Models.Dto;
using iRentCar.UserActor.Interfaces;
using iRentCar.UsersService.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;

namespace iRentCar.FrontEnd.Controllers
{
    public class UsersController : Controller
    {
        public UsersController(IUsersServiceProxy usersServiceProxy, IActorFactory actorFactory)
        {
            if (usersServiceProxy == null)
                throw new ArgumentNullException(nameof(usersServiceProxy));
            if (actorFactory == null)
                throw new ArgumentNullException(nameof(actorFactory));

            this.usersServiceProxy = usersServiceProxy;
            this.actorFactory = actorFactory;
        }

        private readonly IUsersServiceProxy usersServiceProxy;
        private readonly IActorFactory actorFactory;


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

        [Route("Users/Details/{username}")]
        // GET: Users/Details/5
        public async Task<ActionResult> Details(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest();

            var user = await this.usersServiceProxy.GetUserByUserNameAsync(username, default(CancellationToken));

            if (user == null)
                return NotFound();

            var userProxy =
                this.actorFactory.Create<IUserActor>(new ActorId(username), new Uri(UriConstants.UserActorUri));

            UserActor.Interfaces.UserInfo userInfo = null;

            try
            {
                userInfo = await userProxy.GetInfoAsync(default(CancellationToken));
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }

            if (userInfo == null)
                return NotFound();

            UserInfoForDetails userInfoForDetails = new UserInfoForDetails(username, userInfo);
            return View(userInfoForDetails);
        }

        [Route("Users/Create")]
        public ActionResult Create()
        {
            var userInfo = new UserInfoForCreate();
            return View(userInfo);
        }

        // POST: Users/Create
        [HttpPost("Users/Create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([FromForm] UserInfoForCreate userInfo)
        {
            if (ModelState.IsValid)
            {
                var user = await this.usersServiceProxy.GetUserByUserNameAsync(userInfo.Username, default(CancellationToken));
                if (user != null)
                {
                    ModelState.AddModelError("username", "A user with the same username already exists");
                    return View(userInfo);
                }

                user = new UsersService.Interfaces.UserInfo()
                {
                    Username = userInfo.Username,
                    Email = userInfo.Email,
                    FirstName = userInfo.FirstName,
                    LastName = userInfo.LastName,
                    IsEnabled = userInfo.IsEnabled
                };

                bool response;

                try
                {
                    response = await this.usersServiceProxy.AddOrUpdateUserAsync(user, default(CancellationToken));
                }
                catch (Exception ex)
                {
                    response = false;
                }

                if (!response)
                {
                    ModelState.AddModelError("", "There was an error during insert operation");
                    return View(userInfo);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(userInfo);
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