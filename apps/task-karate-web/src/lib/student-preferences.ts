export type HubTheme = 'midnight' | 'paper' | 'contrast';

export const HUB_THEMES: { id: HubTheme; label: string; description: string }[] = [
  { id: 'midnight', label: 'Midnight', description: 'The original dojo-at-night look.' },
  { id: 'paper', label: 'Paper dojo', description: 'A warmer, softer reading surface.' },
  { id: 'contrast', label: 'High contrast', description: 'Sharper edges and stronger text contrast.' }
];

export const DEFAULT_PROFILE_TILES = [
  'rank', 'next-stripe', 'total-classes', 'monthly-classes', 'streak', 'next-class',
  'active-goals', 'programs', 'favorite-technique', 'profile-readiness'
];

export function readPreference<T>(key: string, fallback: T): T {
  if (typeof localStorage === 'undefined') return fallback;
  try {
    const value = localStorage.getItem(key);
    return value ? JSON.parse(value) as T : fallback;
  } catch { return fallback; }
}

export function writePreference<T>(key: string, value: T) {
  if (typeof localStorage !== 'undefined') localStorage.setItem(key, JSON.stringify(value));
}

