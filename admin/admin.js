(async () => {
  const keys = { note: "task-karate-admin-note", stars: "task-karate-portal-gold-stars", hidden: "task-karate-hidden-posts" };
  const read = (key, fallback) => { try { return JSON.parse(localStorage.getItem(key)) ?? fallback; } catch { return fallback; } };
  const write = (key, value) => localStorage.setItem(key, JSON.stringify(value));
  const esc = (value) => String(value ?? "").replace(/[&<>"']/g, (char) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#039;" }[char]));
  const download = (filename, value) => { const link = document.createElement("a"); link.href = URL.createObjectURL(new Blob([JSON.stringify(value, null, 2)], { type: "application/json" })); link.download = filename; link.click(); URL.revokeObjectURL(link.href); };
  const show = (node, message) => { if (!node) return; node.hidden = false; node.textContent = message; window.setTimeout(() => { node.hidden = true; }, 3500); };
  let data;
  try { data = await fetch("../data/portal-demo.json", { cache: "no-store" }).then((response) => response.json()); } catch (error) { console.error(error); document.querySelector(".admin-workspace").innerHTML = '<div class="admin-card"><h2>Demo data unavailable.</h2><p>Start the site through a local HTTP server, then reload this page.</p></div>'; return; }

  const note = { ...data.staffNote, ...read(keys.note, {}) };
  const stars = read(keys.stars, data.goldStars);
  const hidden = read(keys.hidden, []);
  const posts = read("task-karate-portal-posts", data.posts);
  const consent = read("task-karate-portal-consent", null);
  const localStudent = read("task-karate-demo-student", data.student);

  document.querySelector("[data-metric-posts]").textContent = posts.filter((post) => !hidden.includes(post.id)).length;
  document.querySelector("[data-metric-messages]").textContent = data.messages.filter((message) => message.unread).length;
  document.querySelector("[data-metric-stars]").textContent = stars.length;
  document.querySelector("[data-metric-consent]").textContent = consent ? "Ready" : "Pending";
  document.querySelector("[data-note-preview-title]").textContent = note.title;
  document.querySelector("[data-note-preview-body]").textContent = note.body;
  document.querySelector("[data-note-preview-author]").textContent = `${note.author} · ${note.role}`;
  document.querySelector("[data-consent-title]").textContent = consent ? "Local acknowledgment saved" : "Guardian review pending";
  document.querySelector("[data-consent-copy]").textContent = consent ? `Saved for ${consent.student || "the demo student"} in this browser.` : "No local acknowledgment has been saved for the demo student.";
  document.querySelector("[data-moderation-title]").textContent = `${posts.filter((post) => !hidden.includes(post.id)).length} visible / ${hidden.length} hidden`;
  document.querySelector("[data-guardian-status]").textContent = consent ? "Local demo complete" : data.student.guardianStatus;
  document.querySelector("[data-student-name]").value = localStudent.name || localStudent.fullName || "";
  document.querySelector("[data-student-rank]").value = localStudent.rank || "";
  document.querySelector("[data-note-title]").value = note.title;
  document.querySelector("[data-note-body]").value = note.body;
  document.querySelector("[data-note-author]").value = note.author;

  renderPosts(); renderStars();
  document.querySelector("#student-form").addEventListener("submit", (event) => { event.preventDefault(); const values = Object.fromEntries(new FormData(event.currentTarget)); const next = { ...localStudent, name: values.name, fullName: values.name, rank: values.rank }; write("task-karate-demo-student", next); show(document.querySelector("[data-student-status]"), "Saved a local synthetic student draft."); });
  document.querySelector("[data-reset-student]").addEventListener("click", () => { localStorage.removeItem("task-karate-demo-student"); location.reload(); });
  document.querySelector("#note-form").addEventListener("submit", (event) => { event.preventDefault(); const next = Object.fromEntries(new FormData(event.currentTarget)); write(keys.note, { ...note, ...next, date: "Just now" }); show(document.querySelector("[data-note-status]"), "Staff note saved to local demo state."); document.querySelector("[data-note-preview-title]").textContent = next.title; document.querySelector("[data-note-preview-body]").textContent = next.body; document.querySelector("[data-note-preview-author]").textContent = `${next.author} · Instructor note`; });
  document.querySelector("#gold-star-form").addEventListener("submit", (event) => { event.preventDefault(); const values = Object.fromEntries(new FormData(event.currentTarget)); stars.unshift({ id: `star-${Date.now()}`, student: values.student, reason: values.reason, from: "Staff demo", date: "Just now", color: values.color }); write(keys.stars, stars); renderStars(); document.querySelector("[data-metric-stars]").textContent = stars.length; show(document.querySelector("[data-star-status]"), "Gold Star added to the local wall."); event.currentTarget.reset(); });
  document.querySelector("[data-download-snapshot]").addEventListener("click", () => download("task-karate-portal-demo-snapshot.json", { exportedAt: new Date().toISOString(), student: localStudent, note, stars, posts, hiddenPostIds: hidden, consent, source: "Synthetic local demo only" }));

  function renderPosts() { document.querySelector("[data-community-count]").textContent = `${posts.length} total`; document.querySelector("[data-admin-posts]").innerHTML = posts.map((post) => { const isHidden = hidden.includes(post.id); return `<div class="admin-post-row ${isHidden ? "is-hidden" : ""}"><div><strong>${esc(post.author)}</strong><span>${esc(post.role)} · ${esc(post.date)}</span><p>${esc(post.content)}</p></div><button class="admin-button admin-button--small ${isHidden ? "admin-button--blue" : "admin-button--outline"}" type="button" data-toggle-post="${esc(post.id)}">${isHidden ? "Show post" : "Hide post"}</button></div>`; }).join(""); document.querySelectorAll("[data-toggle-post]").forEach((button) => button.addEventListener("click", () => { const id = button.dataset.togglePost; const index = hidden.indexOf(id); if (index >= 0) hidden.splice(index, 1); else hidden.push(id); write(keys.hidden, hidden); renderPosts(); document.querySelector("[data-moderation-title]").textContent = `${posts.filter((post) => !hidden.includes(post.id)).length} visible / ${hidden.length} hidden`; document.querySelector("[data-metric-posts]").textContent = posts.filter((post) => !hidden.includes(post.id)).length; })); }
  function renderStars() { document.querySelector("[data-admin-stars]").innerHTML = stars.slice(0, 8).map((star) => `<div class="admin-star-row"><span class="admin-star-mark admin-star-mark--${esc(star.color || "gold")}">★</span><span><strong>${esc(star.student)}</strong><small>${esc(star.reason)}</small></span><time>${esc(star.date)}</time></div>`).join(""); }
})();
