// Custom Toast Notification System for PocketFlow

class ToastSystem {
    constructor() {
        let existingContainer = document.getElementById('pf-toast-container');
        if (!existingContainer) {
            existingContainer = document.createElement('div');
            existingContainer.id = 'pf-toast-container';
            document.body.appendChild(existingContainer);
        }
        this.container = existingContainer;
    }

    show(message, type = 'info', duration = 4000) {
        const toast = document.createElement('div');
        toast.className = `pf-toast pf-toast-${type}`;
        toast.setAttribute('role', 'alert');
        toast.setAttribute('aria-live', 'assertive');
        toast.setAttribute('aria-atomic', 'true');

        const icons = {
            success: 'bi-check-circle-fill',
            error: 'bi-exclamation-triangle-fill',
            warning: 'bi-exclamation-circle-fill',
            info: 'bi-info-circle-fill'
        };
        
        const icon = icons[type] || icons['info'];

        toast.innerHTML = `
            <div class="pf-toast-icon"><i class="bi ${icon}"></i></div>
            <div class="pf-toast-content">${message}</div>
            <button class="pf-toast-close" aria-label="Cerrar"><i class="bi bi-x"></i></button>
        `;

        const closeBtn = toast.querySelector('.pf-toast-close');
        closeBtn.addEventListener('click', () => this.dismiss(toast));

        this.container.appendChild(toast);

        // Animate in
        requestAnimationFrame(() => {
            toast.classList.add('show');
        });

        // Auto dismiss
        if (duration > 0) {
            setTimeout(() => {
                this.dismiss(toast);
            }, duration);
        }
    }

    dismiss(toast) {
        if (!toast || !toast.parentNode) return;
        toast.classList.remove('show');
        toast.classList.add('hide');
        toast.addEventListener('transitionend', () => {
            if (toast.parentNode) {
                toast.parentNode.removeChild(toast);
            }
        });
    }

    success(message, duration = 4000) { this.show(message, 'success', duration); }
    error(message, duration = 5000) { this.show(message, 'error', duration); }
    warning(message, duration = 4000) { this.show(message, 'warning', duration); }
    info(message, duration = 4000) { this.show(message, 'info', duration); }
}

// Global instance (singleton)
if (!window.Toasts) {
    window.Toasts = new ToastSystem();
}
