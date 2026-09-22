<script lang="ts">
  import { onMount } from 'svelte';
  import { page } from '$app/stores';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';

  let session: StudentSession | null = null;
  let item: any = null;
  let error = '';
  onMount(async () => {
    session = await requireStudent();
    if (!session) return;
    try { item = await api(`/api/student/news/${$page.params.postId}`); } catch (e) { error = apiError(e); }
  });
</script>

<svelte:head><title>Task Karate | News</title></svelte:head>
{#if session}<StudentShell {session} active="news"><section class="student-heading"><a class="back-link student-back-link" href="/student/news">← Back to news feed</a><span class="student-eyebrow">OFFICIAL DOJO UPDATE</span><h1>{item?.title ?? 'NEWS UPDATE'}</h1><p>News and announcements from Task Karate.</p></section>{#if error}<div class="error" role="alert">{error}</div>{:else if !item}<div class="student-panel loading-panel">Loading news…</div>{:else}<article class="student-panel detail-panel news-detail"><div class="feed-author"><span class="mini-avatar">TK</span><div><strong>Task Karate</strong><small>Official dojo update · {new Date(item.publishedAt).toLocaleDateString(undefined, { weekday: 'long', month: 'long', day: 'numeric', year: 'numeric' })}</small></div></div><h2>{item.title}</h2><p class="detail-description">{item.body}</p><div class="feed-actions"><span>▤ Published dojo news</span></div></article>{/if}</StudentShell>{/if}
