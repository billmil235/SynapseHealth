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

        static async Task<int> Main(string[] args)
        {
            IHost _host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IOrdersService, OrdersService>();
                    services.AddTransient<IAlertService, AlertService>();
                })
                .Build();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                         .CreateLogger();

            Log.Logger.Information("Start of Orders Processing Application");

            IServiceProvider provider = _host.Services.CreateScope().ServiceProvider;
            var _ordersService = provider.GetService<IOrdersService>();
            var _alertService = provider.GetService<IAlertService>();

            var medicalEquipmentOrders = await _ordersService!.FetchMedicalEquipmentOrders();

            foreach (var order in medicalEquipmentOrders)
            {
                var updatedOrder = _ordersService.ProcessOrder(order);
                await _alertService!.SendAlertAndUpdateOrder(updatedOrder);
            }

            Log.Logger.Information("Completed processing orders.");

            return 0;
        }
    }
}