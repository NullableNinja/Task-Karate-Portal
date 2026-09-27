<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type Document = { documentId: number; name: string; version: string; category: string; description?: string | null; storageKey?: string | null; isActive: boolean; requiredForEnrollment: boolean; acceptanceCount: number; acceptedForStudent: boolean; acceptedAtUtc?: string | null };
  type Student = { studentId: number; displayName: string; status: string };
  let documents: Document[] = [];
  let studentDocuments: Document[] = [];
  let students: Student[] = [];
  let selectedStudent = '';
  let selectedDocument = '';
  let loading = true;
  let busy = '';
  let error = '';
  let notice = '';
  let form = { name: '', version: '1.0', category: 'Waiver', description: '', storageKey: '', requiredForEnrollment: true };

  async function load() {
    loading = true; error = '';
    try {
      [documents, students] = await Promise.all([
        api<Document[]>('/api/portal-admin/documents'),
        api<Student[]>('/api/portal-admin/students')
      ]);
      if (selectedStudent) await loadStudentDocuments();
    } catch (e) { error = e instanceof Error ? e.message : 'Could not load documents.'; }
    finally { loading = false; }
  }

  async function loadStudentDocuments() {
    if (!selectedStudent) { studentDocuments = []; return; }
    try { studentDocuments = await api<Document[]>(`/api/portal-admin/documents?studentId=${selectedStudent}`); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not load student document status.'; }
  }

  async function createDocument() {
    busy = 'create'; error = ''; notice = '';
    try {
      await api('/api/portal-admin/documents', { method: 'POST', body: JSON.stringify(form) });
      form = { name: '', version: '1.0', category: 'Waiver', description: '', storageKey: '', requiredForEnrollment: true };
      notice = 'Document definition created.'; await load();
    } catch (e) { error = e instanceof Error ? e.message : 'Could not create document.'; }
    finally { busy = ''; }
  }

  async function toggleDocument(item: Document) {
    busy = `toggle-${item.documentId}`; error = ''; notice = '';
    try { await api(`/api/portal-admin/documents/${item.documentId}/active`, { method: 'POST', body: JSON.stringify({ active: !item.isActive }) }); notice = `${item.name} ${item.isActive ? 'archived' : 'restored'}.`; await load(); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not update document.'; }
    finally { busy = ''; }
  }

  async function acceptDocument() {
    if (!selectedStudent || !selectedDocument) return;
    busy = 'accept'; error = ''; notice = '';
    try { await api(`/api/portal-admin/students/${selectedStudent}/documents/${selectedDocument}/accept`, { method: 'POST', body: JSON.stringify({ guardianId: null, notes: null }) }); notice = 'Document acceptance recorded.'; await loadStudentDocuments(); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not record acceptance.'; }
    finally { busy = ''; }
  }

  $: activeDocuments = documents.filter((item) => item.isActive);
  $: selectedStudentName = students.find((item) => String(item.studentId) === selectedStudent)?.displayName ?? 'student';
  onMount(load);
</script>

<svelte:head><title>Staff · Documents & waivers | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">ADMINISTRATION</span><h2>Documents & waivers</h2><p class="muted">This is a metadata register and staff acknowledgement log—not a file-storage or e-signature system.</p></div><button class="button secondary" type="button" on:click={load} disabled={loading}>Refresh</button></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}
{#if loading}<section class="card"><p class="muted">Loading document register…</p></section>{:else}<div class="configuration-grid"><section class="card"><div class="section-heading"><div><span class="eyebrow">DOCUMENT REGISTER</span><h3>Active and archived forms</h3><p class="muted">A new version is a new record. Existing acceptance history is never overwritten.</p></div><span class="pill">{documents.length}</span></div><div class="config-list">{#each documents as item}<article class:inactive={!item.isActive} class="config-row"><div><strong>{item.name} <small>v{item.version}</small></strong><small>{item.category} · {item.acceptanceCount} acceptance{item.acceptanceCount === 1 ? '' : 's'}{item.requiredForEnrollment ? ' · required for enrollment' : ''}</small>{#if item.description}<small>{item.description}</small>{/if}</div><button class="button secondary small-button" type="button" on:click={() => toggleDocument(item)} disabled={busy === `toggle-${item.documentId}`}>{item.isActive ? 'Archive' : 'Restore'}</button></article>{:else}<p class="muted">No document definitions exist yet.</p>{/each}</div></section><section class="card"><div class="section-heading"><div><span class="eyebrow">NEW VERSION</span><h3>Add a waiver or form</h3><p class="muted">The portal stores metadata and an optional external storage key; it does not pretend to be a file-management system.</p></div></div><form class="form-grid" on:submit|preventDefault={createDocument}><label>Name<input bind:value={form.name} maxlength="160" required placeholder="Annual liability waiver" /></label><label>Version<input bind:value={form.version} maxlength="40" required /></label><label>Category<input bind:value={form.category} maxlength="80" required /></label><label>Storage key / URL<input bind:value={form.storageKey} maxlength="500" placeholder="Optional signed or managed file reference" /></label><label class="full">Description<textarea bind:value={form.description} maxlength="2000" rows="3" placeholder="What this document covers"></textarea></label><label class="check-row full"><input type="checkbox" bind:checked={form.requiredForEnrollment} /> Required before enrollment is complete</label><div class="full"><button class="button" disabled={busy === 'create' || !form.name.trim()}>{busy === 'create' ? 'Creating…' : 'Create document definition'}</button></div></form></section></div><section class="card"><div class="section-heading"><div><span class="eyebrow">STUDENT RECORD</span><h3>Record an acceptance</h3><p class="muted">Use this for a paper form, in-person review, or another verified staff workflow. It is audited.</p></div></div><div class="form-grid"><label>Student<select bind:value={selectedStudent} on:change={loadStudentDocuments}><option value="">Choose a student</option>{#each students.filter((student) => student.status === 'active') as student}<option value={student.studentId}>{student.displayName}</option>{/each}</select></label><label>Document<select bind:value={selectedDocument} disabled={!selectedStudent}><option value="">Choose an active document</option>{#each studentDocuments.filter((item) => item.isActive) as item}<option value={item.documentId}>{item.name} · v{item.version}{item.acceptedForStudent ? ' · already accepted' : ''}</option>{/each}</select></label><div class="full"><button class="button" type="button" on:click={acceptDocument} disabled={!selectedStudent || !selectedDocument || busy === 'accept'}>{busy === 'accept' ? 'Recording…' : `Record acceptance for ${selectedStudentName}`}</button></div></div>{#if selectedStudent}<div class="config-list document-status-list">{#each studentDocuments as item}<article class="config-row"><div><strong>{item.name} · v{item.version}</strong><small>{item.acceptedForStudent ? `Accepted ${item.acceptedAtUtc ? new Date(item.acceptedAtUtc).toLocaleDateString() : ''}` : item.requiredForEnrollment ? 'Required · not accepted' : 'Optional · not accepted'}</small></div><span class:status-ok={item.acceptedForStudent} class="pill">{item.acceptedForStudent ? 'Accepted' : 'Outstanding'}</span></article>{/each}</div>{/if}</section><section class="card detail-callout"><span class="eyebrow">SYSTEM BOUNDARY</span><p>Documents are operational records in the portal database. Payment agreements and billing remain in MyStudio, and lead capture remains the Task-Karate-Web email workflow.</p></section>{/if}
