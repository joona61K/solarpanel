namespace Hems.Api.Models;

public class AutomationLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string TriggerReason { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public bool Success { get; set; }
}
