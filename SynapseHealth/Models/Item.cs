using System;
namespace SynapseHealth.Models
{
	public record Item
	{
		public string Status { get; set; }
		public string Description { get; set; }
		public int deliveryNotification { get; set; }
	}
}

