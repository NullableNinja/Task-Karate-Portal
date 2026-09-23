<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import type { Program, Session, Template } from '$lib/types';
  let programs: Program[] = [];
  let sessions: Session[] = [];
  let templates: Template[] = [];
  let selectedDate = new Date().toISOString().slice(0, 10);
  let error = '';
  let notice = '';
  let loading = true;
  let programName = '';
  let template = { programId: '', name: '', dayOfWeek: 'Monday', startTime: '05:00 PM', durationMinutes: 60, beltScope: '' };
  let session = { classTemplateId: '', sessionDateUtc: selectedDate, notes: '' };

  async function load() { loading = true; error = ''; try { [programs, sessions] = await Promise.all([api<Program[]>('/api/programs'), api<Session[]>(`/api/sessions?date=${selectedDate}`)]); templates = programs.flatMap((item) => item.templates); session = { ...session, sessionDateUtc: selectedDate }; } catch (e) { error = e instanceof Error ? e.message : 'Could not load class data.'; } finally { loading = false; } }
  async function createProgram() { try { await api('/api/programs', { method: 'POST', body: JSON.stringify({ name: programName, description: 'Managed in the Task Karate staff workspace.' }) }); programName = ''; notice = 'Program created.'; await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not create program.'; } }
  async function createTemplate() { try { await api('/api/class-templates', { method: 'POST', body: JSON.stringify(template) }); notice = 'Class template created.'; await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not create template.'; } }
  async function createSession() { try { await api('/api/sessions', { method: 'POST', body: JSON.stringify({ ...session, sessionDateUtc: new Date(`${selectedDate}T00:00:00Z`).toISOString() }) }); notice = `Session created for ${new Date(`${selectedDate}T12:00:00`).toLocaleDateString()}.`; await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not create session.'; } }
  onMount(load);
</script>

<svelte:head><title>Staff · Classes | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">CLASSES</span><h2>Programs and class sessions</h2><p class="muted">Templates describe recurring classes; sessions are the dated occurrences students can attend.</p></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}
<section class="card date-toolbar"><label>Working date<input type="date" bind:value={selectedDate} on:change={load} /></label><span class="muted">{sessions.length} session{sessions.length === 1 ? '' : 's'} on this date</span></section>
<div class="split"><section class="card"><h3>Program</h3><form on:submit|preventDefault={createProgram} class="form-grid"><label class="full">Program name<input bind:value={programName} required placeholder="e.g. Kids Karate" /></label><div class="full"><button class="button" disabled={!programName.trim()}>Create program</button></div></form><h3>Class template</h3><form on:submit|preventDefault={createTemplate} class="form-grid"><label class="full">Program<select bind:value={template.programId} required><option value="" disabled>Select program</option>{#each programs as program}<option value={program.id}>{program.name}</option>{/each}</select></label><label class="full">Template name<input bind:value={template.name} required placeholder="e.g. White · Gold · Orange" /></label><label>Day<select bind:value={template.dayOfWeek}>{#each ['Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday'] as day}<option>{day}</option>{/each}</select></label><label>Start time<input bind:value={template.startTime} required /></label><label>Duration (minutes)<input type="number" min="1" max="480" bind:value={template.durationMinutes} required /></label><label>Belt scope<input bind:value={template.beltScope} placeholder="Optional" /></label><div class="full"><button class="button" disabled={!template.programId || !template.name.trim()}>Create template</button></div></form></section><section class="card"><h3>Create dated session</h3><form on:submit|preventDefault={createSession} class="form-grid"><label class="full">Template<select bind:value={session.classTemplateId} required><option value="" disabled>Select template</option>{#each templates as item}<option value={item.id}>{item.name} · {item.startTime}</option>{/each}</select></label><label class="full">Session date<input type="date" bind:value={selectedDate} required /></label><label class="full">Staff notes<textarea bind:value={session.notes} maxlength="500" rows="2" placeholder="Optional notes for the instructor team"></textarea></label><div class="full"><button class="button" disabled={!session.classTemplateId}>Create session</button></div></form><h3>Sessions on {new Date(`${selectedDate}T12:00:00`).toLocaleDateString()}</h3>{#if loading}<p class="muted">Loading sessions…</p>{:else}{#each sessions as item}<article class="announcement"><strong>{item.name}</strong><br /><span>{item.startTime} · {item.durationMinutes} minutes</span>{#if item.isCancelled}<span class="pill cancelled-pill">Cancelled</span>{/if}</article>{:else}<p class="muted">No sessions on this date.</p>{/each}{/if}</section></div>
