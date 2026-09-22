# Task Karate School

Task Karate now contains a local-first platform foundation alongside the preserved public HTML site. The new operational source of truth is:

`SvelteKit browser → authenticated same-origin API → EF Core / SQLite`

The legacy JSON, browser storage, `server/desk_server.py`, and old portal routes remain for reference and public-site continuity. They are not safe for real student records and are not used by the new app.

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

Open [http://localhost:5173](http://localhost:5173). The staff sign-in is at `/staff/signin`; the public schedule is at `/`.

The API creates `server/TaskKarate.Api/App_Data/task-karate.db` through EF Core migrations. Database, WAL/SHM files, local settings, and credentials are ignored by Git. The first administrator is created only when `TASK_KARATE_ADMIN_EMAIL` and `TASK_KARATE_ADMIN_PASSWORD` (or matching .NET user secrets) are present; there is no default credential.

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
