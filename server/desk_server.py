"""Local Task Karate desk server.

Run on the supervised check-in computer with: python server/desk_server.py
It serves this static site and stores roster/check-in records in SQLite.
It binds only to localhost; it is not an internet-facing production backend.
"""
from __future__ import annotations

import json
import sqlite3
from datetime import datetime, timezone
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import parse_qs, unquote, urlparse

ROOT = Path(__file__).resolve().parents[1]
DB_PATH = ROOT / "data" / "task-karate-desk.sqlite3"


def now_iso() -> str:
    return datetime.now(timezone.utc).isoformat()


def connect() -> sqlite3.Connection:
    db = sqlite3.connect(DB_PATH)
    db.row_factory = sqlite3.Row
    return db


def seed_database() -> None:
    DB_PATH.parent.mkdir(parents=True, exist_ok=True)
    roster = json.loads((ROOT / "data" / "portal-students.json").read_text(encoding="utf-8"))["students"]
    with connect() as db:
        db.executescript("""
        CREATE TABLE IF NOT EXISTS students (
          id TEXT PRIMARY KEY, name TEXT NOT NULL, display_name TEXT, rank TEXT NOT NULL,
          rank_type TEXT, age_group TEXT NOT NULL DEFAULT 'teensAdults', belt_size TEXT,
          uniform_size TEXT, join_date TEXT, active INTEGER NOT NULL DEFAULT 1, updated_at TEXT NOT NULL
        );
        CREATE TABLE IF NOT EXISTS checkins (
          id TEXT PRIMARY KEY, student_id TEXT NOT NULL REFERENCES students(id), class_id TEXT NOT NULL,
          class_label TEXT NOT NULL, date TEXT NOT NULL, checked_in_at TEXT NOT NULL,
          UNIQUE(student_id, class_id, date)
        );
        """)
        if db.execute("SELECT COUNT(*) AS count FROM students").fetchone()["count"] == 0:
            for student in roster:
                db.execute("""INSERT INTO students
                  (id, name, display_name, rank, rank_type, age_group, belt_size, uniform_size, join_date, updated_at)
                  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)""", (
                    student["id"], student["name"], student.get("displayName"), student["rank"],
                    student.get("rankType"), student.get("ageGroup", "teensAdults"), student.get("beltSize"),
                    student.get("uniformSize"), student.get("joinDate"), now_iso()))


def student_payload(row: sqlite3.Row) -> dict:
    return {"id": row["id"], "name": row["name"], "displayName": row["display_name"], "rank": row["rank"],
            "rankType": row["rank_type"], "ageGroup": row["age_group"], "beltSize": row["belt_size"],
            "uniformSize": row["uniform_size"], "joinDate": row["join_date"], "active": bool(row["active"])}


class DeskHandler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(ROOT), **kwargs)

    def json_response(self, payload: dict, status: int = 200) -> None:
        body = json.dumps(payload).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Cache-Control", "no-store")
        self.send_header("Content-Length", str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def read_json(self) -> dict:
        length = int(self.headers.get("Content-Length", "0"))
        return json.loads(self.rfile.read(length) or b"{}")

    def do_GET(self) -> None:  # noqa: N802
        parsed = urlparse(self.path)
        if parsed.path == "/api/health":
            self.json_response({"ok": True, "storage": "sqlite", "database": DB_PATH.name})
            return
        if parsed.path == "/api/students":
            with connect() as db:
                rows = db.execute("SELECT * FROM students WHERE active = 1 ORDER BY name COLLATE NOCASE").fetchall()
            self.json_response({"students": [student_payload(row) for row in rows]})
            return
        if parsed.path == "/api/checkins":
            date = parse_qs(parsed.query).get("date", [datetime.now().date().isoformat()])[0]
            with connect() as db:
                rows = db.execute("SELECT * FROM checkins WHERE date = ? ORDER BY checked_in_at", (date,)).fetchall()
            self.json_response({"checkins": [dict(row) for row in rows]})
            return
        super().do_GET()

    def do_POST(self) -> None:  # noqa: N802
        parsed = urlparse(self.path)
        payload = self.read_json()
        if parsed.path == "/api/checkins":
            record = {"id": payload.get("id") or f"checkin-{int(datetime.now().timestamp() * 1000)}",
                      "student_id": payload.get("studentId"), "class_id": payload.get("classId"),
                      "class_label": payload.get("classLabel") or "Class check-in",
                      "date": payload.get("date") or datetime.now().date().isoformat(),
                      "checked_in_at": payload.get("checkedInAt") or now_iso()}
            if not record["student_id"] or not record["class_id"]:
                self.json_response({"error": "studentId and classId are required"}, 400)
                return
            with connect() as db:
                db.execute("""INSERT OR IGNORE INTO checkins
                  (id, student_id, class_id, class_label, date, checked_in_at)
                  VALUES (:id, :student_id, :class_id, :class_label, :date, :checked_in_at)""", record)
                row = db.execute("SELECT * FROM checkins WHERE student_id = ? AND class_id = ? AND date = ?",
                                 (record["student_id"], record["class_id"], record["date"])).fetchone()
            self.json_response({"checkin": dict(row)}, 201)
            return
        if parsed.path == "/api/students":
            if any(not payload.get(key) for key in ("id", "name", "rank")):
                self.json_response({"error": "id, name, and rank are required"}, 400)
                return
            with connect() as db:
                db.execute("""INSERT INTO students
                  (id, name, display_name, rank, rank_type, age_group, belt_size, uniform_size, join_date, updated_at)
                  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)""", (payload["id"], payload["name"], payload.get("displayName"),
                  payload["rank"], payload.get("rankType"), payload.get("ageGroup", "teensAdults"), payload.get("beltSize"),
                  payload.get("uniformSize"), payload.get("joinDate"), now_iso()))
                row = db.execute("SELECT * FROM students WHERE id = ?", (payload["id"],)).fetchone()
            self.json_response({"student": student_payload(row)}, 201)
            return
        self.json_response({"error": "Not found"}, 404)

    def do_PATCH(self) -> None:  # noqa: N802
        prefix = "/api/students/"
        if not self.path.startswith(prefix):
            self.json_response({"error": "Not found"}, 404)
            return
        student_id = unquote(self.path[len(prefix):])
        payload = self.read_json()
        columns = {"name": "name", "displayName": "display_name", "rank": "rank", "rankType": "rank_type",
                   "ageGroup": "age_group", "beltSize": "belt_size", "uniformSize": "uniform_size", "joinDate": "join_date"}
        updates = [(column, payload[key]) for key, column in columns.items() if key in payload]
        if not updates:
            self.json_response({"error": "No editable fields supplied"}, 400)
            return
        sql = ", ".join(f"{column} = ?" for column, _ in updates)
        with connect() as db:
            db.execute(f"UPDATE students SET {sql}, updated_at = ? WHERE id = ?", [value for _, value in updates] + [now_iso(), student_id])
            row = db.execute("SELECT * FROM students WHERE id = ?", (student_id,)).fetchone()
        if not row:
            self.json_response({"error": "Student not found"}, 404)
            return
        self.json_response({"student": student_payload(row)})

    def do_DELETE(self) -> None:  # noqa: N802
        prefix = "/api/students/"
        if not self.path.startswith(prefix):
            self.json_response({"error": "Not found"}, 404)
            return
        student_id = unquote(self.path[len(prefix):])
        with connect() as db:
            db.execute("UPDATE students SET active = 0, updated_at = ? WHERE id = ?", (now_iso(), student_id))
        self.json_response({"deleted": student_id})

    def log_message(self, format: str, *args) -> None:
        print(f"[desk] {self.address_string()} - {format % args}")


if __name__ == "__main__":
    seed_database()
    server = ThreadingHTTPServer(("127.0.0.1", 4173), DeskHandler)
    print(f"Task Karate desk server: http://127.0.0.1:4173/ (SQLite: {DB_PATH})")
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nStopping desk server.")
    finally:
        server.server_close()
