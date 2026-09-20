/*
 * Desk-friendly persistence adapter.
 *
 * If the optional local desk server is running, writes go to its SQLite-backed
 * API. On static hosting, the same UI falls back to IndexedDB so the supervised
 * check-in computer still keeps its roster/check-ins between browser sessions.
 * Neither mode is a substitute for a hosted, authenticated production backend.
 */
(function () {
  const DB_NAME = "task-karate-desk";
  const STORE_NAME = "records";
  const apiBase = new URL("../api/", window.location.href).toString();
  let apiAvailable;

  function openDb() {
    return new Promise((resolve, reject) => {
      const request = indexedDB.open(DB_NAME, 1);
      request.onupgradeneeded = () => request.result.createObjectStore(STORE_NAME);
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
  }

  async function localGet(key, fallback = null) {
    try {
      const db = await openDb();
      return await new Promise((resolve, reject) => {
        const request = db.transaction(STORE_NAME, "readonly").objectStore(STORE_NAME).get(key);
        request.onsuccess = () => resolve(request.result ?? fallback);
        request.onerror = () => reject(request.error);
      });
    } catch {
      try { return JSON.parse(localStorage.getItem(key)) ?? fallback; } catch { return fallback; }
    }
  }

  async function localSet(key, value) {
    try {
      const db = await openDb();
      await new Promise((resolve, reject) => {
        const request = db.transaction(STORE_NAME, "readwrite").objectStore(STORE_NAME).put(value, key);
        request.onsuccess = resolve;
        request.onerror = () => reject(request.error);
      });
    } catch {
      try { localStorage.setItem(key, JSON.stringify(value)); } catch { /* optional demo persistence */ }
    }
    return value;
  }

  async function api(path, options = {}) {
    if (apiAvailable === false) return null;
    try {
      const response = await fetch(`${apiBase}${path}`, { headers: { "Content-Type": "application/json" }, ...options });
      if (!response.ok) { apiAvailable = false; return null; }
      apiAvailable = true;
      return response.status === 204 ? true : response.json();
    } catch {
      apiAvailable = false;
      return null;
    }
  }

  window.TaskKarateStore = {
    async get(key, fallback = null) { return localGet(key, fallback); },
    async set(key, value) { return localSet(key, value); },
    async getRoster() {
      const remote = await api("students");
      if (remote?.students) return remote.students;
      const response = await fetch("../data/portal-students.json", { cache: "no-store" });
      const local = await response.json();
      const overrides = await localGet("roster-overrides", {});
      const additions = await localGet("roster-additions", []);
      return [...(local.students || []), ...additions].map((student) => ({ ...student, ...(overrides[student.id] || {}) })).filter((student) => student.active !== false);
    },
    async getSchedule() {
      const response = await fetch("../data/schedules.json", { cache: "no-store" });
      return response.json();
    },
    async getCheckins(date = new Date().toISOString().slice(0, 10)) {
      const remote = await api(`checkins?date=${encodeURIComponent(date)}`);
      if (remote?.checkins) return remote.checkins;
      const records = await localGet("checkins", []);
      return records.filter((record) => record.date === date);
    },
    async checkIn(studentId, classId, classLabel) {
      const record = { id: `checkin-${Date.now()}`, studentId, classId, classLabel, date: new Date().toISOString().slice(0, 10), checkedInAt: new Date().toISOString() };
      const remote = await api("checkins", { method: "POST", body: JSON.stringify(record) });
      if (remote?.checkin) return remote.checkin;
      const records = await localGet("checkins", []);
      const exists = records.find((item) => item.studentId === studentId && item.classId === classId && item.date === record.date);
      if (exists) return exists;
      records.push(record);
      await localSet("checkins", records);
      return record;
    },
    async saveRosterOverride(studentId, changes) {
      const overrides = await localGet("roster-overrides", {});
      overrides[studentId] = { ...(overrides[studentId] || {}), ...changes };
      const remote = await api(`students/${encodeURIComponent(studentId)}`, { method: "PATCH", body: JSON.stringify(changes) });
      if (!remote) await localSet("roster-overrides", overrides);
      return changes;
    },
    async addStudent(student) {
      const remote = await api("students", { method: "POST", body: JSON.stringify(student) });
      if (remote?.student) return remote.student;
      const additions = await localGet("roster-additions", []);
      additions.push({ ...student, roles: ["student"], badges: [], buddies: [], assignments: [], practice: [], messages: [], goldStars: [] });
      await localSet("roster-additions", additions);
      return student;
    },
    async deactivateStudent(studentId) {
      const remote = await api(`students/${encodeURIComponent(studentId)}`, { method: "DELETE" });
      if (remote) return remote;
      return this.saveRosterOverride(studentId, { active: false });
    },
    async serverMode() { await api("health"); return apiAvailable === true; }
  };
})();
