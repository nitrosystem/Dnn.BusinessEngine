ComponentRegistry.register("Captcha", function (controller, globalService) {
    this.init = (field, element) => {
        this._render(field, element);
        this.refresh();

        field.validateMethod = this.verify;
    }

    this.refresh = async () => {
        try {
            this._setLoading(true);

            const module = 'BE_ExtraExtensions';
            let baseUrl = `/API/` + module + '/';

            if (typeof $.ServicesFramework === 'function') {
                const sf = $.ServicesFramework();
                baseUrl = sf.getServiceRoot(module);
            }

            const apiUrl = baseUrl + 'Service/GenerateCaptcha';
            const res = await fetch(apiUrl);

            if (!res.ok)
                throw new Error('CAPTCHA loading is incorrect.');

            const data = await res.json();
            this._token = data.Token;

            this.elements.image.src = data.ImageBase64;
            this.elements.input.value = '';

            this.clearStatus();
        }
        catch (err) {
            this._showError('Error loading CAPTCHA. Please try again.');

            console.error(err);
        } finally {
            this._setLoading(false);
        }
    }

    this.verify = async (field) => {
        try {
            const answer = this.elements.input.value.trim();
            const module = 'BE_ExtraExtensions';
            let baseUrl = `/API/` + module + '/';

            if (typeof $.ServicesFramework === 'function') {
                const sf = $.ServicesFramework();
                baseUrl = sf.getServiceRoot(module);
            }

            const apiUrl = baseUrl + 'Service/VerifyCaptcha';
            const res = await fetch(apiUrl, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    token: this._token,
                    answer: answer
                })
            });

            const data = await res.json();
            if (data.success) {
                this.clearStatus();

                return true;
            }
            else if (!data.success) {
                this._showError(field.Settings.IncorrectCaptchaMessage ?? 'The code is incorrect.');

                return false;
            }
        }
        catch (err) {
            this._showError('Error!!');

            console.error(err);

            return false;
        }
    }

    this.reset = async () => {
        this.elements.input.value = '';

        this.clearStatus();
        this.refresh();
    }

    this.destroy = () => {
        this.container.innerHTML = '';
        this.elements = {};
        this._token = '';
    }

    this.clearStatus = () => {
        this.elements.status.textContent = '';
        this.elements.status.className = 'captcha-status';
    }

    this._render = (field, element) => {
        element.innerHTML = `
            <div class="captcha-wrapper">
                <div class="captcha-image-box">
                    <img class="captcha-image" alt="captcha" />
                    <button type="button" class="captcha-refresh" title="Reload Captcha">
                        <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-arrow-clockwise" viewBox="0 0 16 16">
                        <path fill-rule="evenodd" d="M8 3a5 5 0 1 0 4.546 2.914.5.5 0 0 1 .908-.417A6 6 0 1 1 8 2z"/>
                        <path d="M8 4.466V.534a.25.25 0 0 1 .41-.192l2.36 1.966c.12.1.12.284 0 .384L8.41 4.658A.25.25 0 0 1 8 4.466"/>
                        </svg>
                    </button>
                </div>
                <input 
                    type="text" 
                    class="captcha-input" 
                    placeholder="${field.Settings.Placeholder ?? ''}"
                    maxlength="6"
                    autocomplete="off"
                    spellcheck="false"
                />
                <div class="captcha-status"></div>
            </div>
        `;

        this.elements = {
            image: element.querySelector('.captcha-image'),
            input: element.querySelector('.captcha-input'),
            refreshBtn: element.querySelector('.captcha-refresh'),
            status: element.querySelector('.captcha-status')
        };

        this.elements.refreshBtn.addEventListener('click', () => this.refresh());
        this.elements.input.addEventListener('input', () => this.clearStatus());
    }

    this._setLoading = (loading) => {
        this.elements.refreshBtn.disabled = loading;
        this.elements.refreshBtn.style.opacity = loading ? '0.5' : '1';
    }

    this._showError = (msg) => {
        this.elements.status.textContent = msg;
        this.elements.status.className = 'captcha-status captcha-error';
    }
});
