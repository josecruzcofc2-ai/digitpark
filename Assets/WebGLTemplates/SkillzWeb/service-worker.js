// self.addEventListener("install", function (event) {
//   console.log("Service worker installed");
//   event.waitUntil(
//     (async function () {
//       const cache = await caches.open("assets-cache");
//       console.log("[Service Worker] caching assets");
//     })()
//   );
// });

self.addEventListener('fetch', function (e) {
  console.log('Service worker fetching', e.request.url);
  if (e.request.url.includes('Build') && e.request.url.endsWith('.gz')) {
    e.respondWith(
      caches.open('assets-cache').then(function (cache) {
        return cache.match(e.request).then(function (response) {
          if (response) {
            console.log('Service worker intercepting', e.request.url);
            return response;
          }
          return fetch(e.request).then(function (networkResponse) {
            cache.put(e.request, networkResponse.clone());
            return networkResponse;
          });
        });
      })
    );
  } else {
    console.log('Service worker not intercepting', e.request.url);
    return fetch(e.request);
  }
});
