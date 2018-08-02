using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using iRentCar.Core.Interfaces;

namespace iRentCar.Core.Implementations
{
    public class FakeVehiclesRepository : IVehiclesRepository
    {
        static FakeVehiclesRepository()
        {
            CreateVehiclesList();
        }

        private static List<VehicleInfo> vehicles;

        private static List<Tuple<string, List<Tuple<string, decimal>>>> brands =
            new List<Tuple<string, List<Tuple<string, decimal>>>>()
            {
                new Tuple<string, List<Tuple<string, decimal>>>("Audi", new List<Tuple<string, decimal>>()
                {
                    new Tuple<string, decimal>("A1", 30),
                    new Tuple<string, decimal>("A2", 50),
                    new Tuple<string, decimal>("A3", 70),
                    new Tuple<string, decimal>("A4", 100)
                }),
                new Tuple<string, List<Tuple<string, decimal>>>("Ford", new List<Tuple<string, decimal>>()
                {
                    new Tuple<string, decimal>("Ka", 30),
                    new Tuple<string, decimal>("Fiesta", 50),
                    new Tuple<string, decimal>("Mondeo", 70),
                }),
                new Tuple<string, List<Tuple<string, decimal>>>("FIAT", new List<Tuple<string, decimal>>()
                {
                    new Tuple<string, decimal>("500", 30),
                    new Tuple<string, decimal>("Panda", 50),
                    new Tuple<string, decimal>("Uno", 70),
                    new Tuple<string, decimal>("Bravo", 100)
                }),
            };

        private static void CreateVehiclesList()
        {
            vehicles = new List<VehicleInfo>();
            for (int i = 0; i < 10000; i++)
            {
                var brand = brands.ElementAt(RndGenerator.Next(brands.Count));
                var model = brand.Item2.ElementAt(RndGenerator.Next(brand.Item2.Count));
                var vehicle = new VehicleInfo()
                {
                    Brand = brand.Item1,
                    Model = model.Item1,
                    DailyCost = model.Item2,
                    Plate = GenerateNewPlate(vehicles.Where(v => v.Plate != null).Select(v => v.Plate)),
                    State = VehicleState.Free
                };
                vehicles.Add(vehicle);
            }
        }

        private const string UppercaseLetters = "ABCDEFGHILMNOPQRSTUVZWXJ";
        private const string Numbers = "0123456789";

        private static Random RndGenerator = new Random(DateTime.Now.Millisecond);

        private static string AlphaRandom(int numChar)
        {
            string alphaRandom = null;
            for (int i = 0; i < numChar; i++)
            {
                alphaRandom += UppercaseLetters.Substring(RndGenerator.Next(UppercaseLetters.Length), 1);
            }

            return alphaRandom;
        }

        private static string NumericRandom(int numChar)
        {
            string alphaRandom = null;
            for (int i = 0; i < numChar; i++)
            {
                alphaRandom += Numbers.Substring(RndGenerator.Next(Numbers.Length), 1);
            }

            return alphaRandom;
        }

        private static string GenerateNewPlate(IEnumerable<string> plates)
        {
            string plate = null;
            bool plateOk;
            do
            {
                plate = $"{AlphaRandom(2)}{NumericRandom(3)}{AlphaRandom(2)}";
                plateOk = !plates.Any(p => p == plate);
            } while (!plateOk);

            return plate;
        }

        public Task<IQueryable<VehicleInfo>> GetAllVehiclesAsync(long lowPartitionKey, long highPartitionKey, CancellationToken token)
        {
            var query = vehicles.Where(a => a.PartitionKey >= lowPartitionKey)
                .Where(a => a.PartitionKey <= highPartitionKey);
            return Task.FromResult(query.AsQueryable());
        }

        public void SetServiceHost(ServiceContext hostContext)
        {
        }
    }
}
