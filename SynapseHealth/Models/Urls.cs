using Microsoft.Extensions.Configuration;

public class Urls
{
    public Urls(IConfiguration configuration)
    {
        configuration.Bind(this);
    }

    public string OrdersApiUrl { get; set; } = string.Empty;
    public string AlertApiUrl { get; set; } = string.Empty;
    public string UpdateApiUrl { get; set; } = string.Empty;
}