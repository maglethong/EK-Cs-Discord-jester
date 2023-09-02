using EK.Discord.Common.Base.Component.Business;
using EK.Discord.Common.TemplateComponent.Api;
using EK.Discord.Server.TemplateComponent.Api;
using EK.Discord.Server.TemplateComponent.Persistence;

namespace EK.Discord.Server.TemplateComponent.Business; 

public class WeatherForecastService : AbstractServiceBase, IWeatherForecastService {

    private WeatherForecastRepository Repository { get; }

    public WeatherForecastService(IServiceProvider serviceProvider, WeatherForecastRepository repository) : base(serviceProvider) {
        Repository = repository;
    }
    
    public IEnumerable<WeatherForecast> GetAllForecasts() {
        return Repository.GetAllForecasts();
    }

}