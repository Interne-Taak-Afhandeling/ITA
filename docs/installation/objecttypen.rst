===========
Objecttypen
===========

ITA maakt gebruik van verschillende objecttypen in de Objecten API. Bij de installatie van ITA moet men ervoor zorgen dat deze objecttypen aanwezig zijn in een Objecttypen API en dat deze correct geregistreerd zijn in de Objecten API. Zie de documentatie (https://objects-and-objecttypes-api.readthedocs.io/en/latest/) voor instructies.

Logboek (Activiteitenlog)
-------------------------

ITA slaat de history rondom de afhandeling van een Contactverzoek op in de Objecten API. Dit gebeurt in een logboek-objecttype genaamd Activiteitenlog.

- Het schema van het objecttype staat in de repository van ITA: `https://github.com/Interne-Taak-Afhandeling/ITA/blob/main/docs/schema/Logboek-schema.json <https://github.com/Interne-Taak-Afhandeling/ITA/blob/main/docs/schema/Logboek-schema.json>`_

Medewerker
----------

ITA gebruikt het Medewerker-objecttype om medewerkers te koppelen aan contactverzoeken. Het objecttype moet voldoen aan het community-concept schema:

- `https://github.com/open-objecten/objecttypes/blob/main/community-concepts/Medewerker/medewerker-schema.json <https://github.com/open-objecten/objecttypes/blob/main/community-concepts/Medewerker/medewerker-schema.json>`_

Configureer de volgende helm values:

- ``medewerker.type``: de URL van het medewerker objecttype in de Objecttypen API (gemeentespecifiek).
- ``medewerker.typeVersion``: de versie van het objecttype (standaard: 1).

Groep
-----

ITA gebruikt het Groep-objecttype om groepen te koppelen aan contactverzoeken. Het objecttype moet voldoen aan het community-concept schema:

- `https://github.com/open-objecten/objecttypes/blob/main/community-concepts/Afdeling%20en%20Groep/groep-schema.json <https://github.com/open-objecten/objecttypes/blob/main/community-concepts/Afdeling%20en%20Groep/groep-schema.json>`_

Configureer de volgende helm values:

- ``groep.type``: de URL van het groep objecttype in de Objecttypen API (gemeentespecifiek).
- ``groep.typeVersion``: de versie van het objecttype (standaard: 1).

Afdeling
--------

ITA gebruikt het Afdeling-objecttype om afdelingen te koppelen aan contactverzoeken. Het objecttype moet voldoen aan het community-concept schema:

- `https://github.com/open-objecten/objecttypes/blob/main/community-concepts/Afdeling%20en%20Groep/afdeling-schema.json <https://github.com/open-objecten/objecttypes/blob/main/community-concepts/Afdeling%20en%20Groep/afdeling-schema.json>`_

Configureer de volgende helm values:

- ``afdeling.type``: de URL van het afdeling objecttype in de Objecttypen API (gemeentespecifiek).
- ``afdeling.typeVersion``: de versie van het objecttype (standaard: 1).

                    
