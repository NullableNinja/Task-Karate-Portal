# Task Karate School · Paper-Fu 2.0

This folder is the modernization branch for the Task Karate public website and student portal foundation.

## Run locally

The site uses `fetch()` for shared navigation, footer, schedules, news, and demo portal data, so open it through a local HTTP server rather than `file://`.

```powershell
cd "C:\Users\Thoma\OneDrive\Web Design\Task_Karate_v4"
python -m http.server 4173
```

Then open [http://localhost:4173/](http://localhost:4173/).

Key paths:

- `/index.html` — public home
- `/programs.html` — programs
- `/schedule.html` — responsive, JSON-backed schedule
- `/students.html` — student resources and preserved downloads
- `/belts.html` — public belt testing requirements and downloads
- `/rules.html` — public Task Karate rules
- `/news.html` — historical/demo news archive
- `/about.html` — school and instructor story
- `/contact.html` — trial/contact flow with a safe demo state
- `/admin/index.html` — local staff workspace demo for moderation, notes, Gold Stars, and consent status
- `/checkin.html` — supervised desk check-in / searchable student roster
- `/portal/login.html` — same check-in surface from the portal route
- `/portal/index.html` — Student Hub home/community feed
- `/portal/training.html`, `/portal/progress.html`, `/portal/messages.html`, `/portal/events.html`, `/portal/profile.html`, `/portal/waivers.html` — direct portal routes

## Configuration

Edit `data/site.json` for school identity and contact details. Leave `siteUrl` empty until the production domain is verified. The new pages use relative links and do not require a repository-name base path.

Public class times live in `data/schedules.json`; the historical Kids and Teens/Adults PDF exports are in `files/schedules/`. The admin demo can download the current JSON, but it does not publish changes to a server. News is managed from `news/posts-index.json` and the associated post JSON files. Do not add capacity/countdown values unless the school has an authoritative live source.

The Student Hub restores the strongest SvelteKit-era ideas—searchable roster check-in, profile facts, belt/stripe progression, community feed, instructor notes, private-looking threads, Gold Stars, achievements, assignments, rank progression, and a journey timeline—while keeping the seed data clearly labeled. The integrated class list is generated from `data/schedules.json` and the selected student's belt/age-group fields.

For a persistent desk deployment, run `python server/desk_server.py`. It uses a local SQLite file and exposes roster/check-in endpoints. Without that service, the check-in UI falls back to browser IndexedDB. See `docs/LOCAL_DESK_HOSTING.md`. Neither mode is a secure internet-facing student backend.

The exact student rules and belt requirements are public and do not require portal sign-in.

## Content and assets

The original high-resolution images remain in place. The modernization adds responsive dimensions and lazy loading to the new pages, while preserving the older v4 CSS/JS and belt HTML as historical compatibility material. Original v2 PDFs were copied into `files/forms/` and `files/testing/` without overwriting the source archive.

The current news JSON contains historical/demo test entries. Confirm editorial content before publishing it as current news.

## Portal security boundary

The Student Portal is a frontend prototype. Its login sets a local demo session and loads synthetic data; it is not authentication and must not be used for real student records. The companion admin page and family acknowledgment workflow are likewise local-only. The acknowledgment is not legal advice or a liability shield. Production requires attorney-reviewed policy language, verified guardian consent, server-side authorization between students/guardians/instructors, validation, CSRF protection, rate limiting, moderation, privacy controls, audit history, and safe message handling. See `docs/PORTAL_SAFETY_AND_CONSENT.md`.

## Deployment checklist

1. Verify the production domain and set `data/site.json.siteUrl`.
2. Generate a sitemap from that verified origin; the included historical sitemap is intentionally not treated as authoritative.
3. Confirm contact details, schedule, program language, and news with staff.
4. Replace the demo portal adapter with a secure backend/auth provider.
5. Run responsive and keyboard QA at 320, 375, 390, 768, 1024, and 1440px on the deployed host.
6. Add an image build step for AVIF/WebP and `srcset` variants when the hosting pipeline is selected.

See `docs/DESIGN_ARCHAEOLOGY.md` and `docs/MODERNIZATION_PLAN.md` for the evidence, preserved functionality, retired behavior, architecture decision, and known limitations.
