using System;
using Newtonsoft.Json.Linq;

namespace SynapseHealth.Services
{
	public interface IAlertService
	{
        Task SendAlertMessage(JToken item, string orderId);
        Task SendAlertAndUpdateOrder(JObject order);
    }
}

