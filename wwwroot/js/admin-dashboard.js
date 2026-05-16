(() => {
  const chartElement = document.getElementById("submissionsChart");
  const dataElement = document.getElementById("dashboardChartData");
  if (!chartElement || !dataElement || typeof Chart === "undefined") {
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

  const context = chartElement.getContext("2d");
  if (!context) {
    return;
  }

  const gradient = context.createLinearGradient(0, 0, 0, 300);
  gradient.addColorStop(0, "rgba(37,99,235,0.24)");
  gradient.addColorStop(1, "rgba(20,184,166,0.04)");

  new Chart(chartElement, {
    type: "line",
    data: {
      labels,
      datasets: [
        {
          label: "Bài nộp",
          data: values,
          borderColor: "#2563eb",
          pointBackgroundColor: "#f4b740",
          pointBorderColor: "#172033",
          pointRadius: 4,
          backgroundColor: gradient,
          fill: true,
          tension: 0.35,
          borderWidth: 3
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: true } },
      scales: {
        y: { beginAtZero: true, grid: { color: "#e7eef9" } },
        x: { grid: { display: false } }
      }
    }
  });
})();
