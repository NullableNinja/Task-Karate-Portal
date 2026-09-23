<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import type { Guardian, Student } from '$lib/types';
  type GuardianForm = { firstName: string; lastName: string; email: string; phone: string; studentIds: string[] };
  const blankForm = (): GuardianForm => ({ firstName: '', lastName: '', email: '', phone: '', studentIds: [] });
  let guardians: Guardian[] = [];
  let students: Student[] = [];
  let form = blankForm();
  let editingId = '';
  let query = '';
  let error = '';
  let notice = '';
  let loading = true;
  let saving = false;

  async function load() { loading = true; try { [guardians, students] = await Promise.all([api<Guardian[]>('/api/guardians'), api<Student[]>('/api/students')]); } catch (e) { error = e instanceof Error ? e.message : 'Could not load guardians.'; } finally { loading = false; } }
  $: visibleGuardians = guardians.filter((guardian) => `${guardian.firstName} ${guardian.lastName} ${guardian.email}`.toLowerCase().includes(query.trim().toLowerCase()));
  function edit(guardian: Guardian) { editingId = guardian.id; form = { firstName: guardian.firstName, lastName: guardian.lastName, email: guardian.email, phone: guardian.phone ?? '', studentIds: guardian.students.map((student) => student.id) }; }
  function resetForm() { editingId = ''; form = blankForm(); }
  async function save() { error = ''; notice = ''; saving = true; try { if (editingId) { await api(`/api/guardians/${editingId}`, { method: 'PUT', body: JSON.stringify(form) }); notice = 'Guardian record updated.'; } else { await api('/api/guardians', { method: 'POST', body: JSON.stringify(form) }); notice = 'Guardian created and linked.'; } resetForm(); await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not save guardian.'; } finally { saving = false; } }
  onMount(load);
</script>

<svelte:head><title>Staff · Guardians | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">PEOPLE</span><h2>Guardians</h2><p class="muted">One guardian record can be linked to multiple students without duplicates.</p></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}
<section class="card"><div class="section-heading"><div><h3>{editingId ? 'Edit guardian' : 'Create guardian and link students'}</h3><p class="muted">Changing links replaces only this guardian’s student relationships.</p></div>{#if editingId}<button class="button secondary small-button" type="button" on:click={resetForm}>Cancel edit</button>{/if}</div><form class="form-grid" on:submit|preventDefault={save}><label>First name<input bind:value={form.firstName} required /></label><label>Last name<input bind:value={form.lastName} required /></label><label>Email<input type="email" bind:value={form.email} required /></label><label>Phone<input bind:value={form.phone} /></label><label class="full">Linked students<select multiple size="6" bind:value={form.studentIds}>{#each students as student}<option value={student.id}>{student.preferredName || student.firstName} {student.lastName}</option>{/each}</select></label><div class="full"><button class="button" disabled={saving}>{saving ? 'Saving…' : editingId ? 'Save changes' : 'Save guardian'}</button></div></form></section>
<section class="card"><div class="section-heading"><div><h3>Guardian records</h3><span class="pill">{visibleGuardians.length}</span></div><input class="staff-list-search" bind:value={query} type="search" placeholder="Search guardians…" aria-label="Search guardians" /></div>{#if loading}<p class="muted">Loading guardian records…</p>{:else}<div class="table-wrap"><table><thead><tr><th>Name</th><th>Contact</th><th>Linked students</th><th>Actions</th></tr></thead><tbody>{#each visibleGuardians as guardian}<tr><td><strong>{guardian.firstName} {guardian.lastName}</strong></td><td>{guardian.email}<br />{guardian.phone || 'No phone'}</td><td>{guardian.students.map((student) => student.name).join(', ') || 'None linked'}</td><td><button class="button secondary small-button" type="button" on:click={() => edit(guardian)}>Edit</button></td></tr>{:else}<tr><td colspan="4" class="muted">No guardians match this view.</td></tr>{/each}</tbody></table></div>{/if}</section>
