window.eMarketing = {
  focus: (selector) => {
    const element = document.querySelector(selector);
    if (element) {
      element.focus();
    }
  },
  scrollTop: () => window.scrollTo({ top: 0, behavior: "smooth" })
};
