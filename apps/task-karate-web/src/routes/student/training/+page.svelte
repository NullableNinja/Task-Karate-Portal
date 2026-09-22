<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  import { kidsTrainingPlans, nextRank, trainingPlans } from '$lib/training-requirements';
  import { is3LevelGuides } from '$lib/is3-levels';

  let session: StudentSession | null = null;
  let profile: any = null;
  let dailyMissions: any[] = [];
  let practiceLogs: any[] = [];
  let goals: any[] = [];
  let error = '';
  let loading = true;
  let loadingDaily = false;
  let requestedTrack = 'karate';
  let activeTrackKey = '';
  let missionDate = '';
  let practiceSkill = '';
  let practiceMinutes = 10;
  let practiceReflection = '';
  let goalTitle = '';
  let goalDate = '';

  function localDateKey(date = new Date()) { return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`; }
  function dayOfYear(date = new Date()) { const start = new Date(date.getFullYear(), 0, 0); return Math.floor((date.getTime() - start.getTime()) / 86400000); }
  function missionDescription(item: string) { return `Practice this requirement with control, then ask your instructor for one piece of feedback: ${item}`; }

  onMount(async () => {
    session = await requireStudent(); if (!session) return;
    requestedTrack = new URL(window.location.href).searchParams.get('track') === 'is3' ? 'is3' : 'karate';
    missionDate = localDateKey();
    try { [profile, practiceLogs, goals] = await Promise.all([api('/api/student/profile'), api<any[]>('/api/student/practice'), api<any[]>('/api/student/goals')]); }
    catch (e) { error = apiError(e); }
    finally { loading = false; }
  });

  $: is3Program = profile?.programs?.find((program: any) => program.programCode === 'is3');
  $: activeTrack = requestedTrack === 'is3' && is3Program ? 'is3' : 'karate';
  $: nextRankName = nextRank(profile?.rankName);
  $: karatePlan = (profile?.ageGroup === 'kids' ? kidsTrainingPlans[nextRankName] : undefined) ?? trainingPlans[nextRankName] ?? trainingPlans['White Belt'];
  $: is3Guide = is3LevelGuides[is3Program?.levelName ?? 'IS3 Student Level 0'] ?? is3LevelGuides['IS3 Student Level 0'];
  $: sections = activeTrack === 'is3' ? is3Guide.sections : karatePlan.sections;
  $: trackLabel = activeTrack === 'is3' ? 'IS3 level progression' : profile?.ageGroup === 'kids' ? 'Kids Karate' : 'Teens & Adults Karate';
  $: stripePercent = profile?.classesPerStripe ? Math.min(100, (profile.classesIntoStripe / profile.classesPerStripe) * 100) : 0;
  $: requirementItems = sections.flatMap((section: any) => section.items.map((item: string) => ({ category: section.category, item })));
  $: dailySeeds = requirementItems.length && missionDate ? Array.from({ length: Math.min(3, requirementItems.length) }, (_, offset) => { const selected = requirementItems[(dayOfYear(new Date(`${missionDate}T12:00:00`)) + offset) % requirementItems.length]; return { missionKey: `${activeTrack}:${activeTrack === 'is3' ? is3Guide.current : nextRankName}:${selected.category}:${selected.item}`, title: selected.item, description: missionDescription(selected.item), category: `${activeTrack === 'is3' ? 'IS3 level' : 'Next belt'} · ${selected.category}` }; }) : [];
  $: nextMissionKey = `${missionDate}:${activeTrack}:${dailySeeds.map((mission) => mission.missionKey).join('|')}`;
  $: if (session && profile && missionDate && dailySeeds.length && activeTrackKey !== nextMissionKey) { activeTrackKey = nextMissionKey; syncDailyMissions(); }

  async function syncDailyMissions() { loadingDaily = true; try { dailyMissions = await api<any[]>('/api/student/daily-missions/sync', { method: 'POST', body: JSON.stringify({ date: missionDate, missions: dailySeeds }) }); } catch (e) { error = apiError(e); } finally { loadingDaily = false; } }
  async function toggleDailyMission(missionId: number) { try { await api(`/api/student/daily-missions/${missionId}/toggle?date=${missionDate}`, { method: 'POST' }); dailyMissions = await api<any[]>(`/api/student/daily-missions?date=${missionDate}`); } catch (e) { error = apiError(e); } }
  async function logPractice() { if (!practiceSkill.trim()) return; try { await api('/api/student/practice', { method: 'POST', body: JSON.stringify({ skill: practiceSkill, minutes: Number(practiceMinutes), reflection: practiceReflection || null }) }); practiceSkill = ''; practiceMinutes = 10; practiceReflection = ''; practiceLogs = await api<any[]>('/api/student/practice'); } catch (e) { error = apiError(e); } }
  async function addGoal() { if (!goalTitle.trim()) return; try { await api('/api/student/goals', { method: 'POST', body: JSON.stringify({ title: goalTitle, targetDate: goalDate || null }) }); goalTitle = ''; goalDate = ''; goals = await api<any[]>('/api/student/goals'); } catch (e) { error = apiError(e); } }
  async function toggleGoal(goalId: number) { try { await api(`/api/student/goals/${goalId}/toggle`, { method: 'POST' }); goals = await api<any[]>('/api/student/goals'); } catch (e) { error = apiError(e); } }
</script>

<svelte:head><title>Task Karate | Training requirements</title></svelte:head>

{#if session}
  <StudentShell {session} active="training">
    <section class="student-heading"><span class="student-eyebrow">YOUR REQUIREMENTS · {trackLabel}</span><h1>TRAINING</h1><p>Your active training track, next milestone, complete requirements, and today’s practice mission are all together here.</p></section>
    {#if error}<div class="error" role="alert">{error}</div>{/if}
    {#if loading}<div class="student-panel loading-panel">Loading your requirements…</div>{:else}
      <nav class="training-track-nav training-program-nav" aria-label="Training requirement tracks"><a class:active={activeTrack === 'karate'} href="/student/training?track=karate">Karate requirements</a>{#if is3Program}<a class:active={activeTrack === 'is3'} href="/student/training?track=is3">IS3 requirements</a>{/if}</nav>
      {#if activeTrack === 'karate'}
        <div class="student-grid two-column training-top-grid"><section class="student-panel training-hero"><span class="belt-graphic" style={`--belt-color:${karatePlan.color}`}><span class="belt-knot"></span><span class="belt-strip"></span></span><span class="student-eyebrow">NEXT RANK · {trackLabel}</span><h2>{nextRankName}</h2><p>{karatePlan.promotionGuide}</p></section><section class="student-panel stripe-panel"><div class="panel-title"><span>STRIPE PROGRESS</span><a href="/student/profile">View profile →</a></div><div class="stripe-track" aria-label={`${profile.classesIntoStripe} of ${profile.classesPerStripe || 0} classes toward next stripe`}><span style={`width:${stripePercent}%`}></span></div><div class="stripe-summary"><strong>{profile.classesToNextStripe || 'Instructor tracked'} {profile.classesToNextStripe ? (profile.classesToNextStripe === 1 ? 'class' : 'classes') : ''} until next stripe</strong><span>{profile.classesPerStripe ? `${profile.classesIntoStripe} of ${profile.classesPerStripe} completed` : 'Black belt degrees are tracked by your instructor.'}</span></div></section></div>
      {:else}
        <section class="student-panel training-hero is3-track-hero"><div><span class="student-eyebrow">CURRENT LEVEL · IS3</span><h2>{is3Guide.current}</h2><p>{is3Guide.note}</p></div><div class="program-code large">IS3</div></section><section class="student-panel is3-panel training-next-panel"><div class="panel-title"><span>NEXT LEVEL</span><span class="muted">Instructor confirmed</span></div><h2>{is3Guide.next}</h2><p class="muted">Complete the requirements below with your instructor before moving forward.</p></section>
      {/if}
      <section class="student-panel requirement-intro"><div class="panel-title"><span>{activeTrack === 'is3' ? 'IS3 LEVEL REQUIREMENTS' : 'NEXT BELT TEST REQUIREMENTS'}</span><span class="muted">{requirementItems.length} items · instructor confirms readiness</span></div><p>Use this checklist to focus your practice. Marking a daily mission means you practiced it; it does not award a belt, stripe, or IS3 level.</p></section>
      <div class="requirement-grid">{#each sections as section}<section class="student-panel requirement-card"><div class="panel-title"><span>{section.category}</span><span class="requirement-count">{section.items.length}</span></div><ul>{#each section.items as item}<li><span class="requirement-marker" aria-hidden="true">○</span><span>{item}</span></li>{/each}</ul></section>{/each}</div>
      <section class="student-panel mission-panel daily-mission-panel"><div class="panel-title"><span>TODAY’S PRACTICE MISSIONS</span><span class="muted">{missionDate} · resets tomorrow</span></div><p class="muted daily-mission-intro">Three focused items are pulled from your current {activeTrack === 'is3' ? 'IS3 level' : 'belt-test'} requirements each day.</p>{#if loadingDaily}<p class="muted">Preparing today’s missions…</p>{:else}<div class="mission-grid">{#each dailyMissions as mission}<button class:completed={mission.completed} class="mission-card" type="button" on:click={() => toggleDailyMission(mission.dailyMissionId)}><span class="mission-check">{mission.completed ? '✓' : '○'}</span><span><small>{mission.category}</small><strong>{mission.title}</strong><em>{mission.completed ? 'Completed today · tap to reopen' : 'Tap when you have practiced this with focus.'}</em></span></button>{/each}</div>{/if}</section>
      <div class="training-tools-grid"><section class="student-panel practice-panel"><div class="panel-title"><span>PRACTICE LOG</span><span class="muted">Make the reps visible</span></div><form class="practice-form" on:submit|preventDefault={logPractice}><label>Skill or focus<input bind:value={practiceSkill} maxlength="100" placeholder="e.g. Roundhouse kick" /></label><label>Minutes<input type="number" min="1" max="240" bind:value={practiceMinutes} /></label><label class="full">Reflection<textarea bind:value={practiceReflection} maxlength="1000" rows="2" placeholder="What felt better today?"></textarea></label><button class="primary-button" type="submit" disabled={!practiceSkill.trim()}>Log practice</button></form>{#if practiceLogs.length}<div class="practice-log-list">{#each practiceLogs.slice(0, 3) as log}<div class="practice-log"><span class="practice-minutes">{log.minutes}<small>min</small></span><div><strong>{log.skill}</strong><p>{log.reflection ?? 'Practice recorded.'}</p><small>{new Date(log.loggedAt).toLocaleDateString()}</small></div></div>{/each}</div>{/if}</section><section class="student-panel goals-panel"><div class="panel-title"><span>MY GOALS</span><span class="muted">Small targets, big momentum</span></div><form class="goal-form" on:submit|preventDefault={addGoal}><input bind:value={goalTitle} maxlength="160" placeholder="Add a training goal…" aria-label="New training goal" /><input type="date" bind:value={goalDate} aria-label="Goal target date" /><button class="outline-button" type="submit" disabled={!goalTitle.trim()}>Add goal</button></form><div class="goal-list">{#each goals as goal}<button type="button" class:completed={goal.completed} class="goal-row" on:click={() => toggleGoal(goal.goalId)}><span>{goal.completed ? '✓' : '○'}</span><span><strong>{goal.title}</strong><small>{goal.completed ? 'Completed · tap to reopen' : goal.targetDate ? `Target ${new Date(goal.targetDate).toLocaleDateString()}` : 'No target date'}</small></span></button>{:else}<p class="muted">Set a goal for your next class.</p>{/each}</div></section></div>
    {/if}
  </StudentShell>
{/if}
