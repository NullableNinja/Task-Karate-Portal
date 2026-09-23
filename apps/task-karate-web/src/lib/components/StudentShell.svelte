<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { initials, type StudentSession } from '$lib/student-session';
  export let session: StudentSession;
  export let active = 'status';
  let error = '';
  let currentProfile: { rankName?: string | null; programs?: { programCode: string; levelName?: string | null }[] } | null = null;
  let scrollPercent = 0;
  const tabs = [
    { id: 'status', label: 'Status', href: '/student' },
    { id: 'social', label: 'Social', href: '/student/social' },
    { id: 'achievements', label: 'Achievements', href: '/student/achievements' },
    { id: 'training', label: 'Training', href: '/student/training' },
    { id: 'profile', label: 'Profile', href: '/student/profile' }
  ];
  async function logout() {
    error = '';
    try { await api('/api/student/auth/logout', { method: 'POST' }); await goto('/student/login'); }
    catch (e) { error = e instanceof Error ? e.message : 'Unable to sign out.'; await goto('/student/login'); }
  }
  const scrollBelts = [
    { name: 'White Belt', color: '#eef2f4' },
    { name: 'Gold Belt', color: '#d8b45d' },
    { name: 'Orange Belt', color: '#e28b45' },
    { name: 'Green Belt', color: '#4eaa79' },
    { name: 'Purple Belt', color: '#9671cb' },
    { name: 'Blue Belt', color: '#4f9fe3' },
    { name: 'Brown Belt', color: '#966440' },
    { name: 'Red Belt', color: '#df6470' },
    { name: 'Black Belt', color: '#273342' }
  ];
  $: scrollBelt = scrollBelts[Math.min(scrollBelts.length - 1, Math.floor((scrollPercent / 100) * scrollBelts.length))];
  function updateScrollProgress() { const maximum = document.documentElement.scrollHeight - window.innerHeight; scrollPercent = maximum > 0 ? Math.round((window.scrollY / maximum) * 100) : 0; }
  onMount(() => { api<typeof currentProfile>('/api/student/profile').then((profile) => { currentProfile = profile; }).catch(() => undefined); updateScrollProgress(); window.addEventListener('scroll', updateScrollProgress, { passive: true }); window.addEventListener('resize', updateScrollProgress); return () => { window.removeEventListener('scroll', updateScrollProgress); window.removeEventListener('resize', updateScrollProgress); }; });
</script>

<svelte:head><meta name="theme-color" content="#081526" /></svelte:head>
<div class="student-app">
  <a class="skip-link" href="#student-content">Skip to content</a>
  <div class="portal-orbs" aria-hidden="true"><span class="portal-orb orb-a"></span><span class="portal-orb orb-b"></span><span class="portal-orb orb-c"></span><span class="portal-orb orb-d"></span><span class="portal-orb orb-e"></span><span class="portal-orb orb-f"></span><span class="portal-orb orb-g"></span><span class="portal-orb orb-h"></span></div>
  <header class="student-topbar">
    <a class="brand" href="/schedule" aria-label="Task Karate schedule"><span class="brand-mark">TK</span><span><strong>TASK KARATE</strong><small>STUDENT HUB</small></span></a>
    <div class="topbar-note">{session.displayName ?? 'Student'} <span aria-hidden="true">·</span> connected securely</div>
  </header>
  <a class="student-identity student-identity-link" href="/student/profile" aria-label={`Open ${session.displayName ?? 'student'} profile`}><span class="identity-avatar">{initials(session.displayName)}</span><span class="identity-copy"><span class="identity-label">{session.displayName ?? 'Student'}</span><small>{currentProfile?.rankName ?? currentProfile?.programs?.find((item) => item.programCode === 'is3')?.levelName ?? 'Rank in progress'}</small></span></a>
  <nav class="floating-nav" aria-label="Student sections">
    {#each tabs as tab}<a class:active={active === tab.id} href={tab.href} aria-current={active === tab.id ? 'page' : undefined}><span aria-hidden="true">{tab.id === 'status' ? '◉' : tab.id === 'social' ? '✦' : tab.id === 'achievements' ? '★' : tab.id === 'training' ? '➜' : '◎'}</span>{tab.label}</a>{/each}
  </nav>
  <button class="floating-logout" type="button" on:click={logout}><span aria-hidden="true">⏻</span> Log out</button>
  {#if error}<div class="student-toast error" role="alert">{error}</div>{/if}
  <main id="student-content" class="student-main"><slot /></main>
  <div class="scroll-rank-indicator" role="progressbar" aria-label={`Page progress · ${scrollBelt.name}`} aria-valuemin="0" aria-valuemax="100" aria-valuenow={scrollPercent} style={`--scroll-progress:${scrollPercent}%;--scroll-color:${scrollBelt.color}`}><span class="scroll-rank-fill"></span></div>
</div>
