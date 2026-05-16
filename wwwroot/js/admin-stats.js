(() => {
  if (typeof Chart === "undefined") {
    return;
  }

  const buildBarChart = (canvasId, dataId, label, color) => {
    const canvas = document.getElementById(canvasId);
    const dataElement = document.getElementById(dataId);
    if (!canvas || !dataElement) {
      return;
    }

    let labels = [];
    let values = [];
    try {
      labels = JSON.parse(dataElement.dataset.labels ?? "[]");
      values = JSON.parse(dataElement.dataset.values ?? "[]");
    } catch {
      return;
    }

    new Chart(canvas, {
      type: "bar",
      data: {
        labels,
        datasets: [
          {
            label,
            data: values,
            backgroundColor: color
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
          y: { beginAtZero: true, grid: { color: "#f1f5f9" } },
          x: { grid: { display: false } }
        }
      }
    });
  };

  buildBarChart("studentChart", "studentStatsData", "Điểm TB", "#2563eb");
  buildBarChart("exerciseChart", "exerciseStatsData", "Số lượt làm", "#10b981");
})();
