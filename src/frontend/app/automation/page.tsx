'use client';

import { useEffect, useState } from 'react';
import axios from 'axios';

interface AutomationLog {
  id: number;
  timestamp: string;
  triggerReason: string;
  deviceName: string;
  action: string;
  success: boolean;
}

interface AutomationRule {
  id: number;
  name: string;
  description: string;
  condition: string;
  action: string;
  enabled: boolean;
}

export default function AutomationPage() {
  const [logs, setLogs] = useState<AutomationLog[]>([]);
  const [rules, setRules] = useState<AutomationRule[]>([]);
  const [loading, setLoading] = useState(true);
  const [triggering, setTriggering] = useState(false);
  const [triggerMsg, setTriggerMsg] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([
      axios.get<AutomationLog[]>('/api/automation/logs'),
      axios.get<AutomationRule[]>('/api/automation/rules'),
    ])
      .then(([logsRes, rulesRes]) => {
        setLogs(logsRes.data);
        setRules(rulesRes.data);
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const triggerNow = async () => {
    setTriggering(true);
    setTriggerMsg(null);
    try {
      await axios.post('/api/automation/trigger');
      setTriggerMsg('Automation evaluation triggered successfully!');
      const res = await axios.get<AutomationLog[]>('/api/automation/logs');
      setLogs(res.data);
    } catch {
      setTriggerMsg('Failed to trigger automation.');
    } finally {
      setTriggering(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-bold">Automation</h2>
        <button
          onClick={triggerNow}
          disabled={triggering}
          className="px-4 py-2 bg-emerald-600 hover:bg-emerald-500 text-white font-semibold rounded-lg text-sm transition-colors disabled:opacity-50"
        >
          {triggering ? 'Running…' : '▶ Run Now'}
        </button>
      </div>

      {triggerMsg && (
        <div className="bg-emerald-900/30 border border-emerald-700 rounded-lg p-3 text-emerald-300 text-sm">
          {triggerMsg}
        </div>
      )}

      <div>
        <h3 className="text-lg font-semibold text-slate-200 mb-3">Active Rules</h3>
        <div className="grid gap-3">
          {rules.map(rule => (
            <div key={rule.id} className="bg-slate-800 rounded-xl p-5 border border-slate-700 flex gap-4">
              <div className={`w-3 h-3 rounded-full mt-1 flex-shrink-0 ${rule.enabled ? 'bg-emerald-400' : 'bg-slate-600'}`} />
              <div className="flex-1">
                <p className="font-semibold text-slate-100">{rule.name}</p>
                <p className="text-sm text-slate-400 mt-0.5">{rule.description}</p>
                <div className="mt-2 flex gap-4 text-xs font-mono">
                  <span className="text-blue-400">IF: {rule.condition}</span>
                  <span className="text-amber-400">THEN: {rule.action}</span>
                </div>
              </div>
              <span className={`text-xs px-2 py-1 rounded-full self-start font-medium ${rule.enabled ? 'bg-emerald-900/50 text-emerald-300' : 'bg-slate-700 text-slate-400'}`}>
                {rule.enabled ? 'Active' : 'Disabled'}
              </span>
            </div>
          ))}
        </div>
      </div>

      <div>
        <h3 className="text-lg font-semibold text-slate-200 mb-3">Recent Actions</h3>
        {loading ? (
          <div className="flex justify-center py-12">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-amber-400" />
          </div>
        ) : logs.length === 0 ? (
          <div className="bg-slate-800 rounded-xl p-8 text-center text-slate-400 border border-slate-700">
            No automation actions yet.
          </div>
        ) : (
          <div className="bg-slate-800 rounded-xl border border-slate-700 overflow-hidden">
            <table className="w-full text-sm">
              <thead className="bg-slate-700/50">
                <tr>
                  <th className="px-4 py-3 text-left text-slate-400 font-medium">Time</th>
                  <th className="px-4 py-3 text-left text-slate-400 font-medium">Device</th>
                  <th className="px-4 py-3 text-left text-slate-400 font-medium">Action</th>
                  <th className="px-4 py-3 text-left text-slate-400 font-medium">Reason</th>
                  <th className="px-4 py-3 text-center text-slate-400 font-medium">Status</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-700/50">
                {logs.map(log => (
                  <tr key={log.id} className="hover:bg-slate-700/30">
                    <td className="px-4 py-2.5 font-mono text-xs text-slate-400">
                      {new Date(log.timestamp).toLocaleString()}
                    </td>
                    <td className="px-4 py-2.5 font-medium">{log.deviceName}</td>
                    <td className="px-4 py-2.5">
                      <span className={`px-2 py-0.5 rounded text-xs font-semibold ${
                        log.action.toLowerCase().includes('on')
                          ? 'bg-emerald-900/50 text-emerald-300'
                          : 'bg-red-900/50 text-red-300'
                      }`}>
                        {log.action}
                      </span>
                    </td>
                    <td className="px-4 py-2.5 text-slate-400 text-xs max-w-xs truncate">{log.triggerReason}</td>
                    <td className="px-4 py-2.5 text-center">
                      {log.success
                        ? <span className="text-emerald-400 text-lg">✓</span>
                        : <span className="text-red-400 text-lg">✗</span>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
