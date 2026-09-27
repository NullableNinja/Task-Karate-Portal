<script lang="ts">
  import { onMount } from 'svelte';

  type Bubble = {
    id: string;
    baseSize: number;
    x: number;
    y: number;
    vx: number;
    vy: number;
    radius: number;
    pulse: number;
    phase: number;
  };

  const seeds = [
    { id: 'a', baseSize: 210, x: .08, y: .19, vx: 16, vy: 12, phase: 0.2 },
    { id: 'b', baseSize: 115, x: .34, y: .05, vx: -12, vy: 16, phase: 1.5 },
    { id: 'c', baseSize: 300, x: .94, y: .25, vx: -14, vy: 10, phase: 2.4 },
    { id: 'd', baseSize: 78, x: .73, y: .56, vx: 18, vy: -13, phase: 3.1 },
    { id: 'e', baseSize: 165, x: .49, y: .99, vx: -15, vy: -11, phase: 4.2 },
    { id: 'f', baseSize: 52, x: .15, y: .53, vx: 13, vy: -16, phase: 5.1 },
    { id: 'g', baseSize: 260, x: .76, y: .99, vx: -11, vy: -12, phase: 5.8 },
    { id: 'h', baseSize: 92, x: .92, y: .68, vx: -16, vy: 14, phase: 6.6 },
    { id: 'i', baseSize: 64, x: .57, y: .31, vx: 15, vy: 11, phase: 7.3 }
  ];

  let field: HTMLDivElement;
  let bubbles: Bubble[] = [];

  function resize() {
    if (!field) return;
    const { width, height } = field.getBoundingClientRect();
    const scale = Math.min(1.15, Math.max(.58, width / 1280));
    bubbles = seeds.map((seed) => {
      const size = seed.baseSize * scale;
      return { ...seed, x: seed.x * width, y: seed.y * height, radius: size / 2, pulse: 1, baseSize: size };
    });
  }

  function update(time: number, lastTime: number) {
    const dt = Math.min((time - lastTime) / 1000, .04);
    if (!dt || !field) return;
    const { width, height } = field.getBoundingClientRect();
    for (const bubble of bubbles) {
      bubble.x += bubble.vx * dt;
      bubble.y += bubble.vy * dt;
      if (bubble.x - bubble.radius < 0 || bubble.x + bubble.radius > width) {
        bubble.x = Math.max(bubble.radius, Math.min(width - bubble.radius, bubble.x));
        bubble.vx *= -1;
      }
      if (bubble.y - bubble.radius < 0 || bubble.y + bubble.radius > height) {
        bubble.y = Math.max(bubble.radius, Math.min(height - bubble.radius, bubble.y));
        bubble.vy *= -1;
      }
    }
    bubbles = bubbles.map((bubble) => ({ ...bubble, pulse: 1 + Math.sin(time / 1450 + bubble.phase) * .045 }));
  }

  function bubbleStyle(bubble: Bubble) {
    return `--bubble-x:${bubble.x - bubble.radius}px;--bubble-y:${bubble.y - bubble.radius}px;--bubble-size:${bubble.baseSize}px;--bubble-pulse:${bubble.pulse}`;
  }

  onMount(() => {
    let frame = 0;
    let lastTime = performance.now();
    resize();
    const animate = (time: number) => {
      update(time, lastTime);
      lastTime = time;
      frame = requestAnimationFrame(animate);
    };
    frame = requestAnimationFrame(animate);
    window.addEventListener('resize', resize);
    return () => { cancelAnimationFrame(frame); window.removeEventListener('resize', resize); };
  });
</script>

<div class="bubble-field" bind:this={field} aria-hidden="true">
  {#each bubbles as bubble (bubble.id)}
    <span class={`portal-orb orb-${bubble.id}`} style={bubbleStyle(bubble)}></span>
  {/each}
</div>
