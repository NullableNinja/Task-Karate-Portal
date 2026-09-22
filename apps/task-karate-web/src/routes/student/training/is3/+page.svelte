<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  import { is3LevelGuides } from '$lib/is3-levels';

  let session: StudentSession | null = null;
  let profile: any = null;
  let error = '';
  onMount(async () => { session = await requireStudent(); if (!session) return; try { profile = await api('/api/student/profile'); } catch (e) { error = apiError(e); } });
  $: is3Program = profile?.programs?.find((program: any) => program.programCode === 'is3');
  $: guide = is3LevelGuides[is3Program?.levelName ?? 'IS3 Student Level 0'] ?? is3LevelGuides['IS3 Student Level 0'];
</script>

<svelte:head><title>Task Karate | IS3 requirements</title></svelte:head>

{#if session}<StudentShell {session} active="training">
  <section class="student-heading"><span class="student-eyebrow">TRAINING TRACK · IS3</span><h1>IS3 LEVEL TRACK</h1><p>IS3 is a separate Teen / Adult program with its own level progression, movement vocabulary, and instructor-led testing path.</p></section>
  <nav class="training-track-nav" aria-label="Training tracks"><a href="/student/training/karate">Karate track</a>{#if is3Program}<a class="active" href="/student/training/is3">IS3 track</a>{/if}<a href="/student/training">Training overview</a></nav>
  {#if error}<div class="error" role="alert">{error}</div>{:else if !profile}<div class="student-panel loading-panel">Loading your IS3 track…</div>{:else if !is3Program}<section class="student-panel empty-panel"><span class="program-code large">IS3</span><h2>IS3 is not on your program record</h2><p>IS3 is available to Teen / Adult students. Ask staff if you would like to learn more about enrolling.</p><a class="outline-button" href="/student/training/karate">Return to Karate track</a></section>{:else}
    <section class="student-panel training-hero is3-track-hero"><div><span class="student-eyebrow">CURRENT LEVEL</span><h2>{guide.current}</h2><p>{guide.note}</p></div><div class="program-code large">IS3</div></section>
    <section class="student-panel is3-panel"><div class="panel-title"><span>NEXT LEVEL</span><span class="muted">Instructor confirmed</span></div><h2>{guide.next}</h2><p class="muted">Build these three areas with your instructor before moving forward.</p><div class="is3-focus-grid">{#each guide.focus as focus}<div><span>FOCUS</span><strong>{focus}</strong></div>{/each}</div></section>
    <section class="student-panel training-note-panel"><div class="panel-title"><span>IS3 PRACTICE NOTES</span><span class="muted">Separate from belt testing</span></div><p>IS3 progress is tracked by levels rather than belt colors. Your Karate track and IS3 track can progress together, but each has its own requirements and instructor check-ins.</p></section>
  {/if}
</StudentShell>{/if}
