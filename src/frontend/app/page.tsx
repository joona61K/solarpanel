'use client';

import { useEffect, useState } from 'react';
import axios from 'axios';
import StatCard from '@/components/ui/StatCard';
import EnergyCard from '@/components/ui/EnergyCard';

interface DashboardData {
  reading: {
    productionWatts: number;
    consumptionWatts: number;
    gridFeedInWatts: number;
    batteryChargePercent: number;
    surplusWatts: number;
    timestamp: string;
  } | null;
  currentSpotPrice: {
    pricePerKwh: number;
    currency: string;
    area: string;
    hourUtc: string;
  } | null;
  weather: {
    temperatureCelsius: number;
    cloudCoverPercent: number;
    solarRadiationWm2: number;
    precipitationMm: number;
    forecastTime: string;
  } | null;
  lastAutomationAction: {
    deviceName: string;
    action: string;
    triggerReason: string;
    timestamp: string;
    success: boolean;
  } | null;
}

export default function DashboardPage() {
  const [data, setData] = useState<DashboardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdated, setLastUpdated] = useState<Date | null>(null);

  const fetchData = async () => {
    try {
      const res = await axios.get<DashboardData>('/api/dashboard/current');
      setData(res.data);
      setLastUpdated(new Date());
      setError(null);
    } catch (err) {
      setError('Failed to load dashboard data');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
    const interval = setInterval(fetchData, 30000);
    return () => clearInterval(interval);
  }, []);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-400" />
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-900/30 border border-red-700 rounded-lg p-6 text-red-300">
        <p className="font-semibold">Error</p>
        <p>{error}</p>
        <button onClick={fetchData} className="mt-3 px-4 py-2 bg-red-700 rounded hover:bg-red-600 text-white text-sm">
          Retry
        </button>
      </div>
    );
  }

  const r = data?.reading;
  const sp = data?.currentSpotPrice;
  const w = data?.weather;
  const a = data?.lastAutomationAction;

  const surplusWatts = r ? r.productionWatts - r.consumptionWatts : 0;
  const isSurplus = surplusWatts >= 0;

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold text-slate-100 mb-1">Dashboard</h2>
        {lastUpdated && (
          <p className="text-sm text-slate-400">Last updated: {lastUpdated.toLocaleTimeString()}</p>
        )}
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">
        <EnergyCard
          title="Solar Production"
          value={r ? `${r.productionWatts.toFixed(0)} W` : '—'}
          subtitle="Current output"
          color="yellow"
          icon="☀️"
        />
        <EnergyCard
          title="Consumption"
          value={r ? `${r.consumptionWatts.toFixed(0)} W` : '—'}
          subtitle="House load"
          color="blue"
          icon="🏠"
        />
        <EnergyCard
          title={isSurplus ? 'Surplus' : 'Deficit'}
          value={r ? `${Math.abs(surplusWatts).toFixed(0)} W` : '—'}
          subtitle={isSurplus ? 'Feeding to grid' : 'Drawing from grid'}
          color={isSurplus ? 'green' : 'red'}
          icon={isSurplus ? '⚡' : '🔌'}
        />
        <EnergyCard
          title="Battery"
          value={r ? `${r.batteryChargePercent.toFixed(1)}%` : '—'}
          subtitle="Charge level"
          color="green"
          icon="🔋"
        />
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
        <StatCard
          title="Current Spot Price"
          value={sp ? `${sp.pricePerKwh.toFixed(4)}` : '—'}
          unit={sp ? `${sp.currency}/kWh` : ''}
          description={sp ? `Area: ${sp.area}` : 'No price data'}
          color={
            sp
              ? sp.pricePerKwh < 0.05
                ? 'green'
                : sp.pricePerKwh > 0.20
                ? 'red'
                : 'yellow'
              : 'gray'
          }
          icon="💶"
        />
        <StatCard
          title="Weather"
          value={w ? `${w.temperatureCelsius.toFixed(1)}°C` : '—'}
          unit=""
          description={
            w
              ? `☁️ ${w.cloudCoverPercent.toFixed(0)}% clouds · ☀️ ${w.solarRadiationWm2.toFixed(0)} W/m²`
              : 'No weather data'
          }
          color="blue"
          icon="🌤️"
        />
        <StatCard
          title="Last Automation"
          value={a ? `${a.deviceName}` : 'None'}
          unit=""
          description={
            a
              ? `${a.action} · ${new Date(a.timestamp).toLocaleTimeString()}`
              : 'No automation actions yet'
          }
          color={a ? (a.success ? 'green' : 'red') : 'gray'}
          icon="🤖"
        />
      </div>

      {r && (
        <div className="bg-slate-800 rounded-xl p-5 border border-slate-700">
          <h3 className="text-sm font-semibold text-slate-400 uppercase tracking-wider mb-4">Grid Status</h3>
          <div className="flex items-center gap-6">
            <div>
              <p className="text-xs text-slate-500">Grid Feed-In</p>
              <p className="text-xl font-bold text-emerald-400">{r.gridFeedInWatts.toFixed(0)} W</p>
            </div>
            <div className="flex-1 h-3 bg-slate-700 rounded-full overflow-hidden">
              <div
                className="h-full bg-gradient-to-r from-amber-400 to-emerald-400 transition-all duration-500"
                style={{ width: `${Math.min(100, (r.productionWatts / Math.max(r.productionWatts + r.consumptionWatts, 1)) * 100)}%` }}
              />
            </div>
            <div className="text-right">
              <p className="text-xs text-slate-500">Self-consumption</p>
              <p className="text-xl font-bold text-blue-400">
                {r.productionWatts > 0
                  ? `${Math.min(100, (r.consumptionWatts / r.productionWatts) * 100).toFixed(0)}%`
                  : '0%'}
              </p>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
