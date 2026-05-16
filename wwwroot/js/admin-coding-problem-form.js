(() => {
  const body = document.getElementById("testCasesBody");
  const addButton = document.getElementById("addTestCaseRow");
  if (!body || !addButton) {
    return;
  }

  const createRow = () => {
    const row = document.createElement("tr");
    row.setAttribute("data-testcase-row", "");
    row.innerHTML = `
      <td>
        <input class="form-control testcase-order-input" name="TestCases[0].OrderIndex" readonly value="1" />
      </td>
      <td>
        <textarea class="form-control" name="TestCases[0].Input" rows="2"></textarea>
      </td>
      <td>
        <textarea class="form-control" name="TestCases[0].ExpectedOutput" rows="2"></textarea>
      </td>
      <td class="text-center">
        <input type="hidden" name="TestCases[0].IsSample" value="false" />
        <input class="form-check-input" type="checkbox" name="TestCases[0].IsSample" value="true" />
      </td>
      <td class="text-center">
        <button type="button" class="btn btn-sm btn-outline-primary mb-1" data-mark-sample>Đặt mẫu</button>
        <button type="button" class="btn btn-sm btn-outline-danger" data-remove-testcase>Xóa</button>
      </td>`;
    return row;
  };

  const reindexRows = () => {
    const rows = body.querySelectorAll("[data-testcase-row]");
    rows.forEach((row, index) => {
      row.querySelectorAll("input, textarea").forEach((el) => {
        const currentName = el.getAttribute("name");
        if (!currentName) return;
        el.setAttribute("name", currentName.replace(/TestCases\[\d+\]/, `TestCases[${index}]`));
      });

      const orderInput = row.querySelector(".testcase-order-input");
      if (orderInput) {
        orderInput.value = String(index + 1);
      }
    });
  };

  body.addEventListener("click", (event) => {
    const target = event.target;
    if (!(target instanceof HTMLElement)) return;
    if (target.matches("[data-mark-sample]")) {
      const row = target.closest("[data-testcase-row]");
      if (!row) return;
      const selectedCheckbox = row.querySelector('input[type="checkbox"][name$=".IsSample"]');
      if (!selectedCheckbox) return;
      body.querySelectorAll('input[type="checkbox"][name$=".IsSample"]').forEach((cb) => {
        cb.checked = cb === selectedCheckbox;
      });
      return;
    }
    if (!target.matches("[data-remove-testcase]")) return;

    const row = target.closest("[data-testcase-row]");
    if (!row) return;

    row.remove();
    if (!body.querySelector("[data-testcase-row]")) {
      body.appendChild(createRow());
    }
    reindexRows();
  });

  addButton.addEventListener("click", () => {
    body.appendChild(createRow());
    reindexRows();
  });

  body.addEventListener("change", (event) => {
    const target = event.target;
    if (!(target instanceof HTMLInputElement)) return;
    if (target.type !== "checkbox" || !target.name.endsWith(".IsSample")) return;
    const sampleCheckboxes = body.querySelectorAll('input[type="checkbox"][name$=".IsSample"]');
    if (target.checked) {
      sampleCheckboxes.forEach((cb) => {
        if (cb !== target) cb.checked = false;
      });
    } else {
      const stillChecked = Array.from(sampleCheckboxes).some((cb) => cb.checked);
      if (!stillChecked && sampleCheckboxes.length > 0) {
        sampleCheckboxes[0].checked = true;
      }
    }
  });

  const firstSample = body.querySelector('input[type="checkbox"][name$=".IsSample"]:checked')
    ?? body.querySelector('input[type="checkbox"][name$=".IsSample"]');
  if (firstSample instanceof HTMLInputElement) {
    firstSample.checked = true;
    body.querySelectorAll('input[type="checkbox"][name$=".IsSample"]').forEach((cb) => {
      if (cb !== firstSample) cb.checked = false;
    });
  }

  reindexRows();
})();
