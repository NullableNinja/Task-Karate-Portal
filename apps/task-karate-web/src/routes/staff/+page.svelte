<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type PortalStudent = { studentId: number; displayName: string; status: string; programs: string[]; totalClasses: number; achievementCount: number; goldStarCount: number; attendanceStreak: number };
  type PortalGuardian = { isActive: boolean };
  type PortalProgram = { programId: number; name: string; isActive?: boolean };
  type PortalContent = { status: string; title?: string; publishedAtUtc?: string | null };
  type Session = { sessionId: number; className: string; sessionDate: string; startTime?: string | null; cancelled: boolean; attendanceCount?: number };
  type ReportSummary = { activeStudents: number; pausedStudents: number; sessionsThisWeek: number; attendanceThisMonth: number; checkInsThisMonth: number; openSocialReports: number };

  let students: PortalStudent[] = [];
  let guardians: PortalGuardian[] = [];
  let programs: PortalProgram[] = [];
  let announcements: PortalContent[] = [];
  let sessions: Session[] = [];
  let reportSummary: ReportSummary | null = null;
  let error = '';
  let loading = true;

  $: activeStudents = students.filter((student) => student.status === 'active');
  $: attentionStudents = activeStudents.filter((student) => student.attendanceStreak === 0).slice(0, 6);
  $: todaySessions = sessions.filter((session) => !session.cancelled).slice(0, 8);

  onMount(async () => {
    try {
      [students, guardians, programs, announcements, reportSummary] = await Promise.all([
        api<PortalStudent[]>('/api/portal-admin/students'),
        api<PortalGuardian[]>('/api/portal-admin/guardians'),
        api<PortalProgram[]>('/api/portal-admin/programs'),
        api<PortalContent[]>('/api/portal-admin/announcements'),
        api<ReportSummary>('/api/portal-admin/reports/summary')
      ]);
    } catch (e) { error = e instanceof Error ? e.message : 'Could not load dashboard.'; }
    finally { loading = false; }
  });

  onMount(async () => {
    try { sessions = await api<Session[]>(`/api/portal-admin/sessions?date=${new Date().toISOString().slice(0, 10)}`); }
    catch { /* The rest of the dashboard remains useful if today's schedule is unavailable. */ }
  });
</script>

<svelte:head><title>Staff dashboard | TASK Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">TODAY · STAFF OVERVIEW</span><h2>Run the dojo from one clear desk.</h2><p class="muted">The admin console is the operational source for the portal. Start with what needs attention, then jump directly into the record or workflow.</p></div><div class="toolbar-actions"><a class="button" href="/staff/students">Find a student</a><a class="button secondary" href="/staff/attendance">Take attendance</a></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}
{#if loading}<section class="card loading-card"><p class="muted">Loading operational counts…</p></section>{:else}
  <div class="metric-grid admin-metric-grid"><section class="card metric"><strong>{reportSummary?.activeStudents ?? activeStudents.length}</strong><span>Active students</span><small>Canonical portal records</small></section><section class="card metric"><strong>{reportSummary?.pausedStudents ?? students.filter((student) => student.status === 'paused').length}</strong><span>Paused students</span><small>Retained, not enrollable</small></section><section class="card metric"><strong>{guardians.length}</strong><span>Families</span><small>Guardian records</small></section><section class="card metric"><strong>{programs.filter((program) => program.isActive !== false).length}</strong><span>Programs</span><small>Configured offerings</small></section><section class="card metric"><strong>{todaySessions.length}</strong><span>Today's classes</span><small>Scheduled sessions</small></section><section class="card metric"><strong>{announcements.filter((item) => item.status === 'Published').length}</strong><span>Live notices</span><small>Published announcements</small></section></div>
  <div class="admin-dashboard-grid"><section class="card admin-panel"><div class="section-heading"><div><span class="eyebrow">TODAY'S OPERATIONS</span><h2>Class sessions</h2></div><a class="text-link" href="/staff/classes">Manage schedule →</a></div>{#if todaySessions.length}<div class="admin-session-list">{#each todaySessions as session}<a class="admin-session-row" href="/staff/attendance"><span class="admin-session-time">{session.startTime ?? 'Time TBD'}</span><span><strong>{session.className}</strong><small>{session.attendanceCount ?? 0} attendance records</small></span><span class="pill">Attendance</span></a>{/each}</div>{:else}<div class="empty-state"><strong>No sessions loaded for today.</strong><p class="muted">Create today's occurrences from Programs & classes, then take attendance here.</p><a class="button secondary small-button" href="/staff/classes">Open class planner</a></div>{/if}</section><section class="card admin-panel"><div class="section-heading"><div><span class="eyebrow">FOLLOW-UP QUEUE</span><h2>Students to review</h2></div><a class="text-link" href="/staff/students">Open roster →</a></div>{#if attentionStudents.length}<div class="admin-attention-list">{#each attentionStudents as student}<a href="/staff/students"><span class="admin-avatar">{student.displayName.slice(0, 1)}</span><span><strong>{student.displayName}</strong><small>{student.totalClasses} recorded classes · no current streak</small></span><span>→</span></a>{/each}</div>{:else}<div class="empty-state"><strong>The follow-up queue is clear.</strong><p class="muted">Students with no current attendance streak will appear here.</p></div>{/if}</section></div>
  <section class="card"><div class="section-heading"><div><span class="eyebrow">PULSE REPORT</span><h2>What needs attention this month</h2></div><a class="text-link" href="/staff/audit">Review audit history →</a></div><div class="schedule-grid"><a class="schedule-item" href="/staff/attendance"><b>{reportSummary?.attendanceThisMonth ?? 0} attendance records</b><small>Present or helper records this calendar month · {reportSummary?.sessionsThisWeek ?? 0} sessions in the last completed week.</small></a><a class="schedule-item" href="/staff/check-ins"><b>{reportSummary?.checkInsThisMonth ?? 0} studio check-ins</b><small>Daily HIYAH! visits recorded this month across the dojo.</small></a><a class="schedule-item" href="/staff/social"><b>{reportSummary?.openSocialReports ?? 0} open safety reports</b><small>Review community reports before they become a blind spot.</small></a></div></section>
  <section class="card"><div class="section-heading"><div><span class="eyebrow">WORKFLOWS</span><h2>Common staff actions</h2></div><span class="muted">Every feature remains backed by the existing portal database.</span></div><div class="schedule-grid"><a class="schedule-item" href="/staff/students"><b>Manage people</b><small>Search, create, edit, pause, deactivate, or restore student records.</small></a><a class="schedule-item" href="/staff/classes"><b>Configure programs and classes</b><small>Set up recurring classes and create dated session occurrences.</small></a><a class="schedule-item" href="/staff/attendance"><b>Record attendance</b><small>Choose a class session, review the roster, and record attendance safely.</small></a><a class="schedule-item" href="/staff/documents"><b>Track documents & waivers</b><small>Define versions, see outstanding forms, and record verified acceptance.</small></a><a class="schedule-item" href="/staff/social"><b>Review community activity</b><small>Moderate posts, comments, and safety reports.</small></a><a class="schedule-item" href="/staff/configuration"><b>Configure recognition</b><small>Manage achievement definitions, missions, and rank progression.</small></a></div></section>
{/if}
