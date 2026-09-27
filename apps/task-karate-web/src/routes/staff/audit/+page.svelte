<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type Audit = { id: string; action: string; entity: string; entityId: string; occurredAtUtc: string; actor: string; metadataJson?: string | null };
  let events: Audit[] = [];
  let loading = true;
  let error = '';
  let search = '';
  let actionFilter = 'All';
  let entityFilter = 'All';
  let sort = 'newest';

  async function load() {
    loading = true;
    error = '';
    try { events = await api<Audit[]>('/api/portal-admin/audit?limit=250'); }
    catch (e) { error = e instanceof Error ? e.message : 'Could not load audit history.'; }
    finally { loading = false; }
  }

  $: actions = [...new Set(events.map((event) => event.action))].sort();
  $: entities = [...new Set(events.map((event) => event.entity))].sort();
  $: visibleEvents = events
    .filter((event) => actionFilter === 'All' || event.action === actionFilter)
    .filter((event) => entityFilter === 'All' || event.entity === entityFilter)
    .filter((event) => {
      const query = search.trim().toLowerCase();
      return !query || `${event.action} ${event.entity} ${event.entityId} ${event.actor} ${event.metadataJson ?? ''}`.toLowerCase().includes(query);
    })
    .sort((left, right) => {
      if (sort === 'oldest') return left.occurredAtUtc.localeCompare(right.occurredAtUtc);
      if (sort === 'action') return left.action.localeCompare(right.action) || right.occurredAtUtc.localeCompare(left.occurredAtUtc);
      if (sort === 'entity') return left.entity.localeCompare(right.entity) || right.occurredAtUtc.localeCompare(left.occurredAtUtc);
      return right.occurredAtUtc.localeCompare(left.occurredAtUtc);
    });

  onMount(load);
</script>

<svelte:head><title>Staff · Audit history | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">ADMINISTRATION</span><h2>Audit history</h2><p class="muted">A chronological record of staff actions across the platform. Sensitive values are not written here.</p></div><button class="button secondary" type="button" on:click={load} disabled={loading}>Refresh</button></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}
{#if loading}<section class="card"><p class="muted">Loading audit history…</p></section>{:else}
  <section class="card">
    <div class="section-heading"><div><span class="eyebrow">RECENT ACTIVITY</span><h3>Staff operations</h3><p class="muted">Filter by action, record type, or any text in the event details.</p></div><span class="pill">{visibleEvents.length} / {events.length}</span></div>
    <div class="audit-filter-bar">
      <label>Find in history<input type="search" bind:value={search} placeholder="Actor, record, action…" /></label>
      <label>Action<select bind:value={actionFilter}><option>All</option>{#each actions as action}<option value={action}>{action}</option>{/each}</select></label>
      <label>Record type<select bind:value={entityFilter}><option>All</option>{#each entities as entity}<option value={entity}>{entity}</option>{/each}</select></label>
      <label>Sort<select bind:value={sort}><option value="newest">Newest first</option><option value="oldest">Oldest first</option><option value="action">Action</option><option value="entity">Record type</option></select></label>
    </div>
    <div class="audit-list">{#each visibleEvents as event}<article class="audit-row"><div class="audit-icon">{event.action.slice(0, 1)}</div><div><strong>{event.action} · {event.entity}</strong><small>{event.actor} · {new Date(event.occurredAtUtc).toLocaleString()} · record {event.entityId}</small>{#if event.metadataJson}<code>{event.metadataJson}</code>{/if}</div></article>{:else}<div class="empty-state"><strong>No matching audit activity.</strong><p class="muted">Try clearing a filter or searching for a different record.</p></div>{/each}</div>
  </section>
{/if}
