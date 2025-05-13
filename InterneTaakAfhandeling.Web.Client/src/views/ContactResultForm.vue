<template>
    <div class="contact-result-form">
      <utrecht-heading :level="2">Resultaat klantcontact registreren</utrecht-heading>
      
      <utrecht-form-fieldset>
        <utrecht-fieldset-legend>Registreer het resultaat van het klantcontact</utrecht-fieldset-legend>
        
        <!-- Informatie over het klantcontact -->
        <utrecht-form-field>
          <utrecht-form-label for="contact-info">
            Informatie over het klantcontact
          </utrecht-form-label>
          <utrecht-textbox
            id="contact-info"
            v-model="contactResultData.inhoud"
            multiline
            :rows="4"
            placeholder="Beschrijf het resultaat van het contact met de klant..."
            required
          />
        </utrecht-form-field>
        
        <!-- Kanaal -->
        <utrecht-form-field>
          <utrecht-form-label for="contact-channel">
            Kanaal
          </utrecht-form-label>
          <utrecht-select
            id="contact-channel"
            v-model="contactResultData.kanaal"
            required
          >
            <option value="" disabled selected>Selecteer een kanaal</option>
            <option value="telefoon">Telefoon</option>
            <option value="email">E-mail</option>
            <option value="website">Website</option>
            <option value="whatsapp">WhatsApp</option>
            <option value="brief">Brief</option>
          </utrecht-select>
        </utrecht-form-field>
        
        <!-- Contact gelukt ja/nee -->
        <utrecht-form-field>
          <utrecht-form-label>
            Contact gelukt?
          </utrecht-form-label>
          <div class="radio-group">
            <div class="radio-option">
              <input
                type="radio"
                id="contact-success-yes"
                v-model="contactResultData.indicatieContactGelukt"
                :value="true"
                name="contact-success"
              />
              <label for="contact-success-yes">Ja</label>
            </div>
            <div class="radio-option">
              <input
                type="radio"
                id="contact-success-no"
                v-model="contactResultData.indicatieContactGelukt"
                :value="false"
                name="contact-success"
              />
              <label for="contact-success-no">Nee</label>
            </div>
          </div>
        </utrecht-form-field>
        
        <!-- Automatisch ingevulde velden (toon alleen ter informatie) -->
        <div class="auto-filled-fields">
          <p><strong>Taal:</strong> {{ contactResultData.taal }}</p>
          <p><strong>Vertrouwelijkheid:</strong> {{ contactResultData.vertrouwelijkheid ? 'Ja' : 'Nee' }}</p>
          <p><strong>Datum:</strong> {{ formatDate(contactResultData.datumTijd) }}</p>
        </div>
        
        <!-- Actieknoppen -->
        <div class="action-buttons">
          <utrecht-button 
            type="button" 
            class="utrecht-button--secondary" 
            @click="cancel"
          >
            Annuleren
          </utrecht-button>
          
          <utrecht-button 
            type="button" 
            class="utrecht-button--primary" 
            @click="saveAndContinue"
            :disabled="!isFormValid || isSaving"
          >
            {{ isSaving ? 'Bezig met opslaan...' : 'Opslaan en open laten' }}
          </utrecht-button>
          
          <utrecht-button 
            type="button" 
            class="utrecht-button--primary-action" 
            @click="saveAndClose"
            :disabled="!isFormValid || isSaving"
          >
            {{ isSaving ? 'Bezig met opslaan...' : 'Opslaan en afronden' }}
          </utrecht-button>
        </div>
      </utrecht-form-fieldset>
      
      <utrecht-alert v-if="errorMessage" type="error">
        {{ errorMessage }}
      </utrecht-alert>
    </div>
  </template>
  
  <script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import UtrechtAlert from '@/components/UtrechtAlert.vue';
  import { post } from '@/utils/fetchWrapper';
  
  interface ContactResultData {
    inhoud: string;
    kanaal: string;
    indicatieContactGelukt: boolean | null;
    taal: string;
    vertrouwelijkheid: boolean;
    datumTijd: Date;
    contactMomentId: string;
  }
  
  const route = useRoute();
  const router = useRouter();
  const contactMomentId = route.params.id as string;
  const isSaving = ref(false);
  const errorMessage = ref('');
  
  // Formulierdata initialiseren
  const contactResultData = ref<ContactResultData>({
    inhoud: '',
    kanaal: '',
    indicatieContactGelukt: null,
    taal: 'Nederlands', // Default waarde
    vertrouwelijkheid: false, // Default waarde
    datumTijd: new Date(),
    contactMomentId: contactMomentId
  });
  
  // Validatie check
  const isFormValid = computed(() => {
    return (
      contactResultData.value.inhoud.trim() !== '' &&
      contactResultData.value.kanaal !== '' &&
      contactResultData.value.indicatieContactGelukt !== null
    );
  });
  
  // Datum formatteren voor weergave
  function formatDate(date: Date): string {
    return date.toLocaleString('nl-NL', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
  
  // Contact resultaat opslaan en contactverzoek open laten
  async function saveAndContinue() {
    await saveContactResult(false);
  }
  
  // Contact resultaat opslaan en contactverzoek afronden
  async function saveAndClose() {
    await saveContactResult(true);
  }
  
  // Contact resultaat opslaan
  async function saveContactResult(afhandelen: boolean) {
    isSaving.value = true;
    errorMessage.value = '';
    
    try {
      // API call om contact resultaat op te slaan
      await post(`/api/contactmomenten/${contactMomentId}/resultaat`, {
        ...contactResultData.value,
        afhandelen
      });
      
      // Terug naar detailpagina navigeren
      router.push({
        name: 'contactmoment-detail',
        params: { id: contactMomentId }
      });
    } catch (error: any) {
      errorMessage.value = error.message || 'Er is een fout opgetreden bij het opslaan van het contactresultaat.';
      console.error('Error saving contact result:', error);
    } finally {
      isSaving.value = false;
    }
  }
  
  // Annuleren en terug naar detail pagina
  function cancel() {
    router.push({
      name: 'contactmoment-detail',
      params: { id: contactMomentId }
    });
  }
  
  // Bij het laden van de component, laad de gegevens van het oorspronkelijke contactmoment
  onMounted(async () => {
    try {
      // Optioneel: Je kunt hier een API-aanroep doen om informatie op te halen van het oorspronkelijke contactmoment
      // Dit kan handig zijn om contextuele informatie te tonen aan de gebruiker
    } catch (error) {
      console.error('Error loading contact moment details:', error);
    }
  });
  </script>
  
  <style scoped>
  .contact-result-form {
    max-width: 800px;
    margin: 0 auto;
    padding: 1rem;
  }
  
  .radio-group {
    display: flex;
    gap: 1.5rem;
    margin-top: 0.5rem;
  }
  
  .radio-option {
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
  
  .auto-filled-fields {
    margin: 1.5rem 0;
    padding: 1rem;
    background-color: #f5f5f5;
    border-radius: 4px;
  }
  
  .action-buttons {
    display: flex;
    gap: 1rem;
    margin-top: 2rem;
    justify-content: flex-end;
  }
  </style>