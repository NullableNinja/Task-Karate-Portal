# Local desk hosting and persistence

## What a browser can and cannot do

A browser cannot safely open an arbitrary `.db` file and write to it directly. A static GitHub Pages site also cannot receive writes. The practical desk setup is a small local service running on the check-in computer:

1. `server/desk_server.py` serves the website on `127.0.0.1:4173`.
2. Python's built-in `sqlite3` module creates `data/task-karate-desk.sqlite3` on first run.
3. The check-in screen uses `/api/students` and `/api/checkins` when that service is available.
4. The same screen falls back to IndexedDB when the site is running without the service, so the kiosk still has browser-local persistence.

The SQLite file is intentionally ignored by Git. It should stay on the desk computer and should be backed up securely by the school rather than committed to the public repository.

## Start the desk service

From the project folder:

```powershell
python server/desk_server.py
```

Open `http://127.0.0.1:4173/checkin.html` on the desk computer. The service seeds its roster from `data/portal-students.json` only on first run. After that, roster changes belong to SQLite.

The service currently persists:

- active/inactive roster records;
- student name, display name, rank, age group, belt size, uniform size, and join date;
- dated class check-ins with class identity and timestamp.

The front-end still labels the service as a desk adapter, not a secure internet backend. Do not expose this server to the public network. Before handling sensitive student data, add authentication, encrypted backups, access logging, role checks, retention/deletion rules, and a school-approved privacy policy.

## Desk workflow and local gates

The check-in screen follows the historical SvelteKit roster flow: search for a student, select the student, optionally choose a class, and check in. Opening the private student portal is a separate action. It requires the student's local demo PIN and a fresh profile-access acknowledgment each time. Seed records use `1234` only as a frontend demonstration value; this is not suitable for production authentication.

Staff management is available at `/admin/index.html`. The local prototype gate uses `8675309`, matching the historical instructor-demo PIN. This is only a convenience boundary for a supervised local prototype. It is not a secure staff login and must be replaced with server-side authentication and authorization before real student records are used.

## Managing roster records

The local API supports:

- `GET /api/students` — active roster;
- `POST /api/students` — add a student record;
- `PATCH /api/students/{id}` — edit profile fields, including sizes;
- `DELETE /api/students/{id}` — soft-deactivate a student;
- `GET /api/checkins?date=YYYY-MM-DD` — attendance for a date;
- `POST /api/checkins` — record a class check-in.

The `/admin/` page provides the browser workflow for these operations, including adding records, editing belt/uniform sizes, and deactivating records. When the SQLite service is running, the page writes through to the desk API; otherwise changes remain in that browser's IndexedDB. The page's PIN is not permission enforcement.
