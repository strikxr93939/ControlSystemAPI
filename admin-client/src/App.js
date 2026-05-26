import React, { useEffect, useState, useCallback, useRef } from 'react';
import { getConnection } from './signalr';
import {
  getViolations, deleteViolation,
  getPolicies, createPolicy, togglePolicy, deletePolicy,
  getComputers,
} from './api';

const BLOCK_TYPE_LABELS = ['⚠️ Warning', '🔶 SoftBlock', '🔴 HardBlock', '⏱️ Timed'];
const BLOCK_TYPE_COLORS = ['#f59e0b', '#f97316', '#ef4444', '#8b5cf6'];

// ── Helpers ────────────────────────────────────────────────────────────
function fmt(dt) {
  if (!dt) return '—';
  return new Date(dt).toLocaleString('ru-RU');
}
function timeAgo(dt) {
  const diff = Date.now() - new Date(dt).getTime();
  const s = Math.floor(diff / 1000);
  if (s < 60) return `${s}s ago`;
  if (s < 3600) return `${Math.floor(s / 60)}m ago`;
  return `${Math.floor(s / 3600)}h ago`;
}

// ── Badge ──────────────────────────────────────────────────────────────
function Badge({ type }) {
  const label = BLOCK_TYPE_LABELS[type] ?? '❓ Unknown';
  const color = BLOCK_TYPE_COLORS[type] ?? '#6b7280';
  return (
    <span style={{
      background: color + '22', color, border: `1px solid ${color}`,
      borderRadius: 6, padding: '2px 8px', fontSize: 12, fontWeight: 600,
    }}>
      {label}
    </span>
  );
}

// ── Status dot ─────────────────────────────────────────────────────────
function StatusDot({ connected }) {
  return (
    <span style={{
      display: 'inline-block', width: 10, height: 10, borderRadius: '50%',
      background: connected ? '#22c55e' : '#ef4444',
      boxShadow: connected ? '0 0 6px #22c55e' : '0 0 6px #ef4444',
      marginRight: 6,
    }} />
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Violations Panel
// ═══════════════════════════════════════════════════════════════════════
function ViolationsPanel({ violations, onDelete }) {
  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 12 }}>
        <h2 style={{ margin: 0, fontSize: 18 }}>
          🚨 Violations
          <span style={{
            marginLeft: 10, background: '#ef444422', color: '#ef4444',
            border: '1px solid #ef4444', borderRadius: 12,
            padding: '1px 10px', fontSize: 13
          }}>
            {violations.length}
          </span>
        </h2>
      </div>

      {violations.length === 0 ? (
        <div style={styles.empty}>✅ No violations detected</div>
      ) : (
        <div style={styles.list}>
          {violations.map(v => (
            <div key={v.id} style={styles.card}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <div>
                  <span style={{ fontWeight: 700, fontSize: 15 }}>💻 {v.computerName}</span>
                  <span style={{ margin: '0 8px', color: '#6b7280' }}>›</span>
                  <span style={{ fontWeight: 600, color: '#f97316' }}>⚙️ {v.programName}</span>
                </div>
                <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
                  <Badge type={v.blockType} />
                  <button onClick={() => onDelete(v.id)} style={styles.delBtn} title="Delete">✕</button>
                </div>
              </div>

              <div style={{ marginTop: 6, display: 'flex', gap: 16, flexWrap: 'wrap' }}>
                {v.version && (
                  <span style={styles.meta}>
                    Detected: <b>{v.version}</b>
                  </span>
                )}
                {v.requiredVersion && (
                  <span style={styles.meta}>
                    Required: <b>{v.requiredVersion}</b>
                  </span>
                )}
                <span style={styles.meta}>
                  Action: <b>{v.userAction}</b>
                </span>
                {v.userName && (
                  <span style={styles.meta}>
                    User: <b>{v.userName}</b>
                  </span>
                )}
                <span style={{ ...styles.meta, marginLeft: 'auto', color: '#9ca3af' }}>
                  🕐 {timeAgo(v.timestamp)}
                </span>
              </div>

              {v.message && (
                <div style={{ marginTop: 6, color: '#d97706', fontSize: 13, fontStyle: 'italic' }}>
                  📝 {v.message}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Policies Panel
// ═══════════════════════════════════════════════════════════════════════
const emptyPolicy = {
  programPattern: '', minVersion: '', maxVersion: '',
  blockType: 0, workshop: '', message: '',
  isActive: true, startTime: new Date().toISOString().slice(0, 16),
  exceptions: '',
};

function PoliciesPanel({ policies, onRefresh }) {
  const [form, setForm] = useState(emptyPolicy);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const set = (k, v) => setForm(f => ({ ...f, [k]: v }));

  const handleSubmit = async () => {
    if (!form.programPattern.trim()) {
      setError('Program pattern is required');
      return;
    }
    setSaving(true);
    setError('');
    try {
      await createPolicy({
        ...form,
        blockType: parseInt(form.blockType),
        startTime: new Date(form.startTime).toISOString(),
        endTime: form.endTime ? new Date(form.endTime).toISOString() : null,
      });
      setForm(emptyPolicy);
      onRefresh();
    } catch (e) {
      setError(e.message);
    } finally {
      setSaving(false);
    }
  };

  const handleToggle = async (id) => {
    try { await togglePolicy(id); onRefresh(); } catch (e) { alert(e.message); }
  };
  const handleDelete = async (id) => {
    if (!window.confirm('Delete this policy?')) return;
    try { await deletePolicy(id); onRefresh(); } catch (e) { alert(e.message); }
  };

  return (
    <div>
      <h2 style={{ margin: '0 0 16px', fontSize: 18 }}>📋 Policies</h2>

      {/* ── Create form ── */}
      <div style={styles.formBox}>
        <h3 style={{ margin: '0 0 12px', fontSize: 15 }}>➕ New Policy</h3>

        <div style={styles.formGrid}>
          <label style={styles.label}>
            Program Pattern *
            <input style={styles.input} value={form.programPattern}
              onChange={e => set('programPattern', e.target.value)}
              placeholder="chrome, teams, notepad…" />
          </label>

          <label style={styles.label}>
            Block Type
            <select style={styles.input} value={form.blockType}
              onChange={e => set('blockType', e.target.value)}>
              <option value={0}>⚠️ Warning</option>
              <option value={1}>🔶 SoftBlock</option>
              <option value={2}>🔴 HardBlock (kill)</option>
              <option value={3}>⏱️ Timed</option>
            </select>
          </label>

          <label style={styles.label}>
            Min Version
            <input style={styles.input} value={form.minVersion}
              onChange={e => set('minVersion', e.target.value)}
              placeholder="1.0.0.0" />
          </label>

          <label style={styles.label}>
            Max Version
            <input style={styles.input} value={form.maxVersion}
              onChange={e => set('maxVersion', e.target.value)}
              placeholder="optional" />
          </label>

          <label style={styles.label}>
            Workshop
            <input style={styles.input} value={form.workshop}
              onChange={e => set('workshop', e.target.value)}
              placeholder="Workshop A" />
          </label>

          <label style={styles.label}>
            Exceptions (computers)
            <input style={styles.input} value={form.exceptions}
              onChange={e => set('exceptions', e.target.value)}
              placeholder="PC-001, PC-002" />
          </label>

          <label style={{ ...styles.label, gridColumn: '1 / -1' }}>
            Message
            <input style={styles.input} value={form.message}
              onChange={e => set('message', e.target.value)}
              placeholder="Human-readable policy message" />
          </label>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: 12, marginTop: 12 }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: 6, cursor: 'pointer' }}>
            <input type="checkbox" checked={form.isActive}
              onChange={e => set('isActive', e.target.checked)} />
            Active
          </label>
          {error && <span style={{ color: '#ef4444', fontSize: 13 }}>❌ {error}</span>}
          <button style={{ ...styles.btn, marginLeft: 'auto' }}
            onClick={handleSubmit} disabled={saving}>
            {saving ? '⏳ Saving…' : '✅ Create Policy'}
          </button>
        </div>
      </div>

      {/* ── List ── */}
      {policies.length === 0 ? (
        <div style={styles.empty}>No policies defined yet.</div>
      ) : (
        <div style={styles.list}>
          {policies.map(p => (
            <div key={p.id} style={{
              ...styles.card,
              borderLeft: `4px solid ${p.isActive ? '#22c55e' : '#6b7280'}`
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                  <code style={{ fontWeight: 700, fontSize: 15 }}>🔍 {p.programPattern}</code>
                  {p.workshop && (
                    <span style={{ marginLeft: 10, color: '#6b7280', fontSize: 13 }}>
                      🏭 {p.workshop}
                    </span>
                  )}
                </div>
                <div style={{ display: 'flex', gap: 8 }}>
                  <Badge type={p.blockType} />
                  <button
                    onClick={() => handleToggle(p.id)}
                    style={{
                      ...styles.btn,
                      background: p.isActive ? '#22c55e22' : '#6b728022',
                      color: p.isActive ? '#22c55e' : '#9ca3af',
                      border: `1px solid ${p.isActive ? '#22c55e' : '#6b7280'}`,
                      padding: '3px 10px',
                    }}>
                    {p.isActive ? '🟢 Active' : '⚫ Inactive'}
                  </button>
                  <button onClick={() => handleDelete(p.id)} style={styles.delBtn}>✕</button>
                </div>
              </div>

              <div style={{ marginTop: 6, display: 'flex', gap: 16, flexWrap: 'wrap' }}>
                {p.minVersion && <span style={styles.meta}>Min: <b>{p.minVersion}</b></span>}
                {p.maxVersion && <span style={styles.meta}>Max: <b>{p.maxVersion}</b></span>}
                {p.exceptions && <span style={styles.meta}>Except: <b>{p.exceptions}</b></span>}
                <span style={styles.meta}>Since: {fmt(p.startTime)}</span>
              </div>
              {p.message && (
                <div style={{ marginTop: 6, color: '#9ca3af', fontSize: 13 }}>
                  💬 {p.message}
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Computers Panel
// ═══════════════════════════════════════════════════════════════════════
function ComputersPanel({ computers }) {
  return (
    <div>
      <h2 style={{ margin: '0 0 16px', fontSize: 18 }}>🖥️ Computers</h2>
      {computers.length === 0 ? (
        <div style={styles.empty}>No computers registered.</div>
      ) : (
        <div style={styles.list}>
          {computers.map(c => (
            <div key={c.id} style={{
              ...styles.card,
              borderLeft: `4px solid ${c.isOnline ? '#22c55e' : '#ef4444'}`
            }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ fontWeight: 700 }}>
                  {c.isOnline ? '🟢' : '🔴'} {c.name}
                </span>
                <span style={{ color: '#9ca3af', fontSize: 13 }}>
                  Last seen: {timeAgo(c.lastSeen)}
                </span>
              </div>
              <div style={{ marginTop: 6, display: 'flex', gap: 16, flexWrap: 'wrap' }}>
                {c.workshop && <span style={styles.meta}>🏭 {c.workshop}</span>}
                {c.ipAddress && <span style={styles.meta}>🌐 {c.ipAddress}</span>}
                {c.lastUser && <span style={styles.meta}>👤 {c.lastUser}</span>}
                {c.osVersion && <span style={styles.meta}>💿 {c.osVersion}</span>}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

// ═══════════════════════════════════════════════════════════════════════
// Main App
// ═══════════════════════════════════════════════════════════════════════
export default function App() {
  const [tab, setTab] = useState('violations');
  const [violations, setViolations] = useState([]);
  const [policies, setPolicies] = useState([]);
  const [computers, setComputers] = useState([]);
  const [connected, setConnected] = useState(false);
  const [toasts, setToasts] = useState([]);
  const connRef = useRef(null);

  const toast = useCallback((msg, color = '#22c55e') => {
    const id = Date.now();
    setToasts(t => [...t, { id, msg, color }]);
    setTimeout(() => setToasts(t => t.filter(x => x.id !== id)), 4000);
  }, []);

  // ── Load data ──────────────────────────────────────────────────────
  const loadViolations = useCallback(async () => {
    try { setViolations(await getViolations(100)); } catch (e) { console.error(e); }
  }, []);

  const loadPolicies = useCallback(async () => {
    try { setPolicies(await getPolicies()); } catch (e) { console.error(e); }
  }, []);

  const loadComputers = useCallback(async () => {
    try { setComputers(await getComputers()); } catch (e) { console.error(e); }
  }, []);

  const loadAll = useCallback(() => {
    loadViolations(); loadPolicies(); loadComputers();
  }, [loadViolations, loadPolicies, loadComputers]);

  useEffect(() => { loadAll(); }, [loadAll]);

  // Auto-refresh every 30s
  useEffect(() => {
    const id = setInterval(loadAll, 30_000);
    return () => clearInterval(id);
  }, [loadAll]);

  // ── SignalR ────────────────────────────────────────────────────────
  useEffect(() => {
    const conn = getConnection();
    connRef.current = conn;

    conn.on('ViolationReceived', (v) => {
      setViolations(prev => [v, ...prev.slice(0, 99)]);
      toast(`🚨 Violation: ${v.programName} on ${v.computerName}`, '#ef4444');
    });

    conn.on('PoliciesUpdated', () => {
      loadPolicies();
      toast('📋 Policies updated');
    });

    conn.on('ComputerStatusChanged', () => {
      loadComputers();
    });

    const start = async () => {
      try {
        await conn.start();
        setConnected(true);
        toast('🔗 SignalR connected');
      } catch (e) {
        setConnected(false);
        setTimeout(start, 5000);
      }
    };

    conn.onclose(() => setConnected(false));
    conn.onreconnecting(() => setConnected(false));
    conn.onreconnected(() => { setConnected(true); loadAll(); });

    start();
    return () => { conn.stop(); };
  }, [loadPolicies, loadAll, toast]);

  // ── Violation actions ──────────────────────────────────────────────
  const handleDeleteViolation = async (id) => {
    try {
      await deleteViolation(id);
      setViolations(v => v.filter(x => x.id !== id));
      toast('🗑️ Violation deleted');
    } catch (e) { alert(e.message); }
  };

  // ── Stats ──────────────────────────────────────────────────────────
  const activeViolations = violations.filter(v => {
    const age = Date.now() - new Date(v.timestamp).getTime();
    return age < 3600_000; // last hour
  });

  // ── Render ─────────────────────────────────────────────────────────
  return (
    <div style={styles.root}>
      {/* Toast notifications */}
      <div style={styles.toastContainer}>
        {toasts.map(t => (
          <div key={t.id} style={{ ...styles.toast, borderLeft: `4px solid ${t.color}` }}>
            {t.msg}
          </div>
        ))}
      </div>

      {/* Header */}
      <header style={styles.header}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
          <span style={{ fontSize: 24 }}>🛡️</span>
          <div>
            <div style={{ fontWeight: 800, fontSize: 18 }}>VersionControl Admin</div>
            <div style={{ fontSize: 12, color: '#9ca3af' }}>Software Monitoring Dashboard</div>
          </div>
        </div>
        <div style={{ display: 'flex', alignItems: 'center', gap: 24 }}>
          {/* Stats */}
          <div style={styles.statBox}>
            <div style={{ fontSize: 20, fontWeight: 700, color: '#ef4444' }}>
              {activeViolations.length}
            </div>
            <div style={{ fontSize: 11, color: '#9ca3af' }}>violations/hr</div>
          </div>
          <div style={styles.statBox}>
            <div style={{ fontSize: 20, fontWeight: 700, color: '#22c55e' }}>
              {policies.filter(p => p.isActive).length}
            </div>
            <div style={{ fontSize: 11, color: '#9ca3af' }}>active policies</div>
          </div>
          <div style={styles.statBox}>
            <div style={{ fontSize: 20, fontWeight: 700, color: '#60a5fa' }}>
              {computers.length}
            </div>
            <div style={{ fontSize: 11, color: '#9ca3af' }}>computers</div>
          </div>
          {/* SignalR status */}
          <div style={{ display: 'flex', alignItems: 'center', fontSize: 13 }}>
            <StatusDot connected={connected} />
            {connected ? 'Live' : 'Offline'}
          </div>
          <button onClick={loadAll} style={styles.refreshBtn}>🔄 Refresh</button>
        </div>
      </header>

      {/* Tabs */}
      <nav style={styles.nav}>
        {[
          ['violations', `🚨 Violations (${violations.length})`],
          ['policies',   `📋 Policies (${policies.length})`],
          ['computers',  `🖥️ Computers (${computers.length})`],
        ].map(([key, label]) => (
          <button key={key} onClick={() => setTab(key)}
            style={{ ...styles.tabBtn, ...(tab === key ? styles.tabActive : {}) }}>
            {label}
          </button>
        ))}
      </nav>

      {/* Content */}
      <main style={styles.main}>
        {tab === 'violations' && (
          <ViolationsPanel violations={violations} onDelete={handleDeleteViolation} />
        )}
        {tab === 'policies' && (
          <PoliciesPanel policies={policies} onRefresh={loadPolicies} />
        )}
        {tab === 'computers' && (
          <ComputersPanel computers={computers} />
        )}
      </main>
    </div>
  );
}

// ── Styles ─────────────────────────────────────────────────────────────
const styles = {
  root: {
    minHeight: '100vh',
    background: '#0f172a',
    color: '#e2e8f0',
    fontFamily: "'Segoe UI', system-ui, sans-serif",
  },
  header: {
    background: '#1e293b',
    borderBottom: '1px solid #334155',
    padding: '14px 24px',
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  nav: {
    background: '#1e293b',
    borderBottom: '1px solid #334155',
    padding: '0 24px',
    display: 'flex',
    gap: 4,
  },
  tabBtn: {
    background: 'transparent',
    border: 'none',
    color: '#94a3b8',
    padding: '12px 18px',
    cursor: 'pointer',
    fontSize: 14,
    fontWeight: 500,
    borderBottom: '2px solid transparent',
    transition: 'all 0.15s',
  },
  tabActive: {
    color: '#60a5fa',
    borderBottom: '2px solid #60a5fa',
  },
  main: {
    maxWidth: 1200,
    margin: '0 auto',
    padding: 24,
  },
  card: {
    background: '#1e293b',
    border: '1px solid #334155',
    borderRadius: 10,
    padding: '14px 16px',
    marginBottom: 10,
    transition: 'border-color 0.15s',
  },
  list: { display: 'flex', flexDirection: 'column' },
  empty: {
    textAlign: 'center',
    color: '#6b7280',
    padding: '40px 0',
    fontSize: 15,
  },
  meta: { fontSize: 13, color: '#94a3b8' },
  btn: {
    background: '#2563eb',
    color: '#fff',
    border: 'none',
    borderRadius: 8,
    padding: '8px 18px',
    cursor: 'pointer',
    fontWeight: 600,
    fontSize: 14,
  },
  delBtn: {
    background: '#ef444422',
    color: '#ef4444',
    border: '1px solid #ef4444',
    borderRadius: 6,
    padding: '2px 8px',
    cursor: 'pointer',
    fontSize: 13,
  },
  refreshBtn: {
    background: '#334155',
    color: '#e2e8f0',
    border: '1px solid #475569',
    borderRadius: 8,
    padding: '6px 14px',
    cursor: 'pointer',
    fontSize: 13,
  },
  statBox: {
    textAlign: 'center',
    padding: '0 12px',
    borderRight: '1px solid #334155',
  },
  formBox: {
    background: '#1e293b',
    border: '1px solid #334155',
    borderRadius: 10,
    padding: 20,
    marginBottom: 20,
  },
  formGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
    gap: 12,
  },
  label: {
    display: 'flex',
    flexDirection: 'column',
    gap: 4,
    fontSize: 13,
    color: '#94a3b8',
    fontWeight: 500,
  },
  input: {
    background: '#0f172a',
    border: '1px solid #334155',
    borderRadius: 6,
    color: '#e2e8f0',
    padding: '8px 10px',
    fontSize: 14,
    outline: 'none',
    marginTop: 4,
  },
  toastContainer: {
    position: 'fixed',
    top: 16,
    right: 16,
    zIndex: 9999,
    display: 'flex',
    flexDirection: 'column',
    gap: 8,
  },
  toast: {
    background: '#1e293b',
    border: '1px solid #334155',
    borderRadius: 8,
    padding: '10px 16px',
    fontSize: 14,
    color: '#e2e8f0',
    boxShadow: '0 4px 20px rgba(0,0,0,0.5)',
    minWidth: 260,
    animation: 'fadeIn 0.2s ease',
  },
};
