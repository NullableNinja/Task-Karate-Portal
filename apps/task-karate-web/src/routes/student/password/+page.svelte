<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { apiError, type StudentSession } from '$lib/student-session';

  let session: StudentSession | null = null;
  let form = { currentPin: '', newPin: '', confirmPin: '' };
  let error = '';
  let loading = true;
  let saving = false;

  onMount(async () => {
    try {
      session = await api<StudentSession>('/api/student/auth/me');
      if (session.disclaimerRequired) await goto('/student/disclaimer');
    } catch { await goto('/student/login'); }
    finally { loading = false; }
  });

  async function submit() {
    saving = true; error = '';
    try {
      await api('/api/student/auth/pin', { method: 'POST', body: JSON.stringify(form) });
      const next = await api<StudentSession>('/api/student/auth/me');
      await goto(next.disclaimerRequired ? '/student/disclaimer' : '/student');
    } catch (e) { error = apiError(e, 'Your student PIN could not be changed.'); }
    finally { saving = false; }
  }
</script>

<svelte:head><title>Task Karate | Change student PIN</title></svelte:head>

<main class="auth-stage">
  <section class="disclaimer-frame password-change-frame" aria-labelledby="password-title">
    {#if loading}<p>Checking your secure session…</p>{:else}
      <span class="student-eyebrow">STUDENT ACCOUNT ACCESS</span>
      <h1 id="password-title">Change your PIN.</h1>
      <p>Your student PIN is used for both the Student Hub and supervised class check-in.</p>
      <form class="profile-form password-form" on:submit|preventDefault={submit}>
        <label>Current PIN<input type="password" bind:value={form.currentPin} inputmode="numeric" minlength="4" maxlength="6" autocomplete="current-password" required /></label>
        <label>New PIN<input type="password" bind:value={form.newPin} inputmode="numeric" minlength="4" maxlength="6" pattern="[0-9][0-9][0-9][0-9][0-9]?[0-9]?" autocomplete="new-password" required /><small class="muted">Use 4–6 digits.</small></label>
        <label>Confirm new PIN<input type="password" bind:value={form.confirmPin} inputmode="numeric" minlength="4" maxlength="6" pattern="[0-9][0-9][0-9][0-9][0-9]?[0-9]?" autocomplete="new-password" required /></label>
        {#if error}<div class="error" role="alert">{error}</div>{/if}
        <div class="disclaimer-actions"><a class="outline-button" href="/student/profile">Cancel</a><button class="primary-button" type="submit" disabled={saving}>{saving ? 'Saving…' : 'Save PIN'}</button></div>
      </form>
    {/if}
  </section>
</main>
