using System;
using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Chaos.DataStructures;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core;
using iRentCar.Core.Implementations;
using iRentCar.MailService.Interfaces;
using iRentCar.UserActor.Interfaces;
using iRentCar.VehicleActor.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using TestConsole.Commands;

namespace TestConsole
{
    class Program
    {
        private static Dictionary<string, CommandBase> commands = new Dictionary<string, CommandBase>()
        {
            { "reserve" , new ReserveVehicleCommand()},
            { "searchVehicles" , new SearchVeiclesCommand()},
            { "userInfo" , new UserInfoCommand()}
        };

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            if (!args.Any())
                return;

            var commandName = args[0];
            if (commandName == "-h")
            {
                Console.WriteLine("Commands:");
                foreach (var command in commands.Keys)
                {
                    Console.WriteLine($"\t{command}");
                }
            }
            else  if (commands.ContainsKey(commandName))
            {
                var command = commands[commandName];
                if (args.Any(a => a == "-h"))
                {
                    command.ShowArguments();
                }
                else if (await command.ParseArgumentsAsync(args))
                {
                    await command.ExecuteAsync(args);
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("command unknown!");
                Console.ResetColor();
            }


            //MailInfo mail = new MailInfo();
            //mail.From = "massimo.bonanni@tiscali.it";
            //mail.TOAddresses = new List<string>() { "mabonann@microsoft.com" };
            //mail.Subject = "Test";
            //mail.Body = "Test";

            //var response = MailServiceProxy.Instance.SendMailAsync(mail, null, default(CancellationToken)).GetAwaiter().GetResult();


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
