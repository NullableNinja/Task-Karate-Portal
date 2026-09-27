<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { api } from '$lib/api';

  type EmergencyContact = { contactId: number; name: string; firstName?: string | null; middleInitial?: string | null; lastName?: string | null; pronouns?: string | null; relationship: string; phone?: string | null; email?: string | null; newsletterOptIn?: boolean; smsOptIn?: boolean };
  type Profile = { studentId: number; displayName: string; nickname?: string | null; pronouns?: string | null; honorific?: string | null; roles: string[]; bio?: string | null; favoriteTechnique?: string | null; rankName?: string | null; joinDate?: string | null; totalClasses: number; classesThisMonth: number; unreadMessages: number; achievementCount: number; helperClasses: number; ageGroup?: string | null; birthDate?: string | null; email?: string | null; phone?: string | null; uniformSize?: string | null; beltSize?: string | null; guardians: { name: string; relationship: string; phone?: string | null; email?: string | null }[]; emergencyContacts: EmergencyContact[]; programs: { programName: string; programCode: string; progressionType: string; levelName?: string | null; enrolledDate?: string | null }[] };
  type Achievement = { name: string; description?: string | null; awardedAt?: string | null };
  type Goal = { goalId: number; title: string; targetDate?: string | null; completed: boolean };
  type CheckIns = { checkedInToday: boolean; totalCheckIns: number; checkInsThisMonth: number; lastCheckInDate?: string | null };
  type Membership = { membershipName: string; programName?: string | null; status: string; endsOn?: string | null; syncedAtUtc: string };
  type Document = { documentId: number; name: string; version: string; category: string; requiredForEnrollment: boolean; acceptedForStudent: boolean; acceptedAtUtc?: string | null; isActive: boolean };
  type Program = { programId: number; name: string; description?: string | null };

  let profile: Profile | null = null;
  let achievements: Achievement[] = [];
  let goals: Goal[] = [];
  let checkIns: CheckIns | null = null;
  let memberships: Membership[] = [];
  let documents: Document[] = [];
  let programs: Program[] = [];
  let loading = true;
  let error = '';
  let contactBusy = false;
  let programBusy = false;
  let rolesBusy = false;
  let programForm = { programId: '', progressionType: 'belt', levelName: '', enrolledDate: new Date().toISOString().slice(0, 10) };
  let editingContactId: number | null = null;
  let contactForm = { name: '', firstName: '', middleInitial: '', lastName: '', pronouns: '', relationship: '', phone: '', email: '', newsletterOptIn: false, smsOptIn: false };

  onMount(async () => {
    try {
      const [result, programList] = await Promise.all([
        api<{ profile: Profile; achievements: Achievement[]; goals: Goal[]; checkIns: CheckIns; memberships: Membership[]; documents: Document[] }>(`/api/portal-admin/students/${$page.params.studentId}/detail`),
        api<Program[]>('/api/portal-admin/programs')
      ]);
      profile = result.profile;
      achievements = result.achievements;
      goals = result.goals;
      checkIns = result.checkIns;
      memberships = result.memberships ?? [];
      documents = result.documents ?? [];
      programs = programList;
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not load the student record.';
    } finally {
      loading = false;
    }
  });

  async function addProgram() {
    if (!profile || !programForm.programId) return;
    programBusy = true;
    error = '';
    try {
      await api(`/api/portal-admin/students/${profile.studentId}/programs`, { method: 'POST', body: JSON.stringify({ programId: Number(programForm.programId), progressionType: programForm.progressionType, levelName: programForm.levelName || null, enrolledDate: programForm.enrolledDate || null }) });
      const selected = programs.find((program) => program.programId === Number(programForm.programId));
      if (selected) {
        const next = { programName: selected.name, programCode: `program-${selected.programId}`, progressionType: programForm.progressionType, levelName: programForm.levelName || null, enrolledDate: programForm.enrolledDate || null };
        profile = { ...profile, programs: [...profile.programs.filter((item) => item.programName !== selected.name), next] };
      }
      programForm = { ...programForm, programId: '', levelName: '' };
    } catch (e) { error = e instanceof Error ? e.message : 'Could not add this program.'; }
    finally { programBusy = false; }
  }

  async function saveRoles(roles: string[]) {
    if (!profile) return;
    rolesBusy = true;
    error = '';
    const nextRoles = roles.includes('student') ? roles : ['student', ...roles];
    try {
      await api(`/api/portal-admin/students/${profile.studentId}/roles`, { method: 'PUT', body: JSON.stringify({ roles: nextRoles }) });
      profile = { ...profile, roles: nextRoles };
    } catch (e) { error = e instanceof Error ? e.message : 'Could not update dojo roles.'; }
    finally { rolesBusy = false; }
  }

  function changeRole(role: string, enabled: boolean) {
    if (!profile) return;
    void saveRoles(enabled ? [...profile.roles, role] : profile.roles.filter((item) => item !== role));
  }

  async function removeProgram(program: Profile['programs'][number]) {
    if (!profile || !window.confirm(`Remove ${program.programName} from this student record?`)) return;
    error = '';
    try {
      await api(`/api/portal-admin/students/${profile.studentId}/programs/${encodeURIComponent(program.programCode)}`, { method: 'DELETE' });
      profile = { ...profile, programs: profile.programs.filter((item) => item.programCode !== program.programCode) };
    } catch (e) { error = e instanceof Error ? e.message : 'Could not remove this program.'; }
  }

  function resetContactForm() {
    editingContactId = null;
    contactForm = { name: '', firstName: '', middleInitial: '', lastName: '', pronouns: '', relationship: '', phone: '', email: '', newsletterOptIn: false, smsOptIn: false };
  }

  function editContact(contact: EmergencyContact) {
    editingContactId = contact.contactId;
    contactForm = { name: contact.name, firstName: contact.firstName ?? '', middleInitial: contact.middleInitial ?? '', lastName: contact.lastName ?? '', pronouns: contact.pronouns ?? '', relationship: contact.relationship, phone: contact.phone ?? '', email: contact.email ?? '', newsletterOptIn: contact.newsletterOptIn ?? false, smsOptIn: contact.smsOptIn ?? false };
  }

  async function saveContact() {
    if (!profile) return;
    contactBusy = true;
    error = '';
    try {
      const url = editingContactId
        ? `/api/portal-admin/students/${profile.studentId}/emergency-contacts/${editingContactId}`
        : `/api/portal-admin/students/${profile.studentId}/emergency-contacts`;
      const saved = await api<EmergencyContact>(url, {
        method: editingContactId ? 'PUT' : 'POST',
        body: JSON.stringify({ ...contactForm, phone: contactForm.phone || null, email: contactForm.email || null })
      });
      profile = {
        ...profile,
        emergencyContacts: editingContactId
          ? profile.emergencyContacts.map((contact) => contact.contactId === editingContactId ? saved : contact)
          : [...profile.emergencyContacts, saved]
      };
      resetContactForm();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not save the emergency contact.';
    } finally {
      contactBusy = false;
    }
  }

  async function removeContact(contact: EmergencyContact) {
    if (!profile || !window.confirm(`Remove ${contact.name} from this student record?`)) return;
    error = '';
    try {
      await api(`/api/portal-admin/students/${profile.studentId}/emergency-contacts/${contact.contactId}`, { method: 'DELETE' });
      profile = { ...profile, emergencyContacts: profile.emergencyContacts.filter((item) => item.contactId !== contact.contactId) };
      if (editingContactId === contact.contactId) resetContactForm();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not remove the emergency contact.';
    }
  }
</script>

<svelte:head><title>Staff · Student record | Task Karate</title></svelte:head>

<div class="toolbar">
  <div>
    <a class="back-link" href="/staff/students">← Back to students</a>
    <span class="eyebrow">PEOPLE · STUDENT 360</span>
    <h2>{profile?.displayName ?? 'Student record'}</h2>
    <p class="muted">One operational record for identity, family, progress, attendance, recognition, documents, and account context.</p>
  </div>
  <div class="toolbar-actions">
    {#if profile}
      <a class="button secondary" href={`/student/profile/${profile.studentId}`}>View student profile</a>
      <a class="button" href="/staff/attendance">Take attendance</a>
    {/if}
  </div>
</div>

{#if error}<div class="error" role="alert">{error}</div>{/if}

{#if loading}
  <section class="card loading-card"><p class="muted">Loading student record…</p></section>
{:else if profile}
  <div class="student-admin-summary">
    <section class="card"><span class="eyebrow">CURRENT STATUS</span><strong>{profile.rankName ?? 'Rank not assigned'}</strong><span class="muted">{profile.ageGroup ?? 'Age group not set'} · Member since {profile.joinDate?.slice(0, 10) ?? 'not recorded'}</span></section>
    <section class="card"><span class="eyebrow">TRAINING</span><strong>{profile.totalClasses} classes</strong><span class="muted">{profile.classesThisMonth} this month · {profile.helperClasses} helper classes</span></section>
    <section class="card"><span class="eyebrow">RECOGNITION</span><strong>{profile.achievementCount} achievements</strong><span class="muted">{checkIns?.totalCheckIns ?? 0} studio check-ins</span></section>
    <section class="card"><span class="eyebrow">MESSAGES</span><strong>{profile.unreadMessages} unread</strong><span class="muted">Staff review should remain safety-aware.</span></section>
  </div>

  <div class="admin-detail-grid">
    <section class="card">
      <div class="section-heading"><div><span class="eyebrow">IDENTITY & FAMILY</span><h3>Contact record</h3></div><a class="text-link" href="/staff/guardians">Manage families →</a></div>
      <dl class="detail-list">
        <div><dt>Nickname</dt><dd>{profile.nickname ?? 'Not recorded'}</dd></div>
        <div><dt>Pronouns</dt><dd>{profile.pronouns ?? 'Not recorded'}</dd></div>
        <div><dt>Honorific</dt><dd>{profile.honorific ?? 'Not set'}{profile.rankName?.toLowerCase().includes('black') && profile.honorific ? ` · addressed ${profile.honorific} ${profile.displayName.split(' ').slice(-1).join(' ')}` : ''}</dd></div>
        <div><dt>Email</dt><dd>{profile.email ?? 'Not recorded'}</dd></div>
        <div><dt>Phone</dt><dd>{profile.phone ?? 'Not recorded'}</dd></div>
        <div><dt>Uniform / belt size</dt><dd>{profile.uniformSize ?? '—'} / {profile.beltSize ?? '—'}</dd></div>
        <div><dt>Favorite technique</dt><dd>{profile.favoriteTechnique ?? 'Not recorded'}</dd></div>
      </dl>
      <fieldset class="student-role-fieldset"><legend>Dojo roles & responsibilities</legend><p class="muted">A student can also help teach or publish dojo news. Staff login access is managed separately from Staff access.</p><div class="student-role-grid">{#each [{ id: 'student', label: 'Student' }, { id: 'assistant_instructor', label: 'Assistant instructor' }, { id: 'instructor', label: 'Instructor' }, { id: 'news_admin', label: 'News admin' }] as role}<label class="check-row"><input type="checkbox" checked={profile.roles.includes(role.id)} disabled={rolesBusy || role.id === 'student'} on:change={(event) => changeRole(role.id, (event.currentTarget as HTMLInputElement).checked)} />{role.label}</label>{/each}</div></fieldset>
      {#if profile.guardians.length}
        <h4>Guardians</h4>
        <ul class="admin-detail-list">{#each profile.guardians as guardian}<li><strong>{guardian.name}</strong><span>{guardian.relationship} · {guardian.email ?? guardian.phone ?? 'No contact detail'}</span></li>{/each}</ul>
      {:else}<p class="muted">No linked guardians are recorded.</p>{/if}
      <div class="section-heading contact-heading"><h4>Emergency contacts</h4><button class="button secondary small-button" type="button" on:click={resetContactForm}>Add contact</button></div>
      {#if profile.emergencyContacts.length}
        <ul class="admin-detail-list">{#each profile.emergencyContacts as contact}<li><strong>{contact.name}</strong><span>{contact.relationship} · {contact.phone ?? contact.email ?? 'No contact detail'}</span><div class="table-actions"><button class="button secondary small-button" type="button" on:click={() => editContact(contact)}>Edit</button><button class="button danger small-button" type="button" on:click={() => removeContact(contact)}>Remove</button></div></li>{/each}</ul>
      {:else}<p class="muted">No emergency contacts are recorded.</p>{/if}
      <form class="form-grid compact-form" on:submit|preventDefault={saveContact}>
        <label>First name<input bind:value={contactForm.firstName} maxlength="80" required /></label>
        <label>Middle initial<input bind:value={contactForm.middleInitial} maxlength="4" placeholder="e.g. R." /></label>
        <label>Last name<input bind:value={contactForm.lastName} maxlength="80" required /></label>
        <label>Pronouns<input bind:value={contactForm.pronouns} maxlength="80" placeholder="e.g. she/her" /></label>
        <label>Relationship<input bind:value={contactForm.relationship} maxlength="80" required placeholder="Parent, guardian, neighbor…" /></label>
        <label>Phone<input bind:value={contactForm.phone} maxlength="50" type="tel" /></label>
        <label>Email<input bind:value={contactForm.email} maxlength="254" type="email" /></label>
        <div class="full detail-contact-preferences"><label class="check-row"><input type="checkbox" bind:checked={contactForm.newsletterOptIn} /> Sign up for the dojo newsletter</label><label class="check-row"><input type="checkbox" bind:checked={contactForm.smsOptIn} /> Opt in to SMS updates</label></div>
        <div class="full"><button class="button" disabled={contactBusy}>{contactBusy ? 'Saving…' : editingContactId ? 'Save contact' : 'Add emergency contact'}</button>{#if editingContactId}<button class="button secondary" type="button" on:click={resetContactForm}>Cancel</button>{/if}</div>
      </form>
    </section>
    <section class="card">
      <div class="section-heading"><div><span class="eyebrow">PROGRAMS</span><h3>Active memberships</h3></div><a class="text-link" href="/staff/classes">Manage classes →</a></div>
      {#if profile.programs.length}<ul class="admin-detail-list">{#each profile.programs as program}<li><strong>{program.programName}</strong><span>{program.progressionType}{program.levelName ? ` · ${program.levelName}` : ''} · joined {program.enrolledDate ?? 'date not recorded'}</span><button class="button danger small-button" type="button" on:click={() => removeProgram(program)}>Remove from program</button></li>{/each}</ul>{:else}<p class="muted">No active program memberships are recorded.</p>{/if}
      <form class="form-grid compact-form" on:submit|preventDefault={addProgram}>
        <label>Program<select bind:value={programForm.programId} required><option value="" disabled>Select a program</option>{#each programs as program}<option value={program.programId}>{program.name}</option>{/each}</select></label>
        <label>Progression<select bind:value={programForm.progressionType}><option value="belt">Belt track</option><option value="level">Level track</option><option value="attendance">Attendance / participation</option><option value="other">Other</option></select></label>
        <label>Current level <small class="muted">optional</small><input bind:value={programForm.levelName} placeholder="e.g. Green Belt" /></label>
        <label>Enrolled date<input type="date" bind:value={programForm.enrolledDate} /></label>
        <div class="full"><button class="button" disabled={programBusy || !programForm.programId}>{programBusy ? 'Adding…' : 'Add to program'}</button></div>
      </form>
      <div class="detail-callout"><span class="eyebrow">MYSTUDIO BOUNDARY</span><p>Payment and membership billing remain managed in MyStudio. This portal record is for student operations and engagement.</p></div>
    </section>
  </div>

  <div class="admin-detail-grid">
    <section class="card"><div class="section-heading"><div><span class="eyebrow">MYSTUDIO MEMBERSHIP SNAPSHOT</span><h3>Billing context</h3></div><span class="pill">Read-only</span></div><p class="muted">MyStudio remains the source of truth for payments, invoices, autopay, and refunds.</p>{#if memberships.length}<ul class="admin-detail-list">{#each memberships as membership}<li><strong>{membership.membershipName}</strong><span>{membership.status}{membership.programName ? ` · ${membership.programName}` : ''}{membership.endsOn ? ` · ends ${membership.endsOn.slice(0, 10)}` : ''} · synced {new Date(membership.syncedAtUtc).toLocaleDateString()}</span></li>{/each}</ul>{:else}<p class="muted">No MyStudio snapshot has been imported for this student.</p>{/if}</section>
    <section class="card"><div class="section-heading"><div><span class="eyebrow">DOCUMENTS & WAIVERS</span><h3>Compliance status</h3></div><a class="text-link" href="/staff/documents">Manage documents →</a></div>{#if documents.length}<ul class="admin-detail-list">{#each documents.filter((item) => item.isActive) as document}<li><strong>{document.name} · v{document.version}</strong><span class:status-ok={document.acceptedForStudent}>{document.acceptedForStudent ? `Accepted ${document.acceptedAtUtc ? new Date(document.acceptedAtUtc).toLocaleDateString() : ''}` : document.requiredForEnrollment ? 'Required · outstanding' : 'Optional · outstanding'}</span></li>{/each}</ul>{:else}<p class="muted">No document definitions have been configured.</p>{/if}</section>
  </div>

  <div class="admin-detail-grid">
    <section class="card"><div class="section-heading"><div><span class="eyebrow">RECOGNITION</span><h3>Achievements</h3></div><a class="text-link" href="/staff/awards">Manage awards →</a></div>{#if achievements.length}<ul class="admin-detail-list">{#each achievements as achievement}<li><strong>{achievement.name}</strong><span>{achievement.awardedAt ? new Date(achievement.awardedAt).toLocaleDateString() : 'Awarded'} · {achievement.description ?? 'No description'}</span></li>{/each}</ul>{:else}<p class="muted">No achievements recorded.</p>{/if}</section>
    <section class="card"><div class="section-heading"><div><span class="eyebrow">DOJO CHECK-INS</span><h3>HIYAH! history</h3></div><a class="text-link" href="/staff/check-ins">Open check-in board →</a></div>{#if checkIns}<div class="mini-stat-row"><div><strong>{checkIns.totalCheckIns}</strong><span>All time</span></div><div><strong>{checkIns.checkInsThisMonth}</strong><span>This month</span></div><div><strong>{checkIns.lastCheckInDate ?? '—'}</strong><span>Last visit</span></div></div><p class="muted">{checkIns.checkedInToday ? 'Checked in today.' : 'Not checked in today.'}</p>{/if}<div class="detail-callout"><span class="eyebrow">STAFF CONTROL</span><p>Check-in frequency is protected server-side at one per calendar day.</p></div></section>
  </div>

  <section class="card"><div class="section-heading"><div><span class="eyebrow">TRAINING WORK</span><h3>Goals</h3></div><a class="text-link" href="/student/training">View student training →</a></div>{#if goals.length}<div class="admin-goal-grid">{#each goals as goal}<div class:complete={goal.completed} class="admin-goal"><strong>{goal.title}</strong><span>{goal.completed ? 'Complete' : 'In progress'}{goal.targetDate ? ` · target ${goal.targetDate}` : ''}</span></div>{/each}</div>{:else}<p class="muted">No goals recorded.</p>{/if}</section>
{:else}
  <section class="card empty-state"><strong>Student record unavailable.</strong><p class="muted">The record may be inactive or no longer available in the portal database.</p><a class="button secondary" href="/staff/students">Return to students</a></section>
{/if}
