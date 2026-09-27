<script lang="ts">
  export let achievement: any = null;
  export let onClose: () => void;

  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') onClose();
  }

  function achievementIcon(iconName?: string) {
    const icons: Record<string, string> = {
      anniversary: '✦', star: '★', leadership: '↗', is3: 'IS3',
      ten: '10×', hundred: '100×', thousand: '1K', 'ten-thousand': '10K',
      'helper-one': '🤝', 'helper-ten': '10×', 'helper-hundred': '100×', 'helper-thousand': '1K', 'helper-ten-thousand': '10K',
      'dojo-checkin-one': 'HI', 'dojo-checkin-five': '5×', 'dojo-checkin-twenty-five': '25×', 'dojo-checkin-hundred': '100×',
      calendar: '◷', 'calendar-star': '✦', flame: '🔥', compass: '◎', bell: '🔔', belt: '🥋', scroll: '📜', kata: '🎮', speaker: '📣', sticker: '✨', card: '🃏', wisdom: '🧠', 'hidden-dojo': '🌟'
    };
    return icons[String(iconName ?? '').toLowerCase()] ?? '★';
  }
</script>

<svelte:window on:keydown={handleKeydown} />

{#if achievement}
  <div class="achievement-modal-backdrop" role="presentation" on:click={onClose}>
    <div class="achievement-modal" role="dialog" aria-modal="true" aria-labelledby="achievement-detail-title" tabindex="-1" on:click|stopPropagation on:keydown|stopPropagation>
      <button class="achievement-modal-close" type="button" aria-label="Close achievement details" on:click={onClose}>×</button>
      <span class="achievement-modal-icon" aria-hidden="true">{achievement.iconName?.length <= 3 ? achievement.iconName : achievement.icon ?? achievementIcon(achievement.iconName)}</span>
      <span class="student-eyebrow">ACHIEVEMENT DETAILS</span>
      <h2 id="achievement-detail-title">{achievement.name ?? achievement.label ?? 'Dojo milestone'}</h2>
      <p>{achievement.description ?? achievement.clue ?? achievement.group ?? 'Dojo milestone earned.'}</p>
      {#if achievement.current !== undefined}<small>{achievement.complete ? 'Milestone earned · keep building.' : `${achievement.current} of ${achievement.target} toward this milestone.`}</small>{:else if achievement.awardedAt}<small>Earned {new Date(achievement.awardedAt).toLocaleDateString()}</small>{/if}
    </div>
</div>
{/if}
