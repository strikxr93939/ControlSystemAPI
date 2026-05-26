const API_BASE = process.env.REACT_APP_API_URL || 'http://localhost:5186';

async function request(path, options = {}) {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...options.headers },
    ...options,
  });
  if (!res.ok) throw new Error(`API ${res.status}: ${await res.text()}`);
  if (res.status === 204) return null;
  return res.json();
}

// ── Violations ─────────────────────────────────────────────────────────
export const getViolations = (limit = 100) =>
  request(`/api/violations?limit=${limit}`);

export const deleteViolation = (id) =>
  request(`/api/violations/${id}`, { method: 'DELETE' });

// ── Policies ───────────────────────────────────────────────────────────
export const getPolicies = () => request('/api/policies');

export const createPolicy = (body) =>
  request('/api/policies', { method: 'POST', body: JSON.stringify(body) });

export const updatePolicy = (id, body) =>
  request(`/api/policies/${id}`, { method: 'PUT', body: JSON.stringify(body) });

export const togglePolicy = (id) =>
  request(`/api/policies/${id}/toggle`, { method: 'PATCH' });

export const deletePolicy = (id) =>
  request(`/api/policies/${id}`, { method: 'DELETE' });

// ── Computers ──────────────────────────────────────────────────────────
export const getComputers = () => request('/api/computers');
