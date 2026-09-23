<script lang="ts">
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  let identifier = '';
  let password = '';
  let disclaimerAccepted = false;
  let disclaimerOpen = false;
  let error = '';
  let busy = false;
  async function submit() {
    error = '';
    if (!disclaimerOpen) { disclaimerOpen = true; return; }
    busy = true;
    try { await api('/api/auth/login', { method: 'POST', body: JSON.stringify({ email: identifier.trim(), password, disclaimerAccepted: true }) }); await goto('/staff'); }
    catch (e) { error = e instanceof Error ? e.message : 'Sign-in failed.'; disclaimerOpen = false; }
    finally { busy = false; }
  }
</script>
<svelte:head><title>Staff sign-in | TASK Karate</title></svelte:head>
<main class="auth-shell"><section class="auth-card"><span class="eyebrow">TASK KARATE SCHOOL</span><h1>Staff sign-in</h1><p class="muted">Use the local staff account issued for this dojo computer. Sign in with a username such as <strong>FirstName.LastName</strong> or the account email.</p>{#if error}<div class="error" role="alert">{error}</div>{/if}<form on:submit|preventDefault={submit}><label>Username or email<input type="text" bind:value={identifier} autocomplete="username" required /></label><label>Password<input type="password" bind:value={password} autocomplete="current-password" required /></label><button class="button" disabled={busy}>{busy ? 'Signing in…' : 'Sign in'}</button></form><a class="back-link" href="/schedule">← Public schedule</a></section></main>
{#if disclaimerOpen}<div class="auth-modal-backdrop" role="presentation"><div class="auth-modal" role="dialog" aria-modal="true" aria-labelledby="staff-ack-title"><span class="eyebrow">BEFORE YOU ENTER</span><h2 id="staff-ack-title">Staff-use acknowledgment</h2><p>This workspace can display student profiles, guardian contacts, attendance, and community posts. Use it only for Task Karate operations, keep the screen supervised, and do not share student information.</p><label class="check-row"><input type="checkbox" bind:checked={disclaimerAccepted} /> I understand and accept this acknowledgment.</label><div class="auth-modal-actions"><button class="button secondary" type="button" on:click={() => { disclaimerOpen = false; disclaimerAccepted = false; }}>Cancel</button><button class="button" type="button" disabled={!disclaimerAccepted || busy} on:click={submit}>{busy ? 'Signing in…' : 'Accept and enter'}</button></div></div></div>{/if}
