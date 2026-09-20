(async () => {
  const status = document.querySelector("[data-admin-status]");
  const studentStatus = document.querySelector("[data-student-status]");
  let schedule = null;
  let newsIndex = null;
  let student = null;
  try {
    [schedule, newsIndex, student] = await Promise.all([
      fetch("../data/schedules.json", { cache: "no-store" }).then((r) => r.json()),
      fetch("../news/posts-index.json", { cache: "no-store" }).then((r) => r.json()),
      fetch("../data/demo-student.json", { cache: "no-store" }).then((r) => r.json())
    ]);
    const programCount = Object.keys(schedule || {}).length;
    const newsCount = (newsIndex?.posts || []).length;
    document.querySelector("[data-admin-schedule-count]").textContent = `${programCount} programs`;
    document.querySelector("[data-admin-news-count]").textContent = `${newsCount} archive entries`;
    const localStudent = JSON.parse(localStorage.getItem("task-karate-demo-student") || "null");
    student = localStudent || student;
    document.querySelector("[data-student-name]").value = student.name || "";
    document.querySelector("[data-student-rank]").value = student.rank || "";
  } catch (error) {
    show(status, "Admin data could not be loaded.");
    console.error(error);
  }
  document.querySelector("[data-export-schedule]")?.addEventListener("click", () => download("task-karate-schedules.json", schedule));
  document.querySelector("[data-export-news]")?.addEventListener("click", () => download("task-karate-news-index.json", newsIndex));
  document.querySelector("[data-save-student]")?.addEventListener("click", () => {
    if (!student) return;
    student.name = document.querySelector("[data-student-name]").value.trim() || student.name;
    student.rank = document.querySelector("[data-student-rank]").value.trim() || student.rank;
    localStorage.setItem("task-karate-demo-student", JSON.stringify(student));
    show(studentStatus, "Saved a local demo draft. This does not change the deployed portal.");
  });
  document.querySelector("[data-reset-student]")?.addEventListener("click", () => { localStorage.removeItem("task-karate-demo-student"); location.reload(); });
  function show(node, message) { if (node) { node.hidden = false; node.textContent = message; } }
  function download(filename, value) { const blob = new Blob([JSON.stringify(value, null, 2)], { type: "application/json" }); const link = document.createElement("a"); link.href = URL.createObjectURL(blob); link.download = filename; link.click(); URL.revokeObjectURL(link.href); }
})();
