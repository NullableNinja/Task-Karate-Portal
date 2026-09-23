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

If the Admin account already exists and its local test password must be changed, set the development-only reset value for one startup, then remove it before restarting:

```powershell
$env:BootstrapAdmin__ResetPassword = "Cobra Kai Never Dies!7"
dotnet run --project server/TaskKarate.Api
Remove-Item Env:BootstrapAdmin__ResetPassword
```

The reset value is development-only and is validated by Identity. Do not use the shorter phrase without the final digit, and do not leave the reset variable set for routine launches.

Staff sign-in includes a local-use acknowledgment because this workspace can display student, guardian, attendance, and community records. The acknowledgment is enforced by the API, not merely displayed by the browser.

The staff sign-in form does not persist a “keep me signed in” choice. After credentials are submitted, the required staff-use acknowledgment appears in a confirmation dialog. The resulting cookie is a session-scoped local staff session.

Class times are entered/imported in the database’s normal schedule format but are displayed to users in 12-hour local time. The public schedule collapses same-time, same-dojo belt tracks into one physical class block; the check-in dialog lets staff choose the appropriate underlying track.

Students may choose regular attendance or helper check-in from a current class. Helper status is never trusted from the browser: the API checks the student’s current belt against the class threshold and rejects an under-ranked helper.

## Migrations and database

```powershell
dotnet ef database update --project server\TaskKarate.Api --startup-project server\TaskKarate.Api
dotnet run --project server\TaskKarate.Api --urls http://127.0.0.1:5167
```

The runtime database is `server\TaskKarate.Api\App_Data\task-karate.db`; SQLite may also create `-wal` and `-shm` files. All are ignored. Both the EF/Identity platform tables and the portal/student tables live in this one file. The EF tables use a `platform_*` namespace during the transition so the two historical schemas do not collide.

The development schedule/student seed switches are explicit: set `Seed:ImportLegacySchedules` or `Seed:ImportDemoStudents` to `true` in a local settings/user-secret configuration, and never enable demo import for a production database. Legacy schedules are imported as unverified templates; demo students are synthetic.

## Student schedule and profile database

The student experience reads the same runtime database file as the staff/EF endpoints. During migration, point the API at the old supplied file as an import source:

```powershell
$env:TASK_KARATE_STARTER_DB = "C:\Users\Thoma\OneDrive\Web Design\Task-Karate-School\TaskKarate_Starter.db"
```

On startup, missing legacy tables and rows are imported into `task-karate.db` with conflict-safe inserts. The old file is not used as a second active database and is not modified. Back up both files before first use. After the import has been verified, remove `TASK_KARATE_STARTER_DB` and the development import switches for normal/production launches; the runtime file is then the only database the application reads.

This import is intentionally a one-time transition tool. It does not continuously synchronize two databases. After the first verified migration, the active staff and student screens use the portal tables in the runtime file as their canonical operational model; the `platform_*` tables are retained for Identity and compatibility. Make changes through the API/admin UI, not by editing either SQLite file directly.

To create a local student login without committing a password, set all three variables before starting the API:

The supplied file currently contains the schema but may contain no student or session rows yet. In that case `/schedule` correctly reports that no sessions are published until staff or an explicit development seed creates them; the application does not invent live student records.

For synthetic development data, opt into the repeatable import from the repository archive:

```powershell
$env:TASK_KARATE_IMPORT_STARTER_SCHEDULES = "1"
$env:TASK_KARATE_IMPORT_STARTER_STUDENTS = "1"
$env:TASK_KARATE_DEMO_STUDENT_PIN = "4826"
```

The schedule importer reads `data/schedules.json` into recurring `classes` and `class_schedule` rows, then materializes the next 35 days into `class_sessions`. The student importer reads `data/portal-students.json` into clearly marked demo profiles and rank history. When `TASK_KARATE_DEMO_STUDENT_PIN` is present, it also creates development-only accounts for imported records whose role includes `student`, so the roster sign-in can be exercised. This PIN is never committed and must never be used for a real database. Both import switches are off by default.

```powershell
$env:TASK_KARATE_STUDENT_ID = "1"
$env:TASK_KARATE_STUDENT_USERNAME = "student.demo"
$env:TASK_KARATE_STUDENT_PIN = "4826"
```

The bootstrap account is created once if that student is active and has no account. Credentials are hashed and never written to source control. Remove these variables after the account exists. Open `/schedule` for the live schedule and `/student/login` for the profile hub. The student sign-in lists active, provisioned student accounts alphabetically; selecting a profile opens the same server-verified 4–6 digit PIN prompt used by supervised class check-in, followed by the profile acknowledgment when required.

Do not use a short test phrase such as `8675309` or `Cobra Kai Never Dies!` as the administrator password. The configured Identity policy requires a long password with upper/lowercase characters, a number, and a non-alphanumeric character. For a temporary local test credential, use an environment-only value such as `Cobra Kai Never Dies!7` and remove it after bootstrapping; never commit it or use it for real student records.

`/schedule` is the supervised front-desk schedule. It prioritizes today's in-progress and upcoming classes, keeps future dates view-only, and uses the imported schedule database. A same-origin check-in requires a searched student selection and an explicit confirmation; it does not require the student to enter a password at the desk. Keep this page on the local dojo computer or a deliberately supervised LAN only. Do not expose it to the public internet.

## Student PINs

The public-facing Student Hub uses one shared student credential:

- Students use one server-verified 4–6 digit PIN for both internet Hub sign-in and supervised `/schedule` class check-in.
- The PIN is never returned to the browser or stored in plaintext. Staff can assign a new PIN from `Staff > Students`; students can change their own PIN from `Student Hub > Profile`.

There is no separate student password or forced password-change workflow. An authenticated student can open `Student Hub > Profile` to change the single PIN. An Administrator can select a student in `Staff > Students`, choose `Edit profile`, and assign a new PIN. Existing secrets are never displayed or recoverable, and changes are recorded in the audit log. The public schedule check-in requires a student search, that same PIN, and an explicit confirmation.

The staff student roster is filtered by status and paged at 25 records per page so it remains usable for a 100–250 student school. Active students are eligible for enrollment and attendance. Paused students remain in the student directory and retain their history, but are excluded from attendance pickers and class enrollment choices. Deactivated records remain available under the Deactivated filter for historical administration.

In `Staff > Classes`, the recurring enrollment roster includes an explicit `Remove student from class` action. Removing a recurring enrollment does not erase attendance history. The same page is also where staff can create seminars and appointment-only private-lesson sessions.

`Staff > Helper roster` is the pre-class coverage board. Staff can volunteer themselves for today’s or a future dated session, remove their own signup, and see both staff volunteers and student helpers who were actually checked in. A volunteer signup is not attendance and does not replace the normal check-in process.

## Internet hosting architecture

The website and the portal API/database should not be treated as the same kind of deployment:

- `Task-Karate-Web` can remain a static site hosted on GitHub Pages or another static host.
- `Task-Karate-Portal` needs the ASP.NET Core API running on a server, plus a persistent database and HTTPS. A browser cannot safely open a SQLite file directly, and GitHub Pages or ordinary static Yahoo hosting cannot run the API.
- A mini PC at the dojo is a viable first production server if it has reliable power/internet, automatic updates, a UPS, a firewall, HTTPS reverse proxy, and tested backups. Keep the SQLite file on that machine's local SSD and expose only HTTPS; never expose port 5167 or the database file directly.
- SQLite is appropriate for one API process on one carefully managed server with backups. If usage grows to multiple API instances, frequent concurrent writes, or high availability requirements, migrate the same logical data to PostgreSQL or MySQL/MariaDB and keep the database on a private network.

### Mini-PC production checklist

1. Install a supported Windows or Ubuntu release, the .NET 8 ASP.NET Core runtime, and the production build of the SvelteKit app.
2. Copy the verified shared database to a local, non-synchronized data directory on the mini PC. Do not run it from OneDrive, Dropbox, a NAS share, or a web-hosting directory.
3. Run the API as a service (Windows Service/IIS or systemd) under a restricted account. Store production secrets outside Git and outside the static site.
4. Put IIS, Caddy, or Nginx in front of the API and frontend. Terminate TLS on port 443 and proxy internally to the API; do not port-forward the development port.
5. Use a subdomain such as `portal.taskkarateschool.com`. Keep the domain/DNS with the current registrar/host if it supports DNS records, or move only DNS to a provider that does.
6. Allow only 80/443 at the router, restrict staff-only operations through authentication and role checks, and keep the supervised schedule/check-in workflow protected.
7. Configure nightly encrypted backups, retain multiple generations, and test restoring a copy on a separate machine before calling the deployment production-ready.
8. Monitor disk space, service health, certificate expiry, failed logins, and backup success. Document a manual fallback for class attendance if the mini PC or internet connection is unavailable.

Do not expose the SQLite file, the staff check-in route, or database credentials as public web files. Production deployment also requires HTTPS, secure cookie settings, restricted CORS, secret management, backups, account lockout/rate limiting, and a plan for safely issuing replacement PINs.

## Backup and restore

Each day, while the API is stopped (or after a SQLite checkpoint), copy the one shared database to a separate encrypted drive/location. Include the `-wal` and `-shm` files if they exist, or use SQLite's backup API/checkpoint process so the copy is consistent:

```powershell
$source = "server\TaskKarate.Api\App_Data\task-karate.db"
$destination = "E:\EncryptedBackups\TaskKarate\task-karate-$(Get-Date -Format yyyy-MM-dd).db"
Copy-Item $source $destination
```

Do not back up the old `TASK_KARATE_STARTER_DB` source as if it were still live. Keep the pre-migration copy only as a recovery/archive artifact until the cutover has been accepted.

Use BitLocker or an equivalent encrypted location, restrict access, and retain several dated copies. To test restore, copy a backup to a temporary path, change `ConnectionStrings:TaskKarate` in a local ignored settings file to that path, start the API with test credentials, sign in, and verify student/session/attendance counts. Never test a restore by overwriting the live database.

## Supervised LAN use

Default local launch binds to loopback. For a supervised staff computer, explicitly choose a private LAN address (for example `http://192.168.1.20:5167`), add the exact SvelteKit origin to `AllowedOrigins`, use HTTPS with a trusted local certificate where possible, restrict Windows Firewall to the private profile and staff subnet, and stop the process when finished. Do not port-forward or expose this application to the internet. A LAN bind is deliberately not the default.
