export type Is3LevelGuide = { current: string; next: string; focus: string[]; note: string };

export const is3LevelGuides: Record<string, Is3LevelGuide> = {
  'IS3 Student Level 0': {
    current: 'IS3 Student Level 0',
    next: 'IS3 Student Level 1',
    focus: ['Ready position and safe spacing', 'Foundational stick-handling patterns', 'Respectful training etiquette and partner awareness'],
    note: 'Build the movement vocabulary first. Your instructor will confirm when you are ready to test for the next level.'
  },
  'IS3 Student Level 1': {
    current: 'IS3 Student Level 1',
    next: 'IS3 Student Level 2',
    focus: ['Clean striking lines and recovery', 'Footwork through the basic angles', 'Partner timing and controlled contact'],
    note: 'Keep the patterns smooth and controlled before adding speed.'
  }
};
