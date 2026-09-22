<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import type { Content } from '$lib/types';
  let schedule: any[] = []; let announcements: Content[] = []; let error = ''; let loading = true;
  onMount(async () => { try { [schedule, announcements] = await Promise.all([api<any[]>('/api/student/public/schedule'), api<Content[]>('/api/public/announcements')]); } catch (e) { error = e instanceof Error ? e.message : 'The public API is unavailable.'; } finally { loading = false; } });
</script>
<svelte:head><title>TASK Karate | Schedule and announcements</title></svelte:head>
<main class="public-shell">
  <header class="public-header"><div><span class="eyebrow">TASK KARATE SCHOOL</span><h1>Train with intention.</h1><p>Public schedule and current dojo announcements.</p></div><a class="button secondary" href="/staff/signin">Staff sign-in</a></header>
  {#if loading}<p class="status">Loading current information…</p>{:else if error}<div class="error"><strong>Live data unavailable.</strong><p>{error}</p></div>{:else}
    <section class="card"><div class="section-heading"><div><span class="eyebrow">UPCOMING</span><h2>Class schedule</h2></div><a class="button" href="/schedule">Student schedule</a></div>{#if schedule.length === 0}<p class="muted">No published sessions are available yet.</p>{:else}<div class="schedule-grid">{#each schedule as item}<article class="schedule-item"><strong>{new Date(item.sessionDate).toLocaleDateString()}</strong><span>{item.startTime ?? 'Time to be announced'}</span><b>{item.className}</b><small>{item.location ?? 'Task Karate dojo'}</small></article>{/each}</div>{/if}</section>
    <section class="card"><div class="section-heading"><div><span class="eyebrow">DOJO NEWS</span><h2>Announcements</h2></div></div>{#if announcements.length === 0}<p class="muted">No published announcements.</p>{:else}{#each announcements as item}<article class="announcement"><h3>{item.title}</h3><p>{item.body}</p><small>{item.publishedAtUtc ? new Date(item.publishedAtUtc).toLocaleDateString() : ''}</small></article>{/each}{/if}</section>
  {/if}
</main>
