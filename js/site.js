const Site = (() => {
  const body = document.body;
  const root = body?.dataset.root || "";
  const page = body?.dataset.page || "";
  let config = null;
  let activeModal = null;
  let lastFocus = null;

  const beltStages = [
    { threshold: 0, name: "White", color: "var(--belt-white)" },
    { threshold: 11, name: "Gold", color: "var(--belt-gold)" },
    { threshold: 22, name: "Orange", color: "var(--belt-orange)" },
    { threshold: 33, name: "Green", color: "var(--belt-green)" },
    { threshold: 44, name: "Purple", color: "var(--belt-purple)" },
    { threshold: 56, name: "Blue", color: "var(--belt-blue)" },
    { threshold: 67, name: "Red", color: "var(--belt-red)" },
    { threshold: 78, name: "Brown", color: "var(--belt-brown)" },
    { threshold: 89, name: "Black", color: "var(--belt-black)" }
  ];

  async function loadJson(path) {
    const response = await fetch(path, { cache: "no-store" });
    if (!response.ok) throw new Error(`Could not load ${path}`);
    return response.json();
  }

  function interpolate(template, values) {
    return template.replace(/\{(\w+)\}/g, (_, key) => values[key] ?? "");
  }

  function applyTheme(theme) {
    const safeTheme = theme === "dark" ? "dark" : "light";
    document.documentElement.dataset.theme = safeTheme;
    document.querySelectorAll("[data-theme-toggle]").forEach((toggle) => {
      const isDark = safeTheme === "dark";
      toggle.setAttribute("aria-pressed", String(isDark));
      toggle.setAttribute("aria-checked", String(isDark));
      toggle.setAttribute("aria-label", isDark ? "Use light mode" : "Use dark mode");
      toggle.setAttribute("title", isDark ? "Use light mode" : "Use dark mode");
      toggle.classList.toggle("is-dark", isDark);
      toggle.innerHTML = `<span class="theme-toggle__track" aria-hidden="true"><span class="theme-toggle__thumb"></span></span><span class="theme-toggle__label">${isDark ? "Dark" : "Light"}</span>`;
    });
  }

  function preferredTheme() {
    try {
      const saved = localStorage.getItem("task-karate-theme");
      if (saved) return saved;
    } catch { /* session-only preference is fine */ }
    return typeof window !== "undefined" && window.matchMedia?.("(prefers-color-scheme: dark)").matches ? "dark" : "light";
  }

  function wireTheme() {
    applyTheme(preferredTheme());
    document.querySelectorAll("[data-theme-toggle]").forEach((toggle) => {
      toggle.addEventListener("click", () => {
        const next = document.documentElement.dataset.theme === "dark" ? "light" : "dark";
        applyTheme(next);
        try { localStorage.setItem("task-karate-theme", next); } catch { /* preference remains session-only */ }
      });
    });
  }

  async function loadShell() {
    try {
      config = await loadJson(`${root}data/site.json`);
      if (body.classList.contains("portal-page") && !document.querySelector("[data-portal-modern-styles]")) {
        const portalStyles = document.createElement("link");
        portalStyles.rel = "stylesheet";
        portalStyles.href = `${root}css/portal-modern.css?v=portal-20260920`;
        portalStyles.dataset.portalModernStyles = "true";
        document.head.appendChild(portalStyles);
      }
      const [nav, footer] = await Promise.all([
        fetch(`${root}partials/navigation.html`).then((r) => r.text()),
        fetch(`${root}partials/footer.html`).then((r) => r.text())
      ]);
      const values = { root, ...config };
      document.querySelector("#site-header")?.replaceWith(document.createRange().createContextualFragment(interpolate(nav, values)));
      document.querySelector("#site-footer")?.replaceWith(document.createRange().createContextualFragment(interpolate(footer, values)));
      document.querySelector(`[data-nav="${page}"]`)?.setAttribute("aria-current", "page");
      injectGlobalUI();
      wireNavigation();
      wireTheme();
      wireGlobalInteractions();
      document.querySelectorAll("[data-year]").forEach((node) => { node.textContent = new Date().getFullYear(); });
      setMetadata(config);
    } catch (error) {
      const status = document.querySelector("[data-shell-status]");
      if (status) status.textContent = "Navigation is temporarily unavailable. Use the page links below.";
      console.error(error);
    }
  }

  function injectGlobalUI() {
    if (document.querySelector("#global-interactions")) return;
    const rootNode = document.createElement("div");
    rootNode.id = "global-interactions";
    rootNode.innerHTML = `
      <div class="scroll-progress" data-scroll-progress aria-hidden="true"></div>
      <div class="scroll-rank-indicator" data-scroll-rank role="status" aria-live="polite"><span class="scroll-rank-indicator__swatch" data-scroll-swatch></span><span data-scroll-name>White belt</span></div>
      <button class="back-to-top" type="button" data-back-to-top aria-label="Back to top" title="Back to top">↑</button>
      <div class="floating-actions" aria-label="Quick actions"><button type="button" data-open-contact>Contact us</button><button type="button" data-open-trial>Two-week trial</button></div>
      <div class="site-modal" id="contact-modal" role="dialog" aria-modal="true" aria-labelledby="contact-modal-title" aria-hidden="true" hidden>
        <div class="site-modal__backdrop" data-modal-close></div><div class="site-modal__panel" tabindex="-1"><button class="site-modal__close" type="button" data-modal-close aria-label="Close contact form">×</button><span class="eyebrow">Talk to the school</span><h2 id="contact-modal-title">Start a conversation.</h2><p class="muted">Ask about programs, class fit, or a first visit. This local demo records no message and sends no email.</p><form data-modal-contact-form><div class="form-grid"><div class="field"><label for="modal-name">Name</label><input id="modal-name" name="name" required autocomplete="name"></div><div class="field"><label for="modal-email">Email</label><input id="modal-email" name="email" required type="email" autocomplete="email"></div><div class="field"><label for="modal-message">How can we help?</label><textarea id="modal-message" name="message" required></textarea></div></div><button class="button" type="submit">Prepare message</button><p class="form-status" data-modal-form-status hidden role="status"></p></form></div>
      </div>
      <div class="site-modal" id="trial-modal" role="dialog" aria-modal="true" aria-labelledby="trial-modal-title" aria-hidden="true" hidden>
        <div class="site-modal__backdrop" data-modal-close></div><div class="site-modal__panel paper-panel--ruled" tabindex="-1"><button class="site-modal__close" type="button" data-modal-close aria-label="Close trial information">×</button><span class="eyebrow">New students</span><h2 id="trial-modal-title">Two weeks to find your fit.</h2><p>Ask Task Karate about a two-week free trial and find a class that feels right. The school confirms current eligibility, dates, and program fit before you begin.</p><div class="button-row"><button class="button" type="button" data-open-contact>Ask about the trial</button><button class="button button--outline" type="button" data-modal-close>Maybe later</button></div></div>
      </div>
      <div class="site-modal" id="class-detail-modal" role="dialog" aria-modal="true" aria-labelledby="class-modal-title" aria-hidden="true" hidden>
        <div class="site-modal__backdrop" data-modal-close></div><div class="site-modal__panel" tabindex="-1"><button class="site-modal__close" type="button" data-modal-close aria-label="Close class details">×</button><span class="eyebrow">Class details</span><h2 id="class-modal-title" data-class-title>Class</h2><p data-class-detail class="muted"></p><div class="button-row"><a class="button" href="${root}contact.html">Ask a question</a><button class="button button--outline" type="button" data-modal-close>Close</button></div></div>
      </div>
      <div class="site-modal" id="news-detail-modal" role="dialog" aria-modal="true" aria-labelledby="news-modal-title" aria-hidden="true" hidden>
        <div class="site-modal__backdrop" data-modal-close></div><div class="site-modal__panel" tabindex="-1"><button class="site-modal__close" type="button" data-modal-close aria-label="Close news story">×</button><span class="eyebrow" data-news-date>Task Karate news</span><h2 id="news-modal-title" data-news-title>News</h2><p data-news-body></p><button class="button button--outline" type="button" data-modal-close>Close</button></div>
      </div>`;
    document.body.append(rootNode);
  }

  function wireNavigation() {
    const toggle = document.querySelector(".nav-toggle");
    const links = document.querySelector(".nav-links");
    if (!toggle || !links) return;
    toggle.addEventListener("click", () => {
      const open = links.dataset.open === "true";
      links.dataset.open = String(!open);
      toggle.setAttribute("aria-expanded", String(!open));
      toggle.textContent = open ? "Menu" : "Close";
    });
    links.addEventListener("click", (event) => {
      if (event.target.closest("a") && !event.target.closest("[data-open-contact]")) { links.dataset.open = "false"; toggle.setAttribute("aria-expanded", "false"); toggle.textContent = "Menu"; }
    });
  }

  function wireGlobalInteractions() {
    document.querySelectorAll("[data-open-contact]").forEach((button) => button.addEventListener("click", (event) => { if (button.tagName === "A") event.preventDefault(); openModal("contact-modal"); }));
    document.querySelectorAll("[data-open-trial]").forEach((button) => button.addEventListener("click", () => openModal("trial-modal")));
    document.querySelectorAll("[data-modal-close]").forEach((button) => button.addEventListener("click", (event) => { if (event.target === button || button.closest("button")) closeModal(); }));
    document.querySelectorAll("[data-modal-contact-form]").forEach((form) => form.addEventListener("submit", (event) => { event.preventDefault(); const status = form.querySelector("[data-modal-form-status]"); if (status) { status.hidden = false; status.textContent = "Thanks — your message is ready for the school team. This demo does not send email until a production endpoint is configured."; } form.reset(); }));
    document.querySelector("[data-back-to-top]")?.addEventListener("click", () => window.scrollTo({ top: 0, behavior: "smooth" }));
    let ticking = false;
    const updateScroll = () => {
      const docHeight = document.documentElement.scrollHeight - window.innerHeight;
      const percent = docHeight > 0 ? Math.min(100, Math.max(0, (window.scrollY / docHeight) * 100)) : 0;
      const stage = beltStages.reduce((chosen, belt) => percent >= belt.threshold ? belt : chosen, beltStages[0]);
      const progress = document.querySelector("[data-scroll-progress]");
      const swatch = document.querySelector("[data-scroll-swatch]");
      const label = document.querySelector("[data-scroll-name]");
      if (progress) { progress.style.width = `${percent}%`; progress.style.background = stage.color; }
      if (swatch) swatch.style.background = stage.color;
      if (label) label.textContent = `${stage.name} belt · ${Math.round(percent)}%`;
      document.querySelector("[data-back-to-top]")?.classList.toggle("is-visible", window.scrollY > 320);
      ticking = false;
    };
    window.addEventListener("scroll", () => { if (!ticking) { window.requestAnimationFrame(updateScroll); ticking = true; } }, { passive: true });
    updateScroll();
    if (page === "home") {
      try { if (sessionStorage.getItem("task-karate-trial-dismissed") !== "true") window.setTimeout(() => openModal("trial-modal"), 1400); } catch { window.setTimeout(() => openModal("trial-modal"), 1400); }
    }
  }

  function openModal(id, data = {}) {
    const modal = document.getElementById(id);
    if (!modal) return;
    lastFocus = document.activeElement;
    activeModal = modal;
    if (data.title) modal.querySelector("[data-class-title]")?.replaceChildren(document.createTextNode(data.title));
    if (data.detail) modal.querySelector("[data-class-detail]")?.replaceChildren(document.createTextNode(data.detail));
    if (data.newsTitle) modal.querySelector("[data-news-title]")?.replaceChildren(document.createTextNode(data.newsTitle));
    if (data.newsDate) modal.querySelector("[data-news-date]")?.replaceChildren(document.createTextNode(data.newsDate));
    if (data.newsBody) modal.querySelector("[data-news-body]")?.replaceChildren(document.createTextNode(data.newsBody));
    modal.hidden = false; modal.setAttribute("aria-hidden", "false"); document.body.style.overflow = "hidden";
    window.setTimeout(() => modal.querySelector(".site-modal__close")?.focus(), 0);
  }

  function closeModal() {
    if (!activeModal) return;
    const closed = activeModal;
    closed.hidden = true; closed.setAttribute("aria-hidden", "true"); activeModal = null; document.body.style.overflow = "";
    if (closed.id === "trial-modal") { try { sessionStorage.setItem("task-karate-trial-dismissed", "true"); } catch { /* no-op */ } }
    if (lastFocus && document.contains(lastFocus)) lastFocus.focus();
  }

  document.addEventListener("keydown", (event) => {
    if (!activeModal) return;
    if (event.key === "Escape") { closeModal(); return; }
    if (event.key !== "Tab") return;
    const focusable = [...activeModal.querySelectorAll("button, a, input, textarea, select, [tabindex]:not([tabindex='-1'])")].filter((node) => !node.disabled);
    if (!focusable.length) return;
    const first = focusable[0]; const last = focusable[focusable.length - 1];
    if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
    else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
  });

  function setMetadata(site) {
    const siteUrl = String(site.siteUrl || "").replace(/\/$/, "");
    if (!siteUrl) { document.querySelector("link[rel=canonical]")?.remove(); return; }
    const path = `${location.pathname.split("/").pop() || "index.html"}`;
    const canonical = `${siteUrl}/${path === "index.html" ? "" : path}`;
    document.querySelector("link[rel=canonical]")?.setAttribute("href", canonical);
    document.querySelectorAll("[data-site-url]").forEach((node) => node.setAttribute("content", canonical));
  }

  function wireForms() {
    document.querySelectorAll("form[data-demo-form]").forEach((form) => form.addEventListener("submit", (event) => { event.preventDefault(); const status = form.querySelector("[data-form-status]"); if (status) { status.hidden = false; status.textContent = "Thanks — your message is ready for the school team. This demo does not send email until a production form endpoint is configured."; } form.reset(); }));
  }

  window.TaskSite = { loadJson, openModal, closeModal };
  loadShell().then(wireForms);
  return { loadJson, openModal, closeModal };
})();
