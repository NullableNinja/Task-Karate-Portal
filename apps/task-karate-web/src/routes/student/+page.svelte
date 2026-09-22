<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  let session: StudentSession | null = null; let profile: any = null; let nextClass: any = null; let error = ''; let loading = true;
  onMount(async () => { session = await requireStudent(); if (!session) return; try { [profile, nextClass] = await Promise.all([api('/api/student/profile'), api<any[]>('/api/student/public/schedule').then((items) => items.find((item) => !item.cancelled) ?? null)]); } catch (e) { error = apiError(e); } finally { loading = false; } });
  $: month = new Date().toLocaleString(undefined, { month: 'long' });
  $: stripePercent = profile?.classesPerStripe ? Math.min(100, (profile.classesIntoStripe / profile.classesPerStripe) * 100) : 0;
</script>

<svelte:head><title>Task Karate | Status dashboard</title></svelte:head>

{#if session}
  <StudentShell {session} active="status">
    <section class="student-heading"><span class="student-eyebrow">STUDENT HUB / TODAY</span><h1>STATUS DASHBOARD</h1><p>{session.displayName} · {profile?.rankName ?? 'Rank in progress'}. A clear view of your training day.</p></section>
    {#if loading}<div class="student-panel">Loading your training record…</div>{:else if error}<div class="student-panel error">{error}</div>{:else}
      <div class="metric-grid dark-metrics"><article><span>CLASSES THIS MONTH</span><strong>{profile?.classesThisMonth ?? 0}</strong><small>{month}</small></article><article><span>TOTAL CLASSES</span><strong>{profile?.totalClasses ?? 0}</strong><small>attendance records</small></article><article><span>UNREAD MESSAGES</span><strong>{profile?.unreadMessages ?? 0}</strong><small>secure conversations</small></article><article><span>ACHIEVEMENTS</span><strong>{profile?.achievementCount ?? 0}</strong><small>earned milestones</small></article></div>
      <div class="student-grid two-column"><section class="student-panel stripe-panel"><div class="panel-title"><span>STRIPE PROGRESS</span><a href="/student/profile">View profile →</a></div><div class="rank-display"><span class="rank-orb">{profile?.rankName?.slice(0, 1) ?? 'TK'}</span><div><h2>{profile?.rankName ?? 'White Belt'}</h2><p>{profile?.nextMilestone ?? 'Keep showing up and sharpen the fundamentals.'}</p></div></div>{#if profile?.classesPerStripe}<div class="stripe-track" aria-label={`${profile.classesIntoStripe} of ${profile.classesPerStripe} classes toward next stripe`}><span style={`width:${stripePercent}%`}></span></div><div class="stripe-summary"><strong>{profile.classesToNextStripe} {profile.classesToNextStripe === 1 ? 'class' : 'classes'} until next stripe</strong><span>{profile.classesIntoStripe} of {profile.classesPerStripe} completed</span></div>{:else}<div class="degree-note"><strong>Black belt degrees are instructor tracked.</strong><span>Degree timing begins at 2 years for second degree, then adds a year for each successive degree.</span></div>{/if}</section><section class="student-panel"><div class="panel-title"><span>PERSONAL RECORDS</span><a href="/schedule">View schedule →</a></div><div class="record-list"><div><span>Member since</span><strong>{profile?.joinDate ?? 'On file'}</strong></div><div><span>Attendance streak</span><strong>{profile?.attendanceDates?.length ?? 0} recent</strong></div><div><span>Next action</span><strong>Check in to class</strong></div></div></section></div>
      <section class="student-panel next-class-panel"><div class="panel-title"><span>NEXT CLASS</span><a href="/schedule">Open schedule →</a></div>{#if nextClass}<div class="next-class-content"><div><span class="student-eyebrow">{new Date(nextClass.sessionDate).toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric' })}</span><h2>{nextClass.className}</h2><p>{nextClass.startTime} – {nextClass.endTime} · {nextClass.location ?? 'Main dojo'}</p></div><a class="primary-button" href="/schedule">Check in</a></div>{:else}<p class="muted">No upcoming class is published yet.</p>{/if}</section><section class="student-panel"><div class="panel-title"><span>RECENT ATTENDANCE</span><a href="/schedule">Check in →</a></div><div class="attendance-dots">{#each profile?.attendanceDates ?? [] as date}<span title={date} class="present-dot"></span>{/each}{#if (profile?.attendanceDates ?? []).length === 0}<p class="muted">Your class check-ins will appear here.</p>{/if}</div></section>
    {/if}
  </StudentShell>
{/if}
