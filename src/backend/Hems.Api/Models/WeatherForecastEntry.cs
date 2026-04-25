namespace Hems.Api.Models;

public class WeatherForecastEntry
{
    public int Id { get; set; }
    public DateTime ForecastTime { get; set; }
    public double TemperatureCelsius { get; set; }
    public double CloudCoverPercent { get; set; }
    public double SolarRadiationWm2 { get; set; }
    public double PrecipitationMm { get; set; }
}
