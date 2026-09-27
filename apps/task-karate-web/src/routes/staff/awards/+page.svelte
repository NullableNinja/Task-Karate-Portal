<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { api } from '$lib/api';

  type PortalStudent = { studentId: number; displayName: string; rankName?: string; ageGroup?: string; isActive: boolean; totalClasses: number; attendanceStreak: number; achievementCount: number; goldStarCount: number; programs: string[] };
  type GoldStarEvent = { eventId: number; name: string; description: string; eventDate?: string; isActive: boolean; awardCount: number };
  type Achievement = { name: string; description: string; iconName: string; awardCount: number };
  const tiers = ['All tiers', 'Bronze', 'Silver', 'Gold', 'Platinum', 'Diamond'];
  let students: PortalStudent[] = [];
  let events: GoldStarEvent[] = [];
  let achievements: Achievement[] = [];
  let selectedStudent = '';
  let selectedEvent = '';
  let studentSearch = '';
  let note = '';
  let awardDate = new Date().toISOString().slice(0, 10);
  let eventName = '';
  let eventDescription = '';
  let error = '';
  let notice = '';
  let loading = true;
  let busy = false;
  let milestoneSearch = '';
  let milestoneTier = 'All tiers';
  let milestoneSort = 'tier';

  function tierFor(item: Achievement) {
    const text = `${item.name} ${item.description}`.toLowerCase();
    if (/10,?000|5-year|10-year|180-day|black belt/.test(text)) return 'Diamond';
    if (/1,?000|2,?500|5,?000|500 classes|3-year|90-day/.test(text)) return 'Platinum';
    if (/250 classes|100 classes|50 classes|1-year|60-day/.test(text)) return 'Gold';
    if (/25 classes|30-day|14-day|10 classes|50 practice/.test(text)) return 'Silver';
    return 'Bronze';
  }

  async function load() {
    loading = true; error = '';
    try {
      [students, events, achievements] = await Promise.all([api<PortalStudent[]>('/api/portal-admin/students'), api<GoldStarEvent[]>('/api/portal-admin/gold-star-events'), api<Achievement[]>('/api/portal-admin/achievements')]);
      const requestedStudentId = $page.url.searchParams.get('studentId');
      if (requestedStudentId && students.some((student) => String(student.studentId) === requestedStudentId)) selectedStudent = requestedStudentId;
      if (!selectedStudent && students[0]) selectedStudent = String(students[0].studentId);
      if (!selectedEvent && events.find((item) => item.isActive)) selectedEvent = String(events.find((item) => item.isActive)?.eventId ?? '');
    } catch (e) { error = e instanceof Error ? e.message : 'Could not load portal awards.'; }
    finally { loading = false; }
  }

  async function createEvent() {
    busy = true; error = ''; notice = '';
    try { await api('/api/portal-admin/gold-star-events', { method: 'POST', body: JSON.stringify({ name: eventName.trim(), description: eventDescription.trim(), eventDate: null }) }); eventName = ''; eventDescription = ''; notice = 'Gold Star template created.'; await load(); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not create the Gold Star template.'; }
    finally { busy = false; }
  }

  async function award() {
    busy = true; error = ''; notice = '';
    try { await api(`/api/portal-admin/gold-star-events/${selectedEvent}/award`, { method: 'POST', body: JSON.stringify({ studentId: Number(selectedStudent), note: note.trim() || null, eventDate: awardDate || null }) }); note = ''; notice = 'Gold Star awarded with its event date recorded.'; await load(); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not award Gold Star.'; }
    finally { busy = false; }
  }

  async function recalculate() { busy = true; error = ''; try { const result = await api<{ awarded: number }>('/api/portal-admin/milestones/recalculate', { method: 'POST' }); notice = `${result.awarded} automatic milestone${result.awarded === 1 ? '' : 's'} awarded.`; await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not recalculate milestones.'; } finally { busy = false; } }

  $: matchingStudents = students.filter((student) => student.isActive && `${student.displayName} ${student.rankName ?? ''} ${student.programs.join(' ')}`.toLowerCase().includes(studentSearch.trim().toLowerCase()));
  $: visibleAchievements = achievements.filter((item) => (milestoneTier === 'All tiers' || tierFor(item) === milestoneTier) && `${item.name} ${item.description} ${item.iconName}`.toLowerCase().includes(milestoneSearch.trim().toLowerCase())).sort((a, b) => milestoneSort === 'name' ? a.name.localeCompare(b.name) : milestoneSort === 'awarded' ? b.awardCount - a.awardCount : `${tiers.indexOf(tierFor(a))}-${a.name}`.localeCompare(`${tiers.indexOf(tierFor(b))}-${b.name}`));
  onMount(load);
</script>

<svelte:head><title>Staff · Awards | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">RECOGNITION</span><h2>Gold Stars & milestones</h2><p class="muted">Award a Gold Star to a searchable student, record the date it was earned, and manage the milestone library without scrolling through an unstructured wall of awards.</p></div><button class="button secondary small-button" type="button" on:click={recalculate} disabled={busy}>Recalculate automatic milestones</button></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}
{#if loading}<section class="card"><p class="muted">Loading recognition records…</p></section>{:else}
<div class="split">
  <section class="card"><span class="eyebrow">STAFF AWARD</span><h3>Award a Gold Star</h3><form class="form-grid" on:submit|preventDefault={award}><label class="full">Find student<input type="search" bind:value={studentSearch} placeholder="Search by name, rank, or program" /></label><label class="full">Student<select bind:value={selectedStudent} required><option value="" disabled>Select student</option>{#each matchingStudents as student}<option value={student.studentId}>{student.displayName} · {student.rankName ?? 'Rank not assigned'}</option>{/each}</select></label><label class="full">Gold Star template<select bind:value={selectedEvent} required><option value="" disabled>Select template</option>{#each events.filter((event) => event.isActive) as event}<option value={event.eventId}>{event.name}</option>{/each}</select></label><label>Event date<input type="date" bind:value={awardDate} required /></label><label class="full">Staff note (optional)<textarea bind:value={note} maxlength="500" placeholder="What did the student demonstrate or participate in?"></textarea></label><div class="full"><button class="button" disabled={busy || !selectedStudent || !selectedEvent || !awardDate}>{busy ? 'Saving…' : 'Award Gold Star'}</button></div></form></section>
  <section class="card"><span class="eyebrow">GOLD STAR TEMPLATES</span><h3>Create a reusable Gold Star template</h3><p class="muted">Templates describe the kind of recognition. The actual event date belongs to each student’s award.</p><form class="form-grid" on:submit|preventDefault={createEvent}><label class="full">Template name<input bind:value={eventName} maxlength="160" required placeholder="Tournament participation" /></label><label class="full">Description<textarea bind:value={eventDescription} maxlength="2000" required placeholder="What qualifies for this recognition?"></textarea></label><div class="full"><button class="button" disabled={busy || !eventName.trim() || !eventDescription.trim()}>{busy ? 'Saving…' : 'Create Gold Star template'}</button></div></form></section>
</div>
<section class="card"><div class="section-heading"><div><span class="eyebrow">PORTAL DATABASE</span><h3>Student recognition</h3></div><span class="pill">{students.length} students</span></div><div class="table-wrap"><table><thead><tr><th>Student</th><th>Programs</th><th>Classes</th><th>Streak</th><th>Achievements</th><th>Gold Stars</th></tr></thead><tbody>{#each students as student}<tr><td><strong>{student.displayName}</strong><small class="table-subtext">{student.rankName ?? 'Rank not assigned'}</small></td><td>{student.programs.join(', ') || 'No program'}</td><td>{student.totalClasses}</td><td>{student.attendanceStreak} days</td><td>{student.achievementCount}</td><td>{student.goldStarCount}</td></tr>{:else}<tr><td colspan="6" class="muted">No portal students found.</td></tr>{/each}</tbody></table></div></section>
<section class="card"><div class="section-heading"><div><span class="eyebrow">AUTOMATIC RULES</span><h3>Milestones currently supported</h3><p class="muted">Filter by award tier or search the milestone name and description. Tiers are presentation groupings for quick scanning.</p></div><span class="pill">{visibleAchievements.length} shown / {achievements.length}</span></div><div class="milestone-filters"><label>Search<input type="search" bind:value={milestoneSearch} placeholder="Name, description, or icon" /></label><label>Tier<select bind:value={milestoneTier}>{#each tiers as tier}<option>{tier}</option>{/each}</select></label><label>Sort<select bind:value={milestoneSort}><option value="tier">Tier, then name</option><option value="name">Name</option><option value="awarded">Most awarded</option></select></label></div><div class="schedule-grid">{#each visibleAchievements as achievement}<div class={`schedule-item milestone-tier-${tierFor(achievement).toLowerCase()}`}><div class="milestone-heading"><b>{achievement.name}</b><span class="pill">{tierFor(achievement)}</span></div><small>{achievement.description} · {achievement.awardCount} awarded</small></div>{:else}<p class="muted">No milestones match those filters.</p>{/each}</div></section>
{/if}
