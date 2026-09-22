export const API_BASE = (import.meta.env.VITE_API_BASE as string | undefined) ?? '';

let csrf: string | null = null;

async function csrfToken() {
  if (csrf) return csrf;
  const response = await fetch(`${API_BASE}/api/auth/csrf`, { credentials: 'include' });
  if (!response.ok) throw new Error('The API is unavailable. Start the Task Karate API and try again.');
  const value = (await response.json()).token as string | null;
  if (!value) throw new Error('The API did not provide a CSRF token.');
  csrf = value;
  return value;
}

export async function api<T>(path: string, init: RequestInit = {}): Promise<T> {
  const method = (init.method ?? 'GET').toUpperCase();
  const headers = new Headers(init.headers);
  headers.set('Accept', 'application/json');
  if (init.body && !headers.has('Content-Type')) headers.set('Content-Type', 'application/json');
  if (method !== 'GET' && method !== 'HEAD') headers.set('X-XSRF-TOKEN', await csrfToken());
  const response = await fetch(`${API_BASE}${path}`, { ...init, method, headers, credentials: 'include' });
  if (!response.ok) {
    if (response.status === 401) throw new Error('Your staff session has expired. Sign in again.');
    const payload = await response.json().catch(() => ({}));
    throw new Error(payload.detail ?? payload.title ?? `Request failed (${response.status}).`);
  }
  return response.status === 204 ? (undefined as T) : response.json();
}

export function resetCsrf() { csrf = null; }
