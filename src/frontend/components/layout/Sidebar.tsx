'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';

const navItems = [
  { href: '/', label: 'Dashboard', icon: '⚡' },
  { href: '/history', label: 'History', icon: '📊' },
  { href: '/prices', label: 'Spot Prices', icon: '💶' },
  { href: '/automation', label: 'Automation', icon: '🤖' },
];

export default function Sidebar() {
  const pathname = usePathname();

  return (
    <aside className="fixed left-0 top-0 h-screen w-64 bg-slate-800 border-r border-slate-700 flex flex-col z-10">
      <div className="p-6 border-b border-slate-700">
        <div className="flex items-center gap-3">
          <span className="text-3xl">☀️</span>
          <div>
            <h1 className="text-lg font-bold text-slate-100">HEMS</h1>
            <p className="text-xs text-slate-400">Energy Manager</p>
          </div>
        </div>
      </div>
      <nav className="flex-1 p-4 space-y-1">
        {navItems.map(item => {
          const active = pathname === item.href;
          return (
            <Link
              key={item.href}
              href={item.href}
              className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
                active
                  ? 'bg-amber-500/20 text-amber-400 border border-amber-500/30'
                  : 'text-slate-400 hover:bg-slate-700 hover:text-slate-100'
              }`}
            >
              <span className="text-xl">{item.icon}</span>
              {item.label}
            </Link>
          );
        })}
      </nav>
      <div className="p-4 border-t border-slate-700">
        <div className="bg-slate-700/50 rounded-lg p-3">
          <p className="text-xs text-slate-400 font-medium">System Status</p>
          <div className="flex items-center gap-2 mt-1.5">
            <span className="w-2 h-2 bg-emerald-400 rounded-full animate-pulse" />
            <span className="text-xs text-emerald-400">Online</span>
          </div>
        </div>
      </div>
    </aside>
  );
}
