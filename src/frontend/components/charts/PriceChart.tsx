'use client';

import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from 'recharts';

interface PriceDataPoint {
  hour: string;
  price: number;
  isCurrent: boolean;
}

interface PriceChartProps {
  data: PriceDataPoint[];
}

export default function PriceChart({ data }: PriceChartProps) {
  const getColor = (price: number, isCurrent: boolean) => {
    if (isCurrent) return '#f59e0b';
    if (price < 5) return '#10b981';
    if (price > 20) return '#ef4444';
    return '#3b82f6';
  };

  return (
    <ResponsiveContainer width="100%" height={280}>
      <BarChart data={data} margin={{ top: 5, right: 20, left: 10, bottom: 40 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="#334155" vertical={false} />
        <XAxis
          dataKey="hour"
          stroke="#64748b"
          tick={{ fill: '#94a3b8', fontSize: 10 }}
          angle={-45}
          textAnchor="end"
        />
        <YAxis
          stroke="#64748b"
          tick={{ fill: '#94a3b8', fontSize: 11 }}
          tickFormatter={v => `${v}c`}
        />
        <Tooltip
          contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }}
          labelStyle={{ color: '#94a3b8' }}
          formatter={(v: number) => [`${v} c/kWh`, 'Price']}
        />
        <Bar dataKey="price" radius={[4, 4, 0, 0]}>
          {data.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={getColor(entry.price, entry.isCurrent)} />
          ))}
        </Bar>
      </BarChart>
    </ResponsiveContainer>
  );
}
