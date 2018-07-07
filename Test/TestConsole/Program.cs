using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Chaos.DataStructures;
using System.Threading;
using iRentCar.Core;
using iRentCar.Core.Implementations;
using iRentCar.MailService.Interfaces;
using iRentCar.UserActor.Interfaces;
using iRentCar.VehicleActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var mailService = ServiceProxy.Create<IMailService>(new Uri("fabric:/iRentCar/MailService"), 
                new Microsoft.ServiceFabric.Services.Client.ServicePartitionKey(0));

            MailInfo mail = new MailInfo();
            mail.From = "massimo.bonanni@tiscali.it";
            mail.TOAddresses = new List<string>() { "mabonann@microsoft.com" };
            mail.Subject = "Test";
            mail.Body = "Test";
            var response= mailService.SendMailAsync(mail, null, default(CancellationToken)).GetAwaiter().GetResult();

            //var repo = new FakeVehiclesRepository();

            //var list = repo.GetAllVehiclesAsync(0, 100).GetAwaiter().GetResult();

            //var actorProxy = ActorProxy.Create<IVehicleActor>(new Microsoft.ServiceFabric.Actors.ActorId("FE251JX"),
            //    new Uri(UriConstants.VehicleActorUri));

            //var response = actorProxy.GetInfoAsync(default(CancellationToken)).GetAwaiter().GetResult();

            //var actorProxy = ActorProxy.Create<IUserActor>(new Microsoft.ServiceFabric.Actors.ActorId("massimo.bonanni"),
            //    new Uri("fabric:/iRentCar/UserActor"));

            //var response = actorProxy.IsValidAsync(default(CancellationToken)).GetAwaiter().GetResult();


            //using (var client = new FabricClient())
            //{
            //    var chaosParams = new ChaosParameters()
            //    {
            //        MaxConcurrentFaults = 3,
            //        WaitTimeBetweenFaults = TimeSpan.FromMinutes(5),
            //        WaitTimeBetweenIterations = TimeSpan.FromMinutes(5)
            //    };

            //    await client.TestManager.StartChaosAsync(chaosParams);
            //};

        }
    }
}
