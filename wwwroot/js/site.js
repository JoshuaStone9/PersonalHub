document.addEventListener('click', e => {
  const opener = e.target.closest('[data-open]');
  if (opener) document.getElementById(opener.dataset.open)?.classList.add('show');
  if (e.target.matches('[data-close]') || e.target.classList.contains('modal')) e.target.closest('.modal')?.classList.remove('show');
});


// Personal Vault merged scripts
// ── Lucide icons ────────────────────────────────────────────────────────────
if (window.lucide) if (window.lucide) { lucide.createIcons(); }

// ── Theme toggle (persisted) ───────────────────────────────────────────────
const THEME_KEY = 'pv-theme';
const root = document.documentElement;
const saved = localStorage.getItem(THEME_KEY);
if (saved) root.setAttribute('data-theme', saved);

document.querySelectorAll('#themeToggle, #themeToggleInline, .theme-toggle, .theme-toggle-inline').forEach(btn => {
    btn.addEventListener('click', () => {
        const next = root.getAttribute('data-theme') === 'light' ? 'dark' : 'light';
        root.setAttribute('data-theme', next);
        localStorage.setItem(THEME_KEY, next);
    });
});

// ── Sidebar toggle (mobile) ────────────────────────────────────────────────
document.getElementById('sidebarToggle')?.addEventListener('click', () => {
    document.getElementById('sidebar')?.classList.toggle('open');
});

// ── Confirm before delete forms ────────────────────────────────────────────
document.querySelectorAll('form[data-confirm]').forEach(form => {
    form.addEventListener('submit', e => {
        if (!confirm(form.dataset.confirm || 'Are you sure?')) e.preventDefault();
    });
});

// ── AJAX writes with post-write navigation ────────────────────────────────
const OPERATION_MESSAGE_KEY = 'finance-operation-message';

try {
    const message = sessionStorage.getItem(OPERATION_MESSAGE_KEY);
    const target = document.querySelector('[data-operation-message-target]');
    if (message && target) {
        sessionStorage.removeItem(OPERATION_MESSAGE_KEY);
        const alert = document.createElement('div');
        alert.className = document.getElementById('fmAppShell')
            ? 'fm-alert fm-alert-success'
            : 'toast toast-success';
        alert.setAttribute('role', 'status');
        const text = document.createElement('strong');
        text.textContent = message;
        alert.append(text);
        target.prepend(alert);
    }
} catch { /* storage may be unavailable */ }

document.querySelectorAll('form[data-ajax-redirect]').forEach(form => {
    form.addEventListener('submit', async event => {
        if (event.defaultPrevented) return;
        event.preventDefault();

        const submitter = event.submitter;
        if (submitter) submitter.disabled = true;
        form.setAttribute('aria-busy', 'true');

        try {
            const response = await fetch(form.action, {
                method: 'POST',
                body: new FormData(form),
                headers: {
                    'Accept': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });
            const result = await response.json();
            if (!response.ok || result.success !== true) {
                throw new Error(result.message || `Unable to save (${response.status}).`);
            }

            if (result.message) sessionStorage.setItem(OPERATION_MESSAGE_KEY, result.message);
            if (result.redirectUrl) window.location.assign(result.redirectUrl);
        } catch (error) {
            const message = error instanceof Error ? error.message : 'Unable to save these changes.';
            window.alert(message);
            if (submitter) submitter.disabled = false;
            form.removeAttribute('aria-busy');
        }
    });
});

// ── Auto-dismiss toast ──────────────────────────────────────────────────────
setTimeout(() => {
    document.querySelectorAll('.toast').forEach(t => {
        t.style.transition = 'opacity .4s';
        t.style.opacity = '0';
        setTimeout(() => t.remove(), 400);
    });
}, 4000);

// ── Image preview on file input ────────────────────────────────────────────
document.querySelectorAll('input[type="file"][data-preview]').forEach(input => {
    input.addEventListener('change', () => {
        const target = document.querySelector(input.dataset.preview);
        if (!target || !input.files?.[0]) return;
        const reader = new FileReader();
        reader.onload = e => { target.src = e.target.result; };
        reader.readAsDataURL(input.files[0]);
    });
});

// ── Personal Vault: template item helper ───────────────────────────────────
const useTemplateToggle = document.getElementById('useTemplateToggle');
const templateFields = document.getElementById('templateFields');
const templateTypeFilter = document.getElementById('templateTypeFilter');
const templateItemSelect = document.getElementById('templateItemSelect');

useTemplateToggle?.addEventListener('change', () => {
    templateFields?.classList.toggle('is-hidden', !useTemplateToggle.checked);
});

templateTypeFilter?.addEventListener('change', () => {
    const typeId = templateTypeFilter.value;
    [...templateItemSelect.options].forEach(option => {
        if (!option.value) return;
        option.hidden = !!typeId && option.dataset.type !== typeId;
    });
    templateItemSelect.value = '';
});

templateItemSelect?.addEventListener('change', () => {
    const option = templateItemSelect.selectedOptions[0];
    if (!option || !option.value) return;

    const setValue = (name, value) => {
        if (value === undefined || value === null || value === '') return;
        const el = document.querySelector(`[name="${name}"]`);
        if (el) el.value = value;
    };

    setValue('Description', option.dataset.description);
    setValue('Brand', option.dataset.brand);
    setValue('Model', option.dataset.model);
    setValue('Manufacturer', option.dataset.manufacturer);
    setValue('CaseType', option.dataset.casetype);
    setValue('MediaFormat', option.dataset.mediaformat);
    setValue('Instruction', option.dataset.instruction);
    setValue('Memory', option.dataset.memory);
    setValue('Owner', option.dataset.owner);
    setValue('ReleaseYear', option.dataset.releaseyear);
    setValue('Tested', option.dataset.tested);
    setValue('Boxed', option.dataset.boxed?.toLowerCase());
    setValue('Sell', option.dataset.sell?.toLowerCase());
    setValue('PurchasedFrom', option.dataset.purchasefrom);
    setValue('WarrantyInfo', option.dataset.warrantyinfo);
    setValue('ManualUrl', option.dataset.manualurl);
    setValue('CustomStatus', option.dataset.customstatus);
    setValue('CategoryId', option.dataset.category);
    setValue('ItemTypeId', option.dataset.itemtype);
    setValue('PlatformId', option.dataset.platform);
    setValue('LocationId', option.dataset.location);
    setValue('Condition', option.dataset.condition);

    const status = option.dataset.status;
    if (status) {
        const statusRadio = document.querySelector(`input[name="Status"][value="${status}"]`);
        if (statusRadio) statusRadio.checked = true;
    }
});

// ── Preserve page position after edits/saves ───────────────────────────────
// Applies across Finance Manager so users return to the same context after a POST/redirect.
document.querySelectorAll('form[method="post"], form[method="POST"]').forEach(form => {
    form.addEventListener('submit', () => {
        try {
            sessionStorage.setItem(`scroll:${window.location.pathname}`, String(window.scrollY));
        } catch { /* storage may be unavailable */ }
    });
});

window.addEventListener('load', () => {
    if (window.location.hash) return; // explicit anchors take priority
    try {
        const key = `scroll:${window.location.pathname}`;
        const value = sessionStorage.getItem(key);
        if (value !== null) {
            sessionStorage.removeItem(key);
            window.scrollTo({ top: Number(value) || 0, behavior: 'instant' });
        }
    } catch { /* storage may be unavailable */ }
});

// Phase 8.1: Finance Manager application shell
(() => {
    const shell = document.getElementById('fmAppShell');
    const sidebar = document.getElementById('fmSidebar');
    const backdrop = document.getElementById('fmSidebarBackdrop');
    const collapseButton = document.getElementById('fmSidebarCollapse');
    const mobileButton = document.getElementById('fmMobileMenu');
    const storageKey = 'fm-sidebar-collapsed';

    const updateSidebarControl = () => {
        if (!shell || !collapseButton) return;
        const isCollapsed = shell.classList.contains('sidebar-collapsed');
        collapseButton.setAttribute('aria-expanded', String(!isCollapsed));
        collapseButton.setAttribute('aria-label', isCollapsed ? 'Expand sidebar' : 'Collapse sidebar');
        collapseButton.setAttribute('title', isCollapsed ? 'Expand sidebar' : 'Collapse sidebar');

        const icon = collapseButton.querySelector('[data-sidebar-icon]');
        if (icon) {
            icon.setAttribute('data-lucide', isCollapsed ? 'chevron-right' : 'chevron-left');
            window.lucide?.createIcons();
        }
    };

    if (shell && localStorage.getItem(storageKey) === 'true') {
        shell.classList.add('sidebar-collapsed');
    }
    updateSidebarControl();

    collapseButton?.addEventListener('click', () => {
        shell?.classList.toggle('sidebar-collapsed');
        localStorage.setItem(storageKey, String(shell?.classList.contains('sidebar-collapsed')));
        updateSidebarControl();
    });

    const closeMobileNavigation = () => {
        sidebar?.classList.remove('open');
        backdrop?.classList.remove('show');
    };

    mobileButton?.addEventListener('click', () => {
        sidebar?.classList.toggle('open');
        backdrop?.classList.toggle('show');
    });
    backdrop?.addEventListener('click', closeMobileNavigation);
    sidebar?.querySelectorAll('a').forEach(link => link.addEventListener('click', closeMobileNavigation));
})();
