'use client';

import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

interface DataPoint {
  time: string;
  Production: number;
  Consumption: number;
  'Grid Feed-In': number;
}

interface EnergyChartProps {
  data: DataPoint[];
}

export default function EnergyChart({ data }: EnergyChartProps) {
  return (
    <ResponsiveContainer width="100%" height={350}>
      <LineChart data={data} margin={{ top: 5, right: 30, left: 20, bottom: 60 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="#334155" />
        <XAxis
          dataKey="time"
          stroke="#64748b"
          tick={{ fill: '#94a3b8', fontSize: 11 }}
          angle={-45}
          textAnchor="end"
          interval="preserveStartEnd"
        />
        <YAxis
          stroke="#64748b"
          tick={{ fill: '#94a3b8', fontSize: 11 }}
          tickFormatter={v => `${v}W`}
        />
        <Tooltip
          contentStyle={{ backgroundColor: '#1e293b', border: '1px solid #334155', borderRadius: '8px' }}
          labelStyle={{ color: '#94a3b8' }}
          itemStyle={{ color: '#e2e8f0' }}
          formatter={(v: number) => [`${v} W`]}
        />
        <Legend
          wrapperStyle={{ paddingTop: '20px', color: '#94a3b8' }}
        />
        <Line type="monotone" dataKey="Production" stroke="#f59e0b" strokeWidth={2} dot={false} />
        <Line type="monotone" dataKey="Consumption" stroke="#3b82f6" strokeWidth={2} dot={false} />
        <Line type="monotone" dataKey="Grid Feed-In" stroke="#10b981" strokeWidth={2} dot={false} />
      </LineChart>
    </ResponsiveContainer>
  );
}
