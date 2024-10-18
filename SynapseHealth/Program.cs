using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using SynapseHealth.Services;

namespace Synapse.OrdersExample
{
    /// <summary>
    /// I Get a list of orders from the API
    /// I check if the order is in a delviered state, If yes then send a delivery alert and add one to deliveryNotification
    /// I then update the order.   
    /// </summary>
    class Program
    {

        // This method was made ASYNC so I do not have to use GetAwaiter/GetResult
        // Its possible that it may cause a deadlock.
        static async Task<int> Main(string[] args)
        {
            // Set up dependency injection
            IHost _host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IOrdersService, OrdersService>();
                    services.AddTransient<IAlertService, AlertService>();
                })
                .Build();

            // Set up to read serilog config from AppSettings file.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .Build();

            //  Set up Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                         .CreateLogger();

            Log.Logger.Information("Start of Orders Processing Application");

            // Retrieve the service from the DI container
            IServiceProvider provider = _host.Services.CreateScope().ServiceProvider;
            var _ordersService = provider.GetService<IOrdersService>();

            var medicalEquipmentOrders = await _ordersService!.FetchMedicalEquipmentOrders();

            foreach (var order in medicalEquipmentOrders)
            {
                var updatedOrder = _ordersService.ProcessOrder(order);
                await _ordersService!.UpdateOrder(updatedOrder);
            }

            Log.Logger.Information("Completed processing orders.");

            return 0;
        }
    }
}