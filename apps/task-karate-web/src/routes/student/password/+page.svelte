<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { apiError, type StudentSession } from '$lib/student-session';

  let session: StudentSession | null = null;
  let form = { currentPassword: '', newPassword: '', confirmPassword: '' };
  let error = '';
  let loading = true;
  let saving = false;

  onMount(async () => {
    try {
      session = await api<StudentSession>('/api/student/auth/me');
      if (!session.passwordChangeRequired) await goto(session.disclaimerRequired ? '/student/disclaimer' : '/student');
    } catch { await goto('/student/login'); }
    finally { loading = false; }
  });

  async function submit() {
    saving = true; error = '';
    try {
      await api('/api/student/auth/password', { method: 'POST', body: JSON.stringify(form) });
      const next = await api<StudentSession>('/api/student/auth/me');
      await goto(next.disclaimerRequired ? '/student/disclaimer' : '/student');
    } catch (e) { error = apiError(e, 'Your password could not be changed.'); }
    finally { saving = false; }
  }
</script>

<svelte:head><title>Task Karate | Change password</title></svelte:head>

<main class="auth-stage">
  <section class="disclaimer-frame password-change-frame" aria-labelledby="password-title">
    {#if loading}<p>Checking your secure session…</p>{:else}
      <span class="student-eyebrow">FIRST SIGN-IN / SECURITY STEP</span>
      <h1 id="password-title">Choose your password.</h1>
      <p>Your temporary dojo password worked. Before opening the student hub, choose a private password for internet access.</p>
      <form class="profile-form password-form" on:submit|preventDefault={submit}>
        <label>Temporary password<input type="password" bind:value={form.currentPassword} autocomplete="current-password" required /></label>
        <label>New password<input type="password" bind:value={form.newPassword} minlength="12" autocomplete="new-password" required /><small class="muted">Use at least 12 characters with uppercase, lowercase, and a number.</small></label>
        <label>Confirm new password<input type="password" bind:value={form.confirmPassword} minlength="12" autocomplete="new-password" required /></label>
        {#if error}<div class="error" role="alert">{error}</div>{/if}
        <div class="disclaimer-actions"><a class="outline-button" href="/student/login">Cancel</a><button class="primary-button" type="submit" disabled={saving}>{saving ? 'Saving…' : 'Save password'}</button></div>
      </form>
    {/if}
  </section>
</main>
