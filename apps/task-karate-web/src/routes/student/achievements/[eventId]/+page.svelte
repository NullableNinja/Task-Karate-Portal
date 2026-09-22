<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';

  let session: StudentSession | null = null;
  let event: any = null;
  let error = '';
  let interestBusy = false;
  onMount(async () => {
    session = await requireStudent();
    if (!session) return;
    try { event = await api(`/api/student/gold-stars/${$page.params.eventId}`); } catch (e) { error = apiError(e); }
  });
  async function toggleInterest() { if (!event) return; interestBusy = true; try { const result = await api<{ interested: boolean }>(`/api/student/gold-stars/${event.eventId}/interest`, { method: 'POST' }); event = { ...event, interested: result.interested }; } catch (e) { error = apiError(e); } finally { interestBusy = false; } }
</script>

<svelte:head><title>Task Karate | Gold Star event</title></svelte:head>
{#if session}<StudentShell {session} active="achievements"><section class="student-heading"><a class="back-link student-back-link" href="/student/achievements">← Back to achievements</a><span class="student-eyebrow">GOLD-STAR EVENT</span><h1>{event?.name ?? 'EVENT DETAILS'}</h1><p>Special events are part of the training story students build together.</p></section>{#if error}<div class="error" role="alert">{error}</div>{:else if !event}<div class="student-panel loading-panel">Loading event details…</div>{:else}<section class="student-panel detail-panel"><div class="detail-icon gold-star-detail-icon">★</div><span class:awarded={event.awarded} class="detail-status">{event.awarded ? 'Gold star earned' : 'Event opportunity'}</span><h2>{event.name}</h2><p class="detail-description">{event.description}</p><div class="detail-facts"><div><span>Event date</span><strong>{event.eventDate ? new Date(event.eventDate).toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' }) : 'Date to be announced'}</strong></div><div><span>Recorded by</span><strong>Task Karate staff</strong></div></div>{#if !event.awarded}<button class="primary-button interest-button" type="button" disabled={interestBusy} on:click={toggleInterest}>{event.interested ? '✓ You’re on the interest list · Remove' : 'I’m interested in this event'}</button>{/if}<p class="muted">Gold Stars are awarded by the dojo after attendance is confirmed. If this event is marked as an opportunity, the interest list helps staff understand who plans to participate.</p></section>{/if}</StudentShell>{/if}
