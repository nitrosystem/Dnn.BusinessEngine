/**
 * Minimal image lightbox
 * Zero dependencies, ~3KB, modern JS
 */
export class Lightbox {
    constructor(container, options = {}) {
        this.options = {
            leftArrowIcon: '‹',
            rightArrowIcon: '›',
            closeIcon: '×',
            loop: true,
            ...options,
        };
        const links = container.querySelectorAll('a');
        this.images = [...links].map(a => ({
            src: a.getAttribute('href'),
            title: a.getAttribute('title') ?? '',
        }));

        // Event delegation — single listener on container
        container.addEventListener('click', (e) => {
            const link = e.target.closest('a');
            if (!link || !container.contains(link)) return;

            e.preventDefault();

            const index = [...links].indexOf(link);
            this._open(index);
        });
    }

    _open(index) {
        this.currentIndex = index;
        this._render();
        this._show();

        document.addEventListener('keydown', this._onKeydown);
    }

    _close = () => {
        this.overlay?.classList.remove('slb-visible');
        setTimeout(() => {
            this.overlay?.remove();
            this.overlay = null;
        }, 200);

        document.removeEventListener('keydown', this._onKeydown);
    };

    _onKeydown = (e) => {
        if (e.key === 'Escape') this._close();
        else if (e.key === 'ArrowLeft') this._prev();
        else if (e.key === 'ArrowRight') this._next();
    };

   _prev = () => {
    const { loop } = this.options;
    if (this.currentIndex > 0) this.currentIndex--;
    else if (loop) this.currentIndex = this.images.length - 1;
    else return;
this._updateImage(-1); // جهت چپ
};
_next = () => {
    const { loop } = this.options;
    if (this.currentIndex < this.images.length - 1) this.currentIndex++;
    else if (loop) this.currentIndex = 0;
    else return;
this._updateImage(1); // جهت راست
};

   _updateImage(direction = 0) {
    const { src, title } = this.images[this.currentIndex];
// اگر تصویر قبلی وجود داره، افکت خروج بزن
    const oldImg = this.imgEl;
// ساخت تصویر جدید
    const newImg = document.createElement('img');
    newImg.className = 'slb-image';
    newImg.alt = title;
// تعیین جهت ورود بر اساس direction
    if (direction > 0) {
        newImg.classList.add('slb-enter-right');
    } else if (direction < 0) {
        newImg.classList.add('slb-enter-left');
    } else {
        newImg.classList.add('slb-enter-fade');
    }
newImg.onload = () => {
        // اضافه کردن تصویر جدید به DOM
        oldImg.parentNode.insertBefore(newImg, oldImg);
// افکت خروج تصویر قدیمی
        if (direction > 0) {
            oldImg.classList.add('slb-exit-left');
        } else if (direction < 0) {
            oldImg.classList.add('slb-exit-right');
        } else {
            oldImg.classList.add('slb-exit-fade');
        }
// در فریم بعدی، افکت ورود رو فعال کن
        requestAnimationFrame(() => {
            newImg.classList.add('slb-loaded');
        });
// حذف تصویر قدیمی بعد از پایان انیمیشن
        setTimeout(() => {
            oldImg.remove();
        }, 400);
this.imgEl = newImg;
    };
newImg.src = src;
    this.captionEl.textContent = title;
}


    _render() {
        const { leftArrowIcon, rightArrowIcon, closeIcon } = this.options;
        const showNav = this.images.length > 1;
        this.overlay = document.createElement('div');
        this.overlay.className = 'slb-overlay';
        this.overlay.innerHTML = `
        <button class="slb-btn slb-close" aria-label="Close">
            <i class="${closeIcon}">${closeIcon.includes(' ') ? '' : closeIcon}</i>
        </button>
        ${showNav ? `
            <button class="slb-btn slb-prev" aria-label="Previous">
            <i class="${leftArrowIcon}">${leftArrowIcon.includes(' ') ? '' : leftArrowIcon}</i>
            </button>
            <button class="slb-btn slb-next" aria-label="Next">
            <i class="${rightArrowIcon}">${rightArrowIcon.includes(' ') ? '' : rightArrowIcon}</i>
            </button>
        ` : ''}
        <figure class="slb-figure">
            <img class="slb-image" alt="" />
            <figcaption class="slb-caption"></figcaption>
        </figure>
        `;

        this.imgEl = this.overlay.querySelector('.slb-image');
        this.captionEl = this.overlay.querySelector('.slb-caption');

        // Bind events
        this.overlay.querySelector('.slb-close').addEventListener('click', this._close);
        this.overlay.querySelector('.slb-prev')?.addEventListener('click', this._prev);
        this.overlay.querySelector('.slb-next')?.addEventListener('click', this._next);

        // Click outside image closes
        this.overlay.addEventListener('click', (e) => {
            if (e.target === this.overlay) this._close();
        });

        document.body.appendChild(this.overlay);
        this._updateImage();
    }

    _show() {
        // Trigger transition on next frame
        requestAnimationFrame(() => this.overlay.classList.add('slb-visible'));
    }
}
