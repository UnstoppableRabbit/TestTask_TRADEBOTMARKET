using ArbitrageDomain.Config;
using ArbitrageDomain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BinanceScraperService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBinanceScraper(this IServiceCollection services, IConfiguration configuration)
        {
            var section = configuration.GetSection("BinancePairs");
            var configs = section.GetChildren();

            foreach (var configSection in configs)
            {
                var config = configSection.Get<BinanceScraperConfig>();
                var name = configSection.Key;

                services.AddHttpClient(name);
                services.AddTransient<IScraperService>(provider =>
                {
                    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                    var client = httpClientFactory.CreateClient(name);
                    return new BinanceScraperService(config, client, name);
                });
            }

            return services;
        }
    }
}
