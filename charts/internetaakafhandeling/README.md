# internetaakafhandeling

![Version: 0.0.0](https://img.shields.io/badge/Version-0.0.0-informational?style=flat-square) ![Type: application](https://img.shields.io/badge/Type-application-informational?style=flat-square) ![AppVersion: 0.0.0](https://img.shields.io/badge/AppVersion-0.0.0-informational?style=flat-square)

Helm chart for InterneTaakAfhandeling including Web API and Poller

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| https://charts.bitnami.com/bitnami | postgresql | 12.5.6 |
> [!NOTE]
> For production environments, it is recommended to use [CloudNativePG](https://github.com/cloudnative-pg/cloudnative-pg) for PostgreSQL in stead of the dependent Helm charts included here. The bundled charts are primarily intended for testing and development purposes. Also, be aware of the upcoming changes to the bitnami catalog described in this [issue](https://github.com/bitnami/containers/issues/83267).

## Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| afdeling.type | string | `""` | De url van het afdeling objecttype in de objecttypen api. zie de objecttypen pagina in de documentatie for meer informatie |
| afdeling.typeVersion | int | `1` | De versie van het afdeling objecttype dat gebruikt wordt (hoogstwaarschijnlijk 1) |
| apiConnections.object.apiKey | string | `""` |  |
| apiConnections.object.baseUrl | string | `""` |  |
| apiConnections.openKlant.apiKey | string | `""` |  |
| apiConnections.openKlant.baseUrl | string | `""` |  |
| apiConnections.zaakSysteem.baseUrl | string | `""` |  |
| apiConnections.zaakSysteem.clientId | string | `""` |  |
| apiConnections.zaakSysteem.key | string | `""` |  |
| database.host | string | `""` |  |
| database.name | string | `"interneTaakAfhandeling"` |  |
| database.password | string | `""` |  |
| database.port | string | `"5432"` |  |
| database.username | string | `""` |  |
| fullnameOverride | string | `""` |  |
| groep.type | string | `""` | De url van het groep objecttype in de objecttypen api. zie de objecttypen pagina in de documentatie for meer informatie |
| groep.typeVersion | int | `1` | De versie van het groep objecttype dat gebruikt wordt (hoogstwaarschijnlijk 1) |
| ingress.annotations | object | `{}` |  |
| ingress.className | string | `""` |  |
| ingress.enabled | bool | `false` |  |
| ingress.hosts | list | `[]` | ingress hosts |
| ingress.tls | list | `[]` |  |
| ita.baseUrl | string | `""` |  |
| logboek.type | string | `""` | De url van het logboek objecttype in de objecttypen api. zie de objecttypen pagina in de documentatie for meer informatie |
| logboek.typeVersion | int | `1` | De versie van het logboek objecttype dat gebruikt wordt (hoogstwaarschijnlijk 1) |
| nameOverride | string | `""` |  |
| poller.image.pullPolicy | string | `"Always"` |  |
| poller.image.repository | string | `"ghcr.io/interne-taak-afhandeling/internetaakafhandeling.poller"` |  |
| poller.image.tag | string | `"latest"` |  |
| poller.notification.hourThreshold | string | `"-24"` |  |
| poller.notification.pollerMessage | string | `"Poller uitgevoerd om:"` |  |
| poller.schedule | string | `"*/15 * * * *"` |  |
| postgresql.auth.database | string | `"interneTaakAfhandeling"` |  |
| postgresql.auth.password | string | `""` |  |
| postgresql.auth.postgresPassword | string | `""` |  |
| postgresql.auth.username | string | `""` |  |
| postgresql.enabled | bool | `true` |  |
| postgresql.image.repository | string | `"bitnamilegacy/postgresql"` |  |
| postgresql.metrics.enabled | bool | `false` |  |
| postgresql.metrics.image.repository | string | `"bitnamilegacy/postgres-exporter"` |  |
| postgresql.primary.persistence.enabled | bool | `true` |  |
| postgresql.primary.persistence.size | string | `"8Gi"` |  |
| postgresql.volumePermissions.image.repository | string | `"bitnamilegacy/os-shell"` |  |
| smtp.enableSsl | string | `"true"` |  |
| smtp.fromEmail | string | `""` |  |
| smtp.host | string | `""` |  |
| smtp.password | string | `""` |  |
| smtp.port | string | `"25"` |  |
| smtp.username | string | `""` |  |
| web.appsettings.setting1 | string | `""` |  |
| web.appsettings.setting2 | string | `""` |  |
| web.image.pullPolicy | string | `"Always"` |  |
| web.image.repository | string | `"ghcr.io/interne-taak-afhandeling/internetaakafhandeling.web"` |  |
| web.image.tag | string | `"latest"` |  |
| web.oidc.authority | string | `""` |  |
| web.oidc.clientId | string | `""` |  |
| web.oidc.clientSecret | string | `""` |  |
| web.oidc.emailClaimType | string | `""` |  |
| web.oidc.functioneelBeheerderRole | string | `""` |  |
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
