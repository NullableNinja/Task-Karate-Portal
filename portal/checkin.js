(async () => {
  const root = document.querySelector("[data-checkin-app]");
  if (!root || !window.TaskKarateStore) return;
  const esc = (value) => String(value ?? "").replace(/[&<>"']/g, (char) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;" }[char]));
  const beltKey = (rank) => { const value = String(rank || "").toLowerCase(); return ["white", "yellow", "orange", "green", "blue", "purple", "red", "brown", "black"].find((belt) => value.includes(belt)) || "white"; };
  const displayName = (student) => student.displayName || student.name;
  const initials = (value) => String(value || "TK").replace(/^(mr|ms|mrs|miss|dr)\.?\s+/i, "").split(/\s+/).map((part) => part[0]).join("").slice(0, 2).toUpperCase();
  const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
  const today = new Intl.DateTimeFormat("en-US", { weekday: "long" }).format(new Date());
  let roster = [], schedule = {}, selectedStudent = null, selectedDay = days.includes(today) ? today : "Monday", query = "", selectedClassId = "";
  const profileHref = window.location.pathname.includes("/portal/") ? "index.html" : "portal/index.html";

  try { [roster, schedule] = await Promise.all([window.TaskKarateStore.getRoster(), window.TaskKarateStore.getSchedule()]); } catch (error) { root.innerHTML = '<section class="checkin-error"><h1>Roster unavailable.</h1><p>Start the local desk server or open the site through a local HTTP server, then try again.</p></section>'; console.error(error); return; }
  render();

  function classesFor(student, day = selectedDay) {
    if (!student) return [];
    const belt = beltKey(student.rank);
    const keys = student.ageGroup === "kids" ? ["kids", "sparring", "weapons", "special"] : student.ageGroup === "staff" ? Object.keys(schedule) : ["teensAdults", "sparring", "weapons", "special", "eskrima"];
    return keys.flatMap((programKey) => {
      const program = schedule[programKey];
      if (!program?.groups) return [];
      return program.groups.flatMap((group) => {
        if (!group.belts?.includes(belt) && !(student.ageGroup === "staff" && group.belts?.includes("black"))) return [];
        return (group.schedule?.[day] || []).map((entry, index) => ({ id: `${programKey}-${day}-${entry.time}-${group.name}-${index}`, programKey, program: program.title, group: group.name, day, time: entry.time, isHour: entry.isHour, note: entry.note || group.specialNote || program.note }));
      });
    });
  }

  function render() {
    const visible = roster.filter((student) => displayName(student).toLowerCase().includes(query.toLowerCase()) || student.rank.toLowerCase().includes(query.toLowerCase())).sort((a, b) => displayName(a).localeCompare(displayName(b)));
    root.innerHTML = `<div class="checkin-shell"><header class="checkin-header"><div><span class="checkin-kicker">Task Karate / supervised desk check-in</span><h1>Enter the dojo.</h1><p>Search your name, choose today’s class, and check in. Your profile is optional.</p></div><div class="checkin-header-actions"><a class="portal-button portal-button--outline" href="../index.html">Public site ↗</a><a class="portal-button portal-button--ghost" href="../admin/index.html">Staff tools</a></div></header><div class="checkin-boundary"><strong>Desk mode.</strong> This roster is designed for the studio check-in computer. It is not a public password login; production access and privacy controls still belong on a real backend.</div><div class="checkin-layout"><section class="checkin-roster"><div class="checkin-section-heading"><div><span class="checkin-kicker">1 / Find your name</span><h2>Who’s training today?</h2></div><span class="checkin-count">${visible.length} people</span></div><label class="checkin-search"><span class="visually-hidden">Search students</span><input data-roster-search type="search" placeholder="Search by first name, last name, or belt…" value="${esc(query)}"><span>⌕</span></label><div class="checkin-roster-scroll">${visible.length ? visible.map((student) => `<button class="roster-student ${selectedStudent?.id === student.id ? "is-selected" : ""}" type="button" data-student-id="${esc(student.id)}"><span class="roster-avatar" data-belt="${esc(beltKey(student.rank))}">${initials(displayName(student))}</span><span><strong>${esc(displayName(student))}</strong><small>${esc(student.rank)} · ${esc(student.ageGroup === "kids" ? "Kids" : student.ageGroup === "staff" ? "Staff" : "Teens & Adults")}</small></span><span class="roster-chevron">→</span></button>`).join("") : '<div class="checkin-empty"><strong>No match yet.</strong><span>Try a first name, last name, or belt.</span></div>'}</div></section><section class="checkin-action-panel">${selectedStudent ? renderSelected() : '<div class="checkin-placeholder"><span class="checkin-placeholder-mark">TK</span><span class="checkin-kicker">2 / Choose a student</span><h2>Pick your name to see your classes.</h2><p>Your rank, age group, belt size, uniform size, and eligible schedule stay attached to your profile.</p></div>'}</section></div><footer class="checkin-footer"><span>Need help? Ask an instructor at the desk.</span><a href="../contact.html">Contact the school</a><span class="checkin-persistence">Local persistence is enabled on this computer. SQLite sync is used when the desk server is running.</span></footer></div>`;
    wire(visible);
  }

  function renderSelected() {
    const classes = classesFor(selectedStudent);
    const currentCheckins = JSON.parse(sessionStorage.getItem("task-karate-checkins-today") || "[]");
    const isChecked = currentCheckins.includes(selectedClassId);
    return `<div class="selected-student-head"><div class="selected-avatar" data-belt="${esc(beltKey(selectedStudent.rank))}">${initials(displayName(selectedStudent))}</div><div><span class="checkin-kicker">Selected student</span><h2>${esc(displayName(selectedStudent))}</h2><span class="belt-chip" data-belt="${esc(beltKey(selectedStudent.rank))}">${esc(selectedStudent.rank)}</span></div><button type="button" class="checkin-clear" data-clear-student>Change</button></div><div class="profile-facts"><div><span>Belt size</span><strong>${esc(selectedStudent.beltSize || "Not recorded")}</strong></div><div><span>Uniform size</span><strong>${esc(selectedStudent.uniformSize || "Not recorded")}</strong></div><div><span>Classes</span><strong>${esc(selectedStudent.totalClasses || 0)}</strong></div></div><div class="checkin-step"><div class="checkin-section-heading"><div><span class="checkin-kicker">2 / Pick a class</span><h3>Today’s schedule.</h3></div><select data-day-select aria-label="Schedule day">${days.map((day) => `<option ${day === selectedDay ? "selected" : ""}>${day}</option>`).join("")}</select></div><div class="class-pills">${classes.length ? classes.map((item) => `<button type="button" class="class-pill ${selectedClassId === item.id ? "is-selected" : ""}" data-class-id="${esc(item.id)}"><span>${esc(item.time)}</span><strong>${esc(item.program)}</strong><small>${esc(item.group)}${item.isHour ? " · 1 hour" : " · 45 min"}</small></button>`).join("") : `<div class="checkin-empty"><strong>No eligible classes on ${esc(selectedDay)}.</strong><span>Choose another day or ask the desk about a rotating session.</span></div>`}</div></div><div class="checkin-step checkin-submit-step"><span class="checkin-kicker">3 / Confirm</span><p>${selectedClassId ? "You’re checking in for the selected class." : "Choose a class above to enable check-in."}</p><button class="portal-button portal-button--yellow checkin-submit" type="button" data-checkin ${!selectedClassId ? "disabled" : ""}>${isChecked ? "Checked in ✓" : "Check in for class ↗"}</button>${isChecked ? `<a class="portal-button portal-button--blue" href="${profileHref}">View my profile ↗</a>` : ""}<p class="checkin-smallprint">Checking in records today’s attendance on this desk only. Staff can correct mistakes.</p></div>`;
  }

  function wire(visible) {
    root.querySelector("[data-roster-search]")?.addEventListener("input", (event) => { query = event.target.value; render(); root.querySelector("[data-roster-search]")?.focus(); });
    root.querySelectorAll("[data-student-id]").forEach((button) => button.addEventListener("click", () => { selectedStudent = roster.find((student) => student.id === button.dataset.studentId); selectedClassId = classesFor(selectedStudent)[0]?.id || ""; render(); }));
    root.querySelector("[data-clear-student]")?.addEventListener("click", () => { selectedStudent = null; selectedClassId = ""; render(); });
    root.querySelector("[data-day-select]")?.addEventListener("change", (event) => { selectedDay = event.target.value; selectedClassId = classesFor(selectedStudent)[0]?.id || ""; render(); });
    root.querySelectorAll("[data-class-id]").forEach((button) => button.addEventListener("click", () => { selectedClassId = button.dataset.classId; render(); }));
    root.querySelector("[data-checkin]")?.addEventListener("click", async () => { if (!selectedStudent || !selectedClassId) return; const selected = classesFor(selectedStudent).find((item) => item.id === selectedClassId); const record = await window.TaskKarateStore.checkIn(selectedStudent.id, selected.id, `${selected.day} · ${selected.time} · ${selected.program} · ${selected.group}`); const checkins = JSON.parse(sessionStorage.getItem("task-karate-checkins-today") || "[]"); if (!checkins.includes(record.classId)) checkins.push(record.classId); sessionStorage.setItem("task-karate-checkins-today", JSON.stringify(checkins)); sessionStorage.setItem("task-karate-student-session", JSON.stringify({ studentId: selectedStudent.id, checkedInAt: record.checkedInAt, classId: record.classId })); render(); });
  }
})();
