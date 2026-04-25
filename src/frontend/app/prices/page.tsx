'use client';

import { useEffect, useState } from 'react';
import axios from 'axios';
import PriceChart from '@/components/charts/PriceChart';

interface SpotPrice {
  id: number;
  hourUtc: string;
  pricePerKwh: number;
  currency: string;
  area: string;
}

export default function PricesPage() {
  const [prices, setPrices] = useState<SpotPrice[]>([]);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);

  const fetchPrices = async () => {
    try {
      const res = await axios.get<SpotPrice[]>('/api/prices/today');
      setPrices(res.data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const refresh = async () => {
    setRefreshing(true);
    await axios.get('/api/prices/refresh').catch(console.error);
    await fetchPrices();
    setRefreshing(false);
  };

  useEffect(() => { fetchPrices(); }, []);

  const now = new Date();
  const currentHour = now.getUTCHours();

  const chartData = prices.map(p => {
    const hour = new Date(p.hourUtc).getUTCHours();
    return {
      hour: `${String(hour).padStart(2, '0')}:00`,
      price: parseFloat((p.pricePerKwh * 100).toFixed(2)),
      isCurrent: hour === currentHour,
    };
  });

  const currentPrice = prices.find(p => new Date(p.hourUtc).getUTCHours() === currentHour);
  const minPrice = prices.length > 0 ? Math.min(...prices.map(p => p.pricePerKwh)) : 0;
  const maxPrice = prices.length > 0 ? Math.max(...prices.map(p => p.pricePerKwh)) : 0;
  const avgPrice = prices.length > 0 ? prices.reduce((s, p) => s + p.pricePerKwh, 0) / prices.length : 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Spot Prices</h2>
        <button
          onClick={refresh}
          disabled={refreshing}
          className="px-4 py-2 bg-amber-500 hover:bg-amber-400 text-slate-900 font-semibold rounded-lg text-sm transition-colors disabled:opacity-50"
        >
          {refreshing ? 'Refreshing…' : '↻ Refresh'}
        </button>
      </div>

      {loading ? (
        <div className="flex justify-center py-24">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-amber-400" />
        </div>
      ) : (
        <>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {[
              { label: 'Current', value: currentPrice ? `${(currentPrice.pricePerKwh * 100).toFixed(2)} c/kWh` : '—', color: 'text-amber-400' },
              { label: 'Min Today', value: `${(minPrice * 100).toFixed(2)} c/kWh`, color: 'text-emerald-400' },
              { label: 'Max Today', value: `${(maxPrice * 100).toFixed(2)} c/kWh`, color: 'text-red-400' },
              { label: 'Average', value: `${(avgPrice * 100).toFixed(2)} c/kWh`, color: 'text-blue-400' },
            ].map(s => (
              <div key={s.label} className="bg-slate-800 rounded-xl p-5 border border-slate-700">
                <p className="text-xs text-slate-400 uppercase tracking-wider">{s.label}</p>
                <p className={`text-xl font-bold mt-1 ${s.color}`}>{s.value}</p>
              </div>
            ))}
          </div>

          <div className="bg-slate-800 rounded-xl p-6 border border-slate-700">
            <h3 className="text-sm font-semibold text-slate-400 uppercase tracking-wider mb-6">
              Hourly Prices (c/kWh) — {prices[0]?.area ?? 'FI'}
            </h3>
            <PriceChart data={chartData} />
          </div>

          <div className="bg-slate-800 rounded-xl border border-slate-700 overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-slate-700/50">
                <tr>
                  <th className="px-4 py-3 text-left text-slate-400 font-medium">Hour (UTC)</th>
                  <th className="px-4 py-3 text-right text-slate-400 font-medium">Price (c/kWh)</th>
                  <th className="px-4 py-3 text-right text-slate-400 font-medium">Level</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-700/50">
                {prices.map(p => {
                  const hour = new Date(p.hourUtc).getUTCHours();
                  const isCurrent = hour === currentHour;
                  const cents = p.pricePerKwh * 100;
                  const level = cents < 5 ? 'Cheap' : cents > 20 ? 'Expensive' : 'Normal';
                  const levelColor = cents < 5 ? 'text-emerald-400' : cents > 20 ? 'text-red-400' : 'text-yellow-400';
                  return (
                    <tr key={p.id} className={isCurrent ? 'bg-amber-900/20' : 'hover:bg-slate-700/30'}>
                      <td className="px-4 py-2.5 font-mono">
                        {String(hour).padStart(2, '0')}:00
                        {isCurrent && <span className="ml-2 text-xs bg-amber-500 text-slate-900 px-1.5 py-0.5 rounded font-semibold">NOW</span>}
                      </td>
                      <td className="px-4 py-2.5 text-right font-mono">{cents.toFixed(2)}</td>
                      <td className={`px-4 py-2.5 text-right font-medium ${levelColor}`}>{level}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
