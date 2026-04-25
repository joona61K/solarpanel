interface StatCardProps {
  title: string;
  value: string;
  unit: string;
  description: string;
  color: 'green' | 'red' | 'yellow' | 'blue' | 'gray';
  icon: string;
}

const colorMap = {
  green: 'text-emerald-400',
  red: 'text-red-400',
  yellow: 'text-amber-400',
  blue: 'text-blue-400',
  gray: 'text-slate-400',
};

export default function StatCard({ title, value, unit, description, color, icon }: StatCardProps) {
  return (
    <div className="bg-slate-800 rounded-xl p-5 border border-slate-700">
      <div className="flex items-center gap-2 mb-3">
        <span className="text-xl">{icon}</span>
        <span className="text-sm font-medium text-slate-400">{title}</span>
      </div>
      <div className="flex items-baseline gap-1">
        <span className={`text-2xl font-bold ${colorMap[color]}`}>{value}</span>
        {unit && <span className="text-sm text-slate-500">{unit}</span>}
      </div>
      <p className="text-xs text-slate-500 mt-1.5">{description}</p>
    </div>
  );
}
