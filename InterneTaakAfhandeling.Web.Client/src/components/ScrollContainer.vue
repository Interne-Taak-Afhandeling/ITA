<template>
  <div><slot /></div>
</template>

<style lang="scss" scoped>
div {
  max-inline-size: 100%;
  overflow-x: auto;

  // indicate that the container can be scrolled.
  // this especially useful on mobile where there are usually no scroll bars
  animation: scroll-shadow-inset linear;
  animation-timeline: scroll(self inline);
}

@keyframes scroll-shadow-inset {
  /* start with inset shadow on right */
  from {
    box-shadow: inset -10px -10px 15px 0px rgb(0 0 0 / 0.2);
  }

  /* end with inset shadow on left */
  to {
    box-shadow: inset 10px -10px 15px 0px rgb(0 0 0 / 0.2);
  }
}
</style>
