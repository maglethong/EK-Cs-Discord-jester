using EK.Discord.Server.TemplateComponent.Api;
using EK.Discord.Server.TemplateComponent.Business;
using EK.Discord.Server.TemplateComponent.Persistence;

namespace EK.Discord.Server.TemplateComponent; 

public static class TemplateControllerDependencyInjectionConfiguration {

    public static IServiceCollection AddTemplateController(this IServiceCollection services) {
        return services.AddScoped<IWeatherForecastService, WeatherForecastService>()
                       .AddTransient<WeatherForecastRepository>();
    }
}