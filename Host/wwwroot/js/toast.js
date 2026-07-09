

const Toast = (() => {
    const TYPE_CONFIG = {
        loading: {
            icon: null, // spinner is used instead of an icon
            autoDismiss: false,
        },
        uploading: {
            icon: 'ti-cloud-upload',
            autoDismiss: false,
        },
        success: {
            icon: 'ti-circle-check',
            autoDismiss: 3500,
            badgeClass: 'is-success',
        },
        error: {
            icon: 'ti-alert-circle',
            autoDismiss: 5000, // errors stay a little longer
            badgeClass: 'is-error',
        },
    };

    let container = null;
    let counter = 0;

    function ensureContainer() {
        if (container) return container;
        container = document.createElement('div');
        container.className = 'toast-container';
        container.setAttribute('role', 'region');
        container.setAttribute('aria-label', 'Notifications');
        document.body.appendChild(container);
        return container;
    }

    function buildBadge(type, config) {
        const badge = document.createElement('div');
        badge.className = 'notify-badge' + (config.badgeClass ? ' ' + config.badgeClass : '');

        if (type === 'loading') {
            const spinner = document.createElement('div');
            spinner.className = 'notify-spinner';
            badge.appendChild(spinner);
        } else {
            const icon = document.createElement('i');
            icon.className = 'ti ' + config.icon;
            icon.setAttribute('aria-hidden', 'true');
            badge.appendChild(icon);
        }
        return badge;
    }

    function buildProgress(progress) {
        const wrap = document.createElement('div');
        wrap.className = 'notify-progress';
        wrap.innerHTML = `
      <div class="notify-progress-track">
        <div class="notify-progress-fill" style="width:${progress}%"></div>
      </div>
      <span class="notify-progress-label">${progress}%</span>
    `;
        return wrap;
    }

    function show(type, { title, message, progress } = {}) {
        const config = TYPE_CONFIG[type];
        if (!config) {
            console.error(`Toast: unknown type "${type}"`);
            return null;
        }

        ensureContainer();

        const id = ++counter;
        const el = document.createElement('div');
        el.className = 'toast';
        el.dataset.toastId = id;
        el.setAttribute('role', type === 'error' ? 'alert' : 'status');

        el.appendChild(buildBadge(type, config));

        const body = document.createElement('div');
        body.className = 'toast-body';

        const titleEl = document.createElement('p');
        titleEl.className = 'notify-title';
        titleEl.textContent = title || '';
        body.appendChild(titleEl);

        const msgEl = document.createElement('p');
        msgEl.className = 'notify-message';
        msgEl.textContent = message || '';
        body.appendChild(msgEl);

        if (typeof progress === 'number') {
            body.appendChild(buildProgress(progress));
        }

        el.appendChild(body);

        const closeBtn = document.createElement('button');
        closeBtn.className = 'notify-close';
        closeBtn.setAttribute('aria-label', 'Dismiss');
        closeBtn.innerHTML = '<i class="ti ti-x" aria-hidden="true"></i>';
        closeBtn.addEventListener('click', () => dismiss(id));
        el.appendChild(closeBtn);

        container.appendChild(el);
        requestAnimationFrame(() => el.classList.add('is-visible'));

        if (config.autoDismiss) {
            setTimeout(() => dismiss(id), config.autoDismiss);
        }

        return id;
    }

    function update(id, { title, message, progress } = {}) {
        const el = container?.querySelector(`[data-toast-id="${id}"]`);
        if (!el) return;

        if (title !== undefined) {
            el.querySelector('.notify-title').textContent = title;
        }
        if (message !== undefined) {
            el.querySelector('.notify-message').textContent = message;
        }
        if (progress !== undefined) {
            let progressEl = el.querySelector('.notify-progress');
            if (!progressEl) {
                progressEl = buildProgress(progress);
                el.querySelector('.toast-body').appendChild(progressEl);
            } else {
                progressEl.querySelector('.notify-progress-fill').style.width = progress + '%';
                progressEl.querySelector('.notify-progress-label').textContent = progress + '%';
            }
        }
    }

    function dismiss(id) {
        const el = container?.querySelector(`[data-toast-id="${id}"]`);
        if (!el) return;
        el.classList.remove('is-visible');
        el.classList.add('is-leaving');
        setTimeout(() => el.remove(), 220);
    }

    return { show, update, dismiss };
})();