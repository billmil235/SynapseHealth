using System;
namespace SynapseHealth.Services
{
	public interface IConfigurationProvider
	{
        Urls GetConfiguration();
    }
}

