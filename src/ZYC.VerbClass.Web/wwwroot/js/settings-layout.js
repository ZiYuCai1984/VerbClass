(() => {
  const activeClass = "is-current";
  const pageSelector = "[data-settings-page]";
  const navLinkSelector = ".settings-nav__item[href^=\"#\"]";

  const getHash = () => window.location.hash || "";
  const getTarget = hash => hash.length > 1 ? document.getElementById(hash.slice(1)) : null;

  const getActiveLink = (root, hash) => {
    const links = Array.from(root.querySelectorAll(navLinkSelector));
    return links.find(link => link.getAttribute("href") === hash)
      || links.find(link => link.classList.contains(activeClass))
      || links[0]
      || null;
  };

  const setActiveLink = (root, hash) => {
    const activeLink = getActiveLink(root, hash);
    root.querySelectorAll(navLinkSelector).forEach(link => {
      link.classList.toggle(activeClass, link === activeLink);
    });
  };

  const syncFromHash = shouldScroll => {
    const hash = getHash();
    document.querySelectorAll(pageSelector).forEach(root => {
      setActiveLink(root, hash);

      const target = getTarget(hash);
      if (shouldScroll && target && root.contains(target)) {
        target.scrollIntoView({ block: "start" });
      }
    });
  };

  document.addEventListener("click", event => {
    if (!(event.target instanceof Element)) {
      return;
    }

    const link = event.target.closest(`${pageSelector} ${navLinkSelector}`);
    if (!link) {
      return;
    }

    const root = link.closest(pageSelector);
    if (root) {
      setActiveLink(root, link.getAttribute("href") || "");
    }
  });

  window.addEventListener("hashchange", () => syncFromHash(false));

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", () => syncFromHash(true));
  } else {
    syncFromHash(true);
  }
})();
