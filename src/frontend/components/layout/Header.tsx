'use client';

import { useState, useEffect } from 'react';

export default function Header() {
  const [time, setTime] = useState<string>('');

  useEffect(() => {
    const update = () => setTime(new Date().toLocaleTimeString());
    update();
    const interval = setInterval(update, 1000);
    return () => clearInterval(interval);
  }, []);

  return (
    <header className="h-16 bg-slate-800/80 backdrop-blur border-b border-slate-700 flex items-center justify-between px-6 sticky top-0 z-10">
      <div>
        <p className="text-xs text-slate-500">Home Energy Management System</p>
      </div>
      <div className="flex items-center gap-4">
        <div className="text-right">
          <p className="text-sm font-mono text-slate-300">{time}</p>
          <p className="text-xs text-slate-500">{new Date().toLocaleDateString([], { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' })}</p>
        </div>
        <div className="w-8 h-8 bg-amber-500 rounded-full flex items-center justify-center text-slate-900 font-bold text-sm">
          H
        </div>
      </div>
    </header>
  );
}
