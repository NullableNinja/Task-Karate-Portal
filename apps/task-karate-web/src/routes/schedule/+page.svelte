<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import { apiError } from '$lib/student-session';
  import { formatDojoRange } from '$lib/time';
  import '$lib/student.css';
  import '$lib/visual-refresh.css';
  import PortalNav from '$lib/components/PortalNav.svelte';

  type ScheduleItem = { sessionId: number; sessionDate: string; startTime?: string; endTime?: string; className: string; description?: string | null; location?: string | null; cancelled?: boolean };
  type ScheduleGroup = { key: string; primary: ScheduleItem; items: ScheduleItem[] };
  type DirectoryStudent = { studentId: number; displayName: string; rankName?: string | null; is3LevelName?: string | null; profileImagePath?: string | null };
  type RosterEntry = { attendanceId?: number; studentId?: number; studentName: string; checkedInAtUtc?: string; status?: 'present' | 'helper' | string; notes?: string | null };
  type PublicRosterEntry = { studentName: string };
  const beltColors: Record<string, string> = { white: '#f4f7fb', gold: '#d5ad3c', yellow: '#e6c94b', orange: '#f07b25', purple: '#a365d3', green: '#39b47d', blue: '#4f9ee8', brown: '#a56d44', red: '#e2585e', black: '#1d2735' };
  const beltOrderNames = ['white', 'gold', 'orange', 'green', 'purple', 'blue', 'red', 'brown', 'black'];
  let schedule: ScheduleItem[] = [];
  let students: DirectoryStudent[] = [];
  let loading = true;
  let error = '';
  let action = '';
  let selectedProgram = 'All programs';
  let futureProgramType = 'All program types';
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
  let preEnrolled = new Set<number>();
  let currentTime = new Date();
  let actionMode: 'check-in' | 'pre-enroll' = 'check-in';
  let modalError = '';
  let eligibilityError = '';
  let modalShake = false;
  let modalStep: 1 | 2 = 1;
  let rosterGroup: ScheduleGroup | null = null;
  let rosterEntries: RosterEntry[] = [];
  let rosterLoading = false;
  let rosterAvailable = false;
  let rosterStaffAccess = false;
  let rosterError = '';
  let rosterRemovingId: number | null = null;
  let shakeTimer: number | undefined;

  function localDateKey(value = currentTime) {
    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
  $: todayKey = localDateKey();
  function dateKey(value: string) { return value.slice(0, 10); }
  function dayLabel(value: string, options: Intl.DateTimeFormatOptions) { return new Intl.DateTimeFormat(undefined, options).format(new Date(`${dateKey(value)}T12:00:00`)); }
  function timeLabel(item: ScheduleItem) { return formatDojoRange(item.startTime, item.endTime); }
  function minutes(value?: string) { if (!value) return null; const match = value.match(/(\d{1,2}):(\d{2})\s*(AM|PM)?/i); if (!match) return null; let hour = Number(match[1]); const minute = Number(match[2]); if (match[3]?.toUpperCase() === 'PM' && hour < 12) hour += 12; if (match[3]?.toUpperCase() === 'AM' && hour === 12) hour = 0; return hour * 60 + minute; }
  function classState(item: ScheduleItem): 'past' | 'live' | 'upcoming' | 'future' {
    const itemDate = dateKey(item.sessionDate);
    if (itemDate < todayKey) return 'past';
    if (itemDate > todayKey) return 'future';
    const current = currentTime.getHours() * 60 + currentTime.getMinutes();
    const start = minutes(item.startTime);
    const end = minutes(item.endTime) ?? (start === null ? null : start + 60);
    if (start === null || end === null) return 'upcoming';
    if (current >= end) return 'past';
    if (current >= start) return 'live';
    return 'upcoming';
  }
  function isInProgress(item: ScheduleItem) { return classState(item) === 'live'; }
  function relativeLabel(group: ScheduleGroup) {
    if (isInProgress(group.primary)) return 'HAPPENING NOW';
    const start = minutes(group.primary.startTime);
    const current = currentTime.getHours() * 60 + currentTime.getMinutes();
    if (start !== null && start > current && start - current <= 60) return `STARTS IN ${start - current} MIN`;
    return 'LATER TODAY';
  }
  function nextLabel(group: ScheduleGroup) {
    const start = minutes(group.primary.startTime);
    const current = currentTime.getHours() * 60 + currentTime.getMinutes();
    return start !== null && start > current && start - current <= 60 ? `STARTS IN ${start - current} MIN` : 'NEXT UP';
  }
  function groupStart(group: ScheduleGroup) { return minutes(group.primary.startTime) ?? Number.MAX_SAFE_INTEGER; }
  function sortGroups(groups: ScheduleGroup[]) { return [...groups].sort((a, b) => groupStart(a) - groupStart(b)); }
  function programName(item: ScheduleItem) { return item.className.split('—')[0].trim() || 'Other'; }
  function programType(item: ScheduleItem) {
    const text = `${programName(item)} ${item.className}`.toLowerCase();
    if (text.includes('is3')) return 'IS3';
    if (text.includes('kid')) return 'Kids';
    if (text.includes('teen') || text.includes('adult')) return 'Teen/Adult';
    return 'Other';
  }
  function displayName(item: ScheduleItem) { return item.className.includes('—') ? item.className.split('—').slice(1).join('—').trim() : item.className; }
  function beltNames(group: ScheduleGroup) {
    const text = group.items.map(displayName).join(' · ').toLowerCase();
    if (text.includes('all belt')) return [...beltOrderNames];
    const names = new Set<string>();
    for (const belt of beltOrderNames) if (text.includes(belt)) names.add(belt);
    const allFrom = text.match(/\b(white|gold|orange|green|purple|blue|red|brown|black)\s+belt\s*(?:&|and)?\s*up\b/);
    if (allFrom) {
      const start = beltOrderNames.indexOf(allFrom[1]);
      beltOrderNames.slice(start).forEach((belt) => names.add(belt));
    }
    return beltOrderNames.filter((belt) => names.has(belt));
  }
  function beltOrder(value?: string | null) {
    const normalized = String(value ?? '').toLowerCase();
    const belt = beltOrderNames.find((name) => normalized.includes(name));
    return belt ? beltOrderNames.indexOf(belt) + 1 : null;
  }
  function classRequirement(item: ScheduleItem) {
    const names = beltNames({ key: String(item.sessionId), primary: item, items: [item] });
    return names.length ? { name: `${names[0][0].toUpperCase()}${names[0].slice(1)} Belt`, order: Math.min(...names.map((name) => beltOrderNames.indexOf(name) + 1)) } : null;
  }
  function shouldRecordAsHelper(student: DirectoryStudent, item: ScheduleItem) {
    const requirement = classRequirement(item);
    const studentRank = beltOrder(student.rankName);
    return !!requirement && !!selectedGroup && !isAllBelts(selectedGroup) && studentRank !== null && studentRank > requirement.order;
  }
  function eligibleClassForStudent(student: DirectoryStudent) {
    const studentOrder = beltOrder(student.rankName);
    const candidates = selectedGroup?.items.filter((item) => {
      const requirement = classRequirement(item);
      return !requirement || (studentOrder !== null && studentOrder >= requirement.order);
    }) ?? [];
    return candidates.sort((a, b) => (classRequirement(b)?.order ?? 0) - (classRequirement(a)?.order ?? 0))[0] ?? null;
  }
  function canStudentUseClass(student: DirectoryStudent) { return eligibleClassForStudent(student) !== null; }
  function announceError(message: string) {
    modalError = message;
    eligibilityError = message;
    modalShake = false;
    if (shakeTimer) window.clearTimeout(shakeTimer);
    shakeTimer = window.setTimeout(() => { modalShake = true; window.setTimeout(() => modalShake = false, 460); }, 0);
    if (typeof navigator !== 'undefined' && 'vibrate' in navigator) navigator.vibrate([70, 40, 90]);
  }
  function groupSchedule(items: ScheduleItem[]): ScheduleGroup[] { const groups = new Map<string, ScheduleGroup>(); for (const item of items) { const key = `${dateKey(item.sessionDate)}|${item.startTime ?? ''}|${item.endTime ?? ''}|${item.location ?? ''}|${programName(item)}`; const existing = groups.get(key); if (existing) existing.items.push(item); else groups.set(key, { key, primary: item, items: [item] }); } return [...groups.values()]; }
  function isAllBelts(group: ScheduleGroup) { return beltNames(group).length === beltOrderNames.length; }
  function groupName(group: ScheduleGroup) { return isAllBelts(group) ? 'All belts' : [...new Set(group.items.map(displayName))].join(' · '); }
  function openRoster(group: ScheduleGroup) { rosterGroup = group; rosterError = ''; void loadRoster(group); }
  async function loadRoster(group: ScheduleGroup, showLoading = true) { rosterGroup = group; if (showLoading) rosterLoading = true; rosterError = ''; try { const responses = await Promise.all(group.items.map((item) => api<RosterEntry[]>(`/api/portal-admin/attendance?sessionId=${item.sessionId}`))); rosterEntries = [...new Map(responses.flat().filter((entry) => entry.studentId !== undefined).map((entry) => [entry.studentId, entry])).values()]; rosterAvailable = true; rosterStaffAccess = true; } catch { try { const responses = await Promise.all(group.items.map((item) => api<PublicRosterEntry[]>(`/api/student/public/schedule/${item.sessionId}/roster`))); rosterEntries = [...new Map(responses.flat().map((entry) => [entry.studentName.toLowerCase(), { studentName: entry.studentName }])).values()]; rosterAvailable = true; rosterStaffAccess = false; } catch { rosterAvailable = false; rosterStaffAccess = false; rosterEntries = []; rosterError = 'The class roster is unavailable right now.'; } } finally { rosterLoading = false; } }
  async function removeRosterEntry(entry: RosterEntry) { if (!rosterStaffAccess || entry.attendanceId === undefined || rosterRemovingId !== null) return; const reason = window.prompt(`Why are you removing ${entry.studentName} from this roster?`); if (!reason?.trim() || !rosterGroup) return; rosterRemovingId = entry.attendanceId; rosterError = ''; try { await api(`/api/portal-admin/attendance/${entry.attendanceId}`, { method: 'DELETE', body: JSON.stringify({ reason: reason.trim() }) }); action = `${entry.studentName} was removed from the class roster.`; await loadRoster(rosterGroup, false); } catch (e) { rosterError = apiError(e); } finally { rosterRemovingId = null; } }
  function openCheckIn(group: ScheduleGroup, mode: 'check-in' | 'pre-enroll' = 'check-in') { selectedGroup = group; selectedClass = group.primary; selectedClassId = String(group.primary.sessionId); selectedStudent = null; studentSearch = ''; checkInPin = ''; attendanceMode = 'regular'; action = ''; modalError = ''; eligibilityError = ''; modalShake = false; modalStep = 1; actionMode = mode; openRoster(group); }
  function selectStudent(student: DirectoryStudent) {
    const match = eligibleClassForStudent(student);
    if (!match) {
      selectedStudent = null;
      announceError(`${student.displayName} is not eligible for this class. A ${student.rankName ?? 'student without a belt rank'} cannot attend this belt track.`);
      return;
    }
    selectedStudent = student;
    eligibilityError = '';
    modalError = '';
    selectedClass = match;
    selectedClassId = String(match.sessionId);
    attendanceMode = shouldRecordAsHelper(student, match) ? 'helper' : 'regular';
  }
  function continueToConfirmation() { if (!selectedStudent || !selectedClass || !canStudentUseClass(selectedStudent)) return; modalError = ''; eligibilityError = ''; checkInPin = ''; modalStep = 2; }
  function backToStudentSearch() { if (!checkInBusy) { modalStep = 1; checkInPin = ''; modalError = ''; } }
  function closeCheckIn() { if (!checkInBusy) { selectedClass = null; selectedGroup = null; checkInPin = ''; modalError = ''; eligibilityError = ''; modalStep = 1; } }
  async function checkIn() {
    if (!selectedClass || !selectedStudent || !checkInPin.trim()) return;
    const helper = attendanceMode === 'helper';
    checkInBusy = true; action = ''; modalError = '';
    try {
      const rosterToRefresh = selectedGroup;
      const endpoint = actionMode === 'pre-enroll' ? `/api/student/public/schedule/${selectedClass.sessionId}/pre-enroll` : `/api/student/public/schedule/${selectedClass.sessionId}/check-in`;
      const result = await api<{ helper?: boolean }>(endpoint, { method: 'POST', body: JSON.stringify({ studentId: selectedStudent.studentId, pin: checkInPin, confirmed: true, helper }) });
      const recordedAsHelper = actionMode === 'check-in' && Boolean(result?.helper ?? helper);
      if (actionMode === 'check-in') checkedIn = new Set([...checkedIn, selectedClass.sessionId]);
      else preEnrolled = new Set([...preEnrolled, selectedClass.sessionId]);
      action = actionMode === 'pre-enroll' ? `${selectedStudent.displayName} is on the list for ${displayName(selectedClass)}.` : `${selectedStudent.displayName} is checked in${recordedAsHelper ? ' as a helper' : ''} for ${displayName(selectedClass)}.`;
      if (typeof navigator !== 'undefined' && 'vibrate' in navigator) navigator.vibrate(55);
      if (actionMode === 'check-in' && rosterToRefresh) await loadRoster(rosterToRefresh, false);
      selectedClass = null; selectedGroup = null; checkInPin = ''; modalError = '';
    } catch (e) {
      announceError(actionMode === 'pre-enroll' ? `Unable to pre-enroll in this class. ${apiError(e)}` : `Unable to check in to this class. ${apiError(e)}`);
    } finally { checkInBusy = false; }
  }

  onMount(async () => {
    try { [schedule, students] = await Promise.all([api<ScheduleItem[]>('/api/student/public/schedule'), api<DirectoryStudent[]>('/api/student/public/students')]); selectedDate = schedule.some((item) => dateKey(item.sessionDate) === todayKey) ? todayKey : (schedule[0] ? dateKey(schedule[0].sessionDate) : todayKey); const groups = sortGroups(groupSchedule(schedule.filter((item) => dateKey(item.sessionDate) === todayKey))); const initialRosterGroup = groups.find((group) => isInProgress(group.primary)) ?? groups.find((group) => classState(group.primary) === 'upcoming') ?? null; if (initialRosterGroup) void loadRoster(initialRosterGroup); }
    catch (e) { error = apiError(e); }
    finally { loading = false; }
  });
  onMount(() => {
    const timer = window.setInterval(() => currentTime = new Date(), 30_000);
    return () => window.clearInterval(timer);
  });

  $: dates = Array.from(new Map(schedule.map((item) => [dateKey(item.sessionDate), item.sessionDate])));
  $: programs = ['All programs', ...Array.from(new Set(schedule.map((item) => programName(item))))];
  $: selectedItems = schedule.filter((item) => dateKey(item.sessionDate) === selectedDate && (selectedProgram === 'All programs' || programName(item) === selectedProgram));
  $: todayItems = schedule.filter((item) => dateKey(item.sessionDate) === todayKey && (selectedProgram === 'All programs' || programName(item) === selectedProgram));
  $: selectedGroups = sortGroups(groupSchedule(selectedItems));
  $: futureGroups = sortGroups(groupSchedule(schedule.filter((item) => dateKey(item.sessionDate) === selectedDate && (futureProgramType === 'All program types' || programType(item) === futureProgramType))));
  $: todayGroups = sortGroups(groupSchedule(todayItems));
  $: currentItems = todayGroups.filter((group) => isInProgress(group.primary));
  $: upcomingToday = todayGroups.filter((group) => classState(group.primary) === 'upcoming');
  $: nextGroup = upcomingToday[0] ?? null;
  $: rosterCheckedCount = rosterEntries.length;
  $: rosterChoices = [...currentItems, ...(nextGroup && !currentItems.some((group) => group.key === nextGroup.key) ? [nextGroup] : [])];
  $: normalizedStudentSearch = studentSearch.trim().toLowerCase();
  $: matchingStudents = normalizedStudentSearch.length < 2 ? [] : students.filter((student) => `${student.displayName} ${student.rankName ?? ''} ${student.is3LevelName ?? ''}`.toLowerCase().includes(normalizedStudentSearch)).slice(0, 8);
</script>

<svelte:head><title>Task Karate | Today's schedule</title><meta name="description" content="See today's Task Karate class schedule and check in at the dojo." /></svelte:head>

<main class="schedule-page schedule-live-page">
  <PortalNav current="schedule" />
  <section class="schedule-hero"><div><span class="student-eyebrow">DOJO SCHEDULE · TODAY FIRST</span><h1>Today's dojo schedule.</h1><p><span class="desktop-only">See what is happening now, what is next, and check in for the class you are actually attending.</span><span class="mobile-only">See what’s next and check in for your class.</span></p></div><div class="schedule-hero-note"><strong>{currentItems.length ? 'CHECK-IN IS OPEN' : nextGroup ? 'NEXT UP' : 'DAY COMPLETE'}</strong><span>{#if currentItems.length}Select a class marked “Happening now” to check in.{:else if nextGroup}{groupName(nextGroup)} starts at {timeLabel(nextGroup.primary)}. Reserve it from the highlighted card below.{:else}There are no more classes scheduled today.{/if}</span></div></section>

  {#if loading}<div class="schedule-state loading-panel">Loading the live dojo schedule…</div>{:else if error}<div class="schedule-state error" role="alert"><strong>Schedule unavailable.</strong><p>{error}</p></div>{:else}
    <section class="schedule-live-board"><div class="schedule-board-layout"><div class="schedule-board-main">
      <div class="schedule-live-heading"><div><span class="control-kicker">{dayLabel(todayKey, { weekday: 'long', month: 'long', day: 'numeric' })}</span><h2>Today at the dojo</h2></div><div class="schedule-heading-tools"><span class="session-count">{todayGroups.length} total class blocks</span><label>View program<select bind:value={selectedProgram}>{#each programs as program}<option value={program}>{program}</option>{/each}</select></label></div></div>
      <div class="schedule-summary" aria-label="Schedule summary"><div><span>NOW</span><strong>{currentItems.length ? `${currentItems.length} in progress` : 'Between classes'}</strong></div><div><span>NEXT</span><strong>{nextGroup ? timeLabel(nextGroup.primary) : 'No more today'}</strong></div><div><span>REMAINING</span><strong>{currentItems.length + upcomingToday.length} class blocks</strong></div></div>
      {#if currentItems.length}<div class="current-class-list">{#each currentItems as group}<button class="current-class-card" type="button" on:click={() => openCheckIn(group)}><span class="live-badge">● HAPPENING NOW</span><strong>{groupName(group)}</strong><span>{programName(group.primary)} · {timeLabel(group.primary)} · {group.primary.location ?? 'Main dojo'}</span><em>Open check-in →</em></button>{/each}</div>{:else if nextGroup}<div class="no-current next-up-panel"><div><span class="live-badge">NEXT UP</span><strong>{groupName(nextGroup)}</strong><span>{programName(nextGroup.primary)} · {timeLabel(nextGroup.primary)} · {nextGroup.primary.location ?? 'Main dojo'}</span></div><span class="next-up-badge">RESERVE BELOW</span></div>{:else}<div class="no-current"><div><strong>Day complete</strong><span>All classes for today are complete.</span></div></div>{/if}
      <div class="schedule-section-heading"><div><span class="control-kicker">{upcomingToday.length ? 'UP NEXT' : 'DAY COMPLETE'}</span><h3>{upcomingToday.length ? 'Remaining classes today' : 'You’re all set for today'}</h3></div><span class="schedule-section-help">Select a class only when you are ready to check in.</span></div>
      <div class="today-class-list">{#each upcomingToday as group, index}{#if index === 0}<button class:next-class-card={true} class="today-class-card" type="button" on:click={() => openCheckIn(group, 'pre-enroll')}><span class="today-class-time"><small>{nextLabel(group)}</small><strong>{timeLabel(group.primary)}</strong></span><span class="today-class-details"><strong>{programName(group.primary)} class</strong><small>{group.primary.location ?? 'Main dojo'}</small><span class="belt-allowlist" aria-label={isAllBelts(group) ? 'Allowed ranks: all belts' : beltNames(group).length ? `Allowed ranks: ${beltNames(group).join(', ')}` : 'Class track'}>{#if beltNames(group).length}{#each beltNames(group) as belt}<span class="belt-dot" style={`--belt-color: ${beltColors[belt]}`} title={`${belt} belt`}></span>{/each}{:else}<span class="class-track-label">{displayName(group.primary)}</span>{/if}</span></span><span class="today-class-action">{group.items.some((item) => preEnrolled.has(item.sessionId)) ? 'On the list' : 'Join next →'}</span></button>{:else}<article class="today-class-card read-only-class"><span class="today-class-time"><small>{relativeLabel(group)}</small><strong>{timeLabel(group.primary)}</strong></span><span class="today-class-details"><strong>{programName(group.primary)} class</strong><small>{group.primary.location ?? 'Main dojo'}</small><span class="belt-allowlist" aria-label={isAllBelts(group) ? 'Allowed ranks: all belts' : beltNames(group).length ? `Allowed ranks: ${beltNames(group).join(', ')}` : 'Class track'}>{#if beltNames(group).length}{#each beltNames(group) as belt}<span class="belt-dot" style={`--belt-color: ${beltColors[belt]}`} title={`${belt} belt`}></span>{/each}{:else}<span class="class-track-label">{displayName(group.primary)}</span>{/if}</span></span><span class="today-class-action">View only</span></article>{/if}{:else}<p class="muted">There are no later classes scheduled today.</p>{/each}</div>
    </div><aside class="schedule-roster-panel"><div class="schedule-roster-heading"><div><span class="control-kicker">{rosterStaffAccess ? 'STAFF VIEW' : 'DOJO ROSTER'}</span><h3>Class roster</h3></div>{#if rosterGroup}<span class="roster-count">{rosterCheckedCount} checked in</span>{/if}</div>{#if rosterChoices.length}<div class="roster-class-tabs" role="tablist" aria-label="Roster class"><!-- roster choices stay compact so current and next class can be compared without opening a modal -->{#each rosterChoices as group}<button class:active={rosterGroup?.key === group.key} type="button" role="tab" aria-selected={rosterGroup?.key === group.key} on:click={() => openRoster(group)}><strong>{isInProgress(group.primary) ? 'Current' : 'Next'}</strong><span>{groupName(group)}</span></button>{/each}</div>{/if}{#if rosterLoading}<p class="muted roster-state">Loading roster…</p>{:else if !rosterAvailable}<div class="roster-state"><strong>Roster unavailable</strong><p>We could not load the check-in list right now.</p></div>{:else if rosterGroup}<div class="roster-state roster-summary"><strong>{groupName(rosterGroup)}</strong><span>{rosterCheckedCount} checked in{rosterStaffAccess ? ' · Staff can remove entries' : ''}</span></div><div class="schedule-roster-list">{#each rosterEntries as entry}<div class="roster-person checked"><span class="roster-status">✓</span><span class="roster-person-copy"><strong>{entry.studentName}</strong><small>{entry.status === 'helper' ? 'Helper check-in' : 'Checked in'}{#if entry.checkedInAtUtc} · {new Date(entry.checkedInAtUtc).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })}{/if}</small></span>{#if rosterStaffAccess && entry.attendanceId !== undefined}<button class="roster-remove-button" type="button" disabled={rosterRemovingId === entry.attendanceId} on:click={() => removeRosterEntry(entry)}>{rosterRemovingId === entry.attendanceId ? 'Removing…' : 'Remove'}</button>{/if}</div>{:else}<p class="muted">No students checked in yet.</p>{/each}</div>{#if rosterError}<p class="roster-error" role="alert">{rosterError}</p>{/if}{/if}</aside></div></section>

    <section class="schedule-future-board"><div class="schedule-live-heading"><div><span class="control-kicker">VIEW ONLY</span><h2>Future schedule</h2><p class="muted">Choose a future day to plan. Check-in only opens for today's sessions.</p></div><label class="future-program-filter">Program type<select bind:value={futureProgramType}><option>All program types</option><option>Kids</option><option>Teen/Adult</option><option>IS3</option><option>Other</option></select></label></div><div class="future-date-row">{#each dates.filter((value) => value[0] !== todayKey) as [key, value]}<button class:selected={selectedDate === key} class="future-date" type="button" on:click={() => selectedDate = key}><strong>{dayLabel(value, { weekday: 'short' })}</strong><span>{dayLabel(value, { month: 'short', day: 'numeric' })}</span></button>{/each}</div>{#if selectedDate !== todayKey}<div class="future-class-list">{#each futureGroups as group}<article class="future-class-card"><div><span class="class-program">{programName(group.primary)}</span><h3>{groupName(group)}</h3><p>{timeLabel(group.primary)} · {group.primary.location ?? 'Main dojo'}</p></div><span class="pill">View only</span></article>{:else}<p class="muted">No classes match that program type on this day.</p>{/each}</div>{/if}</section>
  {/if}
  {#if action}<p class="schedule-feedback" role="status">{action}</p>{/if}

  {#if selectedClass}<div class="schedule-modal-backdrop" role="presentation" on:click={closeCheckIn}><div class:modal-shake={modalShake} class="schedule-modal" role="dialog" aria-modal="true" aria-labelledby="check-in-title" tabindex="-1" on:click|stopPropagation on:keydown|stopPropagation><button class="modal-close" type="button" aria-label="Close check-in" on:click={closeCheckIn}>×</button><span class="control-kicker">{actionMode === 'pre-enroll' ? 'NEXT CLASS RESERVATION' : 'TODAY\'S CHECK-IN'}</span><div class="check-in-stepper" aria-label={`Step ${modalStep} of 2`}><span class:active={modalStep === 1}>1. Student</span><span class:active={modalStep === 2}>2. Confirm</span></div><h2 id="check-in-title">{selectedGroup ? groupName(selectedGroup) : displayName(selectedClass)}</h2><p class="muted">{programName(selectedClass)} · {timeLabel(selectedClass)} · {selectedClass.location ?? 'Main dojo'}</p>{#if actionMode === 'pre-enroll'}<p class="modal-guidance">This is the next class at the dojo. Reserving a spot does not check you in; check-in opens when class starts.</p>{/if}{#if modalStep === 1}<label>Find the student<input type="search" bind:value={studentSearch} placeholder="Type at least 2 letters…" autocomplete="off" /></label>{#if normalizedStudentSearch.length < 2}<p class="check-in-search-hint">Start typing a first or last name. Students who are not eligible for this belt track will be stopped before PIN entry.</p>{/if}<div class="public-student-results">{#if matchingStudents.length}{#each matchingStudents as student}<button class:ineligible-student={!canStudentUseClass(student)} class:selected={selectedStudent?.studentId === student.studentId} type="button" on:click={() => selectStudent(student)}><span class="mini-avatar">{student.displayName.split(' ').map((part) => part[0]).join('').slice(0, 2)}</span><span><strong>{student.displayName}</strong><small>{student.rankName ?? student.is3LevelName ?? 'Rank not assigned'}</small>{#if !canStudentUseClass(student)}<small class="eligibility-note">Not eligible for this class</small>{/if}</span></button>{/each}{:else if normalizedStudentSearch.length >= 2}<p class="muted">No students match “{studentSearch}”.</p>{/if}</div>{#if eligibilityError}<div class="check-in-inline-error" role="alert">{eligibilityError}</div>{/if}<button class="primary-button wide check-in-next-button" type="button" on:click={continueToConfirmation} disabled={!selectedStudent || Boolean(eligibilityError)}>Continue to confirmation →</button>{:else}<div class="check-in-confirmation"><div class="check-in-selected-student"><span class="mini-avatar">{selectedStudent?.displayName.split(' ').map((part) => part[0]).join('').slice(0, 2)}</span><span><strong>{selectedStudent?.displayName}</strong><small>{displayName(selectedClass)} · {selectedStudent?.rankName ?? selectedStudent?.is3LevelName ?? 'Rank not assigned'}</small></span></div>{#if actionMode === 'check-in'}<label class="attendance-mode-label" for="attendance-mode"><strong>Attendance type</strong><select id="attendance-mode" bind:value={attendanceMode}><option value="regular">Regular attendance</option><option value="helper">Check in as helper</option></select></label>{#if attendanceMode === 'helper'}<p>Helper check-in is only accepted when the student outranks every belt in this class.</p>{/if}{/if}<label for="check-in-pin"><strong>Student PIN</strong><input id="check-in-pin" type="password" bind:value={checkInPin} minlength="4" maxlength="6" pattern="[0-9][0-9][0-9][0-9][0-9]?[0-9]?" inputmode="numeric" placeholder="Enter 4–6 digit PIN" /></label>{#if modalError}<div class="check-in-inline-error" role="alert">{modalError}</div>{/if}<div class="check-in-confirm-actions"><button class="outline-button" type="button" on:click={backToStudentSearch} disabled={checkInBusy}>← Back</button><button class="primary-button" type="button" on:click={checkIn} disabled={checkInBusy || !checkInPin.trim()}>{checkInBusy ? 'Recording…' : actionMode === 'pre-enroll' ? 'Reserve next class' : attendanceMode === 'helper' ? 'Confirm helper check-in' : 'Confirm and check in'}</button></div></div>{/if}</div></div>{/if}
</main>
