# internetaakafhandeling

![Version: 0.0.0](https://img.shields.io/badge/Version-0.0.0-informational?style=flat-square) ![Type: application](https://img.shields.io/badge/Type-application-informational?style=flat-square) ![AppVersion: 0.0.0](https://img.shields.io/badge/AppVersion-0.0.0-informational?style=flat-square)

Helm chart for InterneTaakAfhandeling including Web API and Poller

This README is the single source of documentation for the configuration of ITA (Interne Taak Afhandeling). It is generated with [helm-docs](https://github.com/norwoodj/helm-docs) from the comments in [values.yaml](./values.yaml) and the template [README.md.gotmpl](./README.md.gotmpl). To change this documentation, edit those files, not this README.

- Every ITA setting lists the **environment variable** it is passed to ITA as (`Env: …`). Use these names if you install ITA without this helm chart, or run it locally.
- Functional background, such as the objecttypes ITA uses, is described in the [ITA documentation](https://ita.readthedocs.io/nl/stable/installation/index.html).

## Requirements

| Repository | Name | Version |
|------------|------|---------|
| https://charts.bitnami.com/bitnami | postgresql | 12.5.6 |
> [!NOTE]
> For production environments, it is recommended to use [CloudNativePG](https://github.com/cloudnative-pg/cloudnative-pg) for PostgreSQL in stead of the dependent Helm charts included here. The bundled charts are primarily intended for testing and development purposes. Also, be aware of the upcoming changes to the bitnami catalog described in this [issue](https://github.com/bitnami/containers/issues/83267).

## General rules

- Settings marked **Required** must be filled in. Without them, ITA does not start or does not work.
- The website and the poller CronJobs get the same environment variables, from one ConfigMap and one Secret. Settings marked `(secret)` are stored in the Secret.
- A double underscore (`__`) in an environment variable separates configuration sections, as usual in ASP.NET Core. For example, `OpenKlantApi__BaseUrl` is the same as `"OpenKlantApi": { "BaseUrl": "…" }` in an `appsettings.json`.

## Values

### Kubernetes

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| nameOverride | string | `""` | Changes the prefix of the names of the Kubernetes resources into `<release name>-<nameOverride>`. By default the prefix is the release name. Useful when ITA is used as a subchart. Don't combine with the bundled PostgreSQL, see [Built-in or external PostgreSQL](#built-in-or-external-postgresql). |
| fullnameOverride | string | `""` | Replaces the prefix of the names of the Kubernetes resources. By default the prefix is the release name. Don't combine with the bundled PostgreSQL, see [Built-in or external PostgreSQL](#built-in-or-external-postgresql). |
| nodeSelector | object | `{}` | Node selector for the website and the poller CronJobs. Useful in clusters with multiple node pools (e.g. AKS system/user pool separation), where workloads must run on specific nodes. |
| tolerations | list | `[]` | Tolerations for the website and the poller CronJobs. |
| affinity | object | `{}` | Affinity rules for the website and the poller CronJobs. |

### Database

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| postgresql.enabled | bool | `true` | Install a PostgreSQL database with the bundled bitnami subchart. Set to `false` to use an existing PostgreSQL server; then fill in `database.host`. See [Built-in or external PostgreSQL](#built-in-or-external-postgresql). |
| postgresql.auth.username | string | `""` | User that the bundled PostgreSQL creates. Must be the same as `database.username`. Only used when `postgresql.enabled` is `true`. |
| postgresql.auth.password | string | `""` | Password of the user that the bundled PostgreSQL creates. Must be the same as `database.password`. Only used when `postgresql.enabled` is `true`. |
| postgresql.auth.database | string | `"interneTaakAfhandeling"` | Database that the bundled PostgreSQL creates. Must be the same as `database.name`. Only used when `postgresql.enabled` is `true`. |
| postgresql.auth.postgresPassword | string | `""` | Password of the `postgres` admin user of the bundled PostgreSQL. Not used by ITA itself. |
| database.host | string | `""` | Host of the PostgreSQL server. Only used when `postgresql.enabled` is `false`. With the bundled PostgreSQL, the chart uses `<fullname>-postgresql` (by default `<release name>-postgresql`).<br/>Env: part of `ConnectionStrings__DefaultConnection` |
| database.port | string | `"5432"` | Port of the PostgreSQL server.<br/>Env: part of `ConnectionStrings__DefaultConnection` |
| database.name | string | `"interneTaakAfhandeling"` | **Required.** Name of the database of ITA.<br/>Env: part of `ConnectionStrings__DefaultConnection` |
| database.username | string | `""` | **Required.** Username for ITA to access the database.<br/>Env: part of `ConnectionStrings__DefaultConnection` |
| database.password | string | `""` | **Required.** Password for ITA to access the database.<br/>Env: part of `ConnectionStrings__DefaultConnection` (secret) |

### Ingress

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| ingress.enabled | bool | `false` | Create an Ingress for the ITA website. |
| ingress.annotations | object | `{}` | Annotations of the Ingress, for example for cert-manager. |
| ingress.className | string | `""` | IngressClass of the Ingress, for example `nginx`. |
| ingress.hosts | list | `[]` | Hosts and paths of the Ingress. See the example below this value in [values.yaml](./values.yaml). |
| ingress.tls | list | `[]` | TLS configuration of the Ingress. See the example below this value in [values.yaml](./values.yaml). |

### Registers

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| apiConnections.openKlant.baseUrl | string | `""` | **Required.** URL of the Klantinteracties API of Open Klant, including `/klantinteracties/api/v1/` and **ending with a slash**. See [Base URLs of the registers](#base-urls-of-the-registers).<br/>Example: `https://openklant.mijngemeente.nl/klantinteracties/api/v1/`<br/>Env: `OpenKlantApi__BaseUrl` |
| apiConnections.openKlant.apiKey | string | `""` | **Required.** Token for the Klantinteracties API of Open Klant.<br/>Env: `OpenKlantApi__ApiKey` (secret) |
| apiConnections.object.baseUrl | string | `""` | **Required.** URL of the Objects API (Overige Objecten, e.g. Open Objecten), including `/api/v2/` and **ending with a slash**. ITA reads Medewerkers, Afdelingen and Groepen from this API and writes the logboek to it.<br/>Example: `https://objecten.mijngemeente.nl/api/v2/`<br/>Env: `ObjectApi__BaseUrl` |
| apiConnections.object.apiKey | string | `""` | **Required.** Token for the Objects API.<br/>Env: `ObjectApi__ApiKey` (secret) |
| apiConnections.zaakSysteem.baseUrl | string | `""` | **Required.** Root URL of the zaaksysteem (ZGW APIs), **without** `/zaken/api/v1` (ITA adds that itself). When you use the e-Suite, this is the root URL of the podiumd-adapter. See [Base URLs of the registers](#base-urls-of-the-registers).<br/>Example: `https://openzaak.mijngemeente.nl/` or `https://podiumd-adapter.mijngemeente.nl/`<br/>Env: `ZaakSysteem__BaseUrl` |
| apiConnections.zaakSysteem.clientId | string | `""` | **Required.** Client ID for the ZGW APIs of the zaaksysteem. With the podiumd-adapter, this is the client ID as configured in the adapter.<br/>Env: `ZaakSysteem__ClientId` |
| apiConnections.zaakSysteem.key | string | `""` | **Required.** Secret for the ZGW APIs of the zaaksysteem, used to sign the JWT token. **At least 32 characters.** With the podiumd-adapter, this is the client secret as configured in the adapter.<br/>Env: `ZaakSysteem__Key` (secret) |

### Objecttypes

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| logboek.type | string | `""` | **Required.** URL of the Logboek (Activiteitenlog) objecttype in the Objecttypen API. ITA stores the history of the handling of a Contactverzoek with this objecttype. See [Objecttypes](#about-the-objecttypes).<br/>Example: `https://objecttypen.mijngemeente.nl/api/v2/objecttypes/<uuid>`<br/>Env: `LogBoekOptions__Type` |
| logboek.typeVersion | int | `1` | Version of the Logboek objecttype that is used (most likely `1`).<br/>Env: `LogBoekOptions__TypeVersion` |
| afdeling.type | string | `""` | **Required.** URL of the Afdeling objecttype in the Objecttypen API. See [Objecttypes](#about-the-objecttypes).<br/>Env: `AfdelingOptions__Type` |
| afdeling.typeVersion | int | `1` | Version of the Afdeling objecttype that is used (most likely `1`).<br/>Env: `AfdelingOptions__TypeVersion` |
| groep.type | string | `""` | **Required.** URL of the Groep objecttype in the Objecttypen API. See [Objecttypes](#about-the-objecttypes).<br/>Env: `GroepOptions__Type` |
| groep.typeVersion | int | `1` | Version of the Groep objecttype that is used (most likely `1`).<br/>Env: `GroepOptions__TypeVersion` |
| medewerker.type | string | `""` | **Required.** URL of the Medewerker objecttype in the Objecttypen API. See [Objecttypes](#about-the-objecttypes).<br/>Env: `MedewerkerOptions__Type` |
| medewerker.typeVersion | int | `1` | Version of the Medewerker objecttype that is used (most likely `1`).<br/>Env: `MedewerkerOptions__TypeVersion` |

### E-mail notifications

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| smtp.host | string | `""` | **Required.** Host name of the mail server (SMTP) that ITA uses to send e-mails.<br/>Env: `Email__SmtpSettings__Host` |
| smtp.port | string | `"25"` | Port of the mail server.<br/>Env: `Email__SmtpSettings__Port` |
| smtp.username | string | `""` | Username for the mail server.<br/>Env: `Email__SmtpSettings__Username` |
| smtp.password | string | `""` | Password for the mail server.<br/>Env: `Email__SmtpSettings__Password` (secret) |
| smtp.fromEmail | string | `""` | **Required.** Sender address of the e-mails that ITA sends.<br/>Env: `Email__SmtpSettings__FromEmail` |
| smtp.enableSsl | string | `"true"` | `true` or `false`: use SSL/TLS for the connection with the mail server.<br/>Env: `Email__SmtpSettings__EnableSsl` |
| ita.baseUrl | string | `""` | **Required.** Public URL of the ITA website. Used for the links to ITA in the e-mails that ITA sends.<br/>Example: `https://ita.mijngemeente.nl`<br/>Env: `Ita__BaseUrl` |
| nieuweInternetaakNotificatie.schedule | string | `"*/15 * * * *"` | Cron expression that controls how often the poller checks for new Contactverzoeken (Interne Taken) to send a notification e-mail for. Default: every 15 minutes. See [Poller jobs](#poller-jobs). |
| nieuweInternetaakNotificatie.notification.pollerMessage | string | `"Poller uitgevoerd om:"` | Message written to the logs when a poller run starts (both poller jobs).<br/>Env: `pollerMessage` |
| verlopenContactverzoekHerinneringNotificatie.enabled | bool | `true` | Set to `false` to disable the daily reminder e-mail for overdue Contactverzoeken (per Medewerker and per Afdeling/Groep). See [Poller jobs](#poller-jobs). |
| verlopenContactverzoekHerinneringNotificatie.schedule | string | `"0 7 * * 1-5"` | Cron expression that controls when the daily reminder e-mail is sent. Default: every working day at 07:00. |

### Contactverzoeken

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| urgentie.afhandeltermijnUren | int | `48` | Optional. Number of working hours within which a Contactverzoek should be handled. Weekends are not counted. Leave empty to use the default. See [Urgency validation at startup](#urgency-validation-at-startup).<br/>Env: `Urgentie__AfhandeltermijnUren` |
| urgentie.bijnaVerlopenDrempelUren | int | `6` | Optional. Number of remaining working hours at which the urgency label of a Contactverzoek turns orange. Leave empty to use the default. See [Urgency validation at startup](#urgency-validation-at-startup).<br/>Env: `Urgentie__BijnaVerlopenDrempelUren` |
| web.partijIdentificatorTonen | bool | `false` | Optional. Set to `true` to show the BSN (person) or the KVK-nummer and/or vestigingsnummer (company) of the partij linked to a Contactverzoek. If a Contactverzoek has multiple partijen, or a partij has multiple identificatoren of the same kind, all are shown. Nothing is shown when no partij is linked.<br/>Env: `PARTIJ_IDENTIFICATOR_TONEN` |

### Deployment

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| web.image.repository | string | `"ghcr.io/interne-taak-afhandeling/internetaakafhandeling.web"` | Image of the ITA website. |
| web.image.tag | string | `"latest"` | Tag of the website image. Use a fixed version in production, for example `3.4.0`. |
| web.image.pullPolicy | string | `"Always"` | Pull policy of the website image. |
| web.service.type | string | `"ClusterIP"` | Type of the Kubernetes Service of the website. |
| web.service.port | int | `80` | Port of the Kubernetes Service of the website. |
| web.resources | object | `{}` | Resource requests and limits of the website container. |
| web.appsettings.setting1 | string | `""` | Placeholder: written as `Setting1` to the `appsettings.json` that the chart mounts in the website container, but not used by ITA. |
| web.appsettings.setting2 | string | `""` | Placeholder: written as `Setting2` to the `appsettings.json` that the chart mounts in the website container, but not used by ITA. |
| poller.image.repository | string | `"ghcr.io/interne-taak-afhandeling/internetaakafhandeling.poller"` | Image of the poller, used by all poller CronJobs. See [Poller jobs](#poller-jobs). |
| poller.image.tag | string | `"latest"` | Tag of the poller image. Use the same version as the website image. |
| poller.image.pullPolicy | string | `"Always"` | Pull policy of the poller image. |
| poller.resources | object | `{}` | Resource requests and limits of the poller containers. |

### Authentication (OIDC)

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| web.oidc.authority | string | `""` | **Required.** URL of the OpenID Connect Identity Provider.<br/>Example: `https://login.microsoftonline.com/<tenant-id>/v2.0`<br/>Env: `OIDC_AUTHORITY` <details><summary>More information</summary>The chart only passes the `web.oidc` settings (except `clientSecret`) to ITA when this value is filled in. The website does not start without them.</details> |
| web.oidc.clientId | string | `""` | **Required.** Client ID of ITA at the OpenID Connect Identity Provider.<br/>Env: `OIDC_CLIENT_ID` |
| web.oidc.clientSecret | string | `""` | **Required.** Client secret of ITA at the OpenID Connect Identity Provider.<br/>Env: `OIDC_CLIENT_SECRET` (secret) |
| web.oidc.itaSystemAccessRole | string | `""` | **Required.** Value of the role claim that gives a user access to ITA.<br/>Example: `ITA-Gebruiker`<br/>Env: `OIDC_ITA_SYSTEM_ACCESS_ROLE` |
| web.oidc.functioneelBeheerderRole | string | `""` | **Required.** Value of the role claim for functioneel beheerders. A functioneel beheerder also needs the role of `itaSystemAccessRole`.<br/>Example: `ITA-Functioneel-Beheerder`<br/>Env: `OIDC_FUNCTIONEEL_BEHEERDER_ROLE` <details><summary>More information</summary>With this role, a user can also manage kanalen (menu option Beheer), see all Contactverzoeken, reopen a Contactverzoek, and open Contactverzoeken of every Afdeling and Groep, not only those of their own Afdelingen and Groepen.</details> |
| web.oidc.nameClaimType | string | `name` | Name of the claim in the token of the Identity Provider that contains the full name of the logged in user.<br/>Env: `OIDC_NAME_CLAIM_TYPE` |
| web.oidc.roleClaimType | string | `roles` | Name of the claim in the token of the Identity Provider that contains the roles of the logged in user.<br/>Env: `OIDC_ROLE_CLAIM_TYPE` |
| web.oidc.objectregisterMedewerkerIdClaimType | string | `""` | **Required in practice.** Name of the claim in the token of the Identity Provider that contains a unique identification of the logged in user. Its value must match the attribute `identificatie` of the Medewerker in the Objects API. In most cases this is `email`.<br/>Env: `OIDC_OBJECTREGISTER_MEDEWERKER_ID_CLAIM_TYPE` <details><summary>More information</summary>ITA uses this value to find the Contactverzoeken that are assigned to the logged in user, and to find the Afdelingen and Groepen of the user. Without it, a user without the functioneel beheerder role cannot open Contactverzoeken of their Afdelingen and Groepen. When this value is set and the token of a user does not contain the claim, the user gets an error.<br/><br/>**Note:** ITA also sends this value as the `user_id` claim in the JWT token to the zaaksysteem. When you use the e-Suite via the podiumd-adapter, the e-Suite uses this value: it must match the username of the medewerker in the e-Suite.</details> |
| web.oidc.emailClaimType | string | `email` | Name of the claim in the token of the Identity Provider that contains the e-mail address of the logged in user. ITA also uses it to find the Contactverzoeken that are assigned to the logged in user.<br/>Env: `OIDC_EMAIL_CLAIM_TYPE` |

### Styling

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| web.styling.logoUrl | string | `""` | Optional. Public, absolute URL of the logo of the municipality.<br/>Example: `https://www.mijngemeente.nl/logo.svg`<br/>Env: `RESOURCES__LOGO_URL` <details><summary>More information</summary>The chart only passes the `web.styling` settings to ITA when this value is filled in. ITA ignores URLs that are not absolute.</details> |
| web.styling.faviconUrl | string | `""` | Optional. Public, absolute URL of the favicon of the municipality. Only passed to ITA when `logoUrl` is set.<br/>Example: `https://www.mijngemeente.nl/favicon.ico`<br/>Env: `RESOURCES__FAVICON_URL` |
| web.styling.designTokensUrl | string | `""` | Optional. Public, absolute URL of a CSS file with [NL Design System](https://nldesignsystem.nl) design tokens, to style ITA in the house style of the municipality. Only passed to ITA when `logoUrl` is set.<br/>Example: `https://unpkg.com/@gemeente/design-tokens/dist/index.css`<br/>Env: `RESOURCES__DESIGN_TOKENS_URL` |
| web.styling.webFontSources | list | `[]` | Optional. One or more public, absolute URLs of web font files of the house style, or of the location where all font files are. Only passed to ITA when `logoUrl` is set.<br/>Example: `["https://fonts.mijngemeente.nl/custom-regular-font.woff2", "https://fonts.mijngemeente.nl/custom-bold-font.woff2"]` or `["https://fonts.mijngemeente.nl/custom-font/"]`<br/>Env: `RESOURCES__WEB_FONT_SOURCES` (separated by spaces) <details><summary>More information</summary>This setting only allows the browser to load the font files (it adds the URLs to the Content Security Policy). The fonts must also be declared with `@font-face` rules in the theme CSS.</details> |

### Other Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| postgresql.image.repository | string | `"bitnamilegacy/postgresql"` |  |
| postgresql.primary.persistence.enabled | bool | `true` |  |
| postgresql.primary.persistence.size | string | `"8Gi"` |  |
| postgresql.metrics.enabled | bool | `false` |  |
| postgresql.metrics.image.repository | string | `"bitnamilegacy/postgres-exporter"` |  |
| postgresql.volumePermissions.image.repository | string | `"bitnamilegacy/os-shell"` |  |

## Configuration details

### Base URLs of the registers

ITA appends relative paths to the base URLs of the registers. Therefore a base URL that contains a path must **end with a slash**: without it, the last part of the path is lost.

- **Open Klant:** `apiConnections.openKlant.baseUrl` includes `/klantinteracties/api/v1/`, for example `https://openklant.mijngemeente.nl/klantinteracties/api/v1/`. ITA expects the Klantinteracties API at exactly this path of the host: Open Klant can't run under an extra path prefix.
- **Objects API:** `apiConnections.object.baseUrl` includes `/api/v2/`, for example `https://objecten.mijngemeente.nl/api/v2/`.
- **Zaaksysteem:** `apiConnections.zaakSysteem.baseUrl` is the root URL of the zaaksysteem, for example `https://openzaak.mijngemeente.nl/`. ITA adds the path of the API itself (for example `zaken/api/v1/zaken`). If the zaaksysteem runs under a path prefix, end the URL with a slash, for example `https://mijngemeente.nl/openzaak/`.
  - When you use the e-Suite, ITA connects through the podiumd-adapter. Use the root URL of the adapter, for example `https://podiumd-adapter.mijngemeente.nl/`. The adapter routes based on the paths of the ZGW APIs. `clientId` and `key` are the client ID and client secret as configured in the adapter.

### Built-in or external PostgreSQL

ITA stores its data in a PostgreSQL database. The chart combines the `database.*` values into one connection string:

```text
ConnectionStrings__DefaultConnection=Host=<host>;Port=<port>;Database=<name>;Username=<username>;Password=<password>;
```

- With `postgresql.enabled: true` (default), the chart installs PostgreSQL with the bundled bitnami subchart. The host is set automatically to `<fullname>-postgresql`. `postgresql.auth.username`, `postgresql.auth.password` and `postgresql.auth.database` must be the same as `database.username`, `database.password` and `database.name`: the subchart creates that user and database, and ITA connects with the `database.*` values. See the note about CloudNativePG above.
  - Don't set `nameOverride` or `fullnameOverride` together with the bundled PostgreSQL. The subchart names its database service after the release name, while ITA connects to `<fullname>-postgresql`. With an override, these names no longer match and ITA can't reach the database.
- With `postgresql.enabled: false`, fill in `database.host` (and `database.port` if needed) of your own PostgreSQL server.

ITA creates and updates its tables itself, when the website or a poller job starts.

### About the objecttypes

ITA reads Medewerkers, Afdelingen and Groepen, including their e-mail addresses, from the Objects API (Overige Objecten), and writes the logboek of a Contactverzoek to it. The objecttypes must be present in an Objecttypen API. Medewerker, Afdeling and Groep must follow the community-concept schemas; the Logboek (Activiteitenlog) follows a schema that is part of ITA. The [Objecttypen page of the ITA documentation](https://ita.readthedocs.io/nl/stable/installation/objecttypen.html) describes each objecttype and links to its schema.

### Urgency validation at startup

The `urgentie.*` settings are checked when the website starts:

- A value of `0`, a negative number or a value that is not a whole number is rejected. The website does not start and logs an error that names the invalid setting. The poller job for the daily reminder (see [Poller jobs](#poller-jobs)) fails with the same error when it runs.
- A `bijnaVerlopenDrempelUren` that is greater than or equal to `afhandeltermijnUren` is valid but unusual: every new Contactverzoek is then marked as almost overdue right away. ITA starts and logs a warning.

### Poller jobs

Next to the website, the chart installs the poller as two Kubernetes CronJobs. Both use the poller image and the same configuration as the website. The chart sets the environment variable `POLLER_MODE` per CronJob, to select the job:

| CronJob | `POLLER_MODE` | What it does | Schedule |
|---------|---------------|--------------|----------|
| `<fullname>-nieuw-cv-notify` | `nieuwe-internetaak-notificatie` | Sends a notification e-mail about new Contactverzoeken (Interne Taken). | `nieuweInternetaakNotificatie.schedule` |
| `<fullname>-verlopen-cv-notify` | `verlopen-contactverzoek-herinnering-notificatie` | Sends a daily reminder e-mail about overdue Contactverzoeken, per Medewerker and per Afdeling/Groep. Only installed when `verlopenContactverzoekHerinneringNotificatie.enabled` is `true`. | `verlopenContactverzoekHerinneringNotificatie.schedule` |

Without helm, run the poller with one of these values in `POLLER_MODE`. When `POLLER_MODE` is not set, the poller runs `nieuwe-internetaak-notificatie`. With an unknown value, the poller logs an error and stops without running a job. It then still exits successfully (exit code 0), so Kubernetes does not report the job as failed: check the logs.

### Settings set by the chart

The chart always sets these environment variables. If you install ITA without this chart, set them yourself where needed.

| Env | Value | Description |
|-----|-------|-------------|
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` | Lets the website process the `X-Forwarded-For` and `X-Forwarded-Proto` headers of the ingress / reverse proxy. Keep this `true` when ITA runs behind an ingress. |
| `POLLER_MODE` | per CronJob | Selects the poller job. See [Poller jobs](#poller-jobs). |

### Settings without a chart value

ITA supports an environment variable that this chart doesn't offer as a value. The chart also has no option to add extra environment variables, so you can only use it when you install ITA without this chart.

| Env | Description |
|-----|-------------|
| `RESOURCES__THEME_NAAM` | Name of a CSS class that ITA adds to the `<body>` of the page. Use it when the design tokens of your house style (`web.styling.designTokensUrl`) only apply within a theme class, for example `mijngemeente-theme`. |

----------------------------------------------
Autogenerated from chart metadata using [helm-docs v1.14.2](https://github.com/norwoodj/helm-docs/releases/v1.14.2)
