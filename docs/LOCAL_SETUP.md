# Local setup (Windows / PowerShell)

## Prerequisites

Install Node.js 20+ and the .NET 8 SDK. If the SDK is not installed system-wide, the repository was verified with a user-local SDK under `%LOCALAPPDATA%\TaskKarate\dotnet8`; set `DOTNET_ROOT` and call that `dotnet.exe` as shown in the README.

## First administrator

Do not put credentials in Git. For one local run:

```powershell
$env:TASK_KARATE_ADMIN_USERNAME = "Admin"
$env:TASK_KARATE_ADMIN_EMAIL = "admin@example.test" # optional fallback identifier
$env:TASK_KARATE_ADMIN_PASSWORD = "Use-a-local-password-with-12-or-more-chars!"
```

For a persistent local secret, use user secrets from `server\TaskKarate.Api`:

```powershell
dotnet user-secrets init
dotnet user-secrets set "BootstrapAdmin:Username" "Admin"
dotnet user-secrets set "BootstrapAdmin:Email" "admin@example.test"
dotnet user-secrets set "BootstrapAdmin:Password" "Use-a-local-password-with-12-or-more-chars!"
```

The API creates roles and the administrator on startup. Staff can sign in with the configured username or email. If no username is supplied, the email becomes the username. Remove the environment variables after the first successful bootstrap if user secrets are used. Identity enforces a strong password and locks out repeated failures.

Staff sign-in includes a local-use acknowledgment because this workspace can display student, guardian, attendance, and community records. The acknowledgment is enforced by the API, not merely displayed by the browser.

The staff sign-in form does not persist a “keep me signed in” choice. After credentials are submitted, the required staff-use acknowledgment appears in a confirmation dialog. The resulting cookie is a session-scoped local staff session.

Class times are entered/imported in the database’s normal schedule format but are displayed to users in 12-hour local time. The public schedule collapses same-time, same-dojo belt tracks into one physical class block; the check-in dialog lets staff choose the appropriate underlying track.

Students may choose regular attendance or helper check-in from a current class. Helper status is never trusted from the browser: the API checks the student’s current belt against the class threshold and rejects an under-ranked helper.

## Migrations and database

```powershell
dotnet ef database update --project server\TaskKarate.Api --startup-project server\TaskKarate.Api
dotnet run --project server\TaskKarate.Api --urls http://127.0.0.1:5167
```

The database is `server\TaskKarate.Api\App_Data\task-karate.db`; SQLite may also create `-wal` and `-shm` files. All are ignored. The development import is explicit: set `Seed:ImportLegacySchedules` or `Seed:ImportDemoStudents` to `true` in a local settings/user-secret configuration, and never enable demo import for a production database. Legacy schedules are imported as unverified templates; demo students are synthetic.

## Student schedule and profile database

The student experience reads the local starter database through the API. Point it at the supplied file with an environment variable in the API PowerShell window:

```powershell
$env:TASK_KARATE_STARTER_DB = "C:\Users\Thoma\OneDrive\Web Design\Task-Karate-School\TaskKarate_Starter.db"
```

On first API startup the service creates only its missing support tables (`student_accounts`, disclaimer acknowledgments, friendships, and messages). It does not import social data or rewrite existing schedule records. Back up the original file before first use. To create a local student login without committing a password, set all three variables before starting the API:

The supplied file currently contains the schema but may contain no student or session rows yet. In that case `/schedule` correctly reports that no sessions are published until staff or an explicit development seed creates them; the application does not invent live student records.

For synthetic development data, opt into the repeatable import from the repository archive:

```powershell
$env:TASK_KARATE_IMPORT_STARTER_SCHEDULES = "1"
$env:TASK_KARATE_IMPORT_STARTER_STUDENTS = "1"
$env:TASK_KARATE_DEMO_STUDENT_PASSWORD = "Use-a-local-demo-password-with-12-chars!"
```

The schedule importer reads `data/schedules.json` into recurring `classes` and `class_schedule` rows, then materializes the next 35 days into `class_sessions`. The student importer reads `data/portal-students.json` into clearly marked demo profiles and rank history. When `TASK_KARATE_DEMO_STUDENT_PASSWORD` is present, it also creates development-only accounts for imported records whose role includes `student`, so the roster sign-in can be exercised. This password is never committed and must never be used for a real database. Both import switches are off by default.

```powershell
$env:TASK_KARATE_STUDENT_ID = "1"
$env:TASK_KARATE_STUDENT_USERNAME = "student.demo"
$env:TASK_KARATE_STUDENT_PASSWORD = "Use-a-long-local-password-with-12-chars!"
```

The bootstrap account is created once if that student is active and has no account. Credentials are hashed and never written to source control. Remove these variables after the account exists. Open `/schedule` for the live schedule and `/student/login` for the profile hub. The student sign-in lists active, provisioned student accounts alphabetically; selecting a profile opens the server-verified PIN/password prompt, followed by the profile acknowledgment when required.

Do not use a short test phrase such as `8675309` or `Cobra Kai Never Dies!` as the administrator password. The configured Identity policy requires a long password with upper/lowercase characters, a number, and a non-alphanumeric character. For a temporary local test credential, use an environment-only value such as `Cobra Kai Never Dies!7` and remove it after bootstrapping; never commit it or use it for real student records.

`/schedule` is the supervised front-desk schedule. It prioritizes today's in-progress and upcoming classes, keeps future dates view-only, and uses the imported schedule database. A same-origin check-in requires a searched student selection and an explicit confirmation; it does not require the student to enter a password at the desk. Keep this page on the local dojo computer or a deliberately supervised LAN only. Do not expose it to the public internet.

## Student password changes

An authenticated student can open `Student Hub > Profile > Account password` and change their own password. The current password is required, and the new password must be at least 12 characters with uppercase, lowercase, and a number. An Administrator can select a student in `Staff > Students`, choose `Edit profile`, and use `Reset student password`; the old password is never displayed or recoverable, and the reset is recorded in the audit log. Share an administrator-assigned password with the student through a supervised channel.

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
