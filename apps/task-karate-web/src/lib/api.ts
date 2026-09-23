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
  const controller = new AbortController();
  const timer = window.setTimeout(() => controller.abort(), 15000);
  try {
    const response = await fetch(`${API_BASE}${path}`, { ...init, method, headers, credentials: 'include', signal: init.signal ?? controller.signal });
    if (!response.ok) {
      if (response.status === 401) {
        resetCsrf();
        throw new Error(path.startsWith('/api/student') ? 'Your student session has expired. Please sign in again.' : 'Your staff session has expired. Please sign in again.');
      }
      if (response.status === 403) throw new Error('You do not have permission to complete that action.');
      const payload = await response.json().catch(() => ({}));
      const validation = payload.errors ? Object.values(payload.errors).flat().join(' ') : '';
      throw new Error(validation || payload.detail || payload.title || `Request failed (${response.status}).`);
    }
    return response.status === 204 ? (undefined as T) : response.json();
  } catch (error) {
    if (error instanceof DOMException && error.name === 'AbortError') throw new Error('The API took too long to respond. Check that the local server is running and try again.');
    if (error instanceof TypeError) throw new Error('The API is unavailable. Start the Task Karate API and try again.');
    throw error;
  } finally {
    window.clearTimeout(timer);
  }
}

export function resetCsrf() { csrf = null; }
