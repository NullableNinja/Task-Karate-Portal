# Architecture

## Runtime boundary

The new platform has one persistence boundary:

```text
SvelteKit UI (browser)
        │ same-origin API requests, credentials: include
        ▼
ASP.NET Core 8 Minimal API
        ├── staff Identity cookie → EF Core platform database
        └── student cookie → parameterized StarterDatabase service
                              ↓
                         SQLite (local starter DB)
```

The browser never opens SQLite and never treats localStorage or IndexedDB as a record store. API failures are shown as errors. The preserved HTML site can still read its historical JSON files, but that content is explicitly not the new platform source of truth.

## Backend

`server/TaskKarate.Api` uses ASP.NET Core Minimal API, ASP.NET Core Identity with GUID users and roles, EF Core SQLite, migrations, protected cookies, and loopback-oriented local launch settings. Staff endpoints require `Administrator`, `Instructor`, or `Staff`. Student access uses a separate protected cookie and strong password hashing through the Identity password hasher. Student schedule/profile/social queries are parameterized against the supplied starter database; the browser never receives a database connection.

All mutation endpoints validate input, create UTC timestamps, and record safe audit metadata. Attendance has a database uniqueness constraint on `(ClassSessionId, StudentId)`. Published public endpoints select only published content and schedule fields; no student or guardian query is reachable anonymously.

## Frontend

`apps/task-karate-web` is a real SvelteKit application with the public `/schedule` check-in experience, server-backed student sign-in and acknowledgment, a floating student hub, social/friend/message pages, and the existing staff workflows. `src/lib/api.ts` attaches cookies and the CSRF header and has no persistence fallback.

The student service adds only missing tables to the supplied starter database: `student_accounts`, `student_disclaimer_acceptances`, `student_friendships`, and `student_messages`. Existing class, attendance, rank, achievement, profile, and post tables remain the source of truth.

## Legacy import boundary

Development-only import flags can seed class templates from `data/schedules.json` and clearly marked demo students from `data/portal-students.json`. Social data, messages, consent records, and old news are never imported automatically. The importer is repeatable by unique names and runs only when explicitly enabled in Development configuration.
