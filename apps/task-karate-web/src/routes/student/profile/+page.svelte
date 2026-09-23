<script lang="ts">
  import { onMount } from 'svelte';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { api } from '$lib/api';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  import { rankColor, rankTextColor } from '$lib/rank-colors';

  let session: StudentSession | null = null;
  let profile: any = null;
  let error = '';
  let editing = false;
  let saving = false;
  let changingPassword = false;
  let passwordNotice = '';
  let passwordError = '';
  let form = { displayName: '', bio: '', favoriteTechnique: '', email: '', phone: '', uniformSize: '', beltSize: '' };
  let passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' };
  type GuardianRecord = { name: string; relationship: string; phone?: string; email?: string };
  let uniqueGuardians: GuardianRecord[] = [];

  onMount(async () => {
    session = await requireStudent();
    if (!session) return;
    try { profile = await api('/api/student/profile'); } catch (e) { error = apiError(e); }
  });
  $: uniqueGuardians = profile?.guardians ? Array.from(new Map<string, GuardianRecord>(profile.guardians.map((guardian: GuardianRecord) => [`${guardian.name}|${guardian.relationship}|${guardian.email ?? ''}`, guardian])).values()) : [];
  $: profileFields = [profile?.email, profile?.phone, profile?.uniformSize, profile?.beltSize, uniqueGuardians.length, profile?.programs?.length];
  $: profileCompletion = profile ? Math.round((profileFields.filter(Boolean).length / profileFields.length) * 100) : 0;
  function beginEdit() { form = { displayName: profile.displayName ?? '', bio: profile.bio ?? '', favoriteTechnique: profile.favoriteTechnique ?? '', email: profile.email ?? '', phone: profile.phone ?? '', uniformSize: profile.uniformSize ?? '', beltSize: profile.beltSize ?? '' }; editing = true; }
  async function saveProfile() { saving = true; error = ''; try { profile = await api('/api/student/profile', { method: 'PUT', body: JSON.stringify(form) }); editing = false; } catch (e) { error = apiError(e); } finally { saving = false; } }
  async function changePassword() { changingPassword = true; passwordError = ''; passwordNotice = ''; try { await api('/api/student/auth/password', { method: 'POST', body: JSON.stringify(passwordForm) }); passwordForm = { currentPassword: '', newPassword: '', confirmPassword: '' }; passwordNotice = 'PIN changed. Use the new PIN the next time you enter the dojo.'; } catch (e) { passwordError = apiError(e); } finally { changingPassword = false; } }
</script>

<svelte:head><title>Task Karate | My profile</title></svelte:head>

{#if session}
  <StudentShell {session} active="profile">
    <section class="student-heading profile-heading"><span class="student-eyebrow">YOUR DOJO PROFILE</span><h1>MY PROFILE</h1><p>A personal view of your rank, training record, and the story you are building at Task Karate.</p></section>
    {#if error}<div class="error" role="alert">{error}</div>{:else if !profile}<div class="student-panel loading-panel">Loading your profile…</div>{:else}
      <section class="student-panel profile-completion-panel"><div><span class="student-eyebrow">PROFILE READINESS</span><h2>{profileCompletion}% complete</h2><p>Keep your contact, uniform, and training details current so the dojo can support you.</p></div><div class="profile-completion-actions"><div class="mini-progress"><span style={`width:${profileCompletion}%`}></span></div><button class="primary-button" type="button" on:click={beginEdit}>{editing ? 'Editing below' : 'Complete my profile'}</button></div></section>
      {#if editing}<section class="student-panel profile-editor"><div class="panel-title"><span>EDIT PROFILE</span><span class="muted">You can update these details anytime.</span></div><form class="profile-form" on:submit|preventDefault={saveProfile}><label>Display name<input bind:value={form.displayName} maxlength="100" /></label><label>Email<input type="email" bind:value={form.email} maxlength="254" /></label><label>Phone<input bind:value={form.phone} maxlength="50" /></label><label>Uniform size<input bind:value={form.uniformSize} maxlength="50" placeholder="e.g. Youth Medium" /></label><label>Belt size<input bind:value={form.beltSize} maxlength="50" placeholder="e.g. Size 2" /></label><label>Favorite technique<input bind:value={form.favoriteTechnique} maxlength="100" placeholder="e.g. Roundhouse kick" /></label><label class="full">Short bio<textarea bind:value={form.bio} maxlength="500" rows="3" placeholder="A sentence about your training journey…"></textarea></label><div class="profile-form-actions"><button class="outline-button" type="button" on:click={() => editing = false}>Cancel</button><button class="primary-button" type="submit" disabled={saving}>{saving ? 'Saving…' : 'Save profile'}</button></div></form></section>{/if}
      <div class="profile-layout">
        <section class="student-panel profile-card">
          <span class="profile-avatar">{profile.displayName?.slice(0, 2).toUpperCase()}</span>
          <span class="student-eyebrow">STUDENT</span>
          <h2>{profile.displayName}</h2>
          <p>{profile.bio ?? 'Your instructor can add a short dojo biography here.'}</p>
          <div class="profile-tags"><span class="belt-pill" style={`--belt-color:${rankColor(profile.rankName)};--belt-text:${rankTextColor(profile.rankName)}`}>{profile.rankName ?? 'Rank in progress'}</span><span>Member since {profile.joinDate ?? 'on file'}</span></div>
        </section>
        <section class="student-panel profile-details">
          <div class="panel-title"><span>TRAINING RECORD</span><a href="/student/training">Open training →</a></div>
          <div class="record-list"><div><span>Total classes</span><strong>{profile.totalClasses}</strong></div><div><span>Current stripe</span><strong>{profile.classesIntoStripe} / {profile.classesPerStripe || '—'}</strong></div><div><span>Next milestone</span><strong>{profile.nextMilestone}</strong></div><div><span>Classes to go</span><strong>{profile.classesToNextStripe || 'Instructor tracked'}</strong></div></div>
        </section>
      </div>
      <div class="profile-info-grid">
        <section class="student-panel"><div class="panel-title"><span>UNIFORM & BELT</span></div><div class="record-list"><div><span>Uniform size</span><strong>{profile.uniformSize ?? 'Not on file'}</strong></div><div><span>Belt size</span><strong>{profile.beltSize ?? 'Not on file'}</strong></div><div><span>Age group</span><strong>{profile.ageGroup ?? 'Not on file'}</strong></div><div><span>Birthday</span><strong>{profile.birthDate ? new Date(profile.birthDate).toLocaleDateString() : 'Not on file'}</strong></div></div></section>
        <section class="student-panel"><div class="panel-title"><span>GUARDIANS</span><span class="muted">Staff-managed relationships</span></div>{#if uniqueGuardians.length}<div class="guardian-list">{#each uniqueGuardians as guardian}<div class="guardian-row"><strong>{guardian.name}</strong><span>{guardian.relationship}</span>{#if guardian.phone}<small>{guardian.phone}</small>{/if}{#if guardian.email}<small>{guardian.email}</small>{/if}</div>{/each}</div>{:else}<p class="muted">No guardian record is linked yet. Ask staff to connect a guardian account.</p>{/if}</section>
      </div>
      <section class="student-panel program-memberships"><div class="panel-title"><span>PROGRAMS & LEVELS</span><span class="muted">Your progress can span both tracks</span></div>{#if profile.programs?.length}<div class="program-membership-grid">{#each profile.programs as program}<div class="program-membership"><span class="program-code">{program.programCode}</span><div><strong>{program.programName}</strong><span>{program.progressionType === 'level' ? 'Level track' : 'Belt track'}</span><small>{program.levelName ?? 'Level in progress'}</small></div></div>{/each}</div>{:else}<p class="muted">No program memberships are on file yet.</p>{/if}</section>
      {#if profile.email || profile.phone}<section class="student-panel profile-contact"><div class="panel-title"><span>CONTACT ON FILE</span></div><div class="profile-contact-values">{#if profile.email}<span>{profile.email}</span>{/if}{#if profile.phone}<span>{profile.phone}</span>{/if}</div></section>{/if}
      <section class="student-panel password-panel"><div class="panel-title"><span>ACCOUNT PIN</span><span class="muted">Private to you</span></div><p class="muted">Change the 4–6 digit PIN used when you enter your student profile. Your current PIN is required.</p>{#if passwordError}<div class="error" role="alert">{passwordError}</div>{/if}{#if passwordNotice}<div class="status" role="status">{passwordNotice}</div>{/if}<form class="profile-form password-form" on:submit|preventDefault={changePassword}><label>Current PIN<input type="password" bind:value={passwordForm.currentPassword} autocomplete="current-password" required /></label><label>New PIN<input type="password" bind:value={passwordForm.newPassword} minlength="4" maxlength="6" pattern="[0-9][0-9][0-9][0-9][0-9]?[0-9]?" autocomplete="new-password" required /><small class="muted">Use 4–6 digits. Choose something easy to remember but not obvious.</small></label><label>Confirm new PIN<input type="password" bind:value={passwordForm.confirmPassword} minlength="4" maxlength="6" pattern="[0-9][0-9][0-9][0-9][0-9]?[0-9]?" autocomplete="new-password" required /></label><div class="profile-form-actions"><button class="primary-button" type="submit" disabled={changingPassword}>{changingPassword ? 'Changing…' : 'Change PIN'}</button></div></form></section>
    {/if}
  </StudentShell>
{/if}
