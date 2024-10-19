using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Serilog;

namespace SynapseHealth.Services
{
    public class AlertService : IAlertService
	{
        private string alertApiUrl { get; set; }

        private HttpClient _alertHttpClient { get; set; }
        private readonly MockHttpMessageHandler _alertMockHttp = new();

        public AlertService(IConfigurationProvider configurationProvider)
		{
            // Retrieve URLs from AppSettings.  Hard coding them in the application is bad practice.
            var config = configurationProvider.GetConfiguration();
            alertApiUrl = config!.AlertApiUrl;

            // Set up the mock for the HttpClient to return the dummy data created previously.
            _alertMockHttp.When(alertApiUrl)
                .Respond(req => new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            _alertHttpClient = _alertMockHttp.ToHttpClient();
		}

        /// <summary>
        /// Delivery alert
        /// </summary>
        /// <param name="orderId">The order id for the alert</param>
        /// <param name="item">The item to alert about.</param>
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
    }
}

