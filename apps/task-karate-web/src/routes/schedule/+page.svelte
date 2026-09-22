<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { apiError, type StudentSession } from '$lib/student-session';
  import '$lib/student.css';
  import '$lib/visual-refresh.css';

  type ScheduleItem = { sessionId: number; sessionDate: string; startTime?: string; endTime?: string; className: string; description?: string | null; location?: string | null; cancelled?: boolean };
  let schedule: ScheduleItem[] = [];
  let loading = true;
  let error = '';
  let session: StudentSession | null = null;
  let checked = new Set<number>();
  let action = '';
  let selectedDate = '';
  let selectedProgram = 'All classes';
  let search = '';
  let weekOffset = 0;
  let dates: [string, string][] = [];
  let weekDates: [string, string][] = [];
  let programs: string[] = [];
  let visibleItems: ScheduleItem[] = [];

  function dateKey(value: string) { return value.slice(0, 10); }
  function dayLabel(value: string, options: Intl.DateTimeFormatOptions) { return new Intl.DateTimeFormat(undefined, options).format(new Date(`${dateKey(value)}T12:00:00`)); }
  function programName(className: string) { return className.split('—')[0].trim() || 'Other'; }
  function timeLabel(item: ScheduleItem) { return item.startTime ? (item.endTime ? `${item.startTime} – ${item.endTime}` : item.startTime) : 'Time TBA'; }
  function selectDate(key: string) { selectedDate = key; window.scrollTo({ top: 360, behavior: 'smooth' }); }
  function changeWeek(direction: number) { weekOffset = Math.max(0, Math.min(Math.ceil(dates.length / 7) - 1, weekOffset + direction)); }

  async function checkIn(item: ScheduleItem) {
    if (!session) return goto('/student/login');
    if (session.disclaimerRequired) return goto('/student/disclaimer');
    action = '';
    try {
      await api(`/api/student/schedule/${item.sessionId}/check-in`, { method: 'POST' });
      checked = new Set([...checked, item.sessionId]);
      action = 'Check-in recorded in the dojo database.';
    } catch (e) { action = apiError(e); }
  }

  onMount(async () => {
    try { schedule = await api<ScheduleItem[]>('/api/student/public/schedule'); } catch (e) { error = apiError(e); }
    try { session = await api<StudentSession>('/api/student/auth/me'); } catch { session = null; } finally { loading = false; }
  });

  $: dates = Array.from(new Map(schedule.map((item) => [dateKey(item.sessionDate), item.sessionDate])));
  $: weekDates = dates.slice(weekOffset * 7, weekOffset * 7 + 7);
  $: if (!selectedDate && weekDates.length) selectedDate = weekDates[0][0];
  $: if (selectedDate && !weekDates.some(([key]) => key === selectedDate) && weekDates.length) selectedDate = weekDates[0][0];
  $: programs = ['All classes', ...Array.from(new Set(schedule.map((item) => programName(item.className))))];
  $: visibleItems = schedule.filter((item) => dateKey(item.sessionDate) === selectedDate && (selectedProgram === 'All classes' || programName(item.className) === selectedProgram) && (!search.trim() || item.className.toLowerCase().includes(search.trim().toLowerCase())));
</script>

<svelte:head><title>Task Karate | Class schedule</title><meta name="description" content="View upcoming Task Karate classes and securely check in to your session." /></svelte:head>

<main class="schedule-page">
  <header class="schedule-header"><a class="brand" href="/schedule" aria-label="Task Karate schedule"><span class="brand-mark">TK</span><span><strong>TASK KARATE</strong><small>LA CROSSE · WISCONSIN</small></span></a><div class="schedule-actions">{#if session}<a class="outline-button" href="/student">Open student hub</a>{:else}<a class="outline-button" href="/student/login">Student sign-in</a>{/if}<a class="outline-button" href="/staff/signin">Staff</a></div></header>
  <section class="schedule-hero"><span class="student-eyebrow">LIVE DOJO SCHEDULE</span><h1>Plan your training.</h1><p>Choose a day, find your class, and check in securely when you arrive.</p></section>

  {#if loading}
    <div class="schedule-state loading-panel">Loading the current schedule…</div>
  {:else if error}
    <div class="schedule-state error" role="alert"><strong>Schedule unavailable.</strong><p>{error}</p><a class="primary-button" href="/student/login">Sign in</a></div>
  {:else if schedule.length === 0}
    <div class="schedule-state empty-panel"><h2>No sessions published</h2><p>Ask the front desk to publish the next class sessions.</p></div>
  {:else}
    <section class="schedule-controls" aria-label="Schedule filters">
      <div class="schedule-control-heading"><div><span class="control-kicker">UPCOMING CLASSES</span><h2>What day are you training?</h2></div><span class="session-count">{dates.length} days published</span></div>
      <div class="date-picker-heading"><button class="week-button" type="button" on:click={() => changeWeek(-1)} disabled={weekOffset === 0} aria-label="Show previous week">←</button><strong>{weekDates.length ? `${dayLabel(weekDates[0][1], { month: 'short', day: 'numeric' })} – ${dayLabel(weekDates[weekDates.length - 1][1], { month: 'short', day: 'numeric', year: 'numeric' })}` : 'Upcoming dates'}</strong><button class="week-button" type="button" on:click={() => changeWeek(1)} disabled={(weekOffset + 1) * 7 >= dates.length} aria-label="Show next week">→</button></div>
      <div class="date-picker" role="tablist" aria-label="Choose a class date">
        {#each weekDates as [key, value]}
          <button class:selected={selectedDate === key} class="date-choice" type="button" role="tab" aria-selected={selectedDate === key} on:click={() => selectDate(key)}><span>{dayLabel(value, { weekday: 'short' })}</span><strong>{dayLabel(value, { day: 'numeric' })}</strong><small>{schedule.filter((item) => dateKey(item.sessionDate) === key).length} {schedule.filter((item) => dateKey(item.sessionDate) === key).length === 1 ? 'class' : 'classes'}</small></button>
        {/each}
      </div>
      <div class="schedule-filters"><label>Find a class<input bind:value={search} type="search" placeholder="Search by class name" /></label><label>Program<select bind:value={selectedProgram}>{#each programs as program}<option value={program}>{program}</option>{/each}</select></label></div>
    </section>

    <section class="selected-day" aria-live="polite" aria-label="Classes for selected day">
      <div class="selected-day-heading"><div><span class="control-kicker">YOUR SELECTED DAY</span><h2>{selectedDate ? dayLabel(selectedDate, { weekday: 'long', month: 'long', day: 'numeric' }) : 'Choose a date'}</h2></div><span class="session-count">{visibleItems.length} {visibleItems.length === 1 ? 'class' : 'classes'}</span></div>
      {#if visibleItems.length === 0}
        <div class="day-empty"><strong>No classes match those filters.</strong><p>Try another program or clear your search.</p></div>
      {:else}
        <div class="class-list">
          {#each visibleItems as item}
            <article class:cancelled={item.cancelled} class="class-card"><div class="class-time"><strong>{timeLabel(item)}</strong><span>{item.location ?? 'Main dojo'}</span></div><div class="class-details"><span class="class-program">{programName(item.className)}</span><h3>{item.className.includes('—') ? item.className.split('—').slice(1).join('—').trim() : item.className}</h3>{#if item.description}<p>{item.description}</p>{/if}</div><div class="class-action">{#if item.cancelled}<span class="pill cancelled-pill">Cancelled</span>{:else if checked.has(item.sessionId)}<span class="pill success-pill">✓ Checked in</span>{:else}<button class="primary-button" type="button" on:click={() => checkIn(item)}>{session ? 'Check in' : 'Sign in to check in'}</button>{/if}</div></article>
          {/each}
        </div>
      {/if}
    </section>
  {/if}
  {#if action}<p class="schedule-feedback" role="status">{action}</p>{/if}
</main>
