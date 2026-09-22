<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { apiError, type StudentSession } from '$lib/student-session';
  let schedule: any[] = []; let loading = true; let error = ''; let session: StudentSession | null = null; let checked = new Set<number>(); let action = '';
  onMount(async () => { try { schedule = await api<any[]>('/api/student/public/schedule'); } catch (e) { error = apiError(e); } try { session = await api<StudentSession>('/api/student/auth/me'); } catch { session = null; } finally { loading = false; } });
  async function checkIn(item: any) { if (!session) return goto('/student/login'); if (session.disclaimerRequired) return goto('/student/disclaimer'); action = ''; try { await api(`/api/student/schedule/${item.sessionId}/check-in`, { method: 'POST' }); checked = new Set([...checked, item.sessionId]); action = 'Check-in recorded in the dojo database.'; } catch (e) { action = apiError(e); } }
  function day(value: string) { return new Date(value).toLocaleDateString(undefined, { weekday: 'long', month: 'short', day: 'numeric' }); }
</script>
<svelte:head><title>Task Karate | Class schedule</title></svelte:head>
<main class="schedule-page">
  <header class="schedule-header"><a class="brand" href="/schedule"><span class="brand-mark">TK</span><span><strong>TASK KARATE</strong><small>LA CROSSE · WISCONSIN</small></span></a><div class="schedule-actions">{#if session}<a class="outline-button" href="/student">Open student hub</a>{:else}<a class="outline-button" href="/student/login">Student sign-in</a>{/if}<a class="outline-button" href="/staff/signin">Staff</a></div></header>
  <section class="schedule-hero"><span class="student-eyebrow">LIVE DOJO SCHEDULE</span><h1>Find your next class.</h1><p>Actual class sessions from the Task Karate database. Sign in to check yourself in securely.</p></section>
  {#if loading}<div class="student-panel loading-panel">Loading the current schedule…</div>{:else if error}<div class="student-panel error" role="alert"><strong>Schedule unavailable.</strong><p>{error}</p><a class="primary-button" href="/student/login">Sign in</a></div>{:else if schedule.length === 0}<div class="student-panel empty-panel"><h2>No sessions published</h2><p>Ask the front desk to publish the next class sessions.</p></div>{:else}<section class="schedule-list" aria-label="Upcoming classes">{#each schedule as item}<article class:cancelled={item.cancelled} class="class-card"><div><span class="class-date">{day(item.sessionDate)}</span><h2>{item.className}</h2><p>{item.startTime ?? 'Time to be announced'}{item.endTime ? ` – ${item.endTime}` : ''} {item.location ? ` · ${item.location}` : ''}</p>{#if item.description}<small>{item.description}</small>{/if}</div><div class="class-action">{#if item.cancelled}<span class="pill cancelled-pill">Cancelled</span>{:else if checked.has(item.sessionId)}<span class="pill success-pill">✓ Checked in</span>{:else}<button class="primary-button" type="button" on:click={() => checkIn(item)}>{session ? 'Check in' : 'Sign in to check in'}</button>{/if}</div></article>{/each}</section>{/if}
  {#if action}<p class="schedule-feedback" role="status">{action}</p>{/if}
</main>
