# ☀️ HEMS — Home Energy Management System

A full-stack system to monitor solar production, track electricity spot prices, display weather forecasts, and automate devices (water heater, car charger) based on configurable rules.

```
┌─────────────────────────────────────────────────────────┐
│                    Browser (port 3000)                  │
│              Next.js + React + Tailwind CSS             │
│   Dashboard │ History │ Spot Prices │ Automation Logs   │
└───────────────────────┬─────────────────────────────────┘
                        │  HTTP /api/*  (proxied)
┌───────────────────────▼─────────────────────────────────┐
│                 Backend (port 8080)                     │
│            ASP.NET Core 8 Web API (C#)                  │
│  DashboardCtrl │ EnergyCtrl │ PricesCtrl │ AutomCtrl   │
│  SolarService  │ SpotPriceService │ WeatherService      │
│  AutomationEngineService (IHostedService, every 5 min)  │
└───────────────────────┬─────────────────────────────────┘
                        │  EF Core + SQLite
┌───────────────────────▼─────────────────────────────────┐
│              hems.db  (SQLite)                          │
│  EnergyReadings │ SpotPrices │ WeatherForecasts         │
│  AutomationLogs                                         │
└─────────────────────────────────────────────────────────┘
        │                │                │
  Fronius Inverter   spot-hinta.fi    open-meteo.com
  (Shelly relays)   (Finland spot)   (weather/solar)
```

## Prerequisites

- **Docker + Docker Compose** (recommended) — or
- **.NET 8 SDK** + **Node.js 20+** for manual setup

## Quick Start (Docker)

```bash
git clone https://github.com/joona61K/solarpanel.git
cd solarpanel
mkdir -p data
docker compose up --build
```

- Frontend: http://localhost:3000
- Backend API: http://localhost:8080
- Swagger UI: http://localhost:8080/swagger

## Manual Setup

### Backend

```bash
cd src/backend/Hems.Api
dotnet restore
dotnet ef database update      # creates hems.db
dotnet run                     # listens on http://localhost:8080
```

### Frontend

```bash
cd src/frontend
npm install
BACKEND_URL=http://localhost:8080 npm run dev   # http://localhost:3000
```

## Configuration (`appsettings.json` / environment variables)

| Key | Default | Description |
|-----|---------|-------------|
| `Hems:InverterUrl` | `http://192.168.1.100` | Fronius inverter base URL |
| `Hems:SpotPriceArea` | `FI` | Electricity market area |
| `Hems:WeatherLatitude` | `60.17` | Location latitude |
| `Hems:WeatherLongitude` | `24.94` | Location longitude |
| `Hems:ShellyBaseUrl` | `http://192.168.1.200` | Shelly relay base URL |
| `Hems:SurplusThresholdWatts` | `500` | Min surplus (W) to turn on water heater |
| `Hems:CheapPriceThresholdEur` | `0.05` | Price (EUR/kWh) below which to start car charger |
| `Hems:ExpensivePriceThresholdEur` | `0.20` | Price (EUR/kWh) above which to shut off devices |

Override with Docker environment variables using double-underscore notation:
```yaml
environment:
  - Hems__SurplusThresholdWatts=800
```

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/dashboard/current` | Live reading + spot price + weather + last automation |
| GET | `/api/dashboard/history?days=7` | Hourly-aggregated history |
| GET | `/api/energy/readings?from=&to=&page=&pageSize=` | Paginated readings |
| GET | `/api/energy/readings/latest` | Most recent reading |
| POST | `/api/energy/readings` | Add manual reading |
| GET | `/api/prices/today` | Today's hourly spot prices |
| GET | `/api/prices/refresh` | Force-refresh from spot-hinta.fi |
| GET | `/api/automation/logs` | Last 50 automation actions |
| GET | `/api/automation/rules` | List of configured rules |
| POST | `/api/automation/trigger` | Manually run automation evaluation |

## Connecting Real Hardware

### Fronius Inverter
Set `Hems:InverterUrl` to your inverter's IP. The service calls:
`/solar_api/v1/GetPowerFlowRealtimeData.fcgi`

### Shelly Relay
Set `Hems:ShellyBaseUrl` to your Shelly device IP. The automation engine calls:
`GET http://<shelly-ip>/relay/0?turn=on|off`

For multiple devices, extend `AutomationEngineService.cs` with a device-to-IP mapping.

### Spot Prices
Currently uses [spot-hinta.fi](https://spot-hinta.fi) (Finland). For other areas, swap the URL in `SpotPriceService.cs`.

### Weather
Uses [Open-Meteo](https://open-meteo.com) (free, no API key). Adjust `WeatherLatitude`/`WeatherLongitude` for your location.

## Adding Automation Rules

Edit `AutomationEngineService.cs` → `EvaluateRulesAsync()`. Example:

```csharp
// Turn on dishwasher if price < 3 c/kWh
if (currentPrice?.PricePerKwh < 0.03)
{
    await ControlRelay("Dishwasher", true);
    await LogActionAsync(db, "Ultra cheap price", "Dishwasher", "TurnOn", true);
}
```

## Project Structure

```
solarpanel/
├── docker-compose.yml
├── .gitignore
├── README.md
├── data/                          # SQLite database (gitignored)
└── src/
    ├── backend/
    │   ├── Dockerfile
    │   └── Hems.Api/
    │       ├── Controllers/       # DashboardController, EnergyController, ...
    │       ├── Data/              # HemsDbContext + EF Migrations
    │       ├── Models/            # EnergyReading, SpotPrice, ...
    │       ├── Services/          # Solar, SpotPrice, Weather, Automation
    │       ├── Program.cs
    │       └── appsettings.json
    └── frontend/
        ├── Dockerfile
        ├── app/                   # Next.js App Router pages
        │   ├── page.tsx           # Dashboard
        │   ├── history/page.tsx
        │   ├── prices/page.tsx
        │   └── automation/page.tsx
        └── components/
            ├── charts/            # EnergyChart, PriceChart (Recharts)
            ├── layout/            # Sidebar, Header
            └── ui/                # EnergyCard, StatCard
```

## License

MIT
