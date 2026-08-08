// PocketFlow Animations using GSAP and CSS

document.addEventListener('DOMContentLoaded', () => {
    // 1. Dashboard Load Animation
    const dashboardCards = document.querySelectorAll('.gsap-card');
    if (dashboardCards.length > 0 && typeof gsap !== 'undefined') {
        // Reduced motion check
        const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
        
        if (!prefersReducedMotion) {
            // Initial state
            gsap.set(dashboardCards, { y: 20, opacity: 0 });
            
            // Staggered entry
            gsap.to(dashboardCards, {
                y: 0,
                opacity: 1,
                duration: 0.4,
                stagger: 0.1,
                ease: 'power2.out',
                clearProps: 'all' // Remove GSAP inline styles after animation so CSS hover effects work
            });
        }
    }

    // 2. Check for Celebration Flag
    if (sessionStorage.getItem('pf-celebrate-new-month') === 'true') {
        sessionStorage.removeItem('pf-celebrate-new-month');
        if (typeof triggerCelebration === 'function') {
            triggerCelebration();
        }
    }

    // 3. Check for pending toasts
    const pendingToast = sessionStorage.getItem('pf-toast-success');
    if (pendingToast) {
        sessionStorage.removeItem('pf-toast-success');
        if (window.Toasts) Toasts.success(pendingToast);
    }
});

// Custom CSS Celebration Animation
window.triggerCelebration = function() {
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion) return;

    const overlay = document.createElement('div');
    overlay.className = 'celebration-overlay';
    
    const content = document.createElement('div');
    content.className = 'celebration-content';
    content.innerHTML = `
        <div class="celebration-icon"><i class="bi bi-check-circle-fill text-success"></i></div>
        <h3 class="fw-bold mt-3 mb-1">¡Nuevo ciclo iniciado!</h3>
        <p class="text-muted">Todo listo para este mes</p>
    `;
    
    overlay.appendChild(content);
    document.body.appendChild(overlay);

    // CSS Animation classes
    requestAnimationFrame(() => {
        overlay.classList.add('show');
    });

    setTimeout(() => {
        overlay.classList.remove('show');
        overlay.classList.add('hide');
        setTimeout(() => {
            if (overlay.parentNode) overlay.parentNode.removeChild(overlay);
        }, 300);
    }, 2000);
};
