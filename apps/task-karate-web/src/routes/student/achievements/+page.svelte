<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import AchievementDetailModal from '$lib/components/AchievementDetailModal.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  import { triggerEasterEgg } from '$lib/easter-eggs';
  import { HIDDEN_DOJO_EGGS, EASTER_EGGS } from '$lib/easter-eggs';

  let session: StudentSession | null = null;
  let profile: any = null;
  let items: any[] = [];
  let goldStars: any[] = [];
  let dojoCheckIn: any = null;
  let leaderboard: any = null;
  let leaderboardPeriod: 'month' | 'year' = 'month';
  let leaderboardLoading = false;
  let hiYahClicks = 0;
  let hiYahLastClick = 0;
  let error = '';
  let selectedAchievement: any = null;
  const milestoneSteps = [
    { target: 1, label: 'First class', tier: 'bronze', icon: '🥉' },
    { target: 10, label: '10 classes', tier: 'silver', icon: '🥈' },
    { target: 100, label: '100 classes', tier: 'gold', icon: '🥇' },
    { target: 1000, label: '1,000 classes', tier: 'platinum', icon: '💠' },
    { target: 10000, label: '10,000 classes', tier: 'diamond', icon: '💎' }
  ];
  const checkInMilestones = [
    { target: 1, label: 'First studio check-in', tier: 'bronze', icon: '🥋' },
    { target: 5, label: '5 studio check-ins', tier: 'silver', icon: '⚡' },
    { target: 25, label: '25 studio check-ins', tier: 'gold', icon: '🔥' },
    { target: 100, label: '100 studio check-ins', tier: 'platinum', icon: '💠' }
  ];
  function unlockedSteps(steps: { target: number }[], current: number) { return steps.filter((step, index) => index === 0 || current >= steps[index - 1].target); }
  function achievementIcon(iconName?: string) {
    const icons: Record<string, string> = {
      anniversary: '✦', star: '★', leadership: '↗', is3: 'IS3',
      ten: '10×', hundred: '100×', thousand: '1K', 'ten-thousand': '10K',
      'helper-one': '🤝', 'helper-ten': '10×', 'helper-hundred': '100×', 'helper-thousand': '1K', 'helper-ten-thousand': '10K',
      'dojo-checkin-one': 'HI', 'dojo-checkin-five': '5×', 'dojo-checkin-twenty-five': '25×', 'dojo-checkin-hundred': '100×',
      calendar: '◷', 'calendar-star': '✦', flame: '🔥', compass: '◎', bell: '🔔', belt: '🥋', scroll: '📜', kata: '🎮', speaker: '📣', sticker: '✨', card: '🃏', wisdom: '🧠', 'hidden-dojo': '🌟'
    };
    return icons[String(iconName ?? '').toLowerCase()] ?? '★';
  }
  function clickHiYahHeading() { const now = Date.now(); if (now - hiYahLastClick > 1800) hiYahClicks = 0; hiYahLastClick = now; hiYahClicks += 1; if (hiYahClicks >= 3) { hiYahClicks = 0; triggerEasterEgg('egg-volume-control'); } }
  function openHiddenAchievement(egg: any) { const discovered = earnedEggNames.has(egg.name); selectedAchievement = { name: discovered ? egg.name : 'Undiscovered dojo secret', description: discovered ? egg.description : egg.clue, iconName: discovered ? egg.icon : '?' }; }
  async function loadLeaderboard() {
    leaderboardLoading = true;
    try { leaderboard = await api(`/api/student/dojo-check-in/leaderboard?period=${leaderboardPeriod}`); }
    catch { leaderboard = null; }
    finally { leaderboardLoading = false; }
  }
  let progressMilestones: any[] = [];
  $: progressMilestones = [
    ...unlockedSteps(milestoneSteps, profile?.totalClasses ?? 0).map((step) => ({ ...step, group: 'Training attendance', current: profile?.totalClasses ?? 0 })),
    ...unlockedSteps(milestoneSteps, profile?.helperClasses ?? 0).map((step) => ({
      ...step,
      group: 'Helping the dojo',
      current: profile?.helperClasses ?? 0,
      label: step.target === 1 ? 'First helping class' : `${step.target.toLocaleString()} helping classes`
    })),
    ...unlockedSteps(checkInMilestones, dojoCheckIn?.totalCheckIns ?? 0).map((step) => ({ ...step, group: 'Studio check-ins', current: dojoCheckIn?.totalCheckIns ?? 0 }))
  ].map((milestone) => ({ ...milestone, percent: Math.min(100, Math.round((milestone.current / milestone.target) * 100)), complete: milestone.current >= milestone.target }));
  $: earnedEggNames = new Set(items.map((item) => item.name));
  $: regularItems = items.filter((item) => !EASTER_EGGS.some((egg) => egg.name === item.name));
  $: discoveredEggCount = HIDDEN_DOJO_EGGS.filter((egg) => earnedEggNames.has(egg.name)).length;
  $: collectorEarned = earnedEggNames.has(EASTER_EGGS[EASTER_EGGS.length - 1].name);

  onMount(async () => {
    session = await requireStudent();
    if (!session) return;
    try {
      [profile, items, goldStars, dojoCheckIn] = await Promise.all([
        api('/api/student/profile'),
        api<any[]>('/api/student/achievements'),
        api<any[]>('/api/student/gold-stars'),
        api('/api/student/dojo-check-in')
      ]);
      await loadLeaderboard();
    }
    catch (e) { error = apiError(e); }
  });
</script>

{#if session}
  <StudentShell {session} active="achievements">
    <section class="student-heading"><span class="student-eyebrow">MILESTONES & GOLD STARS</span><h1>ACHIEVEMENTS</h1><p>Your earned milestones and dojo-recognized moments, all in one place.</p></section>
    {#if error}<div class="error">{error}</div>{:else}
      <section class="student-panel milestone-board">
        <div class="panel-title"><span>YOUR MILESTONE PATH</span><span class="muted">Progress updates automatically</span></div>
        <p class="muted">Every class and every time you help the dojo moves a separate path forward. Earned milestones are awarded automatically and posted to the dojo news feed.</p>
        <div class="milestone-grid">
          {#each progressMilestones as milestone}
            <button class:complete={milestone.complete} class={`milestone-progress-card ${milestone.tier}`} type="button" on:click={() => selectedAchievement = milestone}>
              <span class="milestone-medallion" aria-hidden="true">{milestone.icon}</span>
              <div class="milestone-progress-copy"><div class="milestone-progress-heading"><span>{milestone.group}</span><strong>{milestone.label}</strong></div><div class="milestone-progress-track" aria-label={`${milestone.current} of ${milestone.target}`}><span style={`width:${milestone.percent}%`}></span></div><small>{milestone.complete ? 'Milestone earned · keep building.' : `${milestone.target - milestone.current} more to unlock this milestone.`}</small></div>
            </button>
          {/each}
        </div>
      </section>
      <section class="student-panel checkin-achievement-panel">
        <div class="panel-title"><button class="easter-egg-heading-trigger" type="button" on:click={clickHiYahHeading}>HIYAH! CHECK-IN RACE</button><a href="/student/social">Check in on Social →</a></div>
        <div class="checkin-achievement-summary"><div><strong>{dojoCheckIn?.totalCheckIns ?? 0}</strong><span>lifetime studio check-ins</span></div><div><strong>{dojoCheckIn?.checkInsThisMonth ?? 0}</strong><span>this month</span></div><div><strong>{dojoCheckIn?.checkedInToday ? 'Done' : 'Ready'}</strong><span>{dojoCheckIn?.checkedInToday ? 'today’s HIYAH!' : 'one check-in per day'}</span></div></div>
        <div class="checkin-leaderboard-heading"><span>TOP STUDENTS</span><div class="social-leaderboard-tabs" role="tablist" aria-label="Check-in leaderboard period"><button class:active={leaderboardPeriod === 'month'} type="button" role="tab" aria-selected={leaderboardPeriod === 'month'} on:click={() => { leaderboardPeriod = 'month'; loadLeaderboard(); }}>This month</button><button class:active={leaderboardPeriod === 'year'} type="button" role="tab" aria-selected={leaderboardPeriod === 'year'} on:click={() => { leaderboardPeriod = 'year'; loadLeaderboard(); }}>This year</button></div></div>
        {#if leaderboardLoading}<p class="muted">Loading the dojo race…</p>{:else if leaderboard?.entries?.length}<div class="achievement-leaderboard">{#each leaderboard.entries.slice(0, 5) as entry}<div class:current={entry.isCurrentStudent} class="achievement-leaderboard-row"><span>{entry.rank}</span><strong>{entry.displayName}</strong><small>{entry.checkIns} {entry.checkIns === 1 ? 'check-in' : 'check-ins'}</small></div>{/each}</div>{:else}<p class="muted">Be the first student on the board. Press HIYAH! on Social to start the race.</p>{/if}
      </section>
      <section class="student-panel gold-star-panel"><div class="panel-title"><span>GOLD STARS YOU EARNED</span><span class="muted">Recorded by the dojo</span></div><p class="muted">Gold Stars are awarded by staff after participation is confirmed. Upcoming events and interest lists stay in the staff workspace.</p>{#if goldStars.length}<div class="gold-star-grid">{#each goldStars as event}<a href={`/student/achievements/${event.eventId}`} class="gold-star-card awarded"><span class="gold-star-icon">★</span><div><h2>{event.name}</h2><p>{event.description}</p><small>Gold star earned · View details →</small></div></a>{/each}</div>{:else}<p class="muted">Your earned Gold Stars will appear here after staff records them.</p>{/if}</section>
      <section class="student-panel hidden-dojo-panel"><div class="panel-title"><span>HIDDEN DOJO COLLECTION</span><span class="muted">{discoveredEggCount} / {HIDDEN_DOJO_EGGS.length} discovered</span></div><p class="muted">The Hub has secrets. Every silhouette is a clue waiting to be solved.</p><div class="hidden-dojo-grid">{#each HIDDEN_DOJO_EGGS as egg}<button class:complete={earnedEggNames.has(egg.name)} class="hidden-dojo-card" type="button" on:click={() => openHiddenAchievement(egg)}><span class="hidden-dojo-icon" aria-hidden="true">{earnedEggNames.has(egg.name) ? egg.icon : '?'}</span><span><strong>{earnedEggNames.has(egg.name) ? egg.name : 'Undiscovered dojo secret'}</strong><span>{earnedEggNames.has(egg.name) ? egg.description : egg.clue}</span>{#if earnedEggNames.has(egg.name)}<small>Achievement earned</small>{/if}</span></button>{/each}</div>{#if collectorEarned}<div class="hidden-dojo-collector"><span>🌟</span><div><strong>Master of Hidden Dojos</strong><p>You found every hidden Student Hub easter egg. The dojo has no more secrets… for now.</p></div></div>{/if}</section>
      {#if regularItems.length}<section class="student-panel earned-achievements-panel"><div class="panel-title"><span>OTHER EARNED ACHIEVEMENTS</span><span class="muted">{regularItems.length} recorded</span></div><p class="muted">Select an achievement to see its story and earned date.</p><div class="achievement-grid">{#each regularItems as item}<button class="achievement-card achievement-card-button" type="button" on:click={() => selectedAchievement = item}><span class="achievement-icon" aria-hidden="true">{achievementIcon(item.iconName)}</span><span class="achievement-card-copy"><strong>{item.name}</strong><span>{item.description ?? 'Dojo milestone earned.'}</span><small>{item.awardedAt ? new Date(item.awardedAt).toLocaleDateString() : ''}</small></span></button>{/each}</div></section>{:else}<section class="student-panel empty-panel"><span class="rank-orb">★</span><h2>Your next milestone is ahead.</h2><p>Keep training. Your progress cards above show exactly what is next.</p></section>{/if}
      <AchievementDetailModal achievement={selectedAchievement} onClose={() => selectedAchievement = null} />
    {/if}
  </StudentShell>
{/if}
