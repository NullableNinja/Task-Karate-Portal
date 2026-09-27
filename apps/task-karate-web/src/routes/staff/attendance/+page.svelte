<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { api } from '$lib/api';
  import { formatDojoRange } from '$lib/time';

  type PortalStudent = { studentId: number; displayName: string; rankName?: string; isActive: boolean; programs: string[] };
  type Session = { sessionId: number; sessionDate: string; startTime?: string; endTime?: string; className: string; location?: string; cancelled: boolean };
  type AttendanceRecord = { attendanceId: number; studentId: number; studentName: string; checkedInAtUtc: string; notes?: string; status: string };
  type AttendanceHistoryItem = { attendanceId: number; studentId: number; studentName: string; sessionId: number; sessionDate: string; className: string; checkedInAtUtc: string; status: string; notes?: string };
  let students: PortalStudent[] = [];
  let sessions: Session[] = [];
  let records: AttendanceRecord[] = [];
  let history: AttendanceHistoryItem[] = [];
  let studentId = '';
  let classSessionId = '';
  let selectedDate = new Date().toISOString().slice(0, 10);
  let studentSearch = '';
  let error = '';
  let notice = '';
  let loading = true;
  let recording = false;
  let helperMode = false;
  let historyFrom = new Date(Date.now() - 29 * 86400000).toISOString().slice(0, 10);
  let historyTo = new Date().toISOString().slice(0, 10);
  let historyLoading = false;
  let correcting = 0;

  async function load() {
    loading = true; error = ''; notice = '';
    try {
      [students, sessions] = await Promise.all([
        api<PortalStudent[]>('/api/portal-admin/attendance/students'),
        api<Session[]>(`/api/portal-admin/attendance/sessions?date=${selectedDate}`),
        loadHistory()
      ]);
      const requestedStudentId = $page.url.searchParams.get('studentId');
      if (requestedStudentId && students.some((student) => String(student.studentId) === requestedStudentId)) {
        studentSearch = students.find((student) => String(student.studentId) === requestedStudentId)?.displayName ?? '';
      }
      if (!sessions.some((session) => String(session.sessionId) === classSessionId)) { classSessionId = ''; records = []; }
      else await selectSession();
    } catch (e) { error = e instanceof Error ? e.message : 'Could not load attendance.'; }
    finally { loading = false; }
  }

  $: visibleStudents = students.filter((student) => student.isActive && `${student.displayName} ${student.rankName ?? ''} ${student.programs.join(' ')}`.toLowerCase().includes(studentSearch.trim().toLowerCase()));
  $: selectedSession = sessions.find((session) => String(session.sessionId) === classSessionId);
  async function selectSession() { if (!classSessionId) { records = []; return; } try { records = await api<AttendanceRecord[]>(`/api/portal-admin/attendance?sessionId=${classSessionId}`); } catch (e) { error = e instanceof Error ? e.message : 'Could not load the attendance list.'; } }
  async function loadHistory() { historyLoading = true; try { history = await api<AttendanceHistoryItem[]>(`/api/portal-admin/attendance/history?from=${historyFrom}&to=${historyTo}`); } catch (e) { error = e instanceof Error ? e.message : 'Could not load attendance history.'; } finally { historyLoading = false; } }
  async function correctAttendance(item: AttendanceHistoryItem, status: string) { correcting = item.attendanceId; error = ''; notice = ''; try { await api(`/api/portal-admin/attendance/${item.attendanceId}/status`, { method: 'POST', body: JSON.stringify({ status, notes: item.notes ?? null }) }); notice = `${item.studentName}'s attendance was updated.`; await Promise.all([loadHistory(), classSessionId ? selectSession() : Promise.resolve()]); } catch (e) { error = e instanceof Error ? e.message : 'Could not correct attendance.'; } finally { correcting = 0; } }
  async function checkIn() { recording = true; error = ''; notice = ''; try { await api('/api/portal-admin/attendance', { method: 'POST', body: JSON.stringify({ studentId: Number(studentId), sessionId: Number(classSessionId), helper: helperMode }) }); notice = helperMode ? 'Helper attendance recorded in the portal database.' : 'Attendance recorded in the portal database.'; studentId = ''; studentSearch = ''; helperMode = false; await selectSession(); } catch (e) { error = e instanceof Error ? e.message : 'Could not record attendance.'; } finally { recording = false; } }
  onMount(load);
</script>

<svelte:head><title>Staff · Attendance | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">MAT LOG</span><h2>Attendance check-in</h2><p class="muted">Use the three-step flow: choose the class date, choose the actual session, then check in each student who arrived.</p></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}
<section class="card attendance-workflow"><div class="workflow-step"><span class="workflow-number">1</span><div><h3>Choose the class date</h3><p class="muted">Only sessions on this date appear in the next step.</p><label>Date<input type="date" bind:value={selectedDate} on:change={load} /></label></div></div><div class="workflow-step"><span class="workflow-number">2</span><div class="workflow-grow"><h3>Choose the session</h3><p class="muted">This is the real class occurrence students are attending.</p><select bind:value={classSessionId} on:change={selectSession} disabled={loading || !sessions.length}><option value="">{loading ? 'Loading sessions…' : sessions.length ? 'Select a class session' : 'No sessions on this date'}</option>{#each sessions as session}<option value={session.sessionId}>{session.className} · {formatDojoRange(session.startTime, session.endTime)}{session.location ? ` · ${session.location}` : ''}{session.cancelled ? ' · Cancelled' : ''}</option>{/each}</select></div></div><div class="workflow-step"><span class="workflow-number">3</span><div class="workflow-grow"><h3>Check in the student</h3><p class="muted">Search narrows the roster; the second field is the student you will record.</p><div class="attendance-student-controls"><input bind:value={studentSearch} placeholder="Find a student by name…" aria-label="Find a student by name" disabled={!classSessionId} /><select bind:value={studentId} disabled={!classSessionId || !visibleStudents.length} aria-label="Student to check in"><option value="">{visibleStudents.length ? 'Select student to check in' : 'No matching active students'}</option>{#each visibleStudents as student}<option value={student.studentId}>{student.displayName} · {student.rankName ?? 'Rank not assigned'}</option>{/each}</select></div>{#if studentId}<label class="helper-choice"><input type="checkbox" bind:checked={helperMode} /><span><strong>Record as helper</strong><small>The API confirms this student outranks the selected class.</small></span></label>{/if}{#if !students.length && !loading}<p class="empty-help">No active portal students are available. <a href="/staff/students">Add or activate a student first.</a></p>{:else if !visibleStudents.length && studentSearch}<p class="empty-help">No active students match “{studentSearch}”. Clear the search to see the roster.</p>{/if}</div></div><div class="workflow-actions"><button class="button attendance-submit" type="button" on:click={checkIn} disabled={!classSessionId || !studentId || recording || selectedSession?.cancelled}>{recording ? 'Recording…' : helperMode ? 'Record helper attendance' : 'Record attendance'}</button>{#if selectedSession?.cancelled}<span class="muted">Cancelled sessions cannot accept check-ins.</span>{/if}</div></section>
{#if !sessions.length && !loading}<section class="card empty-state"><h3>No class sessions on {new Date(`${selectedDate}T12:00:00`).toLocaleDateString()}</h3><p class="muted">Attendance cannot be recorded until a dated session exists. If this is the correct date, open Classes and create the occurrence from a class template.</p><a class="button" href="/staff/classes">Prepare class sessions</a></section>{/if}
<section class="card"><div class="section-heading"><div><span class="eyebrow">CURRENT ROSTER</span><h3>Checked in{selectedSession ? ` · ${selectedSession.className}` : ''}</h3></div><span class="pill">{records.length} present</span></div>{#if loading}<p class="muted">Loading portal attendance…</p>{:else if !classSessionId}<p class="muted">Select a session above to see who is already checked in.</p>{:else}<div class="table-wrap"><table><thead><tr><th>Student</th><th>Type</th><th>Checked in</th><th>Notes</th></tr></thead><tbody>{#each records as record}<tr><td><strong>{record.studentName}</strong></td><td>{record.status === 'helper' ? 'Helper' : 'Present'}</td><td>{new Date(record.checkedInAtUtc).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })}</td><td>{record.notes || '—'}</td></tr>{:else}<tr><td colspan="4" class="muted">No students checked in yet. Use step 3 above after the student arrives.</td></tr>{/each}</tbody></table></div>{/if}</section>
<section class="card"><div class="section-heading"><div><span class="eyebrow">OPERATIONS HISTORY</span><h3>Review and correct attendance</h3><p class="muted">Corrections are recorded as status changes and remain visible in the audit history.</p></div><div class="inline-filters"><label>From<input type="date" bind:value={historyFrom} /></label><label>To<input type="date" bind:value={historyTo} /></label><button class="button secondary small-button" type="button" on:click={loadHistory} disabled={historyLoading}>{historyLoading ? 'Loading…' : 'Refresh history'}</button></div></div>{#if historyLoading}<p class="muted">Loading attendance history…</p>{:else}<div class="table-wrap"><table><thead><tr><th>Date / class</th><th>Student</th><th>Recorded</th><th>Status</th><th>Correction</th></tr></thead><tbody>{#each history as item}<tr><td><strong>{item.sessionDate.slice(0, 10)}</strong><span class="table-subtext">{item.className}</span></td><td>{item.studentName}</td><td>{new Date(item.checkedInAtUtc).toLocaleTimeString([], { hour: 'numeric', minute: '2-digit' })}</td><td><span class="pill">{item.status}</span></td><td><select value={item.status} on:change={(event) => correctAttendance(item, (event.currentTarget as HTMLSelectElement).value)} disabled={correcting === item.attendanceId}><option value="present">Present</option><option value="helper">Helper</option><option value="excused">Excused</option><option value="absent">Absent</option></select></td></tr>{:else}<tr><td colspan="5" class="muted">No attendance records in this date range.</td></tr>{/each}</tbody></table></div>{/if}</section>
