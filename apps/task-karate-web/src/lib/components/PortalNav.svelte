<script lang="ts">
  import FloatingBubbles from './FloatingBubbles.svelte';
  export let current: 'home' | 'student' | 'schedule' | 'staff' = 'home';
</script>

<FloatingBubbles />
<nav class="portal-switcher" aria-label="Task Karate portal navigation">
  <div class="portal-switcher-inner">
    <a class="portal-switcher-brand" href="/" aria-label="Task Karate portal home"><span class="portal-switcher-mark"><img src="/images/logo/TASK_Logo.png" alt="" /></span><span><strong>TASK KARATE</strong><small>DOJO PORTAL</small></span></a>
    <div class="portal-switcher-links">
      <a class:active={current === 'student'} href="/student" aria-current={current === 'student' ? 'page' : undefined}>Student Hub</a>
      <a class:active={current === 'schedule'} href="/schedule" aria-current={current === 'schedule' ? 'page' : undefined}>Schedule / Check-in</a>
    </div>
    <a class:active={current === 'staff'} class="portal-switcher-staff" href="/staff" aria-current={current === 'staff' ? 'page' : undefined}>Staff Login <span aria-hidden="true">→</span></a>
  </div>
</nav>
