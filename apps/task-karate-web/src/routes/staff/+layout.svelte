<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { page } from '$app/stores';
  import '$lib/staff.css';
  let ready = false;
  let error = '';
  let loggingOut = false;
  onMount(async () => {
    if (isSignin) { ready = true; return; }
    try { await api('/api/auth/me'); ready = true; }
    catch { error = 'Your staff session is not active.'; await goto('/staff/signin'); }
  });
  async function logout() {
    loggingOut = true;
    try { await api('/api/auth/logout', { method: 'POST' }); await goto('/staff/signin'); }
    catch (e) { error = e instanceof Error ? e.message : 'Unable to sign out.'; loggingOut = false; }
  }
  $: isSignin = $page.url.pathname === '/staff/signin';
</script>

{#if ready}
  {#if isSignin}
    <slot />
  {:else}
  <div class="app-shell"><a class="skip-link" href="#staff-content">Skip to content</a><header class="app-header"><div><span class="eyebrow">STAFF WORKSPACE</span><h1>Task Karate operations</h1><p class="muted">Local operations console · protected by server-side staff authentication</p></div><div class="staff-header-actions"><a class="button secondary small-button" href="/schedule">Public schedule</a><button class="button secondary small-button" type="button" on:click={logout} disabled={loggingOut}>{loggingOut ? 'Signing out…' : 'Sign out'}</button></div></header><nav class="app-nav" aria-label="Staff navigation">{#each [{href:'/staff',label:'Dashboard'},{href:'/staff/students',label:'Students'},{href:'/staff/guardians',label:'Guardians'},{href:'/staff/classes',label:'Classes'},{href:'/staff/attendance',label:'Attendance'},{href:'/staff/helpers',label:'Helper roster'},{href:'/staff/awards',label:'Awards'},{href:'/staff/social',label:'Social moderation'},{href:'/staff/announcements',label:'Noticeboard'},{href:'/staff/news',label:'Public News'}] as item}<a class:active={$page.url.pathname === item.href} href={item.href} aria-current={$page.url.pathname === item.href ? 'page' : undefined}>{item.label}</a>{/each}</nav><main id="staff-content"><slot /></main></div>
  {/if}
{:else if error}
  <main class="auth-shell"><div class="auth-card"><div class="error">{error}</div><a class="button" href="/staff/signin">Sign in</a></div></main>
{:else}
  <main class="auth-shell"><div class="auth-card"><p>Checking staff session…</p></div></main>
{/if}
