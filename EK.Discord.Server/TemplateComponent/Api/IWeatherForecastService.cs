using EK.Discord.Common.Base.Component.Api;
using EK.Discord.Common.TemplateComponent.Api;

namespace EK.Discord.Server.TemplateComponent.Api; 

public interface IWeatherForecastService : IService {

    public IEnumerable<WeatherForecast> GetAllForecasts();

}