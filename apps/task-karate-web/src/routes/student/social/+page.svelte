<script lang="ts">
  import { onMount } from 'svelte';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';

  let session: StudentSession | null = null;
  let feed: any[] = [];
  let friends: any[] = [];
  let results: any[] = [];
  let selected: any = null;
  let messages: any[] = [];
  let query = '';
  let text = '';
  let postText = '';
  let error = '';
  let loading = true;
  let publishing = false;
  const reactionOptions = [{ code: 'fist_bump', label: 'Fist bump', icon: '👊' }, { code: 'respect', label: 'Respect', icon: '🙌' }, { code: 'fire', label: 'On fire', icon: '⚡' }];
  let expandedComments: Record<number, boolean> = {};
  let comments: Record<number, any[]> = {};
  let commentDrafts: Record<number, string> = {};

  async function loadFriends() { friends = await api<any[]>('/api/student/friends'); }
  async function loadFeed() { feed = await api<any[]>('/api/student/feed'); }
  onMount(async () => { session = await requireStudent(); if (!session) return; try { await Promise.all([loadFriends(), loadFeed()]); if (new URL(window.location.href).searchParams.get('focus') === 'messages') { const firstFriend = friends.find((friend) => friend.status === 'accepted'); if (firstFriend) await selectFriend(firstFriend); } } catch (e) { error = apiError(e); } finally { loading = false; } });
  async function search() { try { results = await api<any[]>(`/api/student/social/search?q=${encodeURIComponent(query)}`); } catch (e) { error = apiError(e); } }
  async function request(id: number) { try { await api(`/api/student/friends/${id}/request`, { method: 'POST' }); results = results.filter((item) => item.studentId !== id); } catch (e) { error = apiError(e); } }
  async function respond(friend: any, accept: boolean) { try { await api(`/api/student/friends/${friend.studentId}/respond`, { method: 'POST', body: JSON.stringify({ accept }) }); await loadFriends(); } catch (e) { error = apiError(e); } }
  async function selectFriend(friend: any) { selected = friend; try { messages = await api<any[]>(`/api/student/messages/${friend.studentId}`); } catch (e) { error = apiError(e); } }
  async function send() { if (!selected || !text.trim()) return; try { await api('/api/student/messages', { method: 'POST', body: JSON.stringify({ recipientId: selected.studentId, message: text }) }); text = ''; await selectFriend(selected); } catch (e) { error = apiError(e); } }
  async function publish() { if (!postText.trim()) return; publishing = true; error = ''; try { await api('/api/student/feed', { method: 'POST', body: JSON.stringify({ text: postText }) }); postText = ''; await loadFeed(); } catch (e) { error = apiError(e); } finally { publishing = false; } }
  async function react(postId: number, code: string) { try { await api(`/api/student/feed/${postId}/reaction`, { method: 'POST', body: JSON.stringify({ code }) }); await loadFeed(); } catch (e) { error = apiError(e); } }
  async function toggleComments(postId: number) { expandedComments = { ...expandedComments, [postId]: !expandedComments[postId] }; if (!comments[postId]) { try { comments = { ...comments, [postId]: await api<any[]>(`/api/student/feed/${postId}/comments`) }; } catch (e) { error = apiError(e); } } }
  async function submitComment(postId: number) { const textValue = commentDrafts[postId]?.trim(); if (!textValue) return; try { await api(`/api/student/feed/${postId}/comments`, { method: 'POST', body: JSON.stringify({ text: textValue }) }); commentDrafts = { ...commentDrafts, [postId]: '' }; comments = { ...comments, [postId]: await api<any[]>(`/api/student/feed/${postId}/comments`) }; expandedComments = { ...expandedComments, [postId]: true }; } catch (e) { error = apiError(e); } }
  function when(value: string) { return new Date(value).toLocaleString(undefined, { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' }); }
</script>

{#if session}
  <StudentShell {session} active="social">
    <section class="student-heading"><span class="student-eyebrow">DOJO CONNECTIONS</span><h1>SOCIAL</h1><p>Share training moments with your accepted dojo connections and keep private conversations direct.</p></section>
    {#if error}<div class="error" role="alert">{error}</div>{/if}
    <div class="social-feed-layout">
      <section class="feed-column">
        <section class="student-panel post-composer"><div class="panel-title"><span>SHARE WITH THE DOJO</span><span class="muted">Text posts only</span></div><form on:submit|preventDefault={publish}><textarea bind:value={postText} maxlength="2000" placeholder="What are you working on this week?" aria-label="Create a dojo post"></textarea><div class="composer-footer"><span class="muted">Keep it respectful and training-focused.</span><button class="primary-button" disabled={publishing}>{publishing ? 'Posting…' : 'Post update'}</button></div></form></section>
        <section class="feed-stream" aria-label="Dojo feed">
          {#if loading}<div class="student-panel loading-panel">Loading the dojo feed…</div>{:else if feed.length === 0}<div class="student-panel empty-panel"><span class="rank-orb">✦</span><h2>Your dojo feed starts here.</h2><p>Post a training note or connect with another student to see more updates.</p></div>{:else}{#each feed as item}<article class="student-panel feed-card"><div class="feed-author"><span class="mini-avatar">{item.authorName?.slice(0, 2).toUpperCase()}</span><div><strong>{item.authorName}</strong><small>{item.postType === 'news' ? 'Task Karate update' : (item.rankName ?? 'Student')} · {when(item.createdAt)}</small></div></div><p>{item.text}</p><div class="feed-actions"><div class="reaction-row" aria-label={`Reactions for ${item.authorName}'s update`}>{#each reactionOptions as option}{@const existing = item.reactions?.find((reaction: any) => reaction.code === option.code)}<button class:selected={existing?.selected} class="reaction-button" type="button" title={option.label} aria-label={`${option.label}${existing?.selected ? ', selected' : ''}`} on:click={() => react(item.postId, option.code)}><span class="reaction-icon">{option.icon}</span><span>{option.label}</span><b>{existing?.count ?? 0}</b></button>{/each}</div><button class="comment-toggle" type="button" on:click={() => toggleComments(item.postId)}>{expandedComments[item.postId] ? 'Hide comments' : 'Comment'}</button></div>{#if expandedComments[item.postId]}<div class="comment-area"><div class="comment-list">{#each comments[item.postId] ?? [] as comment}<div class="comment-item"><span class="mini-avatar">{comment.authorName?.slice(0, 2).toUpperCase()}</span><div><strong>{comment.authorName}</strong><p>{comment.text}</p><small>{when(comment.createdAt)}</small></div></div>{:else}<p class="muted">No comments yet. Be the first to encourage this training update.</p>{/each}</div><form class="comment-compose" on:submit|preventDefault={() => submitComment(item.postId)}><input bind:value={commentDrafts[item.postId]} maxlength="1000" placeholder="Add an encouraging comment…" aria-label={`Comment on ${item.authorName}'s update`} /><button class="small-action" type="submit">Send</button></form></div>{/if}</article>{/each}{/if}
        </section>
      </section>
      <aside class="social-sidebar">
        <section class="student-panel friends-panel"><div class="panel-title"><span>FRIENDS & REQUESTS</span></div><form class="search-row" on:submit|preventDefault={search}><input bind:value={query} placeholder="Find a student…" aria-label="Find a student" /><button class="outline-button">Search</button></form>{#if results.length}<div class="search-results">{#each results as item}<div class="person-row"><span class="mini-avatar">{item.displayName?.slice(0,2).toUpperCase()}</span><span><strong>{item.displayName}</strong><small>{item.rankName ?? 'Student'}</small></span><button class="small-action" on:click={() => request(item.studentId)}>Add friend</button></div>{/each}</div>{/if}<div class="friend-list">{#if loading}<p class="muted">Loading…</p>{:else if friends.length === 0}<p class="muted">No connections yet. Search the dojo to send a request.</p>{:else}{#each friends as friend}<div class="person-row" class:selected={selected?.studentId === friend.studentId}><button class="person-select" on:click={() => selectFriend(friend)}><span class="mini-avatar">{friend.displayName?.slice(0,2).toUpperCase()}</span><span><strong>{friend.displayName}</strong><small>{friend.rankName ?? 'Student'} · {friend.status}</small></span></button>{#if friend.status === 'pending' && friend.incoming}<span class="request-actions"><button class="small-action" on:click={() => respond(friend, true)}>Accept</button><button class="text-link" on:click={() => respond(friend, false)}>Decline</button></span>{/if}</div>{/each}{/if}</div></section>
        <section class="student-panel message-panel">{#if selected}<div class="panel-title"><span>MESSAGES WITH {selected.displayName?.toUpperCase()}</span></div><div class="message-thread">{#each messages as message}<div class:mine={message.senderId === session.studentId} class="message-bubble"><p>{message.messageText}</p><small>{when(message.createdAt)}</small></div>{/each}{#if messages.length === 0}<p class="muted">Start a respectful dojo conversation.</p>{/if}</div><form class="message-compose" on:submit|preventDefault={send}><textarea bind:value={text} maxlength="2000" placeholder="Write a message…" aria-label="Message"></textarea><button class="primary-button">Send message</button></form>{:else}<div class="message-empty"><span class="rank-orb">✦</span><h2>Select a friend</h2><p>Direct messages stay behind authenticated API access and are available only to accepted friends.</p></div>{/if}</section>
      </aside>
    </div>
  </StudentShell>
{/if}
