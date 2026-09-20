(async () => {
  const mount = document.querySelector("[data-schedule-mount]");
  const pillMount = document.querySelector("[data-schedule-pills]");
  if (!mount || !pillMount) return;
  const beltFilter = document.querySelector("#schedule-belt");
  const dayFilter = document.querySelector("#schedule-day");
  const download = document.querySelector("[data-schedule-download]");
  const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
  const programs = ["kids", "teensAdults", "sparring", "weapons", "special", "eskrima"];
  const pdfs = { kids: "files/schedules/task-schedule-kids.pdf", teensAdults: "files/schedules/task-schedule-teens-adults.pdf" };
  let data = {};
  let selected = "kids";

  try {
    const response = await fetch("data/schedules.json", { cache: "no-store" });
    if (!response.ok) throw new Error("Schedule data unavailable");
    data = await response.json();
    renderPills();
    render();
    beltFilter?.addEventListener("change", render);
    dayFilter?.addEventListener("change", render);
    document.querySelector("[data-print-schedule]")?.addEventListener("click", () => window.print());
    document.addEventListener("click", (event) => {
      const pill = event.target.closest("[data-class-title]");
      if (!pill) return;
      window.TaskSite?.openModal("class-detail-modal", { title: pill.dataset.classTitle, detail: pill.dataset.classDetail });
    });
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
