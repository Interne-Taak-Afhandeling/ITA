<template>

<!-- 
  todo: this is an initial setup. implement in pc-1245 
  https://denhaagdesignsystem.azurewebsites.net/components/detail/card.html is probably the best component for this. 
  there is no vue version. duplicate the html and import the denhaag css
 -->
  <utrecht-heading :level="2">
    Contactmomenten</utrecht-heading>
<utrecht-data-list v-for="contactmoment in  contactmomenten" :key="contactmoment.tekst" >
    <utrecht-data-list-item>
    <utrecht-data-list-key>Contact gelukt</utrecht-data-list-key>
    <utrecht-data-list-value :value="contactmoment.contactGelukt+''">{{ contactmoment.contactGelukt ?  "Contact gelukt" : "Contact niet gelukt"
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

<script setup lang="ts">
import { computed, ref, watchEffect } from 'vue';
import { overviewKlantcontactService, type Contactmoment } from "@/services/overviewKlantcontactService";
import { useUserStore } from '@/stores/user';
import { storeToRefs } from 'pinia';
import type { Internetaken } from '@/types/internetaken';
 
const props = defineProps<{ contactmomentNummer : string | undefined }>();
const isLoading = ref(true);
const error = ref("");
const contactmomenten = ref<Contactmoment[]>([]);
const userStore = useUserStore();
const { assignedInternetaken } = storeToRefs(userStore);
 
const taak = computed(() => {
  if(!props.contactmomentNummer) {
    return null;
  }
  
  return assignedInternetaken.value.find(
    (x: Internetaken) => x.aanleidinggevendKlantcontact?.nummer == props.contactmomentNummer
  ) || null;
});

watchEffect(async () => {  
  isLoading.value = true;
  error.value = "";
  
  if(taak.value?.aanleidinggevendKlantcontact?.uuid) {
    try {
      const response = await overviewKlantcontactService.getContactKeten(
        taak.value.aanleidinggevendKlantcontact.uuid
      );
      
      // Haal de contactmomenten uit de response
      contactmomenten.value = response.contactmomenten;
    } catch (err: unknown) {
      error.value = err instanceof Error && err.message 
        ? err.message 
        : 'Er is een fout opgetreden bij het ophalen van de contactmomenten bij dit contactverzoek';
    } finally {
      isLoading.value = false;
    }  
  }
});
</script>
