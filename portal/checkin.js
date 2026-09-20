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
  const avatarPalette = ["#93c5fd", "#a5f3fc", "#fda4af", "#fcd34d", "#bbf7d0", "#c7d2fe"];
  const tilePalette = ["#1a2232", "#1c2638", "#1d293d", "#202c42", "#1b2635", "#1e2b44"];

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
    const lastName = (student) => displayName(student).trim().split(/\s+/).pop() || "";
    return roster.filter((student) => `${displayName(student)} ${student.rank}`.toLowerCase().includes(query.toLowerCase())).sort((a, b) => lastName(a).localeCompare(lastName(b)) || displayName(a).localeCompare(displayName(b)));
  }

  function renderLegacy() {
    const visible = sortedRoster();
    const groups = visible.reduce((result, student) => { const name = displayName(student).trim().split(/\s+/); const letter = (name[name.length - 1] || name[0] || "?")[0].toUpperCase(); (result[letter] ||= []).push(student); return result; }, {});
    root.innerHTML = `<div class="login-wrapper"><section class="login-card"><header class="login-header"><span class="login-eyebrow">Task Karate / studio desk</span><h1 class="login-title">Enter the dojo.</h1><p class="login-subtitle">Tap your profile to check in, then open your student page only if you want to.</p></header><div class="login-tip"><span>✦</span><div><strong>Tip of the day</strong><p>Be respectful. Do your best. Help the person beside you get better.</p></div></div><div class="login-toolbar"><label class="login-search-wrap"><span class="visually-hidden">Search students</span><input class="login-search" data-roster-search type="search" placeholder="Search students…" value="${esc(query)}"><span aria-hidden="true">⌕</span></label><span class="login-mode-label">${visible.length} students · ${esc(today)}</span></div><div class="login-roster-scroll"><div class="login-grid">${Object.keys(groups).sort().map((letter) => `<div class="letter-header">— ${letter} —</div>${groups[letter].map(renderTile).join("")}`).join("") || '<div class="login-empty"><strong>No match yet.</strong><span>Try a first name, last name, or belt.</span></div>'}</div></div><footer class="login-footer"><span>Supervised studio check-in · persistent on this computer</span><a href="${window.location.pathname.includes("/portal/") ? "../contact.html" : "contact.html"}">Need help?</a></footer></section></div>${renderAccessModal()}`;
    wire(visible);
  }

  function renderTileLegacy(student) {
    const selected = selectedStudent?.id === student.id;
    return `<div class="tile-wrapper ${selected ? "is-selected" : ""}"><button class="login-tile" type="button" data-student-id="${esc(student.id)}"><span class="login-tile-avatar" data-belt="${esc(beltKey(student.rank))}">${initials(displayName(student))}</span><span class="login-tile-info"><strong class="login-tile-name">${esc(displayName(student))}</strong><span class="belt-pill" data-belt="${esc(beltKey(student.rank))}">${esc(student.rank)}</span></span><span class="login-tile-arrow">${selected ? "⌄" : "›"}</span></button>${selected ? renderSelected(student) : ""}</div>`;
  }

  function render() {
    const visible = sortedRoster();
    const groups = visible.reduce((result, student) => { const name = displayName(student).trim().split(/\s+/); const letter = (name[name.length - 1] || name[0] || "?")[0].toUpperCase(); (result[letter] ||= []).push(student); return result; }, {});
    root.innerHTML = `<div class="login-wrapper"><section class="login-card"><header class="login-header"><h1 class="login-title">Enter the Dojo</h1><p class="login-subtitle">Tap your profile to begin your training journey.</p></header><div class="login-toolbar"><label class="login-search-wrap"><span class="visually-hidden">Search students</span><input class="login-search" data-roster-search type="search" placeholder="Search students…" value="${esc(query)}"></label><button class="instructor-switch" type="button" data-instructor-toggle aria-label="Open instructor login"><span class="instructor-toggle"><span class="instructor-thumb"></span></span><span class="instructor-label">Instructor Login</span></button></div><p class="login-tip">Be the student your belt color believes you are.</p><div class="login-roster-scroll"><div class="login-grid">${Object.keys(groups).sort().map((letter) => `<div class="letter-header">— ${letter} —</div>${groups[letter].map((student, index) => renderTile(student, index)).join("")}`).join("") || '<div class="login-empty"><strong>No match yet.</strong><span>Try a first name, last name, or belt.</span></div>'}</div></div></section></div>${renderAccessModal()}${renderStaffModal()}`;
    wire(visible);
  }

  function renderTile(student, index) {
    const selected = selectedStudent?.id === student.id;
    return `<div class="tile-wrapper ${selected ? "is-selected" : ""}"><button class="login-tile" type="button" data-student-id="${esc(student.id)}" style="--tile-bg:${tilePalette[index % tilePalette.length]}"><span class="login-tile-avatar" style="background:${avatarPalette[index % avatarPalette.length]}">${initials(displayName(student))}</span><span class="login-tile-info"><strong class="login-tile-name">${esc(displayName(student))}</strong><span class="belt-pill" data-belt="${esc(beltKey(student.rank))}">${esc(student.rank)}</span></span></button>${selected ? renderSelected(student) : ""}</div>`;
  }

  function renderStaffModal() {
    const access = window.__taskKarateStaffAccess;
    if (!access) return "";
    return `<div class="pin-modal-backdrop"><section class="pin-modal" role="dialog" aria-modal="true" aria-labelledby="staff-pin-title"><span class="login-eyebrow">Instructor access</span><h2 id="staff-pin-title">Instructor Login</h2><p>Staff tools are protected by a separate PIN.</p><form data-staff-pin-form><label for="staff-pin">Instructor PIN</label><input id="staff-pin" data-staff-pin-input inputmode="numeric" type="password" autocomplete="off" required><p class="pin-error" ${access.error ? "" : "hidden"}>${esc(access.error || "")}</p><div class="pin-actions"><button type="button" class="profile-btn-small" data-close-staff>Cancel</button><button class="enter-btn-small" type="submit">Continue</button></div></form></section></div>`;
  }

  function renderSelected(student) {
    return `<div class="expanded-panel"><button class="enter-dojo-button" type="button" data-profile-access>Enter Dojo</button><button class="selected-change" type="button" data-clear-student>Change</button></div>`;
  }

  function renderAccessModal() {
    if (!window.__taskKarateAccess) return "";
    const { student, stage, error } = window.__taskKarateAccess;
    if (stage === "waiver") return `<div class="pin-modal-backdrop"><section class="pin-modal" role="dialog" aria-modal="true" aria-labelledby="waiver-title"><span class="login-eyebrow">Profile access / final step</span><h2 id="waiver-title">Before you open ${esc(displayName(student))}’s page.</h2><p>Review this acknowledgment each time the supervised desk opens a student profile.</p><label class="waiver-check"><input type="checkbox" data-waiver-check> I understand that this profile contains student information and should only be viewed by the student, guardian, or authorized staff.</label><p class="pin-warning">This is a local workflow acknowledgment, not a legal waiver or security boundary.</p><div class="pin-actions"><button type="button" class="profile-btn-small" data-close-access>Cancel</button><button type="button" class="enter-btn-small" data-waiver-accept disabled>Accept &amp; open profile</button></div></section></div>`;
    return `<div class="pin-modal-backdrop"><section class="pin-modal pin-modal--student" role="dialog" aria-modal="true" aria-labelledby="pin-title"><div class="pin-modal-avatar" style="--avatar-belt:${esc(beltKey(student.rank))}">${initials(displayName(student))}</div><h2 id="pin-title">Enter Your PIN</h2><span class="pin-modal-belt" data-belt="${esc(beltKey(student.rank))}">${esc(student.rank)}</span><form data-pin-form><label for="student-pin">PIN</label><input id="student-pin" data-pin-input inputmode="numeric" type="password" autocomplete="off" placeholder="Enter PIN…" required><p class="pin-error" ${error ? "" : "hidden"}>${esc(error || "")}</p><div class="pin-actions"><button type="button" class="profile-btn-small" data-close-access>Cancel</button><button class="enter-btn-small" type="submit">OK</button></div></form></section></div>`;
  }

  function wire(visible) {
    const search = root.querySelector("[data-roster-search]");
    root.querySelector("[data-instructor-toggle]")?.addEventListener("click", () => { window.__taskKarateStaffAccess = { error: "" }; render(); root.querySelector("[data-staff-pin-input]")?.focus(); });
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
    root.querySelectorAll("[data-student-id]").forEach((button) => button.addEventListener("click", () => { selectedStudent = roster.find((student) => student.id === button.dataset.studentId); selectedClassId = ""; render(); }));
    root.querySelector("[data-clear-student]")?.addEventListener("click", () => { selectedStudent = null; selectedClassId = ""; render(); });
    root.querySelector("[data-day-select]")?.addEventListener("change", (event) => { selectedDay = event.target.value; selectedClassId = classesFor(selectedStudent)[0]?.id || ""; render(); });
    root.querySelector("[data-clear-class]")?.addEventListener("click", () => { selectedClassId = ""; render(); });
    root.querySelectorAll("[data-class-id]").forEach((button) => button.addEventListener("click", () => { selectedClassId = button.dataset.classId; render(); }));
    root.querySelector("[data-profile-access]")?.addEventListener("click", () => { window.__taskKarateAccess = { student: selectedStudent, stage: "pin", error: "" }; render(); root.querySelector("[data-pin-input]")?.focus(); });
    root.querySelector("[data-close-access]")?.addEventListener("click", () => { window.__taskKarateAccess = null; render(); });
    root.querySelector("[data-pin-form]")?.addEventListener("submit", (event) => { event.preventDefault(); const pin = event.currentTarget.elements["student-pin"].value; const expected = window.__taskKarateAccess.student.pin || "1234"; if (pin !== expected) { window.__taskKarateAccess.error = "That PIN did not match this profile."; render(); root.querySelector("[data-pin-input]")?.focus(); return; } window.__taskKarateAccess.stage = "waiver"; render(); });
    root.querySelector("[data-waiver-check]")?.addEventListener("change", (event) => { const button = root.querySelector("[data-waiver-accept]"); if (button) button.disabled = !event.target.checked; });
    root.querySelector("[data-waiver-accept]")?.addEventListener("click", () => { if (!root.querySelector("[data-waiver-check]")?.checked) return; sessionStorage.removeItem("task-karate-demo-session"); sessionStorage.setItem("task-karate-student-session", JSON.stringify({ studentId: window.__taskKarateAccess.student.id, accessedAt: new Date().toISOString() })); window.location.href = profileHref; });
    root.querySelector("[data-close-staff]")?.addEventListener("click", () => { window.__taskKarateStaffAccess = null; render(); });
    root.querySelector("[data-staff-pin-form]")?.addEventListener("submit", (event) => { event.preventDefault(); if (event.currentTarget.elements["staff-pin"].value !== "8675309") { window.__taskKarateStaffAccess.error = "That instructor PIN did not match."; render(); root.querySelector("[data-staff-pin-input]")?.focus(); return; } sessionStorage.setItem("task-karate-staff-session", "active"); window.location.href = window.location.pathname.includes("/portal/") ? "../admin/index.html" : "admin/index.html"; });
  }
})();
