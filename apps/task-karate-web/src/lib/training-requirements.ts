export type TrainingSection = { category: string; items: string[] };
export type TrainingPlan = { rank: string; color: string; promotionGuide: string; sections: TrainingSection[] };

export const beltOrder = ['White Belt', 'Gold Belt', 'Orange Belt', 'Green Belt', 'Purple Belt', 'Blue Belt', 'Red Belt', 'Brown Belt', 'Black Belt'];

export const trainingPlans: Record<string, TrainingPlan> = {
  'White Belt': { rank: 'White Belt', color: '#e7edf0', promotionGuide: 'Eligible to test after the second lesson.', sections: [
    { category: 'The three rules', items: ['Do not use it the wrong way.', 'Be respectful.', 'Do your best.'] },
    { category: 'Basic stances', items: ['Attention stance', 'Chun Bi (ready stance)', 'Fighting stance'] },
    { category: 'Reflection', items: ['Bring an example of how you used the second or third rule in daily life.', 'Set a goal to keep practicing the three basic rules.'] }
  ] },
  'Gold Belt': { rank: 'Gold Belt', color: '#d8ad42', promotionGuide: 'Projected guide: 12–24 lessons and 2–3 months of attendance.', sections: [
    { category: 'Stance and movement', items: ['Fighting stance with switching', 'Advancing and retreating', 'Step-slide movement (fencing)'] },
    { category: 'Striking and kicking', items: ['Rear-hand punch', 'Back-leg snap front kick'] },
    { category: 'Self-defense', items: ['Front choke: step back, swing the arm over, and elbow'] },
    { category: 'Traditional movement', items: ['Front stance advancing and retreating', 'Front stance stepping punch'] },
    { category: 'Rotating focus', items: ['Defense against a straight punch, haymaker, or backfist'] }
  ] },
  'Orange Belt': { rank: 'Orange Belt', color: '#e87832', promotionGuide: 'Projected guide: 16–24 lessons and 2–3 months of attendance.', sections: [
    { category: 'Stance and movement', items: ['Fighting stance with switching', 'Advancing and retreating', 'Step-slide movement (fencing)', 'Back stance advancing and retreating'] },
    { category: 'Striking and kicking', items: ['Jab and rear-hand punch', 'Back-leg snap front kick', 'Front-leg round kick', 'Front-leg round kick and rear-hand punch'] },
    { category: 'Self-defense', items: ['Front choke release', 'Double lapel release'] },
    { category: 'Forms', items: ['Front stance stepping punch', 'First half of Form #1; bonus: all of Form #1, #2, or #3'] },
    { category: 'Rotating focus', items: ['Defense against a straight punch, haymaker, or backfist'] }
  ] },
  'Green Belt': { rank: 'Green Belt', color: '#42a879', promotionGuide: 'Projected guide: 24–40 lessons and 3–5 months of attendance.', sections: [
    { category: 'Stance and movement', items: ['Fighting stance with switching', 'Advancing and retreating', 'Step-slide movement (fencing)'] },
    { category: 'Striking and kicking', items: ['Jab and rear-hand punch', 'Double punch', 'Back-leg snap front kick', 'Front-leg round kick', 'Step-slide side kick'] },
    { category: 'Self-defense', items: ['Front choke release', 'Double lapel grab release', 'Same-side, cross-side, and two-to-one wrist grabs'] },
    { category: 'Traditional movement and forms', items: ['Front stance stepping punch', 'Front stance down block', 'Form #1', 'Bonus form: Form #2, #3, Kan-na, or student choice'] },
    { category: 'Combination', items: ['Front-leg round kick and double punch', 'Defense against a straight punch, haymaker, or backfist'] }
  ] },
  'Purple Belt': { rank: 'Purple Belt', color: '#9569c3', promotionGuide: 'Projected guide: 32–56 lessons and 4–7 months of attendance.', sections: [
    { category: 'Stances and strikes', items: ['Chun Bi, front stance, and back stance', 'Rear-hand punch, double punch, and rear-hand ridge hand'] },
    { category: 'Kicking', items: ['Front-leg snap front kick', 'Rear-leg crescent kick', 'Front-leg round kick', 'Front-leg side kick', 'Double round kick'] },
    { category: 'Self-defense', items: ['Front choke', 'Rear two-hand choke', 'Rear forearm choke', 'Double lapel grab', 'Same-side, cross-side, and two-hand wrist grabs'] },
    { category: 'One-step and forms', items: ['Front-kick defense', 'Advancing double knife-hand block', 'Advancing stepping punch', 'Under-middle block, shift-punch, rising block', 'Palgue 7'] },
    { category: 'Combination', items: ['Double round kick and double punch'] }
  ] },
  'Blue Belt': { rank: 'Blue Belt', color: '#4f9ee8', promotionGuide: 'Projected guide: 40–64 lessons and 5–8 months of attendance.', sections: [
    { category: 'Stances and strikes', items: ['Fighting stance, Chun Bi, front stance, and back stance', 'Rear-hand punch and double punch with front ridge hand'] },
    { category: 'Kicking', items: ['Front-leg snap front kick', 'Rear-leg crescent kick', 'Front-leg round, side, and hook kicks', 'Hook kick and round kick combination'] },
    { category: 'Self-defense', items: ['Front choke', 'Rear two-hand choke', 'Rear forearm choke', 'Double lapel and single lapel grabs', 'Same-side, cross-side, and two-hand wrist grabs'] },
    { category: 'One-step and forms', items: ['Front-kick defense', 'Advancing double knife-hand block', 'Advancing stepping punch', 'Under-middle block, shift-punch, rising block, back-leg side kick', 'Palgue 7'] },
    { category: 'Combination and sparring', items: ['Double round kick, double punch, back-leg round kick', 'Two rounds of sparring'] }
  ] },
  'Red Belt': { rank: 'Red Belt', color: '#d85a63', promotionGuide: 'Projected guide: 50–72 lessons and 6–9 months of attendance.', sections: [
    { category: 'Striking and kicking', items: ['Rear-hand punch, double punch, jab, and uppercut', 'Front-leg snap, round, side, and hook kicks', 'Rear-leg crescent and round kicks', 'Front-leg side-to-round and back-leg side-to-turning-side combinations'] },
    { category: 'Self-defense', items: ['Two-hand front and rear chokes', 'Rear forearm and side headlock', 'Same-side and cross-side wrist grabs', 'Two-to-one wrist grab', 'Single, double, and side-shoulder shirt grabs'] },
    { category: 'One-step and forms', items: ['Inside block, grab, and elbow', 'Outside round kick, grab, knee, and double elbow', 'Front stance stepping punch', 'Double knife-hand block, shift, and punch', 'Thrust front kick, front stance, punch', 'Chung-Mu'] },
    { category: 'X-rays and combination', items: ['Spinning crescent', 'Spinning hook', 'Double round kick, double punch, back-leg round kick, front-hand ridge hand'] },
    { category: 'Sparring', items: ['Three rounds of sparring'] }
  ] },
  'Brown Belt': { rank: 'Brown Belt', color: '#9c6a45', promotionGuide: 'Projected guide: 50+ lessons and 6–12 months of attendance.', sections: [
    { category: 'Striking and kicking', items: ['Rear-hand punch, double punch, hook punch', 'Back-fist, spin back-fist, rear punch', 'Front-leg snap, hook, round, and side kicks', 'Back-leg round and side kicks', 'Rear-leg crescent kick', 'Triple round kick and back-leg side-to-turning-side kick'] },
    { category: 'Self-defense', items: ['Two-hand front and rear chokes', 'Rear forearm and side headlock', 'Same-side, cross-side, two-to-one, and two-to-two wrist grabs', 'Single lapel, double lapel, and side-shoulder shirt grabs'] },
    { category: 'One-step and forms', items: ['Outside-in block, rear elbow, rear knee', 'Inside parry, grab, and strike', 'Double knife hand, shift, and punch', 'Back-leg front kick and punch', 'Front stance stepping punch', 'Front stance down block', 'Form #1 and Chung-Mu'] },
    { category: 'X-rays, pads, and sparring', items: ['Spinning crescent, spinning hook, tornado, pop-up round kick', 'Back-leg front, defensive side, turning side, and round kicks on pads', 'Four rounds of sparring'] }
  ] },
  'Black Belt': { rank: 'Black Belt', color: '#aeb8c5', promotionGuide: 'Black belt degrees are instructor tracked. Second degree begins at 2 years; each later degree adds one year.', sections: [
    { category: 'Advanced curriculum', items: ['Punching combinations and blitz work', 'Advanced kicking combinations and x-rays', 'Chokes, wrist grabs, shirt grabs, and holds', 'One-step sparring and Kadena De Mano cross-block drill', 'Traditional movements and advanced forms', 'Pads, power combinations, board breaking, and sparring'] },
    { category: 'Black belt path', items: ['First degree: continue building depth, teaching ability, and consistency.', 'Second degree: minimum two-year development window.', 'Third degree: minimum three-year development window.', 'Fourth degree and beyond: minimum window increases by one year per degree.'] }
  ] }
};

export const kidsTrainingPlans: Record<string, TrainingPlan> = {
  'Purple Belt': { rank: 'Purple Belt', color: '#9569c3', promotionGuide: 'Kids track · show control, confidence, and a strong understanding of the dojo rules before testing.', sections: [
    { category: 'Ready to learn', items: ['Arrive ready, listen for the full instruction, and demonstrate a respectful attitude.', 'Use safe control with a partner and stop immediately when asked.'] },
    { category: 'Core karate', items: ['Chun Bi, front stance, back stance, and fighting stance', 'Front-leg snap front kick, front-leg round kick, and front-leg side kick', 'Rear-hand punch, double punch, and ridge-hand motion'] },
    { category: 'Self-defense and forms', items: ['Front choke response with safe distance and a clear voice', 'Wrist-grab releases with balance and awareness', 'Palgue 7 opening sequence and one-step front-kick defense'] },
    { category: 'Kids confidence challenge', items: ['Explain one way karate helps you make a good choice outside class.', 'Complete the combination with control, focus, and a strong finish.'] }
  ] }
};

export function nextRank(current: string | null | undefined) {
  if (!current) return 'White Belt';
  const index = beltOrder.findIndex((rank) => rank.toLowerCase() === current.toLowerCase());
  return index < 0 ? 'White Belt' : beltOrder[Math.min(index + 1, beltOrder.length - 1)];
}
