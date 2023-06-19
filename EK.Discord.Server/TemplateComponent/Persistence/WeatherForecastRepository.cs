
using EK.Discord.Common.TemplateComponent.Api;

namespace EK.Discord.Server.TemplateComponent.Persistence; 

public class WeatherForecastRepository {

    private static readonly string[] Summaries = new[] {
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    };

    public IEnumerable<WeatherForecast> GetAllForecasts() {
        return Enumerable.Range(1, 5)
                         .Select(index => new WeatherForecast {
                                 Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                                 TemperatureC = Random.Shared.Next(-20, 55),
                                 Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                             }
                         )
                         .ToList();;
    }
}