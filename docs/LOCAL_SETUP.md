# Local setup (Windows / PowerShell)

## Prerequisites

Install Node.js 20+ and the .NET 8 SDK. If the SDK is not installed system-wide, the repository was verified with a user-local SDK under `%LOCALAPPDATA%\TaskKarate\dotnet8`; set `DOTNET_ROOT` and call that `dotnet.exe` as shown in the README.

## First administrator

Do not put credentials in Git. For one local run:

```powershell
$env:TASK_KARATE_ADMIN_EMAIL = "admin@example.test"
$env:TASK_KARATE_ADMIN_PASSWORD = "Use-a-local-password-with-12-or-more-chars!"
```

For a persistent local secret, use user secrets from `server\TaskKarate.Api`:

```powershell
dotnet user-secrets init
dotnet user-secrets set "BootstrapAdmin:Email" "admin@example.test"
dotnet user-secrets set "BootstrapAdmin:Password" "Use-a-local-password-with-12-or-more-chars!"
```

The API creates roles and the administrator on startup. Remove the environment variables after the first successful bootstrap if user secrets are used. Identity enforces a strong password and locks out repeated failures.

## Migrations and database

```powershell
dotnet ef database update --project server\TaskKarate.Api --startup-project server\TaskKarate.Api
dotnet run --project server\TaskKarate.Api --urls http://127.0.0.1:5167
```

The database is `server\TaskKarate.Api\App_Data\task-karate.db`; SQLite may also create `-wal` and `-shm` files. All are ignored. The development import is explicit: set `Seed:ImportLegacySchedules` or `Seed:ImportDemoStudents` to `true` in a local settings/user-secret configuration, and never enable demo import for a production database. Legacy schedules are imported as unverified templates; demo students are synthetic.

## Backup and restore

Each day, while the API is stopped (or after a SQLite checkpoint), copy the database to a separate encrypted drive/location:

```powershell
$source = "server\TaskKarate.Api\App_Data\task-karate.db"
$destination = "E:\EncryptedBackups\TaskKarate\task-karate-$(Get-Date -Format yyyy-MM-dd).db"
Copy-Item $source $destination
```

Use BitLocker or an equivalent encrypted location, restrict access, and retain several dated copies. To test restore, copy a backup to a temporary path, change `ConnectionStrings:TaskKarate` in a local ignored settings file to that path, start the API with test credentials, sign in, and verify student/session/attendance counts. Never test a restore by overwriting the live database.

## Supervised LAN use

Default local launch binds to loopback. For a supervised staff computer, explicitly choose a private LAN address (for example `http://192.168.1.20:5167`), add the exact SvelteKit origin to `AllowedOrigins`, use HTTPS with a trusted local certificate where possible, restrict Windows Firewall to the private profile and staff subnet, and stop the process when finished. Do not port-forward or expose this application to the internet. A LAN bind is deliberately not the default.
