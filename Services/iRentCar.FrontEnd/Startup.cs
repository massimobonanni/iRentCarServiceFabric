using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iRentCar.Core.Implementations;
using iRentCar.Core.Interfaces;
using iRentCar.InvoicesService.Interfaces;
using iRentCar.UsersService.Interfaces;
using iRentCar.VehiclesService.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace iRentCar.FrontEnd
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddTransient<IVehiclesServiceProxy>(a => VehiclesServiceProxy.Instance);
            services.AddTransient<IUsersServiceProxy>(a => UsersServiceProxy.Instance);
            services.AddTransient<IInvoicesServiceProxy>(a => InvoicesServiceProxy.Instance);
            services.AddSingleton<IActorFactory, ReliableFactory>();
            services.AddSingleton<IServiceFactory, ReliableFactory>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller}/{action}/{id?}",
            //        defaults: new { controller = "Home", action = "Index" });
            //});

            app.UseMvc();
        }
    }
}
