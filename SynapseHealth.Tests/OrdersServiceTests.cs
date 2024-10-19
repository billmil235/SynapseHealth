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
        Mock<Services.IConfigurationProvider> configurationProviderMock = _mocker.GetMock<Services.IConfigurationProvider>();

        configurationProviderMock.Setup(x => x.GetConfiguration()).Returns(new Urls()
        {
            AlertApiUrl = "/alerts",
            OrdersApiUrl = "/Orders",
            UpdateApiUrl = "/Update"
        });

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
    [InlineData(new Object[] { "Delivered", true })]
    [InlineData(new Object[] { "Not Delivered", false })]
    public void Test_IsItemDelivered(string status, bool expected)
    {
        JToken item = JToken.Parse($"{{'Status': {status}}}");

        var delivered = _ordersService.IsItemDelivered(item);
        Assert.Equal(delivered, expected);
    }
}
