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

## Managing roster records

The local API supports:

- `GET /api/students` — active roster;
- `POST /api/students` — add a student record;
- `PATCH /api/students/{id}` — edit profile fields, including sizes;
- `DELETE /api/students/{id}` — soft-deactivate a student;
- `GET /api/checkins?date=YYYY-MM-DD` — attendance for a date;
- `POST /api/checkins` — record a class check-in.

The next admin pass should put these operations behind a staff-only screen instead of exposing raw API calls. The current repository's `/admin/` page remains a browser demo and is not permission enforcement.
