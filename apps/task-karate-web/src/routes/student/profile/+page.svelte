<script lang="ts">
  import { onMount } from 'svelte';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { api } from '$lib/api';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';

  let session: StudentSession | null = null;
  let profile: any = null;
  let error = '';

  onMount(async () => {
    session = await requireStudent();
    if (!session) return;
    try { profile = await api('/api/student/profile'); } catch (e) { error = apiError(e); }
  });
</script>

<svelte:head><title>Task Karate | My profile</title></svelte:head>

{#if session}
  <StudentShell {session} active="profile">
    <section class="student-heading profile-heading"><span class="student-eyebrow">YOUR DOJO PROFILE</span><h1>MY PROFILE</h1><p>A personal view of your rank, training record, and the story you are building at Task Karate.</p></section>
    {#if error}<div class="error" role="alert">{error}</div>{:else if !profile}<div class="student-panel loading-panel">Loading your profile…</div>{:else}
      <div class="profile-layout">
        <section class="student-panel profile-card">
          <span class="profile-avatar">{profile.displayName?.slice(0, 2).toUpperCase()}</span>
          <span class="student-eyebrow">STUDENT</span>
          <h2>{profile.displayName}</h2>
          <p>{profile.bio ?? 'Your instructor can add a short dojo biography here.'}</p>
          <div class="profile-tags"><span>{profile.rankName ?? 'Rank in progress'}</span><span>Member since {profile.joinDate ?? 'on file'}</span></div>
        </section>
        <section class="student-panel profile-details">
          <div class="panel-title"><span>TRAINING RECORD</span><a href="/student/training">Open training →</a></div>
          <div class="record-list"><div><span>Total classes</span><strong>{profile.totalClasses}</strong></div><div><span>Current stripe</span><strong>{profile.classesIntoStripe} / {profile.classesPerStripe || '—'}</strong></div><div><span>Next milestone</span><strong>{profile.nextMilestone}</strong></div><div><span>Classes to go</span><strong>{profile.classesToNextStripe || 'Instructor tracked'}</strong></div></div>
        </section>
      </div>
      <div class="profile-info-grid">
        <section class="student-panel"><div class="panel-title"><span>UNIFORM & BELT</span></div><div class="record-list"><div><span>Uniform size</span><strong>{profile.uniformSize ?? 'Not on file'}</strong></div><div><span>Belt size</span><strong>{profile.beltSize ?? 'Not on file'}</strong></div><div><span>Age group</span><strong>{profile.ageGroup ?? 'Not on file'}</strong></div><div><span>Birthday</span><strong>{profile.birthDate ? new Date(profile.birthDate).toLocaleDateString() : 'Not on file'}</strong></div></div></section>
        <section class="student-panel"><div class="panel-title"><span>GUARDIANS</span></div>{#if profile.guardians?.length}<div class="guardian-list">{#each profile.guardians as guardian}<div class="guardian-row"><strong>{guardian.name}</strong><span>{guardian.relationship}</span>{#if guardian.phone}<small>{guardian.phone}</small>{/if}{#if guardian.email}<small>{guardian.email}</small>{/if}</div>{/each}</div>{:else}<p class="muted">No guardian record is linked in this development dataset. Staff can add one when the guardian workflow is connected.</p>{/if}</section>
      </div>
      {#if profile.email || profile.phone}<section class="student-panel profile-contact"><div class="panel-title"><span>CONTACT ON FILE</span></div><div class="profile-contact-values">{#if profile.email}<span>{profile.email}</span>{/if}{#if profile.phone}<span>{profile.phone}</span>{/if}</div></section>{/if}
    {/if}
  </StudentShell>
{/if}
