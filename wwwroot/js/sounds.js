/**
 * sounds.js
 * Handles subtle UI sounds for PocketFlow.
 */

class SoundManager {
    constructor() {
        this.enabled = localStorage.getItem('pocketflow-sounds-enabled') !== 'false';
        
        this.sounds = {
            success: new Audio('/sounds/success.wav'),
            error: new Audio('/sounds/error.wav'),
            delete: new Audio('/sounds/delete.wav')
        };
        
        // Preload sounds
        for (let key in this.sounds) {
            this.sounds[key].load();
        }

        // Check for pending sounds from a reload
        window.addEventListener('load', () => {
            if (sessionStorage.getItem('pf-sound-success')) {
                this.success();
                sessionStorage.removeItem('pf-sound-success');
            }
            if (sessionStorage.getItem('pf-sound-delete')) {
                this.delete();
                sessionStorage.removeItem('pf-sound-delete');
            }
        });
    }

    setEnabled(isEnabled) {
        this.enabled = isEnabled;
        localStorage.setItem('pocketflow-sounds-enabled', isEnabled);
    }

    play(type) {
        if (!this.enabled) return;
        
        const audio = this.sounds[type];
        if (audio) {
            // Clone the node to allow overlapping sounds of the same type
            const clone = audio.cloneNode();
            clone.volume = 0.3; // Keep it subtle
            clone.play().catch(e => {
                // Ignore autoplay restrictions or missing files silently
            });
        }
    }

    success() {
        this.play('success');
    }

    error() {
        this.play('error');
    }

    delete() {
        this.play('delete');
    }
}

window.PocketFlowSound = new SoundManager();
