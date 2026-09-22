<script lang="ts">
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { initials, type StudentSession } from '$lib/student-session';
  export let session: StudentSession;
  export let active = 'status';
  let error = '';
  const tabs = [
    { id: 'status', label: 'Status', href: '/student' },
    { id: 'social', label: 'Social', href: '/student/social' },
    { id: 'achievements', label: 'Achievements', href: '/student/achievements' },
    { id: 'training', label: 'Training', href: '/student/training' },
    { id: 'news', label: 'News', href: '/student/news' }
  ];
  async function logout() {
    error = '';
    try { await api('/api/student/auth/logout', { method: 'POST' }); await goto('/schedule'); }
    catch (e) { error = e instanceof Error ? e.message : 'Unable to sign out.'; }
  }
</script>

<svelte:head><meta name="theme-color" content="#081526" /></svelte:head>
<div class="student-app">
  <header class="student-topbar">
    <a class="brand" href="/schedule" aria-label="Task Karate schedule"><span class="brand-mark">TK</span><span><strong>TASK KARATE</strong><small>STUDENT HUB</small></span></a>
    <div class="topbar-note">{session.displayName ?? 'Student'} <span aria-hidden="true">·</span> connected securely</div>
  </header>
  <aside class="student-identity" aria-label="Student identity"><span class="identity-avatar">{initials(session.displayName)}</span><span class="identity-label">{session.displayName ?? 'Student'}</span></aside>
  <nav class="floating-nav" aria-label="Student sections">
    {#each tabs as tab}<a class:active={active === tab.id} href={tab.href} aria-current={active === tab.id ? 'page' : undefined}><span aria-hidden="true">{tab.id === 'status' ? '◉' : tab.id === 'social' ? '✦' : tab.id === 'achievements' ? '★' : tab.id === 'training' ? '➜' : '▤'}</span>{tab.label}</a>{/each}
  </nav>
  <button class="floating-logout" type="button" on:click={logout}><span aria-hidden="true">⏻</span> Log out</button>
  {#if error}<div class="student-toast error" role="alert">{error}</div>{/if}
  <main class="student-main"><slot /></main>
</div>
