<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type GuardianStudent = { studentId: number; name: string; relationship: string };
  type Guardian = { guardianId: number; firstName: string; lastName: string; email?: string; phone?: string; isActive: boolean; students: GuardianStudent[] };
  type StudentOption = { studentId: number; name: string };
  type GuardianForm = { firstName: string; lastName: string; email: string; phone: string; studentIds: number[] };
  const blankForm = (): GuardianForm => ({ firstName: '', lastName: '', email: '', phone: '', studentIds: [] });
  let guardians: Guardian[] = [];
  let students: StudentOption[] = [];
  let form = blankForm();
  let editingId = 0;
  let query = '';
  let error = '';
  let notice = '';
  let loading = true;
  let saving = false;

  async function load() { loading = true; error = ''; try { [guardians, students] = await Promise.all([api<Guardian[]>('/api/portal-admin/guardians'), api<StudentOption[]>('/api/portal-admin/guardian-students')]); } catch (e) { error = e instanceof Error ? e.message : 'Could not load portal guardians.'; } finally { loading = false; } }
  $: visibleGuardians = guardians.filter((guardian) => `${guardian.firstName} ${guardian.lastName} ${guardian.email ?? ''} ${guardian.students.map((student) => student.name).join(' ')}`.toLowerCase().includes(query.trim().toLowerCase()));
  function resetForm() { form = blankForm(); editingId = 0; }
  function edit(guardian: Guardian) { editingId = guardian.guardianId; form = { firstName: guardian.firstName, lastName: guardian.lastName, email: guardian.email ?? '', phone: guardian.phone ?? '', studentIds: guardian.students.map((student) => student.studentId) }; window.scrollTo({ top: 0, behavior: 'smooth' }); }
  async function save() { error = ''; notice = ''; saving = true; try { if (editingId) { await api(`/api/portal-admin/guardians/${editingId}`, { method: 'PUT', body: JSON.stringify(form) }); notice = 'Guardian record and student links updated in the portal database.'; } else { await api('/api/portal-admin/guardians', { method: 'POST', body: JSON.stringify(form) }); notice = 'Guardian created and linked in the portal database.'; } resetForm(); await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not save guardian.'; } finally { saving = false; } }
  onMount(load);
</script>

<svelte:head><title>Staff · Guardians | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">PORTAL PEOPLE</span><h2>Guardians</h2><p class="muted">This page reads the same portal database as student profiles and supports one guardian linked to multiple students.</p></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}
<section class="card"><div class="section-heading"><div><h3>{editingId ? 'Edit guardian record' : 'Create guardian record'}</h3><p class="muted">Only staff can change guardian links. Keep contact details current and appropriate for supervised use.</p></div>{#if editingId}<button class="button secondary small-button" type="button" on:click={resetForm}>Cancel edit</button>{/if}</div><form class="form-grid" on:submit|preventDefault={save}><label>First name<input bind:value={form.firstName} required /></label><label>Last name<input bind:value={form.lastName} required /></label><label>Email<input type="email" bind:value={form.email} /></label><label>Phone<input type="tel" bind:value={form.phone} /></label><fieldset class="full"><legend>Link students</legend><div class="student-link-grid">{#each students as student}<label class="check-row"><input type="checkbox" value={student.studentId} checked={form.studentIds.includes(student.studentId)} on:change={(event) => { const checked = (event.currentTarget as HTMLInputElement).checked; form = { ...form, studentIds: checked ? [...form.studentIds, student.studentId] : form.studentIds.filter((id) => id !== student.studentId) }; }} /> {student.name}</label>{/each}</div></fieldset><div class="full"><button class="button" disabled={saving}>{saving ? 'Saving…' : editingId ? 'Save guardian changes' : 'Create guardian'}</button></div></form></section>
<section class="card"><div class="section-heading"><div><h3>Portal guardian records</h3><span class="pill">{visibleGuardians.length}</span></div><input class="staff-list-search" bind:value={query} type="search" placeholder="Search guardians or students…" aria-label="Search portal guardians" /></div>{#if loading}<p class="muted">Loading portal database…</p>{:else}<div class="table-wrap"><table><thead><tr><th>Guardian</th><th>Contact</th><th>Linked students</th><th>Actions</th></tr></thead><tbody>{#each visibleGuardians as guardian}<tr><td><strong>{guardian.firstName} {guardian.lastName}</strong></td><td>{guardian.email || 'No email'}<br />{guardian.phone || 'No phone'}</td><td>{guardian.students.map((student) => `${student.name} (${student.relationship})`).join(', ') || 'None linked'}</td><td><button class="button secondary small-button" type="button" on:click={() => edit(guardian)}>Edit links</button></td></tr>{:else}<tr><td colspan="4" class="muted">No portal guardians match this view.</td></tr>{/each}</tbody></table></div>{/if}</section>
