<script lang="ts">
  import PortalNav from '$lib/components/PortalNav.svelte';
</script>
<svelte:head><title>TASK Karate | Dojo Portal</title><meta name="description" content="Choose the Task Karate Student Hub or Schedule and Check-in." /></svelte:head>
<main class="public-shell">
  <PortalNav current="home" />
  <section class="landing-hero"><div><span class="eyebrow">TASK KARATE SCHOOL · DOJO PORTAL</span><h1>Choose your dojo door.</h1><p>Pick the place you need and get moving.</p></div><div class="landing-choice-grid"><a class="landing-choice" href="/student/login"><span class="landing-choice-icon" aria-hidden="true">◉</span><span><strong>Student Hub</strong><span>Training, Social, Achievements, and Profile</span></span><span class="landing-choice-arrow" aria-hidden="true">→</span></a><a class="landing-choice" href="/schedule"><span class="landing-choice-icon" aria-hidden="true">◷</span><span><strong>Schedule / Check-in</strong><span>Today’s classes and front-desk check-in</span></span><span class="landing-choice-arrow" aria-hidden="true">→</span></a></div></section>
  <section class="landing-staff-callout"><div><span class="eyebrow">STAFF ACCESS</span><h2>Run the dojo.</h2><p>Open the protected operations workspace for students, attendance, guardians, classes, and news.</p></div><a class="button secondary" href="/staff/signin">Staff Login</a></section>
</main>
