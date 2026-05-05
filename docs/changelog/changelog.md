# Changelog

## v3.0.1

- vulnerability patches
  
## v3.0.0

### Helm chart breaking changes
- `web.resources` in de helm chart values is hernoemd naar `web.styling` voor styling config (logoUrl, faviconUrl etc)
- `web.resources` is nu beschikbaar voor kubernetes pod resource limits/requests, consistent met andere podiumd subcharts
- `poller.resources` toegevoegd voor kubernetes pod resource limits/requests

**Let op:** geen functionele code wijzigingen. alleen helm chart values structuur is aangepast. als je `web.resources.logoUrl` gebruikt in je values, wijzig dit naar `web.styling.logoUrl`.

## v2.1.1

- fix vulnerable dependencies

## v2.1.0

- [ITA communicatie met e-Suite mogelijk maken (Oost Gelre)](https://dimpact.atlassian.net/browse/ITA-429)
- [Support: poller lijkt het niet te doen op Oost Gelre Acc](https://dimpact.atlassian.net/browse/ITA-430)

## previous versions

See release notes https://github.com/Interne-Taak-Afhandeling/ITA/releases
