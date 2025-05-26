<template>
  <div class="forbidden-container">
    <div class="forbidden-content">
      <h1 class="utrecht-heading-1">Toegang geweigerd</h1>
      <div class="forbidden-icon">
        <i class="fas fa-lock"></i>
      </div>
      <p class="utrecht-paragraph">
        U heeft geen toegang tot deze pagina. Alleen ITA gebruikers hebben toegang tot deze
        functionaliteit.
      </p>
      <p class="utrecht-paragraph">
        Neem contact op met de systeembeheerder als u denkt dat dit een fout is.
      </p>
    </div>
  </div>
</template>

<style scoped>
.forbidden-container {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 70vh;
  padding: 1rem;
}

.forbidden-content {
  width: 100%;
  max-width: 600px;
  text-align: center;
  background-color: var(--utrecht-surface-background-color);
  border-radius: var(--utrecht-border-radius);
  padding: 2rem;
  box-shadow: var(--utrecht-shadow-medium);
  border-left: 4px solid var(--utrecht-color-green-30);
}

.forbidden-icon {
  font-size: 4rem;
  color: var(--utrecht-color-red-50);
  margin: 1.5rem 0;
}
</style>
