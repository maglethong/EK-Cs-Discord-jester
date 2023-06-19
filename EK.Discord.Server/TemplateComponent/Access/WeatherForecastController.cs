using EK.Discord.Common.TemplateComponent.Api;
using EK.Discord.Server.Base.Component.Access;
using EK.Discord.Server.TemplateComponent.Api;
using Microsoft.AspNetCore.Mvc;

namespace EK.Discord.Server.TemplateComponent.Access; 

[ApiController]
[Route("api/WeatherForecast")]
public class WeatherForecastController : AbstractControllerBase {

    private IWeatherForecastService Service { get; }
    public WeatherForecastController(IServiceProvider sp, IWeatherForecastService service) : base(sp) {
        Service = service;
    }

    [HttpGet]
    public IEnumerable<WeatherForecast> Get() {
        Logger?.LogTrace("Starting call {}#{}", nameof(WeatherForecastController), nameof(Get));
        
        return Service.GetAllForecasts();
    }

}