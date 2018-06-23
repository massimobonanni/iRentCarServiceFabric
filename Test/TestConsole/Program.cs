using System;
using iRentCar.Core.Implementations;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var repo = new FakeVehiclesRepository();

            var list = repo.GetAllVehiclesAsync(0, 100).GetAwaiter().GetResult();
        }
    }
}
