<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { apiError, initials, type StudentSession } from '$lib/student-session';
  import { HUB_THEMES, readPreference, writePreference, type HubTheme } from '$lib/student-preferences';
  import { EASTER_EGG_BY_ID, triggerEasterEgg, type EasterEggId, type EasterEggDefinition } from '$lib/easter-eggs';
  import FloatingBubbles from '$lib/components/FloatingBubbles.svelte';
  import EasterEggCelebration from '$lib/components/EasterEggCelebration.svelte';
  import StudentMessages from '$lib/components/StudentMessages.svelte';
  import ContextBackButton from '$lib/components/ContextBackButton.svelte';
  export let session: StudentSession;
  export let active = 'status';
  let error = '';
  let currentProfile: { rankName?: string | null; unreadMessages?: number; programs?: { programCode: string; levelName?: string | null }[] } | null = null;
  let scrollPercent = 0;
  let birthdayCelebration = false;
  let toolsOpen = false;
  let accountOpen = false;
  let commandOpen = false;
  let theme: HubTheme = 'midnight';
  let accentColor = '#38bdf8';
  let reminderNotice = '';
  let avatarUrl = '';
  let messagesUnread = 0;
  let activeEgg: EasterEggDefinition | null = null;
  let newlyUnlocked = false;
  let celebrationTimer: number | undefined;
  const busyEggs = new Set<string>();
  let logoClicks = 0;
  let avatarClicks = 0;
  let scrollIndicatorClicks = 0;
  let lastEggClick = 0;
  let navSequence: string[] = [];
  let messagesOpen = false;
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
    { name: 'Red Belt', color: '#df6470' },
    { name: 'Brown Belt', color: '#966440' },
    { name: 'Black Belt', color: '#202a35' }
  ];
  $: scrollBelt = scrollBelts[Math.min(scrollBelts.length - 1, Math.floor((scrollPercent / 100) * scrollBelts.length))];
  function updateScrollProgress() { const maximum = document.documentElement.scrollHeight - window.innerHeight; scrollPercent = maximum > 0 ? Math.round((window.scrollY / maximum) * 100) : 0; }
  function launchBirthdayConfetti() {
    if (typeof document === 'undefined') return;
    const container = document.createElement('div');
    container.className = 'birthday-confetti';
    container.setAttribute('aria-hidden', 'true');
    const colors = ['#f5f7fb', '#38bdf8', '#d8b45d', '#4eaa79', '#9671cb', '#df6470'];
    for (let index = 0; index < 132; index += 1) {
      const piece = document.createElement('span');
      piece.style.setProperty('--confetti-x', `${Math.round(Math.random() * 100)}vw`);
      piece.style.setProperty('--confetti-delay', `${(Math.random() * 2.8).toFixed(2)}s`);
      piece.style.setProperty('--confetti-drift', `${Math.round((Math.random() - 0.5) * 22)}vw`);
      piece.style.setProperty('--confetti-size', `${6 + Math.round(Math.random() * 7)}px`);
      piece.style.setProperty('--confetti-color', colors[index % colors.length]);
      piece.style.setProperty('--confetti-rotation', `${Math.round(Math.random() * 360)}deg`);
      container.appendChild(piece);
    }
    document.body.appendChild(container);
    window.setTimeout(() => container.remove(), 9800);
  }
  function setTheme(next: HubTheme) { theme = next; writePreference('task-karate-hub-theme', next); }
  function normalizeAccent(value: string) { return /^#[0-9a-f]{6}$/i.test(value) ? value : '#38bdf8'; }
  function setAccentColor(next: string) { accentColor = normalizeAccent(next); writePreference(`task-karate-accent-${session.studentId}`, accentColor); }
  function openCommandPalette() { commandOpen = true; toolsOpen = false; accountOpen = false; }
  function closeOverlays() { commandOpen = false; toolsOpen = false; accountOpen = false; }
  function resetCelebration() { if (celebrationTimer) window.clearTimeout(celebrationTimer); activeEgg = null; newlyUnlocked = false; }
  function showCelebration(id: EasterEggId, unlocked = false) {
    const egg = EASTER_EGG_BY_ID[id]; if (!egg) return;
    if (celebrationTimer) window.clearTimeout(celebrationTimer);
    activeEgg = egg; newlyUnlocked = unlocked;
    celebrationTimer = window.setTimeout(() => { activeEgg = null; newlyUnlocked = false; }, 5600);
  }
  async function unlockEasterEgg(id: EasterEggId) {
    if (busyEggs.has(id)) return;
    busyEggs.add(id); showCelebration(id);
    try {
      const result = await api<{ unlocked: boolean; collectorUnlocked: boolean; achievement: { name: string }; collectorAchievement?: { name: string } | null }>(`/api/student/easter-eggs/${encodeURIComponent(id)}`, { method: 'POST' });
      showCelebration(id, result.unlocked);
      // The API correctly returns `unlocked: false` for a repeat discovery, but
      // the interaction is still a discovery moment for the student. Replay
      // the celebration every time instead of making already-earned eggs feel
      // broken during testing or later visits.
      launchBirthdayConfetti();
      if (result.collectorUnlocked) window.setTimeout(() => { showCelebration('egg-master-hidden-dojo', true); launchBirthdayConfetti(); }, 2500);
    } catch (error) {
      console.warn('Easter egg achievement could not be saved:', apiError(error));
    } finally { busyEggs.delete(id); }
  }
  function handleEggEvent(event: Event) {
    const id = (event as CustomEvent<{ id?: EasterEggId }>).detail?.id;
    if (id && EASTER_EGG_BY_ID[id]) void unlockEasterEgg(id);
  }
  function handleBrandClick() { logoClicks += 1; if (logoClicks >= 5) { logoClicks = 0; triggerEasterEgg('egg-bell-finder'); } window.setTimeout(() => { logoClicks = 0; }, 3200); }
  function handleAvatarClick() { avatarClicks += 1; if (avatarClicks >= 4) { avatarClicks = 0; triggerEasterEgg('egg-card-carrying-student'); } window.setTimeout(() => { avatarClicks = 0; }, 2600); }
  function handleScrollIndicatorClick() { const now = Date.now(); if (now - lastEggClick > 2200) scrollIndicatorClicks = 0; lastEggClick = now; scrollIndicatorClicks += 1; if (scrollPercent >= 95 && scrollIndicatorClicks >= 3) { scrollIndicatorClicks = 0; triggerEasterEgg('egg-scroll-long-way'); } }
  function handleNavClick(id: string) {
    const now = Date.now();
    const sequenceKey = `task-karate-nav-sequence-${session.studentId}`;
    const visitedKey = `task-karate-visited-tabs-${session.studentId}`;
    const explorerKey = `task-karate-dojo-explorer-${session.studentId}`;
    const stored = sessionStorage.getItem(sequenceKey);
    const parsed = stored ? JSON.parse(stored) as { at: number; values: string[] } : null;
    navSequence = parsed && now - parsed.at < 10000 ? [...parsed.values, id].slice(-5) : [id];
    sessionStorage.setItem(sequenceKey, JSON.stringify({ at: now, values: navSequence }));
    if (navSequence.join(',') === 'profile,training,achievements,social,status') { navSequence = []; sessionStorage.removeItem(sequenceKey); triggerEasterEgg('egg-secret-kata'); }
    const visited = new Set(JSON.parse(sessionStorage.getItem(visitedKey) ?? '[]') as string[]); visited.add(id); sessionStorage.setItem(visitedKey, JSON.stringify([...visited]));
    if (visited.size === tabs.length && !sessionStorage.getItem(explorerKey)) {
      sessionStorage.setItem(explorerKey, 'shown');
      triggerEasterEgg('egg-dojo-explorer');
    }
  }
  function handleKeydown(event: KeyboardEvent) {
    if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); openCommandPalette(); }
    if (event.key === 'Escape') closeOverlays();
  }
  function enableReminder() {
    if (!('Notification' in window)) { reminderNotice = 'Browser notifications are not available here. Add the next class to your calendar instead.'; return; }
    Notification.requestPermission().then((permission) => {
      reminderNotice = permission === 'granted' ? 'Reminders are enabled while this hub is open.' : 'Notifications stayed off. You can still use Add to calendar.';
      if (permission === 'granted') writePreference('task-karate-reminders', true);
    });
  }
  async function refreshProfile() {
    try {
      currentProfile = await api<typeof currentProfile>('/api/student/profile');
      messagesUnread = currentProfile?.unreadMessages ?? 0;
    } catch { /* The shell can keep its last known badge state. */ }
  }
  function openMessages() {
    messagesOpen = true;
  }
  function handleMessagesRead() { refreshProfile(); }
  onMount(() => {
    theme = readPreference<HubTheme>('task-karate-hub-theme', 'midnight');
    accentColor = normalizeAccent(readPreference<string>(`task-karate-accent-${session.studentId}`, '#38bdf8'));
    avatarUrl = readPreference<string>(`task-karate-avatar-${session.studentId}`, '');
    window.addEventListener('keydown', handleKeydown);
    window.addEventListener('task-karate:easter-egg', handleEggEvent);
    refreshProfile();
    window.addEventListener('task-karate:messages-read', handleMessagesRead);
    if (session.birthdayWeek) {
      const celebrationKey = `task-karate-birthday-${session.loginId ?? session.studentId}`;
      if (!sessionStorage.getItem(celebrationKey)) {
        sessionStorage.setItem(celebrationKey, 'shown');
        birthdayCelebration = true;
        launchBirthdayConfetti();
        window.setTimeout(() => { birthdayCelebration = false; }, 9800);
      }
    }
    updateScrollProgress(); window.addEventListener('scroll', updateScrollProgress, { passive: true }); window.addEventListener('resize', updateScrollProgress); return () => { window.removeEventListener('keydown', handleKeydown); window.removeEventListener('task-karate:easter-egg', handleEggEvent); window.removeEventListener('task-karate:messages-read', handleMessagesRead); window.removeEventListener('scroll', updateScrollProgress); window.removeEventListener('resize', updateScrollProgress); };
  });
</script>

<svelte:head><meta name="theme-color" content="#081526" /></svelte:head>
<div class={`student-app active-${active} theme-${theme}`} style={`--hub-accent:${accentColor};--cyan:${accentColor};`}>
  <FloatingBubbles />
  <header class="student-topbar">
    <div class="brand"><button class="brand-mark" type="button" on:click={handleBrandClick} aria-label="Task Karate logo — hidden dojo bell"><img src="/images/logo/TASK_Logo.png" alt="" /></button><a href="/schedule" aria-label="Task Karate schedule"><strong>TASK KARATE</strong><small>STUDENT HUB</small></a></div>
    <nav class="floating-nav" data-student-nav aria-label="Student sections">
      {#each tabs as tab}<a class:active={active === tab.id} href={tab.href} on:click={() => handleNavClick(tab.id)} aria-current={active === tab.id ? 'page' : undefined}><span aria-hidden="true">{tab.id === 'status' ? '◉' : tab.id === 'social' ? '✦' : tab.id === 'achievements' ? '★' : tab.id === 'training' ? '➜' : '◎'}</span>{tab.label}</a>{/each}
    </nav>
    <button class="hub-header-messages" type="button" on:click={openMessages} aria-label={`Open Messages${messagesUnread ? `, ${messagesUnread} unread` : ''}`}><span aria-hidden="true">✉</span><span>Messages</span>{#if messagesUnread}<b>{messagesUnread > 99 ? '99+' : messagesUnread}</b>{/if}</button>
    <div class="student-account header-account">
      <button class="student-identity student-identity-link account-trigger" type="button" on:click={() => { accountOpen = !accountOpen; handleAvatarClick(); }} aria-expanded={accountOpen} aria-label="Open account options"><span class="identity-avatar">{#if avatarUrl}<img src={avatarUrl} alt="" />{:else}{initials(session.displayName)}{/if}</span><span class="identity-copy"><span class="identity-context">SIGNED IN AS</span><strong class="identity-name">{session.displayName ?? 'Student'}</strong><small>{currentProfile?.rankName ?? currentProfile?.programs?.find((item) => item.programCode === 'is3')?.levelName ?? 'Rank in progress'}</small></span><span class:open={accountOpen} class="account-chevron" aria-hidden="true"></span></button>
      {#if accountOpen}<div class="student-account-menu"><a class="account-menu-link" href="/student/profile">Open my profile</a><a class="account-menu-link" href="/student/password">Change student PIN</a><button class="account-menu-link" type="button" on:click={() => toolsOpen = !toolsOpen}>{toolsOpen ? 'Hide Hub Tools' : 'Hub Tools'}</button>{#if toolsOpen}<section class="hub-tools-panel" aria-label="Student Hub tools"><div class="panel-title"><span>APPEARANCE</span><button class="text-link" type="button" on:click={() => toolsOpen = false}>Close</button></div><div class="theme-choices">{#each HUB_THEMES as option}<button type="button" class:active={theme === option.id} on:click={() => setTheme(option.id)}><strong>{option.label}</strong><small>{option.description}</small></button>{/each}</div><label class="accent-picker"><span>ACCENT COLOR</span><div><input type="color" value={accentColor} aria-label="Choose accent color" on:input={(event) => setAccentColor((event.currentTarget as HTMLInputElement).value)} /><strong>{accentColor}</strong></div><small>Personalize the hub highlights.</small></label><div class="tools-actions"><button class="outline-button" type="button" on:click={enableReminder}>Enable class reminders</button></div>{#if reminderNotice}<p class="tools-notice" role="status">{reminderNotice}</p>{/if}</section>{/if}<button class="floating-logout" type="button" on:click={logout}><span aria-hidden="true">⏻</span> Log out</button></div>{/if}
    </div>
  </header>
  {#if error}<div class="student-toast error" role="alert">{error}</div>{/if}
  {#if birthdayCelebration}<div class="birthday-toast" role="status"><strong>Happy birthday week, {session.displayName ?? 'student'}! 🎉</strong><span>The whole dojo is celebrating you.</span></div>{/if}
  {#if commandOpen}<div class="command-backdrop" role="presentation" on:click={closeOverlays}><div class="command-palette" role="dialog" aria-modal="true" aria-labelledby="command-title" tabindex="-1" on:click|stopPropagation on:keydown|stopPropagation><div class="panel-title"><span id="command-title">JUMP TO</span><button class="modal-close" type="button" on:click={closeOverlays} aria-label="Close command palette">×</button></div>{#each tabs as tab}<a href={tab.href} on:click={closeOverlays}><span aria-hidden="true">{tab.id === 'status' ? '◉' : tab.id === 'social' ? '✦' : tab.id === 'achievements' ? '★' : tab.id === 'training' ? '➜' : '◎'}</span><strong>{tab.label}</strong><small>{tab.href}</small></a>{/each}<a href="/schedule" on:click={closeOverlays}><span aria-hidden="true">◷</span><strong>Class schedule</strong><small>/schedule</small></a></div></div>{/if}
  <ContextBackButton />
  <main id="student-content" class="student-main"><slot /></main>
  <StudentMessages {session} bind:open={messagesOpen} />
  <button class="scroll-rank-indicator" type="button" on:click={handleScrollIndicatorClick} aria-label={`Page progress · ${scrollBelt.name}`} style={`--scroll-progress:${scrollPercent}%;--scroll-color:${scrollBelt.color}`}><span class="scroll-rank-fill"></span></button>
  <EasterEggCelebration egg={activeEgg} {newlyUnlocked} studentName={session.displayName ?? 'Student'} rankName={currentProfile?.rankName ?? 'Rank in progress'} onClose={resetCelebration} />
</div>
