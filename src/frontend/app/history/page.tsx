'use client';

import { useEffect, useState } from 'react';
import axios from 'axios';
import EnergyChart from '@/components/charts/EnergyChart';

interface HistoryPoint {
  timestamp: string;
  avgProductionWatts: number;
  avgConsumptionWatts: number;
  avgGridFeedInWatts: number;
}

export default function HistoryPage() {
  const [data, setData] = useState<HistoryPoint[]>([]);
  const [loading, setLoading] = useState(true);
  const [days, setDays] = useState(7);

  useEffect(() => {
    setLoading(true);
    axios.get<HistoryPoint[]>(`/api/dashboard/history?days=${days}`)
      .then(res => setData(res.data))
      .catch(console.error)
      .finally(() => setLoading(false));
  }, [days]);

  const chartData = data.map(d => ({
    time: new Date(d.timestamp).toLocaleString([], { month: 'short', day: 'numeric', hour: '2-digit' }),
    Production: Math.round(d.avgProductionWatts),
    Consumption: Math.round(d.avgConsumptionWatts),
    'Grid Feed-In': Math.round(d.avgGridFeedInWatts),
  }));

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Energy History</h2>
        <div className="flex gap-2">
          {[1, 7, 14, 30].map(d => (
            <button
              key={d}
              onClick={() => setDays(d)}
              className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                days === d
                  ? 'bg-amber-500 text-slate-900'
                  : 'bg-slate-700 text-slate-300 hover:bg-slate-600'
              }`}
            >
              {d}d
            </button>
          ))}
        </div>
      </div>

      {loading ? (
        <div className="flex justify-center py-24">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-400" />
        </div>
      ) : data.length === 0 ? (
        <div className="bg-slate-800 rounded-xl p-12 text-center text-slate-400">
          <p className="text-lg">No history data yet.</p>
          <p className="text-sm mt-1">Data will appear after the system has been running for a while.</p>
        </div>
      ) : (
        <div className="bg-slate-800 rounded-xl p-6 border border-slate-700">
          <h3 className="text-sm font-semibold text-slate-400 uppercase tracking-wider mb-6">
            Production vs Consumption (Hourly Averages)
          </h3>
          <EnergyChart data={chartData} />
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {data.length > 0 && [
          {
            label: 'Avg Production',
            value: `${(data.reduce((s, d) => s + d.avgProductionWatts, 0) / data.length).toFixed(0)} W`,
            color: 'text-amber-400',
          },
          {
            label: 'Avg Consumption',
            value: `${(data.reduce((s, d) => s + d.avgConsumptionWatts, 0) / data.length).toFixed(0)} W`,
            color: 'text-blue-400',
          },
          {
            label: 'Avg Grid Feed-In',
            value: `${(data.reduce((s, d) => s + d.avgGridFeedInWatts, 0) / data.length).toFixed(0)} W`,
            color: 'text-emerald-400',
          },
        ].map(stat => (
          <div key={stat.label} className="bg-slate-800 rounded-xl p-5 border border-slate-700">
            <p className="text-sm text-slate-400">{stat.label}</p>
            <p className={`text-2xl font-bold mt-1 ${stat.color}`}>{stat.value}</p>
          </div>
        ))}
      </div>
    </div>
  );
}
