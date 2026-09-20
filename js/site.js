const Site = (() => {
  const body = document.body;
  const root = body?.dataset.root || "";
  const page = body?.dataset.page || "";
  let config = null;

  async function loadJson(path) {
    const response = await fetch(path, { cache: "no-store" });
    if (!response.ok) throw new Error(`Could not load ${path}`);
    return response.json();
  }

  function interpolate(template, values) {
    return template.replace(/\{(\w+)\}/g, (_, key) => values[key] ?? "");
  }

  async function loadShell() {
    try {
      config = await loadJson(`${root}data/site.json`);
      const [nav, footer] = await Promise.all([
        fetch(`${root}partials/navigation.html`).then((r) => r.text()),
        fetch(`${root}partials/footer.html`).then((r) => r.text())
      ]);
      const values = { root, ...config };
      document.querySelector("#site-header")?.replaceWith(document.createRange().createContextualFragment(interpolate(nav, values)));
      document.querySelector("#site-footer")?.replaceWith(document.createRange().createContextualFragment(interpolate(footer, values)));
      document.querySelector(`[data-nav="${page}"]`)?.setAttribute("aria-current", "page");
      wireNavigation();
      document.querySelectorAll("[data-year]").forEach((node) => { node.textContent = new Date().getFullYear(); });
      setMetadata(config);
    } catch (error) {
      const status = document.querySelector("[data-shell-status]");
      if (status) status.textContent = "Navigation is temporarily unavailable. Use the page links below.";
      console.error(error);
    }
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
      if (event.target.closest("a")) { links.dataset.open = "false"; toggle.setAttribute("aria-expanded", "false"); toggle.textContent = "Menu"; }
    });
  }

  function setMetadata(site) {
    const siteUrl = String(site.siteUrl || "").replace(/\/$/, "");
    if (!siteUrl) { document.querySelector("link[rel=canonical]")?.remove(); return; }
    const path = `${location.pathname.split("/").pop() || "index.html"}`;
    const canonical = `${siteUrl}/${path === "index.html" ? "" : path}`;
    document.querySelector("link[rel=canonical]")?.setAttribute("href", canonical);
    document.querySelectorAll("[data-site-url]").forEach((node) => node.setAttribute("content", canonical));
  }

  function wireForms() {
    document.querySelectorAll("form[data-demo-form]").forEach((form) => {
      form.addEventListener("submit", (event) => {
        event.preventDefault();
        const status = form.querySelector("[data-form-status]");
        if (status) { status.hidden = false; status.textContent = "Thanks — your message is ready for the school team. This demo does not send email until a production form endpoint is configured."; }
        form.reset();
      });
    });
  }

  loadShell().then(wireForms);
  return { loadJson };
})();
