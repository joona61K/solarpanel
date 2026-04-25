using Hems.Api.Models;

namespace Hems.Api.Services;

public interface IWeatherForecastService
{
    Task<List<WeatherForecastEntry>> GetForecastAsync();
}
