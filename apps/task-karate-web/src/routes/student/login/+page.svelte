<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  import { api } from '$lib/api';
  import { apiError } from '$lib/student-session';

  type StudentDirectoryItem = {
    studentId: number;
    displayName: string;
    rankName?: string | null;
    is3LevelName?: string | null;
    profileImagePath?: string | null;
  };

  let students: StudentDirectoryItem[] = [];
  let search = '';
  let selectedStudent: StudentDirectoryItem | null = null;
  let pin = '';
  let remember = false;
  let error = '';
  let loading = true;
  let signingIn = false;
  let showPin = false;

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
    try {
      students = await api<StudentDirectoryItem[]>('/api/student/public/students');
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
    if (value.includes('black')) return '#aeb8c5';
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

  function choose(student: StudentDirectoryItem) {
    selectedStudent = selectedStudent?.studentId === student.studentId ? null : student;
    error = '';
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
      const result = await api<{ disclaimerRequired: boolean; passwordChangeRequired: boolean }>('/api/student/auth/login', {
        method: 'POST',
        body: JSON.stringify({ studentId: selectedStudent.studentId, password: pin, rememberMe: remember })
      });
      await goto(result.passwordChangeRequired ? '/student/password' : result.disclaimerRequired ? '/student/disclaimer' : '/student');
    } catch (e) {
      error = apiError(e, 'That PIN or password was not accepted.');
    } finally {
      signingIn = false;
    }
  }
</script>

<svelte:head><title>Task Karate | Enter the dojo</title></svelte:head>

<main class="auth-stage roster-auth-stage">
  <div class="auth-orbs" aria-hidden="true">
    <span class="auth-orb auth-orb-a"></span>
    <span class="auth-orb auth-orb-b"></span>
    <span class="auth-orb auth-orb-c"></span>
    <span class="auth-orb auth-orb-d"></span>
    <span class="auth-orb auth-orb-e"></span>
    <span class="auth-orb auth-orb-f"></span>
  </div>
  <section class="auth-frame roster-frame" aria-labelledby="dojo-title">
    <a class="brand" href="/schedule"><span class="brand-mark">TK</span><span><strong>TASK KARATE</strong><small>STUDENT HUB</small></span></a>
    <span class="student-eyebrow">SECURE STUDENT ACCESS</span>
    <h1 id="dojo-title">Enter the dojo.</h1>
    <p class="lead">Choose your student profile, then enter the password issued by Task Karate staff.</p>

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
                <div class:selected={selectedStudent?.studentId === student.studentId} class="student-option">
                  <button class="student-option-main" type="button" aria-pressed={selectedStudent?.studentId === student.studentId} on:click={() => choose(student)}>
                    <span class="student-avatar" style={`--avatar-color: ${beltColor(student.rankName)}; --avatar-text: ${avatarTextColor(student.rankName)}`}>{initials(student.displayName)}</span>
                    <span class="student-option-copy"><strong>{student.displayName}</strong><small class="roster-rank" style={`--rank-color:${beltColor(student.rankName)}`}>{profileLabel(student)}</small></span>
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
      <label class="check-row"><input type="checkbox" bind:checked={remember} /> Keep me signed in on this device</label>
      <a class="text-link" href="/schedule">← Back to schedule</a>
    </div>
  </section>
</main>

{#if showPin && selectedStudent}
  <div class="login-overlay" role="presentation">
    <div class="pin-dialog" role="dialog" aria-modal="true" aria-labelledby="pin-title" tabindex="-1">
      <span class="student-avatar pin-avatar" style={`--avatar-color: ${beltColor(selectedStudent.rankName)}; --avatar-text: ${avatarTextColor(selectedStudent.rankName)}`}>{initials(selectedStudent.displayName)}</span>
      <h2 id="pin-title">Enter Your Password</h2>
      <p class="pin-name">{selectedStudent.displayName}</p>
      <span class="dialog-rank" style={`--rank-color:${beltColor(selectedStudent.rankName)}`}>{profileLabel(selectedStudent)}</span>
      <form on:submit|preventDefault={submit}>
        <label for="student-pin">Password</label>
        <input id="student-pin" class="pin-input" type="password" bind:value={pin} autocomplete="current-password" maxlength="128" placeholder="Enter your password…" required />
        {#if error}<div class="error" role="alert">{error}</div>{/if}
        <div class="pin-actions"><button class="outline-button" type="button" on:click={cancelPin}>Cancel</button><button class="primary-button" disabled={signingIn}>{signingIn ? 'Checking…' : 'Enter Dojo'}</button></div>
      </form>
    </div>
  </div>
{/if}
