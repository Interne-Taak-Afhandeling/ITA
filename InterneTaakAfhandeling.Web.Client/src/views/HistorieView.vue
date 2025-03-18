<template>
  <utrecht-heading :level="1">Historie</utrecht-heading>

  <section>
    <utrecht-heading :level="2" id="h2-a">Mijn afgeronde contacten</utrecht-heading>

    <utrecht-table aria-labelledby="h2-a">
      <utrecht-table-header>
        <utrecht-table-row>
          <utrecht-table-header-cell scope="col">Datum</utrecht-table-header-cell>
          <utrecht-table-header-cell scope="col">Klantnaam</utrecht-table-header-cell>
          <utrecht-table-header-cell scope="col">Onderwerp</utrecht-table-header-cell>
          <utrecht-table-header-cell scope="col">Afgerond op</utrecht-table-header-cell>
        </utrecht-table-row>
      </utrecht-table-header>

      <utrecht-table-body>
        <utrecht-table-row>
          <utrecht-table-cell>01-01-2025</utrecht-table-cell>
          <utrecht-table-cell>Saskia Swart</utrecht-table-cell>
          <utrecht-table-cell>Lorem ipsum</utrecht-table-cell>
          <utrecht-table-cell>03-01-2025 12:34</utrecht-table-cell>
        </utrecht-table-row>

        <utrecht-table-row>
          <utrecht-table-cell>02-01-2025</utrecht-table-cell>
          <utrecht-table-cell>Martijn de Groot</utrecht-table-cell>
          <utrecht-table-cell>Dolor sit amet</utrecht-table-cell>
          <utrecht-table-cell>04-01-2025 13:44</utrecht-table-cell>
        </utrecht-table-row>
      </utrecht-table-body>
    </utrecht-table>
  </section>
</template>
