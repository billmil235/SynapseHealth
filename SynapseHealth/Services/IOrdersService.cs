using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace SynapseHealth.Services
{
	public interface IOrdersService
	{
        Task<JObject[]> FetchMedicalEquipmentOrders();
        JObject ProcessOrder(JObject order);
        Task UpdateOrder(JObject order);
        bool IsItemDelivered(JToken item);
        void IncrementDeliveryNotification(JToken item);
    }
}

