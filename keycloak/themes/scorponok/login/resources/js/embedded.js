// Dentro do modal da loja (iframe): marca a página para o CSS compacto e avisa a
// loja da altura do conteúdo, para o iframe crescer sem barra de rolagem.
// Só a altura vai na mensagem, então não importa quem está do outro lado.
(function () {
  if (window.parent === window) return;
  document.documentElement.classList.add("kc-embedded");

  function postHeight() {
    window.parent.postMessage(
      { type: "scorponok:kc-height", height: document.documentElement.scrollHeight },
      "*"
    );
  }

  document.addEventListener("DOMContentLoaded", function () {
    postHeight();
    new ResizeObserver(postHeight).observe(document.body);
  });
})();
