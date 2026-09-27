<script lang="ts">
  import { onMount } from 'svelte';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { api } from '$lib/api';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  import { rankColor, rankTextColor } from '$lib/rank-colors';
  import { DEFAULT_PROFILE_TILES, readPreference, writePreference } from '$lib/student-preferences';
  import { triggerEasterEgg } from '$lib/easter-eggs';

  let session: StudentSession | null = null;
  let profile: any = null;
  let error = '';
  let mottoIndex = 0;
  let mottoClicks = 0;
  let mottoLastClick = 0;
  const dojoMottos = ['Train with intention.', 'Breathe first. Punch later.', 'Your future self has better balance.', 'A clean stance is a tiny victory.', 'The mat remembers the work.'];
  let editing = false;
  let saving = false;
  let changingPin = false;
  let pinModalOpen = false;
  let pinNotice = '';
  let pinError = '';
  let publicProfileNotice = '';
  let form = { displayName: '', nickname: '', pronouns: '', bio: '', favoriteTechnique: '', email: '', phone: '', uniformSize: '', beltSize: '' };
  let pinForm = { currentPin: '', newPin: '', confirmPin: '' };
  type GuardianRecord = { name: string; relationship: string; phone?: string; email?: string };
  type EmergencyContactRecord = { contactId: number; name: string; firstName?: string; middleInitial?: string; lastName?: string; pronouns?: string; relationship: string; phone?: string; email?: string; newsletterOptIn?: boolean; smsOptIn?: boolean };
  let uniqueGuardians: GuardianRecord[] = [];
  let emergencyModalOpen = false;
  let emergencySaving = false;
  let emergencyError = '';
  let editingEmergencyContactId: number | null = null;
  let emergencyForm = { name: '', firstName: '', middleInitial: '', lastName: '', pronouns: '', relationship: '', phone: '', email: '', newsletterOptIn: false, smsOptIn: false };
  let customizeTiles = false;
  let selectedTiles = [...DEFAULT_PROFILE_TILES];
  let goals: any[] = [];
  const defaultPublicTileIds = ['rank', 'next-stripe', 'total-classes', 'monthly-classes', 'streak', 'achievements', 'programs', 'favorite-technique', 'dojo-motto'];
  const publicTileAllowlist = new Set(['rank', 'next-stripe', 'total-classes', 'helper-classes', 'monthly-classes', 'streak', 'programs', 'favorite-technique', 'achievements', 'training-track', 'dojo-motto']);
  let publicTileIds = [...defaultPublicTileIds];
  type TileSize = 'small' | 'medium' | 'wide' | 'feature';
  type DashboardLayout = 'balanced' | 'spotlight' | 'scrapbook';
  type DashboardSection = 'Training momentum' | 'Dojo life' | 'About me' | 'Recognition';
  type DashboardTileType = 'progress' | 'stat' | 'story' | 'spotlight' | 'collection' | 'quote';
  let tileSizes: Record<string, TileSize> = {};
  let dashboardLayout: DashboardLayout = 'balanced';
  let draggingTileId = '';
  let dragOverTileId = '';
  let resizingTile: { id: string; startX: number; startIndex: number } | null = null;
  const tileSizeOrder: TileSize[] = ['small', 'medium', 'wide', 'feature'];
  const tileSizeSpans: Record<TileSize, number> = { small: 3, medium: 4, wide: 6, feature: 12 };
  const tileSections: Record<string, DashboardSection> = {
    rank: 'Training momentum', 'next-stripe': 'Training momentum', 'total-classes': 'Training momentum', 'helper-classes': 'Training momentum', 'monthly-classes': 'Training momentum', streak: 'Training momentum', 'profile-readiness': 'Training momentum', 'weekly-goal': 'Training momentum', 'attendance-rate': 'Training momentum', 'last-attended': 'Training momentum', 'daily-mission': 'Training momentum', 'practice-minutes': 'Training momentum', 'training-track': 'Training momentum',
    'next-class': 'Dojo life', 'active-goals': 'Dojo life', programs: 'Dojo life', 'favorite-technique': 'Dojo life', unread: 'Dojo life', 'goals-completed': 'Dojo life', 'dojo-motto': 'Dojo life',
    'member-since': 'About me', 'age-group': 'About me', birthday: 'About me', uniform: 'About me', 'belt-size': 'About me', bio: 'About me', email: 'About me', phone: 'About me', guardians: 'About me',
    achievements: 'Recognition', 'gold-stars': 'Recognition'
  };
  const tileKinds: Record<string, DashboardTileType> = {
    rank: 'progress', 'next-stripe': 'progress', 'profile-readiness': 'progress', 'weekly-goal': 'progress', 'attendance-rate': 'progress', 'training-track': 'progress',
    'total-classes': 'stat', 'helper-classes': 'stat', 'monthly-classes': 'stat', streak: 'stat', 'member-since': 'stat', 'age-group': 'stat', birthday: 'stat', uniform: 'stat', 'belt-size': 'stat', email: 'stat', phone: 'stat', unread: 'stat', 'last-attended': 'stat',
    bio: 'story', 'favorite-technique': 'story', 'dojo-motto': 'quote',
    'next-class': 'spotlight', 'active-goals': 'spotlight', 'daily-mission': 'spotlight', 'practice-minutes': 'spotlight',
    programs: 'collection', guardians: 'collection', achievements: 'collection', 'gold-stars': 'collection', 'goals-completed': 'collection'
  };
  const dashboardSectionMeta: Record<DashboardSection, { icon: string; description: string }> = {
    'Training momentum': { icon: '↗', description: 'The rhythm you are building on the mat.' },
    'Dojo life': { icon: '✦', description: 'The people, plans, and practice around your training.' },
    'About me': { icon: '◎', description: 'The details that make this space yours.' },
    Recognition: { icon: '★', description: 'Wins and moments worth keeping.' }
  };
  const defaultTileSizes: Record<string, TileSize> = { rank: 'feature', 'next-stripe': 'medium', 'total-classes': 'small', 'helper-classes': 'small', 'monthly-classes': 'small', streak: 'small', 'next-class': 'wide', 'profile-readiness': 'wide', programs: 'medium' };
  const tileGroups: Record<string, string> = { rank: 'Training progress', 'next-stripe': 'Training progress', 'total-classes': 'Training progress', 'helper-classes': 'Training progress', 'monthly-classes': 'Training progress', streak: 'Training progress', 'next-class': 'Dojo life', 'active-goals': 'Dojo life', programs: 'Dojo life', 'favorite-technique': 'Dojo life', 'profile-readiness': 'Profile details', 'member-since': 'Profile details', 'age-group': 'Profile details', birthday: 'Profile details', uniform: 'Profile details', 'belt-size': 'Profile details', bio: 'Profile details', email: 'Profile details', phone: 'Profile details', guardians: 'Profile details', unread: 'Dojo life', achievements: 'Recognition', 'gold-stars': 'Recognition', 'weekly-goal': 'Training progress', 'attendance-rate': 'Training progress', 'last-attended': 'Training progress', 'daily-mission': 'Training progress', 'practice-minutes': 'Training progress', 'goals-completed': 'Dojo life', 'training-track': 'Training progress', 'dojo-motto': 'Dojo life' };
  let avatarUrl = '';
  let avatarError = '';
  const tileCatalog = [
    ['rank', 'Current rank', 'Your current belt or progression level.'], ['next-stripe', 'Next stripe', 'Classes remaining in your current stripe.'], ['total-classes', 'Total classes', 'Your all-time recorded attendance.'], ['helper-classes', 'Helper classes', 'Classes where you supported the dojo; these do not count toward stripes.'], ['monthly-classes', 'This month', 'Classes completed this calendar month.'], ['streak', 'Attendance streak', 'Your current day-by-day rhythm.'], ['next-class', 'Next class', 'The next published class for you.'], ['active-goals', 'Active goals', 'Training targets you are still working toward.'], ['programs', 'Programs', 'Your active karate and IS3 tracks.'], ['favorite-technique', 'Favorite technique', 'The technique you chose in your profile.'], ['profile-readiness', 'Profile readiness', 'How complete your student record is.'],
    ['member-since', 'Member since', 'The date your Task Karate journey began.'], ['age-group', 'Age group', 'The program age group on file.'], ['birthday', 'Birthday', 'Your birthday, visible only to your signed-in account.'], ['uniform', 'Uniform size', 'Your preferred uniform size.'], ['belt-size', 'Belt size', 'Your belt size for gear planning.'], ['bio', 'Training story', 'Your short personal dojo biography.'], ['email', 'Email', 'The best email address on file.'], ['phone', 'Phone', 'The best phone number on file.'], ['guardians', 'Guardians', 'Your linked guardian relationships.'], ['unread', 'Unread messages', 'Messages waiting in Social.'], ['achievements', 'Achievements', 'Milestones recorded by the dojo.'], ['gold-stars', 'Gold Stars', 'Dojo-recognized moments you earned.'], ['weekly-goal', 'Weekly rhythm', 'Classes attended this week compared with your target.'], ['attendance-rate', '30-day attendance', 'Your recent attendance percentage.'], ['last-attended', 'Last attended', 'The latest date in your attendance record.'], ['daily-mission', 'Practice mission', 'A nudge for your next focused practice.'], ['practice-minutes', 'Practice minutes', 'A reminder to log the work between classes.'], ['goals-completed', 'Goals completed', 'Training goals you have checked off.'], ['training-track', 'Training track', 'The progression track you are currently following.'], ['dojo-motto', 'Dojo motto', 'A small piece of Task Karate spirit.']
  ] as [string, string, string][];

  onMount(async () => {
    session = await requireStudent();
    if (!session) return;
    selectedTiles = readPreference<string[]>(`task-karate-profile-tiles-${session.studentId}`, DEFAULT_PROFILE_TILES);
    tileSizes = readPreference<Record<string, TileSize>>(`task-karate-profile-tile-sizes-${session.studentId}`, defaultTileSizes);
    dashboardLayout = readPreference<DashboardLayout>(`task-karate-profile-layout-${session.studentId}`, 'balanced');
    avatarUrl = readPreference<string>(`task-karate-avatar-${session.studentId}`, '');
    try {
      profile = await api('/api/student/profile');
      publicTileIds = (profile.publicTileIds ?? defaultPublicTileIds).filter((id: string) => publicTileAllowlist.has(id));
      if (!publicTileIds.length) publicTileIds = [...defaultPublicTileIds];
      if (!profile.programs?.length && profile.rankName) profile = { ...profile, programs: [{ programName: 'Task Karate', programCode: 'karate', progressionType: 'belt', levelName: profile.rankName, enrolledDate: profile.joinDate }] };
      try { goals = await api<any[]>('/api/student/goals'); } catch { goals = []; }
      void persistShowcase();
    } catch (e) { error = apiError(e); }
  });
  $: uniqueGuardians = profile?.guardians ? Array.from(new Map<string, GuardianRecord>(profile.guardians.map((guardian: GuardianRecord) => [`${guardian.name}|${guardian.relationship}|${guardian.email ?? ''}`, guardian])).values()) : [];
  $: emergencyContacts = (profile?.emergencyContacts ?? []) as EmergencyContactRecord[];
  $: isAdult = profile ? (profile.birthDate ? new Date(profile.birthDate).getTime() <= new Date(new Date().setFullYear(new Date().getFullYear() - 18)).getTime() : profile.ageGroup?.trim().toLowerCase() === 'adult') : false;
  $: profileFields = [profile?.email, profile?.phone, profile?.uniformSize, profile?.beltSize, isAdult || uniqueGuardians.length > 0, profile?.programs?.length];
  $: profileCompletion = profile ? Math.round((profileFields.filter(Boolean).length / profileFields.length) * 100) : 0;
  $: tileById = new Map(tileCatalog.map(([id, label, description]) => [id, { id, label, description, group: tileGroups[id] ?? 'Profile details' }]));
  $: visibleTiles = selectedTiles.filter((id) => profileCompletion < 100 || id !== 'profile-readiness').map((id) => tileById.get(id)).filter(Boolean) as { id: string; label: string; description: string; group: string }[];
  $: groupedTiles = [...new Set(visibleTiles.map((tile) => tile.group))].map((group) => ({ group, tiles: visibleTiles.filter((tile) => tile.group === group) }));
  $: dashboardSections = (Object.keys(dashboardSectionMeta) as DashboardSection[]).map((section) => ({ section, ...dashboardSectionMeta[section], tiles: visibleTiles.filter((tile) => tileSections[tile.id] === section) })).filter((group) => group.tiles.length);
  $: dashboardHandle = profile?.nickname?.trim() || profile?.displayName?.split(' ')[0] || 'Student';
  $: dashboardBio = profile?.bio?.trim() || 'Training with intention, one class at a time.';
  $: dashboardTechnique = profile?.favoriteTechnique?.trim() || 'Choose a favorite technique';
  $: attendanceDates = profile?.attendanceDates ?? [];
  $: attendanceRate = Math.round(Math.min(100, (attendanceDates.length / 30) * 100));
  $: weeklyClasses = attendanceDates.filter((value: string) => new Date(`${value}T12:00:00`).getTime() >= new Date(Date.now() - 6 * 86400000).setHours(0, 0, 0, 0)).length;
  $: attendanceStreak = (() => { const dates = [...new Set(attendanceDates)].sort().reverse(); if (!dates.length) return 0; let streak = 1; for (let index = 1; index < dates.length; index += 1) { const previous = new Date(`${dates[index - 1]}T12:00:00`); const current = new Date(`${dates[index]}T12:00:00`); if (Math.round((previous.getTime() - current.getTime()) / 86400000) !== 1) break; streak += 1; } return streak; })();
  function beginEdit() { form = { displayName: profile.displayName ?? '', nickname: profile.nickname ?? '', pronouns: profile.pronouns ?? '', bio: profile.bio ?? '', favoriteTechnique: profile.favoriteTechnique ?? '', email: profile.email ?? '', phone: profile.phone ?? '', uniformSize: profile.uniformSize ?? '', beltSize: profile.beltSize ?? '' }; editing = true; }
  async function saveProfile() { saving = true; error = ''; try { profile = await api('/api/student/profile', { method: 'PUT', body: JSON.stringify(form) }); editing = false; } catch (e) { error = apiError(e); } finally { saving = false; } }
  function openEmergencyModal(contact?: EmergencyContactRecord) { editingEmergencyContactId = contact?.contactId ?? null; emergencyForm = { name: contact?.name ?? '', firstName: contact?.firstName ?? '', middleInitial: contact?.middleInitial ?? '', lastName: contact?.lastName ?? '', pronouns: contact?.pronouns ?? '', relationship: contact?.relationship ?? '', phone: contact?.phone ?? '', email: contact?.email ?? '', newsletterOptIn: contact?.newsletterOptIn ?? false, smsOptIn: contact?.smsOptIn ?? false }; emergencyError = ''; emergencyModalOpen = true; }
  async function saveEmergencyContact() { emergencySaving = true; emergencyError = ''; try { const path = editingEmergencyContactId ? `/api/student/profile/emergency-contacts/${editingEmergencyContactId}` : '/api/student/profile/emergency-contacts'; await api(path, { method: editingEmergencyContactId ? 'PUT' : 'POST', body: JSON.stringify(emergencyForm) }); profile = await api('/api/student/profile'); emergencyModalOpen = false; } catch (e) { emergencyError = apiError(e); } finally { emergencySaving = false; } }
  async function deleteEmergencyContact(contact: EmergencyContactRecord) { if (!window.confirm(`Remove ${contact.name} from your emergency contacts?`)) return; try { await api(`/api/student/profile/emergency-contacts/${contact.contactId}`, { method: 'DELETE' }); profile = await api('/api/student/profile'); } catch (e) { error = apiError(e); } }
  async function changePin() { changingPin = true; pinError = ''; pinNotice = ''; try { await api('/api/student/auth/pin', { method: 'POST', body: JSON.stringify(pinForm) }); pinForm = { currentPin: '', newPin: '', confirmPin: '' }; pinModalOpen = false; pinNotice = 'Student PIN changed for both Hub access and class check-in.'; } catch (e) { pinError = apiError(e); } finally { changingPin = false; } }
  async function persistShowcase() { if (!session) return; try { await api('/api/student/profile/showcase', { method: 'PUT', body: JSON.stringify({ tileIds: publicTileIds, tileSizes }) }); } catch { /* Keep the device-local dashboard usable if the API is unavailable. */ } }
  function toggleTile(id: string) { const isSelected = selectedTiles.includes(id); selectedTiles = isSelected ? selectedTiles.filter((item) => item !== id) : [...selectedTiles, id]; if (isSelected) publicTileIds = publicTileIds.filter((item) => item !== id); writePreference(`task-karate-profile-tiles-${session?.studentId}`, selectedTiles); void persistShowcase(); }
  function togglePublicTile(id: string) { if (!publicTileAllowlist.has(id)) return; publicTileIds = publicTileIds.includes(id) ? publicTileIds.filter((item) => item !== id) : [...publicTileIds, id]; void persistShowcase(); }
  function setTileSize(id: string, size: TileSize) { tileSizes = { ...tileSizes, [id]: size }; writePreference(`task-karate-profile-tile-sizes-${session?.studentId}`, tileSizes); void persistShowcase(); }
  function applyDashboardLayout(layout: DashboardLayout) {
    dashboardLayout = layout;
    const priority = layout === 'spotlight'
      ? ['rank', 'bio', 'favorite-technique', 'dojo-motto', 'next-class', 'active-goals']
      : layout === 'scrapbook'
        ? ['dojo-motto', 'favorite-technique', 'bio', 'rank', 'next-class', 'achievements']
        : ['rank', 'next-class', 'weekly-goal', 'attendance-rate', 'favorite-technique', 'dojo-motto'];
    selectedTiles = [...selectedTiles].sort((a, b) => (priority.indexOf(a) < 0 ? 999 : priority.indexOf(a)) - (priority.indexOf(b) < 0 ? 999 : priority.indexOf(b)));
    const preset: Record<string, TileSize> = { ...tileSizes };
    selectedTiles.forEach((id, index) => {
      if (layout === 'spotlight') preset[id] = ['rank', 'bio', 'favorite-technique', 'dojo-motto'].includes(id) ? 'feature' : index < 6 ? 'medium' : 'small';
      else if (layout === 'scrapbook') preset[id] = index % 5 === 0 ? 'wide' : index % 3 === 0 ? 'medium' : 'small';
      else preset[id] = id === 'rank' ? 'feature' : ['next-class', 'weekly-goal', 'attendance-rate'].includes(id) ? 'wide' : index < 7 ? 'medium' : 'small';
    });
    tileSizes = preset;
    writePreference(`task-karate-profile-tiles-${session?.studentId}`, selectedTiles);
    writePreference(`task-karate-profile-tile-sizes-${session?.studentId}`, tileSizes);
    writePreference(`task-karate-profile-layout-${session?.studentId}`, dashboardLayout);
    void persistShowcase();
  }
  function moveTile(id: string, direction: -1 | 1) { const index = selectedTiles.indexOf(id); const nextIndex = index + direction; if (index < 0 || nextIndex < 0 || nextIndex >= selectedTiles.length) return; const next = [...selectedTiles]; [next[index], next[nextIndex]] = [next[nextIndex], next[index]]; selectedTiles = next; writePreference(`task-karate-profile-tiles-${session?.studentId}`, selectedTiles); void persistShowcase(); }
  function tileSection(id: string) { return tileSections[id] ?? 'About me'; }
  function tileKind(id: string) { return tileKinds[id] ?? 'stat'; }
  function tileSizeIndex(id: string) { return tileSizeOrder.indexOf(tileSizes[id] ?? defaultTileSizes[id] ?? 'small'); }
  function setTileSizeByIndex(id: string, index: number) { const nextIndex = Math.max(0, Math.min(tileSizeOrder.length - 1, index)); setTileSize(id, tileSizeOrder[nextIndex]); }
  function resizeTile(id: string, direction: -1 | 1) { setTileSizeByIndex(id, tileSizeIndex(id) + direction); }
  function startTileResize(event: PointerEvent, id: string) { resizingTile = { id, startX: event.clientX, startIndex: tileSizeIndex(id) }; window.addEventListener('pointermove', handleTileResize); window.addEventListener('pointerup', endTileResize, { once: true }); }
  function handleTileResize(event: PointerEvent) { if (!resizingTile) return; const step = Math.round((event.clientX - resizingTile.startX) / 70); setTileSizeByIndex(resizingTile.id, resizingTile.startIndex + step); }
  function endTileResize() { resizingTile = null; window.removeEventListener('pointermove', handleTileResize); }
  function beginTileDrag(event: DragEvent, id: string) { draggingTileId = id; event.dataTransfer?.setData('text/plain', id); if (event.dataTransfer) event.dataTransfer.effectAllowed = 'move'; }
  function dropTile(id: string) { if (!draggingTileId || draggingTileId === id) return; const next = [...selectedTiles]; const from = next.indexOf(draggingTileId); const to = next.indexOf(id); if (from < 0 || to < 0) return; next.splice(from, 1); next.splice(to, 0, draggingTileId); selectedTiles = next; writePreference(`task-karate-profile-tiles-${session?.studentId}`, selectedTiles); void persistShowcase(); draggingTileId = ''; dragOverTileId = ''; }
  function finishTileDrag() { draggingTileId = ''; dragOverTileId = ''; }
  function resetTiles() { selectedTiles = [...DEFAULT_PROFILE_TILES]; publicTileIds = [...defaultPublicTileIds]; applyDashboardLayout('balanced'); }
  function openCustomizer() { customizeTiles = true; window.setTimeout(() => document.querySelector('.profile-customizer')?.scrollIntoView({ behavior: 'smooth', block: 'start' }), 0); }
  async function copyPublicProfileLink() { if (!session) return; const url = `${window.location.origin}/student/profile/${session.studentId}`; try { await navigator.clipboard.writeText(url); publicProfileNotice = 'Public profile link copied.'; } catch { publicProfileNotice = url; } window.setTimeout(() => publicProfileNotice = '', 3500); }
  function tileValue(id: string) { const values: Record<string, string> = { rank: profile?.rankName ?? 'Rank in progress', 'next-stripe': profile?.classesToNextStripe ? `${profile.classesToNextStripe} classes to go` : 'Instructor tracked', 'total-classes': `${profile?.totalClasses ?? 0} classes`, 'helper-classes': `${profile?.helperClasses ?? 0} classes helped`, 'monthly-classes': `${profile?.classesThisMonth ?? 0} classes`, streak: `${attendanceStreak} days`, 'next-class': 'Open Status for the next class', 'active-goals': `${goals.filter((goal: any) => !goal.completed).length} active`, programs: `${profile?.programs?.length ?? 0} tracks`, 'favorite-technique': profile?.favoriteTechnique ?? 'Add one in Edit profile', 'profile-readiness': `${profileCompletion}% complete`, 'member-since': profile?.joinDate ?? 'On file', 'age-group': profile?.ageGroup ?? 'On file', birthday: profile?.birthDate ? new Date(profile.birthDate).toLocaleDateString() : 'On file', uniform: profile?.uniformSize ?? 'Not set', 'belt-size': profile?.beltSize ?? 'Not set', bio: profile?.bio ?? 'Add a short story', email: profile?.email ?? 'Not set', phone: profile?.phone ?? 'Not set', guardians: `${uniqueGuardians.length} linked`, unread: `${profile?.unreadMessages ?? 0} unread`, achievements: `${profile?.achievementCount ?? 0} earned`, 'gold-stars': `${profile?.goldStarCount ?? 0} earned`, 'weekly-goal': `${weeklyClasses} / 2 classes`, 'attendance-rate': `${attendanceRate}%`, 'last-attended': attendanceDates.length ? new Date(`${attendanceDates[attendanceDates.length - 1]}T12:00:00`).toLocaleDateString() : 'No record yet', 'daily-mission': 'Open Training for today’s mission', 'practice-minutes': 'Log your between-class reps', 'goals-completed': `${goals.filter((goal: any) => goal.completed).length} completed`, 'training-track': profile?.programs?.map((program: any) => program.programName).join(' · ') || 'Karate progression', 'dojo-motto': dojoMottos[mottoIndex] }; return values[id] ?? 'Add detail'; }
  function tileLink(id: string) { return ({ rank: '/student/training', 'next-stripe': '/student/training', 'total-classes': '/student', 'helper-classes': '/student/training', 'monthly-classes': '/student', streak: '/student', 'next-class': '/schedule', 'active-goals': '/student/training', programs: '/student/training', 'profile-readiness': '/student/profile', unread: '/student/social', achievements: '/student/achievements', 'gold-stars': '/student/achievements', 'weekly-goal': '/student', 'attendance-rate': '/student', 'last-attended': '/student', 'daily-mission': '/student/training', 'practice-minutes': '/student/training', 'goals-completed': '/student/training', 'training-track': '/student/training' } as Record<string, string>)[id] ?? ''; }
  function tileProgress(id: string) { if (id === 'next-stripe' && profile?.classesPerStripe) return Math.round(Math.min(100, (profile.classesIntoStripe / profile.classesPerStripe) * 100)); if (id === 'weekly-goal') return Math.round(Math.min(100, (weeklyClasses / 2) * 100)); if (id === 'attendance-rate' || id === 'profile-readiness') return id === 'attendance-rate' ? attendanceRate : profileCompletion; return null; }
  function tileIcon(id: string) { return ({ rank: '◈', 'next-stripe': '↗', 'total-classes': '◎', 'helper-classes': '✦', 'monthly-classes': '▦', streak: '⚡', 'next-class': '→', 'active-goals': '◆', programs: '⌘', 'favorite-technique': '✺', 'profile-readiness': '✓', birthday: '✹', achievements: '★', 'gold-stars': '✦', 'daily-mission': '➜', 'training-track': '⌁', 'dojo-motto': '☼' } as Record<string, string>)[id] ?? '·'; }
  function clickMotto() { const now = Date.now(); if (now - mottoLastClick > 2600) mottoClicks = 0; mottoLastClick = now; mottoClicks += 1; mottoIndex = (mottoIndex + 1) % dojoMottos.length; if (mottoClicks >= 5) { mottoClicks = 0; triggerEasterEgg('egg-wall-wisdom'); } }
  function uploadAvatar(event: Event) { const input = event.currentTarget as HTMLInputElement; const file = input.files?.[0]; avatarError = ''; if (!file) return; if (!file.type.startsWith('image/')) { avatarError = 'Choose an image file.'; return; } if (file.size > 2_000_000) { avatarError = 'Choose an image under 2 MB.'; return; } const reader = new FileReader(); reader.onload = () => { avatarUrl = String(reader.result); writePreference(`task-karate-avatar-${session?.studentId}`, avatarUrl); }; reader.readAsDataURL(file); }
  function removeAvatar() { avatarUrl = ''; writePreference(`task-karate-avatar-${session?.studentId}`, ''); }
</script>

<svelte:head><title>Task Karate | My profile</title></svelte:head>

{#if session}
  <StudentShell {session} active="profile">
    <section class="student-heading profile-heading"><div><span class="student-eyebrow">YOUR DOJO PROFILE</span><h1>MY PROFILE</h1><p>A personal view of your rank, training record, and the story you are building at Task Karate.</p></div><div class="profile-heading-actions"><a class="primary-button profile-action profile-public-view" href={`/student/profile/${session.studentId}`}>View public profile</a><details class="profile-actions-menu"><summary aria-label="More profile actions"><span aria-hidden="true">•••</span><span>More</span></summary><div class="profile-actions-popover"><button type="button" on:click={copyPublicProfileLink}>Copy public link</button><button type="button" on:click={openCustomizer}>Edit profile tiles</button></div></details></div></section>
    {#if publicProfileNotice}<div class="status profile-public-notice" role="status">{publicProfileNotice}</div>{/if}
    {#if error}<div class="error" role="alert">{error}</div>{:else if !profile}<div class="student-panel loading-panel">Loading your profile…</div>{:else}
      {#if profileCompletion < 100}<section class="student-panel profile-completion-panel"><div><span class="student-eyebrow">PROFILE READINESS</span><h2>{profileCompletion}% complete</h2><p>Keep your contact, uniform, and training details current so the dojo can support you.</p></div><div class="profile-completion-actions"><div class="mini-progress"><span style={`width:${profileCompletion}%`}></span></div><button class="primary-button" type="button" on:click={beginEdit}>{editing ? 'Editing below' : 'Complete my profile'}</button></div></section>{/if}
      {#if editing}<section class="student-panel profile-editor"><div class="panel-title"><span>EDIT PROFILE</span><span class="muted">You can update these details anytime.</span></div><form class="profile-form" on:submit|preventDefault={saveProfile}><label>Display name<input bind:value={form.displayName} maxlength="100" /></label><label>Nickname <small class="muted">optional</small><input bind:value={form.nickname} maxlength="100" placeholder="What friends and classmates call you" /></label><label>Pronouns <small class="muted">optional</small><input bind:value={form.pronouns} maxlength="50" placeholder="e.g. she/her, they/them" /></label><label>Email<input type="email" bind:value={form.email} maxlength="254" /></label><label>Phone<input bind:value={form.phone} maxlength="50" /></label><label>Uniform size<input bind:value={form.uniformSize} maxlength="50" placeholder="e.g. Youth Medium" /></label><label>Belt size<input bind:value={form.beltSize} maxlength="50" placeholder="e.g. Size 2" /></label><label>Favorite technique<input bind:value={form.favoriteTechnique} maxlength="100" placeholder="e.g. Roundhouse kick" /></label><label class="full">Short bio<textarea bind:value={form.bio} maxlength="500" rows="3" placeholder="A sentence about your training journey…"></textarea></label><div class="profile-form-actions"><button class="outline-button" type="button" on:click={() => editing = false}>Cancel</button><button class="primary-button" type="submit" disabled={saving}>{saving ? 'Saving…' : 'Save profile'}</button></div></form></section>{/if}
      <div class="profile-layout">
        <section class="student-panel profile-card">
          <div class="profile-avatar-wrap"><span class="profile-avatar">{#if avatarUrl}<img src={avatarUrl} alt="Profile" />{:else}{profile.displayName?.slice(0, 2).toUpperCase()}{/if}</span><label class="avatar-upload"><input type="file" accept="image/png,image/jpeg,image/webp" on:change={uploadAvatar} />{avatarUrl ? 'Change photo' : 'Add profile photo'}</label>{#if avatarUrl}<button class="text-link avatar-remove" type="button" on:click={removeAvatar}>Remove photo</button>{/if}{#if avatarError}<small class="avatar-error" role="alert">{avatarError}</small>{/if}</div>
          <span class="student-eyebrow">STUDENT</span>
          <h2>{profile.displayName}</h2>
          <p>{profile.bio ?? 'Your instructor can add a short dojo biography here.'}</p>
      <div class="profile-tags"><span class="belt-pill" style={`--belt-color:${rankColor(profile.rankName)};--belt-text:${rankTextColor(profile.rankName)}`}>{profile.rankName ?? 'Rank in progress'}</span><span>Member since {profile.joinDate ?? 'on file'}</span>{#if profile.pronouns}<span>{profile.pronouns}</span>{/if}</div>
        </section>
        <section class="student-panel profile-details">
          <div class="panel-title"><span>TRAINING RECORD</span><a href="/student/training">Open training →</a></div>
          <div class="record-list"><div><span>Total classes</span><strong>{profile.totalClasses}</strong></div><div><span>Helper classes</span><strong>{profile.helperClasses ?? 0}</strong></div><div><span>Current stripe</span><strong>{profile.classesIntoStripe} / {profile.classesPerStripe || '—'}</strong></div><div><span>Next milestone</span><strong>{profile.nextMilestone}</strong></div><div><span>Classes to go</span><strong>{profile.classesToNextStripe || 'Instructor tracked'}</strong></div></div>
        </section>
      </div>
      <div class="profile-info-grid">
        <section class="student-panel"><div class="panel-title"><span>UNIFORM & BELT</span></div><div class="record-list"><div><span>Uniform size</span><strong>{profile.uniformSize ?? 'Not on file'}</strong></div><div><span>Belt size</span><strong>{profile.beltSize ?? 'Not on file'}</strong></div><div><span>Age group</span><strong>{profile.ageGroup ?? 'Not on file'}</strong></div><div><span>Birthday</span><strong>{profile.birthDate ? new Date(profile.birthDate).toLocaleDateString() : 'Not on file'}</strong></div></div></section>
        <section class="student-panel">{#if isAdult}<div class="panel-title"><span>EMERGENCY CONTACTS</span><button class="text-link" type="button" on:click={() => openEmergencyModal()}>+ Add contact</button></div>{#if emergencyContacts.length}<div class="guardian-list">{#each emergencyContacts as contact}<div class="guardian-row"><div class="guardian-row-heading"><strong>{contact.name}</strong><span>{contact.relationship}</span></div>{#if contact.phone}<small>{contact.phone}</small>{/if}{#if contact.email}<small>{contact.email}</small>{/if}<div class="guardian-row-actions"><button class="text-link" type="button" on:click={() => openEmergencyModal(contact)}>Edit</button><button class="text-link danger-link" type="button" on:click={() => deleteEmergencyContact(contact)}>Remove</button></div></div>{/each}</div>{:else}<p class="muted">No emergency contacts added yet. Add someone the dojo should reach if needed.</p>{/if}{:else}<div class="panel-title"><span>GUARDIANS</span><span class="muted">Staff-managed relationships</span></div>{#if uniqueGuardians.length}<div class="guardian-list">{#each uniqueGuardians as guardian}<div class="guardian-row"><strong>{guardian.name}</strong><span>{guardian.relationship}</span>{#if guardian.phone}<small>{guardian.phone}</small>{/if}{#if guardian.email}<small>{guardian.email}</small>{/if}</div>{/each}</div>{:else}<p class="muted">No guardian record is linked yet. Ask staff to connect a guardian account.</p>{/if}{/if}</section>
      </div>
      <section class="student-panel program-memberships"><div class="panel-title"><span>PROGRAMS & LEVELS</span><span class="muted">Your progress can span both tracks</span></div>{#if profile.programs?.length}<div class="program-membership-grid">{#each profile.programs as program}<div class="program-membership"><span class="program-code">{program.programCode}</span><div><strong>{program.programName}</strong><span>{program.progressionType === 'level' ? 'Level track' : 'Belt track'}</span><small>{program.levelName ?? 'Level in progress'}</small></div></div>{/each}</div>{:else}<p class="muted">No program memberships are on file yet.</p>{/if}</section>
      {#if profile.email || profile.phone}<section class="student-panel profile-contact"><div class="panel-title"><span>CONTACT ON FILE</span></div><div class="profile-contact-values">{#if profile.email}<span>{profile.email}</span>{/if}{#if profile.phone}<span>{profile.phone}</span>{/if}</div></section>{/if}
      <section class="student-panel profile-customizer"><div class="panel-title"><span>PROFILE CUSTOMIZATION</span><span class="muted">{selectedTiles.length} of {tileCatalog.length} tiles visible</span></div><div class="customizer-intro"><div><h2>Build your dashboard.</h2><p>Choose details, arrange them, and give important tiles more room. Public-safe choices also appear on your community profile; private details stay private.</p></div><button class="outline-button" type="button" on:click={() => customizeTiles = !customizeTiles}>{customizeTiles ? 'Done customizing' : 'Choose tiles'}</button></div>{#if customizeTiles}<div class="tile-editor"><div class="tile-editor-toolbar"><span class="muted">Choose your vibe, then fine-tune individual tiles below.</span><div class="dashboard-layout-tools"><label>Layout vibe<select value={dashboardLayout} on:change={(event) => applyDashboardLayout((event.currentTarget as HTMLSelectElement).value as DashboardLayout)}><option value="balanced">Balanced dojo</option><option value="spotlight">Spotlight story</option><option value="scrapbook">Scrapbook shuffle</option></select></label><button class="text-link" type="button" on:click={resetTiles}>Reset to recommended</button></div></div><div class="tile-catalog">{#each tileCatalog as [id, label, description]}<label class:selected={selectedTiles.includes(id)} class="tile-option"><input type="checkbox" checked={selectedTiles.includes(id)} on:change={() => toggleTile(id)} /><span><strong>{label}</strong><small>{description}</small></span>{#if publicTileAllowlist.has(id)}<span class="tile-public-toggle"><input type="checkbox" checked={publicTileIds.includes(id)} on:change|stopPropagation={() => togglePublicTile(id)} /><small>Public</small></span>{:else}<span class="tile-private-note">Private</span>{/if}{#if selectedTiles.includes(id)}<select value={tileSizes[id] ?? defaultTileSizes[id] ?? 'small'} on:click|stopPropagation on:change={(event) => setTileSize(id, (event.currentTarget as HTMLSelectElement).value as TileSize)} aria-label={`Size for ${label}`}><option value="small">Small</option><option value="medium">Medium</option><option value="wide">Wide</option><option value="feature">Feature</option></select><span class="tile-order"><button type="button" aria-label={`Move ${label} earlier`} on:click|stopPropagation={() => moveTile(id, -1)}>↑</button><button type="button" aria-label={`Move ${label} later`} on:click|stopPropagation={() => moveTile(id, 1)}>↓</button></span>{/if}</label>{/each}</div></div>{/if}</section>
      <section class={`dashboard-board dashboard-style-${dashboardLayout}`}>
        <div class="dashboard-board-header">
          <div><span class="student-eyebrow">MY DASHBOARD</span><h2>{dashboardHandle}'s dojo board</h2><p>A personal space for the work, wins, and details that make your training yours.</p></div>
          <div class="dashboard-board-meta"><span>{visibleTiles.length} widgets</span><span>{dashboardLayout === 'spotlight' ? 'Story-led' : dashboardLayout === 'scrapbook' ? 'Scrapbook' : 'Balanced'}</span></div>
        </div>
        <div class="dashboard-profile-banner">
          <div class="dashboard-profile-avatar">{#if avatarUrl}<img src={avatarUrl} alt="" />{:else}{profile?.displayName?.slice(0, 2).toUpperCase()}{/if}</div>
          <div class="dashboard-profile-intro"><span class="student-eyebrow">CURRENTLY TRAINING</span><strong>{profile?.rankName ?? 'Rank in progress'}</strong><p>{dashboardBio}</p><div class="dashboard-chip-row"><span>{dashboardTechnique}</span><span>{profile?.programs?.length ?? 0} active tracks</span></div></div>
          <div class="dashboard-signature"><span>DOJO SIGNATURE</span><strong>{dojoMottos[mottoIndex]}</strong><small>Tap the motto widget to change it.</small></div>
        </div>
        <div class="dashboard-quick-stats"><div><span>CLASSES</span><strong>{profile?.totalClasses ?? 0}</strong><small>all time</small></div><div><span>STREAK</span><strong>{attendanceStreak}</strong><small>days in rhythm</small></div><div><span>THIS MONTH</span><strong>{profile?.classesThisMonth ?? 0}</strong><small>classes completed</small></div><div><span>MEMBER SINCE</span><strong>{profile?.joinDate ?? 'On file'}</strong><small>your dojo story</small></div></div>
        {#if visibleTiles.length}
          <div class="dashboard-widgets-heading"><div><span class="student-eyebrow">YOUR WIDGETS</span><h3>Build your board.</h3><p>Drag cards to rearrange them. Pull the corner handle to change their footprint.</p></div><div class="dashboard-board-help"><span>DRAG</span><span>RESIZE</span><span>CLICK TO EXPLORE</span></div></div>
          <div class="dashboard-section-stack">
            {#each dashboardSections as dashboardSection}
              <section class="dashboard-section">
                <div class="dashboard-section-heading"><div class="dashboard-section-name"><span class="dashboard-section-icon" aria-hidden="true">{dashboardSection.icon}</span><div><strong>{dashboardSection.section}</strong><small>{dashboardSection.description}</small></div></div><span>{dashboardSection.tiles.length} {dashboardSection.tiles.length === 1 ? 'widget' : 'widgets'}</span></div>
                <div class="dashboard-section-grid">
                  {#each dashboardSection.tiles as tile}
                    <div role="group" aria-label={`Arrange ${tile.label} widget`} class:dragging={draggingTileId === tile.id} class:drag-over={dragOverTileId === tile.id} class={`dashboard-widget-frame size-${tileSizes[tile.id] ?? defaultTileSizes[tile.id] ?? 'small'}`} draggable="true" on:dragstart={(event) => beginTileDrag(event, tile.id)} on:dragover|preventDefault={() => dragOverTileId = tile.id} on:drop|preventDefault={() => dropTile(tile.id)} on:dragend={finishTileDrag}>
                      {#if tileLink(tile.id)}
                        <a class={`dashboard-widget widget-${tile.id} type-${tileKind(tile.id)}`} href={tileLink(tile.id)}><span class="profile-tile-icon" aria-hidden="true">{tileIcon(tile.id)}</span><span class="student-eyebrow">{tile.label}</span><strong>{tileValue(tile.id)}</strong>{#if tileProgress(tile.id) !== null}<span class="profile-tile-progress"><span style={`width:${tileProgress(tile.id)}%`}></span></span>{/if}<small>{tile.description}</small><span class="profile-tile-action">Explore →</span></a>
                      {:else}
                        <div class={`dashboard-widget widget-${tile.id} type-${tileKind(tile.id)} ${tile.id === 'dojo-motto' ? 'profile-motto-secret' : ''}`} role="button" tabindex="0" aria-label={tile.id === 'dojo-motto' ? 'Dojo motto — click for hidden wisdom' : tile.label} on:click={() => tile.id === 'dojo-motto' && clickMotto()} on:keydown={(event) => tile.id === 'dojo-motto' && event.key === 'Enter' && clickMotto()}><span class="profile-tile-icon" aria-hidden="true">{tileIcon(tile.id)}</span><span class="student-eyebrow">{tile.label}</span><strong>{tileValue(tile.id)}</strong>{#if tileProgress(tile.id) !== null}<span class="profile-tile-progress"><span style={`width:${tileProgress(tile.id)}%`}></span></span>{/if}<small>{tile.description}</small></div>
                      {/if}
                      <button class="dashboard-resize-handle" type="button" aria-label={`Resize ${tile.label}`} title="Drag to resize; use arrow keys for precise sizing" on:pointerdown={(event) => { event.preventDefault(); event.stopPropagation(); startTileResize(event, tile.id); }} on:click|stopPropagation on:keydown|stopPropagation={(event) => { if (event.key === 'ArrowLeft') { event.preventDefault(); resizeTile(tile.id, -1); } if (event.key === 'ArrowRight') { event.preventDefault(); resizeTile(tile.id, 1); } }}>↔</button>
                    </div>
                  {/each}
                </div>
              </section>
            {/each}
          </div>
        {:else}<p class="muted dashboard-empty">Choose at least one widget to start building your board.</p>{/if}
      </section>
      {#if emergencyModalOpen}<div class="profile-modal-backdrop" role="presentation" on:click={() => emergencyModalOpen = false}><div class="profile-modal" role="dialog" aria-modal="true" aria-labelledby="emergency-modal-title" tabindex="-1" on:click|stopPropagation on:keydown|stopPropagation><div class="panel-title"><span id="emergency-modal-title">{editingEmergencyContactId ? 'EDIT EMERGENCY CONTACT' : 'ADD EMERGENCY CONTACT'}</span><button class="modal-close" type="button" on:click={() => emergencyModalOpen = false} aria-label="Close emergency contact dialog">×</button></div><p class="muted">This contact is private to your student record and can be updated anytime.</p>{#if emergencyError}<div class="error" role="alert">{emergencyError}</div>{/if}<form class="profile-form emergency-contact-form" on:submit|preventDefault={saveEmergencyContact}><label>First name<input bind:value={emergencyForm.firstName} maxlength="80" autocomplete="given-name" required /></label><label>Middle initial<input bind:value={emergencyForm.middleInitial} maxlength="4" autocomplete="additional-name" placeholder="e.g. R." /></label><label>Last name<input bind:value={emergencyForm.lastName} maxlength="80" autocomplete="family-name" required /></label><label>Pronouns<input bind:value={emergencyForm.pronouns} maxlength="80" placeholder="e.g. they/them" /></label><label>Relationship<input bind:value={emergencyForm.relationship} maxlength="80" placeholder="e.g. Spouse, sibling, friend" required /></label><label>Phone<input bind:value={emergencyForm.phone} maxlength="50" type="tel" autocomplete="tel" /></label><label>Email<input bind:value={emergencyForm.email} maxlength="254" type="email" autocomplete="email" /></label><div class="profile-form-preferences full"><label class="check-row"><input type="checkbox" bind:checked={emergencyForm.newsletterOptIn} /> Sign up for the dojo newsletter</label><label class="check-row"><input type="checkbox" bind:checked={emergencyForm.smsOptIn} /> Opt in to SMS updates</label></div><div class="profile-form-actions"><button class="outline-button" type="button" on:click={() => emergencyModalOpen = false}>Cancel</button><button class="primary-button" type="submit" disabled={emergencySaving}>{emergencySaving ? 'Saving…' : 'Save contact'}</button></div></form></div></div>{/if}
      {#if pinModalOpen}<div class="profile-modal-backdrop" role="presentation" on:click={() => pinModalOpen = false}><div class="profile-modal" role="dialog" aria-modal="true" aria-labelledby="pin-modal-title" tabindex="-1" on:click|stopPropagation on:keydown|stopPropagation><div class="panel-title"><span id="pin-modal-title">CHANGE STUDENT PIN</span><button class="modal-close" type="button" on:click={() => pinModalOpen = false} aria-label="Close PIN dialog">×</button></div><p class="muted">This PIN controls both Student Hub access and class check-in.</p><form class="profile-form password-form" on:submit|preventDefault={changePin}><label>Current PIN<input type="password" bind:value={pinForm.currentPin} inputmode="numeric" minlength="4" maxlength="6" autocomplete="current-password" required /></label><label>New PIN<input type="password" bind:value={pinForm.newPin} inputmode="numeric" minlength="4" maxlength="6" pattern="[0-9][0-9][0-9][0-9][0-9]?[0-9]?" autocomplete="new-password" required /><small class="muted">Use 4–6 digits.</small></label><label>Confirm new PIN<input type="password" bind:value={pinForm.confirmPin} inputmode="numeric" minlength="4" maxlength="6" pattern="[0-9][0-9][0-9][0-9][0-9]?[0-9]?" autocomplete="new-password" required /></label><div class="profile-form-actions"><button class="outline-button" type="button" on:click={() => pinModalOpen = false}>Cancel</button><button class="primary-button" type="submit" disabled={changingPin}>{changingPin ? 'Saving…' : 'Save new PIN'}</button></div></form></div></div>{/if}
    {/if}
  </StudentShell>
{/if}
