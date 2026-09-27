export type EasterEggId =
  | 'egg-bell-finder'
  | 'egg-belt-whisperer'
  | 'egg-scroll-long-way'
  | 'egg-dojo-explorer'
  | 'egg-secret-kata'
  | 'egg-volume-control'
  | 'egg-sticker-sensei'
  | 'egg-card-carrying-student'
  | 'egg-wall-wisdom'
  | 'egg-master-hidden-dojo';

export type EasterEggDefinition = {
  id: EasterEggId;
  name: string;
  description: string;
  icon: string;
  clue: string;
};

export const EASTER_EGGS: EasterEggDefinition[] = [
  { id: 'egg-bell-finder', name: 'Bell Finder', description: 'You found the hidden dojo bell.', icon: '🔔', clue: 'The logo has a rhythm if you listen closely.' },
  { id: 'egg-belt-whisperer', name: 'Belt Whisperer', description: 'You made the belt colors orbit.', icon: '🥋', clue: 'Some rank displays have more motion than they let on.' },
  { id: 'egg-scroll-long-way', name: 'Scroll of the Long Way', description: 'You followed the progress trail all the way to black belt.', icon: '📜', clue: 'Progress indicators reward patience.' },
  { id: 'egg-dojo-explorer', name: 'Dojo Explorer', description: 'You visited every Student Hub door in one session.', icon: '🧭', clue: 'Five doors. One dojo.' },
  { id: 'egg-secret-kata', name: 'Secret Kata', description: 'You discovered the reverse navigation pattern.', icon: '🎮', clue: 'Some patterns are easier backward.' },
  { id: 'egg-volume-control', name: 'Volume Control', description: 'You found the hidden HIYAH! overdrive.', icon: '📣', clue: 'The loudest button may not be a button.' },
  { id: 'egg-sticker-sensei', name: 'Sticker Sensei', description: 'You opened the hidden dojo sticker drawer.', icon: '✨', clue: 'The GIF button knows one more trick.' },
  { id: 'egg-card-carrying-student', name: 'Card-Carrying Student', description: 'You flipped your Student Hub identity card.', icon: '🃏', clue: 'Your identity has a reverse side.' },
  { id: 'egg-wall-wisdom', name: 'Wall Wisdom', description: 'You activated the dojo motto machine.', icon: '🧠', clue: 'Tap the quietest tile a few times.' },
  { id: 'egg-master-hidden-dojo', name: 'Master of Hidden Dojos', description: 'You discovered every Student Hub easter egg.', icon: '🌟', clue: 'Nine secrets make one legend.' }
];

export const EASTER_EGG_BY_ID = Object.fromEntries(EASTER_EGGS.map((egg) => [egg.id, egg])) as Record<EasterEggId, EasterEggDefinition>;
export const HIDDEN_DOJO_EGGS = EASTER_EGGS.filter((egg) => egg.id !== 'egg-master-hidden-dojo');

export function triggerEasterEgg(id: EasterEggId) {
  if (typeof window !== 'undefined') window.dispatchEvent(new CustomEvent('task-karate:easter-egg', { detail: { id } }));
}
