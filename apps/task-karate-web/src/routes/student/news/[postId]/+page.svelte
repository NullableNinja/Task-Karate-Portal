<script lang="ts">
  import { onMount } from 'svelte';
  import { goto } from '$app/navigation';
  onMount(() => goto('/student/classes'));
</script>

<svelte:head><title>Task Karate | My classes</title></svelte:head>
<p class="visually-hidden">Student news moved to the public Task Karate website. Redirecting to My Classes.</p>
