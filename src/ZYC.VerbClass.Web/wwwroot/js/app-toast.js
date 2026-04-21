

(() => {
    const componentTagName = 'verbclass-toast-region';
    const initialToastsElementId = 'verbclass-initial-toasts';
    const toastEventName = 'verbclass:toast';

    class VerbClassToastRegion extends HTMLElement {
        static durations = {
            info: 4200,
            warn: 5600,
            error: 7200
        };

        static labels = {
            info: 'Info',
            warn: 'Warn',
            error: 'Error'
        };

        static maxVisibleToasts = 5;

        constructor() {
            super();

            this.attachShadow({ mode: 'open' });
            this.shadowRoot.innerHTML = `
<style>
    :host {
        all: initial;
        position: fixed;
        right: 1rem;
        bottom: 1rem;
        z-index: 1100;
        display: block;
        width: min(22rem, calc(100vw - 1.5rem));
        color-scheme: light;
        color: #1f2937;
        font-family: "Segoe UI", "Microsoft YaHei UI", sans-serif;
        font-size: 14px;
        font-weight: 400;
        line-height: 1.4;
        isolation: isolate;
        pointer-events: none;
        text-align: left;
    }

    :host,
    :host *,
    :host *::before,
    :host *::after {
        box-sizing: border-box;
    }

    .stack {
        display: flex;
        flex-direction: column;
        gap: 0.75rem;
        width: 100%;
    }

    .toast {
        --toast-accent: #2563eb;
        pointer-events: auto;
        display: grid;
        grid-template-columns: auto 1fr auto;
        gap: 0.75rem;
        align-items: start;
        padding: 0.9rem 0.95rem;
        border: 1px solid rgba(148, 163, 184, 0.3);
        border-left: 0.35rem solid var(--toast-accent);
        border-radius: 0;
        background: #fff;
        box-shadow: none;
        backdrop-filter: none;
        color: inherit;
        font: inherit;
        opacity: 0;
        transform: translateY(0.7rem);
        transition: opacity 160ms ease, transform 160ms ease;
    }

    .toast.is-visible {
        opacity: 1;
        transform: translateY(0);
    }

    .toast.is-closing {
        opacity: 0;
        transform: translateY(0.7rem);
    }

    .toast[data-type="warn"] {
        --toast-accent: #d97706;
    }

    .toast[data-type="error"] {
        --toast-accent: #dc2626;
    }

    .label {
        font-size: 0.72rem;
        font-weight: 700;
        line-height: 1.2;
        letter-spacing: 0.08em;
        text-transform: uppercase;
        color: var(--toast-accent);
        padding-top: 0.18rem;
    }

    .body {
        min-width: 0;
    }

    .title {
        margin: 0 0 0.25rem;
        font-weight: 700;
        line-height: 1.3;
    }

    .message {
        margin: 0;
        line-height: 1.4;
        color: #475569;
        word-break: break-word;
    }

    .close {
        appearance: none;
        -webkit-appearance: none;
        border: 0;
        border-radius: 0;
        background: transparent;
        box-shadow: none;
        color: #475569;
        font: inherit;
        font-size: 0.82rem;
        line-height: 1;
        padding: 0.15rem 0;
        margin: 0;
        min-width: auto;
        cursor: pointer;
        text-align: left;
        text-decoration: none;
        text-transform: none;
    }

    .close:is(:hover, :focus) {
        color: var(--toast-accent);
        background: transparent;
    }

    @media (max-width: 640px) {
        :host {
            left: 0.75rem;
            right: 0.75rem;
            bottom: 0.75rem;
            width: auto;
        }
    }

    @media (prefers-reduced-motion: reduce) {
        .toast {
            transition: none;
        }
    }
</style>
<div class="stack"></div>`;

            this.stack = this.shadowRoot.querySelector('.stack');
        }

        show(input) {
            const payload = this.#normalizePayload(input);

            if (Array.isArray(payload)) {
                payload.forEach(item => this.#renderToast(this.#normalizeToast(item)));
                return;
            }

            this.#renderToast(this.#normalizeToast(payload));
        }

        info(message, options) {
            return this.#renderToast(this.#createToast('info', message, options));
        }

        warn(message, options) {
            return this.#renderToast(this.#createToast('warn', message, options));
        }

        error(message, options) {
            return this.#renderToast(this.#createToast('error', message, options));
        }

        #normalizePayload(detail) {
            if (detail == null) {
                return null;
            }

            if (Array.isArray(detail)) {
                return detail;
            }

            if (typeof detail === 'object' && Array.isArray(detail.value)) {
                return detail.value;
            }

            if (typeof detail === 'object' && detail.value != null) {
                return detail.value;
            }

            return detail;
        }

        #createToast(type, message, options) {
            if (typeof message === 'object' && message !== null) {
                return this.#normalizeToast(message, type);
            }

            const normalizedOptions = typeof options === 'string'
                ? { title: options }
                : options ?? {};

            return this.#normalizeToast(
                {
                    ...normalizedOptions,
                    type: type,
                    message: message
                },
                type
            );
        }

        #normalizeToast(input, fallbackType) {
            if (input == null) {
                return null;
            }

            if (typeof input === 'string') {
                return {
                    type: fallbackType ?? 'info',
                    message: input,
                    title: ''
                };
            }

            if (typeof input !== 'object') {
                return null;
            }

            const type = typeof input.type === 'string' ? input.type.toLowerCase() : fallbackType ?? 'info';
            let title = typeof input.title === 'string' ? input.title.trim() : '';
            let message = typeof input.message === 'string'
                ? input.message.trim()
                : typeof input.value === 'string'
                    ? input.value.trim()
                    : '';

            if (!message && !title) {
                return null;
            }

            if (!message && title) {
                message = title;
                title = '';
            }

            return {
                type: type === 'warn' || type === 'error' ? type : 'info',
                title: title,
                message: message
            };
        }

        #renderToast(toast) {
            if (!toast || !this.stack) {
                return null;
            }

            const element = document.createElement('section');
            const label = document.createElement('div');
            const body = document.createElement('div');
            const closeButton = document.createElement('button');

            element.className = 'toast';
            element.dataset.type = toast.type;

            label.className = 'label';
            label.textContent = VerbClassToastRegion.labels[toast.type] ?? VerbClassToastRegion.labels.info;

            body.className = 'body';

            if (toast.title) {
                const title = document.createElement('div');
                title.className = 'title';
                title.textContent = toast.title;
                body.appendChild(title);
            }

            const message = document.createElement('div');
            message.className = 'message';
            message.textContent = toast.message;
            body.appendChild(message);

            closeButton.type = 'button';
            closeButton.className = 'close';
            closeButton.textContent = 'X';
            closeButton.addEventListener('click', () => this.#dismissToast(element));

            element.append(label, body, closeButton);
            this.stack.appendChild(element);

            window.requestAnimationFrame(() => {
                element.classList.add('is-visible');
            });

            this.#scheduleDismiss(element, VerbClassToastRegion.durations[toast.type] ?? VerbClassToastRegion.durations.info);
            this.#trimToastStack();

            return element;
        }

        #dismissToast(element) {
            if (!(element instanceof HTMLElement) || element.dataset.state === 'closing') {
                return;
            }

            element.dataset.state = 'closing';
            element.classList.remove('is-visible');
            element.classList.add('is-closing');

            window.setTimeout(() => {
                element.remove();
            }, 180);
        }

        #scheduleDismiss(element, duration) {
            let timeoutId = 0;

            const start = () => {
                timeoutId = window.setTimeout(() => this.#dismissToast(element), duration);
            };

            const stop = () => {
                if (timeoutId !== 0) {
                    window.clearTimeout(timeoutId);
                    timeoutId = 0;
                }
            };

            element.addEventListener('mouseenter', stop);
            element.addEventListener('mouseleave', start);

            start();
        }

        #trimToastStack() {
            if (!this.stack) {
                return;
            }

            const overflow = this.stack.children.length - VerbClassToastRegion.maxVisibleToasts;
            for (let index = 0; index < overflow; index += 1) {
                this.stack.firstElementChild?.remove();
            }
        }
    }

    if (!customElements.get(componentTagName)) {
        customElements.define(componentTagName, VerbClassToastRegion);
    }

    function ensureToastRegion() {
        let region = document.querySelector(componentTagName);
        if (region instanceof VerbClassToastRegion) {
            return region;
        }

        region = document.createElement(componentTagName);
        document.body.appendChild(region);
        return region;
    }

    function readInitialToasts(region) {
        const element = document.getElementById(initialToastsElementId);
        if (!(element instanceof HTMLScriptElement)) {
            return;
        }

        const raw = element.textContent?.trim();
        if (!raw) {
            return;
        }

        try {
            region.show(JSON.parse(raw));
        } catch {
            region.error('Initial toast payload could not be parsed.', 'Toast bootstrap failed');
        }
    }

    const region = ensureToastRegion();
    const api = {
        show(input) {
            return region.show(input);
        },
        info(message, options) {
            return region.info(message, options);
        },
        warn(message, options) {
            return region.warn(message, options);
        },
        error(message, options) {
            return region.error(message, options);
        }
    };

    window.appToast = api;

    if (!window.toast) {
        window.toast = api;
    }

    window.addEventListener(toastEventName, event => {
        region.show(event.detail);
    });

    window.addEventListener('htmx:sendError', () => {
        region.error('Please check your network connection and try again.', 'Network error');
    });

    window.addEventListener('htmx:timeout', () => {
        region.error('The request timed out before the server responded.', 'Request timeout');
    });

    window.addEventListener('htmx:responseError', event => {
        const status = event?.detail?.xhr?.status;
        const statusText = event?.detail?.xhr?.statusText;
        const suffix = typeof status === 'number' && status > 0
            ? ` (${status}${statusText ? ` ${statusText}` : ''})`
            : '';

        region.error(`The server returned an unexpected response${suffix}.`, 'Request failed');
    });

    readInitialToasts(region);
})();
