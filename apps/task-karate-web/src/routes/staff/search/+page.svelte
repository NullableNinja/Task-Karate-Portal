<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { api } from '$lib/api';
  type Result = { type: string; id: number; title: string; subtitle: string };
  let results: Result[] = []; let loading = true; let error = '';
  $: query = $page.url.searchParams.get('q') ?? '';
  function href(result: Result) { if (result.type === 'student') return `/staff/students/${result.id}`; if (result.type === 'guardian') return '/staff/guardians'; if (result.type === 'program') return '/staff/classes'; return '/staff/social'; }
  onMount(async () => { if (query.length < 2) { loading = false; return; } try { results = await api<Result[]>(`/api/portal-admin/search?q=${encodeURIComponent(query)}`); } catch (e) { error = e instanceof Error ? e.message : 'Could not search portal records.'; } finally { loading = false; } });
</script>
<svelte:head><title>Staff · Search | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">GLOBAL SEARCH</span><h2>Search the dojo workspace</h2><p class="muted">Search canonical portal records without guessing which section owns them.</p></div></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}
{#if loading}<section class="card"><p class="muted">Searching the portal database…</p></section>{:else if query.length < 2}<section class="card empty-state"><strong>Enter at least two characters.</strong><p class="muted">Search students, family contacts, programs, and community posts.</p></section>{:else}<section class="card"><div class="section-heading"><div><span class="eyebrow">RESULTS FOR</span><h3>“{query}”</h3></div><span class="pill">{results.length}</span></div>{#if results.length}<div class="admin-search-results">{#each results as result}<a href={href(result)}><span class="admin-search-type">{result.type}</span><span><strong>{result.title}</strong><small>{result.subtitle}</small></span><span>→</span></a>{/each}</div>{:else}<div class="empty-state"><strong>No matching records.</strong><p class="muted">Try a name, email, phone number, program, or phrase from a community post.</p></div>{/if}</section>{/if}
