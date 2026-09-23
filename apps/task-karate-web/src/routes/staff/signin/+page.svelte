<script lang="ts">
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  let identifier = '';
  let password = '';
  let rememberMe = false;
  let disclaimerAccepted = false;
  let error = '';
  let busy = false;
  async function submit() { busy = true; error = ''; try { await api('/api/auth/login', { method: 'POST', body: JSON.stringify({ email: identifier.trim(), password, rememberMe, disclaimerAccepted }) }); await goto('/staff'); } catch (e) { error = e instanceof Error ? e.message : 'Sign-in failed.'; } finally { busy = false; } }
</script>
<svelte:head><title>Staff sign-in | TASK Karate</title></svelte:head>
<main class="auth-shell"><section class="auth-card"><span class="eyebrow">TASK KARATE SCHOOL</span><h1>Staff sign-in</h1><p class="muted">Use the local staff account issued for this dojo computer. Sign in with a username such as <strong>FirstName.LastName</strong> or the account email.</p>{#if error}<div class="error" role="alert">{error}</div>{/if}<form on:submit|preventDefault={submit}><label>Username or email<input type="text" bind:value={identifier} autocomplete="username" required /></label><label>Password<input type="password" bind:value={password} autocomplete="current-password" required /></label><div class="auth-disclaimer"><strong>Local staff-use acknowledgment</strong><p>This workspace can display student profiles, guardian contacts, attendance, and community posts. I will use it only for Task Karate operations, keep the screen supervised, and avoid sharing student information.</p><label class="check-row"><input type="checkbox" bind:checked={disclaimerAccepted} required /> I understand and accept this acknowledgment.</label></div><label class="check-row"><input type="checkbox" bind:checked={rememberMe} /> Keep me signed in on this device</label><button class="button" disabled={busy || !disclaimerAccepted}>{busy ? 'Signing in…' : 'Sign in'}</button></form><a class="back-link" href="/schedule">← Public schedule</a></section></main>
