using System;
using Serilog;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using SynapseHealth.Models;

namespace SynapseHealth.Services
{
	public class OrdersService : IOrdersService
    {
        private string ordersApiUrl { get; set; }

        private readonly IAlertService _alertService;

        private HttpClient _ordersHttpClient { get; set; }
        private readonly MockHttpMessageHandler _ordersMockHttp = new();

        public OrdersService(IAlertService alertService, IConfiguration configuration)
		{
            _alertService = alertService;

            var config = configuration.GetSection("Urls").Get<Urls>();

            ordersApiUrl = config!.OrdersApiUrl;

            var json = JsonConvert.SerializeObject(GetOrders());

            _ordersMockHttp.When(ordersApiUrl)
                .Respond("application/json", json);

            _ordersHttpClient = _ordersMockHttp.ToHttpClient();
		}

        public async Task<JObject[]> FetchMedicalEquipmentOrders()
        {
            var response = await _ordersHttpClient.GetAsync(ordersApiUrl);
            if (response.IsSuccessStatusCode)
            {
                var ordersData = await response.Content.ReadAsStringAsync();
                return JArray.Parse(ordersData).ToObject<JObject[]>();
            }
            else
            {
                Log.Logger.Error("Failed to fetch orders from API.");
                return new JObject[0];
            }
        }

        public JObject ProcessOrder(JObject order)
        {
            var items = order["Items"]!.ToObject<JArray>();
            foreach (var item in items)
            {
                if (IsItemDelivered(item))
                {
                    _alertService.SendAlertMessage(item, order["OrderId"]!.ToString());
                    IncrementDeliveryNotification(item);
                }
            }

            return order;
        }

        private void IncrementDeliveryNotification(JToken item)
        {
            item["deliveryNotification"] = item["deliveryNotification"]!.Value<int>() + 1;
        }

        private bool IsItemDelivered(JToken item)
        {
            return item["Status"].ToString().Equals("Delivered", StringComparison.OrdinalIgnoreCase);
        }

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

