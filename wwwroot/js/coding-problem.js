(() => {
  const dataElement = document.getElementById("codingProblemData");
  const textarea = document.getElementById("codeEditor");
  const stdinInput = document.getElementById("stdinInput");
  const langSelect = document.getElementById("languageSelect");
  const resultPanel = document.getElementById("resultPanel");
  const resultBody = document.getElementById("resultBody");
  const btnRunSample = document.getElementById("btnRunSample");
  const btnSubmit = document.getElementById("btnSubmit");
  const btnCloseResult = document.getElementById("btnCloseResult");

  if (!dataElement || !textarea || !stdinInput || !langSelect || !resultPanel || !resultBody || !btnRunSample || !btnSubmit || !btnCloseResult) {
    return;
  }

  const problemId = Number(dataElement.dataset.problemId || "0");
  const runUrl = dataElement.dataset.runUrl || "";
  const submitUrl = dataElement.dataset.submitUrl || "";
  const modeMap = { python: "python", cpp: "text/x-c++src" };

  const editor = CodeMirror.fromTextArea(textarea, {
    mode: "python",
    theme: "dracula",
    lineNumbers: true,
    tabSize: 4,
    indentUnit: 4,
    indentWithTabs: false,
    lineWrapping: true,
    autoCloseBrackets: true,
    matchBrackets: true,
    viewportMargin: Infinity
  });
  editor.setSize("100%", "100%");

  const getToken = () => {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : "";
  };

  const showResult = (html) => {
    resultBody.innerHTML = html;
    resultPanel.classList.remove("d-none");
  };

  const setLoading = (button, loading) => {
    button.disabled = loading;
    if (loading) {
      button.dataset.origHtml = button.innerHTML;
      button.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Đang chạy...';
    } else {
      button.innerHTML = button.dataset.origHtml || button.innerHTML;
    }
  };

  const escHtml = (s) => {
    const d = document.createElement("div");
    d.textContent = s;
    return d.innerHTML;
  };

  const verdictClass = (v) => ({ AC: "coding-verdict-ac", WA: "coding-verdict-wa", TLE: "coding-verdict-tle", RE: "coding-verdict-re", CE: "coding-verdict-ce" }[v] || "");
  const verdictLabel = (v) => ({ AC: "Accepted", WA: "Wrong Answer", TLE: "Time Limit Exceeded", RE: "Runtime Error", CE: "Compilation Error" }[v] || v);
  const verdictIcon = (v) => ({
    AC: '<i class="bi bi-check-circle-fill"></i>',
    WA: '<i class="bi bi-x-circle-fill"></i>',
    TLE: '<i class="bi bi-hourglass-split"></i>',
    RE: '<i class="bi bi-exclamation-triangle-fill"></i>',
    CE: '<i class="bi bi-bug-fill"></i>'
  }[v] || '<i class="bi bi-question-circle"></i>');

  const callApi = (url, button) => {
    setLoading(button, true);
    return fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "X-CSRF-TOKEN": getToken(),
        RequestVerificationToken: getToken()
      },
      body: JSON.stringify({
        problemId,
        sourceCode: editor.getValue(),
        language: langSelect.value,
        stdin: stdinInput.value
      })
    })
      .then((r) => r.json())
      .then((data) => ({ ok: true, data }))
      .catch((err) => ({ ok: false, err }))
      .finally(() => setLoading(button, false));
  };

  langSelect.addEventListener("change", () => {
    const lang = langSelect.value;
    editor.setOption("mode", modeMap[lang] || "python");
    if (lang === "cpp" && editor.getValue().startsWith("#")) {
      editor.setValue("#include <iostream>\nusing namespace std;\n\nint main() {\n    int n;\n    cin >> n;\n    \n    return 0;\n}\n");
    }
  });

  btnCloseResult.addEventListener("click", () => {
    resultPanel.classList.add("d-none");
  });

  btnRunSample.addEventListener("click", async () => {
    const stdinValue = stdinInput.value.trim();
    if (!stdinValue) {
      showResult(`<div class="text-warning"><i class="bi bi-exclamation-triangle-fill me-1"></i>Vui lòng nhập dữ liệu vào (stdin) trước khi chạy thử.</div>`);
      return;
    }

    const result = await callApi(runUrl, btnRunSample);
    if (!result.ok) {
      showResult(`<div class="text-danger">Lỗi kết nối: ${escHtml(result.err?.message || "")}</div>`);
      return;
    }

    const data = result.data;
    if (!data.success) {
      showResult(`<div class="text-danger"><i class="bi bi-x-circle me-1"></i>${escHtml(data.error || "Lỗi không xác định")}</div>`);
      return;
    }

    let html = "";
    if (data.compileOutput) {
      html += `<div class="coding-verdict-ce mb-2"><strong>Lỗi biên dịch:</strong><pre class="mb-0 mt-1">${escHtml(data.compileOutput)}</pre></div>`;
    }
    if (data.stderr) {
      html += `<div class="text-warning small mb-2"><strong>Stderr:</strong><pre class="mb-0 mt-1">${escHtml(data.stderr)}</pre></div>`;
    }
    html += `<div class="mb-1"><strong>Output:</strong></div><pre class="coding-io-pre">${escHtml(data.stdout || "(trống)")}</pre>`;
    html += `<div class="small text-muted mt-2">Thời gian: ${data.timeMs}ms · Bộ nhớ: ${data.memoryKb}KB</div>`;
    showResult(html);
  });

  btnSubmit.addEventListener("click", async () => {
    const result = await callApi(submitUrl, btnSubmit);
    if (!result.ok) {
      showResult(`<div class="text-danger">Lỗi kết nối: ${escHtml(result.err?.message || "")}</div>`);
      return;
    }

    const data = result.data;
    if (!data.success) {
      showResult(`<div class="text-danger"><i class="bi bi-x-circle me-1"></i>${escHtml(data.error || "Lỗi không xác định")}</div>`);
      return;
    }

    let html = `<div class="coding-verdict-banner ${verdictClass(data.verdict)}">`;
    html += `<span class="coding-verdict-icon">${verdictIcon(data.verdict)}</span>`;
    html += `<span class="coding-verdict-text">${verdictLabel(data.verdict)}</span></div>`;
    html += `<div class="mt-2 small"><span class="me-3">Test đúng: <strong>${data.passedTests}/${data.totalTests}</strong></span>`;
    html += `<span class="me-3">Thời gian: ${data.executionTimeMs}ms</span><span>Bộ nhớ: ${data.memoryUsedKb}KB</span></div>`;
    if (data.compileOutput) {
      html += `<div class="mt-2"><strong>Lỗi biên dịch:</strong><pre class="coding-io-pre mt-1">${escHtml(data.compileOutput)}</pre></div>`;
    }
    showResult(html);
  });
})();
