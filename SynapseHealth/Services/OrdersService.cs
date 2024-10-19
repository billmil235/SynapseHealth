using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Serilog;
using SynapseHealth.Models;

namespace SynapseHealth.Services
{
    public class OrdersService : IOrdersService
    {
        private string updateApiUrl { get; set; }
        private string ordersApiUrl { get; set; }

        private readonly IAlertService _alertService;

        private HttpClient _updateHttpClient { get; set; }
        private HttpClient _ordersHttpClient { get; set; }

        private readonly MockHttpMessageHandler _ordersMockHttp = new();
        private readonly MockHttpMessageHandler _updateMockHttp = new();

        public OrdersService(IAlertService alertService, IConfigurationProvider configurationProvider)
		{
            _alertService = alertService;

            // Retrieve URLs from AppSettings.  Hard coding them in the application is bad practice.
            var config = configurationProvider.GetConfiguration();
            ordersApiUrl = config!.OrdersApiUrl;
            updateApiUrl = config!.UpdateApiUrl;

            // Create some dummy data for the application to process.  A production level application
            // would have a repository to retrieve this.
            var json = JsonConvert.SerializeObject(GetOrders());

            // Set up the mock for the HttpClient to return the dummy data created previously.
            _ordersMockHttp.When(ordersApiUrl)
                .Respond("application/json", json);

            _updateMockHttp.When(updateApiUrl)
                .Respond(req => new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            _ordersHttpClient = _ordersMockHttp.ToHttpClient();
            _updateHttpClient = _updateMockHttp.ToHttpClient();
		}

        /// <summary>
        /// Retrieves new orders from the orders web service.
        /// </summary>
        /// <returns>An array of JObjects containing order data.</returns>
        public async Task<JObject[]> FetchMedicalEquipmentOrders()
        {
            var response = await _ordersHttpClient.GetAsync(ordersApiUrl);

            if (response.IsSuccessStatusCode)
            {
                // In an actual production scenario I woul deserialize this to actual
                // C# objects rather than pass these generic JSON objects around.
                var ordersData = await response.Content.ReadAsStringAsync();
                return JArray.Parse(ordersData).ToObject<JObject[]>();
            }
            else
            {
                Log.Logger.Error("Failed to fetch orders from API.");
                return Array.Empty<JObject>();
            }
        }

        /// <summary>
        /// Processes the orders.
        /// </summary>
        /// <param name="order">An order to be processed.</param>
        /// <returns>The order that was processed.</returns>
        public JObject ProcessOrder(JObject order)
        {
            var items = order["Items"]!.ToObject<JArray>();

            // Only orders with items are valid.  Inform the user if order is invalid.
            if (items == null)
            {
                Log.Logger.Error($"Order contains no items. Order Id: {order["OrderId"]}");
            }
            else
            {
                Log.Logger.Information($"Processing items. Order Id: {order["OrderId"]}");
                foreach (var item in items)
                {
                    if (IsItemDelivered(item))
                    {
                        _alertService.SendAlertMessage(item, order["OrderId"]!.ToString());
                        IncrementDeliveryNotification(item);
                    }
                }

                // I don't know if this was an intentional bug or an oversight but by not assigning the items
                // array back to the order, you are not going to return updated order information to the caller
                // as intended.
                order["Items"] = items;
            }

            return order;
        }

        /// <summary>
        /// Delivery alert with order update.  This method was renamed from SendAlertAndUpdateOrder because it doesn't actually send an alert.
        /// </summary>
        /// <param name="order">The order to alert about.</param>
        public async Task UpdateOrder(JObject order)
        {
            var content = new StringContent(order.ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await _updateHttpClient.PostAsync(updateApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Log.Logger.Information($"Updated order sent for processing: OrderId {order["OrderId"]}");
            }
            else
            {
                Log.Logger.Error($"Failed to send updated order for processing: OrderId {order["OrderId"]}");
            }
        }

        /// <summary>
        /// Increments the number of times a delivery notification was sent.
        /// </summary>
        /// <param name="item">The item the delivery notification was sent for.</param>
        public void IncrementDeliveryNotification(JToken item)
        {
            item["deliveryNotification"] = item["deliveryNotification"]!.Value<int>() + 1;
        }

        /// <summary>
        /// Determes if the item has been delivered.
        /// </summary>
        /// <param name="item">The item to check the delivery status of.</param>
        /// <returns>True or false depending on whether the item has been delivered or not.</returns>
        public bool IsItemDelivered(JToken item)
        {
            return item["Status"].ToString().Equals("Delivered", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Method to create dummy order data.  This would not be in a production level app.
        /// </summary>
        /// <returns>A list of fake orders for testing.</returns>
        private static List<Order> GetOrders()
        {
            List<Order> orders = new();

            Order o1 = new()
            {
                OrderId = "o11",
            };

            o1.Items.AddRange(new List<Item>
            {
                new Item()
                {
                    Description = "Wheelchair",
                    Status = "Delivered",
                    deliveryNotification = 1
                },
                new Item()
                {
                    Description = "Test Strips",
                    Status = "Not delivered",
                    deliveryNotification = 0
                }

            });

            orders.Add(o1);

            Order o2 = new()
            {
                OrderId = "o99",
            };

            o2.Items.AddRange(new List<Item>
            {
                new Item()
                {
                    Description = "Oxygen",
                    Status = "Not Delivered",
                    deliveryNotification = 1
                },
                new Item()
                {
                    Description = "CPAP",
                    Status = "Not delivered",
                    deliveryNotification = 0
                }

            });

            orders.Add(o2);

            Order o3 = new()
            {
                OrderId = "o25",
            };

            o3.Items.AddRange(new List<Item>
            {
                new Item()
                {
                    Description = "Syringe",
                    Status = "Delivered",
                    deliveryNotification = 1
                },
                new Item()
                {
                    Description = "Test Strips",
                    Status = "Delivered",
                    deliveryNotification = 0
                }

            });

            orders.Add(o3);

            return orders;
        }
    }
}

