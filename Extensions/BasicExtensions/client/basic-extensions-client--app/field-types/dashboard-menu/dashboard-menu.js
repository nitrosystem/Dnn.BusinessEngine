ComponentRegistry.register("DashboardMenu", function (controller, globalService) {
    this.init = (field) => {
        field.__ignoreReinit = true;
      
        const currentPage = globalService.getParameterByName('page');
        const expandedNodes = findExpandedPath(controller.dashboard.Pages, currentPage) || new Set();
        const menuHtml = buildMenuItems(controller.dashboard.Pages, currentPage, expandedNodes);
        const finalHtml = `<ul class="menu-nav">${menuHtml}</ul>`;
        const elm = document.getElementById(`dshMenu${field.FieldName}`);

        if (elm) {
            elm.setHTMLUnsafe(finalHtml);
            elm.querySelectorAll(".expand-toggle.expanded > .sub-menu").forEach(child => {
                child.style.height = child.scrollHeight + "px";
            });
            elm.querySelectorAll(".expand-toggle > a").forEach(anchor => {
                anchor.addEventListener("click", (e) => {
                    const li = e.currentTarget.parentElement;
                    const child = li.querySelector(".sub-menu");
                    if (!child) return;

                    if (li.classList.contains("expanded")) {
                        child.style.height = child.scrollHeight + "px";
                        requestAnimationFrame(() => {
                            child.style.height = "0";
                        });
                    } else {
                        child.style.height = child.scrollHeight + "px";
                    }

                    li.classList.toggle("expanded");
                });
            });
        }
    };

    function buildMenuItems(pages, currentPage, expandedNodes) {
        let html = "";

        for (const page of pages.filter(p => p.IsVisible)) {
            let classes = "menu-item";

            if (currentPage && page.PageType === 1 && page.PageName === currentPage) {
                classes += " active";
            }

            if (page.PageType === 1) {
                html += `
                <li class="${classes}">
                    <a href="${page.Url || "#"}" class="menu-link">
                        ${page.Icon ? `<i class="menu-icon ${page.Icon}"></i>` : ""}
                        <span class="menu-text">${page.Title}</span>
                    </a>
                </li>`;
            }
            else if (page.PageType === 3) {
                html += `
                <li class="${classes} menu-section">
                    <a><span class="menu-label">${page.Title}</span></a>
                </li>`;
                continue;
            }
            else {
                if (page.Pages?.length) {
                    classes += " expand-toggle";
                }

                if (expandedNodes.has(page.PageName)) {
                    classes += " expanded";
                }

                if (page.Pages?.length) {
                    html += `
                    <li class="${classes}">
                        <a href="javascript:void(0);">
                            ${page.Icon ? `<i class="menu-icon ${page.Icon}"></i>` : ""}
                            <span class="menu-text">${page.Title}</span>
                        </a>
                        <ul class="sub-menu">${buildMenuItems(page.Pages, currentPage, expandedNodes)}</ul>
                    </li>`;
                    continue;
                }

                const baseUrl = location.origin + location.pathname;
                const link = page.PageType === 1
                    ? (page.Url || "#")
                    : `${baseUrl}/?page=${page.PageName}`;

                html += `
                <li class="${classes}">
                    <a href="${link}" class="menu-link">
                        ${page.Icon ? `<i class="menu-icon ${page.Icon}"></i>` : ""}
                        <span class="menu-text">${page.Title}</span>
                    </a>
                </li>`;
            }
        }

        return html;
    }

    function findExpandedPath(pages, targetPage, path = new Set()) {
        for (const page of pages) {
            if (page.PageName === targetPage) {
                return path;
            }
            if (page.Pages?.length) {
                const result = findExpandedPath(page.Pages, targetPage, new Set(path).add(page.PageName));
                if (result) return result;
            }
        }
        return null;
    }
});

