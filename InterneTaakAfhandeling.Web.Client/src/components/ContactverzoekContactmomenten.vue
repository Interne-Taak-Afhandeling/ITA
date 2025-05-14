<template>


  <utrecht-heading :level="2">
    Contactmomenten</utrecht-heading>
<utrecht-data-list v-for="contactmoment in  contactmomenten" :key="contactmoment.tekst" >
    <utrecht-data-list-item>
    <utrecht-data-list-key>Contact gelukt</utrecht-data-list-key>
    <utrecht-data-list-value :value="contactmoment.contactGelukt">{{ contactmoment.contactGelukt ?  "Contact gelukt" : "Contact niet gelukt"
    }}</utrecht-data-list-value>
    </utrecht-data-list-item>
    <utrecht-data-list-item>
      <utrecht-data-list-key>Informatie voor burger/bedrijf</utrecht-data-list-key>
      <utrecht-data-list-value :value="contactmoment.tekst">{{ contactmoment.tekst }}</utrecht-data-list-value>
    </utrecht-data-list-item>
    <utrecht-data-list-item>
      <utrecht-data-list-key>Datum</utrecht-data-list-key>
      <utrecht-data-list-value :value="contactmoment.datum">{{ contactmoment.datum  }}</utrecht-data-list-value>
    </utrecht-data-list-item>
    <utrecht-data-list-item>
      <utrecht-data-list-key>Medewerker</utrecht-data-list-key>
      <utrecht-data-list-value :value="contactmoment.medewerker">{{ contactmoment.medewerker }}</utrecht-data-list-value>
    </utrecht-data-list-item>
    <utrecht-data-list-item>
      <utrecht-data-list-key>Kanaal</utrecht-data-list-key>
      <utrecht-data-list-value :value="contactmoment.kanaal">{{ contactmoment.kanaal }}</utrecht-data-list-value>
    </utrecht-data-list-item>
    </utrecht-data-list>

</template>

<script setup lang="ts" >
import { computed, ref, watchEffect } from 'vue';
import { klantcontactService, type Contactmoment  } from "@/services/createKlantcontactService";
import { useUserStore } from '@/stores/user';
import { storeToRefs } from 'pinia';
import type { Internetaken } from '@/types/internetaken';
 
const props = defineProps<{ nummer : string }>();
const isLoading = ref(true);
const error = ref("");
const contactmomenten = ref<Contactmoment[]>([]);
const userStore = useUserStore();
const { assignedInternetaken } = storeToRefs(userStore);
 
  const taak = computed(() => {
    return assignedInternetaken.value.find(
      (x: Internetaken) => x.aanleidinggevendKlantcontact?.nummer == props.nummer
    ) || null;
  });


  watchEffect(async () => {  
    isLoading.value = true;
    error.value = "";
    if(taak.value?.aanleidinggevendKlantcontact?.uuid) {
      try {
        contactmomenten.value = await klantcontactService.getInterneTaakContactmomenten(taak.value.aanleidinggevendKlantcontact?.uuid);

          // contactmomenten.value = [ 
          //     { contactGelukt: true, tekst: "sdfsfdsdf", datum: "15-04-2025", medewerker : "Piet van Gelre", kanaal : "Telefoon" } , 
          //     { contactGelukt: true, tekst: "rtfdgdtyerert", datum: "12-04-2025", medewerker : "Piet van Gelre", kanaal : "Telefoon" }
          // ];  
      } catch (err: unknown) {
        error.value = err.message || 'Er is een fout opgetreden bij het ophalen van de contactmomenten bij dit contactverzoek';
      } finally {
        isLoading.value = false;
      }  
  }
});


</script>
