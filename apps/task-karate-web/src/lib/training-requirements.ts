export type TrainingSection = { category: string; items: string[] };
export type TrainingPlan = { rank: string; color: string; promotionGuide: string; sections: TrainingSection[] };

export const beltOrder = ['White Belt', 'Gold Belt', 'Orange Belt', 'Green Belt', 'Purple Belt', 'Blue Belt', 'Red Belt', 'Brown Belt', 'Black Belt'];

export const trainingPlans: Record<string, TrainingPlan> = {
  'White Belt': { rank: 'White Belt', color: '#e7edf0', promotionGuide: 'Eligible to test after the second lesson.', sections: [
    { category: 'The three rules', items: ['Do not use it the wrong way.', 'Be respectful.', 'Do your best.'] },
    { category: 'Basic stances', items: ['Attention stance', 'Chun Bi (ready stance)', 'Fighting stance'] },
    { category: 'Reflection', items: ['Bring an example of how you used the second or third rule in daily life.', 'Set a goal to keep practicing the three basic rules.'] }
  ] },
  'Gold Belt': { rank: 'Gold Belt', color: '#d8ad42', promotionGuide: 'Projected guide: 12–24 lessons and 2–3 months of attendance, plus an understanding of the rules and techniques.', sections: [
    { category: 'Fighting Stance', items: ['Fighting stance (with switching)', 'Advancing and retreating', 'Step slide (fencing)'] },
    { category: 'Punching', items: ['Rear hand punch'] },
    { category: 'Kicking', items: ['Back leg snap front kick (top of the foot, land in back)'] },
    { category: 'Self-Defense (Emergency and Release)', items: ['Front choke (step back, swing arm over, elbow)'] },
    { category: 'Defense Against Strikes (Rotates Each Month)', items: ['Straight punch, haymaker, backfist'] },
    { category: 'Traditional Form Movements', items: ['Front stance (advancing and retreating)', 'Front stance, stepping punch'] }
  ] },
  'Orange Belt': { rank: 'Orange Belt', color: '#e87832', promotionGuide: 'Projected guide: 16–24 lessons and 2–3 months of attendance, plus an understanding of the rules and techniques.', sections: [
    { category: 'Fighting Stance', items: ['Fighting stance (with switching)', 'Advancing and retreating', 'Step, slide (fencing)'] },
    { category: 'Punching', items: ['Jab', 'Rear hand punch'] },
    { category: 'Kicking', items: ['Back leg snap front kick (top of the foot)', 'Front leg round kick'] },
    { category: 'Self-Defense (Emergency and Release)', items: ['Front choke (step back, swing arm over, elbow)', 'Double lapel (step back, grab elbows, knee, palm)'] },
    { category: 'Defense Against Strikes (Rotates Each Month)', items: ['Straight punch, haymaker, backfist'] },
    { category: 'Traditional Form Movements', items: ['Front stance (advancing and retreating)', 'Front stance, stepping punch', 'Back stance (advancing and retreating, hands up or down chop)'] },
    { category: 'Form', items: ['First half of form #1 (bonus! All of form 1, 2, or 3!)'] },
    { category: 'Kick–Punch Combination', items: ['Front leg round kick, rear hand punch'] }
  ] },
  'Green Belt': { rank: 'Green Belt', color: '#42a879', promotionGuide: 'Projected guide: 24–40 lessons and 3–5 months of attendance, plus an understanding of the rules and techniques.', sections: [
    { category: 'Fighting Stance', items: ['Fighting Stance (with switching)', 'Advancing and Retreating', 'Step-Slide (fencing)'] },
    { category: 'Punching', items: ['Jab', 'Rear hand punch'] },
    { category: 'Punching Combination', items: ['Double punch (front jab, rear punch)'] },
    { category: 'Kicking', items: ['Back leg snap front kick (top of the foot, land in back)', 'Front leg round kick (land in front)', 'Step, slide, side kick (land strong in front, slide back)'] },
    { category: 'Self-Defense (Emergency and Release)', items: ['Front choke', 'Double lapel grab', 'Same side, cross side, and two to one hand wrist grabs (6)'] },
    { category: 'Defense Against Strikes (Rotates Each Month)', items: ['Straight punch, haymaker, backfist'] },
    { category: 'Traditional Form Movements', items: ['Front stance stepping punch', 'Front stance down block', 'Back stance down chop'] },
    { category: 'Form', items: ['Form #1', 'Bonus form (form #2, #3, Kan-na, or choice)'] },
    { category: 'Kick-Punch Combination', items: ['Front leg round kick, double punch'] }
  ] },
  'Purple Belt': { rank: 'Purple Belt', color: '#9569c3', promotionGuide: 'Projected guide: 32–56 lessons and 4–7 months of attendance, plus an understanding of the techniques.', sections: [
    { category: 'Stances', items: ['Fighting Stance', 'Chun Bi', 'Front Stance', 'Back Stance'] },
    { category: 'Punching', items: ['Rear Hand Punch', 'Double Punch', 'Rear Hand Ridge Hand'] },
    { category: 'Kicking', items: ['Front Leg Snap Front Kick', 'Rear Leg Crescent Kick', 'Front Leg Round Kick', 'Front Leg Side Kick'] },
    { category: 'Combination Kicking', items: ['Double Round Kick (Scooting in)'] },
    { category: 'Self Defense (Emergency and Release)', items: ['Front Choke · Rear Choke (Two Hands) · Rear Forearm Choke', 'Double Lapel Grab', 'Same, Cross, and Two Hand Wrist Grabs (6)'] },
    { category: 'One-Step Sparring', items: ['Front Kick Defense'] },
    { category: 'Traditional Form Movements', items: ['Double Knife Hand Block (Advancing)', 'Stepping Punch (Advancing)', 'Under Middle Block, Shift-Punch, Rising Block (Once)'] },
    { category: 'Form', items: ['Palgue 7'] },
    { category: 'Kick-Punch Combination', items: ['Double Round Kick, Double Punch'] }
  ] },
  'Blue Belt': { rank: 'Blue Belt', color: '#4f9ee8', promotionGuide: 'Projected guide: 40–64 lessons and 5–8 months of attendance, plus an understanding of the techniques.', sections: [
    { category: 'Stances', items: ['Fighting Stance', 'Chun Bi', 'Front Stance', 'Back Stance'] },
    { category: 'Punching', items: ['Rear Hand Punch', 'Double Punch', 'Double Punch, Front Ridge Hand'] },
    { category: 'Kicking', items: ['Front Leg Snap Front Kick', 'Rear Leg Crescent Kick', 'Front Leg Round Kick', 'Front Leg Side Kick', 'Front Leg Hook Kick'] },
    { category: 'Combination Kicking', items: ['Hook Kick, Round Kick (Scooting in)'] },
    { category: 'Self Defense (Emergency and Release)', items: ['Front Choke · Rear Choke (Two Hands) · Rear Forearm Choke', 'Double Lapel Grab · Single Lapel', 'Same, Cross, and Two Hand Wrist Grabs (6)'] },
    { category: 'One-Step Sparring', items: ['Front Kick Defense'] },
    { category: 'Traditional Form Movements', items: ['Double Knife Hand Block (Advancing)', 'Stepping Punch (Advancing)', 'Under Middle Block, Shift-Punch, Rising Block, Back Leg Side Kick, Double Knife'] },
    { category: 'Form', items: ['Palgue 7'] },
    { category: 'Kick-Punch Combination', items: ['Double Round Kick, Double Punch, Back Leg Round Kick'] },
    { category: 'Sparring', items: ['2 Rounds'] }
  ] },
  'Red Belt': { rank: 'Red Belt', color: '#d85a63', promotionGuide: 'Projected guide: 50–72 lessons and 6–9 months of attendance, plus an understanding of the techniques.', sections: [
    { category: 'Punching', items: ['Rear Hand Punch', 'Double Punch', 'Jab, Uppercut'] },
    { category: 'Basic Kicking', items: ['Front Leg Snap Front Kick', 'Front Leg Round Kick', 'Back Leg Round Kick', 'Front Leg Side Kick', 'Front Leg Hook Kick', 'Rear Leg Crescent Kick'] },
    { category: 'Combination Kicking', items: ['Front Leg Side, Round (Scooting In)', 'Back Leg Side, Turning Side'] },
    { category: 'Self Defense (Emergency and Release)', items: ['Two Hand Front · Two Hand Back · Rear Forearm · Side Headlock', 'Same Side · Cross Side · Two To One', 'Single Lapel · Double Lapel · Side Shoulder Grab'] },
    { category: 'One-Step Sparring', items: ['Inside Block, Grab and Elbow', 'Outside Round Kick, Grab and Knee, Double Elbow'] },
    { category: 'Traditional Form Movements', items: ['Front Stance, Stepping Punch', 'Double Knife Hand Block, Shift and Punch', 'Thrust Front Kick, Front Stance, Punch'] },
    { category: 'Form', items: ['Chung-Mu'] },
    { category: 'X-Rays', items: ['Spinning Crescent', 'Spinning Hook'] },
    { category: 'Kick-Punch Combination', items: ['Double Round Kick, Double Punch, Back Leg Round Kick, Front Hand Ridge Hand'] },
    { category: 'Sparring', items: ['3 Rounds'] }
  ] },
  'Brown Belt': { rank: 'Brown Belt', color: '#9c6a45', promotionGuide: 'Projected guide: 50+ lessons and 6–12 months of attendance, plus an understanding of the techniques.', sections: [
    { category: 'Punching', items: ['Rear Hand Punch', 'Double Punch', 'Hook Punch', 'Back-fist, Spin Back-fist, Rear Punch'] },
    { category: 'Basic Kicking', items: ['Front Leg Snap Front Kick · Front Leg Hook Kick', 'Front Leg Round Kick · Back Leg Round Kick', 'Front Leg Side Kick · Back Leg Side Kick', 'Rear Leg Crescent Kick'] },
    { category: 'Combination Kicking', items: ['Triple Round Kick (Scooting)', 'Back Leg Side Kick, Turning Side Kick'] },
    { category: 'Self Defense (Emergency and Release)', items: ['Two Hand Front · Two Hand Back · Rear Forearm · Side Headlock', 'Same Side · Cross Side · Two To One · Two To Two (Front) · Two To Two (Rear)', 'Single Lapel · Double Lapel · Side Shoulder Grab'] },
    { category: 'One-Step Sparring', items: ['Outside In Block, Rear Elbow, Rear Knee', 'Inside Parry, Grab, Strike'] },
    { category: 'Traditional Form Movements', items: ['Double Knife Hand, Shift and Punch', 'Back Leg Front Kick, Punch', 'Front Stance Stepping Punch', 'Front Stance Down Block'] },
    { category: 'Form', items: ['Form #1 · Chung-Mu'] },
    { category: 'X-Rays', items: ['Spinning Crescent', 'Spinning Hook', 'Tornado', 'Pop Up Round Kick'] },
    { category: 'Pads', items: ['Back Leg Front Kick', 'Defensive Side Kick', 'Turning Side Kick', 'Back Leg Round Kick'] },
    { category: 'Kick-Punch Combination', items: ['Double Round Kick, Double Punch, Back Leg Round Kick, Front Ridge, Rear Punch', 'Double Punch, Back Leg Round, Tornado, Spin'] },
    { category: 'Sparring', items: ['4 Rounds'] }
  ] },
  'Black Belt': { rank: 'Black Belt', color: '#aeb8c5', promotionGuide: 'Black belt degrees are instructor tracked: 2 years for second degree, 3 years for third degree, 4 years for fourth degree, and one additional year for each later degree.', sections: [
    { category: 'Punching', items: ['Rear hand punch', 'Double punch', 'The blitz (back-fist, stepping punch, rear hand punch)', 'Jab, uppercut, hook, punch'] },
    { category: 'Basic Kicking', items: ['Back leg snap kick · Back leg round kick (3 count)', 'Front leg round kick · Back leg side kick (3 count)', 'Step slide side kick · Back leg form (lock) front kick', 'Front leg hook kick', 'Back leg outside crescent kick (outside of foot)'] },
    { category: 'Combination Kicking', items: ['Front leg front, skipping front, flying front', 'Triple round kick (moving in, three heights)', 'Back leg side, turning side'] },
    { category: 'Self Defense', items: ['Two hand front · Two hand back · Rear forearm · Side headlock', 'Same side · Cross side · Two to one', 'Two to two (front) · Two to two (rear)', 'Single lapel · Double lapel · Side shoulder grab', 'Full nelson · Bear hug from front, arms free or pinned', 'Bear hug from back, arms free or pinned'] },
    { category: 'One Step Sparring', items: ['(Stepping punch) Step side round kick, grab, knee, double elbow', '(Stepping punch) Step outside, break, elbow, spin through', '(Back leg front kick) Step back, (open the gate) sweeping forearm block, rear punch'] },
    { category: 'Kadena De Mano', items: ['Drill #9, also known as the cross-block drill'] },
    { category: 'Traditional Movements', items: ['Double knife hand, shift and punch', 'Front stance stepping punch', 'Back leg form front kick, punch', 'Under middle block, shift, punch, cross, rising block', 'Front stance down block'] },
    { category: 'Forms', items: ['Form #1', 'Palgue 7', 'Chung-Mu', 'Bonus form, weapons form to music, optional additional empty hand form'] },
    { category: 'X-Rays (Speed and Accuracy)', items: ['Pop up round kick', 'Switching axe kick', 'Spinning hook kick', 'Spinning crescent kick', 'Tornado round kick', 'Flying spin crescent kick'] },
    { category: 'Pads (Power)', items: ['Defense front kick (pad holder moves in)', 'Defensive side kick (pad holder moves in)', 'Back leg front kick (land forward)', 'Step slide side kick (land forward)', 'Turning side kick (land forward)', 'Back leg round kick, 5 for power (land forward) 5 fast, (set down)'] },
    { category: 'Kick and Punch Combinations', items: ['1. Double round kick, double punch, back leg round kick, front hand ridge hand, rear punch, spinning crescent kick', '2. Back-fist, spin back-fist, rear punch, back leg round, tornado, spinning crescent', '3. Double punch, back leg round, spin, front round, spin'] },
    { category: 'Board Breaking', items: ['Side Kick and Palm'] },
    { category: 'Sparring', items: ['5 Rounds (or more)'] },
    { category: 'Black belt path', items: ['Second degree: 2-year development window.', 'Third degree: 3-year development window.', 'Fourth degree: 4-year development window.', 'Each later degree adds one year to the development window.'] }
  ] }
};

export const kidsTrainingPlans: Record<string, TrainingPlan> = {
  'Purple Belt': { rank: 'Purple Belt', color: '#9569c3', promotionGuide: 'Projected guide: 32–56 lessons and 4–7 months of attendance, plus an understanding of the techniques.', sections: [
    { category: 'Stances', items: ['Fighting Stance', 'Chun Bi', 'Front Stance', 'Back Stance'] },
    { category: 'Punching', items: ['Rear Hand Punch', 'Double Punch', 'Rear Hand Ridge Hand'] },
    { category: 'Kicking', items: ['Front Leg Snap Front Kick', 'Rear Leg Crescent Kick', 'Front Leg Round Kick', 'Front Leg Side Kick'] },
    { category: 'Combination Kicking', items: ['Double Round Kick (Scooting in)'] },
    { category: 'Self Defense (Emergency and Release)', items: ['Front Choke · Rear Choke (Two Hands) · Rear Forearm Choke', 'Double Lapel Grab', 'Same, Cross, and Two Hand Wrist Grabs (6)'] },
    { category: 'One-Step Sparring', items: ['Front Kick Defense'] },
    { category: 'Traditional Form Movements', items: ['Double Knife Hand Block (Advancing)', 'Stepping Punch (Advancing)', 'Under Middle Block, Shift-Punch, Rising Block (Once)'] },
    { category: 'Form', items: ['Palgue 7'] },
    { category: 'Kick-Punch Combination', items: ['Double Round Kick, Double Punch'] },
    { category: 'Dojo habits', items: ['Do not use it the wrong way.', 'Be respectful.', 'Do your best.'] }
  ] }
};

export function nextRank(current: string | null | undefined) {
  if (!current) return 'White Belt';
  const index = beltOrder.findIndex((rank) => rank.toLowerCase() === current.toLowerCase());
  return index < 0 ? 'White Belt' : beltOrder[Math.min(index + 1, beltOrder.length - 1)];
}
