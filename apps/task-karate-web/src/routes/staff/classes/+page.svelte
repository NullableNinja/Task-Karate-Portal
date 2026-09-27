<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type ScheduleItem = { scheduleId: number; dayOfWeek: string; startTime: string; durationMinutes: number };
  type Template = { templateId: number; name: string; dayOfWeek: string; startTime: string; durationMinutes: number; beltScope?: string | null; classType: string; appointmentOnly?: boolean; programId: number; schedules?: ScheduleItem[] };
  type Program = { programId: number; name: string; description?: string | null; templates: Template[] };
  type Session = { sessionId: number; classTemplateId: number; name: string; startTime: string; durationMinutes: number; sessionDateUtc: string; isCancelled: boolean; cancellationReason?: string | null; classType: string; assignedStudentId?: number | null; location?: string | null };
  type StudentOption = { studentId: number; displayName: string; rankName?: string | null };
  type ScheduleMode = 'recurring' | 'one-time';

  const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
  const beltScopeOptions = ['White Belt', 'Yellow Belt', 'Orange Belt', 'Green Belt', 'Purple Belt', 'Blue Belt', 'Red Belt', 'Brown Belt', 'Black Belt'];
  const classTypes = [
    { id: 'Class', label: 'Regular class', description: 'A scheduled class students can attend.' },
    { id: 'Seminar', label: 'Seminar', description: 'A focused workshop or special training block.' },
    { id: 'Private lesson', label: 'Private lesson', description: 'Appointment-based instruction for one student.' },
    { id: 'Belt testing', label: 'Belt testing', description: 'A promotion or rank-evaluation session.' }
  ];
  const blankTemplate = () => ({ programId: '', name: '', dayOfWeek: 'Monday', startTime: '05:00 PM', durationMinutes: 60, beltScope: [] as string[], classType: 'Class', appointmentOnly: false });

  let programs: Program[] = [];
  let templates: Template[] = [];
  let students: StudentOption[] = [];
  let sessions: Session[] = [];
  let template = blankTemplate();
  let editingTemplateId: number | null = null;
  let scheduleMode: ScheduleMode = 'recurring';
  let oneTimeTemplateId = '';
  let oneTimeDate = new Date().toISOString().slice(0, 10);
  let oneTimeAssignedStudentId = '';
  let manageProgramId = '';
  let cancellationProgramId = '';
  let cancellationTemplateId = '';
  let cancellationDate = new Date().toISOString().slice(0, 10);
  let error = '';
  let notice = '';
  let loading = true;
  let savingTemplate = false;
  let schedulingOneTime = false;
  let cancellingInstance = false;

  function firstTemplateFor(programId: string) {
    return programs.find((program) => String(program.programId) === String(programId))?.templates[0];
  }

  function setDefaultSelections() {
    const firstProgram = programs[0];
    if (!firstProgram) return;
    if (!manageProgramId || !programs.some((program) => String(program.programId) === String(manageProgramId))) manageProgramId = String(firstProgram.programId);
    if (!cancellationProgramId || !programs.some((program) => String(program.programId) === String(cancellationProgramId))) cancellationProgramId = String(firstProgram.programId);
    const cancellationFirst = firstTemplateFor(cancellationProgramId);
    const cancellationProgram = programs.find((program) => String(program.programId) === String(cancellationProgramId));
    if (!cancellationTemplateId || !cancellationProgram?.templates.some((item) => String(item.templateId) === String(cancellationTemplateId))) cancellationTemplateId = cancellationFirst ? String(cancellationFirst.templateId) : '';
    if (!oneTimeTemplateId || !templates.some((item) => String(item.templateId) === String(oneTimeTemplateId))) oneTimeTemplateId = templates[0] ? String(templates[0].templateId) : '';
  }

  async function load() {
    loading = true;
    error = '';
    try {
      [programs, sessions, students] = await Promise.all([
        api<Program[]>('/api/portal-admin/programs'),
        api<Session[]>(`/api/portal-admin/sessions?date=${cancellationDate}`),
        api<StudentOption[]>('/api/portal-admin/attendance/students')
      ]);
      templates = programs.flatMap((program) => program.templates);
      setDefaultSelections();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not load programs and classes.';
    } finally {
      loading = false;
    }
  }

  async function refreshCancellationSessions() {
    try {
      sessions = await api<Session[]>(`/api/portal-admin/sessions?date=${cancellationDate}`);
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not load class instances for that date.';
    }
  }

  async function saveTemplate() {
    error = '';
    notice = '';
    savingTemplate = true;
    try {
      const payload = { ...template, programId: Number(template.programId), durationMinutes: Number(template.durationMinutes) };
      if (editingTemplateId) {
        await api(`/api/portal-admin/class-templates/${editingTemplateId}`, { method: 'PUT', body: JSON.stringify(payload) });
        notice = 'Recurring class updated in the portal database.';
      } else {
        await api('/api/portal-admin/class-templates', { method: 'POST', body: JSON.stringify(payload) });
        notice = 'Recurring class added to the portal database and upcoming schedule.';
      }
      cancelEdit();
      await load();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not save the recurring class.';
    } finally {
      savingTemplate = false;
    }
  }

  async function scheduleOneTime() {
    if (!oneTimeTemplateId || !oneTimeDate) return;
    error = '';
    notice = '';
    schedulingOneTime = true;
    try {
      await api('/api/portal-admin/sessions', {
        method: 'POST',
        body: JSON.stringify({
          classTemplateId: Number(oneTimeTemplateId),
          sessionDateUtc: new Date(`${oneTimeDate}T00:00:00Z`).toISOString(),
          notes: null,
          assignedStudentId: oneTimeAssignedStudentId ? Number(oneTimeAssignedStudentId) : null
        })
      });
      notice = 'One-time class session added to the portal database. The recurring class was not changed.';
      await load();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not schedule the one-time class.';
    } finally {
      schedulingOneTime = false;
    }
  }

  function editTemplate(item: Template) {
    const firstSchedule = classSchedules(item)[0];
    editingTemplateId = item.templateId;
    scheduleMode = 'recurring';
    manageProgramId = String(item.programId);
    template = { programId: String(item.programId), name: item.name, dayOfWeek: firstSchedule?.dayOfWeek ?? item.dayOfWeek, startTime: firstSchedule?.startTime ?? item.startTime, durationMinutes: firstSchedule?.durationMinutes ?? item.durationMinutes, beltScope: item.beltScope ? item.beltScope.split(',').map((scope) => scope.trim()).filter((scope) => beltScopeOptions.includes(scope)) : [], classType: item.classType, appointmentOnly: Boolean(item.appointmentOnly) };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  function addClass() {
    editingTemplateId = null;
    scheduleMode = 'recurring';
    template = { ...blankTemplate(), programId: manageProgramId || String(programs[0]?.programId ?? '') };
    document.getElementById('template-studio')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  function addOneTimeSession() {
    editingTemplateId = null;
    scheduleMode = 'one-time';
    if (!oneTimeTemplateId && templates[0]) oneTimeTemplateId = String(templates[0].templateId);
    document.getElementById('template-studio')?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }

  function cancelEdit() {
    editingTemplateId = null;
    const programId = manageProgramId || (programs[0] ? String(programs[0].programId) : '');
    template = { ...blankTemplate(), programId };
  }

  async function retireTemplate(item: Template) {
    if (!confirm(`Retire “${item.name}”? Past attendance will remain, but future occurrences will stop appearing.`)) return;
    error = '';
    try {
      await api(`/api/portal-admin/class-templates/${item.templateId}`, { method: 'DELETE' });
      notice = `${item.name} retired. Historical records were preserved.`;
      if (editingTemplateId === item.templateId) cancelEdit();
      await load();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not retire the class template.';
    }
  }

  function selectCancellationProgram(event: Event) {
    cancellationProgramId = (event.currentTarget as HTMLSelectElement).value;
    const first = firstTemplateFor(cancellationProgramId);
    cancellationTemplateId = first ? String(first.templateId) : '';
  }

  async function cancelInstance() {
    if (!cancellationTemplateId) return;
    const item = cancellationTemplates.find((templateItem) => String(templateItem.templateId) === String(cancellationTemplateId));
    if (!item) return;
    const reason = prompt(`Why is ${item.name} cancelled on ${new Date(`${cancellationDate}T12:00:00`).toLocaleDateString()}?`, 'Studio closed');
    if (reason === null || !reason.trim()) return;
    error = '';
    cancellingInstance = true;
    try {
      await api(`/api/portal-admin/class-templates/${item.templateId}/cancel-instance`, { method: 'POST', body: JSON.stringify({ sessionDateUtc: new Date(`${cancellationDate}T00:00:00Z`).toISOString(), reason: reason.trim() }) });
      notice = `${item.name} cancelled for ${new Date(`${cancellationDate}T12:00:00`).toLocaleDateString()}.`;
      await refreshCancellationSessions();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not cancel that class instance.';
    } finally {
      cancellingInstance = false;
    }
  }

  async function restoreInstance() {
    if (!selectedCancellationSession) return;
    try {
      await api(`/api/portal-admin/sessions/${selectedCancellationSession.sessionId}/cancellation`, { method: 'POST', body: JSON.stringify({ cancelled: false, reason: '' }) });
      notice = 'Class instance restored.';
      await refreshCancellationSessions();
    } catch (e) {
      error = e instanceof Error ? e.message : 'Could not restore that class instance.';
    }
  }

  function classSchedules(item: Template): ScheduleItem[] {
    return item.schedules?.length ? item.schedules : [{ scheduleId: 0, dayOfWeek: item.dayOfWeek, startTime: item.startTime, durationMinutes: item.durationMinutes }];
  }

  function scheduleLabel(item: Template) {
    return classSchedules(item).map((schedule) => `${schedule.dayOfWeek} · ${schedule.startTime}`).join('  /  ');
  }

  function scopeLabel(scope?: string | null) { return scope?.trim() || 'All ranks'; }
  function typeDescription(type: string) { return classTypes.find((item) => item.id === type)?.description ?? 'Scheduled dojo activity.'; }
  function entriesForDay(program: Program, day: string) {
    return program.templates.flatMap((item) => classSchedules(item).filter((schedule) => schedule.dayOfWeek === day).map((schedule) => ({ item, schedule })));
  }

  $: managedProgram = programs.find((program) => String(program.programId) === String(manageProgramId));
  $: managedTemplates = managedProgram?.templates ?? [];
  $: cancellationProgram = programs.find((program) => String(program.programId) === String(cancellationProgramId));
  $: cancellationTemplates = cancellationProgram?.templates ?? [];
  $: selectedCancellationSession = sessions.find((item) => String(item.classTemplateId) === String(cancellationTemplateId));
  $: selectedTypeDescription = typeDescription(template.classType);
  $: oneTimeClass = templates.find((item) => String(item.templateId) === String(oneTimeTemplateId));
  $: oneTimeNeedsStudent = oneTimeClass?.classType === 'Private lesson';
  $: databaseEntryCount = programs.reduce((total, program) => total + program.templates.reduce((count, item) => count + classSchedules(item).length, 0), 0);

  onMount(load);
</script>

<svelte:head><title>Staff · Programs & Classes | Task Karate</title></svelte:head>

<div class="toolbar"><div><span class="eyebrow">PROGRAMS & CLASSES</span><h2>Programs and class scheduling</h2><p class="muted">Recurring classes are reusable templates. One-time sessions are dated additions. Both are written to the portal database.</p></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}

<section class="program-catalog" aria-labelledby="program-catalog-heading">
  <div class="section-heading"><div><span class="eyebrow">FIXED CATALOG</span><h3 id="program-catalog-heading">The three programs</h3></div><span class="pill">{programs.length} programs</span></div>
  <div class="program-catalog-grid">{#each programs as program}<article class="program-catalog-card"><span class="program-mark">{program.name === 'IS3' ? 'I' : program.name === 'Kids' ? 'K' : 'T'}</span><div><strong>{program.name}</strong><p>{program.name === 'IS3' ? 'IS3 level progression and Filipino martial arts training.' : program.name === 'Kids' ? 'Age-appropriate karate instruction for kids.' : 'Karate training for teens and adults.'}</p></div><span class="pill">{program.templates.length} classes</span></article>{/each}</div>
</section>

<section class="card template-studio" id="template-studio">
  <div class="section-heading"><div><span class="eyebrow">{editingTemplateId ? 'EDIT RECURRING CLASS' : scheduleMode === 'recurring' ? 'ADD RECURRING CLASS' : 'ADD ONE-TIME SESSION'}</span><h3>{editingTemplateId ? 'Edit recurring class' : scheduleMode === 'recurring' ? 'Add a recurring class' : 'Schedule one class session'}</h3><p class="muted">{editingTemplateId ? 'Changes update the reusable class definition and its weekly schedule.' : scheduleMode === 'recurring' ? 'This creates a reusable weekly class template and immediately creates its upcoming occurrences.' : 'This adds one dated session from an existing class. It does not create or change a recurring template.'}</p></div><span class="template-stamp">DB</span></div>
  {#if !editingTemplateId}<div class="schedule-mode-switch" aria-label="Choose what to add"><button type="button" class:active={scheduleMode === 'recurring'} on:click={() => scheduleMode = 'recurring'}><strong>Recurring class</strong><small>Weekly template + upcoming occurrences</small></button><button type="button" class:active={scheduleMode === 'one-time'} on:click={() => scheduleMode = 'one-time'}><strong>One-time session</strong><small>One date only; no recurring change</small></button></div>{/if}
  {#if scheduleMode === 'recurring'}
    <form on:submit|preventDefault={saveTemplate} class="form-grid">
      <label class="full">Program<select bind:value={template.programId} required><option value="" disabled>Select program</option>{#each programs as program}<option value={program.programId}>{program.name}</option>{/each}</select></label>
      <label class="full">Class name<input bind:value={template.name} required placeholder="e.g. Green & Purple class or private lesson" /></label>
      <fieldset class="full class-type-fieldset"><legend>What kind of activity is this?</legend><div class="class-type-options">{#each classTypes as type}<button type="button" class:active={template.classType === type.id} class={`class-type-option type-${type.id.toLowerCase().replace(' ', '-')}`} aria-pressed={template.classType === type.id} on:click={() => template = { ...template, classType: type.id }}><strong>{type.label}</strong><small>{type.description}</small></button>{/each}</div><p class="muted type-help">{selectedTypeDescription}</p></fieldset>
      <label>Day<select bind:value={template.dayOfWeek}>{#each days as day}<option>{day}</option>{/each}</select></label>
      <label>Start time<input bind:value={template.startTime} required /></label>
      <label>Duration (minutes)<input type="number" min="1" max="480" bind:value={template.durationMinutes} required /></label>
      <label class="compact-belt-scope">Belt scope <small class="muted">optional · empty means all ranks</small><select class="belt-scope-select" multiple size="5" bind:value={template.beltScope} aria-label="Select ranks for this class">{#each beltScopeOptions as belt}<option value={belt}>{belt}</option>{/each}</select><small class="muted">Ctrl/Cmd-click to select multiple.</small></label>
      <label class="check-row appointment-check full"><input type="checkbox" bind:checked={template.appointmentOnly} /><span><strong>Appointment-only scheduling</strong><small>Leave off for normal group classes.</small></span></label>
      <div class="full template-form-actions"><button class="button" type="submit" disabled={savingTemplate || !template.programId || !template.name.trim()}>{savingTemplate ? 'Saving…' : editingTemplateId ? 'Save class changes' : 'Add recurring class'}</button>{#if editingTemplateId}<button class="button secondary" type="button" on:click={cancelEdit}>Cancel edit</button>{/if}</div>
    </form>
  {:else}
    <form on:submit|preventDefault={scheduleOneTime} class="one-time-session-form">
      <label>Existing class<select bind:value={oneTimeTemplateId} required><option value="" disabled>Select a class template</option>{#each programs as program}<optgroup label={program.name}>{#each program.templates as item}<option value={item.templateId}>{item.name} · {scheduleLabel(item)}</option>{/each}</optgroup>{/each}</select></label>
      <label>Date<input type="date" bind:value={oneTimeDate} required /></label>
      {#if oneTimeNeedsStudent}<label>Student for private lesson<select bind:value={oneTimeAssignedStudentId} required><option value="" disabled>Select student</option>{#each students as student}<option value={student.studentId}>{student.displayName}{student.rankName ? ` · ${student.rankName}` : ''}</option>{/each}</select></label>{/if}
      <div class="one-time-session-note"><strong>What will happen</strong><span>One dated session will be added to the schedule and attendance system. The class template stays unchanged.</span></div>
      <div class="template-form-actions"><button class="button" type="submit" disabled={schedulingOneTime || !oneTimeTemplateId || !oneTimeDate || (oneTimeNeedsStudent && !oneTimeAssignedStudentId)}>{schedulingOneTime ? 'Scheduling…' : 'Add one-time session'}</button></div>
    </form>
  {/if}
</section>

<section class="card schedule-management"><div class="section-heading"><div><span class="eyebrow">SCHEDULE MANAGEMENT</span><h3>Classes by program</h3><p class="muted">Choose a program to see the reusable classes stored for it. Each card shows every recurring day.</p></div><div class="section-heading-actions"><button class="button secondary" type="button" on:click={addOneTimeSession}>＋ One-time session</button><button class="button" type="button" on:click={addClass}>＋ Recurring class</button></div></div><label class="program-filter-label">Program<select bind:value={manageProgramId}><option value="" disabled>Select program</option>{#each programs as program}<option value={program.programId}>{program.name}</option>{/each}</select></label>{#if managedProgram}<div class="managed-class-list">{#each managedTemplates as item}<article class={`managed-class-card template-${item.classType.toLowerCase().replace(' ', '-')}`}><div class="managed-class-main"><div class="managed-class-title"><span class="type-pill">{item.classType}</span><strong>{item.name}</strong></div><span>{scheduleLabel(item)} · {classSchedules(item)[0]?.durationMinutes ?? item.durationMinutes} minutes</span><small>{scopeLabel(item.beltScope)}{item.appointmentOnly ? ' · Appointment only' : ''}</small></div><div class="managed-class-actions"><button class="button secondary small-button" type="button" on:click={() => editTemplate(item)}>Edit</button><button class="button danger small-button" type="button" on:click={() => retireTemplate(item)}>Retire</button></div></article>{:else}<p class="empty-state">No active classes are defined for {managedProgram.name}. Use “＋ Recurring class” to create the first one.</p>{/each}</div>{/if}</section>

<section class="card instance-cancellations"><div class="section-heading"><div><span class="eyebrow">ONE-DAY EXCEPTIONS</span><h3>Cancel one class instance</h3><p class="muted">Use this for holidays, studio closures, or another date-specific exception. It does not retire the class.</p></div><span class="exception-mark">×</span></div><div class="instance-cancellation-form"><label>Date<input type="date" bind:value={cancellationDate} on:change={refreshCancellationSessions} /></label><label>Program<select value={cancellationProgramId} on:change={selectCancellationProgram}><option value="" disabled>Select program</option>{#each programs as program}<option value={program.programId}>{program.name}</option>{/each}</select></label><label class="wide">Class<select bind:value={cancellationTemplateId}><option value="" disabled>Select class</option>{#each cancellationTemplates as item}<option value={item.templateId}>{item.classType} · {item.name}</option>{/each}</select></label></div>{#if cancellationTemplateId}<div class="instance-status"><div><strong>{selectedCancellationSession ? selectedCancellationSession.name : 'No generated instance found yet'}</strong><span>{new Date(`${cancellationDate}T12:00:00`).toLocaleDateString()} · {selectedCancellationSession ? `${selectedCancellationSession.startTime} · ${selectedCancellationSession.durationMinutes} minutes` : 'Cancelling will create a recorded exception for this date.'}</span></div>{#if selectedCancellationSession?.isCancelled}<button class="button secondary" type="button" on:click={restoreInstance}>Restore this instance</button>{:else}<button class="button danger" type="button" disabled={cancellingInstance} on:click={cancelInstance}>{cancellingInstance ? 'Cancelling…' : 'Cancel this class instance'}</button>{/if}</div>{#if selectedCancellationSession?.isCancelled && selectedCancellationSession.cancellationReason}<p class="cancellation-reason">Reason: {selectedCancellationSession.cancellationReason}</p>{/if}{/if}</section>

<section class="card template-library"><div class="section-heading"><div><span class="eyebrow">DATABASE VIEW</span><h3>Schedule by day and program</h3><p class="muted">This is the recurring schedule currently stored in the database. One-time sessions are visible on the dated schedule and attendance pages.</p></div><span class="pill">{databaseEntryCount} schedule entries</span></div><div class="database-day-list">{#each days as day}<section class="database-day-group"><div class="database-day-divider"><span>{day}</span></div>{#each programs as program}{@const entries = entriesForDay(program, day)}{#if entries.length}<div class="database-program-group"><div class="database-program-heading"><strong>{program.name}</strong><span>{entries.length} {entries.length === 1 ? 'class' : 'classes'}</span></div>{#each entries as entry}<article class={`template-card template-${entry.item.classType.toLowerCase().replace(' ', '-')}`}><div><strong>{entry.item.name}</strong><span>{entry.schedule.startTime} · {entry.schedule.durationMinutes} min</span></div><div class="template-card-meta"><span class="type-pill">{entry.item.classType}</span><span>{scopeLabel(entry.item.beltScope)}</span></div></article>{/each}</div>{/if}{/each}</section>{:else}<p class="muted">No recurring classes are stored.</p>{/each}</div></section>
