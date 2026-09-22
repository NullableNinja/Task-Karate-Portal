const rankColors: Record<string, string> = {
  white: '#e8edf2',
  yellow: '#e6c54a',
  orange: '#ee8b2c',
  green: '#43a85b',
  blue: '#4d8bd8',
  purple: '#9564c5',
  red: '#cb414b',
  brown: '#8a5131',
  black: '#171c24',
  gold: '#d4aa3b'
};

export function rankColor(rank: string | null | undefined) {
  const key = Object.keys(rankColors).find((name) => (rank ?? '').toLowerCase().includes(name));
  return key ? rankColors[key] : '#5ab8eb';
}

export function rankTextColor(rank: string | null | undefined) {
  const key = Object.keys(rankColors).find((name) => (rank ?? '').toLowerCase().includes(name));
  return key === 'white' || key === 'yellow' || key === 'orange' || key === 'gold' ? '#172331' : '#f8fbff';
}
