<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  import { nextRank, trainingPlans } from '$lib/training-requirements';
  let session: StudentSession | null = null; let profile: any = null; let error = '';
  onMount(async () => { session = await requireStudent(); if (!session) return; try { profile = await api('/api/student/profile'); } catch (e) { error = apiError(e); } });
  $: nextRankName = nextRank(profile?.rankName);
  $: plan = trainingPlans[nextRankName] ?? trainingPlans['White Belt'];
  $: stripePercent = profile?.classesPerStripe ? Math.min(100, (profile.classesIntoStripe / profile.classesPerStripe) * 100) : 0;
</script>

<svelte:head><title>Task Karate | Training</title></svelte:head>

{#if session}
  <StudentShell {session} active="training">
    <section class="student-heading"><span class="student-eyebrow">YOUR PRACTICE RECORD</span><h1>TRAINING</h1><p>Know what you are working toward before your next rank check.</p></section>
    {#if error}<div class="error">{error}</div>{:else if !profile}<div class="student-panel loading-panel">Loading your training plan…</div>{:else}
      <div class="student-grid two-column training-top-grid">
        <section class="student-panel training-hero"><span class="belt-graphic" style={`--belt-color:${plan.color}`}><span class="belt-knot"></span><span class="belt-strip"></span></span><span class="student-eyebrow">NEXT RANK</span><h2>{nextRankName}</h2><p>{plan.promotionGuide}</p></section>
        <section class="student-panel stripe-panel"><div class="panel-title"><span>STRIPE PROGRESS</span><a href="/student/profile">View profile →</a></div><div class="stripe-track" aria-label={`${profile.classesIntoStripe} of ${profile.classesPerStripe || 0} classes toward next stripe`}><span style={`width:${stripePercent}%`}></span></div><div class="stripe-summary"><strong>{profile.classesToNextStripe || 'Instructor tracked'} {profile.classesToNextStripe ? (profile.classesToNextStripe === 1 ? 'class' : 'classes') : ''} until next stripe</strong><span>{profile.classesPerStripe ? `${profile.classesIntoStripe} of ${profile.classesPerStripe} completed` : 'Black belt degrees are tracked by your instructor.'}</span></div></section>
      </div>
      <section class="student-panel training-note-panel"><div class="panel-title"><span>WHAT TO WORK ON</span><span class="muted">{nextRankName} testing sheet</span></div><p>{profile.favoriteTechnique ? `Current focus: ${profile.favoriteTechnique}. Use the checklist below to guide practice with your instructor.` : 'Use this checklist to guide practice with your instructor. Students can view requirements now; staff completion tracking will come next.'}</p></section>
      <div class="requirement-grid">{#each plan.sections as section}<section class="student-panel requirement-card"><div class="panel-title"><span>{section.category}</span><span class="requirement-count">{section.items.length}</span></div><ul>{#each section.items as item}<li><span class="requirement-marker" aria-hidden="true">○</span><span>{item}</span></li>{/each}</ul></section>{/each}</div>
    {/if}
  </StudentShell>
{/if}
