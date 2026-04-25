namespace Hems.Api.Models;

public class EnergyReading
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public double ProductionWatts { get; set; }
    public double ConsumptionWatts { get; set; }
    public double GridFeedInWatts { get; set; }
    public double BatteryChargePercent { get; set; }
    public double SurplusWatts => ProductionWatts - ConsumptionWatts;
}
