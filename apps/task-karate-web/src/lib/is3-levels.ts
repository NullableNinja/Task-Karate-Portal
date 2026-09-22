export type Is3LevelGuide = { current: string; next: string; focus: string[]; sections: { category: string; items: string[] }[]; note: string };

export const is3LevelGuides: Record<string, Is3LevelGuide> = {
  'IS3 Student Level 0': {
    current: 'IS3 Student Level 0',
    next: 'IS3 Student Level 1',
    focus: ['Ready position and safe spacing', 'Foundational stick-handling patterns', 'Respectful training etiquette and partner awareness'],
    sections: [
      { category: 'Readiness and safety', items: ['Ready position with balanced posture and safe spacing', 'Identify training boundaries, partner signals, and stop commands', 'Demonstrate respectful training etiquette before and after partner work'] },
      { category: 'Foundational stick handling', items: ['Use the basic grip, chamber, and recovery position', 'Practice the foundational striking pattern with controlled lines', 'Move through the opening sequence without sacrificing balance'] },
      { category: 'Partner awareness', items: ['Maintain safe range while moving with a partner', 'Control contact and respond to instructor cues', 'Explain how to keep a partner safe during practice'] }
    ],
    note: 'Build the movement vocabulary first. Your instructor will confirm when you are ready to test for the next level.'
  },
  'IS3 Student Level 1': {
    current: 'IS3 Student Level 1',
    next: 'IS3 Student Level 2',
    focus: ['Clean striking lines and recovery', 'Footwork through the basic angles', 'Partner timing and controlled contact'],
    sections: [
      { category: 'Striking and recovery', items: ['Keep striking lines clean from ready position through recovery', 'Return to a balanced guard after each pattern', 'Maintain control while gradually adding speed'] },
      { category: 'Angles and footwork', items: ['Move through the basic angles without crossing the feet', 'Keep safe spacing while changing direction', 'Connect footwork to the foundational patterns'] },
      { category: 'Partner timing', items: ['Read partner timing and maintain controlled contact', 'Use clear verbal cues before changing drills', 'Demonstrate awareness of range, pace, and safe exits'] }
    ],
    note: 'Keep the patterns smooth and controlled before adding speed.'
  }
};
