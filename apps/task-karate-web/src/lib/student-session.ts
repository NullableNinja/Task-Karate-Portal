import { goto } from '$app/navigation';
import { api } from '$lib/api';

export type StudentSession = { authenticated: boolean; studentId: number; displayName?: string; disclaimerRequired: boolean; birthdayWeek?: boolean; loginId?: string | null };

export async function requireStudent(): Promise<StudentSession | null> {
  try {
    const session = await api<StudentSession>('/api/student/auth/me');
    if (session.disclaimerRequired) {
      await goto('/student/disclaimer');
      return null;
    }
    return session;
  } catch {
    await goto('/student/login');
    return null;
  }
}

export function initials(value: string | undefined | null) {
  return (value ?? 'TK').split(/\s+/).filter(Boolean).slice(0, 2).map((part) => part[0]).join('').toUpperCase() || 'TK';
}

export function apiError(error: unknown, fallback = 'The live Task Karate service is unavailable.') {
  return error instanceof Error ? error.message : fallback;
}
