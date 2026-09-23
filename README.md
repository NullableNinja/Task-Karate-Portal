# Task Karate School

Task Karate now contains a local-first platform foundation alongside the preserved public HTML site. The new operational source of truth is:

`SvelteKit browser → authenticated same-origin API → one server-side database file`

The legacy JSON, browser storage, `server/desk_server.py`, and old portal routes remain for reference and public-site continuity. They are not safe for real student records and are not used by the new app.

The student-facing milestone has two live experiences: `/schedule` for the database-backed class schedule and check-in, and `/student` for authenticated student profiles, status, social, achievements, training tracks, and personal classes. Public news belongs to the future public website (`Task-Karate-Web`) and is exposed by the portal only through its authenticated/public-content API boundary; it is not part of the private student navigation.

The portal now uses one physical runtime database: `server/TaskKarate.Api/App_Data/task-karate.db` by default. The active operational workflows use the canonical portal tables in that file for students, guardians, programs, class templates, sessions, enrollments, attendance, social activity, and published content. EF/Identity tables remain namespaced as `platform_*` for authentication and compatibility. If `TASK_KARATE_STARTER_DB` points to the old standalone starter file, startup imports missing legacy tables and rows into the runtime database. That variable is a migration source, not a second active production database. The source file is never opened by the browser.

## Run the new platform

PowerShell, from the repository root:

```powershell
$env:TASK_KARATE_ADMIN_EMAIL = "admin@example.test"
$env:TASK_KARATE_ADMIN_PASSWORD = "Use-a-local-password-with-12-or-more-chars!"
$sdk = "$env:LOCALAPPDATA\TaskKarate\dotnet8"
$env:DOTNET_ROOT = $sdk
& "$sdk\dotnet.exe" restore TaskKarate.sln
& "$sdk\dotnet.exe" run --project server\TaskKarate.Api --urls http://127.0.0.1:5167
```

In another PowerShell window:

```powershell
cd apps\task-karate-web
npm ci
npm run dev
```

Open [http://localhost:5173](http://localhost:5173). The student schedule is at `/schedule`, student sign-in is at `/student/login`, and staff sign-in is at `/staff/signin`.

The API creates `server/TaskKarate.Api/App_Data/task-karate.db` through EF Core migrations. Database, WAL/SHM files, local settings, and credentials are ignored by Git. The first administrator is created only when `TASK_KARATE_ADMIN_EMAIL` and `TASK_KARATE_ADMIN_PASSWORD` (or matching .NET user secrets) are present; there is no default credential.

For production, treat this as a single-server application: keep the database on the same mini PC/VPS as the API, behind HTTPS and a reverse proxy. Do not place the database in OneDrive, a shared network folder, a public web directory, or a synchronized cloud folder. See [docs/LOCAL_SETUP.md](docs/LOCAL_SETUP.md) for the migration and mini-PC deployment sequence. The runtime cutover is complete for active portal workflows; the `platform_*` EF model remains as an intentional authentication/compatibility boundary until it can be retired in a later schema migration.

## Verification

```powershell
$sdk = "$env:LOCALAPPDATA\TaskKarate\dotnet8"; $env:DOTNET_ROOT = $sdk
& "$sdk\dotnet.exe" restore
& "$sdk\dotnet.exe" build
& "$sdk\dotnet.exe" test
cd apps\task-karate-web
npm ci
npm run check
npm run build
```

See [docs/LOCAL_SETUP.md](docs/LOCAL_SETUP.md) for migrations, bootstrap, backup/restore, and LAN-supervision guidance. See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md), [docs/DATABASE_SCHEMA.md](docs/DATABASE_SCHEMA.md), [docs/SECURITY_BOUNDARY.md](docs/SECURITY_BOUNDARY.md), and [docs/LEGACY_RETIREMENT_PLAN.md](docs/LEGACY_RETIREMENT_PLAN.md).
