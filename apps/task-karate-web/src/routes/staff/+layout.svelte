<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { page } from '$app/stores';
  import { HUB_THEMES, readPreference, writePreference, type HubTheme } from '$lib/student-preferences';
  import PortalNav from '$lib/components/PortalNav.svelte';
  import '$lib/staff.css';

  let ready = false;
  let error = '';
  let loggingOut = false;
  let theme: HubTheme = 'midnight';
  let canManageNews = false;
  let canManageStaffAccess = false;
  let adminSearch = '';

  const coreNavigation = [
    { href: '/staff', label: 'Overview', icon: '⌂' },
    { href: '/staff/students', label: 'Students', icon: '◎' },
    { href: '/staff/guardians', label: 'Families & contacts', icon: '◌' },
  ];
  const operationsNavigation = [
    { href: '/staff/classes', label: 'Programs & classes', icon: '▦' },
    { href: '/staff/attendance', label: 'Attendance', icon: '✓' },
    { href: '/staff/check-ins', label: 'Check-ins & rankings', icon: 'HI' },
    { href: '/staff/helpers', label: 'Helper roster', icon: '♢' },
    { href: '/staff/awards', label: 'Recognition', icon: '★' },
  ];
  const communityNavigation = [
    { href: '/staff/social', label: 'Community', icon: '✦' },
    { href: '/staff/announcements', label: 'Noticeboard', icon: '!' },
  ];
  $: utilityNavigation = [
    { href: '/staff/configuration', label: 'Portal configuration', icon: '⚙' },
    { href: '/staff/documents', label: 'Documents & waivers', icon: '▤' },
    { href: '/staff/reports', label: 'Reports', icon: '▥' },
    ...(canManageNews ? [{ href: '/staff/news', label: 'Public news', icon: '↗' }] : []),
    ...(canManageStaffAccess ? [{ href: '/staff/access', label: 'Staff access', icon: '⚿' }] : []),
    ...(canManageStaffAccess ? [{ href: '/staff/audit', label: 'Audit history', icon: '≡' }] : []),
  ];
  $: currentSection = $page.url.pathname === '/staff'
    ? 'Today'
    : $page.url.pathname.startsWith('/staff/social')
      ? 'Community'
      : $page.url.pathname.startsWith('/staff/announcements') || $page.url.pathname.startsWith('/staff/news')
        ? 'Publishing'
        : $page.url.pathname.startsWith('/staff/access')
          ? 'Administration'
          : 'Operations';
  $: isSignin = $page.url.pathname === '/staff/signin';

  onMount(async () => {
    theme = readPreference<HubTheme>('task-karate-hub-theme', 'midnight');
    if (window.location.pathname === '/staff/signin') { ready = true; return; }
    try {
      const identity = await api<{ roles?: string[] }>('/api/auth/me');
      canManageStaffAccess = identity.roles?.includes('Administrator') ?? false;
      canManageNews = (await api<{ canManage: boolean }>('/api/portal-admin/news/access')).canManage;
      if ($page.url.pathname === '/staff/news' && !canManageNews) { ready = true; await goto('/staff'); return; }
      if ($page.url.pathname === '/staff/access' && !canManageStaffAccess) { ready = true; await goto('/staff'); return; }
      ready = true;
    } catch {
      error = 'Your staff session is not active.';
      await goto('/staff/signin');
    }
  });

  async function logout() {
    loggingOut = true;
    try { await api('/api/auth/logout', { method: 'POST' }); await goto('/staff/signin'); }
    catch (e) { error = e instanceof Error ? e.message : 'Unable to sign out.'; loggingOut = false; }
  }

  function saveTheme() { writePreference('task-karate-hub-theme', theme); }
  function submitSearch() { const query = adminSearch.trim(); if (query.length >= 2) window.location.href = `/staff/search?q=${encodeURIComponent(query)}`; }
</script>

{#if ready}
  {#if isSignin}
    <slot />
  {:else}
    <div class={`app-shell staff-theme-${theme}`}>
      <PortalNav current="staff" />
      <header class="app-header">
        <div class="staff-brand"><span class="eyebrow">OPERATIONS CONSOLE</span><h1>Task Karate Operations</h1><p class="muted">{currentSection} · protected dojo operations</p></div>
        <div class="staff-header-actions">
          <form class="staff-global-search" on:submit|preventDefault={submitSearch}><label for="admin-search">Find anything</label><div><input id="admin-search" bind:value={adminSearch} type="search" placeholder="Student, family, class…" /><button class="button secondary small-button" type="submit">Search</button></div></form>
          <label class="staff-theme-picker"><span>Appearance</span><select bind:value={theme} on:change={saveTheme} aria-label="Staff portal appearance">{#each HUB_THEMES as option}<option value={option.id}>{option.label}</option>{/each}</select></label>
          <button class="button secondary small-button" type="button" on:click={logout} disabled={loggingOut}>{loggingOut ? 'Signing out…' : 'Sign out'}</button>
        </div>
      </header>
      <div class="staff-workspace">
        <aside class="staff-sidebar" aria-label="Staff navigation">
          <div class="staff-sidebar-label">Navigation</div>
          <nav class="staff-sidebar-nav">
            {#each [{label:'Core', items:coreNavigation}, {label:'Operations', items:operationsNavigation}, {label:'Community', items:communityNavigation}, {label:'Administration', items:utilityNavigation}] as group}
              {#if group.items.length}<div class="staff-nav-group"><span>{group.label}</span>{#each group.items as item}<a class:active={$page.url.pathname === item.href || (item.href !== '/staff' && $page.url.pathname.startsWith(`${item.href}/`))} href={item.href} aria-current={$page.url.pathname === item.href ? 'page' : undefined}><b aria-hidden="true">{item.icon}</b><span>{item.label}</span></a>{/each}</div>{/if}
            {/each}
          </nav>
          <div class="staff-sidebar-footer"><a href="/">View public website</a><a href="/student">View student hub</a></div>
        </aside>
        <main id="staff-content"><slot /></main>
      </div>
    </div>
  {/if}
{:else if error && !isSignin}
  <main class="auth-shell"><div class="auth-card"><div class="error">{error}</div><a class="button" href="/staff/signin">Sign in</a></div></main>
{:else if isSignin}
  <slot />
{:else}
  <main class="auth-shell"><div class="auth-card"><p>Checking staff session…</p></div></main>
{/if}
