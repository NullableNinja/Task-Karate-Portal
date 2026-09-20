(async () => {
  const mount = document.querySelector("[data-schedule-mount]");
  const pillMount = document.querySelector("[data-schedule-pills]");
  const checkinMount = document.querySelector("[data-schedule-checkin]");
  if (!mount || !pillMount) return;
  const beltFilter = document.querySelector("#schedule-belt");
  const dayFilter = document.querySelector("#schedule-day");
  const download = document.querySelector("[data-schedule-download]");
  const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
  const programs = ["kids", "teensAdults", "sparring", "weapons", "special", "eskrima"];
  const pdfs = { kids: "files/schedules/task-schedule-kids.pdf", teensAdults: "files/schedules/task-schedule-teens-adults.pdf" };
  let data = {};
  let selected = "kids";
  let roster = [];
  let attendance = [];
  let selectedAttendance = new Set();

  try {
    const [scheduleResponse, rosterResponse] = await Promise.all([fetch("data/schedules.json", { cache: "no-store" }), fetch("data/portal-students.json", { cache: "no-store" })]);
    if (!scheduleResponse.ok) throw new Error("Schedule data unavailable");
    data = await scheduleResponse.json();
    roster = window.TaskKarateStore ? await window.TaskKarateStore.getRoster() : (rosterResponse.ok ? (await rosterResponse.json()).students || [] : []);
    attendance = await loadAttendance();
    renderPills();
    render();
    renderCheckin();
    beltFilter?.addEventListener("change", render);
    dayFilter?.addEventListener("change", render);
    document.querySelector("[data-print-schedule]")?.addEventListener("click", () => window.print());
    document.addEventListener("click", (event) => {
      const pill = event.target.closest("[data-class-title]");
      if (!pill) return;
      window.TaskSite?.openModal("class-detail-modal", { title: pill.dataset.classTitle, detail: pill.dataset.classDetail });
    });
    checkinMount?.addEventListener("click", handleCheckinClick);
    checkinMount?.addEventListener("submit", handleCheckinSubmit);
  } catch (error) {
    mount.innerHTML = '<div class="empty-state">We could not load the schedule right now. Please call <a href="tel:+16087883126">(608) 788-3126</a> for current class times.</div>';
    console.error(error);
  }

  function renderPills() {
    pillMount.innerHTML = programs.map((key) => `<button class="schedule-pill" type="button" role="tab" data-program="${key}" aria-selected="${key === selected}">${esc(data[key]?.title || key)}</button>`).join("");
    pillMount.querySelectorAll("[data-program]").forEach((button) => button.addEventListener("click", () => { selected = button.dataset.program; beltFilter.value = "all"; dayFilter.value = "all"; renderPills(); render(); }));
  }

  function render() {
    const program = data[selected];
    if (!program) return;
    const belt = beltFilter?.value || "all";
    const day = dayFilter?.value || "all";
    const groups = (program.groups || []).filter((group) => belt === "all" || (group.belts || []).includes(belt));
    const visibleDays = day === "all" ? days : [day];
    const rows = groups.map((group) => ({ ...group, schedule: Object.fromEntries(visibleDays.map((name) => [name, group.schedule?.[name] || []])) }));
    if (!rows.length || !rows.some((group) => Object.values(group.schedule).some((times) => times.length))) { mount.innerHTML = '<div class="empty-state">No classes match those filters. Try another belt group or day.</div>'; updateDownload(); return; }
    mount.innerHTML = `<div class="schedule-summary"><div><span class="eyebrow">${esc(program.eyebrow || "Task Karate")}</span><h2>${esc(program.title)}</h2></div><p>${esc(program.description || program.note || "Click any time pill for more information.")}</p></div><div class="schedule-matrix">${renderTable(rows, visibleDays)}</div><div class="schedule-mobile">${renderMobile(rows, visibleDays)}</div><p class="schedule-note">${esc(program.note || "Click a time pill for class details.")}</p>`;
    updateDownload();
  }

  function renderCheckin() {
    if (!checkinMount) return;
    const session = readSession();
    if (!session) {
      checkinMount.innerHTML = `<div class="schedule-checkin__intro"><div><span class="new-kicker">Student portal</span><h2 id="schedule-checkin-title">Checking in for class?</h2><p>Open your student portal first, then return here to choose today’s class. This records attendance; it does not enroll you or reserve a spot.</p></div><a class="new-button new-button--blue" href="portal/login.html">Open student portal <span>↗</span></a></div>`;
      return;
    }
    const student = roster.find((item) => item.id === session.studentId);
    if (!student) { checkinMount.innerHTML = '<p class="schedule-checkin__message">Your student record could not be found on this device. Open the student portal again.</p>'; return; }
    const today = new Intl.DateTimeFormat("en-US", { weekday: "long" }).format(new Date());
    const classes = eligibleClasses(student, today);
    const todayKey = dateKey(new Date());
    const checkedIn = attendance.filter((item) => item.studentId === student.id && item.date === todayKey).map((item) => item.classId);
    checkinMount.innerHTML = `<div class="schedule-checkin__intro"><div><span class="new-kicker">Signed in as ${esc(student.displayName || student.name)}</span><h2 id="schedule-checkin-title">Check in for today</h2><p>Select the class or classes you are attending. Your attendance is saved on this studio computer.</p></div><a class="new-text-link" href="portal/index.html">Open my portal <span>→</span></a></div><div class="schedule-checkin__day"><strong>${esc(today)}</strong><span>Attendance only · no enrollment or capacity claim</span></div><div class="schedule-checkin__classes">${classes.map((item) => { const isChecked = checkedIn.includes(item.id); const isSelected = selectedAttendance.has(item.id); return `<button type="button" class="schedule-checkin__class${isChecked ? " is-checked" : ""}${isSelected ? " is-selected" : ""}" data-attendance-class="${esc(item.id)}" ${isChecked ? "disabled" : ""}><span class="schedule-checkin__time">${esc(item.time)}</span><span><strong>${esc(item.program)}</strong><small>${esc(item.group)} · ${item.isHour ? "1 hour" : "45 minutes"}</small></span><b>${isChecked ? "Recorded" : isSelected ? "Chosen" : "Choose"}</b></button>`; }).join("") || '<p class="schedule-checkin__message">No eligible classes are listed for you today.</p>'}</div>${classes.length ? `<form class="schedule-checkin__actions"><button class="new-button new-button--blue" type="submit" ${selectedAttendance.size ? "" : "disabled"}>Check in to selected class${selectedAttendance.size === 1 ? "" : "es"}</button><span class="schedule-checkin__status" role="status" data-checkin-status>${checkedIn.length ? `${checkedIn.length} class${checkedIn.length === 1 ? "" : "es"} recorded today.` : "Choose a class above."}</span></form>` : ""}`;
  }

  function eligibleClasses(student, day) {
    const rank = String(student.rank || "").toLowerCase();
    const belt = ["white", "gold", "orange", "green", "blue", "purple", "red", "brown", "black"].find((value) => rank.includes(value)) || "white";
    const keys = student.ageGroup === "kids" ? ["kids", "sparring", "weapons", "special"] : student.ageGroup === "staff" ? Object.keys(data) : ["teensAdults", "sparring", "weapons", "special", "eskrima"];
    return keys.flatMap((programKey) => (data[programKey]?.groups || []).flatMap((group) => {
      if (!group.belts?.includes(belt) && !(student.ageGroup === "staff" && group.belts?.includes("black"))) return [];
      return (group.schedule?.[day] || []).map((entry, index) => ({ id: `${programKey}-${day}-${entry.time}-${group.name}-${index}`, time: entry.time, program: data[programKey].title, group: group.name, isHour: entry.isHour }));
    }));
  }

  function handleCheckinClick(event) {
    const button = event.target.closest("[data-attendance-class]");
    if (!button || button.disabled) return;
    const id = button.dataset.attendanceClass;
    if (selectedAttendance.has(id)) selectedAttendance.delete(id); else selectedAttendance.add(id);
    renderCheckin();
  }

  async function handleCheckinSubmit(event) {
    const form = event.target.closest(".schedule-checkin__actions");
    if (!form) return;
    event.preventDefault();
    const session = readSession();
    const student = roster.find((item) => item.id === session?.studentId);
    if (!student || !selectedAttendance.size) return;
    const today = new Intl.DateTimeFormat("en-US", { weekday: "long" }).format(new Date());
    const classes = eligibleClasses(student, today);
    const dayKey = dateKey(new Date());
    const existing = new Set(attendance.filter((item) => item.studentId === student.id && item.date === dayKey).map((item) => item.classId));
    for (const item of classes.filter((candidate) => selectedAttendance.has(candidate.id) && !existing.has(candidate.id))) {
      if (window.TaskKarateStore) await window.TaskKarateStore.checkIn(student.id, item.id, `${item.program} · ${item.group} · ${item.time}`);
      else attendance.push({ studentId: student.id, date: dayKey, classId: item.id, time: item.time, program: item.program, group: item.group, recordedAt: new Date().toISOString() });
    }
    attendance = await loadAttendance();
    selectedAttendance.clear();
    renderCheckin();
  }

  async function loadAttendance() {
    const today = dateKey(new Date());
    if (window.TaskKarateStore) {
      const records = await window.TaskKarateStore.getCheckins(today);
      const normalized = records.map((record) => ({ ...record, studentId: record.studentId || record.student_id, classId: record.classId || record.class_id, date: record.date }));
      try { localStorage.setItem("task-karate-attendance-records", JSON.stringify(normalized)); } catch { /* local mirror is optional */ }
      return normalized;
    }
    try { return JSON.parse(localStorage.getItem("task-karate-attendance-records") || "[]"); } catch { return []; }
  }
  function readSession() { try { return JSON.parse(sessionStorage.getItem("task-karate-student-session") || "null"); } catch { return null; } }
  function dateKey(value) { return value.toISOString().slice(0, 10); }

  function renderTable(groups, visibleDays) {
    const heads = visibleDays.map((day) => `<th scope="col">${day}</th>`).join("");
    const body = groups.map((group) => `<tr><td>${group.belts.map((belt) => `<span class="belt-link" style="--belt:var(--belt-${belt})">${esc(belt)}</span>`).join(" ")}</td><td><strong>${esc(group.name)}</strong></td>${visibleDays.map((day) => `<td>${(group.schedule[day] || []).map((item) => pill(item, group, day)).join("") || '<span class="muted">—</span>'}</td>`).join("")}</tr>`).join("");
    return `<table><thead><tr><th scope="col">Belt group</th><th scope="col">Class</th>${heads}</tr></thead><tbody>${body}</tbody></table>`;
  }

  function renderMobile(groups, visibleDays) {
    return visibleDays.map((day) => { const classes = groups.flatMap((group) => (group.schedule[day] || []).map((item) => ({ item, group }))).sort((a, b) => timeToMinutes(a.item.time) - timeToMinutes(b.item.time)); if (!classes.length) return ""; return `<section class="schedule-mobile__day"><h3>${day}</h3>${classes.map(({ item, group }) => `<div class="schedule-mobile__class"><button class="time-pill" data-belt="${group.belts[0] || "blue"}" data-class-title="${attr(`${data[selected].title} · ${day}`)}" data-class-detail="${attr(`${group.name} · ${item.time} · ${item.isHour ? "1 hour" : "45 minutes"}${item.note ? ` · ${item.note}` : ""}`)}">${esc(item.time)}${item.isHour ? "*" : ""}</button><span>${esc(group.name)}</span></div>`).join("")}</section>`; }).join("");
  }

  function pill(item, group, day) {
    const detail = `${group.name} · ${item.time} · ${item.isHour ? "1 hour" : "45 minutes"}${item.note ? ` · ${item.note}` : ""}`;
    return `<button class="time-pill" type="button" data-belt="${group.belts[0] || "blue"}" data-class-title="${attr(`${data[selected].title} · ${day}`)}" data-class-detail="${attr(detail)}">${esc(item.time)}${item.isHour ? "*" : ""}</button>`;
  }

  function updateDownload() {
    if (!download) return;
    if (pdfs[selected]) { download.hidden = false; download.href = pdfs[selected]; download.textContent = `Download ${data[selected].title} PDF`; }
    else { download.hidden = true; }
  }

  function timeToMinutes(value) { const match = String(value).match(/(\d{1,2}):(\d{2})\s*(AM|PM)/i); if (!match) return Number.MAX_SAFE_INTEGER; let hour = Number(match[1]) % 12; if (match[3].toUpperCase() === "PM") hour += 12; return hour * 60 + Number(match[2]); }
  function esc(value) { return String(value ?? "").replace(/[&<>"']/g, (char) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;" }[char])); }
  function attr(value) { return esc(value); }
})();
