<script lang="ts">
  import { afterNavigate, goto } from '$app/navigation';
  import { onMount } from 'svelte';

  let previousPath = '';
  let currentPath = '';

  function pathKey(path = window.location.pathname + window.location.search) {
    return `task-karate-context-back:${path}`;
  }

  function readablePath(path: string) {
    const value = path.split('?')[0].replace(/^\//, '').split('/').filter(Boolean).pop() ?? 'hub';
    return value.replace(/[-_]/g, ' ').replace(/\b\w/g, (letter) => letter.toUpperCase());
  }

  function rememberDestination(event: MouseEvent) {
    const target = event.target as HTMLElement | null;
    const anchor = target?.closest('a') as HTMLAnchorElement | null;
    if (!anchor || anchor.target === '_blank' || anchor.hasAttribute('download')) return;
    const destination = new URL(anchor.href, window.location.href);
    if (destination.origin !== window.location.origin) return;
    const destinationPath = destination.pathname + destination.search;
    if (destinationPath === currentPath) return;
    if (anchor.closest('[data-student-nav]')) {
      sessionStorage.removeItem(pathKey(destinationPath));
      return;
    }
    sessionStorage.setItem(pathKey(destinationPath), currentPath);
  }

  function refresh(path = window.location.pathname + window.location.search) {
    currentPath = path;
    previousPath = sessionStorage.getItem(pathKey(path)) ?? '';
  }

  function goBack() {
    if (!previousPath) return;
    const destination = previousPath;
    sessionStorage.removeItem(pathKey(currentPath));
    goto(destination);
  }

  afterNavigate(({ to }) => { if (to) refresh(to.url.pathname + to.url.search); });

  onMount(() => {
    refresh();
    document.addEventListener('click', rememberDestination, true);
    return () => document.removeEventListener('click', rememberDestination, true);
  });
</script>

{#if previousPath}
  <div class="student-context-back-wrap">
    <button class="student-context-back" type="button" on:click={goBack} aria-label={`Back to ${readablePath(previousPath)}`}>
      <span aria-hidden="true">←</span> Back to {readablePath(previousPath)}
    </button>
  </div>
{/if}
