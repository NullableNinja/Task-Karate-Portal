# Task Karate modernization plan

## Decision

Start with Option B: a fast static public site and a clearly bounded portal shell in the same project, sharing `css/tokens.css`, `css/site.css`, `css/portal.css`, `js/site.js`, and centralized data/configuration. The current source has no backend, no verified production domain, and no trusted auth boundary. A full SvelteKit migration now would move routing complexity forward without solving those constraints.

The portal uses a demo session only. Production authentication, authorization, student/guardian privacy, message retention, and instructor permissions remain backend responsibilities.

## Phases

### Phase 0 — investigation

- [x] Inspect local v2/v3/v4 snapshots and the original Git history.
- [x] Inspect the connected v2, v3, v4, karate-connect, and schedule-app repositories.
- [x] Inventory reusable content, assets, PDFs, data, and recurring failure modes.
- [x] Record decisions in `DESIGN_ARCHAEOLOGY.md`.

### Phase 1 — foundation

- [x] Capture the v4 baseline in a local `modernization` Git branch.
- [x] Add centralized site configuration with an unset production URL until verified.
- [x] Create the Paper-Fu 2.0 token layer: color, type, spacing, radii, elevation, motion, z-index, containers, and breakpoints.
- [x] Add a persistent light/dark theme preference with a night-blue Paper-Fu palette.
- [x] Replace the dynamic v4 shell with a small accessible shared navigation/footer.
- [x] Preserve old CSS/JS and source assets until the new surface is validated.

### Phase 2 — public experience

- [x] Rebuild the home page around the three visitor questions: what Task Karate is, what classes exist, and how to begin.
- [x] Add a dedicated programs page with current program terminology and schedule links.
- [x] Rebuild schedule rendering from `data/schedules.json` with filters and mobile day cards.
- [x] Rebuild student resources with downloads, belt requirements, and a distinct portal entry point.
- [x] Add a restrained news/archive page and preserve the historical JSON pipeline as clearly non-authoritative content.
- [x] Add about and contact/trial pages with accessible forms and failure/success states.
- [x] Restore the historical About content, instructor credentials, What We Teach copy, and exact TASK dojo code.

### Phase 3 — portal foundation, check-in, and community rebuild

- [x] Add login/demo-login boundary and an explicit demo-data notice.
- [x] Add dashboard, training, progression, achievements, messages, events, and profile views.
- [x] Rebuild the portal around the historical SvelteKit-era student hub hierarchy rather than the previous card-only dashboard.
- [x] Add a moderated-community foundation: feed, instructor notes, private-looking message threads, Gold Stars, achievements, rank progression, assignments, and journey history.
- [x] Add a family/guardian consent workflow with explicit demo-only and attorney-review boundaries.
- [x] Add a local staff workspace for demo moderation, staff notes, Gold Stars, synthetic profile edits, consent status, and snapshot export.
- [x] Restore the historical searchable roster check-in rather than a generic email/PIN login.
- [x] Restore student profile fields for belt size, uniform size, rank history context, and attendance.
- [x] Integrate student belt/age-group data with the shared schedule JSON.
- [x] Add a desk persistence adapter using IndexedDB fallback and an optional localhost SQLite service.
- [x] Keep the social layer training-focused instead of making a generic social feed the product's only purpose.
- [x] Add logout behavior and direct static route files for nested portal views.

### Phase 4 — polish

- [x] Add reduced-motion behavior, focus styles, keyboard-safe menus, dialog behavior, and touch-size targets.
- [x] Add responsive image dimensions, lazy-loading hints, and a small image inventory.
- [ ] Convert large raster sources to AVIF/WebP in a build pipeline when the deployment toolchain is chosen.
- [x] Remove production-domain hardcoding from the new shell and document the remaining legacy pages.
- [x] Restore Paper-Fu schedule pills, public rules/belt pages, schedule PDF access, and interactive class/news details.
- [x] Add themed scroll progress, belt-rank progress, back-to-top, contact, and trial interactions.
- [x] Add an explicitly local-only admin workflow demo and connect its synthetic profile draft to the portal.
- [x] Document production roles, moderation, guardian consent, privacy, audit, retention, and legal-review requirements.

### Phase 5 — validation

- [x] Run static checks for links, asset references, hardcoded domains, and missing JSON.
- [x] Run the local static server and exercise public and portal paths.
- [ ] Complete browser screenshot/keyboard QA at 320, 375, 390, 768, 1024, and 1440px on the final deployed host.
- [ ] Replace demo session/data with a real backend before exposing private student records.

## Data contracts

- `data/site.json` owns school identity, contact details, social links, and the optional canonical origin.
- `data/schedules.json` remains the source of truth for public class times.
- `data/portal-demo.json` is synthetic community/presentation data only.
- `data/portal-students.json` is a seed roster for the desk adapter; it contains no PINs or passwords. The SQLite desk database is local and ignored by Git.
- News JSON remains an archive/demo format until staff supplies verified editorial content.
- `admin/index.html` is a local workflow prototype; it is not a production content-management system. It cannot publish, authorize, or protect real student data.
- `server/desk_server.py` is a localhost deployment adapter for a supervised desk computer. It is not an internet-facing authentication or privacy boundary.

## Deployment and domain

The new pages use relative links and do not require a repository-name base path. Configure `data/site.json` `siteUrl` only after the production domain is verified. If static hosting is GitHub Pages, use a custom-domain or root deployment configuration and test direct navigation to every `.html` file; do not add repository-name prefixes to component links.

## Security boundary

The demo login is a visual/product-flow prototype. It does not authenticate a person, protect data, or authorize an instructor action. A production implementation must use server-managed sessions or a trusted identity provider, server-side authorization, validation/sanitization, CSRF protection for state-changing forms, rate limiting, and privacy-conscious logging. No real student records or secrets belong in this repository.

### Corrective interaction pass

The current pass deliberately restores the most useful v4 interaction language instead of replacing it with generic dashboard controls: schedule pills remain the primary program switcher, each class opens a detail dialog, verified historical schedule PDFs are downloadable, belt requirements and the exact TASK rules are public, news entries open detail dialogs, and the shared shell provides contact/trial dialogs, themed scroll affordances, and a back-to-top control. The two-week trial dialog is phrased as an inquiry because eligibility and terms must be confirmed by the school.

Known limitations remain intentional: only the historical Kids and Teens/Adults PDFs are currently available; other schedule views offer print output until verified PDFs exist. The admin page cannot securely publish or manage real records, and the portal still uses synthetic data.

### Visual reset pass

The first modernization pass was functionally useful but visually too close to a generic card-based school template. The follow-up reset establishes a stronger Paper-Fu / Field Notes direction: a warm paper-and-ink palette, cobalt and yellow as controlled accents, the transparent TASK mark in the shared header, editorial typography, photographic tape layers, fewer rounded surfaces, clearer home-page pathways, and a portal that shares the same visual grammar. Existing factual content, public rules, schedule behavior, belt resources, and demo security boundaries remain intact.

## Exit criteria

The modernization is ready for production handoff when staff verifies content/domain, a backend replaces the demo portal adapter, responsive and keyboard QA passes on the deployed host, and large images are emitted in responsive modern formats by the chosen build/deploy pipeline.
