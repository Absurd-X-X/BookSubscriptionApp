const Modal = (() => {
    const TYPE_CONFIG = {
        loading: { icon: null, dismissible: false },
        uploading: { icon: 'ti-cloud-upload', dismissible: false },
        success: { icon: 'ti-circle-check', badgeClass: 'is-success', dismissible: true },
        error: { icon: 'ti-alert-circle', badgeClass: 'is-error', dismissible: true },
        confirmDelete: { icon: 'ti-alert-triangle', badgeClass: 'is-error', dismissible: true },
    };

    let overlay = null;
    let card = null;

    function ensureDom() {
        if (overlay) return;

        overlay = document.createElement('div');
        overlay.className = 'mode-overlay';
        overlay.setAttribute('role', 'dialog');
        overlay.setAttribute('aria-modal', 'true');

        card = document.createElement('div');
        card.className = 'mode-card';
        overlay.appendChild(card);

        document.body.appendChild(overlay);
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

    function buildConfirmInput(requiredText, onValidChange) {
        const wrap = document.createElement('div');
        wrap.className = 'notify-confirm-input-wrap';

        const input = document.createElement('input');
        input.type = 'text';
        input.className = 'notify-confirm-input';
        input.autocomplete = 'off';
        input.spellcheck = false;
        input.placeholder = `Type ${requiredText} to confirm`;

        input.addEventListener('input', () => {
            onValidChange(input.value.trim() === requiredText);
        });

        wrap.appendChild(input);

        // autofocus after it's in the DOM
        setTimeout(() => input.focus(), 50);

        return { wrap, input };
    }

    function show(type, { title, message, progress, onConfirm, confirmLabel, requiredText, formAction, formMethod } = {}) {
        const config = TYPE_CONFIG[type];
        if (!config) {
            console.error(`Modal: unknown type "${type}"`);
            return;
        }

        ensureDom();
        card.innerHTML = '';
        card.dataset.modalType = type;

        const header = document.createElement('div');
        header.className = 'modal-header';
        header.appendChild(buildBadge(type, config));

        if (config.dismissible) {
            const closeBtn = document.createElement('button');
            closeBtn.className = 'notify-close';
            closeBtn.setAttribute('aria-label', 'Dismiss');
            closeBtn.innerHTML = '<i class="ti ti-x" aria-hidden="true"></i>';
            closeBtn.addEventListener('click', close);
            header.appendChild(closeBtn);
        }

        card.appendChild(header);

        const titleEl = document.createElement('p');
        titleEl.className = 'notify-title';
        titleEl.textContent = title || '';
        card.appendChild(titleEl);

        const msgEl = document.createElement('p');
        msgEl.className = 'notify-message';
        msgEl.textContent = message || '';
        card.appendChild(msgEl);

        if (typeof progress === 'number') {
            card.appendChild(buildProgress(progress));
        }

        let confirmBtnRef = null;

        if (type === 'confirmDelete') {
            const required = requiredText || 'DELETE';
            const { wrap, input } = buildConfirmInput(required, (isValid) => {
                if (confirmBtnRef) confirmBtnRef.disabled = !isValid;
            });
            card.appendChild(wrap);
        }

        if (config.dismissible) {
            const actions = document.createElement('div');
            actions.className = 'modal-actions';

            if (type === 'error') {
                const retryBtn = document.createElement('button');
                retryBtn.className = 'modal-btn is-primary';
                retryBtn.textContent = confirmLabel || 'Try again';
                retryBtn.addEventListener('click', () => {
                    close();
                    if (onConfirm) onConfirm();
                });
                actions.appendChild(retryBtn);
            } else if (type === 'confirmDelete') {
                const cancelBtn = document.createElement('button');
                cancelBtn.className = 'modal-btn';
                cancelBtn.textContent = 'Cancel';
                cancelBtn.addEventListener('click', close);
                actions.appendChild(cancelBtn);

                const confirmBtn = document.createElement('button');
                confirmBtn.className = 'modal-btn is-primary is-danger';
                confirmBtn.textContent = confirmLabel || 'Delete';
                confirmBtn.disabled = true;
                confirmBtn.addEventListener('click', () => {
                    if (formAction) {
                        const form = document.createElement('form');
                        form.method = formMethod || 'post';
                        form.action = formAction;
                        document.body.appendChild(form);
                        form.submit();
                        return;
                    }
                    close();
                    if (onConfirm) onConfirm();
                });
                confirmBtnRef = confirmBtn;
                actions.appendChild(confirmBtn);
            } else {
                const okBtn = document.createElement('button');
                okBtn.className = 'modal-btn is-primary';
                okBtn.textContent = confirmLabel || 'Done';
                okBtn.addEventListener('click', () => {
                    close();
                    if (onConfirm) onConfirm();
                });
                actions.appendChild(okBtn);
            }

            card.appendChild(actions);
        }

        overlay.classList.add('is-visible');

        overlay.onclick = (e) => {
            if (e.target === overlay && config.dismissible) close();
        };
    }

    function update({ title, message, progress } = {}) {
        if (!card) return;
        if (title !== undefined) {
            const el = card.querySelector('.notify-title');
            if (el) el.textContent = title;
        }
        if (message !== undefined) {
            const el = card.querySelector('.notify-message');
            if (el) el.textContent = message;
        }
        if (progress !== undefined) {
            let progressEl = card.querySelector('.notify-progress');
            if (!progressEl) {
                progressEl = buildProgress(progress);
                card.appendChild(progressEl);
            } else {
                progressEl.querySelector('.notify-progress-fill').style.width = progress + '%';
                progressEl.querySelector('.notify-progress-label').textContent = progress + '%';
            }
        }
    }

    function close() {
        if (!overlay) return;
        overlay.classList.remove('is-visible');
    }

    return { show, update, close };
})();