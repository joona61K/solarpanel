interface EnergyCardProps {
  title: string;
  value: string;
  subtitle: string;
  color: 'yellow' | 'blue' | 'green' | 'red';
  icon: string;
}

const colorMap = {
  yellow: { bg: 'bg-amber-900/20', border: 'border-amber-700/50', text: 'text-amber-400', iconBg: 'bg-amber-900/40' },
  blue: { bg: 'bg-blue-900/20', border: 'border-blue-700/50', text: 'text-blue-400', iconBg: 'bg-blue-900/40' },
  green: { bg: 'bg-emerald-900/20', border: 'border-emerald-700/50', text: 'text-emerald-400', iconBg: 'bg-emerald-900/40' },
  red: { bg: 'bg-red-900/20', border: 'border-red-700/50', text: 'text-red-400', iconBg: 'bg-red-900/40' },
};

export default function EnergyCard({ title, value, subtitle, color, icon }: EnergyCardProps) {
  const c = colorMap[color];
  return (
    <div className={`rounded-xl p-5 border ${c.bg} ${c.border}`}>
      <div className="flex items-start justify-between mb-3">
        <span className="text-sm font-medium text-slate-400">{title}</span>
        <span className={`text-2xl p-2 rounded-lg ${c.iconBg}`}>{icon}</span>
      </div>
      <p className={`text-3xl font-bold ${c.text}`}>{value}</p>
      <p className="text-xs text-slate-500 mt-1">{subtitle}</p>
    </div>
  );
}
