const Modal = (() => {
    const TYPE_CONFIG = {
        loading: { icon: null, dismissible: false },
        uploading: { icon: 'ti-cloud-upload', dismissible: false },
        success: { icon: 'ti-circle-check', badgeClass: 'is-success', dismissible: true },
        error: { icon: 'ti-alert-circle', badgeClass: 'is-error', dismissible: true },
        confirmDelete: { icon: 'ti-alert-triangle', badgeClass: 'is-error', dismissible: true },
        editNote: { icon: 'ti-edit', dismissible: true },
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

    function resetCardStyle() {
        card.style.width = '';
        card.style.textAlign = '';
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
        resetCardStyle();
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

    function showEditNote({ title, initialValue, onSave, saveLabel } = {}) {
        const config = TYPE_CONFIG.editNote;

        ensureDom();
        card.innerHTML = '';
        resetCardStyle();
        card.dataset.modalType = 'editNote';

        const header = document.createElement('div');
        header.className = 'modal-header';
        header.appendChild(buildBadge('editNote', config));

        const closeBtn = document.createElement('button');
        closeBtn.className = 'notify-close';
        closeBtn.setAttribute('aria-label', 'Dismiss');
        closeBtn.innerHTML = '<i class="ti ti-x" aria-hidden="true"></i>';
        closeBtn.addEventListener('click', close);
        header.appendChild(closeBtn);

        card.appendChild(header);

        const titleEl = document.createElement('p');
        titleEl.className = 'notify-title';
        titleEl.textContent = title || 'Edit Note';
        card.appendChild(titleEl);

        const textarea = document.createElement('textarea');
        textarea.className = 'notify-confirm-input';
        textarea.style.minHeight = '90px';
        textarea.style.resize = 'vertical';
        textarea.value = initialValue || '';
        card.appendChild(textarea);
        setTimeout(() => textarea.focus(), 50);

        const actions = document.createElement('div');
        actions.className = 'modal-actions';

        const cancelBtn = document.createElement('button');
        cancelBtn.className = 'modal-btn';
        cancelBtn.textContent = 'Cancel';
        cancelBtn.addEventListener('click', close);
        actions.appendChild(cancelBtn);

        const saveBtn = document.createElement('button');
        saveBtn.className = 'modal-btn is-primary';
        saveBtn.textContent = saveLabel || 'Save';
        saveBtn.addEventListener('click', () => {
            const value = textarea.value.trim();
            close();
            if (onSave) onSave(value);
        });
        actions.appendChild(saveBtn);

        card.appendChild(actions);

        overlay.classList.add('is-visible');
        overlay.onclick = (e) => { if (e.target === overlay) close(); };
    }

    function showReadingGoal({
        goalType = 'Books',
        target = 50,
        deadline = '',
        motivation = '',
        postUrl,
        antiForgeryToken
    } = {}) {
        ensureDom();
        card.innerHTML = '';
        card.dataset.modalType = 'readingGoal';
        card.style.width = '420px';
        card.style.textAlign = 'left';

        const header = document.createElement('div');
        header.className = 'modal-header';

        const badge = document.createElement('div');
        badge.className = 'notify-badge';
        badge.innerHTML = '<i class="ti ti-target" aria-hidden="true"></i>';
        header.appendChild(badge);

        const closeBtn = document.createElement('button');
        closeBtn.className = 'notify-close';
        closeBtn.setAttribute('aria-label', 'Dismiss');
        closeBtn.innerHTML = '<i class="ti ti-x" aria-hidden="true"></i>';
        closeBtn.addEventListener('click', close);
        header.appendChild(closeBtn);

        card.appendChild(header);

        const titleEl = document.createElement('p');
        titleEl.className = 'notify-title';
        titleEl.textContent = 'Edit Reading Goal';
        card.appendChild(titleEl);

        const subEl = document.createElement('p');
        subEl.className = 'notify-message';
        subEl.textContent = 'Set a new goal to keep yourself motivated and track your progress.';
        card.appendChild(subEl);

        const body = document.createElement('div');
        body.className = 'goal-form-body';
        body.innerHTML = `
            <div class="goal-form-group">
                <label class="goal-label">Goal Type</label>
                <div class="goal-type-grid">
                    <button type="button" class="goal-type-option" data-value="Books">
                        <span class="goal-type-check"><i class="ti ti-check" aria-hidden="true"></i></span>
                        <i class="ti ti-book goal-type-icon" aria-hidden="true"></i>
                        <span class="goal-type-name">Books</span>
                        <span class="goal-type-desc">Number of books</span>
                    </button>
                    <button type="button" class="goal-type-option" data-value="Pages">
                        <span class="goal-type-check"><i class="ti ti-check" aria-hidden="true"></i></span>
                        <i class="ti ti-file-text goal-type-icon" aria-hidden="true"></i>
                        <span class="goal-type-name">Pages</span>
                        <span class="goal-type-desc">Number of pages</span>
                    </button>
                </div>
            </div>

            <div class="goal-form-group">
                <label class="goal-label">Target</label>
                <div class="goal-input-row">
                    <input type="number" min="1" class="notify-confirm-input goal-target-input" value="${target}" />
                    <span class="goal-unit-label">books</span>
                </div>
                <p class="goal-help-text">Set your target number of books to read.</p>
            </div>

            <div class="goal-form-group">
                <label class="goal-label">Deadline (Optional)</label>
                <input type="date" class="notify-confirm-input goal-deadline-input" value="${deadline}" />
                <p class="goal-help-text">Choose a deadline to achieve your goal.</p>
            </div>

            <div class="goal-form-group">
                <label class="goal-label">Motivation (Optional)</label>
                <textarea class="notify-confirm-input goal-motivation-input" style="min-height:70px;resize:vertical;" placeholder="Read more, learn more, and grow every day!">${motivation}</textarea>
                <p class="goal-help-text">Add a personal note to keep you motivated.</p>
            </div>

            <div class="goal-form-group">
                <label class="goal-label">Preview</label>
                <div class="goal-preview-box">
                    <i class="ti ti-target" aria-hidden="true"></i>
                    <span class="goal-preview-text"></span>
                </div>
            </div>
        `;
        card.appendChild(body);

        const typeButtons = body.querySelectorAll('.goal-type-option');
        const unitLabel = body.querySelector('.goal-unit-label');
        const targetInput = body.querySelector('.goal-target-input');
        const deadlineInput = body.querySelector('.goal-deadline-input');
        const motivationInput = body.querySelector('.goal-motivation-input');
        const previewText = body.querySelector('.goal-preview-text');
        const helpText = body.querySelector('.goal-help-text');

        let selectedType = goalType;

        function setUnit() {
            const unit = selectedType === 'Pages' ? 'pages' : 'books';
            unitLabel.textContent = unit;
            helpText.textContent = `Set your target number of ${unit} to read.`;
        }

        function updatePreview() {
            const unit = selectedType === 'Pages' ? 'pages' : 'books';
            const t = targetInput.value || '0';
            const d = deadlineInput.value
                ? ` by ${new Date(deadlineInput.value).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' })}`
                : '';
            previewText.textContent = `You're aiming to read ${t} ${unit}${d}.`;
        }

        function selectType(value) {
            selectedType = value;
            typeButtons.forEach(btn => {
                btn.classList.toggle('is-selected', btn.dataset.value === value);
            });
            setUnit();
            updatePreview();
        }

        typeButtons.forEach(btn => {
            btn.addEventListener('click', () => selectType(btn.dataset.value));
        });

        targetInput.addEventListener('input', updatePreview);
        deadlineInput.addEventListener('input', updatePreview);

        selectType(goalType);

        const actions = document.createElement('div');
        actions.className = 'modal-actions';

        const cancelBtn = document.createElement('button');
        cancelBtn.className = 'modal-btn';
        cancelBtn.textContent = 'Cancel';
        cancelBtn.addEventListener('click', close);
        actions.appendChild(cancelBtn);

        const saveBtn = document.createElement('button');
        saveBtn.className = 'modal-btn is-primary';
        saveBtn.textContent = 'Save Goal';
        saveBtn.addEventListener('click', () => {
            const form = document.createElement('form');
            form.method = 'post';
            form.action = postUrl;

            const fields = {
                __RequestVerificationToken: antiForgeryToken,
                type: selectedType,
                target: targetInput.value,
                deadline: deadlineInput.value,
                motivation: motivationInput.value
            };

            Object.entries(fields).forEach(([name, value]) => {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = name;
                input.value = value || '';
                form.appendChild(input);
            });

            document.body.appendChild(form);
            form.submit();
        });
        actions.appendChild(saveBtn);

        card.appendChild(actions);

        overlay.classList.add('is-visible');
        overlay.onclick = (e) => { if (e.target === overlay) close(); };
    }

    function showFundWallet({ postUrl, antiForgeryToken } = {}) {
        ensureDom();
        card.innerHTML = '';
        card.dataset.modalType = 'fundWallet';
        card.style.width = '380px';
        card.style.textAlign = 'left';

        const header = document.createElement('div');
        header.className = 'modal-header';

        const badge = document.createElement('div');
        badge.className = 'notify-badge';
        badge.innerHTML = '<i class="ti ti-wallet" aria-hidden="true"></i>';
        header.appendChild(badge);

        const closeBtn = document.createElement('button');
        closeBtn.className = 'notify-close';
        closeBtn.setAttribute('aria-label', 'Dismiss');
        closeBtn.innerHTML = '<i class="ti ti-x" aria-hidden="true"></i>';
        closeBtn.addEventListener('click', close);
        header.appendChild(closeBtn);

        card.appendChild(header);

        const titleEl = document.createElement('p');
        titleEl.className = 'notify-title';
        titleEl.textContent = 'Fund Wallet';
        card.appendChild(titleEl);

        const subEl = document.createElement('p');
        subEl.className = 'notify-message';
        subEl.textContent = 'Enter an amount to add to your wallet. You will be redirected to complete payment securely.';
        card.appendChild(subEl);

        const body = document.createElement('div');
        body.className = 'goal-form-body';
        body.innerHTML = `
            <div class="goal-form-group">
                <label class="goal-label">Amount</label>
                <div class="goal-input-row">
                    <input type="number" min="100" step="0.01" class="notify-confirm-input fund-amount-input" placeholder="0.00" />
                    <span class="goal-unit-label">NGN</span>
                </div>
                <p class="goal-help-text fund-error-text" style="color:#ef4444;display:none;"></p>
            </div>

            <div class="goal-form-group">
                <label class="goal-label">Quick Select</label>
                <div class="goal-type-grid fund-quick-grid">
                    <button type="button" class="goal-type-option fund-quick-option" data-amount="1000">
                        <span class="goal-type-name">₦1,000</span>
                    </button>
                    <button type="button" class="goal-type-option fund-quick-option" data-amount="5000">
                        <span class="goal-type-name">₦5,000</span>
                    </button>
                    <button type="button" class="goal-type-option fund-quick-option" data-amount="10000">
                        <span class="goal-type-name">₦10,000</span>
                    </button>
                    <button type="button" class="goal-type-option fund-quick-option" data-amount="20000">
                        <span class="goal-type-name">₦20,000</span>
                    </button>
                </div>
            </div>
        `;
        card.appendChild(body);

        const amountInput = body.querySelector('.fund-amount-input');
        const errorText = body.querySelector('.fund-error-text');
        const quickOptions = body.querySelectorAll('.fund-quick-option');

        quickOptions.forEach(btn => {
            btn.addEventListener('click', () => {
                amountInput.value = btn.dataset.amount;
                quickOptions.forEach(b => b.classList.remove('is-selected'));
                btn.classList.add('is-selected');
                errorText.style.display = 'none';
            });
        });

        amountInput.addEventListener('input', () => {
            quickOptions.forEach(b => b.classList.remove('is-selected'));
            errorText.style.display = 'none';
        });

        setTimeout(() => amountInput.focus(), 50);

        const actions = document.createElement('div');
        actions.className = 'modal-actions';

        const cancelBtn = document.createElement('button');
        cancelBtn.className = 'modal-btn';
        cancelBtn.textContent = 'Cancel';
        cancelBtn.addEventListener('click', close);
        actions.appendChild(cancelBtn);

        const payBtn = document.createElement('button');
        payBtn.className = 'modal-btn is-primary';
        payBtn.textContent = 'Proceed to Pay';
        payBtn.addEventListener('click', () => {
            const amount = parseFloat(amountInput.value);

            if (!amount || amount <= 0) {
                errorText.textContent = 'Please enter a valid amount.';
                errorText.style.display = 'block';
                return;
            }
            if (amount < 100) {
                errorText.textContent = 'Minimum funding amount is ₦100.';
                errorText.style.display = 'block';
                return;
            }

            payBtn.disabled = true;
            payBtn.textContent = 'Redirecting…';

            const form = document.createElement('form');
            form.method = 'post';
            form.action = postUrl;

            const fields = {
                __RequestVerificationToken: antiForgeryToken,
                amount: amount
            };

            Object.entries(fields).forEach(([name, value]) => {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = name;
                input.value = value;
                form.appendChild(input);
            });

            document.body.appendChild(form);
            form.submit();
        });
        actions.appendChild(payBtn);

        card.appendChild(actions);

        overlay.classList.add('is-visible');
        overlay.onclick = (e) => { if (e.target === overlay) close(); };
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

    return { show, showEditNote, showReadingGoal, showFundWallet, update, close };
})();