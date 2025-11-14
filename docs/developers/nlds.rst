=======================
NLDS - NL Design System
=======================

De interface van de applicatie is opgebouwd met componenten uit het **NL Design System (NLDS)**. Dit is een verzameling ontwerp- en ontwikkelrichtlijnen voor digitale overheidsdiensten in Nederland. Door gebruik te maken van NLDS-componenten blijft de gebruikerservaring consistent en toegankelijk, in lijn met de standaarden van de overheid.

🔗 `Introductie NLDS <https://nldesignsystem.nl/handboek/introductie/>`_
🔗 `NLDS voor developers <https://nldesignsystem.nl/handboek/developer/overzicht/>`_


Aanpasbaarheid voor gemeentes
=============================

Dankzij NLDS kunnen verschillende installaties van de applicatie eenvoudig worden aangepast aan de huisstijl van diverse gemeentes. Dit wordt mogelijk gemaakt door het gebruik van design tokens, die de stijlkenmerken zoals kleuren, typografie en componenten bepalen.


Implementatie op basis van Utrecht Design System
------------------------------------------------

Op dit moment is de implementatie gebaseerd op alleen het Utrecht Design System, een specifieke variant van NLDS. Voor een correcte weergave en de beste resultaten moeten ten minste de Brand en Common tokens correct ingevuld zijn.


Gebruikte CSS-componenten
-------------------------

=============================================  ================================================================================================================================
**Component**                                  **Storybook**
---------------------------------------------  --------------------------------------------------------------------------------------------------------------------------------
**Document** (`utrecht-document`)              `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-document--design-tokens>`_
**Surface** (`utrecht-surface`)                `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-surface--design-tokens>`_
**Page** (`utrecht-page`)                      `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-page--design-tokens>`_
**Page header** (`utrecht-page-header`)        `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-page-header--design-tokens>`_
**Page content** (`utrecht-page-content`)      `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-page-content--design-tokens>`_
**Page footer** (`utrecht-page-footer`)        `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-page-footer--design-tokens>`_
**Navigation bar** (`utrecht-nav-bar`)         `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-nav-bar--design-tokens>`_
**Link** (`utrecht-link`)                      `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-link--design-tokens>`_
**Skip link** (`utrecht-skip-link`)            `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-skip-link--design-tokens>`_
**Article** (`utrecht-article`)                `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-article--design-tokens>`_
**Heading** (`utrecht-heading`)                `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-heading-1--design-tokens>`_
**Paragraph** (`utrecht-paragraph`)            `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-paragraph--design-tokens>`_
**Unordered list** (`utrecht-unordered-list`)  `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-unordered-list--design-tokens>`_
**Button** (`utrecht-button`)                  `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-button--design-tokens>`_
**Form field** (`utrecht-form-field`)          `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-form-field--design-tokens>`_
**Form label** (`utrecht-form-label`)          `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-form-label--design-tokens>`_
**Textbox** (`utrecht-textbox`)                `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-textbox--design-tokens>`_
**Table** (`utrecht-table`)                    `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-table--design-tokens>`_
**Logo** (`utrecht-logo`)                      `🔗 Design Tokens <https://nl-design-system.github.io/utrecht/storybook/?path=/story/css_css-logo--design-tokens>`_
=============================================  ================================================================================================================================


ITA Theme
---------

Naast de bovenstaande componenten bestaat de interface van de applicatie ook uit verschillende custom componenten en elementen. Deze (ita) componenten en elementen kunnen via een aantal voorgedefinieerde css-variabelen worden aangepast voor een uniforme uitstraling binnen de huisstijl van de gemeente.

Ter referentie `ita-theme <https://github.com/Interne-Taak-Afhandeling/ITA/blob/main/InterneTaakAfhandeling.Web.Client/src/assets/_mixin-theme.scss>`_.


Test Theme
----------

In de `public` folder van de ITA Web.Client applicatie staan `test-theme.css` en `test-logo.svg`. Om de installatie van de applicatie te testen kunnen verwijzingen naar deze bestanden worden gebruikt bij de configuratie van de omgevingsvariabelen. **Let op:** deze verwijzingen moeten absolute URL's zijn, zie :ref:`config_omgevingsvariabelen`.

.. code-block:: none

    "THEME_NAAM": "test-theme",
    "LOGO_URL": "https://<host>/test-logo.svg",
    "DESIGN_TOKENS_URL": "https://<host>/test-theme.css"


De test-theme bestanden kunnen daarnaast gebruikt worden om in een ontwikkelomgeving wat custom waardes te testen.

.. code-block:: none

    .test-theme {
        // utrecht
        --utrecht-link-color: #333; // custom waarde

        // ita
        --ita-dashboard-tables-column-gap: 2rem; // custom waarde
    }
