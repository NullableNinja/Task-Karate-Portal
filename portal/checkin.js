(async () => {
  const root = document.querySelector("[data-checkin-app]");
  if (!root || !window.TaskKarateStore) return;

  const esc = (value) => String(value ?? "").replace(/[&<>"']/g, (char) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;" }[char]));
  const displayName = (student) => student.displayName || student.name;
  const initials = (value) => String(value || "TK").replace(/^(mr|ms|mrs|miss|dr)\.?\s+/i, "").split(/\s+/).map((part) => part[0]).join("").slice(0, 2).toUpperCase();
  const beltKey = (rank) => { const value = String(rank || "").toLowerCase(); return ["white", "yellow", "orange", "green", "blue", "purple", "red", "brown", "black"].find((belt) => value.includes(belt)) || "white"; };
  const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
  const today = new Intl.DateTimeFormat("en-US", { weekday: "long" }).format(new Date());
  const profileHref = window.location.pathname.includes("/portal/") ? "index.html" : "portal/index.html";
  let roster = [], schedule = {}, selectedStudent = null, selectedDay = days.includes(today) ? today : "Monday", selectedClassId = "", query = "";

  try { [roster, schedule] = await Promise.all([window.TaskKarateStore.getRoster(), window.TaskKarateStore.getSchedule()]); } catch (error) { root.innerHTML = '<section class="checkin-error"><h1>Roster unavailable.</h1><p>Start the local desk server or open the site through a local HTTP server, then try again.</p></section>'; console.error(error); return; }
  render();

  function classesFor(student, day = selectedDay) {
    if (!student) return [];
    const rank = String(student.rank || "").toLowerCase();
    const belt = ["white", "yellow", "orange", "green", "blue", "purple", "red", "brown", "black"].find((value) => rank.includes(value)) || "white";
    const keys = student.ageGroup === "kids" ? ["kids", "sparring", "weapons", "special"] : student.ageGroup === "staff" ? Object.keys(schedule) : ["teensAdults", "sparring", "weapons", "special", "eskrima"];
    return keys.flatMap((programKey) => {
      const program = schedule[programKey];
      if (!program?.groups) return [];
      return program.groups.flatMap((group) => {
        if (!group.belts?.includes(belt) && !(student.ageGroup === "staff" && group.belts?.includes("black"))) return [];
        return (group.schedule?.[day] || []).map((entry, index) => ({ id: `${programKey}-${day}-${entry.time}-${group.name}-${index}`, program: program.title, group: group.name, day, time: entry.time, isHour: entry.isHour, note: entry.note || group.specialNote || program.note }));
      });
    });
  }

  function sortedRoster() {
    return roster.filter((student) => `${displayName(student)} ${student.rank}`.toLowerCase().includes(query.toLowerCase())).sort((a, b) => displayName(a).localeCompare(displayName(b)));
  }

  function render() {
    const visible = sortedRoster();
    const groups = visible.reduce((result, student) => { const name = displayName(student).trim().split(/\s+/); const letter = (name[name.length - 1] || name[0] || "?")[0].toUpperCase(); (result[letter] ||= []).push(student); return result; }, {});
    root.innerHTML = `<div class="login-wrapper"><section class="login-card"><header class="login-header"><span class="login-eyebrow">Task Karate / studio desk</span><h1 class="login-title">Enter the dojo.</h1><p class="login-subtitle">Tap your profile to check in, then open your student page only if you want to.</p></header><div class="login-tip"><span>✦</span><div><strong>Tip of the day</strong><p>Be respectful. Do your best. Help the person beside you get better.</p></div></div><div class="login-toolbar"><label class="login-search-wrap"><span class="visually-hidden">Search students</span><input class="login-search" data-roster-search type="search" placeholder="Search students…" value="${esc(query)}"><span aria-hidden="true">⌕</span></label><span class="login-mode-label">${visible.length} students · ${esc(today)}</span></div><div class="login-roster-scroll"><div class="login-grid">${Object.keys(groups).sort().map((letter) => `<div class="letter-header">— ${letter} —</div>${groups[letter].map(renderTile).join("")}`).join("") || '<div class="login-empty"><strong>No match yet.</strong><span>Try a first name, last name, or belt.</span></div>'}</div></div><footer class="login-footer"><span>Supervised studio check-in · persistent on this computer</span><a href="${window.location.pathname.includes("/portal/") ? "../contact.html" : "contact.html"}">Need help?</a></footer></section></div>${renderAccessModal()}`;
    wire(visible);
  }

  function renderTile(student) {
    const selected = selectedStudent?.id === student.id;
    return `<div class="tile-wrapper ${selected ? "is-selected" : ""}"><button class="login-tile" type="button" data-student-id="${esc(student.id)}"><span class="login-tile-avatar" data-belt="${esc(beltKey(student.rank))}">${initials(displayName(student))}</span><span class="login-tile-info"><strong class="login-tile-name">${esc(displayName(student))}</strong><span class="belt-pill" data-belt="${esc(beltKey(student.rank))}">${esc(student.rank)}</span></span><span class="login-tile-arrow">${selected ? "⌄" : "›"}</span></button>${selected ? renderSelected(student) : ""}</div>`;
  }

  function renderSelected(student) {
    const classes = classesFor(student);
    const checked = JSON.parse(sessionStorage.getItem("task-karate-checkins-today") || "[]");
    const checkinKey = selectedClassId || `general-${new Date().toISOString().slice(0, 10)}`;
    const isChecked = checked.includes(checkinKey);
    return `<div class="expanded-panel"><div class="selected-student-meta"><div><span class="selected-label">Selected profile</span><h2>${esc(displayName(student))}</h2><span class="belt-pill" data-belt="${esc(beltKey(student.rank))}">${esc(student.rank)}</span></div><button class="selected-change" type="button" data-clear-student>Change</button></div><div class="selected-facts"><span><small>Belt size</small><strong>${esc(student.beltSize || "Not recorded")}</strong></span><span><small>Uniform size</small><strong>${esc(student.uniformSize || "Not recorded")}</strong></span><span><small>Classes</small><strong>${esc(student.totalClasses || 0)}</strong></span></div><div class="selected-class-row"><label>Class day<select data-day-select aria-label="Class day">${days.map((day) => `<option ${day === selectedDay ? "selected" : ""}>${day}</option>`).join("")}</select></label><div class="selected-classes"><button type="button" class="selected-class selected-class--general ${selectedClassId ? "" : "is-selected"}" data-clear-class><strong>Check-in only</strong><span>No class selection needed</span></button>${classes.length ? classes.map((item) => `<button type="button" class="selected-class ${selectedClassId === item.id ? "is-selected" : ""}" data-class-id="${esc(item.id)}"><strong>${esc(item.time)}</strong><span>${esc(item.program)} · ${esc(item.group)}</span></button>`).join("") : '<span class="selected-no-class">No matching class on this day. Check-in only is available.</span>'}</div></div><div class="selected-actions"><button class="enter-btn-small" type="button" data-checkin>${isChecked ? "Checked in ✓" : selectedClassId ? "Check in for class" : "Check in today"}</button><button class="profile-btn-small" type="button" data-profile-access>Open student profile</button></div><p class="selected-note">Checking in does not open private profile information. A student PIN and acknowledgment are required for the profile.</p></div>`;
  }

  function renderAccessModal() {
    if (!window.__taskKarateAccess) return "";
    const { student, stage, error } = window.__taskKarateAccess;
    if (stage === "waiver") return `<div class="pin-modal-backdrop"><section class="pin-modal" role="dialog" aria-modal="true" aria-labelledby="waiver-title"><span class="login-eyebrow">Profile access / final step</span><h2 id="waiver-title">Before you open ${esc(displayName(student))}’s page.</h2><p>Review this acknowledgment each time the supervised desk opens a student profile.</p><label class="waiver-check"><input type="checkbox" data-waiver-check> I understand that this profile contains student information and should only be viewed by the student, guardian, or authorized staff.</label><p class="pin-warning">This is a local workflow acknowledgment, not a legal waiver or security boundary.</p><div class="pin-actions"><button type="button" class="profile-btn-small" data-close-access>Cancel</button><button type="button" class="enter-btn-small" data-waiver-accept disabled>Accept &amp; open profile</button></div></section></div>`;
    return `<div class="pin-modal-backdrop"><section class="pin-modal" role="dialog" aria-modal="true" aria-labelledby="pin-title"><span class="login-eyebrow">Profile access</span><h2 id="pin-title">Enter your student PIN.</h2><p>Checking in is available without opening the profile. The PIN protects private progress, messages, and family information.</p><form data-pin-form><label for="student-pin">Student PIN</label><input id="student-pin" data-pin-input inputmode="numeric" type="password" autocomplete="off" required><p class="pin-warning">Ask an instructor if you need help with your PIN.</p><p class="pin-error" ${error ? "" : "hidden"}>${esc(error || "")}</p><div class="pin-actions"><button type="button" class="profile-btn-small" data-close-access>Cancel</button><button class="enter-btn-small" type="submit">Continue</button></div></form></section></div>`;
  }

  function wire(visible) {
    const search = root.querySelector("[data-roster-search]");
    search?.addEventListener("input", (event) => {
      query = event.target.value;
      const needle = query.trim().toLowerCase();
      let matches = 0;
      root.querySelectorAll(".tile-wrapper").forEach((wrapper) => {
        const studentText = wrapper.querySelector(".login-tile-name")?.textContent || "";
        const beltText = wrapper.querySelector(".belt-pill")?.textContent || "";
        const visible = !needle || `${studentText} ${beltText}`.toLowerCase().includes(needle);
        wrapper.hidden = !visible;
        if (visible) matches += 1;
      });
      const count = root.querySelector(".login-mode-label");
      if (count) count.textContent = `${matches} students · ${today}`;
    });
    root.querySelectorAll("[data-student-id]").forEach((button) => button.addEventListener("click", () => { selectedStudent = roster.find((student) => student.id === button.dataset.studentId); selectedClassId = classesFor(selectedStudent)[0]?.id || ""; render(); }));
    root.querySelector("[data-clear-student]")?.addEventListener("click", () => { selectedStudent = null; selectedClassId = ""; render(); });
    root.querySelector("[data-day-select]")?.addEventListener("change", (event) => { selectedDay = event.target.value; selectedClassId = classesFor(selectedStudent)[0]?.id || ""; render(); });
    root.querySelector("[data-clear-class]")?.addEventListener("click", () => { selectedClassId = ""; render(); });
    root.querySelectorAll("[data-class-id]").forEach((button) => button.addEventListener("click", () => { selectedClassId = button.dataset.classId; render(); }));
    root.querySelector("[data-checkin]")?.addEventListener("click", async () => { if (!selectedStudent) return; const selected = classesFor(selectedStudent).find((item) => item.id === selectedClassId); const classId = selected?.id || `general-${new Date().toISOString().slice(0, 10)}`; const label = selected ? `${selected.day} · ${selected.time} · ${selected.program} · ${selected.group}` : "General dojo check-in"; const record = await window.TaskKarateStore.checkIn(selectedStudent.id, classId, label); const checkins = JSON.parse(sessionStorage.getItem("task-karate-checkins-today") || "[]"); if (!checkins.includes(record.classId)) checkins.push(record.classId); sessionStorage.setItem("task-karate-checkins-today", JSON.stringify(checkins)); render(); });
    root.querySelector("[data-profile-access]")?.addEventListener("click", () => { window.__taskKarateAccess = { student: selectedStudent, stage: "pin", error: "" }; render(); root.querySelector("[data-pin-input]")?.focus(); });
    root.querySelector("[data-close-access]")?.addEventListener("click", () => { window.__taskKarateAccess = null; render(); });
    root.querySelector("[data-pin-form]")?.addEventListener("submit", (event) => { event.preventDefault(); const pin = event.currentTarget.elements["student-pin"].value; const expected = window.__taskKarateAccess.student.pin || "1234"; if (pin !== expected) { window.__taskKarateAccess.error = "That PIN did not match this profile."; render(); root.querySelector("[data-pin-input]")?.focus(); return; } window.__taskKarateAccess.stage = "waiver"; render(); });
    root.querySelector("[data-waiver-check]")?.addEventListener("change", (event) => { const button = root.querySelector("[data-waiver-accept]"); if (button) button.disabled = !event.target.checked; });
    root.querySelector("[data-waiver-accept]")?.addEventListener("click", () => { if (!root.querySelector("[data-waiver-check]")?.checked) return; sessionStorage.removeItem("task-karate-demo-session"); sessionStorage.setItem("task-karate-student-session", JSON.stringify({ studentId: window.__taskKarateAccess.student.id, accessedAt: new Date().toISOString() })); window.location.href = profileHref; });
  }
})();
