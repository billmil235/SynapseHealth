using System;
using Microsoft.Extensions.Configuration;

namespace SynapseHealth.Services
{
	public class ConfigurationProvider : IConfigurationProvider
	{
		private readonly IConfiguration _configuration;

		public ConfigurationProvider(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <summary>
		/// Retrieves the Urls from the config file.
		/// </summary>
		/// <returns>A Urls class populated from the config file.</returns>
		public Urls GetConfiguration()
        {
			return _configuration.GetSection("Urls").Get<Urls>()!;
        }
	}
}

