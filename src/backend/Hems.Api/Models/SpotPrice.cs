namespace Hems.Api.Models;

public class SpotPrice
{
    public int Id { get; set; }
    public DateTime HourUtc { get; set; }
    public string Currency { get; set; } = "EUR";
    public double PricePerKwh { get; set; }
    public string Area { get; set; } = string.Empty;
}
