<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type ContactType = 'guardian' | 'emergency';
  type GuardianStudent = { studentId: number; name: string; relationship: string };
  type Guardian = { guardianId: number; firstName: string; middleInitial?: string; lastName: string; pronouns?: string; email?: string; phone?: string; newsletterOptIn?: boolean; smsOptIn?: boolean; isActive: boolean; students: GuardianStudent[] };
  type StudentOption = { studentId: number; name: string };
  type ContactForm = { name: string; firstName: string; middleInitial: string; lastName: string; pronouns: string; relationship: string; email: string; phone: string; newsletterOptIn: boolean; smsOptIn: boolean; studentIds: number[] };

  const blankForm = (): ContactForm => ({ name: '', firstName: '', middleInitial: '', lastName: '', pronouns: '', relationship: '', email: '', phone: '', newsletterOptIn: false, smsOptIn: false, studentIds: [] });
  let guardians: Guardian[] = [];
  let students: StudentOption[] = [];
  let form = blankForm();
  let contactType: ContactType = 'guardian';
  let editingId = 0;
  let query = '';
  let studentQuery = '';
  let error = '';
  let notice = '';
  let loading = true;
  let saving = false;

  async function load() {
    loading = true;
    error = '';
    try {
      [guardians, students] = await Promise.all([
        api<Guardian[]>('/api/portal-admin/guardians'),
        api<StudentOption[]>('/api/portal-admin/guardian-students')
      ]);
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not load portal contacts.';
    } finally {
      loading = false;
    }
  }

  $: visibleGuardians = guardians.filter((guardian) => `${guardian.firstName} ${guardian.lastName} ${guardian.email ?? ''} ${guardian.students.map((student) => student.name).join(' ')}`.toLowerCase().includes(query.trim().toLowerCase()));
  $: filteredStudents = students.filter((student) => student.name.toLowerCase().includes(studentQuery.trim().toLowerCase()));
  $: selectedStudents = students.filter((student) => form.studentIds.includes(student.studentId));

  function resetForm() {
    form = blankForm();
    contactType = 'guardian';
    editingId = 0;
    studentQuery = '';
  }

  function chooseContactType(type: ContactType) {
    if (editingId) return;
    contactType = type;
    if (type === 'emergency' && form.studentIds.length > 1) form = { ...form, studentIds: form.studentIds.slice(0, 1) };
  }

  function edit(guardian: Guardian) {
    editingId = guardian.guardianId;
    contactType = 'guardian';
    form = { name: '', firstName: guardian.firstName, middleInitial: guardian.middleInitial ?? '', lastName: guardian.lastName, pronouns: guardian.pronouns ?? '', relationship: guardian.students[0]?.relationship ?? 'Guardian', email: guardian.email ?? '', phone: guardian.phone ?? '', newsletterOptIn: guardian.newsletterOptIn ?? false, smsOptIn: guardian.smsOptIn ?? false, studentIds: guardian.students.map((student) => student.studentId) };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  function toggleStudent(studentId: number, checked: boolean) {
    if (contactType === 'emergency') {
      form = { ...form, studentIds: checked ? [studentId] : [] };
      return;
    }
    form = { ...form, studentIds: checked ? [...form.studentIds, studentId] : form.studentIds.filter((id) => id !== studentId) };
  }

  async function save() {
    error = '';
    notice = '';
    if (!form.studentIds.length) {
      error = 'Choose the student this contact belongs to before saving.';
      return;
    }
    if (contactType === 'emergency' && (!form.phone.trim() && !form.email.trim())) {
      error = 'Add a phone number or email so the emergency contact can be reached.';
      return;
    }
    saving = true;
    try {
      if (editingId) {
        await api(`/api/portal-admin/guardians/${editingId}`, { method: 'PUT', body: JSON.stringify({ firstName: form.firstName, middleInitial: form.middleInitial, lastName: form.lastName, pronouns: form.pronouns, relationship: form.relationship, email: form.email, phone: form.phone, newsletterOptIn: form.newsletterOptIn, smsOptIn: form.smsOptIn, studentIds: form.studentIds }) });
        notice = 'Guardian details and student links were updated.';
      } else if (contactType === 'guardian') {
        await api('/api/portal-admin/guardians', { method: 'POST', body: JSON.stringify({ firstName: form.firstName, middleInitial: form.middleInitial, lastName: form.lastName, pronouns: form.pronouns, relationship: form.relationship, email: form.email, phone: form.phone, newsletterOptIn: form.newsletterOptIn, smsOptIn: form.smsOptIn, studentIds: form.studentIds }) });
        notice = 'Guardian created and linked to the selected students.';
      } else {
        await api(`/api/portal-admin/students/${form.studentIds[0]}/emergency-contacts`, { method: 'POST', body: JSON.stringify({ firstName: form.firstName, middleInitial: form.middleInitial, lastName: form.lastName, pronouns: form.pronouns, relationship: form.relationship, phone: form.phone || null, email: form.email || null, newsletterOptIn: form.newsletterOptIn, smsOptIn: form.smsOptIn }) });
        notice = `${[form.firstName, form.middleInitial, form.lastName].filter(Boolean).join(' ')} was added as an emergency contact for ${selectedStudents[0]?.name ?? 'the selected student'}.`;
      }
      resetForm();
      await load();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not save contact.';
    } finally {
      saving = false;
    }
  }

  onMount(load);
</script>

<svelte:head><title>Staff · Families & contacts | Task Karate</title></svelte:head>

<div class="toolbar">
  <div>
    <span class="eyebrow">PEOPLE & SAFETY</span>
    <h2>Families & contacts</h2>
    <p class="muted">Add the person first, then connect them to the student record they belong to.</p>
  </div>
</div>

{#if error}<div class="error" role="alert">{error}</div>{/if}
{#if notice}<div class="status" role="status">{notice}</div>{/if}

<section class="card contact-creator-card" id="contact-creator" aria-labelledby="contact-creator-title">
  <div class="contact-creator-header">
    <div>
      <span class="eyebrow">START HERE</span>
      <h3 id="contact-creator-title">{editingId ? 'Edit guardian and student links' : 'Create a guardian or emergency contact'}</h3>
      <p class="muted">Choose the contact type, enter their information, and then select the student or students they should be connected to.</p>
    </div>
    {#if editingId}<button class="button secondary small-button" type="button" on:click={resetForm}>Cancel edit</button>{/if}
  </div>

  <div class="contact-type-picker" role="radiogroup" aria-label="Contact type">
    <button class:active={contactType === 'guardian'} class="contact-type-choice" type="button" role="radio" aria-checked={contactType === 'guardian'} on:click={() => chooseContactType('guardian')}>
      <span class="contact-choice-number">1</span>
      <span><strong>Guardian</strong><small>Family account · can connect to multiple students</small></span>
      <span class="contact-choice-check" aria-hidden="true">{contactType === 'guardian' ? '✓' : ''}</span>
    </button>
    <button class:active={contactType === 'emergency'} class="contact-type-choice" type="button" role="radio" aria-checked={contactType === 'emergency'} disabled={!!editingId} on:click={() => chooseContactType('emergency')}>
      <span class="contact-choice-number">2</span>
      <span><strong>Emergency contact</strong><small>Safety record · connects to one student</small></span>
      <span class="contact-choice-check" aria-hidden="true">{contactType === 'emergency' ? '✓' : ''}</span>
    </button>
  </div>

  <form class="contact-form" on:submit|preventDefault={save}>
    <div class="contact-form-step">
      <div class="contact-step-heading"><span class="contact-step-number">1</span><div><h4>Person's information</h4><p>{contactType === 'guardian' ? 'This becomes the family account staff will see.' : 'This information will be stored on the student’s emergency contact list.'}</p></div></div>
      <div class="contact-field-grid">
        {#if contactType === 'guardian'}
          <label>First name<input bind:value={form.firstName} autocomplete="given-name" required /></label>
          <label>Middle initial<input bind:value={form.middleInitial} maxlength="4" autocomplete="additional-name" placeholder="e.g. R." /></label>
          <label>Last name<input bind:value={form.lastName} autocomplete="family-name" required /></label>
          <label>Pronouns<input bind:value={form.pronouns} maxlength="80" placeholder="e.g. she/her" /></label>
          <label>Relationship<input bind:value={form.relationship} maxlength="80" required placeholder="Parent, guardian, grandparent…" /></label>
        {:else}
          <label>First name<input bind:value={form.firstName} autocomplete="given-name" required /></label>
          <label>Middle initial<input bind:value={form.middleInitial} maxlength="4" autocomplete="additional-name" placeholder="e.g. R." /></label>
          <label>Last name<input bind:value={form.lastName} autocomplete="family-name" required /></label>
          <label>Pronouns<input bind:value={form.pronouns} maxlength="80" placeholder="e.g. they/them" /></label>
          <label>Relationship<input bind:value={form.relationship} maxlength="80" required placeholder="Parent, neighbor, family friend…" /></label>
        {/if}
        <label>Email<input type="email" bind:value={form.email} autocomplete="email" placeholder="name@example.com" /></label>
        <label>Phone <span class="field-optional">optional</span><input type="tel" bind:value={form.phone} autocomplete="tel" placeholder="(555) 010-1234" /></label>
        <fieldset class="contact-preferences full"><legend>Communication preferences</legend><label class="check-row"><input type="checkbox" bind:checked={form.newsletterOptIn} /> Sign up for the dojo newsletter</label><label class="check-row"><input type="checkbox" bind:checked={form.smsOptIn} /> Opt in to SMS updates</label><small>SMS messages require a phone number and may be disabled by staff later.</small></fieldset>
      </div>
    </div>

    <fieldset class="contact-form-step contact-student-step">
      <legend><span class="contact-step-number">2</span><span><strong>{contactType === 'guardian' ? 'Link this guardian to students' : 'Link this emergency contact to a student'}</strong><small>{contactType === 'guardian' ? 'Select every student this guardian should be able to help with.' : 'An emergency contact belongs to one student record.'}</small></span></legend>
      <div class="student-picker-toolbar">
        <label class="student-search-label">Search students<input class="link-student-search" bind:value={studentQuery} type="search" placeholder="Type a student name…" aria-label="Search students to link" /></label>
        <span class="student-selection-count">{form.studentIds.length} selected</span>
      </div>
      <div class="student-link-grid contact-student-grid">
        {#each filteredStudents as student}
          <label class:selected={form.studentIds.includes(student.studentId)} class="student-link-option">
            <input type={contactType === 'emergency' ? 'radio' : 'checkbox'} name="linked-student" checked={form.studentIds.includes(student.studentId)} on:change={(event) => toggleStudent(student.studentId, (event.currentTarget as HTMLInputElement).checked)} />
            <span class="student-option-copy"><strong>{student.name}</strong><small>{form.studentIds.includes(student.studentId) ? 'Selected' : contactType === 'guardian' ? 'Click to link' : 'Choose this student'}</small></span>
            <span class="student-option-mark" aria-hidden="true">{form.studentIds.includes(student.studentId) ? '✓' : '+'}</span>
          </label>
        {:else}
          <p class="muted student-picker-empty">No active students match “{studentQuery}”.</p>
        {/each}
      </div>
    </fieldset>

    <div class="contact-form-footer">
      <div><strong>{selectedStudents.length ? `${selectedStudents.length} student${selectedStudents.length === 1 ? '' : 's'} selected` : 'No student selected yet'}</strong><p class="muted">{contactType === 'guardian' ? 'A guardian can be linked to more than one student.' : 'Choose one student before saving this emergency contact.'}</p></div>
      <div class="contact-form-actions"><button class="button secondary" type="button" on:click={resetForm}>Clear</button><button class="button" disabled={saving || !form.studentIds.length}>{saving ? 'Saving…' : editingId ? 'Save guardian links' : contactType === 'guardian' ? 'Create guardian' : 'Add emergency contact'}</button></div>
    </div>
  </form>
</section>

<section class="card guardian-directory-card">
  <div class="directory-header">
    <div><span class="eyebrow">SAVED RECORDS</span><h3>Guardian directory</h3><p class="muted">Emergency contacts appear inside the student record they are linked to.</p></div>
    <div class="directory-tools"><span class="pill">{visibleGuardians.length} guardian{visibleGuardians.length === 1 ? '' : 's'}</span><label>Search guardians<input class="staff-list-search" bind:value={query} type="search" placeholder="Name, email, or student…" aria-label="Search guardian accounts" /></label></div>
  </div>
  {#if loading}<p class="muted">Loading portal database…</p>{:else}<div class="table-wrap"><table><thead><tr><th>Guardian</th><th>Contact details</th><th>Linked students</th><th>Actions</th></tr></thead><tbody>{#each visibleGuardians as guardian}<tr><td><strong>{guardian.firstName} {guardian.lastName}</strong><small class="table-subtext">Family account</small></td><td>{guardian.email || 'No email'}<br />{guardian.phone || 'No phone'}</td><td>{guardian.students.map((student) => `${student.name} (${student.relationship})`).join(', ') || 'No students linked'}</td><td><button class="button secondary small-button" type="button" on:click={() => edit(guardian)}>Edit guardian</button></td></tr>{:else}<tr><td colspan="4" class="muted">No guardian accounts match this view.</td></tr>{/each}</tbody></table></div>{/if}
</section>
