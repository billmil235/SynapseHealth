using Microsoft.Extensions.Configuration;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json.Linq;
using SynapseHealth.Services;

namespace SynapseHealth.Tests;

public class OrdersServiceTests
{
    private readonly AutoMocker _mocker;
    private readonly OrdersService _ordersService;

    public OrdersServiceTests()
    {
        _mocker = new AutoMocker();
        _ordersService = _mocker.CreateInstance<OrdersService>();
    }

    [Fact]
    public void Test_IncrementDeliveryNotification()
    {
        JToken item = JToken.Parse("{'deliveryNotification': 1}");

        _ordersService.IncrementDeliveryNotification(item);
        Assert.True(item["deliveryNotification"]!.Value<int>() == 2);
    }

    [Theory]
    [InlineData(new Object[] { "Delivered" })]
    public void Test_IsItemDelivered(string status)
    {
        JToken item = JToken.Parse($"{{'Status': {status}}}");

        var delivered = _ordersService.IsItemDelivered(item);
    }
}
