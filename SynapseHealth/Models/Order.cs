using System;
namespace SynapseHealth.Models
{
	public record Order
	{
		public string OrderId { get; set; }

		public List<Item> Items { get; set; } = new();
	}
}

