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

`apps/task-karate-web` is a real SvelteKit application with a dark, today-first public `/schedule` front-desk experience, server-backed student sign-in and acknowledgment, a floating student hub, social/friend/message pages, and the existing staff workflows. The public schedule reads the starter schedule database through the API, keeps future dates view-only, and uses a searched student plus explicit confirmation for today's supervised check-in. `src/lib/api.ts` attaches cookies and the CSRF header and has no persistence fallback.

The student service adds only missing tables to the supplied starter database: `student_accounts`, `student_disclaimer_acceptances`, `student_friendships`, `student_messages`, practice logs, goals, and saved posts. Existing class, attendance, rank, achievement, profile, and post tables remain the source of truth. Development normalization repairs duplicate guardian seed rows and prevents an IS3 membership from being shown for a non-Teen/Adult student.

## Product boundary: public news vs. student work

Public news and announcements answer questions for prospective and current families and belong on `Task-Karate-Web`. The portal may provide the authenticated API that supplies published content, but the student hub does not duplicate that public-news destination in its navigation. The hub is for private, actionable work: status, social connections and messages, Gold Star achievements, program-specific training requirements, and My Classes/check-in. Staff also has a server-backed social moderation workspace for editing or reversible removal of student posts and comments.

Training is split conceptually into two tracks. The Karate track shows the belt testing requirements from the school’s rank material, adapted for Kids or Teens/Adults. The IS3 track is available only when the student has an active IS3 program membership and uses level progression rather than belts. My Classes is the replacement for the portal News tab: it shows upcoming sessions, location/time, attendance context, and authenticated check-in.

The public schedule groups same-day sessions that share a program, time, and dojo location into one physical class block. Belt tracks remain separate records for attendance and rank validation, but the front-end presents them as one class with selectable tracks so a single-dojo schedule does not imply simultaneous classes.

Attendance supports `present` and `helper` records. Helper check-in is validated server-side: the student must have a current belt rank strictly higher than the highest belt named by the class track. IS3-only classes do not accept belt-helper check-ins.

## Legacy import boundary

Development-only import flags can seed class templates from `data/schedules.json` and clearly marked demo students from `data/portal-students.json`. Social data, messages, consent records, and old news are never imported automatically. The importer is repeatable by unique names and runs only when explicitly enabled in Development configuration.
