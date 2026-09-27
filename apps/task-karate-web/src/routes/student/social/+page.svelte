<script lang="ts">
  import { onMount, tick } from 'svelte';
  import { marked } from 'marked';
  import { api } from '$lib/api';
  import StudentShell from '$lib/components/StudentShell.svelte';
  import EmojiPicker from '$lib/components/EmojiPicker.svelte';
  import { rankColor } from '$lib/rank-colors';
  import { apiError, requireStudent, type StudentSession } from '$lib/student-session';

  type FeedItem = any;
  type Friend = any;
  type GifResult = { id: string; title: string; url: string; preview: string; source?: string; width?: number; height?: number };
  type GifResponse = { configured: boolean; items: GifResult[]; hasMore?: boolean; nextPage?: number | null };

  let session: StudentSession | null = null;
  let profile: any = null;
  let dojoCheckIn: any = null;
  let leaderboard: any = null;
  let leaderboardPeriod: 'month' | 'year' = 'month';
  let leaderboardLoading = false;
  let feed: FeedItem[] = [];
  let feedHasMore = false;
  let feedLoadingMore = false;
  let feedOffset = 0;
  let friends: Friend[] = [];
  let results: Friend[] = [];
  let selected: Friend | null = null;
  let messages: any[] = [];
  let bookmarks: number[] = [];
  let comments: Record<number, any[]> = {};
  let commentDrafts: Record<number, string> = {};
  let commentElements: Record<number, HTMLInputElement> = {};
  let commentLimits: Record<number, number> = {};

  let loading = true;
  let publishing = false;
  let error = '';
  let query = '';
  let searchMessage = '';
  let feedSearch = '';
  let feedMode = 'dojo';
  let topic = 'all';
  let timeRange = 'all';
  let sort = 'recent';
  let postText = '';
  let postKind = 'training';
  let linkUrl = '';
  let imageData = '';
  let imageAlt = '';
  let imageError = '';
  let showComposerTools = false;
  let composerElement: HTMLTextAreaElement;
  let messageElement: HTMLTextAreaElement | undefined;

  let showBackToTop = false;
  let messageText = '';
  let messageView: 'all' | 'pinned' = 'all';
  let messageMenu: { messageId: number; x: number; y: number } | null = null;
  let reportedMessageIds: Record<number, boolean> = {};
  let ownerMenuPostId: number | null = null;
  let editingPostId: number | null = null;
  let editText = '';
  let gifPicker: { kind: 'post' | 'comment' | 'message'; postId?: number } | null = null;
  let gifQuery = '';
  let gifResults: GifResult[] = [];
  let gifLoading = false;
  let gifConfigured = true;
  let gifError = '';
  let gifMode: 'trending' | 'search' = 'trending';
  let gifNextPage: number | null = null;
  let stateReady = false;
  let stateKey = '';
  let restoredScroll = 0;
  let checkInBusy = false;
  let checkInNotice = '';
  let checkInTab: 'checkin' | 'leaderboard' = 'checkin';
  let gifClicks = 0;
  let stickerDrawer = false;

  function insertEmoji(emoji: string, target: 'post' | 'comment' | 'message', postId?: number) {
    const element = target === 'post' ? composerElement : target === 'message' ? messageElement : (postId ? commentElements[postId] : undefined);
    const current = target === 'post' ? postText : target === 'message' ? messageText : (postId ? commentDrafts[postId] ?? '' : '');
    const start = element?.selectionStart ?? current.length;
    const end = element?.selectionEnd ?? current.length;
    const next = `${current.slice(0, start)}${emoji}${current.slice(end)}`;
    const cursor = start + emoji.length;
    if (target === 'post') postText = next;
    else if (target === 'message') messageText = next;
    else if (postId) commentDrafts = { ...commentDrafts, [postId]: next };
    tick().then(() => { element?.focus(); element?.setSelectionRange(cursor, cursor); });
  }

  let gifLastClick = 0;

  const postFormats = [
    { id: 'training', label: 'Training update', icon: '🥋', description: 'Share what you are practicing.' },
    { id: 'win', label: 'Milestone or win', icon: '🏆', description: 'Celebrate progress, big or small.' },
    { id: 'question', label: 'Ask the dojo', icon: '💬', description: 'Invite thoughtful help.' },
    { id: 'encouragement', label: 'Encouragement', icon: '🤝', description: 'Lift someone in your circle.' }
  ];
  const topics = [
    { id: 'all', label: 'Everything' },
    { id: 'training', label: 'Training updates' },
    { id: 'win', label: 'Milestones & wins' },
    { id: 'question', label: 'Questions' },
    { id: 'encouragement', label: 'Encouragement' },
    { id: 'announcement', label: 'Studio news' }
  ];
  const gifSuggestions = ['Karate', 'Celebrate', 'High five', 'Victory'];
  const reactions = [
    { code: 'fist_bump', label: 'Fist bump', icon: '👊' },
    { code: 'respect', label: 'Nice work', icon: '🌟' },
    { code: 'fire', label: 'On fire', icon: '🔥' }
  ];
  const attendanceMilestones = [
    { target: 1, label: 'First studio check-in' },
    { target: 5, label: '5 studio check-ins' },
    { target: 25, label: '25 studio check-ins' },
    { target: 100, label: '100 studio check-ins' }
  ];
  async function loadFeed(reset = true) {
    const offset = reset ? 0 : feedOffset;
    const response = await api<{ items: FeedItem[]; hasMore: boolean; nextOffset: number } | FeedItem[]>(`/api/student/feed?limit=25&offset=${offset}`);
    const page = Array.isArray(response) ? { items: response, hasMore: false, nextOffset: offset + response.length } : response;
    const loadedItems = page.items ?? [];
    if (reset) {
      feed = loadedItems;
      comments = {};
      commentLimits = Object.fromEntries(loadedItems.map((item) => [item.postId, 4]));
    } else {
      feed = [...feed, ...loadedItems];
      commentLimits = { ...commentLimits, ...Object.fromEntries(loadedItems.map((item) => [item.postId, 4])) };
    }
    const loadedComments = await Promise.all(loadedItems.map(async (item) => {
      try { return [item.postId, await api<any[]>(`/api/student/feed/${item.postId}/comments`)] as const; }
      catch { return [item.postId, []] as const; }
    }));
    comments = { ...comments, ...Object.fromEntries(loadedComments) };
    feedOffset = page.nextOffset ?? offset + loadedItems.length;
    feedHasMore = page.hasMore;
  }
  async function loadMoreFeed() {
    if (feedLoadingMore || !feedHasMore) return;
    feedLoadingMore = true;
    try { await loadFeed(false); }
    catch (e) { error = apiError(e); }
    finally { feedLoadingMore = false; }
  }
  async function loadFriends() { friends = await api<Friend[]>('/api/student/friends'); }
  async function loadBookmarks() { bookmarks = await api<number[]>('/api/student/bookmarks'); }

  onMount(() => {
    const onScroll = () => {
      showBackToTop = window.scrollY > 520;
      if (stateReady && stateKey) sessionStorage.setItem(stateKey, JSON.stringify({ feedMode, topic, timeRange, sort, feedSearch, scrollY: window.scrollY }));
      if (!loading && feedHasMore && !feedLoadingMore && window.scrollY + window.innerHeight >= document.documentElement.scrollHeight - 560) void loadMoreFeed();
    };
    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  });

  onMount(async () => {
    session = await requireStudent();
    if (!session) return;
    stateKey = `task-karate-social-${session.studentId}`;
    try {
      const saved = JSON.parse(sessionStorage.getItem(stateKey) ?? '{}');
      feedMode = saved.feedMode ?? 'dojo'; topic = saved.topic ?? 'all'; timeRange = saved.timeRange ?? 'all'; sort = saved.sort ?? 'recent'; feedSearch = saved.feedSearch ?? '';
      restoredScroll = Number(saved.scrollY ?? 0);
    } catch { restoredScroll = 0; }
    try {
      const [loadedProfile, loadedDojoCheckIn, loadedLeaderboard] = await Promise.all([api<any>('/api/student/profile'), api<any>('/api/student/dojo-check-in'), api<any>('/api/student/dojo-check-in/leaderboard?period=month'), loadFeed(), loadFriends(), loadBookmarks()]);
      profile = loadedProfile;
      dojoCheckIn = loadedDojoCheckIn;
      leaderboard = loadedLeaderboard;
    } catch (e) { error = apiError(e); }
    finally { loading = false; stateReady = true; await tick(); if (restoredScroll > 0) { window.scrollTo({ top: restoredScroll, behavior: 'auto' }); showBackToTop = restoredScroll > 520; } }
  });

  async function searchStudents() {
    searchMessage = '';
    if (query.trim().length < 2) { results = []; searchMessage = 'Enter at least two characters to search the dojo.'; return; }
    try {
      results = await api<Friend[]>(`/api/student/social/search?q=${encodeURIComponent(query)}`);
      if (!results.length) searchMessage = 'No matching student profiles found.';
    } catch (e) { error = apiError(e); }
  }

  async function pressHiyah() {
    if (checkInBusy || dojoCheckIn?.checkedInToday) return;
    checkInBusy = true;
    checkInNotice = '';
    try {
      dojoCheckIn = await api<any>('/api/student/dojo-check-in', { method: 'POST' });
      await loadLeaderboard();
    } catch (e) {
      checkInNotice = apiError(e);
      try { dojoCheckIn = await api<any>('/api/student/dojo-check-in'); } catch { /* Keep the original check-in message. */ }
    } finally {
      checkInBusy = false;
      window.setTimeout(() => checkInNotice = '', 4000);
    }
  }

  async function loadLeaderboard() {
    leaderboardLoading = true;
    try { leaderboard = await api<any>(`/api/student/dojo-check-in/leaderboard?period=${leaderboardPeriod}`); }
    catch (e) { error = apiError(e); }
    finally { leaderboardLoading = false; }
  }

  async function requestFriend(id: number) {
    try { await api(`/api/student/friends/${id}/request`, { method: 'POST' }); results = results.filter((item) => item.studentId !== id); await loadFriends(); searchMessage = 'Request sent. It will appear in your circle after acceptance.'; }
    catch (e) { error = apiError(e); }
  }

  async function respondFriend(friend: Friend, accept: boolean) {
    try { await api(`/api/student/friends/${friend.studentId}/respond`, { method: 'POST', body: JSON.stringify({ accept }) }); await loadFriends(); }
    catch (e) { error = apiError(e); }
  }

  async function selectFriend(friend: Friend) {
    selected = friend;
    messageMenu = null;
    messageView = 'all';
    try { messages = await api<any[]>(`/api/student/messages/${friend.studentId}`); window.dispatchEvent(new CustomEvent('task-karate:messages-read')); }
    catch (e) { error = apiError(e); }
  }

  async function sendMessage() {
    if (!selected || !messageText.trim()) return;
    try { await api('/api/student/messages', { method: 'POST', body: JSON.stringify({ recipientId: selected.studentId, message: messageText.trim() }) }); messageText = ''; await selectFriend(selected); }
    catch (e) { error = apiError(e); }
  }

  function openGifPicker(target: { kind: 'post' | 'comment' | 'message'; postId?: number }) {
    const now = Date.now();
    if (now - gifLastClick > 2200) gifClicks = 0;
    gifLastClick = now;
    gifClicks += 1;
    if (gifClicks >= 3) { gifClicks = 0; stickerDrawer = true; window.dispatchEvent(new CustomEvent('task-karate:easter-egg', { detail: { id: 'egg-sticker-sensei' } })); }
    gifPicker = gifPicker?.kind === target.kind && gifPicker?.postId === target.postId ? null : target;
    gifError = '';
    if (gifPicker && !gifResults.length) void loadTrendingGifs();
  }

  async function loadTrendingGifs() {
    gifLoading = true; gifError = ''; gifMode = 'trending'; gifQuery = '';
    try {
      const response = await api<GifResponse>('/api/student/gifs/trending?page=1');
      gifConfigured = response.configured;
      gifResults = response.items ?? [];
      gifNextPage = response.nextPage ?? null;
      if (!gifResults.length && gifConfigured) gifError = 'Trending GIFs are empty right now. Try a search.';
    } catch (e) { gifError = apiError(e); }
    finally { gifLoading = false; }
  }

  async function searchGifs(queryOverride?: string) {
    const query = (queryOverride ?? gifQuery).trim();
    if (!query) return loadTrendingGifs();
    gifQuery = query; gifMode = 'search';
    gifLoading = true; gifError = '';
    try {
      const response = await api<GifResponse>(`/api/student/gifs/search?q=${encodeURIComponent(query)}&page=1`);
      gifConfigured = response.configured;
      gifResults = response.items ?? [];
      gifNextPage = response.nextPage ?? null;
      if (!gifResults.length && gifConfigured) gifError = 'No GIFs found. Try a shorter search.';
    } catch (e) {
      const message = apiError(e);
      gifConfigured = message === 'Not Found' ? false : gifConfigured;
      gifError = message === 'Not Found' ? 'GIF search is temporarily unavailable. Try again in a moment.' : message;
    }
    finally { gifLoading = false; }
  }

  async function loadMoreGifs() {
    if (!gifNextPage || gifLoading) return;
    gifLoading = true; gifError = '';
    try {
      const endpoint = gifMode === 'search' && gifQuery.trim()
        ? `/api/student/gifs/search?q=${encodeURIComponent(gifQuery.trim())}&page=${gifNextPage}`
        : `/api/student/gifs/trending?page=${gifNextPage}`;
      const response = await api<GifResponse>(endpoint);
      const known = new Set(gifResults.map((gif) => gif.id));
      gifResults = [...gifResults, ...(response.items ?? []).filter((gif) => !known.has(gif.id))];
      gifNextPage = response.nextPage ?? null;
    } catch (e) { gifError = apiError(e); }
    finally { gifLoading = false; }
  }

  function insertGif(gif: GifResult) {
    const markdown = `![${gif.title || 'Dojo GIF'}](${gif.url})`;
    if (gifPicker?.kind === 'post') postText = postText.trim() ? `${postText.trim()}\n\n${markdown}` : markdown;
    if (gifPicker?.kind === 'comment' && gifPicker.postId) commentDrafts = { ...commentDrafts, [gifPicker.postId]: commentDrafts[gifPicker.postId]?.trim() ? `${commentDrafts[gifPicker.postId].trim()}\n\n${markdown}` : markdown };
    if (gifPicker?.kind === 'message') messageText = messageText.trim() ? `${messageText.trim()}\n\n${markdown}` : markdown;
    gifPicker = null;
  }

  function gifPickerLabel() { return 'Search dojo GIFs'; }

  async function reportMessage(message: any) {
    if (reportedMessageIds[message.messageId] || !window.confirm('Report this message to the dojo staff for review?')) return;
    try {
      await api(`/api/student/messages/${message.messageId}/report`, { method: 'POST', body: JSON.stringify({ reason: 'safety' }) });
      reportedMessageIds = { ...reportedMessageIds, [message.messageId]: true };
    } catch (e) { error = apiError(e); }
  }

  async function toggleMessagePin(message: any) {
    try {
      const result = await api<{ pinned: boolean }>(`/api/student/messages/${message.messageId}/pin`, { method: 'POST' });
      if (selected) messages = await api<any[]>(`/api/student/messages/${selected.studentId}`);
      else messages = messages.map((item) => item.messageId === message.messageId ? { ...item, isPinned: result.pinned } : item);
    } catch (e) { error = apiError(e); }
    finally { messageMenu = null; }
  }

  function openMessageMenu(event: MouseEvent, message: any) {
    event.preventDefault();
    messageMenu = { messageId: message.messageId, x: Math.min(event.clientX, window.innerWidth - 190), y: Math.min(event.clientY, window.innerHeight - 70) };
  }

  async function publish() {
    if (!postText.trim()) return;
    if (imageData && !imageAlt.trim()) { error = 'Add a short description for the image so everyone can enjoy the post.'; return; }
    publishing = true; error = '';
    try {
      await api('/api/student/feed', { method: 'POST', body: JSON.stringify({ text: postText.trim(), postKind, linkUrl: linkUrl.trim() || null, imageData: imageData || null, imageAlt: imageAlt.trim() || null }) });
      postText = ''; linkUrl = ''; imageData = ''; imageAlt = ''; imageError = ''; postKind = 'training'; await loadFeed(true);
    } catch (e) { error = apiError(e); }
    finally { publishing = false; }
  }

  async function react(postId: number, code: string) {
    try {
      await api(`/api/student/feed/${postId}/reaction`, { method: 'POST', body: JSON.stringify({ code }) });
      const response = await api<{ items: FeedItem[] } | FeedItem[]>('/api/student/feed?limit=25&offset=0');
      const refreshed = Array.isArray(response) ? response.find((item) => item.postId === postId) : response.items?.find((item) => item.postId === postId);
      if (refreshed) feed = feed.map((item) => item.postId === postId ? refreshed : item);
    }
    catch (e) { error = apiError(e); }
  }

  function toggleOwnerMenu(postId: number) { ownerMenuPostId = ownerMenuPostId === postId ? null : postId; }
  function beginEdit(item: FeedItem) { ownerMenuPostId = null; editingPostId = item.postId; editText = item.text; tick().then(() => document.getElementById(`edit-post-${item.postId}`)?.focus()); }
  function cancelEdit() { editingPostId = null; editText = ''; }
  async function saveEdit(item: FeedItem) {
    if (!editText.trim()) return;
    try {
      await api(`/api/student/feed/${item.postId}`, { method: 'PATCH', body: JSON.stringify({ text: editText.trim() }) });
      feed = feed.map((post) => post.postId === item.postId ? { ...post, text: editText.trim() } : post);
      cancelEdit();
    } catch (e) { error = apiError(e); }
  }
  async function deletePost(item: FeedItem) {
    if (!window.confirm('Delete this post? This also removes its comments and reactions.')) return;
    try {
      await api(`/api/student/feed/${item.postId}`, { method: 'DELETE' });
      await loadFeed(true);
      ownerMenuPostId = null;
      cancelEdit();
    } catch (e) { error = apiError(e); }
  }

  async function reportPost(item: FeedItem) {
    if (item.authorId === session?.studentId || item.postType === 'news') return;
    if (!window.confirm('Report this post to the dojo staff for review?')) return;
    try {
      await api(`/api/student/feed/${item.postId}/report`, { method: 'POST', body: JSON.stringify({ reason: 'safety' }) });
      ownerMenuPostId = null;
      error = 'Thanks — the dojo staff has been alerted to review this post.';
    } catch (e) { error = apiError(e); }
  }

  async function submitComment(postId: number) {
    const value = commentDrafts[postId]?.trim();
    if (!value) return;
    try {
      await api(`/api/student/feed/${postId}/comments`, { method: 'POST', body: JSON.stringify({ text: value }) });
      commentDrafts = { ...commentDrafts, [postId]: '' };
      comments = { ...comments, [postId]: await api<any[]>(`/api/student/feed/${postId}/comments`) };
      feed = feed.map((item) => item.postId === postId ? { ...item, commentCount: (item.commentCount ?? 0) + 1 } : item);
      commentLimits = { ...commentLimits, [postId]: Math.max(commentLimits[postId] ?? 4, comments[postId].length) };
    } catch (e) { error = apiError(e); }
  }

  async function toggleBookmark(postId: number) {
    try { await api(`/api/student/feed/${postId}/bookmark`, { method: 'POST' }); await loadBookmarks(); }
    catch (e) { error = apiError(e); }
  }

  function handleImage(event: Event) {
    const input = event.currentTarget as HTMLInputElement;
    const file = input.files?.[0]; imageError = '';
    if (!file) return;
    if (!['image/png', 'image/jpeg', 'image/gif', 'image/webp'].includes(file.type)) { imageError = 'Choose a PNG, JPEG, GIF, or WebP image.'; input.value = ''; return; }
    if (file.size > 800000) { imageError = 'Images must be smaller than 800 KB.'; input.value = ''; return; }
    const reader = new FileReader(); reader.onload = () => { imageData = typeof reader.result === 'string' ? reader.result : ''; }; reader.readAsDataURL(file);
  }

  function applyMarkup(before: string, after = before) {
    if (!composerElement) return;
    const start = composerElement.selectionStart; const end = composerElement.selectionEnd; const selection = postText.slice(start, end) || 'your text';
    postText = `${postText.slice(0, start)}${before}${selection}${after}${postText.slice(end)}`;
    tick().then(() => { composerElement.focus(); const cursor = start + before.length + selection.length + after.length; composerElement.setSelectionRange(cursor, cursor); });
  }

  function applyLinePrefix(prefix: string) {
    if (!composerElement) return;
    const start = composerElement.selectionStart; const lineStart = postText.lastIndexOf('\n', Math.max(0, start - 1)) + 1;
    postText = `${postText.slice(0, lineStart)}${prefix}${postText.slice(lineStart)}`;
    tick().then(() => { composerElement.focus(); composerElement.setSelectionRange(start + prefix.length, start + prefix.length); });
  }

  function insertLink() {
    if (!composerElement) return;
    const start = composerElement.selectionStart; const end = composerElement.selectionEnd; const selection = postText.slice(start, end) || 'link text';
    postText = `${postText.slice(0, start)}[${selection}](${linkUrl.trim() || 'https://'})${postText.slice(end)}`;
    tick().then(() => composerElement.focus());
  }

  function normalizeMarkdown(value: string) {
    const lines = value.replace(/\r\n?/g, '\n').split('\n');
    const isDivider = (line: string) => /^\s*\|?\s*:?-{1,}:?\s*(?:\|\s*:?-{1,}:?\s*)+\|?\s*$/.test(line);
    const normalizeRow = (line: string) => {
      const trimmed = line.trim().replace(/^\|/, '').replace(/\|$/, '');
      return `| ${trimmed.split('|').map((cell) => cell.trim()).join(' | ')} |`;
    };
    const normalizeDivider = (line: string) => {
      const cells = line.trim().replace(/^\|/, '').replace(/\|$/, '').split('|');
      return `| ${cells.map((cell) => { const clean = cell.trim(); return `${clean.startsWith(':') ? ':' : ''}---${clean.endsWith(':') ? ':' : ''}`; }).join(' | ')} |`;
    };
    const nextMeaningful = (from: number) => { let cursor = from; while (cursor < lines.length && !lines[cursor].trim()) cursor += 1; return cursor; };
    const collectRows = (from: number) => {
      const rows: string[] = [];
      let cursor = from;
      while (cursor < lines.length) {
        cursor = nextMeaningful(cursor);
        if (cursor >= lines.length || !lines[cursor].includes('|')) break;
        rows.push(normalizeRow(lines[cursor]));
        cursor += 1;
      }
      return { rows, cursor };
    };
    const normalized: string[] = [];
    let index = 0;
    while (index < lines.length) {
      const dividerAfter = lines[index].includes('|') ? nextMeaningful(index + 1) : -1;
      if (lines[index].includes('|') && dividerAfter < lines.length && isDivider(lines[dividerAfter])) {
        const collected = collectRows(dividerAfter + 1);
        normalized.push(normalizeRow(lines[index]), normalizeDivider(lines[dividerAfter]), ...collected.rows);
        index = collected.cursor;
        continue;
      }
      if (isDivider(lines[index])) {
        const header = nextMeaningful(index + 1);
        if (header < lines.length && lines[header].includes('|')) {
          const collected = collectRows(header + 1);
          normalized.push(normalizeRow(lines[header]), normalizeDivider(lines[index]), ...collected.rows);
          index = collected.cursor;
          continue;
        }
      }
      normalized.push(lines[index]);
      index += 1;
    }
    return normalized.join('\n');
  }

  function safeMarkdown(value: string) {
    const escaped = value.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#039;');
    const normalized = normalizeMarkdown(escaped);
    return marked.parse(normalized.replace(/(!?\[[^\]]*\]\()([^\s)]+)([^)]*\))/g, (_match, prefix, url, suffix) => `${prefix}${/^(https?:|mailto:)/i.test(url) ? url : '#'}${suffix}`), { gfm: true, breaks: true });
  }

  function kindFor(item: FeedItem) { return item.postKind ?? (item.postType === 'news' ? 'announcement' : 'training'); }
  function kindLabel(kind: string) { return topics.find((item) => item.id === kind)?.label ?? 'Dojo update'; }
  function kindIcon(kind: string) { return postFormats.find((item) => item.id === kind)?.icon ?? (kind === 'announcement' ? '📣' : '✦'); }
  function composerPlaceholder(kind: string) {
    return kind === 'win' ? 'What small win are you celebrating?' : kind === 'question' ? 'What would you like the dojo to help you think through?' : kind === 'encouragement' ? 'Who would you like to encourage, and why?' : 'What are you working on this week?';
  }
  function relativeDate(value: string) { const date = new Date(value); const seconds = Math.max(0, Math.floor((Date.now() - date.getTime()) / 1000)); if (seconds < 60) return 'just now'; if (seconds < 3600) return `${Math.floor(seconds / 60)}m`; if (seconds < 86400) return `${Math.floor(seconds / 3600)}h`; if (seconds < 604800) return `${Math.floor(seconds / 86400)}d`; return date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' }); }
  function withinTime(value: string) { if (timeRange === 'all') return true; const age = Date.now() - new Date(value).getTime(); const limit = timeRange === 'today' ? 86400000 : timeRange === 'week' ? 604800000 : 2592000000; return age <= limit; }
  function initials(name: string | undefined) { return (name || 'TK').split(' ').map((part) => part[0]).join('').slice(0, 2).toUpperCase(); }
  function scrollToTop() { window.scrollTo({ top: 0, behavior: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth' }); }
  function resetFeed() { feedMode = 'dojo'; topic = 'all'; timeRange = 'all'; sort = 'recent'; feedSearch = ''; }

  $: acceptedFriends = friends.filter((friend) => friend.status === 'accepted');
  $: totalCheckIns = dojoCheckIn?.totalCheckIns ?? 0;
  $: nextAttendanceMilestone = attendanceMilestones.find((milestone) => totalCheckIns < milestone.target) ?? null;
  $: checkInsToMilestone = nextAttendanceMilestone ? Math.max(0, nextAttendanceMilestone.target - totalCheckIns) : 0;
  $: lastCheckInDate = dojoCheckIn?.lastCheckInDate ?? '';
  $: newUpdatesSinceCheckIn = lastCheckInDate
    ? feed.filter((item) => new Date(item.createdAt).getTime() >= new Date(`${lastCheckInDate}T00:00:00`).getTime()).length
    : 0;
  $: visibleFeed = [...feed].filter((item) => {
    const kind = kindFor(item); const text = `${item.authorName} ${item.text} ${item.linkUrl ?? ''}`.toLowerCase();
    const matchesMode = feedMode === 'dojo' || (feedMode === 'circle' ? item.postType === 'student' : feedMode === 'news' ? item.postType === 'news' || kind === 'announcement' : bookmarks.includes(item.postId));
    const matchesTopic = topic === 'all' || kind === topic; const matchesTime = withinTime(item.createdAt); const matchesSearch = !feedSearch.trim() || text.includes(feedSearch.trim().toLowerCase());
    return matchesMode && matchesTopic && matchesTime && matchesSearch;
  }).sort((a, b) => sort === 'popular' ? ((b.commentCount ?? 0) + (b.reactions ?? []).reduce((sum: number, item: any) => sum + (item.count ?? 0), 0)) - ((a.commentCount ?? 0) + (a.reactions ?? []).reduce((sum: number, item: any) => sum + (item.count ?? 0), 0)) : new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
  $: visibleMessages = messageView === 'pinned' ? messages.filter((message) => message.isPinned) : messages;
  $: contextMessage = messageMenu ? messages.find((message) => message.messageId === messageMenu?.messageId) : null;
  $: if (stateReady && stateKey) sessionStorage.setItem(stateKey, JSON.stringify({ feedMode, topic, timeRange, sort, feedSearch, scrollY: window.scrollY }));
</script>

{#if session}
  <StudentShell {session} active="social">
    <div class="social-page">
      <header class="social-hero">
        <div>
          <span class="social-kicker">THE DOJO COMMUNITY</span>
          <h1>Social</h1>
          <p>Share progress, find your people, and keep the dojo moving together.</p>
        </div>
        <aside class:leaderboard-view={checkInTab === 'leaderboard'} class="social-hero-checkin" aria-label="Dojo check-in and leaderboard">
          <div class="social-checkin-tabs" role="tablist" aria-label="Studio activity">
            <button class:active={checkInTab === 'checkin'} type="button" role="tab" aria-selected={checkInTab === 'checkin'} on:click={() => checkInTab = 'checkin'}>HIYAH! CHECK-IN</button>
            <button class:active={checkInTab === 'leaderboard'} type="button" role="tab" aria-selected={checkInTab === 'leaderboard'} on:click={() => checkInTab = 'leaderboard'}>LEADERBOARD</button>
          </div>
          {#if checkInTab === 'checkin'}
          <div class="social-checkin-stat-tiles">
            <div><span>THIS MONTH</span><strong>{dojoCheckIn?.checkInsThisMonth ?? 0}</strong><small>studio check-in{(dojoCheckIn?.checkInsThisMonth ?? 0) === 1 ? '' : 's'}</small></div>
            <div><span>SINCE LAST HIYAH</span><strong>{newUpdatesSinceCheckIn}</strong><small>new dojo update{newUpdatesSinceCheckIn === 1 ? '' : 's'}</small></div>
            <div><span>IN STUDIO TODAY</span><strong>{dojoCheckIn?.checkedInTodayCount ?? 0}</strong><small>student{(dojoCheckIn?.checkedInTodayCount ?? 0) === 1 ? '' : 's'} checked in</small></div>
          </div>
          <div class="social-checkin-goal" aria-live="polite"><span>NEXT ACHIEVEMENT</span>{#if nextAttendanceMilestone}<strong>{checkInsToMilestone} more check-in{checkInsToMilestone === 1 ? '' : 's'} to {nextAttendanceMilestone.label}</strong>{:else}<strong>All studio check-in goals complete</strong>{/if}</div>
          <button class="social-hiyah social-hiyah-hero" type="button" on:click={pressHiyah} disabled={checkInBusy || dojoCheckIn?.checkedInToday} aria-describedby="social-checkin-note">{dojoCheckIn?.checkedInToday ? 'HIYAH! · CHECKED IN' : checkInBusy ? 'CHECKING IN…' : 'HIYAH!'}<span aria-hidden="true">{dojoCheckIn?.checkedInToday ? '✓' : '→'}</span></button>
          <span class="visually-hidden" id="social-checkin-note">One studio check-in per calendar day.</span>
          {#if checkInNotice}<small class="social-checkin-notice" role="status">{checkInNotice}</small>{/if}
          {:else}
          <div class="social-leaderboard-heading"><div><span class="social-kicker">THE DOJO RACE</span><strong>HIYAH! LEADERBOARD</strong></div></div>
          <div class="social-leaderboard-tabs" role="tablist" aria-label="Check-in leaderboard period"><button class:active={leaderboardPeriod === 'month'} type="button" role="tab" aria-selected={leaderboardPeriod === 'month'} on:click={() => { leaderboardPeriod = 'month'; loadLeaderboard(); }}>Month</button><button class:active={leaderboardPeriod === 'year'} type="button" role="tab" aria-selected={leaderboardPeriod === 'year'} on:click={() => { leaderboardPeriod = 'year'; loadLeaderboard(); }}>Year</button></div>
          {#if leaderboardLoading}<p class="social-muted">Loading the dojo race…</p>{:else if leaderboard?.entries?.length}<div class="social-leaderboard-list">{#each leaderboard.entries.slice(0, 8) as entry}<div class:current={entry.isCurrentStudent} class="social-leaderboard-row"><span class="social-leaderboard-rank">{entry.rank}</span><span class="social-leaderboard-name"><strong>{entry.displayName}</strong><small>{entry.checkIns} {entry.checkIns === 1 ? 'check-in' : 'check-ins'}</small></span>{#if entry.rank <= 3}<span class="social-leaderboard-medal" aria-label={`Rank ${entry.rank}`}>{entry.rank === 1 ? '🥇' : entry.rank === 2 ? '🥈' : '🥉'}</span>{/if}</div>{/each}</div>{:else}<p class="social-muted social-leaderboard-empty">No scores yet — press HIYAH! to start the race.</p>{/if}
          {/if}
        </aside>
      </header>

      <div class="social-summary-line" aria-label="Community overview"><strong>{feed.length}</strong> dojo updates <span>·</span><strong>{acceptedFriends.length}</strong> in your circle <span>·</span><strong>{bookmarks.length}</strong> saved <em>Progress over perfection.</em></div>

      <section class="social-feed-toolbar" aria-label="Feed controls">
        <div class="social-feed-modes"><span class="social-toolbar-label">VIEW</span><button class:active={feedMode === 'dojo'} aria-pressed={feedMode === 'dojo'} type="button" on:click={() => feedMode = 'dojo'}>Dojo wall</button><button class:active={feedMode === 'news'} aria-pressed={feedMode === 'news'} type="button" on:click={() => { feedMode = 'news'; topic = 'all'; }}>Dojo news</button><button class:active={feedMode === 'circle'} aria-pressed={feedMode === 'circle'} type="button" on:click={() => feedMode = 'circle'}>My circle</button><button class:active={feedMode === 'saved'} aria-pressed={feedMode === 'saved'} type="button" on:click={() => feedMode = 'saved'}>Saved</button><span class="social-result-count">{visibleFeed.length} {visibleFeed.length === 1 ? 'post' : 'posts'}</span></div>
        <div class="social-filter-row"><label>Topic<select bind:value={topic}>{#each topics as item}<option value={item.id}>{item.label}</option>{/each}</select></label><label>When<select bind:value={timeRange}><option value="all">Any time</option><option value="today">Today</option><option value="week">This week</option><option value="month">This month</option></select></label><label>Sort<select bind:value={sort}><option value="recent">Most recent</option><option value="popular">Most loved</option></select></label><label class="social-search">Search<input bind:value={feedSearch} placeholder="Search updates, names, and links…" aria-label="Search the dojo feed" /></label></div>
        {#if feedMode !== 'dojo' || topic !== 'all' || timeRange !== 'all' || sort !== 'recent' || feedSearch}<div class="social-filter-summary"><span>Filtered view{feedSearch ? ` · “${feedSearch}”` : ''}</span><button class="social-text-button" type="button" on:click={resetFeed}>Reset feed</button></div>{/if}
      </section>

      {#if error}<div class="social-alert" role="alert">{error}</div>{/if}

      <div class="social-workspace">
        <main class="social-feed-column">
          <section class="social-card social-composer">
            <div class="social-composer-heading"><div class="social-composer-person"><a class="social-avatar" href={`/student/profile/${session.studentId}`} aria-label={`Open ${session.displayName}'s public profile`}>{initials(session.displayName)}</a><div><a class="social-author-link social-composer-name" href={`/student/profile/${session.studentId}`}>{session.displayName}</a><span>Share with the dojo</span></div></div><span class="social-counter">{postText.length}/2000</span></div>
            <form on:submit|preventDefault={publish}>
              <div class="social-post-types" aria-label="Choose a post type">{#each postFormats as format}<button class:active={postKind === format.id} type="button" on:click={() => postKind = format.id}><span>{format.icon}</span><strong>{format.label}</strong><small>{format.description}</small></button>{/each}</div><p class="social-type-note"><strong>{postFormats.find((format) => format.id === postKind)?.label}</strong> is a label for scanning and filtering. Every type uses the same dojo visibility, reactions, comments, and safety rules.</p>
              <textarea bind:this={composerElement} bind:value={postText} maxlength="2000" placeholder={composerPlaceholder(postKind)} aria-label="Create a dojo post"></textarea>
              <div class="social-composer-actions social-compose-toolbar">
                <div class="social-composer-tool-group">
                  <EmojiPicker label="Add emoji to post" on:select={(event) => insertEmoji('' + event.detail, 'post')} />
                  <button class="social-tool-toggle" type="button" aria-expanded={showComposerTools} on:click={() => showComposerTools = !showComposerTools}><span class="tool-toggle-long">{showComposerTools ? 'Hide formatting & media' : 'Add formatting & media'}</span><span class="tool-toggle-short">{showComposerTools ? 'Hide tools' : 'Tools'}</span></button>
                  <button class="social-gif-trigger" type="button" on:click={() => openGifPicker({ kind: 'post' })} aria-expanded={gifPicker?.kind === 'post'} title="Search and add a GIF">GIF</button>
                </div>
                <button class="social-primary" type="submit" disabled={publishing || !postText.trim()}>{publishing ? 'Publishing…' : 'Share update'}</button>
              </div>
              {#if showComposerTools}<div class="social-composer-tools"><div class="social-markdown-tools" aria-label="Markdown formatting tools"><button type="button" on:click={() => applyMarkup('**', '**')} title="Bold selected text"><b>B</b></button><button type="button" on:click={() => applyMarkup('_', '_')} title="Italicize selected text"><i>I</i></button><button type="button" on:click={() => applyLinePrefix('## ')} title="Add heading">H</button><button type="button" on:click={() => applyLinePrefix('- ')} title="Add list">☷</button><button type="button" on:click={() => applyLinePrefix('> ')} title="Add quote">❯</button><button type="button" on:click={() => applyMarkup('```\n', '\n```')} title="Add code block">&lt;/&gt;</button><button type="button" on:click={insertLink} title="Add link">↗</button><span>Shortcuts for common formatting.</span></div><div class="social-attachment-row"><label class="social-file-button">Add photo or GIF<input type="file" accept="image/png,image/jpeg,image/gif,image/webp" on:change={handleImage} /></label><input bind:value={linkUrl} type="url" maxlength="1000" placeholder="https:// add a link (optional)" aria-label="Optional link" /></div>{#if imageError}<small class="social-form-error">{imageError}</small>{/if}{#if imageData}<div class="social-image-preview"><img src={imageData} alt={imageAlt || 'Preview of the image to share'} /><div><strong>Photo ready</strong><input bind:value={imageAlt} maxlength="160" placeholder="Describe the photo for screen readers" aria-label="Photo description" /><button class="social-text-button" type="button" on:click={() => { imageData = ''; imageAlt = ''; }}>Remove</button></div></div>{/if}</div>{/if}
            </form>
            {#if gifPicker?.kind === 'post'}<div class="social-gif-picker"><div class="social-gif-search"><input bind:value={gifQuery} on:keydown={(event) => event.key === 'Enter' && searchGifs()} placeholder="Search GIFs…" aria-label="Search GIFs" /><button class="social-secondary" type="button" on:click={() => searchGifs()} disabled={gifLoading}>{gifLoading ? 'Searching…' : 'Search'}</button></div>{#if !gifConfigured}<small class="social-form-error">{gifPickerLabel()}. GIF search is provided by a public dojo GIF service.</small>{:else if gifError}<small class="social-form-error">{gifError}</small>{/if}<div class="social-gif-grid">{#each gifResults as gif}<button type="button" on:click={() => insertGif(gif)} title={`Insert ${gif.title}`}><img src={gif.preview || gif.url} alt={gif.title || 'GIF result'} /></button>{/each}</div></div>{/if}
            {#if stickerDrawer}<div class="dojo-sticker-drawer" role="dialog" aria-label="Hidden dojo sticker drawer"><div class="social-card-heading"><span>HIDDEN DOJO STICKERS</span><button class="social-text-button" type="button" on:click={() => stickerDrawer = false}>Close</button></div><p class="social-muted">You found the tiny sticker shelf. Choose one to add it to your post.</p><div class="dojo-sticker-grid">{#each ['🥋 Nice work!', '⚡ Keep the rhythm', '★ Small win', '👊 HIYAH!', '🌀 Quiet focus', '🧠 Train with intention'] as sticker}<button type="button" on:click={() => { postText = `${postText}${postText ? ' ' : ''}${sticker}`; stickerDrawer = false; }}>{sticker}</button>{/each}</div></div>{/if}
          </section>

          <div class="social-feed-heading"><div><span class="social-card-title social-wall-heading">{feedMode === 'circle' ? 'MY CIRCLE' : feedMode === 'news' ? 'DOJO NEWS' : feedMode === 'saved' ? 'SAVED MOMENTS' : 'DOJO WALL'}</span><p>{feedMode === 'news' ? 'Studio announcements and news from Task Karate.' : 'Useful updates from students, instructors, and your dojo community.'}</p></div><span>{visibleFeed.length} {visibleFeed.length === 1 ? 'update' : 'updates'}</span></div>
          <div class="social-stream" role="feed" aria-busy={loading} aria-label="Dojo wall posts">
            {#if loading}<div class="social-card social-state"><div class="social-skeleton social-skeleton-short"></div><div class="social-skeleton"></div><div class="social-skeleton social-skeleton-long"></div></div>{:else if visibleFeed.length === 0}<div class="social-card social-state"><span class="social-state-icon">✦</span><h2>{feedMode === 'saved' ? 'Nothing saved yet.' : feedMode === 'news' ? 'No dojo news yet.' : 'No updates match that view.'}</h2><p>Try another filter, search term, or share the first moment in this view.</p><button class="social-secondary" type="button" on:click={() => { feedMode = 'dojo'; topic = 'all'; timeRange = 'all'; feedSearch = ''; }}>Show the dojo wall</button></div>{:else}{#each visibleFeed as item, index}<article class="social-card social-post" aria-posinset={index + 1} aria-setsize={visibleFeed.length}><header class="social-post-header"><span class="social-avatar">{initials(item.authorName)}</span><div class="social-author">{#if !item.authorId}<strong>{item.authorName}</strong>{:else}<a class="social-author-link" href={`/student/profile/${item.authorId}`}>{item.authorName}</a>{/if}<span class="social-author-meta">{#if item.postType === 'news'}Task Karate{:else}<span class="belt-swatch" style={`--belt-color:${rankColor(item.rankName)}`} title={`${item.rankName ?? 'Student'} belt`} aria-label={`${item.rankName ?? 'Student'} belt`}></span>{/if} · {relativeDate(item.createdAt)}</span></div><span class="social-post-kind">{kindIcon(kindFor(item))} {kindLabel(kindFor(item))}</span><div class="social-post-owner-menu"><button class="social-owner-menu-trigger" type="button" aria-label="More post options" aria-expanded={ownerMenuPostId === item.postId} on:click={() => toggleOwnerMenu(item.postId)}>⋯</button>{#if ownerMenuPostId === item.postId}<div class="social-owner-menu-panel">{#if item.authorId === session.studentId}<button class="social-text-button" type="button" on:click={() => beginEdit(item)}>Edit post</button><button class="social-danger-button" type="button" on:click={() => deletePost(item)}>Delete post</button>{/if}<button class="social-text-button" type="button" on:click={() => { toggleBookmark(item.postId); ownerMenuPostId = null; }}>{bookmarks.includes(item.postId) ? 'Remove from saved' : 'Save post'}</button><button class="social-text-button" type="button" on:click={() => { feedMode = 'dojo'; feedSearch = item.authorName; ownerMenuPostId = null; }}>See more from {item.authorName?.split(' ')[0]}</button><button class="social-text-button" type="button" on:click={() => reportPost(item)}>Report post</button></div>{/if}</div></header>{#if editingPostId === item.postId}<form class="social-post-edit" on:submit|preventDefault={() => saveEdit(item)}><textarea id={`edit-post-${item.postId}`} bind:value={editText} maxlength="2000" aria-label="Edit post text"></textarea><div><button class="social-secondary" type="button" on:click={cancelEdit}>Cancel</button><button class="social-primary" type="submit" disabled={!editText.trim()}>Save changes</button></div></form>{:else}<div class="social-post-body social-markdown-content">{@html safeMarkdown(item.text)}</div>{/if}{#if item.imageData}<img class="social-post-image" src={item.imageData} alt={item.imageAlt || 'Shared dojo photo'} />{/if}{#if item.linkUrl}<a class="social-link-preview" href={item.linkUrl} target="_blank" rel="noopener noreferrer"><span>↗</span><span><strong>Open shared link</strong><small>{item.linkUrl}</small></span></a>{/if}<footer class="social-post-footer"><div class="social-reactions" aria-label={`Reactions for ${item.authorName}'s update`}>{#each reactions as reaction}{@const existing = item.reactions?.find((entry: any) => entry.code === reaction.code)}<button class:selected={existing?.selected} type="button" aria-label={`${reaction.label}${existing?.selected ? ', selected' : ''}`} on:click={() => react(item.postId, reaction.code)}><span>{reaction.icon}</span><small>{existing?.count ?? 0}</small></button>{/each}</div><span class="social-comment-count">{item.commentCount ?? 0} {item.commentCount === 1 ? 'comment' : 'comments'}</span></footer><div class="social-comments"><div>{#each (comments[item.postId] ?? []).slice(0, commentLimits[item.postId] ?? 4) as comment}<div class="social-comment"><span class="social-avatar social-avatar-small">{initials(comment.authorName)}</span><div><strong>{comment.authorName}</strong><div class="social-markdown-content">{@html safeMarkdown(comment.text)}</div><small>{relativeDate(comment.createdAt)}</small></div></div>{:else}<p class="social-muted">No comments yet. Be the first to encourage this update.</p>{/each}</div>{#if (comments[item.postId] ?? []).length > (commentLimits[item.postId] ?? 4)}<button class="social-text-button social-show-more-comments" type="button" on:click={() => commentLimits = { ...commentLimits, [item.postId]: comments[item.postId].length }}>Show more comments</button>{/if}<form class="social-comment-form" on:submit|preventDefault={() => submitComment(item.postId)}><input bind:value={commentDrafts[item.postId]} maxlength="1000" placeholder="Add an encouraging comment…" aria-label={`Comment on ${item.authorName}'s update`} /><button class="social-gif-trigger" type="button" on:click={() => openGifPicker({ kind: 'comment', postId: item.postId })} aria-label="Add a GIF to this comment" aria-expanded={gifPicker?.kind === 'comment' && gifPicker?.postId === item.postId}>GIF</button><button class="social-secondary" type="submit">Send</button></form>{#if gifPicker?.kind === 'comment' && gifPicker?.postId === item.postId}<div class="social-gif-picker social-gif-picker-comment"><div class="social-gif-search"><input bind:value={gifQuery} on:keydown={(event) => event.key === 'Enter' && searchGifs()} placeholder="Search GIFs…" aria-label="Search GIFs" /><button class="social-secondary" type="button" on:click={() => searchGifs()} disabled={gifLoading}>{gifLoading ? 'Searching…' : 'Search'}</button></div>{#if !gifConfigured}<small class="social-form-error">{gifPickerLabel()}. GIF search is temporarily unavailable.</small>{:else if gifError}<small class="social-form-error">{gifError}</small>{/if}<div class="social-gif-grid">{#each gifResults as gif}<button type="button" on:click={() => insertGif(gif)} title={`Insert ${gif.title}`}><img src={gif.preview || gif.url} alt={gif.title || 'GIF result'} /></button>{/each}</div></div>{/if}</div></article>{/each}{/if}
          </div>
          {#if feedHasMore}<div class="social-load-more" aria-live="polite"><span>{feedLoadingMore ? 'Loading more dojo updates…' : 'More updates are ready when you are.'}</span><button class="social-secondary" type="button" on:click={loadMoreFeed} disabled={feedLoadingMore}>{feedLoadingMore ? 'Loading…' : 'Load more'}</button></div>{/if}
        </main>

        <aside class="social-community-rail" aria-label="Community connections">
          <section class="social-card social-circle-card"><div class="social-card-heading"><span>YOUR CIRCLE</span><small>{acceptedFriends.length} connected</small></div><form class="social-person-search" on:submit|preventDefault={searchStudents}><input bind:value={query} placeholder="Find a student…" aria-label="Find a student" /><button class="social-secondary" type="submit">Search</button></form>{#if searchMessage}<p class="social-muted" role="status">{searchMessage}</p>{/if}{#if results.length}<div class="social-search-results">{#each results as person}<div class="social-person"><span class="social-avatar social-avatar-small">{initials(person.displayName)}</span><span><strong>{person.displayName}</strong><small>{person.rankName ?? 'Student'}</small></span><button class="social-text-button" type="button" on:click={() => requestFriend(person.studentId)}>Follow</button></div>{/each}</div>{/if}<div class="social-person-list">{#if loading}<p class="social-muted">Loading your circle…</p>{:else if !friends.length}<p class="social-muted">Search the dojo to find a training partner.</p>{:else}{#each friends as friend}<div class="social-person" class:selected={selected?.studentId === friend.studentId}><button class="social-person-select" type="button" on:click={() => { selected = friend; selectFriend(friend); }}><span class="social-avatar social-avatar-small">{initials(friend.displayName)}</span><span><strong>{friend.displayName}</strong><small>{friend.rankName ?? 'Student'} · {friend.status === 'accepted' ? 'Connected' : friend.status}</small></span></button><a class="social-person-profile-link" href={`/student/profile/${friend.studentId}`}>Profile</a>{#if friend.status === 'pending' && friend.incoming}<span class="social-request-actions"><button class="social-text-button" type="button" on:click={() => respondFriend(friend, true)}>Accept</button><button class="social-text-button" type="button" on:click={() => respondFriend(friend, false)}>Decline</button></span>{/if}</div>{/each}{/if}</div></section>
          <section class="social-card social-discovery-card"><div class="social-card-heading"><span>DISCOVER YOUR DOJO</span><small>TRY ONE</small></div><a href="/student/training"><strong>What are you practicing?</strong><span>Share a technique, drill, or lesson from today.</span></a><a href="/student/achievements"><strong>What are you working toward?</strong><span>Celebrate a milestone and let people cheer you on.</span></a><a href="/student/profile"><strong>Who can you encourage?</strong><span>Open a profile to see another student’s journey.</span></a></section>
          <section class="social-card social-safety-card"><div class="social-card-heading"><span>COMMUNITY PROMISE</span></div><p>Keep posts useful, kind, and safe for every age. You can save, mute, or report anything that does not belong here.</p></section>
        </aside>
      </div>

    {#if showBackToTop}<button class="social-back-to-top" type="button" on:click={scrollToTop} aria-label="Back to top">↑ <span>Top</span></button>{/if}

    </div>
  </StudentShell>
{/if}
