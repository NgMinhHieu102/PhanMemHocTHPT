(() => {
  const typeSelect = document.getElementById("Type");
  const mcqBlock = document.getElementById("multipleChoiceAnswers");
  const tfBlock = document.getElementById("multipleTrueAnswers");
  if (!typeSelect || !mcqBlock || !tfBlock) {
    return;
  }

  const setEnabled = (block, enabled) => {
    block.querySelectorAll("input, textarea, select").forEach((el) => {
      el.disabled = !enabled;
    });
  };

  const toggleAnswerUi = () => {
    const type = typeSelect.value;
    const isMcq = type === "MultipleChoice";
    const isTf = type === "MultipleTrue";
    const isEssay = type === "Essay";
    mcqBlock.classList.toggle("d-none", !isMcq);
    tfBlock.classList.toggle("d-none", !isTf);
    setEnabled(mcqBlock, isMcq);
    setEnabled(tfBlock, isTf);
  };

  typeSelect.addEventListener("change", toggleAnswerUi);
  toggleAnswerUi();
})();
