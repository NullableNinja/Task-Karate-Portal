<script lang="ts">
  import { onMount, tick } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { apiError } from '$lib/student-session';
  import PortalNav from '$lib/components/PortalNav.svelte';
  import type { Content } from '$lib/types';

  type StudentDirectoryItem = {
    studentId: number;
    displayName: string;
    rankName?: string | null;
    is3LevelName?: string | null;
    profileImagePath?: string | null;
  };
  type DailyQuote = { quote: string; author: string };

  let students: StudentDirectoryItem[] = [];
  let announcements: Content[] = [];
  let search = '';
  let selectedStudent: StudentDirectoryItem | null = null;
  let pin = '';
  let error = '';
  let loading = true;
  let signingIn = false;
  let showPin = false;
  let dailyQuote: DailyQuote = { quote: 'Small steps, sharp focus, steady progress.', author: 'Task Karate' };

  const fallbackQuotes: DailyQuote[] = [
    { quote: 'Small steps, sharp focus, steady progress.', author: 'Task Karate' },
    { quote: 'Your next good rep starts with showing up.', author: 'Task Karate' },
    { quote: 'Breathe in. Set your stance. Begin.', author: 'Task Karate' },
    { quote: 'Consistency turns practice into progress.', author: 'Task Karate' }
  ];
  const blockedQuoteTerms = /\b(?:fuck|shit|bitch|asshole|bastard|dick|piss|cunt)\b/i;

  $: filteredStudents = students
    .filter((student) => student.displayName.toLowerCase().includes(search.trim().toLowerCase()))
    .sort((a, b) => a.displayName.localeCompare(b.displayName));
  $: groupedStudents = filteredStudents.reduce<Record<string, StudentDirectoryItem[]>>((groups, student) => {
    const letter = student.displayName.trim().charAt(0).toUpperCase() || '#';
    (groups[letter] ??= []).push(student);
    return groups;
  }, {});
  $: rosterLetters = Object.keys(groupedStudents).sort();

  onMount(async () => {
    void loadDailyQuote();
    try {
      [students, announcements] = await Promise.all([
        api<StudentDirectoryItem[]>('/api/student/public/students'),
        api<Content[]>('/api/public/announcements')
      ]);
    } catch (e) {
      error = apiError(e, 'The student roster could not be loaded.');
    } finally {
      loading = false;
    }
  });

  function initials(name: string) {
    return name.trim().split(/\s+/).map((part) => part[0]).join('').slice(0, 2).toUpperCase();
  }

  function beltColor(rank?: string | null) {
    const value = rank?.toLowerCase() ?? '';
    if (value.includes('white')) return '#e6edf1';
    if (value.includes('gold') || value.includes('yellow')) return '#d8ad42';
    if (value.includes('orange')) return '#e87832';
    if (value.includes('green')) return '#42a879';
    if (value.includes('purple')) return '#9569c3';
    if (value.includes('blue')) return '#4f9ee8';
    if (value.includes('red')) return '#d85a63';
    if (value.includes('brown')) return '#9c6a45';
    if (value.includes('black')) return '#171c24';
    return '#38bdf8';
  }

  function beltTextColor(rank?: string | null) {
    const value = rank?.toLowerCase() ?? '';
    return value.includes('black') || value.includes('brown') || value.includes('red') ? '#fff' : '#102038';
  }

  function profileLabel(student: StudentDirectoryItem) {
    if (student.rankName) return student.rankName;
    if (student.is3LevelName) return `IS3 · ${student.is3LevelName}`;
    return 'Student';
  }

  function avatarTextColor(rank?: string | null) {
    return beltTextColor(rank);
  }

  function quoteDate() {
    return new Date().toISOString().slice(0, 10);
  }

  function isSafeQuote(value: unknown): value is DailyQuote {
    if (!value || typeof value !== 'object') return false;
    const candidate = value as Partial<DailyQuote>;
    return typeof candidate.quote === 'string'
      && typeof candidate.author === 'string'
      && candidate.quote.trim().length > 0
      && candidate.quote.length <= 220
      && candidate.author.length <= 80
      && !blockedQuoteTerms.test(candidate.quote);
  }

  async function loadDailyQuote() {
    const today = quoteDate();
    dailyQuote = fallbackQuotes[new Date().getDate() % fallbackQuotes.length];
    try {
      const cacheKey = `task-karate-quote-${today}`;
      const cached = localStorage.getItem(cacheKey);
      if (cached) {
        const parsed = JSON.parse(cached);
        if (isSafeQuote(parsed)) {
          dailyQuote = parsed;
          return;
        }
      }

      const response = await fetch('https://dummyjson.com/quotes/random', {
        headers: { Accept: 'application/json' },
        signal: AbortSignal.timeout(4500)
      });
      if (!response.ok) return;
      const result = await response.json();
      const nextQuote = { quote: result.quote, author: result.author };
      if (isSafeQuote(nextQuote)) {
        dailyQuote = nextQuote;
        localStorage.setItem(cacheKey, JSON.stringify(nextQuote));
      }
    } catch {
      // The sign-in flow never depends on the quote service.
    }
  }

  function choose(student: StudentDirectoryItem) {
    selectedStudent = selectedStudent?.studentId === student.studentId ? null : student;
    error = '';
    if (selectedStudent) {
      void tick().then(() => document.querySelector(`[data-student-id="${student.studentId}"] .enter-dojo-option`)?.scrollIntoView({ behavior: 'smooth', block: 'nearest' }));
    }
  }

  function openDojo(student: StudentDirectoryItem) {
    selectedStudent = student;
    pin = '';
    error = '';
    showPin = true;
  }

  function cancelPin() {
    showPin = false;
    pin = '';
    error = '';
  }

  async function submit() {
    if (!selectedStudent || !pin.trim()) return;
    signingIn = true;
    error = '';
    try {
      const result = await api<{ disclaimerRequired: boolean }>('/api/student/auth/login', {
        method: 'POST',
        body: JSON.stringify({ studentId: selectedStudent.studentId, pin })
      });
      await goto(result.disclaimerRequired ? '/student/disclaimer' : '/student');
    } catch (e) {
      error = apiError(e, 'That PIN or password was not accepted.');
    } finally {
      signingIn = false;
    }
  }
</script>

<svelte:head><title>Task Karate | Enter the dojo</title></svelte:head>

<main class="auth-stage roster-auth-stage">
  <div class="roster-page-shell">
    <PortalNav current="student" />
    <section class="auth-frame roster-frame" aria-labelledby="dojo-title">
    <span class="student-eyebrow">SECURE STUDENT ACCESS</span>
    <h1 id="dojo-title"><span>Enter the</span> <span>dojo.</span></h1>
    <p class="lead"><span class="desktop-only">Choose your student profile, then enter the 4–6 digit PIN issued by Task Karate staff.</span><span class="mobile-only">Choose your profile, then enter your PIN.</span></p>

    <aside class="daily-quote" aria-label="Quote of the day" aria-live="polite">
      <span class="daily-quote-kicker">QUOTE OF THE DAY</span>
      <blockquote>“{dailyQuote.quote}”</blockquote>
      <cite>— {dailyQuote.author}</cite>
    </aside>

    {#if announcements.length > 0}
      <section class="login-announcements" aria-labelledby="login-announcements-title">
        <div class="login-announcements-heading"><span class="student-eyebrow">DOJO UPDATES</span><h2 id="login-announcements-title">Before you enter</h2></div>
        {#each announcements.slice(0, 3) as item}
          <article class="login-announcement">
            <div><h3>{item.title}</h3><p>{item.body}</p></div>
            {#if item.publishedAtUtc}<small>{new Date(item.publishedAtUtc).toLocaleDateString()}</small>{/if}
          </article>
        {/each}
      </section>
    {/if}

    {#if error && !showPin}<div class="error" role="alert">{error}</div>{/if}

    <label class="roster-search-label" for="student-search">Find your profile</label>
    <input id="student-search" class="roster-search" bind:value={search} placeholder="Search students…" autocomplete="off" />

    {#if loading}
      <div class="roster-state" aria-live="polite">Loading the student roster…</div>
    {:else if filteredStudents.length === 0}
      <div class="roster-state">No student profiles match that search.</div>
    {:else}
      <div class="student-roster" aria-label="Student profiles">
        {#each rosterLetters as letter}
          <section class="roster-group" aria-labelledby={`roster-${letter}`}>
            <div class="roster-divider" id={`roster-${letter}`}><span>{letter}</span><i aria-hidden="true"></i></div>
            <div class="roster-group-grid">
              {#each groupedStudents[letter] as student}
                <div data-student-id={student.studentId} class:selected={selectedStudent?.studentId === student.studentId} class="student-option">
                  <button class="student-option-main" type="button" aria-pressed={selectedStudent?.studentId === student.studentId} on:click={() => choose(student)}>
                    <span class="student-avatar" style={`--avatar-color: ${beltColor(student.rankName)}; --avatar-text: ${avatarTextColor(student.rankName)}`}>{initials(student.displayName)}</span>
                    <span class="student-option-copy"><strong>{student.displayName}</strong><small class="roster-rank" style={`--rank-color:${beltColor(student.rankName)}; --rank-text:${beltTextColor(student.rankName)}`}>{profileLabel(student)}</small></span>
                  </button>
                  {#if selectedStudent?.studentId === student.studentId}
                    <button class="enter-dojo-option" type="button" on:click={() => openDojo(student)}>Enter Dojo <span aria-hidden="true">→</span></button>
                  {/if}
                </div>
              {/each}
            </div>
          </section>
        {/each}
      </div>
    {/if}

    <div class="roster-footer">
      <span class="muted">You’ll be signed out automatically after 30 minutes.</span>
      <a class="text-link" href="/schedule">← Back to schedule</a>
    </div>
    </section>
  </div>
</main>

{#if showPin && selectedStudent}
  <div class="login-overlay" role="presentation">
    <div class="pin-dialog" role="dialog" aria-modal="true" aria-labelledby="pin-title" tabindex="-1">
      <span class="student-avatar pin-avatar" style={`--avatar-color: ${beltColor(selectedStudent.rankName)}; --avatar-text: ${avatarTextColor(selectedStudent.rankName)}`}>{initials(selectedStudent.displayName)}</span>
      <h2 id="pin-title">Enter Your PIN</h2>
      <p class="pin-name">{selectedStudent.displayName}</p>
      <span class="dialog-rank" style={`--rank-color:${beltColor(selectedStudent.rankName)}; --rank-text:${beltTextColor(selectedStudent.rankName)}`}>{profileLabel(selectedStudent)}</span>
      <form on:submit|preventDefault={submit}>
        <label for="student-pin">Student PIN</label>
        <input id="student-pin" class="pin-input" type="password" bind:value={pin} inputmode="numeric" autocomplete="current-password" minlength="4" maxlength="6" pattern="[0-9][0-9][0-9][0-9][0-9]?[0-9]?" placeholder="Enter your PIN…" required />
        {#if error}<div class="error" role="alert">{error}</div>{/if}
        <div class="pin-actions"><button class="outline-button" type="button" on:click={cancelPin}>Cancel</button><button class="primary-button" disabled={signingIn}>{signingIn ? 'Checking…' : 'Enter Dojo'}</button></div>
      </form>
    </div>
  </div>
{/if}
