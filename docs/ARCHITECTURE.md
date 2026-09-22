# Architecture

## Runtime boundary

The new platform has one persistence boundary:

```text
SvelteKit UI (browser)
        │ same-origin API requests, credentials: include
        ▼
ASP.NET Core 8 Minimal API
        │ Identity cookie, roles, CSRF double-submit, validation, audit
        ▼
EF Core 8 + SQLite (server/TaskKarate.Api/App_Data/task-karate.db)
```

The browser never opens SQLite and never treats localStorage or IndexedDB as a record store. API failures are shown as errors. The preserved HTML site can still read its historical JSON files, but that content is explicitly not the new platform source of truth.

## Backend

`server/TaskKarate.Api` uses ASP.NET Core Minimal API, ASP.NET Core Identity with GUID users and roles, EF Core SQLite, migrations, protected cookies, and loopback-oriented local launch settings. Staff endpoints require `Administrator`, `Instructor`, or `Staff`. Guardian and student roles exist for schema readiness but have no portal login in this milestone.

All mutation endpoints validate input, create UTC timestamps, and record safe audit metadata. Attendance has a database uniqueness constraint on `(ClassSessionId, StudentId)`. Published public endpoints select only published content and schedule fields; no student or guardian query is reachable anonymously.

## Frontend

`apps/task-karate-web` is a real SvelteKit application with public read-only schedule/announcement pages, server-backed staff sign-in, dashboard, student and guardian management, program/template/session management, attendance, announcement, and news workflows. `src/lib/api.ts` attaches cookies and the CSRF header and has no persistence fallback.

## Legacy import boundary

Development-only import flags can seed class templates from `data/schedules.json` and clearly marked demo students from `data/portal-students.json`. Social data, messages, consent records, and old news are never imported automatically. The importer is repeatable by unique names and runs only when explicitly enabled in Development configuration.
