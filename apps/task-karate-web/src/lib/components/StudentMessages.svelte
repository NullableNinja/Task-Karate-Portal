<script lang="ts">
  import { api } from '$lib/api';
  import { apiError, type StudentSession } from '$lib/student-session';
  import EmojiPicker from './EmojiPicker.svelte';

  export let session: StudentSession;
  export let open = false;

  type Friend = { studentId: number; displayName: string; rankName?: string | null; status: string; incoming?: boolean };
  type Message = { messageId: number; senderId: number; messageText: string; createdAt: string; isPinned?: boolean };

  let friends: Friend[] = [];
  let selected: Friend | null = null;
  let messages: Message[] = [];
  let messageText = '';
  let messageView: 'all' | 'pinned' = 'all';
  let loading = false;
  let sending = false;
  let error = '';
  let loaded = false;
  let messageElement: HTMLTextAreaElement;

  function initials(name: string) { return name.split(/\s+/).filter(Boolean).slice(0, 2).map((part) => part[0]).join('').toUpperCase(); }
  function close() { open = false; error = ''; }

  async function loadFriends() {
    loading = true;
    error = '';
    try {
      const all = await api<Friend[]>('/api/student/friends');
      friends = all.filter((friend) => friend.status === 'accepted');
      if (!selected || !friends.some((friend) => friend.studentId === selected?.studentId)) selected = friends[0] ?? null;
      if (selected) await selectFriend(selected);
    } catch (e) { error = apiError(e); }
    finally { loading = false; }
  }

  async function selectFriend(friend: Friend) {
    selected = friend;
    messageView = 'all';
    error = '';
    try {
      messages = await api<Message[]>(`/api/student/messages/${friend.studentId}`);
      window.dispatchEvent(new CustomEvent('task-karate:messages-read'));
    } catch (e) { error = apiError(e); }
  }

  async function sendMessage() {
    if (!selected || !messageText.trim() || sending) return;
    sending = true;
    error = '';
    try {
      await api('/api/student/messages', { method: 'POST', body: JSON.stringify({ recipientId: selected.studentId, message: messageText.trim() }) });
      messageText = '';
      await selectFriend(selected);
    } catch (e) { error = apiError(e); }
    finally { sending = false; }
  }

  async function togglePin(message: Message) {
    try {
      await api(`/api/student/messages/${message.messageId}/pin`, { method: 'POST' });
      if (selected) await selectFriend(selected);
    } catch (e) { error = apiError(e); }
  }

  async function report(message: Message) {
    if (!window.confirm('Report this message to dojo staff for review?')) return;
    try { await api(`/api/student/messages/${message.messageId}/report`, { method: 'POST', body: JSON.stringify({ reason: 'safety' }) }); }
    catch (e) { error = apiError(e); }
  }

  function insertEmoji(emoji: string) {
    const element = messageElement;
    const start = element?.selectionStart ?? messageText.length;
    const end = element?.selectionEnd ?? messageText.length;
    messageText = `${messageText.slice(0, start)}${emoji}${messageText.slice(end)}`;
    window.setTimeout(() => { element?.focus(); element?.setSelectionRange(start + emoji.length, start + emoji.length); }, 0);
  }

  $: if (open && !loaded) { loaded = true; void loadFriends(); }
  $: visibleMessages = messageView === 'pinned' ? messages.filter((message) => message.isPinned) : messages;
</script>

{#if open}
  <div class="global-messages-host">
    <div class="social-dialog-backdrop" role="presentation" on:click={close}>
      <dialog open class="social-dialog" aria-labelledby="global-messages-title" tabindex="-1" on:click|stopPropagation={() => undefined}>
        <header class="social-dialog-header">
          <div><span class="social-card-title" id="global-messages-title">DOJO MESSAGES</span><p>Private conversations with accepted dojo connections.</p></div>
          <button class="social-dialog-close" type="button" aria-label="Close messages" on:click={close}>×</button>
        </header>
        {#if error}<p class="global-message-error" role="alert">{error}</p>{/if}
        <div class="social-message-layout">
          <div class="social-message-people"><span class="social-toolbar-label">YOUR CIRCLE</span>{#each friends as friend}<button class:selected={selected?.studentId === friend.studentId} type="button" on:click={() => selectFriend(friend)}><span class="social-avatar social-avatar-small">{initials(friend.displayName)}</span><span><strong>{friend.displayName}</strong><small>{friend.rankName ?? 'Student'}</small></span></button>{:else}{#if loading}<p class="social-muted">Loading your connections…</p>{:else}<p class="social-muted">Accept a connection to start messaging.</p>{/if}{/each}</div>
          <div class="social-conversation">
            {#if selected}
              <header><div><strong>{selected.displayName}</strong><span>{selected.rankName ?? 'Student'}</span></div><div class="social-message-view-tabs" role="tablist" aria-label="Message view"><button class:active={messageView === 'all'} type="button" role="tab" aria-selected={messageView === 'all'} on:click={() => messageView = 'all'}>All</button><button class:active={messageView === 'pinned'} type="button" role="tab" aria-selected={messageView === 'pinned'}>Pinned {messages.filter((message) => message.isPinned).length}</button></div><span class="social-message-count">{visibleMessages.length} {visibleMessages.length === 1 ? 'message' : 'messages'}</span></header>
              <div class="social-message-thread" aria-live="polite">{#each visibleMessages as message}<div class:mine={message.senderId === session.studentId} class:pinned={message.isPinned} class="social-message-bubble"><div class="global-message-actions"><button type="button" on:click={() => togglePin(message)}>{message.isPinned ? 'Unpin' : 'Pin'}</button>{#if message.senderId !== session.studentId}<button type="button" on:click={() => report(message)}>Report</button>{/if}</div><p>{message.messageText}</p><small>{new Date(message.createdAt).toLocaleString()}</small></div>{:else}<p class="social-muted">{messageView === 'pinned' ? 'No pinned messages in this conversation.' : 'Start a respectful dojo conversation.'}</p>{/each}</div>
              <form class="social-message-form" on:submit|preventDefault={sendMessage}><textarea bind:this={messageElement} bind:value={messageText} maxlength="2000" placeholder="Write a message…" aria-label="Message"></textarea><EmojiPicker compact label="Add emoji to message" on:select={(event) => insertEmoji(event.detail)} /><button class="social-primary" type="submit" disabled={!messageText.trim() || sending}>{sending ? 'Sending…' : 'Send'}</button></form>
            {:else}<div class="social-message-empty"><span>✦</span><h2>Choose a connection</h2><p>Pick someone from your circle to open a private dojo conversation.</p></div>{/if}
          </div>
        </div>
      </dialog>
    </div>
  </div>
{/if}

<style>
  .global-messages-host { --social-bg: #081a2f; --social-surface: #102944; --social-surface-strong: #153451; --social-surface-soft: #0c2038; --social-border: rgba(128, 190, 233, .32); --social-border-strong: rgba(56, 193, 255, .72); --social-text: #f3f7fc; --social-muted: #aebfd0; --social-accent: #55d3ff; --social-accent-soft: rgba(85, 211, 255, .14); --social-gold: #f0c76d; position: fixed; inset: 0; z-index: 100; pointer-events: none; color: var(--social-text); }
  .global-messages-host .social-dialog-backdrop { pointer-events: auto; }
  .global-message-error { margin: 0; padding: 10px 18px; color: #ffd9df; background: #4a2030; border-bottom: 1px solid #c65c73; font-size: .8rem; }
  .global-message-actions { display: flex; justify-content: flex-end; gap: 5px; margin: -3px -4px 3px; }
  .global-message-actions button { padding: 2px 5px; border: 0; color: var(--social-muted); background: transparent; cursor: pointer; font: inherit; font-size: .62rem; font-weight: 800; }
  .global-message-actions button:hover { color: var(--social-text); text-decoration: underline; }
  .global-messages-host .social-message-bubble p { margin: 0; white-space: pre-wrap; }
  :global(.student-app.theme-paper) .global-messages-host { --social-bg: #e5e2d8; --social-surface: #f2efe7; --social-surface-strong: #e9e5da; --social-surface-soft: #dddcd2; --social-border: #9eafa8; --social-border-strong: #277f86; --social-text: #21343b; --social-muted: #536b70; --social-accent: #207d84; --social-accent-soft: #c9e3df; --social-gold: #87611e; }
  :global(.student-app.theme-paper) .global-messages-host .social-dialog { background: #fffdf8; }
  @media (max-width: 600px) { .global-messages-host .social-dialog-backdrop { align-items: end; padding: 0; } .global-messages-host .social-dialog { width: 100%; height: calc(100vh - 12px); max-height: calc(100vh - 12px); border-radius: 18px 18px 0 0; } }
</style>
