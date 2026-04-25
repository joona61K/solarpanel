using Hems.Api.Models;

namespace Hems.Api.Services;

public interface ISolarIntegrationService
{
    Task<EnergyReading> GetCurrentReadingAsync();
}
