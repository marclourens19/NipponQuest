const CACHE_NAME = 'nipponquest-cache-v2'; // Version bumped to clear old cache
const urlsToCache = [
    '/',
    '/css/site.css',
    '/js/site.js',
    '/lib/bootstrap/dist/css/bootstrap.min.css'
];

self.addEventListener('install', event => {
    self.skipWaiting(); // Forces the new service worker to activate immediately
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                return cache.addAll(urlsToCache);
            })
    );
});

self.addEventListener('activate', event => {
    // Delete any old caches from v1
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});

self.addEventListener('fetch', event => {
    if (event.request.mode === 'navigate') {
        // NETWORK-FIRST STRATEGY for HTML Pages (Ensures Auth State is accurate)
        event.respondWith(
            fetch(event.request).catch(() => {
                // If they are entirely offline, show the cached version
                return caches.match('/');
            })
        );
    } else {
        // CACHE-FIRST STRATEGY for CSS, JS, and Images (Makes app load lightning fast)
        event.respondWith(
            caches.match(event.request)
                .then(response => {
                    return response || fetch(event.request);
                })
        );
    }
});