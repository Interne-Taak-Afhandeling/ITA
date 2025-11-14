Cross-Origin Security Policies
==============================

Deze applicatie maakt gebruik van Cross-Origin-Embedder-Policy (COEP: require-corp), maar de externe resources (afbeeldingen en stylesheets) worden geladen onder CORS (met cross-origin-attributen). Dat betekent dat die externe resources de juiste Access-Control-Allow-Origin-header moeten bevatten.

Alleen moet vanwege COEP voor het favicon ook de Cross-Origin-Resource-Policy-header gezet worden, omdat sommige browsers bij `<link rel=icon crossorigin>` het cross-origin-attribuut negeren en de resource geladen wordt in zogenaamde `no-cors` mode.


Headers
-------

Voor alle externe resources moet `Access-Control-Allow-Origin: *` of bijvoorbeeld `Access-Control-Allow-Origin: *.mijn-gemeente.nl` ingesteld worden.

En voor het favicon naast de CORS-header ook CORP-header `Cross-Origin-Resource-Policy: cross-origin` instellen.

Als een resource niet correct is geconfigureerd, zal deze niet geladen worden door de browser. **Let op:** met uitzondering van de geconfigureerde Web fonts kunnen geconfigureerde externe resources geen verwijzingen bevatten naar andere externe resources of data URIs.
