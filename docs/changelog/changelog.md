# Changelog

## v3.2.0

[Feature: Knop 'Toewijzen aan jezelf' verbergen bij bestaande toewijzing](https://github.com/Interne-Taak-Afhandeling/ITA/issues/347)
[Feature: Afgehandeld contactverzoek alleen leesbaar](https://github.com/Interne-Taak-Afhandeling/ITA/issues/344)
[Feature: Gebruik het nummer van het "contactmoment" in plaats van het nummer van de "interne taak"](https://github.com/Interne-Taak-Afhandeling/ITA/issues/299)
[Feature: Doorsturen via medewerker- of afdelingsselectie in plaats van vrij e-mailadres](https://github.com/Interne-Taak-Afhandeling/ITA/issues/349)
[Feature: Digitale adressen betrokkene via partij bij afwezigheid eigen adressen](https://github.com/Interne-Taak-Afhandeling/ITA/issues/367)
[Feature: Beheerder kan gesloten contactverzoek heropenen](https://github.com/Interne-Taak-Afhandeling/ITA/issues/391)
[Feature: Heldere notificatie-e-mail bij toewijzing contactverzoek](https://github.com/Interne-Taak-Afhandeling/ITA/issues/336)



## v3.1.1

### Helm chart additions
- `nodeSelector`, `tolerations` en `affinity` toegevoegd op chart root (geldt voor zowel `poller-cronjob` als `web-deployment`). Backwards compatible: defaults zijn leeg, bestaand gedrag op single-nodepool clusters ongewijzigd. Voor multi-nodepool clusters (bv AKS systempool/userpool) kan een operator nu workloads pinnen op specifieke nodes, bv:
  ```yaml
  ita:
    nodeSelector:
      agentpool: userpool
  ```

- [beheerder kan exclusief alle contactverzoeken raadplegen] (https://github.com/Interne-Taak-Afhandeling/ITA/issues/359)

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
