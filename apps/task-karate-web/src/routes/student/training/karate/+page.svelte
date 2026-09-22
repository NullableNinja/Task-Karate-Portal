<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  import { kidsTrainingPlans, nextRank, trainingPlans } from '$lib/training-requirements';

  let session: StudentSession | null = null;
  let profile: any = null;
  let error = '';
  onMount(async () => { session = await requireStudent(); if (!session) return; try { profile = await api('/api/student/profile'); } catch (e) { error = apiError(e); } });
  $: nextRankName = nextRank(profile?.rankName);
  $: plan = (profile?.ageGroup === 'kids' ? kidsTrainingPlans[nextRankName] : undefined) ?? trainingPlans[nextRankName] ?? trainingPlans['White Belt'];
  $: hasIs3 = Boolean(profile?.programs?.some((program: any) => program.programCode === 'is3'));
</script>

<svelte:head><title>Task Karate | Karate requirements</title></svelte:head>

{#if session}<StudentShell {session} active="training">
  <section class="student-heading"><span class="student-eyebrow">TRAINING TRACK · KARATE</span><h1>BELT TEST REQUIREMENTS</h1><p>Your next-rank testing sheet, organized around your age track and current belt. Use it as a conversation starter with your instructor.</p></section>
  <nav class="training-track-nav" aria-label="Training tracks"><a class="active" href="/student/training/karate">Karate track</a>{#if hasIs3}<a href="/student/training/is3">IS3 track</a>{/if}<a href="/student/training">Training overview</a></nav>
  {#if error}<div class="error" role="alert">{error}</div>{:else if !profile}<div class="student-panel loading-panel">Loading your belt requirements…</div>{:else}
    <section class="student-panel training-hero track-hero"><div><span class="student-eyebrow">NEXT RANK · {profile.ageGroup === 'kids' ? 'KIDS KARATE' : 'TEENS & ADULTS KARATE'}</span><h2>{nextRankName}</h2><p>{plan.promotionGuide}</p></div><span class="belt-graphic" style={`--belt-color:${plan.color}`}><span class="belt-knot"></span><span class="belt-strip"></span></span></section>
    <section class="student-panel training-note-panel"><div class="panel-title"><span>HOW TO USE THIS SHEET</span><span class="muted">Instructor-confirmed progress comes next</span></div><p>These are the requirements to practice for the next rank. Your instructor still makes the final testing decision; checking a skill here means “working on it,” not “officially passed.”</p></section>
    <div class="requirement-grid">{#each plan.sections as section}<section class="student-panel requirement-card"><div class="panel-title"><span>{section.category}</span><span class="requirement-count">{section.items.length}</span></div><ul>{#each section.items as item}<li><span class="requirement-marker" aria-hidden="true">○</span><span>{item}</span></li>{/each}</ul></section>{/each}</div>
  {/if}
</StudentShell>{/if}
