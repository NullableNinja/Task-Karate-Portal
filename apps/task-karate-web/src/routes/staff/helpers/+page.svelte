<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import { formatDojoRange } from '$lib/time';

  type StaffHelper = { staffUserId: string; displayName: string; signedUpAt: string; isCurrentStaff: boolean };
  type StudentHelper = { attendanceId: number; studentId: number; studentName: string; checkedInAtUtc: string; notes?: string; status: string };
  type HelperSession = { sessionId: number; sessionDate: string; startTime?: string; endTime?: string; className: string; location?: string; cancelled: boolean; staffHelpers: StaffHelper[]; studentHelpers: StudentHelper[] };
  type HelperDay = { date: string; sessions: HelperSession[] };

  let sessions: HelperSession[] = [];
  let loading = true;
  let error = '';
  let notice = '';
  let busySessionId: number | null = null;
  let printDate: string | null = null;
  let throughDate = new Date(Date.now() + 14 * 24 * 60 * 60 * 1000).toISOString().slice(0, 10);

  const today = new Date().toISOString().slice(0, 10);
  function dateKey(value: string) { return value.slice(0, 10); }
  function dateLabel(value: string) { return new Date(`${value}T12:00:00`).toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric' }); }
  function isToday(value: string) { return dateKey(value) === today; }

  async function load() {
    loading = true; error = ''; notice = '';
    try { sessions = await api<HelperSession[]>(`/api/portal-admin/helpers?from=${today}&to=${throughDate}`); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not load the helper roster.'; }
    finally { loading = false; }
  }

  $: days = Array.from(sessions.reduce((map, session) => {
    const key = dateKey(session.sessionDate);
    const current = map.get(key) ?? { date: key, sessions: [] };
    current.sessions.push(session); map.set(key, current); return map;
  }, new Map<string, HelperDay>()).values());
  $: volunteerCount = sessions.reduce((total, session) => total + session.staffHelpers.length, 0);
  $: coveredSessionCount = sessions.filter((session) => session.staffHelpers.length + session.studentHelpers.length > 0).length;
  $: openSessionCount = sessions.filter((session) => !session.cancelled && session.staffHelpers.length + session.studentHelpers.length === 0).length;

  function sessionCoverage(session: HelperSession) {
    if (session.cancelled) return { label: 'Cancelled', className: 'cancelled' };
    const count = session.staffHelpers.length + session.studentHelpers.length;
    return count > 0 ? { label: `${count} helper${count === 1 ? '' : 's'} listed`, className: 'covered' } : { label: 'Needs a helper', className: 'needs-helper' };
  }

  function printDay(date: string) {
    printDate = date;
    requestAnimationFrame(() => window.print());
    window.setTimeout(() => { printDate = null; }, 1000);
  }

  async function volunteer(session: HelperSession) {
    busySessionId = session.sessionId; error = ''; notice = '';
    try { await api(`/api/portal-admin/helpers/${session.sessionId}/signup`, { method: 'POST' }); notice = `You are volunteering for ${session.className}.`; await load(); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not save the helper signup.'; }
    finally { busySessionId = null; }
  }

  async function removeVolunteer(session: HelperSession) {
    busySessionId = session.sessionId; error = ''; notice = '';
    try { await api(`/api/portal-admin/helpers/${session.sessionId}/signup`, { method: 'DELETE' }); notice = `Your helper signup for ${session.className} was removed.`; await load(); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not remove the helper signup.'; }
    finally { busySessionId = null; }
  }

  onMount(load);
</script>

<svelte:head><title>Staff · Helper roster | Task Karate</title></svelte:head>

<div class="toolbar"><div><span class="eyebrow">HELPER ROSTER</span><h2>Volunteer coverage</h2><p class="muted">Every class is visible at a glance. Coverage is shown here so staff can spot open sessions without opening each class.</p></div><div class="helper-summary"><strong>{openSessionCount}</strong><span>sessions needing a helper</span><small>{coveredSessionCount} of {sessions.length} covered</small></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}

<section class="card helper-toolbar"><label>Show sessions through<input type="date" bind:value={throughDate} min={today} on:change={load} /></label><p class="muted">Volunteer signups are available for today and future sessions. This does not check anyone into attendance.</p><button class="button secondary" type="button" on:click={load} disabled={loading}>{loading ? 'Refreshing…' : 'Refresh roster'}</button></section>

{#if loading}<section class="card loading-card"><p class="muted">Loading helper coverage…</p></section>
{:else if !days.length}<section class="card empty-state"><h3>No upcoming sessions found</h3><p class="muted">Create or publish dated sessions in Classes before adding helper coverage.</p><a class="button" href="/staff/classes">Open class setup</a></section>
{:else}
  <div class="helper-days">
    {#each days as day}
      <section class:print-target={printDate === day.date} class="helper-day-group" data-print-day={day.date}>
        <div class="helper-print-heading">TASK KARATE · HELPER ROSTER · {dateLabel(day.date)}</div>
        <div class="helper-day-heading"><div><span class="eyebrow">{isToday(day.date) ? 'TODAY' : 'UPCOMING'}</span><h3>{dateLabel(day.date)}</h3></div><div class="helper-day-actions"><span class="pill">{day.sessions.length} session{day.sessions.length === 1 ? '' : 's'}</span><button class="button secondary small-button print-day-button" type="button" on:click={() => printDay(day.date)}>Print this day</button></div></div>
        <div class="helper-session-list">
          {#each day.sessions as session}
            {@const coverage = sessionCoverage(session)}
            <article class="card helper-session-card {coverage.className}">
              <div class="helper-session-heading"><div><span class="helper-session-time">{formatDojoRange(session.startTime, session.endTime)}</span><h3>{session.className}</h3><p class="muted">{session.location || 'Main dojo'}</p></div><div class="helper-session-action">{#if session.staffHelpers.some((helper) => helper.isCurrentStaff)}<button class="button secondary small-button" type="button" on:click={() => removeVolunteer(session)} disabled={busySessionId === session.sessionId}>{busySessionId === session.sessionId ? 'Saving…' : 'Remove my signup'}</button>{:else}<button class="button small-button" type="button" on:click={() => volunteer(session)} disabled={busySessionId === session.sessionId || session.cancelled}>{busySessionId === session.sessionId ? 'Saving…' : 'Volunteer for this class'}</button>{/if}</div></div>
              <div class="helper-coverage-banner"><span class="helper-coverage-dot" aria-hidden="true"></span><strong>{coverage.label}</strong>{#if coverage.className === 'needs-helper'}<span>Staff can volunteer below.</span>{/if}</div>
              <div class="helper-roster-columns"><div><h4>Staff volunteers <span>{session.staffHelpers.length}</span></h4><div class="helper-chip-list">{#each session.staffHelpers as helper}<span class:current-helper={helper.isCurrentStaff} class="helper-chip">{helper.displayName}{#if helper.isCurrentStaff} <small>(you)</small>{/if}</span>{:else}<span class="muted">No staff volunteers yet.</span>{/each}</div></div><div><h4>Student helpers checked in <span>{session.studentHelpers.length}</span></h4><div class="helper-chip-list">{#each session.studentHelpers as helper}<span class="helper-chip student-helper">{helper.studentName}</span>{:else}<span class="muted">None checked in yet.</span>{/each}</div></div></div>
            </article>
          {/each}
        </div>
      </section>
    {/each}
  </div>
{/if}
