# internetaakafhandeling

![Version: 0.1.0](https://img.shields.io/badge/Version-0.1.0-informational?style=flat-square) ![Type: application](https://img.shields.io/badge/Type-application-informational?style=flat-square) ![AppVersion: 0.1.0](https://img.shields.io/badge/AppVersion-0.1.0-informational?style=flat-square)

Helm chart for InterneTaakAfhandeling including Web API and Poller

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| https://charts.bitnami.com/bitnami | postgresql | 12.5.6 |

## Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| apiConnections.object.apiKey | string | `""` |  |
| apiConnections.object.baseUrl | string | `""` |  |
| apiConnections.openKlant.apiKey | string | `""` |  |
| apiConnections.openKlant.baseUrl | string | `""` |  |
| apiConnections.zaakSysteem.baseUrl | string | `""` |  |
| apiConnections.zaakSysteem.clientId | string | `""` |  |
| apiConnections.zaakSysteem.key | string | `""` |  |
| certificates.commonName | string | `""` |  |
| certificates.email | string | `""` |  |
| certificates.enabled | bool | `true` |  |
| certificates.installClusterIssuer | bool | `true` |  |
| certificates.issuerName | string | `"letsencrypt-prod"` |  |
| database.host | string | `""` |  |
| database.name | string | `"interneTaakAfhandeling"` |  |
| database.password | string | `""` |  |
| database.port | string | `"5432"` |  |
| database.username | string | `""` |  |
| ingress.enabled | bool | `true` |  |
| ingress.host | string | `""` |  |
| ingress.port | int | `80` |  |
| logboek.type | string | `""` |  |
| logboek.typeVersion | int | `1` |  |
| poller.image.pullPolicy | string | `"Always"` |  |
| poller.image.repository | string | `"ghcr.io/interne-taak-afhandeling/internetaakafhandeling.poller"` |  |
| poller.image.tag | string | `"latest"` |  |
| poller.notification.hourThreshold | string | `"-24"` |  |
| poller.notification.pollerMessage | string | `"Poller uitgevoerd om:"` |  |
| poller.schedule | string | `"*/15 * * * *"` |  |
| poller.smtp.enableSsl | string | `"true"` |  |
| poller.smtp.fromEmail | string | `""` |  |
| poller.smtp.host | string | `""` |  |
| poller.smtp.password | string | `""` |  |
| poller.smtp.port | string | `"25"` |  |
| poller.smtp.username | string | `""` |  |
| postgresql.auth.database | string | `"interneTaakAfhandeling"` |  |
| postgresql.auth.password | string | `""` |  |
| postgresql.auth.postgresPassword | string | `""` |  |
| postgresql.auth.username | string | `""` |  |
| postgresql.enabled | bool | `true` |  |
| postgresql.metrics.enabled | bool | `false` |  |
| postgresql.primary.persistence.enabled | bool | `true` |  |
| postgresql.primary.persistence.size | string | `"8Gi"` |  |
| web.appsettings.setting1 | string | `""` |  |
| web.appsettings.setting2 | string | `""` |  |
| web.image.pullPolicy | string | `"Always"` |  |
| web.image.repository | string | `"ghcr.io/interne-taak-afhandeling/internetaakafhandeling.web"` |  |
| web.image.tag | string | `"latest"` |  |
| web.oidc.authority | string | `""` |  |
| web.oidc.clientId | string | `""` |  |
| web.oidc.clientSecret | string | `""` |  |
| web.oidc.emailClaimType | string | `""` |  |
| web.oidc.itaSystemAccessRole | string | `""` |  |
| web.oidc.nameClaimType | string | `""` |  |
| web.oidc.objectregisterMedewerkerIdClaimType | string | `""` |  |
| web.oidc.roleClaimType | string | `""` |  |
| web.resources.designTokensUrl | string | `""` |  |
| web.resources.faviconUrl | string | `""` |  |
| web.resources.logoUrl | string | `""` |  |
| web.resources.webFontSources | list | `[]` |  |
| web.service.port | int | `80` |  |
| web.service.type | string | `"ClusterIP"` |  |

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)
