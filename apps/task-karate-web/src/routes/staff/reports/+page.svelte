<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type Summary = { activeStudents: number; pausedStudents: number; sessionsThisWeek: number; attendanceThisMonth: number; checkInsThisMonth: number; openSocialReports: number };
  type Leaderboard = { periodLabel: string; entries: { rank: number; displayName: string; checkIns: number }[] };
  type Attendance = { attendanceId: number; studentName: string; sessionDate: string; className: string; status: string; checkedInAtUtc: string };
  type Document = { documentId: number; name: string; version: string; isActive: boolean; requiredForEnrollment: boolean; acceptanceCount: number };
  let summary: Summary | null = null;
  let leaderboard: Leaderboard | null = null;
  let attendance: Attendance[] = [];
  let documents: Document[] = [];
  let from = new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().slice(0, 10);
  let to = new Date(Date.now() + 86400000).toISOString().slice(0, 10);
  let loading = true;
  let error = '';

  async function load() {
    loading = true; error = '';
    try {
      [summary, leaderboard, attendance, documents] = await Promise.all([
        api<Summary>('/api/portal-admin/reports/summary'),
        api<Leaderboard>('/api/portal-admin/check-ins/leaderboard?period=month'),
        api<Attendance[]>(`/api/portal-admin/attendance/history?from=${from}&to=${to}`),
        api<Document[]>('/api/portal-admin/documents')
      ]);
    } catch (e) { error = e instanceof Error ? e.message : 'Could not load reports.'; }
    finally { loading = false; }
  }

  $: outstandingRequired = documents.filter((item) => item.isActive && item.requiredForEnrollment && item.acceptanceCount === 0);
  onMount(load);
</script>

<svelte:head><title>Staff · Reports | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">OPERATIONS · REPORTING</span><h2>Reports</h2><p class="muted">A compact view of participation, check-in momentum, attendance activity, and document coverage.</p></div><div class="toolbar-actions"><label>Attendance from<input type="date" bind:value={from} /></label><label>to<input type="date" bind:value={to} /></label><button class="button secondary" type="button" on:click={load} disabled={loading}>{loading ? 'Loading…' : 'Refresh reports'}</button></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}
{#if loading}<section class="card"><p class="muted">Loading operational reports…</p></section>{:else}<div class="metric-grid admin-metric-grid"><section class="card metric"><strong>{summary?.attendanceThisMonth ?? 0}</strong><span>Attendance this month</span><small>Present and helper records</small></section><section class="card metric"><strong>{summary?.checkInsThisMonth ?? 0}</strong><span>Studio check-ins</span><small>Calendar-month HIYAH! visits</small></section><section class="card metric"><strong>{summary?.sessionsThisWeek ?? 0}</strong><span>Sessions last week</span><small>Non-cancelled class occurrences</small></section><section class="card metric"><strong>{summary?.openSocialReports ?? 0}</strong><span>Open social reports</span><small>Safety queue requiring review</small></section></div><div class="admin-dashboard-grid"><section class="card"><div class="section-heading"><div><span class="eyebrow">CHECK-IN CONTEST</span><h3>{leaderboard?.periodLabel ?? 'This month'}</h3><p class="muted">Monthly dojo check-in rankings. The server enforces one check-in per student per calendar day.</p></div><a class="text-link" href="/staff/check-ins">Manage check-ins →</a></div>{#if leaderboard?.entries.length}<ol class="report-ranking">{#each leaderboard.entries.slice(0, 10) as entry}<li><span>{entry.rank}</span><strong>{entry.displayName}</strong><b>{entry.checkIns}</b></li>{/each}</ol>{:else}<p class="muted">No check-ins in this period.</p>{/if}</section><section class="card"><div class="section-heading"><div><span class="eyebrow">DOCUMENT COVERAGE</span><h3>Operational readiness</h3></div><a class="text-link" href="/staff/documents">Manage forms →</a></div>{#if documents.length}<div class="report-document-list">{#each documents.filter((item) => item.isActive) as item}<div><span><strong>{item.name}</strong><small>v{item.version}{item.requiredForEnrollment ? ' · required' : ' · optional'}</small></span><b>{item.acceptanceCount} accepted</b></div>{/each}</div>{:else}<p class="muted">No document definitions configured.</p>{/if}{#if outstandingRequired.length}<p class="detail-callout"><strong>{outstandingRequired.length}</strong> required document definition{outstandingRequired.length === 1 ? '' : 's'} have no recorded acceptance yet. Individual student coverage is available from Student 360.</p>{/if}</section></div><section class="card"><div class="section-heading"><div><span class="eyebrow">ATTENDANCE ACTIVITY</span><h3>Recent records in selected range</h3></div><span class="pill">{attendance.length}</span></div><div class="table-wrap"><table><thead><tr><th>Date / class</th><th>Student</th><th>Status</th><th>Recorded</th></tr></thead><tbody>{#each attendance.slice(0, 50) as item}<tr><td><strong>{item.sessionDate.slice(0, 10)}</strong><span class="table-subtext">{item.className}</span></td><td>{item.studentName}</td><td><span class="pill">{item.status}</span></td><td>{new Date(item.checkedInAtUtc).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' })}</td></tr>{:else}<tr><td colspan="4" class="muted">No attendance records in this range.</td></tr>{/each}</tbody></table></div></section>{/if}
