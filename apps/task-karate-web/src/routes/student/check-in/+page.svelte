<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  import { formatDojoRange } from '$lib/time';

  let session: StudentSession | null = null;
  let profile: any = null;
  let classes: any[] = [];
  let attended = new Set<number>();
  let selected: any = null;
  let error = '';
  let notice = '';
  let loading = true;
  let checkingIn = false;
  let helperMode = false;

  function localDateKey(date = new Date()) { return `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}-${String(date.getDate()).padStart(2, '0')}`; }
  function dateLabel(value: string) { return new Date(`${value.slice(0, 10)}T12:00:00`).toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric' }); }
  function timeLabel(item: any) { return formatDojoRange(item.startTime, item.endTime); }
  function classIsForStudent(item: any) {
    const name = String(item.className ?? '').toLowerCase();
    const ageGroup = String(profile?.ageGroup ?? '').toLowerCase();
    const programs = (profile?.programs ?? []).map((program: any) => String(program.programCode ?? '').toLowerCase());
    const hasIs3 = programs.includes('is3');
    const hasKarate = programs.some((program: string) => program.includes('karate') || program.includes('kids') || program.includes('adult'));
    if (name.includes('is3') || name.includes('eskrima')) return hasIs3;
    if (programs.length && !hasKarate) return false;
    if (name.includes('weapons')) return programs.some((program: string) => program.includes('weapon'));
    if (name.includes('sparring')) return programs.some((program: string) => program.includes('sparring'));
    if (name.includes('kids') && ageGroup !== 'kids') return false;
    if (name.includes('teens & adults') && ageGroup === 'kids') return false;
    const rank = String(profile?.rankName ?? '').toLowerCase().replace(' belt', '');
    return !rank || !name.includes('—') || name.includes(rank);
  }
  $: eligibleClasses = classes.filter(classIsForStudent).sort((a, b) => String(a.startTime ?? '').localeCompare(String(b.startTime ?? '')));
  function choose(item: any) { if (!item.cancelled && !attended.has(item.sessionId)) { selected = item; helperMode = false; error = ''; } }
  async function confirmCheckIn() {
    if (!selected) return;
    checkingIn = true; error = '';
    try { await api(`/api/student/schedule/${selected.sessionId}/check-in`, { method: 'POST', body: JSON.stringify({ helper: helperMode }) }); attended = new Set([...attended, selected.sessionId]); notice = `${selected.className} ${helperMode ? 'helper ' : ''}check-in recorded.`; selected = null; }
    catch (e) { error = apiError(e); }
    finally { checkingIn = false; }
  }
  onMount(async () => {
    session = await requireStudent(); if (!session) return;
    try {
      const today = new Date(); const from = localDateKey(today); const to = localDateKey(new Date(today.getFullYear(), today.getMonth(), today.getDate() + 1));
      [profile, classes, attended] = await Promise.all([
        api('/api/student/profile'),
        api<any[]>(`/api/student/public/schedule?from=${from}&to=${to}`),
        api<number[]>(`/api/student/check-ins?from=${from}&to=${to}`).then((ids) => new Set(ids))
      ]);
    } catch (e) { error = apiError(e); }
    finally { loading = false; }
  });
</script>

<svelte:head><title>Task Karate | Check in</title></svelte:head>

{#if session}
  <StudentShell {session} active="check-in">
    <section class="student-heading"><span class="student-eyebrow">TODAY AT THE DOJO</span><h1>CHECK IN</h1><p>Choose only the class you are attending today. Your attendance is recorded after you confirm.</p></section>
    {#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="success-notice" role="status">✓ {notice}</div>{/if}
    {#if loading}<div class="student-panel loading-panel">Loading today’s classes…</div>{:else if eligibleClasses.length === 0}<section class="student-panel empty-panel"><span class="rank-orb">✓</span><h2>No matching class today</h2><p>There are no published sessions for your current program and rank today. The full public schedule is still available if you need it.</p><a class="outline-button" href="/schedule">View public schedule</a></section>{:else}
      <section class="student-panel check-in-guide"><div><span class="student-eyebrow">ONE DAY · ONE DECISION</span><h2>Which class are you attending?</h2><p>Showing classes that match your current program and rank. Nothing is recorded until you confirm the specific class.</p></div><span class="check-in-date">{dateLabel(new Date().toISOString())}</span></section>
      <div class="today-class-list" aria-label="Today’s class sessions">
        {#each eligibleClasses as item}
          <article class="student-panel today-class-card" class:selected={selected?.sessionId === item.sessionId} class:cancelled={item.cancelled}>
            <div class="today-class-info"><span class="student-eyebrow">{timeLabel(item)}</span><h2>{item.className}</h2><p>{item.location ?? 'Main dojo'}</p>{#if item.description}<small>{item.description}</small>{/if}</div>
            <div class="today-class-action">{#if item.cancelled}<span class="cancelled-badge">Cancelled</span>{:else if attended.has(item.sessionId)}<span class="checked-in-badge">✓ Checked in</span>{:else}<button class="outline-button" type="button" on:click={() => choose(item)}>Select class</button>{/if}</div>
          </article>
        {/each}
      </div>
      {#if selected}<section class="student-panel check-in-confirm"><div><span class="student-eyebrow">READY TO RECORD</span><h2>{selected.className}</h2><p>{timeLabel(selected)} · {selected.location ?? 'Main dojo'}</p><label class="helper-choice"><input type="checkbox" bind:checked={helperMode} /> Record me as a helper (only available when I outrank this class)</label></div><div class="check-in-confirm-actions"><button class="outline-button" type="button" on:click={() => selected = null}>Choose another</button><button class="primary-button" type="button" disabled={checkingIn} on:click={confirmCheckIn}>{checkingIn ? 'Recording…' : helperMode ? 'Confirm helper check-in' : 'Confirm check-in'}</button></div></section>{/if}
    {/if}
  </StudentShell>
{/if}
