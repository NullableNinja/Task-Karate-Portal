<script lang="ts">
  import type { EasterEggDefinition } from '$lib/easter-eggs';

  export let egg: EasterEggDefinition | null = null;
  export let newlyUnlocked = false;
  export let studentName = 'Student';
  export let rankName = 'Rank in progress';
  export let onClose: () => void = () => undefined;
</script>

{#if egg}
  <div class="easter-egg-layer" role="presentation">
    <div class="easter-egg-sparks" aria-hidden="true">
      {#each Array(18) as _, index}<span style={`--spark-index:${index}`}></span>{/each}
    </div>
    <section class="easter-egg-card" role="status" aria-live="polite" aria-label={`${egg.name} discovered`}>
      <button class="easter-egg-close" type="button" on:click={onClose} aria-label="Close easter egg celebration">×</button>
      {#if egg.id === 'egg-card-carrying-student'}
        <div class="student-trading-card">
          <span class="student-trading-card-mark" aria-hidden="true">🥋</span>
          <span class="student-eyebrow">TASK KARATE / STUDENT CARD</span>
          <strong>{studentName}</strong>
          <span>{rankName}</span>
          <small>Card-carrying student · active dojo member</small>
        </div>
      {:else}
        <span class="easter-egg-icon" aria-hidden="true">{egg.icon}</span>
        <span class="student-eyebrow">HIDDEN DOJO DISCOVERED</span>
        <h2>{egg.name}</h2>
        <p>{egg.description}</p>
      {/if}
      <strong>{newlyUnlocked ? 'Achievement unlocked!' : 'You found it again!'}</strong>
    </section>
  </div>
{/if}
