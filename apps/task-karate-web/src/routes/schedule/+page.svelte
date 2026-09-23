<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import { apiError } from '$lib/student-session';
  import { formatDojoRange } from '$lib/time';
  import '$lib/student.css';
  import '$lib/visual-refresh.css';

  type ScheduleItem = { sessionId: number; sessionDate: string; startTime?: string; endTime?: string; className: string; description?: string | null; location?: string | null; cancelled?: boolean };
  type ScheduleGroup = { key: string; primary: ScheduleItem; items: ScheduleItem[] };
  type DirectoryStudent = { studentId: number; displayName: string; rankName?: string | null; is3LevelName?: string | null; profileImagePath?: string | null };
  let schedule: ScheduleItem[] = [];
  let students: DirectoryStudent[] = [];
  let loading = true;
  let error = '';
  let action = '';
  let selectedProgram = 'All programs';
  let selectedDate = '';
  let studentSearch = '';
  let selectedStudent: DirectoryStudent | null = null;
  let checkInPin = '';
  let selectedClass: ScheduleItem | null = null;
  let selectedGroup: ScheduleGroup | null = null;
  let selectedClassId = '';
  let checkInBusy = false;
  let attendanceMode: 'regular' | 'helper' = 'regular';
  let checkedIn = new Set<number>();

  const todayKey = new Date().toISOString().slice(0, 10);
  function dateKey(value: string) { return value.slice(0, 10); }
  function dayLabel(value: string, options: Intl.DateTimeFormatOptions) { return new Intl.DateTimeFormat(undefined, options).format(new Date(`${dateKey(value)}T12:00:00`)); }
  function timeLabel(item: ScheduleItem) { return formatDojoRange(item.startTime, item.endTime); }
  function minutes(value?: string) { if (!value) return null; const match = value.match(/(\d{1,2}):(\d{2})\s*(AM|PM)?/i); if (!match) return null; let hour = Number(match[1]); const minute = Number(match[2]); if (match[3]?.toUpperCase() === 'PM' && hour < 12) hour += 12; if (match[3]?.toUpperCase() === 'AM' && hour === 12) hour = 0; return hour * 60 + minute; }
  function isInProgress(item: ScheduleItem) { if (dateKey(item.sessionDate) !== todayKey) return false; const now = new Date(); const current = now.getHours() * 60 + now.getMinutes(); const start = minutes(item.startTime); const end = minutes(item.endTime) ?? (start === null ? null : start + 60); return start !== null && end !== null && current >= start && current < end; }
  function programName(item: ScheduleItem) { return item.className.split('—')[0].trim() || 'Other'; }
  function displayName(item: ScheduleItem) { return item.className.includes('—') ? item.className.split('—').slice(1).join('—').trim() : item.className; }
  function groupSchedule(items: ScheduleItem[]): ScheduleGroup[] { const groups = new Map<string, ScheduleGroup>(); for (const item of items) { const key = `${dateKey(item.sessionDate)}|${item.startTime ?? ''}|${item.endTime ?? ''}|${item.location ?? ''}|${programName(item)}`; const existing = groups.get(key); if (existing) existing.items.push(item); else groups.set(key, { key, primary: item, items: [item] }); } return [...groups.values()]; }
  function groupName(group: ScheduleGroup) { return [...new Set(group.items.map(displayName))].join(' · '); }
  function openCheckIn(group: ScheduleGroup) { selectedGroup = group; selectedClass = group.primary; selectedClassId = String(group.primary.sessionId); selectedStudent = null; studentSearch = ''; checkInPin = ''; attendanceMode = 'regular'; action = ''; }
  function selectStudent(student: DirectoryStudent) { selectedStudent = student; const rank = String(student.rankName ?? '').replace(/\s+belt$/i, '').toLowerCase(); const match = selectedGroup?.items.find((item) => rank && item.className.toLowerCase().includes(rank)); if (match) { selectedClass = match; selectedClassId = String(match.sessionId); } }
  function changeTrack() { selectedClass = selectedGroup?.items.find((item) => String(item.sessionId) === selectedClassId) ?? selectedGroup?.primary ?? null; }
  function closeCheckIn() { if (!checkInBusy) { selectedClass = null; selectedGroup = null; checkInPin = ''; } }
  async function checkIn() { if (!selectedClass || !selectedStudent || !checkInPin.trim()) return; const helper = attendanceMode === 'helper'; checkInBusy = true; action = ''; try { await api(`/api/student/public/schedule/${selectedClass.sessionId}/check-in`, { method: 'POST', body: JSON.stringify({ studentId: selectedStudent.studentId, pin: checkInPin, confirmed: true, helper }) }); checkedIn = new Set([...checkedIn, selectedClass.sessionId]); action = `${selectedStudent.displayName} is checked in ${helper ? 'as a helper' : ''} for ${displayName(selectedClass)}.`; selectedClass = null; selectedGroup = null; checkInPin = ''; } catch (e) { action = apiError(e); } finally { checkInBusy = false; } }

  onMount(async () => {
    try { [schedule, students] = await Promise.all([api<ScheduleItem[]>('/api/student/public/schedule'), api<DirectoryStudent[]>('/api/student/public/students')]); selectedDate = schedule.some((item) => dateKey(item.sessionDate) === todayKey) ? todayKey : (schedule[0] ? dateKey(schedule[0].sessionDate) : todayKey); }
    catch (e) { error = apiError(e); }
    finally { loading = false; }
  });

  $: dates = Array.from(new Map(schedule.map((item) => [dateKey(item.sessionDate), item.sessionDate])));
  $: programs = ['All programs', ...Array.from(new Set(schedule.map((item) => programName(item))))];
  $: selectedItems = schedule.filter((item) => dateKey(item.sessionDate) === selectedDate && (selectedProgram === 'All programs' || programName(item) === selectedProgram));
  $: todayItems = schedule.filter((item) => dateKey(item.sessionDate) === todayKey && (selectedProgram === 'All programs' || programName(item) === selectedProgram));
  $: selectedGroups = groupSchedule(selectedItems);
  $: todayGroups = groupSchedule(todayItems);
  $: currentItems = todayGroups.filter((group) => isInProgress(group.primary));
  $: upcomingToday = todayGroups.filter((group) => !isInProgress(group.primary));
  $: matchingStudents = students.filter((student) => `${student.displayName} ${student.rankName ?? ''} ${student.is3LevelName ?? ''}`.toLowerCase().includes(studentSearch.trim().toLowerCase())).slice(0, 12);
</script>

<svelte:head><title>Task Karate | Today's schedule</title><meta name="description" content="See today's Task Karate class schedule and check in at the dojo." /></svelte:head>

<main class="schedule-page schedule-live-page">
  <header class="schedule-header"><a class="brand" href="/schedule" aria-label="Task Karate schedule"><span class="brand-mark">TK</span><span><strong>TASK KARATE</strong><small>LA CROSSE · WISCONSIN</small></span></a><div class="schedule-actions"><a class="outline-button" href="/student/login">Student hub</a><a class="outline-button" href="/staff/signin">Staff</a></div></header>
  <section class="schedule-hero"><span class="student-eyebrow">DOJO SCHEDULE · TODAY FIRST</span><h1>Step onto the mat.</h1><p>See what is happening now, what is next, and check in for the class you are actually attending.</p></section>

  {#if loading}<div class="schedule-state loading-panel">Loading the live dojo schedule…</div>{:else if error}<div class="schedule-state error" role="alert"><strong>Schedule unavailable.</strong><p>{error}</p></div>{:else}
    <section class="schedule-live-board">
      <div class="schedule-live-heading"><div><span class="control-kicker">{dayLabel(todayKey, { weekday: 'long', month: 'long', day: 'numeric' })}</span><h2>Happening today</h2></div><span class="session-count">{todayGroups.length} class blocks</span></div>
      {#if currentItems.length}<div class="current-class-list">{#each currentItems as group}<button class="current-class-card" type="button" on:click={() => openCheckIn(group)}><span class="live-badge">● IN PROGRESS</span><strong>{groupName(group)}</strong><span>{programName(group.primary)} · {timeLabel(group.primary)} · {group.primary.location ?? 'Main dojo'}</span><em>Open check-in →</em></button>{/each}</div>{:else}<div class="no-current"><strong>No class is in progress right now.</strong><span>Upcoming classes for today are listed below.</span></div>{/if}
      <div class="schedule-section-heading"><div><span class="control-kicker">UP NEXT</span><h3>Coming classes today</h3></div><label>Program<select bind:value={selectedProgram}>{#each programs as program}<option value={program}>{program}</option>{/each}</select></label></div>
      <div class="today-class-list">{#each upcomingToday as group}<button class="today-class-card" type="button" on:click={() => openCheckIn(group)}><span class="today-class-time">{timeLabel(group.primary)}</span><span><strong>{groupName(group)}</strong><small>{programName(group.primary)} · {group.primary.location ?? 'Main dojo'}</small></span><span class="today-class-action">{group.items.some((item) => checkedIn.has(item.sessionId)) ? 'Checked in' : 'Check in →'}</span></button>{:else}<p class="muted">No more classes are scheduled for today.</p>{/each}</div>
    </section>

    <section class="schedule-future-board"><div class="schedule-live-heading"><div><span class="control-kicker">VIEW ONLY</span><h2>Future schedule</h2><p class="muted">Choose a future day to plan. Check-in only opens for today's sessions.</p></div></div><div class="future-date-row">{#each dates.filter((value) => value[0] !== todayKey) as [key, value]}<button class:selected={selectedDate === key} class="future-date" type="button" on:click={() => selectedDate = key}><strong>{dayLabel(value, { weekday: 'short' })}</strong><span>{dayLabel(value, { month: 'short', day: 'numeric' })}</span></button>{/each}</div>{#if selectedDate !== todayKey}<div class="future-class-list">{#each selectedGroups as group}<article class="future-class-card"><div><span class="class-program">{programName(group.primary)}</span><h3>{groupName(group)}</h3><p>{timeLabel(group.primary)} · {group.primary.location ?? 'Main dojo'}</p></div><span class="pill">View only</span></article>{:else}<p class="muted">No classes published for this day.</p>{/each}</div>{/if}</section>
  {/if}
  {#if action}<p class="schedule-feedback" role="status">{action}</p>{/if}

  {#if selectedClass}<div class="schedule-modal-backdrop" role="presentation" on:click={closeCheckIn}><div class="schedule-modal" role="dialog" aria-modal="true" aria-labelledby="check-in-title" tabindex="-1" on:click|stopPropagation on:keydown|stopPropagation><button class="modal-close" type="button" aria-label="Close check-in" on:click={closeCheckIn}>×</button><span class="control-kicker">TODAY'S CHECK-IN</span><h2 id="check-in-title">{selectedGroup ? groupName(selectedGroup) : displayName(selectedClass)}</h2><p class="muted">{programName(selectedClass)} · {timeLabel(selectedClass)} · {selectedClass.location ?? 'Main dojo'}</p>{#if selectedGroup && selectedGroup.items.length > 1}<label>Class track<select bind:value={selectedClassId} on:change={changeTrack}>{#each selectedGroup.items as track}<option value={track.sessionId}>{displayName(track)}</option>{/each}</select></label>{/if}<label>Find the student<input type="search" bind:value={studentSearch} placeholder="Start typing a name…" /></label><div class="public-student-results">{#each matchingStudents as student}<button class:selected={selectedStudent?.studentId === student.studentId} type="button" on:click={() => selectStudent(student)}><span class="mini-avatar">{student.displayName.split(' ').map((part) => part[0]).join('').slice(0, 2)}</span><span><strong>{student.displayName}</strong><small>{student.rankName ?? student.is3LevelName ?? 'Rank not assigned'}</small></span></button>{:else}<p class="muted">No students match that search.</p>{/each}</div>{#if selectedStudent}<div class="check-in-confirmation"><label class="attendance-mode-label" for="attendance-mode"><strong>Attendance type</strong><select id="attendance-mode" bind:value={attendanceMode}><option value="regular">Regular attendance</option><option value="helper">Check in as helper</option></select></label><label for="check-in-pin"><strong>Student check-in PIN</strong><input id="check-in-pin" type="password" bind:value={checkInPin} minlength="4" maxlength="6" pattern="[0-9][0-9][0-9][0-9][0-9]?[0-9]?" inputmode="numeric" placeholder="Enter 4–6 digit PIN" /></label><p>Helper check-in is only accepted when the student outranks every belt in this class.</p><button class="primary-button wide" type="button" on:click={checkIn} disabled={checkInBusy || !checkInPin.trim()}>{checkInBusy ? 'Recording…' : attendanceMode === 'helper' ? 'Confirm helper check-in' : 'Confirm and check in'}</button></div>{/if}</div></div>{/if}
</main>
