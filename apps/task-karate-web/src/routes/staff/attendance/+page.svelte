<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import type { Session, Student } from '$lib/types';
  let students: Student[] = [];
  let sessions: Session[] = [];
  let records: any[] = [];
  let studentId = '';
  let classSessionId = '';
  let selectedDate = new Date().toISOString().slice(0, 10);
  let studentSearch = '';
  let error = '';
  let notice = '';
  let loading = true;
  let recording = false;

  async function load() { loading = true; error = ''; try { [students, sessions] = await Promise.all([api<Student[]>('/api/students'), api<Session[]>(`/api/sessions?date=${selectedDate}`)]); if (!sessions.some((session) => session.id === classSessionId)) { classSessionId = ''; records = []; } else { await selectSession(); } } catch (e) { error = e instanceof Error ? e.message : 'Could not load attendance.'; } finally { loading = false; } }
  $: visibleStudents = students.filter((student) => `${student.firstName} ${student.lastName} ${student.preferredName ?? ''}`.toLowerCase().includes(studentSearch.trim().toLowerCase()));
  async function selectSession() { if (!classSessionId) { records = []; return; } try { records = await api<any[]>(`/api/attendance?sessionId=${classSessionId}`); } catch (e) { error = e instanceof Error ? e.message : 'Could not load the attendance list.'; } }
  async function checkIn() { recording = true; error = ''; notice = ''; try { await api('/api/attendance', { method: 'POST', body: JSON.stringify({ studentId, classSessionId, notes: null }) }); notice = 'Attendance recorded.'; studentId = ''; studentSearch = ''; await selectSession(); } catch (e) { error = e instanceof Error ? e.message : 'Could not record attendance.'; } finally { recording = false; } }
  onMount(load);
</script>

<svelte:head><title>Staff · Attendance | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">MAT LOG</span><h2>Attendance check-in</h2><p class="muted">Choose the actual class date first, then record each student once.</p></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}
<section class="card"><div class="form-grid"><label>Date<input type="date" bind:value={selectedDate} on:change={load} /></label><label>Class session<select bind:value={classSessionId} on:change={selectSession} disabled={loading}><option value="">Select a session</option>{#each sessions as session}<option value={session.id}>{session.name} · {session.startTime} · {session.durationMinutes} min</option>{/each}</select></label><label>Find student<input bind:value={studentSearch} placeholder="Type a name…" /></label><label>Student<select bind:value={studentId} disabled={!classSessionId}><option value="">Select a student</option>{#each visibleStudents as student}<option value={student.id}>{student.preferredName || student.firstName} {student.lastName}</option>{/each}</select></label></div><button class="button" disabled={!classSessionId || !studentId || recording} on:click={checkIn}>{recording ? 'Recording…' : 'Record attendance'}</button></section>
<section class="card"><div class="section-heading"><h3>Checked in {classSessionId ? `· ${records.length}` : ''}</h3>{#if !sessions.length && !loading}<span class="pill">No sessions on this date</span>{/if}</div>{#if loading}<p class="muted">Loading sessions…</p>{:else if !classSessionId}<p class="muted">Select a session to see attendance.</p>{:else}<div class="table-wrap"><table><thead><tr><th>Student</th><th>Checked in</th><th>Notes</th></tr></thead><tbody>{#each records as record}<tr><td>{record.studentName}</td><td>{new Date(record.checkedInAtUtc).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })}</td><td>{record.notes || '—'}</td></tr>{:else}<tr><td colspan="3" class="muted">No attendance recorded for this session.</td></tr>{/each}</tbody></table></div>{/if}</section>
