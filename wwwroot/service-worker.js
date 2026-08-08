const CACHE_NAME = 'pocketflow-static-v3';
const OFFLINE_URL = '/offline.html';

const ASSETS_TO_CACHE = [
    OFFLINE_URL,
    '/css/app.css',
    '/css/tokens.css',
    '/css/site.css',
    '/js/theme.js',
    '/js/pwa.js',
    '/manifest.webmanifest',
    '/icons/icon-192.png',
    '/icons/icon-512.png',
    '/icons/icon-maskable-192.png',
    '/icons/icon-maskable-512.png',
    '/icons/apple-touch-icon.png',
    '/lib/jquery/dist/jquery.min.js',
    '/lib/jquery-validation/dist/jquery.validate.min.js',
    '/lib/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js',
    '/lib/gsap/gsap.min.js',
    'https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css',
    'https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/js/bootstrap.bundle.min.js',
    'https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css'
];

self.addEventListener('install', event => {
    self.skipWaiting();
    event.waitUntil(
        caches.open(CACHE_NAME).then(async cache => {
            for (const asset of ASSETS_TO_CACHE) {
                try {
                    await cache.add(new Request(asset, { cache: 'reload' }));
                } catch (err) {
                    console.error("Failed to cache asset:", asset, err);
                }
            }
        })
    );
});

self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        return caches.delete(cacheName);
                    }
                })
            );
        }).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', event => {
    // Exclude OAuth routes
    const url = new URL(event.request.url);
    if (url.pathname.startsWith('/Account/Google') || url.pathname.startsWith('/auth/v1')) {
        return; // Fallback to default browser fetch (NETWORK ONLY)
    }

    // Only intercept navigation requests (HTML pages) and some static assets.
    // Do not cache API responses (JSON) or POST requests.
    if (event.request.method !== 'GET') return;

    event.respondWith(
        caches.match(event.request).then(cachedResponse => {
            if (cachedResponse) {
                return cachedResponse;
            }

            return fetch(event.request).catch(() => {
                if (event.request.mode === 'navigate' || 
                    (event.request.headers.get('accept') && event.request.headers.get('accept').includes('text/html'))) {
                    return caches.match(OFFLINE_URL);
                }
                return Response.error();
            });
        })
    );
});

// WEB PUSH EVENTS
self.addEventListener('push', event => {
    console.log("[PocketFlow Push] Event received");
    let payload = {};

    try {
        payload = event.data ? event.data.json() : {};
        console.log("[PocketFlow Push] Payload parsed JSON");
    } catch (error) {
        payload = {
            body: event.data ? event.data.text() : ""
        };
        console.log("[PocketFlow Push] Payload parsed TEXT");
    }

    const notification = payload.notification ?? payload;

    const title =
        notification.title ??
        payload.title ??
        "PocketFlow";

    const options = {
        body:
            notification.body ??
            payload.body ??
            "Tienes una nueva notificación.",
        icon:
            notification.icon ??
            payload.icon ??
            "/icons/icon-192.png",
        badge: '/icons/icon-192.png',
        tag:
            notification.tag ??
            payload.tag ??
            "pocketflow",
        data: {
            url:
                notification.url ??
                payload.url ??
                "/Dashboard",
            type:
                notification.type ??
                payload.type ??
                "generic"
        },
        vibrate: [200, 100, 200]
    };

    console.log("[PocketFlow Push] Calling showNotification");
    event.waitUntil(
        self.registration.showNotification(title, options)
    );
});

self.addEventListener('notificationclick', event => {
    event.notification.close();
    
    const targetUrl = event.notification.data?.url || '/Dashboard';

    // Prevent external URLs from being opened by notification payload
    if (targetUrl.startsWith('http://') || targetUrl.startsWith('https://')) {
        const urlObj = new URL(targetUrl, self.location.origin);
        if (urlObj.origin !== self.location.origin) {
            console.warn('Bloqueada URL externa desde notificación push:', targetUrl);
            return;
        }
    }

    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then(windowClients => {
            for (let i = 0; i < windowClients.length; i++) {
                const client = windowClients[i];
                if (client.url.includes(targetUrl) && 'focus' in client) {
                    return client.focus();
                }
            }
            if (clients.openWindow) {
                return clients.openWindow(targetUrl);
            }
        })
    );
});
