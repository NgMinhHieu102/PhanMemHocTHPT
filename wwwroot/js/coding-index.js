(() => {
  const tabs = document.querySelectorAll("[data-coding-grade],[data-coding-all]");
  const rows = document.querySelectorAll("[data-coding-row-grade]");
  if (tabs.length && rows.length) {
    const apply = (isAll, grade) => {
      rows.forEach((row) => {
        const rowGrade = row.getAttribute("data-coding-row-grade");
        row.style.display = isAll || rowGrade === grade ? "" : "none";
      });
      tabs.forEach((tab) => {
        const isAllTab = tab.hasAttribute("data-coding-all");
        const tabGrade = tab.getAttribute("data-coding-grade");
        tab.classList.toggle("active", (isAll && isAllTab) || (!isAll && tabGrade === grade));
      });
    };

    tabs.forEach((tab) => {
      tab.addEventListener("click", () => {
        apply(tab.hasAttribute("data-coding-all"), tab.getAttribute("data-coding-grade") || "");
      });
    });
  }

  document.querySelectorAll("[data-problem-url]").forEach((row) => {
    row.addEventListener("click", () => {
      const url = row.getAttribute("data-problem-url");
      if (url) {
        window.location.href = url;
      }
    });
  });
})();
