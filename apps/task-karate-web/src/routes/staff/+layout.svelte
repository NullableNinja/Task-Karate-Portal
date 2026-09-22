<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  let ready = false;
  let isSignin = false;
  let error = '';
  onMount(async () => {
    isSignin = window.location.pathname === '/staff/signin';
    if (isSignin) { ready = true; return; }
    try { await api('/api/auth/me'); ready = true; }
    catch { error = 'Your staff session is not active.'; await goto('/staff/signin'); }
  });
</script>

{#if ready}
  {#if isSignin}
    <slot />
  {:else}
    <div class="app-shell"><header class="app-header"><div><span class="eyebrow">STAFF WORKSPACE</span><h1>Task Karate operations</h1></div><a class="button secondary small-button" href="/">Public view</a></header><nav class="app-nav" aria-label="Staff navigation"><a href="/staff">Dashboard</a><a href="/staff/students">Students</a><a href="/staff/guardians">Guardians</a><a href="/staff/classes">Classes</a><a href="/staff/attendance">Attendance</a><a href="/staff/announcements">Announcements</a><a href="/staff/news">News</a></nav><slot /></div>
  {/if}
{:else if error}
  <main class="auth-shell"><div class="auth-card"><div class="error">{error}</div><a class="button" href="/staff/signin">Sign in</a></div></main>
{:else}
  <main class="auth-shell"><div class="auth-card"><p>Checking staff session…</p></div></main>
{/if}
