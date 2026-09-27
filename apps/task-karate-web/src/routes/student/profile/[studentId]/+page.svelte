<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { marked } from 'marked';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { api } from '$lib/api';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';
  import { rankColor } from '$lib/rank-colors';
  import '$lib/social.css';

  type PublicProfileTile = { id: string; label: string; value: string; description: string; icon: string; size: 'small' | 'medium' | 'wide' | 'feature'; progress?: number | null };
  type PublicProfile = {
    studentId: number;
    displayName: string;
    pronouns?: string | null;
    bio?: string | null;
    favoriteTechnique?: string | null;
    profileImagePath?: string | null;
    rankName?: string | null;
    joinDate?: string | null;
    totalClasses: number;
    classesThisMonth: number;
    achievementCount: number;
    helperClasses: number;
    classesIntoStripe: number;
    classesPerStripe: number;
    classesToNextStripe: number;
    stripesEarned: number;
    nextMilestone: string;
    programs: { programName: string; levelName?: string | null; progressionType: string }[];
    achievements: { name: string; description?: string | null; iconName?: string | null; awardedAt?: string | null }[];
    recentPosts: { postId: number; text: string; postKind: string; createdAt: string; reactionCount: number; commentCount: number }[];
    featuredTiles: PublicProfileTile[];
  };

  let session: StudentSession | null = null;
  let profile: PublicProfile | null = null;
  let loading = true;
  let error = '';

  $: studentId = Number($page.params.studentId);
  $: isOwnProfile = Boolean(session && profile && session.studentId === profile.studentId);
  function initials(name: string) { return name.split(' ').map((part) => part[0]).join('').slice(0, 2).toUpperCase(); }
  function achievementIcon(iconName?: string | null) { return iconName === 'flame' ? '🔥' : iconName === 'star' ? '★' : iconName === 'spark' ? '✦' : iconName === 'anniversary' ? '🎉' : iconName === 'twenty-five' || iconName === 'ten' || iconName === 'five' || iconName === 'three' ? '🥋' : '🏅'; }
  function relativeDate(value: string) { const date = new Date(value); const seconds = Math.max(0, Math.floor((Date.now() - date.getTime()) / 1000)); if (seconds < 60) return 'just now'; if (seconds < 3600) return `${Math.floor(seconds / 3600)}h`; if (seconds < 86400) return `${Math.floor(seconds / 86400)}d`; return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' }); }
  function safeMarkdown(value: string) { const escaped = value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#039;'); return marked.parse(escaped.replace(/(!?\[[^\]]*\]\()([^\s)]+)([^)]*\))/g, (_match, prefix, url, suffix) => `${prefix}${/^(https?:|mailto:)/i.test(url) ? url : '#'}${suffix}`), { gfm: true, breaks: true }); }
  function tileAction(id: string) { return ({ rank: '#public-training', 'next-stripe': '#public-training', 'total-classes': '#public-stats', 'helper-classes': '#public-stats', 'monthly-classes': '#public-stats', streak: '#public-stats', programs: '#public-programs', achievements: '#public-achievements', 'training-track': '#public-training' } as Record<string, string>)[id] ?? ''; }

  onMount(async () => {
    session = await requireStudent();
    if (!session || !Number.isInteger(studentId) || studentId <= 0) { loading = false; return; }
    try { profile = await api<PublicProfile>(`/api/student/profiles/${studentId}`); }
    catch (e) { error = apiError(e); }
    finally { loading = false; }
  });
</script>

<svelte:head><title>{profile ? `${profile.displayName} | Task Karate` : 'Public profile | Task Karate'}</title></svelte:head>

{#if session}
  <StudentShell {session} active="social">
    <div class="social-page public-profile-page">
      {#if loading}
        <section class="social-card public-profile-state"><span class="social-kicker">PUBLIC PROFILE</span><h1>Loading profile…</h1></section>
      {:else if error}
        <section class="social-card public-profile-state"><span class="social-kicker">PUBLIC PROFILE</span><h1>Profile unavailable</h1><p>{error}</p><a class="social-secondary" href="/student/social">Back to the dojo wall</a></section>
      {:else if profile}
        <header class="social-card public-profile-hero">
          <div class="public-profile-hero-top"><span class="social-kicker">{isOwnProfile ? 'PUBLIC PREVIEW' : 'DOJO COMMUNITY PROFILE'}</span><a class="public-profile-back" href={isOwnProfile ? '/student/profile' : '/student/social'}>{isOwnProfile ? '← Back to edit profile' : '← Back to Dojo Wall'}</a></div>
          {#if isOwnProfile}<div class="public-profile-preview-note">This is the public-facing version of your profile. Private contact details and personal dashboard tiles stay hidden.</div>{/if}
          <div class="public-profile-identity">
            <div class="public-profile-avatar">{#if profile.profileImagePath}<img src={profile.profileImagePath} alt="" />{:else}{initials(profile.displayName)}{/if}</div>
            <div class="public-profile-copy">
              <div class="public-profile-name-line"><h1>{profile.displayName}</h1><span class="public-profile-spark" aria-hidden="true">✦</span></div>
              <div class="public-profile-rank"><span class="belt-swatch belt-swatch-large" style={`--belt-color:${rankColor(profile.rankName)}`} aria-label={`${profile.rankName ?? 'Student'} belt`}></span><strong>{profile.rankName ?? 'Student'}</strong>{#if profile.pronouns}<span>{profile.pronouns}</span>{/if}{#if profile.joinDate}<span>Training since {new Date(profile.joinDate).getFullYear()}</span>{/if}</div>
              {#if profile.bio}<p class="public-profile-bio">{profile.bio}</p>{:else}<p class="public-profile-bio">Training, learning, and showing up one class at a time.</p>{/if}
              {#if profile.favoriteTechnique}<p class="public-profile-technique"><span>Favorite focus</span>{profile.favoriteTechnique}</p>{/if}
            </div>
            <div class="public-profile-hero-side"><span class="social-kicker">ON THE MAT</span><strong>{profile.achievementCount} moments collected</strong><p>Every class, milestone, and helping hand adds another piece to the story.</p><div><span>⚡</span><small>{profile.classesThisMonth === 1 ? '1 class' : `${profile.classesThisMonth} classes`} this month</small></div></div>
          </div>
          <div class="public-profile-hero-footer"><span>🥋 Progress is better with people cheering you on.</span><a class="social-primary" href="/student/social">Visit the dojo wall</a></div>
        </header>

        <section class="public-profile-showcase social-card" id="public-showcase">
          <div class="public-profile-section-heading"><div><span class="social-kicker">THE SHOWCASE</span><h2>{profile.displayName}'s dojo board</h2><p>The details they chose to share from their profile.</p></div><span class="public-profile-board-mark">{profile.featuredTiles.length} featured</span></div>
          <div class="public-profile-tile-grid">
            {#each profile.featuredTiles as tile}
              {#if tileAction(tile.id)}<a class={`public-profile-tile public-profile-tile-link size-${tile.size} tile-${tile.id}`} href={tileAction(tile.id)}>
                <span class="public-profile-tile-icon" aria-hidden="true">{tile.icon}</span>
                <span class="public-profile-tile-label">{tile.label}</span>
                <strong>{tile.value}</strong>
                {#if tile.progress !== null && tile.progress !== undefined}<span class="public-profile-tile-progress"><span style={`width:${tile.progress}%`}></span></span>{/if}
                <small>{tile.description}</small>
                <span class="public-profile-tile-action">Explore →</span>
              </a>{:else}<article class={`public-profile-tile size-${tile.size} tile-${tile.id}`}>
                <span class="public-profile-tile-icon" aria-hidden="true">{tile.icon}</span>
                <span class="public-profile-tile-label">{tile.label}</span>
                <strong>{tile.value}</strong>
                {#if tile.progress !== null && tile.progress !== undefined}<span class="public-profile-tile-progress"><span style={`width:${tile.progress}%`}></span></span>{/if}
                <small>{tile.description}</small>
              </article>{/if}
            {:else}
              <div class="public-profile-empty-tile"><span>✦</span><strong>This board is still being built.</strong><small>Check back after the next training session.</small></div>
            {/each}
          </div>
        </section>

        <section class="public-profile-stats public-profile-stats-card" id="public-stats">
          <div><span>MAT TIME</span><strong>{profile.totalClasses}</strong><small>Total classes</small></div>
          <div><span>MOMENTUM</span><strong>{profile.classesThisMonth}</strong><small>This month</small></div>
          <div><span>COLLECTION</span><strong>{profile.achievementCount}</strong><small>Achievements</small></div>
          <div><span>GIVING BACK</span><strong>{profile.helperClasses}</strong><small>Helper classes</small></div>
        </section>

        <div class="public-profile-grid">
          <main>
            <section class="social-card public-profile-section public-profile-achievement-shelf" id="public-achievements">
              <div class="public-profile-section-heading compact"><div><span class="social-kicker">TROPHY SHELF</span><h2>Achievements</h2></div><span class="public-profile-board-mark">{profile.achievements.length} collected</span></div>
              <div class="public-profile-achievements">{#each profile.achievements as achievement}<article class="public-profile-achievement"><span class="public-profile-achievement-icon">{achievementIcon(achievement.iconName)}</span><div><strong>{achievement.name}</strong>{#if achievement.description}<p>{achievement.description}</p>{/if}{#if achievement.awardedAt}<small>{new Date(achievement.awardedAt).toLocaleDateString()}</small>{/if}</div></article>{:else}<p class="social-muted">No achievements yet — the next one is waiting.</p>{/each}</div>
            </section>
            <section class="social-card public-profile-section">
              <div class="public-profile-section-heading compact"><div><span class="social-kicker">FROM THE DOJO WALL</span><h2>Recent moments</h2></div><span class="public-profile-board-mark">{profile.recentPosts.length} shared</span></div>
              {#each profile.recentPosts as post}<article class="public-profile-post"><div><span class="social-post-kind">{post.postKind === 'win' ? '🏆 Milestone or win' : post.postKind === 'question' ? '💬 Ask the dojo' : post.postKind === 'encouragement' ? '🌟 Encouragement' : '🥋 Training update'}</span><small>{relativeDate(post.createdAt)} · {post.reactionCount} reactions · {post.commentCount} comments</small></div><div class="social-markdown-content">{@html safeMarkdown(post.text)}</div></article>{:else}<p class="social-muted">No public dojo moments yet.</p>{/each}
            </section>
          </main>
          <aside>
            <section class="social-card public-profile-section public-profile-progress-card" id="public-training"><div class="public-profile-section-heading compact"><div><span class="social-kicker">KEEP MOVING</span><h2>Training snapshot</h2></div><span class="public-profile-progress-emoji">⚡</span></div><div class="public-profile-progress"><div><strong>{profile.classesIntoStripe}{profile.classesPerStripe ? ` / ${profile.classesPerStripe}` : ''}</strong><span>{profile.nextMilestone}</span></div>{#if profile.classesPerStripe}<div class="public-profile-progress-track"><span style={`width:${Math.min(100, (profile.classesIntoStripe / profile.classesPerStripe) * 100)}%`}></span></div><small>{profile.classesToNextStripe} classes to the next stripe</small>{#if profile.classesToNextStripe <= 2}<div class="public-profile-milestone-callout">{profile.classesToNextStripe === 1 ? 'One more class unlocks the next stripe.' : 'The next stripe is within reach.'}</div>{/if}{/if}</div></section>
            <section class="social-card public-profile-section" id="public-programs"><div class="public-profile-section-heading compact"><div><span class="social-kicker">ON THE MAT</span><h2>Programs</h2></div></div>{#each profile.programs as program}<div class="public-profile-program"><strong>{program.programName}</strong><span>{program.levelName ?? program.progressionType}</span></div>{:else}<p class="social-muted">No program details to show.</p>{/each}</section>
            <section class="public-profile-cta"><span aria-hidden="true">✦</span><strong>Training is more fun together.</strong><p>Find a class, share a win, or cheer on your dojo crew.</p><a href="/student/social">Join the conversation →</a></section>
          </aside>
        </div>
      {/if}
    </div>
  </StudentShell>
{/if}
