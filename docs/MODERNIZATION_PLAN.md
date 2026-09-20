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

### Phase 3 — portal foundation

- [x] Add login/demo-login boundary and an explicit demo-data notice.
- [x] Add dashboard, training, progression, achievements, messages, events, and profile views.
- [x] Keep training and instructor communication central; omit the social feed from the primary navigation.
- [x] Add logout behavior and direct static route files for nested portal views.

### Phase 4 — polish

- [x] Add reduced-motion behavior, focus styles, keyboard-safe menus, dialog behavior, and touch-size targets.
- [x] Add responsive image dimensions, lazy-loading hints, and a small image inventory.
- [ ] Convert large raster sources to AVIF/WebP in a build pipeline when the deployment toolchain is chosen.
- [x] Remove production-domain hardcoding from the new shell and document the remaining legacy pages.

### Phase 5 — validation

- [x] Run static checks for links, asset references, hardcoded domains, and missing JSON.
- [x] Run the local static server and exercise public and portal paths.
- [ ] Complete browser screenshot/keyboard QA at 320, 375, 390, 768, 1024, and 1440px on the final deployed host.
- [ ] Replace demo session/data with a real backend before exposing private student records.

## Data contracts

- `data/site.json` owns school identity, contact details, social links, and the optional canonical origin.
- `data/schedules.json` remains the source of truth for public class times.
- `data/demo-student.json` is synthetic portal presentation data only.
- News JSON remains an archive/demo format until staff supplies verified editorial content.

## Deployment and domain

The new pages use relative links and do not require a repository-name base path. Configure `data/site.json` `siteUrl` only after the production domain is verified. If static hosting is GitHub Pages, use a custom-domain or root deployment configuration and test direct navigation to every `.html` file; do not add repository-name prefixes to component links.

## Security boundary

The demo login is a visual/product-flow prototype. It does not authenticate a person, protect data, or authorize an instructor action. A production implementation must use server-managed sessions or a trusted identity provider, server-side authorization, validation/sanitization, CSRF protection for state-changing forms, rate limiting, and privacy-conscious logging. No real student records or secrets belong in this repository.

## Exit criteria

The modernization is ready for production handoff when staff verifies content/domain, a backend replaces the demo portal adapter, responsive and keyboard QA passes on the deployed host, and large images are emitted in responsive modern formats by the chosen build/deploy pipeline.
