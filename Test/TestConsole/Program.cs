using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestConsole.Commands;

namespace TestConsole
{
    class Program
    {
        private static Dictionary<string, CommandBase> commands = new Dictionary<string, CommandBase>()
        {
            { "reserve" , new ReserveVehicleCommand()},
            { "unreserve" , new UnreserveVehicleCommand()},
            { "searchVehicles" , new SearchVehiclesCommand()},
            { "vehicleInfo" , new VehicleInfoCommand()},
            { "upsertVehicle" , new UpsertVehicleCommand()},
            { "userInfo" , new UserInfoCommand()},
            { "invoiceInfo" , new InvoiceInfoCommand()},
            { "searchUsers" , new SearchUsersCommand()}
        };

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            //var repo = new FakeVehiclesRepository();
            //var vehicles = await repo.GetAllVehiclesAsync(Int64.MinValue, Int64.MaxValue, default(CancellationToken));
            //var str = JsonConvert.SerializeObject(vehicles);

            //await File.WriteAllTextAsync(@"e:\Temp\vehicles.json", str);

            ////var repo = new InMemoryUserRepository();
            ////var lista = await repo.GetAllUsersAsync(Int64.MinValue, Int64.MaxValue, default(CancellationToken));

            //var lista=FizzWare.NBuilder.Builder<iRentCar.Core.Interfaces.UserInfo>.CreateListOfSize(100000)
            //    .All()
            //    .With(u => u.FirstName = Faker.Name.First())
            //    .With(u => u.LastName = Faker.Name.Last())
            //    .With(u => u.Username = Faker.Internet.UserName())
            //    .With(u => u.Email = Faker.Internet.Email())
            //    .Build();

            // str = JsonConvert.SerializeObject(lista);

            //await File.WriteAllTextAsync(@"e:\Temp\users.json", str);
            
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
            else if (commands.ContainsKey(commandName))
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
