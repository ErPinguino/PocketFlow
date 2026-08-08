let deferredPrompt;

// Escuchar el evento de instalación en Android/Desktop
window.addEventListener('beforeinstallprompt', (e) => {
    // Prevenir que el navegador muestre el prompt nativo directamente
    e.preventDefault();
    // Guardar el evento para dispararlo luego
    deferredPrompt = e;
    
    // Mostrar nuestro botón o banner personalizado "Instalar PocketFlow"
    showInstallPromotion();
});

function showInstallPromotion() {
    const installBtn = document.getElementById('installPwaBtn');
    if (installBtn) {
        installBtn.style.display = 'block';
        installBtn.addEventListener('click', async () => {
            // Ocultar nuestro botón
            installBtn.style.display = 'none';
            // Mostrar el prompt nativo
            deferredPrompt.prompt();
            // Esperar a que el usuario responda
            const { outcome } = await deferredPrompt.userChoice;
            console.log(`User response to the install prompt: ${outcome}`);
            // No podemos volver a usar este prompt
            deferredPrompt = null;
        });
    }
}

// Escuchar evento cuando la PWA se instala con éxito
window.addEventListener('appinstalled', () => {
    // Ocultar banner y limpiar
    deferredPrompt = null;
    const installBtn = document.getElementById('installPwaBtn');
    if (installBtn) installBtn.style.display = 'none';
    console.log('PWA was installed');
});

// Utilidad para codificar VAPID public key (Uint8Array)
function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - base64String.length % 4) % 4);
    const base64 = (base64String + padding)
        .replace(/-/g, '+')
        .replace(/_/g, '/');

    const rawData = window.atob(base64);
    const outputArray = new Uint8Array(rawData.length);

    for (let i = 0; i < rawData.length; ++i) {
        outputArray[i] = rawData.charCodeAt(i);
    }
    return outputArray;
}

// Suscribirse a Web Push
async function subscribeUserToPush() {
    try {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            console.warn('Push messaging is not supported');
            return null;
        }

        const registration = await navigator.serviceWorker.ready;

        // Obtener la clave pública desde el backend
        const response = await fetch('/Notifications/PublicKey');
        const data = await response.json();
        const vapidPublicKey = data.publicKey;

        const convertedVapidKey = urlBase64ToUint8Array(vapidPublicKey);

        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: convertedVapidKey
        });

        // Enviar la suscripción al servidor
        await fetch('/Notifications/Subscribe', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                endpoint: subscription.endpoint,
                p256dh: arrayBufferToBase64(subscription.getKey('p256dh')),
                auth: arrayBufferToBase64(subscription.getKey('auth'))
            })
        });

        return subscription;
    } catch (error) {
        console.error('Failed to subscribe the user: ', error);
        throw error;
    }
}

function arrayBufferToBase64(buffer) {
    let binary = '';
    const bytes = new Uint8Array(buffer);
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

// Lógica para Ajustes de Notificaciones
document.addEventListener('DOMContentLoaded', () => {
    const pushStatusText = document.getElementById('pushStatusText');
    const btnEnablePush = document.getElementById('btnEnablePush');
    
    if (pushStatusText && btnEnablePush) {
        if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
            pushStatusText.textContent = "Las notificaciones Push no están soportadas en este navegador.";
            return;
        }

        if (Notification.permission === 'granted') {
            pushStatusText.textContent = "Notificaciones activadas y configuradas.";
            pushStatusText.className = "text-success small mb-0 fw-bold";
            checkAndShowTestButton();
        } else if (Notification.permission === 'denied') {
            pushStatusText.textContent = "Notificaciones bloqueadas por el navegador.";
            pushStatusText.className = "text-danger small mb-0 fw-bold";
        } else {
            pushStatusText.textContent = "Puedes recibir alertas en tu dispositivo.";
            btnEnablePush.classList.remove('d-none');
            
            btnEnablePush.addEventListener('click', async () => {
                const permission = await Notification.requestPermission();
                if (permission === 'granted') {
                    btnEnablePush.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> Activando...';
                    btnEnablePush.disabled = true;
                    try {
                        await subscribeUserToPush();
                        pushStatusText.textContent = "Notificaciones activadas y configuradas.";
                        pushStatusText.className = "text-success small mb-0 fw-bold";
                        btnEnablePush.classList.add('d-none');
                        if (typeof Toasts !== 'undefined') Toasts.success("Notificaciones activadas correctamente.");
                        checkAndShowTestButton();
                    } catch (e) {
                        btnEnablePush.innerHTML = 'Reintentar';
                        btnEnablePush.disabled = false;
                        if (typeof Toasts !== 'undefined') Toasts.error("No se pudo registrar la suscripción.");
                    }
                } else {
                    pushStatusText.textContent = "Notificaciones bloqueadas por el usuario.";
                    pushStatusText.className = "text-danger small mb-0 fw-bold";
                    btnEnablePush.classList.add('d-none');
                }
            });
        }
    }

    async function checkAndShowTestButton() {
        const testPushSection = document.getElementById('testPushSection');
        const btnTestPush = document.getElementById('btnTestPush');
        if (!testPushSection || !btnTestPush) return;

        const registration = await navigator.serviceWorker.ready;
        const subscription = await registration.pushManager.getSubscription();

        if (subscription) {
            testPushSection.classList.remove('d-none');
            
            btnTestPush.addEventListener('click', async () => {
                const originalHtml = btnTestPush.innerHTML;
                btnTestPush.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span> Enviando...';
                btnTestPush.disabled = true;

                const controller = new AbortController();
                const timeoutId = setTimeout(() => controller.abort(), 10000);

                try {
                    const antiForgeryToken = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
                    
                    const response = await fetch('/Notifications/SendTestCurrentDevice', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json',
                            'RequestVerificationToken': antiForgeryToken
                        },
                        body: JSON.stringify({ endpoint: subscription.endpoint }),
                        signal: controller.signal
                    });
                    
                    const data = await response.json();
                    if (data.result === 'Success') {
                        if (typeof Toasts !== 'undefined') Toasts.success("Notificación de prueba enviada.");
                    } else if (data.result === 'NoSubscriptions') {
                        if (typeof Toasts !== 'undefined') Toasts.warning("El dispositivo no está registrado.");
                    } else {
                        if (typeof Toasts !== 'undefined') Toasts.error("Error al enviar la notificación.");
                    }
                } catch (e) {
                    if (e.name === 'AbortError') {
                        if (typeof Toasts !== 'undefined') Toasts.error("Tiempo de espera agotado al enviar prueba.");
                    } else {
                        if (typeof Toasts !== 'undefined') Toasts.error("Fallo de red al intentar enviar prueba.");
                    }
                } finally {
                    clearTimeout(timeoutId);
                    btnTestPush.innerHTML = originalHtml;
                    btnTestPush.disabled = false;
                }
            });
        } else {
            const pushStatusText = document.getElementById('pushStatusText');
            if (pushStatusText) {
                pushStatusText.textContent = "Este dispositivo todavía no está suscrito a notificaciones.";
                pushStatusText.className = "text-warning small mb-0 fw-bold";
                const btnEnablePush = document.getElementById('btnEnablePush');
                if (btnEnablePush) {
                    btnEnablePush.classList.remove('d-none');
                }
            }
        }
    }

    // Toggles de preferencias
    const prefForm = document.getElementById('notificationPrefsForm');
    if (prefForm) {
        const toggles = prefForm.querySelectorAll('.pref-toggle');
        toggles.forEach(toggle => {
            toggle.addEventListener('change', async () => {
                const payload = {
                    notifyPayday: document.getElementById('notifyPayday').checked,
                    notifyWeeklyBudget: document.getElementById('notifyWeeklyBudget').checked,
                    notifyPiggyBanks: document.getElementById('notifyPiggyBanks').checked,
                    notifyExpenseReminders: document.getElementById('notifyExpenseReminders').checked
                };

                try {
                    await fetch('/Settings/UpdateNotificationPreferences', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify(payload)
                    });
                    if (typeof Toasts !== 'undefined') Toasts.success("Preferencias guardadas.");
                } catch (e) {
                    if (typeof Toasts !== 'undefined') Toasts.error("Error al guardar preferencias.");
                }
            });
        });
    }
});
