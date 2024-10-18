using System;
using Serilog;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;

namespace SynapseHealth.Services
{
	public class AlertService : IAlertService
	{
        private const string updateApiUrl = "https://update-api.com/update";
        private const string alertApiUrl = "https://alert-api.com/alerts";

        private HttpClient _updateHttpClient { get; set; }
        private HttpClient _alertHttpClient { get; set; }

        private readonly MockHttpMessageHandler _updateMockHttp = new();
        private readonly MockHttpMessageHandler _alertMockHttp = new();

        public AlertService()
		{
            _updateMockHttp.When(updateApiUrl)
                .Respond(req => new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            _alertMockHttp.When(alertApiUrl)
                .Respond(req => new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            _updateHttpClient = _updateMockHttp.ToHttpClient();
            _alertHttpClient = _alertMockHttp.ToHttpClient();
		}

        /// <summary>
        /// Delivery alert
        /// </summary>
        /// <param name="orderId">The order id for the alert</param>
        public async Task SendAlertMessage(JToken item, string orderId)
        { 
            var alertData = new
            {
                Message = $"Alert for delivered item: Order {orderId}, Item: {item["Description"]}, " +
                            $"Delivery Notifications: {item["deliveryNotification"]}"
            };
            var content = new StringContent(JObject.FromObject(alertData).ToString(), System.Text.Encoding.UTF8, "application/json");
            var response = await _alertHttpClient.PostAsync(alertApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                Log.Logger.Information($"Alert sent for delivered item: {item["Description"]}");
            }
            else
            {
                Log.Logger.Error($"Failed to send alert for delivered item: {item["Description"]}");
            }
        }

        public async Task SendAlertAndUpdateOrder(JObject order)
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
    }
}

