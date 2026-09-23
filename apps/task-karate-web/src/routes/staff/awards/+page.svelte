<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type PortalStudent = { studentId: number; displayName: string; rankName?: string; ageGroup?: string; isActive: boolean; totalClasses: number; attendanceStreak: number; achievementCount: number; goldStarCount: number; programs: string[] };
  type GoldStarEvent = { eventId: number; name: string; description: string; eventDate?: string; isActive: boolean; awardCount: number };
  type Achievement = { name: string; description: string; iconName: string; awardCount: number };

  let students: PortalStudent[] = [];
  let events: GoldStarEvent[] = [];
  let achievements: Achievement[] = [];
  let selectedStudent = '';
  let selectedEvent = '';
  let note = '';
  let eventName = '';
  let eventDescription = '';
  let eventDate = '';
  let error = '';
  let notice = '';
  let loading = true;
  let busy = false;

  async function load() { loading = true; error = ''; try { [students, events, achievements] = await Promise.all([api<PortalStudent[]>('/api/portal-admin/students'), api<GoldStarEvent[]>('/api/portal-admin/gold-star-events'), api<Achievement[]>('/api/portal-admin/achievements')]); if (!selectedStudent && students[0]) selectedStudent = String(students[0].studentId); if (!selectedEvent && events.find((item) => item.isActive)) selectedEvent = String(events.find((item) => item.isActive)?.eventId ?? ''); } catch (e) { error = e instanceof Error ? e.message : 'Could not load portal awards.'; } finally { loading = false; } }
  async function createEvent() { busy = true; error = ''; notice = ''; try { await api('/api/portal-admin/gold-star-events', { method: 'POST', body: JSON.stringify({ name: eventName.trim(), description: eventDescription.trim(), eventDate: eventDate || null }) }); eventName = ''; eventDescription = ''; eventDate = ''; notice = 'Gold Star event created.'; await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not create event.'; } finally { busy = false; } }
  async function award() { busy = true; error = ''; notice = ''; try { await api(`/api/portal-admin/gold-star-events/${selectedEvent}/award`, { method: 'POST', body: JSON.stringify({ studentId: Number(selectedStudent), note: note.trim() || null }) }); note = ''; notice = 'Gold Star awarded and added to the news feed.'; await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not award Gold Star.'; } finally { busy = false; } }
  async function recalculate() { busy = true; error = ''; try { const result = await api<{ awarded: number }>('/api/portal-admin/milestones/recalculate', { method: 'POST' }); notice = `${result.awarded} automatic milestone${result.awarded === 1 ? '' : 's'} awarded.`; await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not recalculate milestones.'; } finally { busy = false; } }
  onMount(load);
</script>

<svelte:head><title>Staff · Awards | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">RECOGNITION</span><h2>Gold Stars & milestones</h2><p class="muted">Staff-only controls for earned recognition. Students never enroll in or award events from their hub.</p></div><button class="button secondary small-button" type="button" on:click={recalculate} disabled={busy}>Recalculate automatic milestones</button></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}
{#if loading}<section class="card"><p class="muted">Loading recognition records…</p></section>{:else}<div class="split"><section class="card"><span class="eyebrow">STAFF AWARD</span><h3>Award a Gold Star</h3><form class="form-grid" on:submit|preventDefault={award}><label class="full">Student<select bind:value={selectedStudent} required>{#each students.filter((student) => student.isActive) as student}<option value={student.studentId}>{student.displayName} · {student.rankName ?? 'Rank not assigned'}</option>{/each}</select></label><label class="full">Event<select bind:value={selectedEvent} required>{#each events.filter((event) => event.isActive) as event}<option value={event.eventId}>{event.name}</option>{/each}</select></label><label class="full">Staff note (optional)<textarea bind:value={note} maxlength="500" placeholder="What did the student demonstrate or participate in?"></textarea></label><div class="full"><button class="button" disabled={busy || !selectedStudent || !selectedEvent}>{busy ? 'Saving…' : 'Award Gold Star'}</button></div></form></section><section class="card"><span class="eyebrow">NEW EVENT</span><h3>Create a Gold Star event</h3><p class="muted">Events are internal staff records. Students only see events after they are awarded.</p><form class="form-grid" on:submit|preventDefault={createEvent}><label class="full">Event name<input bind:value={eventName} maxlength="160" required /></label><label class="full">Description<textarea bind:value={eventDescription} maxlength="2000" required></textarea></label><label>Date<input type="date" bind:value={eventDate} /></label><div class="full"><button class="button" disabled={busy || !eventName.trim() || !eventDescription.trim()}>{busy ? 'Saving…' : 'Create event'}</button></div></form></section></div>
<section class="card"><div class="section-heading"><div><span class="eyebrow">PORTAL DATABASE</span><h3>Student recognition</h3></div><span class="pill">{students.length} students</span></div><div class="table-wrap"><table><thead><tr><th>Student</th><th>Programs</th><th>Classes</th><th>Streak</th><th>Achievements</th><th>Gold Stars</th></tr></thead><tbody>{#each students as student}<tr><td><strong>{student.displayName}</strong><small class="table-subtext">{student.rankName ?? 'Rank not assigned'}</small></td><td>{student.programs.join(', ') || 'No program'}</td><td>{student.totalClasses}</td><td>{student.attendanceStreak} days</td><td>{student.achievementCount}</td><td>{student.goldStarCount}</td></tr>{:else}<tr><td colspan="6" class="muted">No portal students found.</td></tr>{/each}</tbody></table></div></section>
<section class="card"><div class="section-heading"><div><span class="eyebrow">AUTOMATIC RULES</span><h3>Milestones currently supported</h3></div></div><div class="schedule-grid">{#each achievements as achievement}<div class="schedule-item"><b>{achievement.name}</b><small>{achievement.description} · {achievement.awardCount} awarded</small></div>{/each}</div></section>{/if}
