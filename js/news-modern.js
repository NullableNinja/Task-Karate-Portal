(async () => {
  const mount = document.querySelector("[data-news-mount]");
  if (!mount) return;
  try {
    const index = await fetch("news/posts-index.json", { cache: "no-store" }).then((r) => r.json());
    const posts = await Promise.all((index.posts || []).map((path) => fetch(path, { cache: "no-store" }).then((r) => r.json())));
    posts.sort((a, b) => String(b.date).localeCompare(String(a.date)));
    if (!posts.length) throw new Error("No posts");
    posts.forEach((post) => {
      const article = document.createElement("article");
      article.className = "paper-panel news-card";
      if (post.hero) { const image = document.createElement("img"); image.src = post.hero; image.alt = post.heroAlt || "Task Karate news"; image.loading = "lazy"; image.width = 900; image.height = 500; article.append(image); }
      const content = document.createElement("div"); content.className = "news-card-content";
      const date = document.createElement("div"); date.className = "news-date"; date.textContent = post.date || "Update";
      const title = document.createElement("h2"); title.textContent = post.title || "Task Karate update";
      const summary = document.createElement("p"); summary.textContent = post.summary || post.subtitle || "Read the latest from Task Karate.";
      content.append(date, title, summary); article.append(content); mount.append(article);
    });
  } catch (error) { mount.innerHTML = '<div class="empty-state">The news archive is temporarily unavailable. Please check back or contact the school for current updates.</div>'; console.error(error); }
})();
