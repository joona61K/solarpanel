using Hems.Api.Models;

namespace Hems.Api.Services;

public interface ISpotPriceService
{
    Task<List<SpotPrice>> GetTodaysPricesAsync();
}
