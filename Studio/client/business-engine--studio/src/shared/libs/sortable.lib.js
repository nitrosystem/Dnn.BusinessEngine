/**
 * SimpleSortable - A lightweight vertical sortable list
 *
 * Features:
 * - Zero dependencies
 * - Smooth FLIP animations via Web Animations API
 * - Y-axis only dragging (locked X) for predictable UX
 * - Custom ghost (no browser drag image)
 * - Auto-scroll when near viewport edges
 * - Handle-based dragging with event delegation
 * - Cross-container drag via `group` option
 * - Safe against double initialization
 */
// Global registry: groupName -> Set<SimpleSortable>
const GROUPS = new Map();
export class Sortable {
    constructor(container, {
        group = null,
        targetAttr = '',
        handle = '[data-handle="true"]',
        onEnd,
        scrollSpeed = 15,
        scrollZone = 60,
        apexY = 0,
        animationDuration = 200
    } = {}) {
        if (container.__simpleSortable) container.__simpleSortable.destroy();
        container.__simpleSortable = this;

        this.container = container;
        this.group = group;
        this.targetAttr = targetAttr;
        this.handle = handle;
        this.onEnd = onEnd;
        this.scrollSpeed = scrollSpeed;
        this.scrollZone = scrollZone;
        this.animationDuration = animationDuration;
        this.apexY = apexY;
        this.dragEl = null;
        this.ghostEl = null;
        this.fromContainer = null;
        this.activeContainer = null;
        this.pointerOffsetY = 0;
        this.startWidth = 0;
        this.startHeight = 0;
        this.pointerId = null;
        this.scrollRAF = null;
        this.lastClientY = 0;
        this.lastClientX = 0;
        this._onPointerDown = this._onPointerDown.bind(this);
        this._onPointerMove = this._onPointerMove.bind(this);
        this._onPointerUp = this._onPointerUp.bind(this);

        this.container.addEventListener('pointerdown', this._onPointerDown);

        // Register in group
        if (this.group) {
            if (!GROUPS.has(this.group)) GROUPS.set(this.group, new Set());
            GROUPS.get(this.group).add(this);
        }
    }

    /** Returns all containers sharing the same group (including self) */
    _getGroupContainers() {
        if (!this.group || !GROUPS.has(this.group)) return [this.container];

        return [...GROUPS.get(this.group)].map(s => s.container);
    }

    _onPointerDown(e) {
        if (e.button !== undefined && e.button !== 0) return;

        const item = e.target.closest(this.targetAttr);
        if (!item || item.parentElement !== this.container) return;

        if (!e.target.closest(this.handle)) return;

        e.preventDefault();

        this.dragEl = item;
        this.fromContainer = this.container;
        this.activeContainer = this.container;
        this.pointerId = e.pointerId;
        this.lastClientY = e.clientY;
        this.lastClientX = e.clientX;
        const rect = item.getBoundingClientRect();
        this.startWidth = rect.width;
        this.startHeight = rect.height;
        this.pointerOffsetY = e.clientY - rect.top;

        this.ghostEl = item.cloneNode(true);
        Object.assign(this.ghostEl.style, {
            position: 'fixed',
            left: `${rect.left}px`,
            top: `${rect.top + this.apexY}px`,
            width: `${rect.width}px`,
            height: `${rect.height}px`,
            margin: '0',
            pointerEvents: 'none',
            zIndex: '9999'
        });
        document.body.appendChild(this.ghostEl);

        item.classList.add('sortable-dragging');

        window.addEventListener('pointermove', this._onPointerMove);
        window.addEventListener('pointerup', this._onPointerUp);
        window.addEventListener('pointercancel', this._onPointerUp);
    }

    _onPointerMove(e) {
        if (e.pointerId !== this.pointerId || !this.dragEl) return;

        this.lastClientY = e.clientY;
        this.lastClientX = e.clientX;

        const newTop = e.clientY - this.pointerOffsetY;
        this.ghostEl.style.top = `${newTop + this.apexY}px`;

        this._updatePosition(e.clientX, e.clientY);
        this._handleAutoScroll(e.clientY);
    }

    /** Find which group-container the pointer is over (prefers deepest/nested) */
    _findTargetContainer(clientX, clientY) {
        const containers = this._getGroupContainers();
        let best = null;
        let bestArea = Infinity;
        for (const c of containers) {
            const r = c.getBoundingClientRect();
            if (clientX >= r.left && clientX <= r.right &&
                clientY >= r.top && clientY <= r.bottom) {

                // Prefer the smallest (innermost) container when nested
                const area = r.width * r.height;
                if (area < bestArea) {
                    bestArea = area;
                    best = c;
                }
            }
        }

        return best || this.activeContainer;
    }
    _updatePosition(clientX, clientY) {
        const targetContainer = this._findTargetContainer(clientX, clientY);
        const changingContainer = targetContainer !== this.activeContainer;

        // Items from both old and new containers get animated via FLIP
        const affectedContainers = changingContainer
            ? [this.activeContainer, targetContainer]
            : [targetContainer];
        const allItems = [];
        affectedContainers.forEach(c => {
            allItems.push(...c.querySelectorAll(`:scope > ${this.targetAttr}`));
        });

        const firstRects = new Map(allItems.map(el => [el, el.getBoundingClientRect()]));
        const afterEl = this._getDragAfterElement(targetContainer, clientY);
        let moved = false;
        if (afterEl == null) {
            // Append to end (or move to empty container)
            const isLastInSameContainer =
                this.dragEl.parentElement === targetContainer &&
                this.dragEl.nextElementSibling === null;
            if (!isLastInSameContainer) {
                targetContainer.appendChild(this.dragEl);
                moved = true;
            }
        } else if (afterEl !== this.dragEl && afterEl !== this.dragEl.nextElementSibling) {
            targetContainer.insertBefore(this.dragEl, afterEl);
            moved = true;
        }

        if (!moved) return;

        this.activeContainer = targetContainer;

        // FLIP: animate all affected items
        requestAnimationFrame(() => {
            allItems.forEach(el => {
                if (el === this.dragEl) return;
                const first = firstRects.get(el);
                if (!first) return;
                const last = el.getBoundingClientRect();
                const dy = first.top - last.top;
                if (dy) {
                    el.animate(
                        [{ transform: `translateY(${dy}px)` }, { transform: 'translateY(0)' }],
                        { duration: this.animationDuration, easing: 'cubic-bezier(0.2, 0, 0, 1)' }
                    );
                }
            });
        });
    }

    _getDragAfterElement(container, y) {
        const items = [...container.querySelectorAll(`:scope > ${this.targetAttr}:not(.sortable-dragging)`)];
        return items.reduce((closest, child) => {
            const box = child.getBoundingClientRect();
            const offset = y - box.top - box.height / 2;
            if (offset < 0 && offset > closest.offset) {
                return { offset, element: child };
            }

            return closest;
        }, { offset: -Infinity }).element;
    }

    _handleAutoScroll(clientY) {
        const { scrollZone, scrollSpeed } = this;
        const viewportH = window.innerHeight;
        let speed = 0;
        if (clientY < scrollZone) {
            speed = -scrollSpeed * (1 - clientY / scrollZone);
        } else if (clientY > viewportH - scrollZone) {
            speed = scrollSpeed * (1 - (viewportH - clientY) / scrollZone);
        }

        if (speed !== 0) this._startAutoScroll(speed);
        else this._stopAutoScroll();
    }

    _startAutoScroll(speed) {
        if (this.scrollRAF) {
            this._scrollSpeed = speed;
            return;
        }

        this._scrollSpeed = speed;
        const scrollParent = this._getScrollParent(this.activeContainer || this.container);
        const step = () => {
            scrollParent.scrollBy(0, this._scrollSpeed);
            this._updatePosition(this.lastClientX, this.lastClientY);
            this.scrollRAF = requestAnimationFrame(step);
        };

        this.scrollRAF = requestAnimationFrame(step);
    }

    _stopAutoScroll() {
        if (this.scrollRAF) {
            cancelAnimationFrame(this.scrollRAF);
            this.scrollRAF = null;
        }
    }

    _getScrollParent(node) {
        let el = node.parentElement;
        while (el) {
            const { overflowY } = getComputedStyle(el);
            if (/(auto|scroll|overlay)/.test(overflowY) && el.scrollHeight > el.clientHeight) {
                return el;
            }
            el = el.parentElement;
        }
        return window;
    }

    _onPointerUp(e) {
        if (e.pointerId !== this.pointerId) return;

        this._stopAutoScroll();

        window.removeEventListener('pointermove', this._onPointerMove);
        window.removeEventListener('pointerup', this._onPointerUp);
        window.removeEventListener('pointercancel', this._onPointerUp);

        if (this.ghostEl) {
            this.ghostEl.remove();
            this.ghostEl = null;
        }
        if (this.dragEl) {
            this.dragEl.classList.remove('sortable-dragging');
        }

        const finishedEl = this.dragEl;
        const from = this.fromContainer;
        const to = this.activeContainer;

        this.dragEl = null;
        this.fromContainer = null;
        this.activeContainer = null;
        this.pointerId = null;
        if (finishedEl && typeof this.onEnd === 'function') {
            // Fire onEnd on the instance that OWNS the destination container,
            // mirroring SortableJS behavior (where onEnd of the source fires with evt.to).
            // We'll just call our own onEnd and pass full context.
            this.onEnd({
                item: finishedEl,
                from,
                to
            });
        }
    }

    destroy() {
        this._stopAutoScroll();
        this.container.removeEventListener('pointerdown', this._onPointerDown);

        window.removeEventListener('pointermove', this._onPointerMove);
        window.removeEventListener('pointerup', this._onPointerUp);
        window.removeEventListener('pointercancel', this._onPointerUp);

        if (this.ghostEl) {
            this.ghostEl.remove();
            this.ghostEl = null;
        }

        // Unregister from group
        if (this.group && GROUPS.has(this.group)) {
            const set = GROUPS.get(this.group);
            set.delete(this);
            if (set.size === 0) GROUPS.delete(this.group);
        }

        delete this.container.__simpleSortable;
    }
}