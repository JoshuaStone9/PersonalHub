(() => {
    const form = document.getElementById('prepare-next-month');
    if (!form) return;
    const choices = [...form.querySelectorAll('input[name="selectedEntries"]')];
    const submit = document.getElementById('prepare-submit');
    const currency = new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP' });
    const update = () => {
        const selected = choices.filter(input => input.checked);
        const total = income => selected.filter(input => (input.dataset.source === 'income') === income)
            .reduce((sum, input) => {
                const amount = input.closest('.fm-prepare-next-item')?.querySelector('.fm-prepare-amount');
                return sum + Math.round(Number(amount?.value ?? input.dataset.amount) * 100);
            }, 0) / 100;
        document.getElementById('prepare-count').textContent = selected.length;
        document.getElementById('prepare-income').textContent = currency.format(total(true));
        document.getElementById('prepare-allocations').textContent = currency.format(total(false));
        submit.textContent = selected.length ? `Prepare ${submit.dataset.month}` : 'Continue without adding entries';
    };
    form.querySelectorAll('.fm-prepare-inline-editor').forEach(editor => {
        const trigger = editor.querySelector('.fm-inline-amount-trigger');
        const panel = editor.querySelector('.fm-prepare-amount-form');
        const input = editor.querySelector('.fm-prepare-amount');
        const original = input?.value;
        const siblingDate = editor.closest('.fm-prepare-next-item')?.querySelector('.fm-prepare-date-inline-editor');
        const row = editor.closest('.fm-prepare-next-item');
        const closeOtherEditors = () => form.querySelectorAll('.fm-prepare-inline-editor').forEach(other => {
            if (other === editor) return;
            other.classList.remove('is-editing');
            other.closest('.fm-prepare-next-item')?.classList.remove('amount-editing');
            other.querySelector('form')?.setAttribute('hidden', '');
            other.querySelector('button.fm-inline-amount-trigger')?.removeAttribute('hidden');
        });
        trigger?.addEventListener('click', () => { closeOtherEditors(); editor.classList.add('is-editing'); row?.classList.add('amount-editing'); if (siblingDate) siblingDate.hidden = true; trigger.hidden = true; panel.hidden = false; input?.focus(); input?.select(); });
        editor.querySelector('.fm-prepare-editor-save')?.addEventListener('click', () => { editor.classList.remove('is-editing'); row?.classList.remove('amount-editing'); if (siblingDate) siblingDate.hidden = false; panel.hidden = true; trigger.hidden = false; update(); });
        editor.querySelector('.fm-prepare-editor-cancel')?.addEventListener('click', () => { if (input) input.value = original ?? input.value; editor.classList.remove('is-editing'); row?.classList.remove('amount-editing'); if (siblingDate) siblingDate.hidden = false; panel.hidden = true; trigger.hidden = false; update(); });
    });
    form.querySelectorAll('.fm-prepare-date-inline-editor').forEach(editor => {
        const trigger = editor.querySelector('.fm-prepare-date-trigger');
        const panel = editor.querySelector('.fm-prepare-date-form');
        const input = editor.querySelector('.fm-prepare-date');
        let original = input?.value;
        const siblingAmount = editor.closest('.fm-prepare-next-item')?.querySelector('.fm-prepare-inline-editor:not(.fm-prepare-date-inline-editor)');
        trigger?.addEventListener('click', () => { form.querySelectorAll('.fm-prepare-inline-editor form:not([hidden])').forEach(open => { open.hidden = true; open.closest('.fm-prepare-inline-editor')?.querySelector('.fm-inline-amount-trigger')?.removeAttribute('hidden'); }); original = input?.value; if (siblingAmount) siblingAmount.hidden = true; trigger.hidden = true; panel.hidden = false; input?.focus(); });
        editor.querySelector('.fm-prepare-editor-save')?.addEventListener('click', () => { if (siblingAmount) siblingAmount.hidden = false; panel.hidden = true; trigger.hidden = false; });
        editor.querySelector('.fm-prepare-editor-cancel')?.addEventListener('click', () => { if (input && original) input.value = original; if (siblingAmount) siblingAmount.hidden = false; panel.hidden = true; trigger.hidden = false; });
    });
    const desktopEditors = false;
    form.addEventListener('change', update);
    form.addEventListener('input', event => {
        if (event.target.matches('.fm-prepare-amount')) update();
    });
    form.querySelectorAll('.fm-prepare-value-editor, .fm-prepare-date-editor').forEach(editor => {
        const input = editor.querySelector('input');
        editor.querySelector('summary')?.addEventListener('click', () => {
            if (input) editor.dataset.originalValue = input.value;
        });
        editor.querySelector('.fm-prepare-editor-save')?.addEventListener('click', () => editor.removeAttribute('open'));
        editor.querySelector('.fm-prepare-editor-cancel')?.addEventListener('click', () => {
            if (input && editor.dataset.originalValue !== undefined) input.value = editor.dataset.originalValue;
            editor.removeAttribute('open');
            update();
        });
    });
    form.querySelectorAll('[data-prepare-select]').forEach(button => {
        button.addEventListener('click', () => {
            button.closest('fieldset').querySelectorAll('input[name="selectedEntries"]').forEach(input => {
                input.checked = button.dataset.prepareSelect === 'all';
            });
            update();
        });
    });
    form.addEventListener('submit', event => {
        const selectedCount = choices.filter(input => input.checked).length;
        if (selectedCount > 0 && !window.confirm(`Prepare ${submit.dataset.month} with ${selectedCount} selected entr${selectedCount === 1 ? 'y' : 'ies'}? This will add the entries and update selected pot balances.`)) {
            event.preventDefault();
            return;
        }
        submit.disabled = true;
    });
    document.addEventListener('click', event => {
        if (desktopEditors) return;
        document.querySelectorAll('.fm-prepare-value-editor[open], .fm-prepare-date-editor[open]').forEach(editor => {
            if (!editor.contains(event.target)) editor.removeAttribute('open');
        });
    });
    window.addEventListener('pageshow', () => { submit.disabled = false; update(); });
    update();
})();
