/**
 * theme.js
 * Handles the logic for the PocketFlow theme system.
 * NOTE: The initial anti-FOUC theme application is done via an inline script in the <head> of the layouts.
 */

document.addEventListener('DOMContentLoaded', () => {
    // 1. Initialize Settings UI if we are on the Settings page
    const themeCards = document.querySelectorAll('.theme-card');
    if (themeCards.length > 0) {
        initThemeSettingsUI(themeCards);
    }
    
    const reducedMotionCheck = document.getElementById('reducedMotionCheck');
    if (reducedMotionCheck) {
        initReducedMotionUI(reducedMotionCheck);
    }
    
    const soundsEnabledCheck = document.getElementById('soundsEnabledCheck');
    if (soundsEnabledCheck) {
        initSoundsEnabledUI(soundsEnabledCheck);
    }
});

function initThemeSettingsUI(themeCards) {
    const currentTheme = localStorage.getItem('pocketflow-theme') || 'light';
    
    // Set initial aria-pressed state
    themeCards.forEach(card => {
        const themeValue = card.getAttribute('data-theme-value');
        if (themeValue === currentTheme) {
            card.setAttribute('aria-pressed', 'true');
            card.classList.add('selected');
        } else {
            card.setAttribute('aria-pressed', 'false');
            card.classList.remove('selected');
        }

        // Add click listener
        card.addEventListener('click', () => {
            if (card.getAttribute('aria-pressed') === 'true') return; // Already selected
            
            // Update UI state
            themeCards.forEach(c => {
                c.setAttribute('aria-pressed', 'false');
                c.classList.remove('selected');
            });
            card.setAttribute('aria-pressed', 'true');
            card.classList.add('selected');
            
            // Apply theme
            applyTheme(themeValue);
        });
    });
}

function applyTheme(themeValue) {
    // Save preference
    localStorage.setItem('pocketflow-theme', themeValue);
    
    // Check if we should animate
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    const userReducedMotion = localStorage.getItem('pocketflow-reduced-motion') === 'true';
    
    if (!prefersReducedMotion && !userReducedMotion) {
        document.documentElement.classList.add('theme-transition');
    }
    
    // Apply theme
    document.documentElement.setAttribute('data-theme', themeValue);
    
    // Remove transition class after it finishes to prevent transition on layout resizes etc
    setTimeout(() => {
        document.documentElement.classList.remove('theme-transition');
    }, 250);
}

function initReducedMotionUI(checkbox) {
    const isReduced = localStorage.getItem('pocketflow-reduced-motion') === 'true';
    checkbox.checked = isReduced;
    
    checkbox.addEventListener('change', (e) => {
        localStorage.setItem('pocketflow-reduced-motion', e.target.checked);
        // Maybe trigger a global event or function for animations.js to re-read it
        if (typeof window.initAnimations === 'function' && !e.target.checked) {
            // Can't easily restart GSAP unless reload or re-init
        }
    });
}

function initSoundsEnabledUI(checkbox) {
    const isEnabled = localStorage.getItem('pocketflow-sounds-enabled') !== 'false';
    checkbox.checked = isEnabled;
    
    checkbox.addEventListener('change', (e) => {
        if (window.PocketFlowSound) {
            window.PocketFlowSound.setEnabled(e.target.checked);
        } else {
            localStorage.setItem('pocketflow-sounds-enabled', e.target.checked);
        }
    });
}
