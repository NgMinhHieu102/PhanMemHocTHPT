(() => {
  const input = document.getElementById("newPasswordInput");
  const hint = document.getElementById("passwordStrengthHint");
  const rules = document.getElementById("passwordRules");
  if (!input || !hint || !rules) {
    return;
  }

  const ruleChecks = {
    length: (s) => s.length >= 10,
    upper: (s) => /[A-Z]/.test(s),
    lower: (s) => /[a-z]/.test(s),
    digit: (s) => /\d/.test(s),
    special: (s) => /[^A-Za-z0-9]/.test(s)
  };

  const render = (password) => {
    const entries = Object.entries(ruleChecks);
    let passed = 0;

    entries.forEach(([key, check]) => {
      const ok = check(password);
      const li = rules.querySelector(`[data-rule="${key}"]`);
      if (!li) return;
      li.classList.toggle("text-success", ok);
      li.classList.toggle("text-danger", !ok && password.length > 0);
      if (ok) passed += 1;
    });

    if (!password) {
      hint.textContent = "Độ mạnh: Chưa nhập";
      hint.className = "form-text";
      return;
    }

    if (passed <= 2) {
      hint.textContent = "Độ mạnh: Yếu";
      hint.className = "form-text text-danger";
    } else if (passed <= 4) {
      hint.textContent = "Độ mạnh: Trung bình";
      hint.className = "form-text text-warning";
    } else {
      hint.textContent = "Độ mạnh: Mạnh";
      hint.className = "form-text text-success";
    }
  };

  input.addEventListener("input", () => render(input.value));
  render(input.value);
})();
