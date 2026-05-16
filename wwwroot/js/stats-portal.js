(() => {
  document.querySelectorAll("tbody td[data-ms]").forEach((el) => {
    const ms = Number(el.getAttribute("data-ms") || "0");
    if (!ms) {
      return;
    }

    const dt = new Date(ms);
    const now = new Date();
    const startToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    const startThat = new Date(dt.getFullYear(), dt.getMonth(), dt.getDate());
    const dayDiff = Math.round((startToday.getTime() - startThat.getTime()) / 86400000);
    const pad = (n) => (n < 10 ? `0${n}` : `${n}`);
    const time = `${pad(dt.getHours())}:${pad(dt.getMinutes())}`;
    const rel = dayDiff === 0
      ? `Hôm nay ${time}`
      : dayDiff === 1
        ? `Hôm qua ${time}`
        : dayDiff <= 7
          ? `${dayDiff} ngày trước`
          : dt.toLocaleDateString("vi-VN");

    el.textContent = rel;
    el.title = dt.toLocaleString("vi-VN");
  });

  document.querySelectorAll("[data-progress-width]").forEach((el) => {
    const w = Number(el.getAttribute("data-progress-width") || "0");
    el.style.width = `${Math.max(0, Math.min(100, w))}%`;
  });

  const chartElement = document.getElementById("scoreChart");
  const dataElement = document.getElementById("statsPortalData");
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

  const gradient = chartElement.getContext("2d").createLinearGradient(0, 0, 0, 320);
  gradient.addColorStop(0, "rgba(37,99,235,0.28)");
  gradient.addColorStop(1, "rgba(248,250,252,0)");

  new Chart(chartElement, {
    type: "line",
    data: {
      labels,
      datasets: [{
        label: "Điểm TB",
        data: values,
        borderColor: "#2563EB",
        backgroundColor: gradient,
        tension: 0.4,
        fill: true,
        pointBackgroundColor: "#2563EB",
        pointRadius: 4,
        borderWidth: 3
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: { legend: { display: false } },
      scales: {
        y: { beginAtZero: true, suggestedMax: 10, grid: { color: "#F1F5F9" } },
        x: { grid: { display: false } }
      }
    }
  });
})();
