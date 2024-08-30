using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mocca.Interfaces;
using Mocca.Services;

namespace Mocca;

public static class ServiceExtensions
{
    public static IServiceCollection AddMocca(this IServiceCollection services)
    {
        return AddMocca(services, _ => { });
    }

    public static IServiceCollection AddMocca(this IServiceCollection services, Action<MoccaOptions> configureOptions)
    {
        services
            .AddHttpClient("ProxyClient")
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<MoccaOptions>>().Value;

                if (!string.IsNullOrWhiteSpace(options.ForwardTo))
                {
                    client.BaseAddress = new Uri(options.ForwardTo);
                }
            })
            .ConfigurePrimaryHttpMessageHandler(
                () => new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    AutomaticDecompression = System.Net.DecompressionMethods.All,
                });
        
        services.AddScoped<IMoccaRepository, MoccaJsonRepository>();
        
        services.Configure(configureOptions);
        return services;
    }
}