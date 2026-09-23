<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';

  type Post = { postId: number; authorId: number; authorName: string; text: string; postType: string; moderationStatus: string; visible: boolean; createdAt: string; updatedAt: string; commentCount: number };
  type Comment = { commentId: number; postId: number; authorId: number; authorName: string; text: string; visible: boolean; moderationStatus: string; createdAt: string };
  let posts: Post[] = [];
  let comments: Record<number, Comment[]> = {};
  let drafts: Record<number, string> = {};
  let expanded = new Set<number>();
  let filter = 'All';
  let search = '';
  let loading = true;
  let busyId = 0;
  let error = '';
  let notice = '';

  async function load() { loading = true; error = ''; try { posts = await api<Post[]>('/api/portal-admin/social/feed'); drafts = Object.fromEntries(posts.map((post) => [post.postId, post.text])); } catch (e) { error = e instanceof Error ? e.message : 'Could not load the social feed.'; } finally { loading = false; } }
  async function save(post: Post) { busyId = post.postId; error = ''; notice = ''; try { await api(`/api/portal-admin/social/posts/${post.postId}`, { method: 'PUT', body: JSON.stringify({ text: drafts[post.postId] ?? post.text }) }); notice = 'Post updated.'; await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not update the post.'; } finally { busyId = 0; } }
  async function setVisibility(post: Post, active: boolean) { busyId = post.postId; error = ''; notice = ''; try { await api(`/api/portal-admin/social/posts/${post.postId}/visibility`, { method: 'POST', body: JSON.stringify({ active }) }); notice = active ? 'Post restored to the feed.' : 'Post removed from the student feed.'; await load(); } catch (e) { error = e instanceof Error ? e.message : 'Could not change post visibility.'; } finally { busyId = 0; } }
  async function toggleComments(postId: number) { const next = new Set(expanded); if (next.has(postId)) next.delete(postId); else { next.add(postId); if (!comments[postId]) comments[postId] = await api<Comment[]>(`/api/portal-admin/social/posts/${postId}/comments`); } expanded = next; }
  async function setCommentVisibility(comment: Comment, active: boolean) { busyId = comment.commentId; try { await api(`/api/portal-admin/social/comments/${comment.commentId}/visibility`, { method: 'POST', body: JSON.stringify({ active }) }); comments[comment.postId] = await api<Comment[]>(`/api/portal-admin/social/posts/${comment.postId}/comments`); } catch (e) { error = e instanceof Error ? e.message : 'Could not moderate the comment.'; } finally { busyId = 0; } }
  $: visiblePosts = posts.filter((post) => (filter === 'All' || (filter === 'Visible' ? post.visible : !post.visible)) && `${post.authorName} ${post.text}`.toLowerCase().includes(search.trim().toLowerCase()));
  onMount(load);
</script>

<svelte:head><title>Staff · Social moderation | Task Karate</title></svelte:head>
<div class="toolbar"><div><span class="eyebrow">COMMUNITY MODERATION</span><h2>Social feed</h2><p class="muted">Review every student post and comment, correct a typo, or remove content from the student hub. Removing content is reversible and audited.</p></div><span class="pill">{posts.length} posts</span></div>
{#if error}<div class="error" role="alert">{error}</div>{/if}{#if notice}<div class="status" role="status">{notice}</div>{/if}
<section class="card"><div class="staff-list-controls"><label>Show<select bind:value={filter}><option>All</option><option>Visible</option><option>Removed</option></select></label><label class="workflow-grow">Search posts<input class="staff-list-search" bind:value={search} type="search" placeholder="Student name or post text" /></label><button class="button secondary small-button" type="button" on:click={load}>Refresh feed</button></div></section>
{#if loading}<section class="card"><p class="muted">Loading all social posts…</p></section>{:else}<div class="staff-social-list">{#each visiblePosts as post}<article class:removed={!post.visible} class="card staff-social-card"><div class="toolbar"><div><span class="eyebrow">{post.postType === 'news' ? 'DOJO NEWS' : 'STUDENT POST'}</span><h3>{post.authorName}</h3><small class="muted">{new Date(post.createdAt).toLocaleString()} · {post.commentCount} comment{post.commentCount === 1 ? '' : 's'}</small></div><span class="pill">{post.visible ? 'Visible' : 'Removed'}</span></div><label class="full">Post text<textarea bind:value={drafts[post.postId]} maxlength="2000" rows="4"></textarea></label><div class="table-actions"><button class="button small-button" type="button" disabled={busyId === post.postId} on:click={() => save(post)}>Save edit</button><button class="button secondary small-button" type="button" disabled={busyId === post.postId} on:click={() => setVisibility(post, !post.visible)}>{post.visible ? 'Remove from feed' : 'Restore to feed'}</button><button class="button secondary small-button" type="button" on:click={() => toggleComments(post.postId)}>{expanded.has(post.postId) ? 'Hide comments' : `Review comments (${post.commentCount})`}</button></div>{#if expanded.has(post.postId)}<div class="staff-comments"><h4>Comments</h4>{#each comments[post.postId] ?? [] as comment}<div class:removed={!comment.visible} class="staff-comment"><div><strong>{comment.authorName}</strong><small class="muted"> · {new Date(comment.createdAt).toLocaleString()}</small><p>{comment.text}</p></div><button class="button secondary small-button" type="button" disabled={busyId === comment.commentId} on:click={() => setCommentVisibility(comment, !comment.visible)}>{comment.visible ? 'Remove' : 'Restore'}</button></div>{:else}<p class="muted">No comments on this post.</p>{/each}</div>{/if}</article>{:else}<section class="card"><p class="muted">No social posts match this view.</p></section>{/each}</div>{/if}
