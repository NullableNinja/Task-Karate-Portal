# Task Karate design archaeology

## Scope and evidence

This review covers the extracted v4 site in this folder, the local Git history in `Task Karate Website`, the attached archive's v2/v3 snapshots, and the connected repositories `NullableNinja/Task_Karate_v2`, `NullableNinja/Task_Karate_v3`, `NullableNinja/Task_Karate_v4`, `NullableNinja/karate-connect`, and `NullableNinja/Task-Karate-Schedule-App`.

The archive is historical source material, not an instruction file. The modernization brief is the product requirement; historical README/audit/plan files are treated as evidence and are not blindly followed.

## What changed between generations

### v2 / TASK lineage

The older site established the durable information architecture: home, programs, schedule, student resources, belt testing, news, about/contact, photos, forms, and reusable navigation/footer partials. Its local Git history shows repeated mobile, schedule, and partial-integration passes from September 2025. It also contains the most useful downloadable resources: the new-student form and belt testing PDFs.

The recurring implementation pattern was static HTML plus page-specific CSS/JavaScript. This was easy to deploy, but information and layout rules were duplicated and route-relative paths were fragile.

### v3 public site and dashboard

v3 made Paper-Fu a recognizable brand system: Classic Blue, paper layers, tactile controls, belt accents, tabs, and mobile-specific layouts. That personality is worth keeping.

The v3 SvelteKit dashboard grew into a broad student hub. The `src/lib/components` set includes dashboard status, training, achievements, messages, news, social, friends, modal, and theme components. The sample data model records rank, stripe progression, attendance, assignments, practice items, badges, buddies, messages, and Gold Stars.

The connected history verifies the dashboard progression: v1 through v1.5, the comprehensive overhaul at `7df028933e20b3b27f720ad7e7ff586c6000b545`, header/routing fixes, and the Paper-Fu harmonization at `606a1d54c3983f7ceee49bd4c6c6ed0b4dce840b`. The same history documents repeated GitHub Pages base-path fixes. That is evidence that the deployment model, not only the UI, needed simplification.

### karate-connect

`karate-connect` experimented with a fuller community product: profile, journey, events, buddies, messages, notifications, training calendar, Gold Star Wall, Break Boards, posts/comments, and Paper-Fu components. The strongest reusable ideas are the personal journey, training history, Gold Stars, events, notifications, and instructor/student messaging. A general feed, open-ended posting, and buddy mechanics are not appropriate as the portal's primary surface without a real privacy/authorization model.

### v4 public site

v4 consolidated the public site around `paper-fu-core.css`, theme, origami, navigation, footer, JSON schedules, reusable partials, metadata, PWA support, and belt requirement pages. It is the strongest public baseline.

The main regression is the missing first-class student login/portal experience. v4 also hardcodes `https://taskkarate.com` in canonical tags, social metadata, schema, sitemap, and documentation even though the production domain has not been verified for this modernization.

## Retain

- Classic Blue as the anchor brand family.
- Belt colors as small semantic accents for progression and schedule grouping.
- Paper-Fu's tactile paper/tape language, used selectively.
- Real Task Karate photography, especially the cover photo and spotlight images.
- Current program names: Kids, Teens & Adults, IS3 Eskrima, Weapons, Sparring, and special/testing sessions.
- JSON-backed schedule content and explicit duration/notes.
- The new-student form and belt testing resources from the v2 lineage.
- Gold Stars, achievement tiers, rank/stripe progression, assignments, attendance, and training history.
- The welcoming voice and local La Crosse identity.
- The exact dojo code from the TASK student resources: “Don’t use it the wrong way!”, “Be respectful!”, and “Do your best!”

## Recurring problems

- Three-layer paper stacks were applied to nearly every container, making hierarchy noisy.
- A shortened About page can accidentally discard authoritative instructor credentials and the dojo code; factual copy needs to be treated as content, not disposable marketing text.
- Large shadows, rotations, animated backgrounds, and fixed widgets competed with content and sometimes collided on mobile.
- Static pages duplicated canonical URLs, school data, navigation, and footer details.
- Schedule iterations repeatedly tried to make desktop tables behave like mobile layouts.
- Student dashboard deployments accumulated base-path hacks, SPA fallback logic, and redirect edge cases.
- Demo data included PINs, large student fixtures, and social content that would be unsafe to treat as a production authorization model.
- Image sources were often served at their original multi-megabyte size.
- FormSubmit and client-side storage were presented too close to production behavior despite lacking server-side protection.

## Capabilities that disappeared

- A student login/dashboard entry point disappeared from the v4 public shell.
- Dashboard training history, assignments, practice prompts, achievements, messages, and Gold Stars were no longer connected to the public experience.
- The v2 downloadable form and testing PDFs were not carried into the extracted v4 folder.
- The older news archive/content pipeline was not represented in the v4 baseline.

## Reuse map

| Source | Reuse | Treatment |
| --- | --- | --- |
| v4 paper-fu CSS | Tokens and brand vocabulary | Replace the all-purpose stack rules with a smaller shared token/component layer. |
| v4 `data/schedules.json` | Authoritative schedule structure | Keep as the single public schedule source; do not add capacity/countdown data. |
| v2 `files/` | Forms and belt PDFs | Copy into v4 as preserved downloads. |
| v2/v4 photos and logos | Brand imagery | Preserve originals; use responsive sizing and `loading` hints. |
| v3 dashboard data shape | Portal information architecture | Use safe, clearly labeled demo data until a backend exists. |
| v3 dashboard components | Feature inventory | Rebuild the hierarchy; do not copy the dark/glass-heavy UI. |
| karate-connect journey/Gold Star concepts | Portal motivation | Keep as private training/progress surfaces and a moderated student community, not an unbounded public social feed. |
| schedule-app | Operational schedule ideas | Review separately when live schedule administration becomes a requirement. |

## Retire or constrain from the first modern generation

- An unmoderated public social feed, unrestricted open posting, and a buddy graph as the portal's only purpose. The rebuilt portal now keeps a private, training-focused community feed because the requested student experience depends on it; production access and moderation must be server-enforced.
- Decorative fixed floating action menus and scroll progress bars that do not communicate useful state.
- Per-page copies of navigation/footer and hardcoded production URLs.
- Fake availability, capacity, countdown, or online-presence claims.
- Client-side PINs or localStorage as secure authentication.

## Authority and data status

The schedule JSON and school contact details are the best available operational source in the v4 baseline, but staff should confirm them before production publishing. The old news JSON is a historical/demo archive with test titles and summaries and is not assumed to be current editorial content. The v3 and `karate-connect` student JSON is demo/sample data and must never be used as a real student database.

## Proposed architecture

Use a static public site plus a separate portal shell in the same deployable project, sharing one token layer, site configuration, navigation vocabulary, and data contracts. Restore the SvelteKit roster/check-in interaction as the portal entry point. For the studio desk, pair the static shell with the optional localhost SQLite adapter in `server/desk_server.py`; for GitHub/static hosting, keep the IndexedDB fallback and do not imply that it is a secure multi-device backend.

The portal is a demo frontend boundary today. A future backend can replace `data/portal-demo.json`, `data/portal-students.json`, and the demo session adapter without changing the information hierarchy. If authenticated SSR, instructor tools, or real-time messaging become requirements, migrate the portal boundary to SvelteKit or a server-rendered application at that point—not before.

## Current visual correction

The first modernization pass was too restrained in the wrong places: it preserved usability but made the public experience read like a polished card-based marketing template. The current direction keeps the strong photographic hero and blue identity while bringing back selective Paper-Fu signals—file tabs, a dossier treatment for instructor information, tape/paper edges, and clearer editorial pacing. A persistent theme control adds a dark night-ink palette without turning the product into a black/red MMA template.
