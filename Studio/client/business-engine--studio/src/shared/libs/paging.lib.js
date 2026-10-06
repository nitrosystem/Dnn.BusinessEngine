/**
 * Paging - A lightweight, modern pagination component
 */
export class Paging {
    // Default configuration
    static _defaults = {
        totalPages: 1,
        visiblePages: 5,
        startPage: 1,
        pageClass: 'page-item',
        first: 'First',
        prev: 'Previous',
        next: 'Next',
        last: 'Last',
        onPageClick: null,
    };

    constructor(element, options = {}) {
        // Resolve element from selector or DOM node
        this.element = typeof element === 'string'
            ? document.querySelector(element)
            : element;
        if (!this.element) {
            throw new Error('Paging: target element not found');
        }

        this.options = { ...Paging._defaults, ...options };
        this.currentPage = this._clamp(this.options.startPage);
        this._bindEvents();
        this._render();
        this.element.classList.add('pagination');
    }

    /**
     * Public API: navigate to a specific page programmatically
     */
    show(page) {
        const target = this._clamp(page);
        if (target === this.currentPage) return;

        this.currentPage = target;
        this._render();
    }

    /**
     * Public API: get current page
     */
    getCurrentPage() {
        return this.currentPage;
    }

    /**
     * Public API: destroy the component and clean up
     */
    destroy() {
        this.element.removeEventListener('click', this._handleClick);
        this.element.classList.remove('pagination');
        this.element.innerHTML = '';
    }

    // Clamp page within valid range
    _clamp(page) {
        return Math.min(Math.max(1, page), this.options.totalPages);
    }

    // Compute visible page range with smart sliding window
    _getVisibleRange() {
        const { visiblePages, totalPages } = this.options;
        const half = Math.floor(visiblePages / 2);
        let start = this.currentPage - half;
        let end = this.currentPage + (visiblePages - half - 1);

        if (start < 1) {
            start = 1;
            end = Math.min(visiblePages, totalPages);
        }

        if (end > totalPages) {
            end = totalPages;
            start = Math.max(1, totalPages - visiblePages + 1);
        }

        return { start, end };
    }

    // Build a single <li> item
    _buildItem(label, page, { active = false, disabled = false } = {}) {
        const li = document.createElement('li');
        li.className = this.options.pageClass;

        if (active) li.classList.add('active');
        if (disabled) li.classList.add('disabled');

        const a = document.createElement('a');
        a.className = 'page-link';
        a.href = '#';
        a.textContent = label;
        a.dataset.page = page;

        if (disabled) a.setAttribute('aria-disabled', 'true');
        if (active) a.setAttribute('aria-current', 'page');

        li.appendChild(a);
        return li;
    }

    // Render the full pagination UI
    _render() {
        const { totalPages, first, prev, next, last } = this.options;
        const current = this.currentPage;
        const { start, end } = this._getVisibleRange();
        const fragment = document.createDocumentFragment(); // Use DocumentFragment for efficient DOM updates
        
        if (first) fragment.appendChild(
            this._buildItem(first, 1, { disabled: current === 1 })
        );

        if (prev) fragment.appendChild(
            this._buildItem(prev, current - 1, { disabled: current === 1 })
        );

        for (let i = start; i <= end; i++) {
            fragment.appendChild(
                this._buildItem(i, i, { active: i === current })
            );
        }
        
        if (next) fragment.appendChild(
            this._buildItem(next, current + 1, { disabled: current === totalPages })
        );
        
        if (last) fragment.appendChild(
            this._buildItem(last, totalPages, { disabled: current === totalPages })
        );
        
        this.element.replaceChildren(fragment);
    }

    // Bind delegated click events
    _bindEvents() {
        this.element.addEventListener('click', this._handleClick);
    }

    // Arrow function preserves `this` binding
    _handleClick = (event) => {
        const link = event.target.closest('a[data-page]');
        if (!link) return;

        event.preventDefault();

        const li = link.parentElement;
        if (li.classList.contains('disabled') || li.classList.contains('active')) return;

        const page = Number(link.dataset.page);
        if (!Number.isInteger(page)) return;

        this.show(page);
        
        // Fire user callback with rich payload
        this.options.onPageClick?.({
            event,
            currentPage: this.currentPage,
            totalPages: this.options.totalPages,
        });
    };
}
