<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';

  let session: StudentSession | null = null;
  let profile: any = null;
  let classes: any[] = [];
  let error = '';
  let loading = true;
  let checkingIn: number | null = null;
  let checkedIn = new Set<string>();

  function dateKey(value: string) { return value.slice(0, 10); }
  function dateLabel(value: string) { return new Date(`${dateKey(value)}T12:00:00`).toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric' }); }
  function isAttended(item: any) { return checkedIn.has(dateKey(item.sessionDate)) || (profile?.attendanceDates ?? []).includes(dateKey(item.sessionDate)); }
  async function checkIn(item: any) {
    checkingIn = item.sessionId; error = '';
    try { await api(`/api/student/schedule/${item.sessionId}/check-in`, { method: 'POST' }); checkedIn = new Set([...checkedIn, dateKey(item.sessionDate)]); }
    catch (e) { error = apiError(e); }
    finally { checkingIn = null; }
  }
  onMount(async () => {
    session = await requireStudent(); if (!session) return;
    try {
      const today = new Date();
      const from = today.toISOString().slice(0, 10);
      const to = new Date(today.getTime() + 1000 * 60 * 60 * 24 * 21).toISOString().slice(0, 10);
      [profile, classes] = await Promise.all([api('/api/student/profile'), api<any[]>(`/api/student/public/schedule?from=${from}&to=${to}`)]);
    } catch (e) { error = apiError(e); }
    finally { loading = false; }
  });
</script>

<svelte:head><title>Task Karate | My classes</title></svelte:head>

{#if session}
  <StudentShell {session} active="classes">
    <section class="student-heading"><span class="student-eyebrow">YOUR DOJO CALENDAR</span><h1>MY CLASSES</h1><p>See the next three weeks, choose the class you are attending, and check in from one clear place.</p></section>
    {#if error}<div class="error" role="alert">{error}</div>{/if}
    {#if loading}<div class="student-panel loading-panel">Loading your classes…</div>{:else if classes.length === 0}<section class="student-panel empty-panel"><span class="rank-orb">▣</span><h2>No upcoming classes published</h2><p>Ask staff to publish the current schedule, then your next class will appear here.</p></section>{:else}
      <section class="student-panel classes-hero"><div><span class="student-eyebrow">NEXT STEP</span><h2>Choose your class, then check in.</h2><p>Check-in is recorded against the real class session and cannot be duplicated.</p></div><a class="outline-button" href="/schedule">View full schedule</a></section>
      <div class="class-list" aria-label="Upcoming classes">{#each classes as item}<article class="student-panel class-session-card"><div class="class-session-date"><strong>{new Date(item.sessionDate).getDate()}</strong><span>{new Date(item.sessionDate).toLocaleDateString(undefined, { month: 'short' })}</span></div><div class="class-session-details"><span class="student-eyebrow">{dateLabel(item.sessionDate)}</span><h2>{item.className}</h2><p>{item.startTime ?? 'Time to be announced'}{item.endTime ? ` – ${item.endTime}` : ''} · {item.location ?? 'Main dojo'}</p>{#if item.description}<small>{item.description}</small>{/if}</div><div class="class-session-action">{#if isAttended(item)}<span class="checked-in-badge">✓ Checked in</span>{:else}<button class="primary-button" type="button" disabled={checkingIn === item.sessionId} on:click={() => checkIn(item)}>{checkingIn === item.sessionId ? 'Checking in…' : 'Check in'}</button>{/if}</div></article>{/each}</div>
    {/if}
  </StudentShell>
{/if}
