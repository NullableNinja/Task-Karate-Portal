(async () => {
  const mount = document.querySelector("[data-schedule-mount]");
  if (!mount) return;
  const filters = { program: document.querySelector("#schedule-program"), belt: document.querySelector("#schedule-belt"), day: document.querySelector("#schedule-day") };
  const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"];
  let rows = [];
  try {
    const response = await fetch("data/schedules.json", { cache: "no-store" });
    if (!response.ok) throw new Error("Schedule data unavailable");
    const data = await response.json();
    Object.entries(data).forEach(([program, value]) => {
      value.groups.forEach((group) => {
        Object.entries(group.schedule || {}).forEach(([day, classes]) => {
          classes.forEach((item) => rows.push({
            program,
            programTitle: value.title,
            group: group.name,
            belts: group.belts,
            day,
            ...item
          }));
        });
      });
    });
    render();
    Object.values(filters).forEach((control) => control?.addEventListener("change", render));
  } catch (error) {
    mount.innerHTML = '<div class="empty-state">We could not load the schedule right now. Please call <a href="tel:+16087883126">(608) 788-3126</a> for current class times.</div>';
    console.error(error);
  }

  function render() {
    const selectedProgram = filters.program?.value || "all";
    const selectedBelt = filters.belt?.value || "all";
    const selectedDay = filters.day?.value || "all";
    const filtered = rows.filter((row) => (selectedProgram === "all" || row.program === selectedProgram) && (selectedBelt === "all" || row.belts.includes(selectedBelt)) && (selectedDay === "all" || row.day === selectedDay));
    if (!filtered.length) { mount.innerHTML = '<div class="empty-state">No classes match those filters. Try another day or program.</div>'; return; }
    mount.innerHTML = days.filter((day) => filtered.some((row) => row.day === day)).map((day) => {
      const dayRows = filtered.filter((row) => row.day === day).sort((a, b) => timeToMinutes(a.time) - timeToMinutes(b.time));
      return `<section class="schedule-day" id="${day.toLowerCase()}"><h3>${day}<span>${dayRows.length} ${dayRows.length === 1 ? "class" : "classes"}</span></h3><div class="class-list">${dayRows.map(card).join("")}</div></section>`;
    }).join("");
  }

  function card(row) {
    const belt = row.belts[0] || "blue";
    const duration = row.isHour ? "1 hour" : "45 minutes";
    const note = row.note ? ` · ${row.note}` : "";
    return `<article class="class-card" data-belt="${belt}"><span class="class-time">${row.time}</span><span class="class-name">${row.programTitle}</span><span class="class-detail">${row.group}<br>${duration}${note}</span></article>`;
  }

  function timeToMinutes(value) {
    const match = String(value).match(/(\d{1,2}):(\d{2})\s*(AM|PM)/i);
    if (!match) return Number.MAX_SAFE_INTEGER;
    let hour = Number(match[1]) % 12;
    if (match[3].toUpperCase() === "PM") hour += 12;
    return hour * 60 + Number(match[2]);
  }
})();
