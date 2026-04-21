function initUserMenus() {
    const menus = Array.from(document.querySelectorAll("[data-user-menu]"));
    if (!menus.length) {
        return;
    }

    function closeAllMenus(exceptMenu) {
        menus.forEach((menu) => {
            if (menu !== exceptMenu) {
                menu.classList.remove("is-open");
                const trigger = menu.querySelector("[data-user-menu-trigger]");
                if (trigger) {
                    trigger.setAttribute("aria-expanded", "false");
                }
            }
        });
    }

    menus.forEach((menu) => {
        const trigger = menu.querySelector("[data-user-menu-trigger]");
        if (!trigger) {
            return;
        }

        trigger.addEventListener("click", () => {
            const willOpen = !menu.classList.contains("is-open");
            closeAllMenus(menu);
            menu.classList.toggle("is-open", willOpen);
            trigger.setAttribute("aria-expanded", willOpen ? "true" : "false");
        });
    });

    document.addEventListener("click", (event) => {
        const target = event.target;
        if (!(target instanceof Element)) {
            return;
        }

        if (!target.closest("[data-user-menu]")) {
            closeAllMenus();
        }
    });

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeAllMenus();
        }
    });
}

document.addEventListener("DOMContentLoaded", () => {
    initUserMenus();
});
