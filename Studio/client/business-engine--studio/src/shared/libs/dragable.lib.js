/**
 * SimpleDragable - A lightweight modern drag & drop library
 * Compatible with ES2020+
 */
export class Dragable {
    constructor(selectors, options = {}) {
        this.items = selectors;
        this.options = {
            drop: options.drop || '[data-drop]',
            animationDuration: options.animationDuration ?? 200,
            onOver: options.onOver || (() => { }),
            onOut: options.onOut || (() => { }),
            onDrop: options.onDrop || (() => { }),
        };

        // Internal state
        this.ghost = null;          // The cloned element following the cursor
        this.source = null;         // The original dragged element
        this.currentDrop = null;    // Currently hovered drop zone
        this.offsetX = 0;
        this.offsetY = 0;
        this._init();
    }

    /**
     * Attach pointer listeners to all draggable items
     */
    _init() {
        this.items.forEach((el) => {
            el.style.touchAction = 'none'; // Prevent default touch scrolling
            el.addEventListener('pointerdown', this._onPointerDown);
            el.__dragable = true;
        });
    }

    /**
     * Start dragging: create ghost clone and bind move/up listeners
     */
    _onPointerDown = (e) => {
        if (e.button !== undefined && e.button !== 0) return; // Left click / touch only

        // Prevent text selection while dragging
        e.preventDefault();

        // Disable text selection globally during drag
        document.body.style.userSelect = 'none';
        document.body.style.webkitUserSelect = 'none';

        this.source = e.currentTarget;
        const rect = this.source.getBoundingClientRect();
        this.offsetX = e.clientX - rect.left;
        this.offsetY = e.clientY - rect.top;

        // Create the floating clone
        this.ghost = this.source.cloneNode(true);
        Object.assign(this.ghost.style, {
            position: 'fixed',
            top: `${rect.top}px`,
            left: `${rect.left}px`,
            width: `${rect.width}px`,
            height: `${rect.height}px`,
            margin: '0',
            pointerEvents: 'none',
            zIndex: '9999',
            opacity: '0',
            transform: 'scale(0.95)',
            transition: `opacity ${this.options.animationDuration}ms ease, transform ${this.options.animationDuration}ms ease`,
            willChange: 'transform, top, left',
        });
        document.body.appendChild(this.ghost);

        // Smooth fade-in animation
        requestAnimationFrame(() => {
            this.ghost.style.opacity = '0.9';
            this.ghost.style.transform = 'scale(1)';
        });

        // Slightly dim the source element
        this.source.style.opacity = '0.4';
        document.addEventListener('pointermove', this._onPointerMove);
        document.addEventListener('pointerup', this._onPointerUp);
    };

    /**
     * Move ghost with cursor and detect drop zone hover
     */
    _onPointerMove = (e) => {
        if (!this.ghost) return;
        // Position the ghost smoothly
        this.ghost.style.top = `${e.clientY - this.offsetY}px`;
        this.ghost.style.left = `${e.clientX - this.offsetX}px`;
        // Detect element under cursor (ignoring the ghost)
        const target = document.elementFromPoint(e.clientX, e.clientY);
        const dropZone = target?.closest(this.options.drop) ?? null;
        if (dropZone !== this.currentDrop) {
            // Leaving previous drop zone
            if (this.currentDrop) {
                this.options.onOut({ from: this.source, to: this.currentDrop, event: e });
            }
            // Entering new drop zone
            if (dropZone) {
                this.options.onOver({ from: this.source, to: dropZone, event: e });
            }
            this.currentDrop = dropZone;
        }
    };

    /**
     * Finalize drag: trigger drop callback and clean up with smooth animation
     */
    _onPointerUp = (e) => {
        document.removeEventListener('pointermove', this._onPointerMove);
        document.removeEventListener('pointerup', this._onPointerUp);

        // Restore text selection
        document.body.style.userSelect = '';
        document.body.style.webkitUserSelect = '';

        if (!this.ghost) return;

        // Successful drop
        if (this.currentDrop) {
            this.options.onOut({ from: this.source, to: this.currentDrop, event: e });
            this.options.onDrop({
                from: this.source,
                to: this.currentDrop,
                event: e,
            });
        }

        // Fade-out animation, then remove ghost
        this.ghost.style.opacity = '0';
        this.ghost.style.transform = 'scale(0.9)';
        const ghostRef = this.ghost;
        setTimeout(() => ghostRef.remove(), this.options.animationDuration);

        // Reset state
        this.source.style.opacity = '';
        this.ghost = null;
        this.source = null;
        this.currentDrop = null;
    };

    /**
     * Detach all event listeners (cleanup)
     */
    destroy() {
        this.items.forEach((el) => el.removeEventListener('pointerdown', this._onPointerDown));
    }
}
