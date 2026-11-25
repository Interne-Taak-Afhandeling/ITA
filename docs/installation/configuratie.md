(config_omgevingsvariabelen)=

# Configuratie

De configuratie van ITA is beschreven in de {{ helm_readme_link }}. Die README is de enige plek waar alle instellingen staan, met per instelling:

- de helm value (bijvoorbeeld `web.oidc.authority`);
- de bijbehorende **environment variabele** (bijvoorbeeld `OIDC_AUTHORITY`), voor wie ITA zonder de helm chart installeert of lokaal draait;
- uitleg, standaardwaarden en voorbeelden.

De link hierboven verwijst naar de README van dezelfde versie als deze documentatie ({{ helm_readme_version }}).

**Andere versie van ITA?** Vervang dan `<versie>` in deze URL door je ITA-versie (bijvoorbeeld `v3.4.0`):

```text
https://github.com/Interne-Taak-Afhandeling/ITA/blob/<versie>/charts/internetaakafhandeling/README.md
```

## Objecttypen

ITA haalt Medewerkers, Afdelingen en Groepen (met hun e-mailadressen) op uit de Objecten API (Overige Objecten), en schrijft het logboek van een Contactverzoek daarin weg. Welke objecttypen daarvoor nodig zijn en aan welke schema's ze moeten voldoen, staat op de pagina [Objecttypen](objecttypen.rst).
