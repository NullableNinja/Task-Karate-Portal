export function formatDojoTime(value?: string | null): string {
  if (!value) return 'Time TBA';
  const match = value.trim().match(/^(\d{1,2})(?::(\d{2}))?\s*(AM|PM)?$/i);
  if (!match) return value;
  let hour = Number(match[1]);
  const minute = Number(match[2] ?? '0');
  const suffix = match[3]?.toUpperCase();
  if (!suffix && hour > 23) return value;
  if (suffix === 'PM' && hour < 12) hour += 12;
  if (suffix === 'AM' && hour === 12) hour = 0;
  if (hour > 23 || minute > 59) return value;
  const displayHour = hour % 12 || 12;
  return `${displayHour}:${String(minute).padStart(2, '0')} ${hour >= 12 ? 'PM' : 'AM'}`;
}

export function formatDojoRange(start?: string | null, end?: string | null): string {
  const first = formatDojoTime(start);
  return end ? `${first} – ${formatDojoTime(end)}` : first;
}
