(() => {
    const dialog = document.getElementById('verbclass-modal');
    const content = document.getElementById('verbclass-modal-content');
    const requestSource = document.getElementById('verbclass-modal-request');

    if (!(dialog instanceof HTMLDialogElement) || !(content instanceof HTMLElement) || !(requestSource instanceof HTMLElement)) {
        return;
    }

    class AppModal {
        constructor(dialogElement, contentElement, requestElement) {
            this.dialog = dialogElement;
            this.content = contentElement;
            this.requestSource = requestElement;
            this.currentUrl = '';
            this.handleDocumentClick = this.handleDocumentClick.bind(this);
            this.handleDialogClick = this.handleDialogClick.bind(this);
            this.handleDialogClose = this.handleDialogClose.bind(this);
            this.handleRequestError = this.handleRequestError.bind(this);
        }

        init() {
            document.addEventListener('click', this.handleDocumentClick);
            this.dialog.addEventListener('click', this.handleDialogClick);
            this.dialog.addEventListener('close', this.handleDialogClose);
            window.addEventListener('htmx:responseError', this.handleRequestError);
            window.addEventListener('htmx:sendError', this.handleRequestError);
            window.addEventListener('htmx:timeout', this.handleRequestError);
        }

        open(url) {
            const normalizedUrl = typeof url === 'string' ? url.trim() : '';
            if (!normalizedUrl || typeof window.htmx?.ajax !== 'function') {
                return Promise.resolve();
            }

            this.currentUrl = normalizedUrl;
            this.renderState('Loading...', 'Fetching modal content from the server.');

            if (!this.dialog.open) {
                this.dialog.showModal();
            }

            window.htmx.trigger(this.requestSource, 'htmx:abort');

            return window.htmx.ajax('GET', normalizedUrl, {
                source: this.requestSource,
                target: this.content,
                swap: 'innerHTML'
            }).catch(() => {
                this.renderState('Request failed', 'The modal content could not be loaded.');
            });
        }

        close() {
            if (this.dialog.open) {
                this.dialog.close();
            } else {
                this.reset();
            }
        }

        reload() {
            if (!this.currentUrl) {
                return Promise.resolve();
            }

            return this.open(this.currentUrl);
        }

        handleDocumentClick(event) {
            const trigger = event.target instanceof Element ? event.target.closest('[data-modal-url]') : null;
            if (!(trigger instanceof HTMLElement)) {
                return;
            }

            if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
                return;
            }

            const url = trigger.dataset.modalUrl?.trim();
            if (!url) {
                return;
            }

            event.preventDefault();
            this.open(url);
        }

        handleDialogClick(event) {
            const closeButton = event.target instanceof Element ? event.target.closest('[data-modal-close]') : null;
            if (closeButton instanceof HTMLElement) {
                event.preventDefault();
                this.close();
            }
        }

        handleDialogClose() {
            this.reset();
        }

        handleRequestError(event) {
            if (event.detail?.target !== this.content) {
                return;
            }

            this.renderState('Request failed', 'The modal content could not be loaded.');
        }

        renderState(title, description) {
            this.content.innerHTML = `
<div class="modal-state">
    <strong>${this.escapeHtml(title)}</strong>
    <p>${this.escapeHtml(description)}</p>
</div>`;
        }

        reset() {
            window.htmx.trigger(this.requestSource, 'htmx:abort');
            this.currentUrl = '';
            this.content.innerHTML = '';
        }

        escapeHtml(value) {
            return String(value)
                .replaceAll('&', '&amp;')
                .replaceAll('<', '&lt;')
                .replaceAll('>', '&gt;')
                .replaceAll('"', '&quot;')
                .replaceAll("'", '&#39;');
        }
    }

    const appModal = new AppModal(dialog, content, requestSource);
    appModal.init();
    window.appModal = appModal;
})();
