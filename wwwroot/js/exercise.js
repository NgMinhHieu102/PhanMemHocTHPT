'use strict';

let totalSeconds;
let elapsedSeconds = 0;
let timerInterval;

function startTimer(seconds) {
  totalSeconds = seconds;
  updateTimerDisplay(seconds);

  timerInterval = setInterval(() => {
    elapsedSeconds++;
    const remaining = totalSeconds - elapsedSeconds;

    document.getElementById('timeSpent').value = elapsedSeconds;
    updateTimerDisplay(remaining);

    if (remaining === 300) {
      const timerEl = document.getElementById('timer');
      timerEl.classList.replace('bg-danger', 'bg-warning');
    }

    if (remaining <= 0) {
      clearInterval(timerInterval);
      alert('⏰ Hết giờ! Bài làm của bạn sẽ được nộp tự động.');
      document.getElementById('exerciseForm').submit();
    }
  }, 1000);
}

function updateTimerDisplay(seconds) {
  if (seconds < 0) {
    seconds = 0;
  }

  const m = Math.floor(seconds / 60).toString().padStart(2, '0');
  const s = (seconds % 60).toString().padStart(2, '0');
  document.getElementById('timerDisplay').textContent = `${m}:${s}`;
}

function selectAnswer(el, questionId) {
  el.closest('.answer-options')
    .querySelectorAll('.answer-option')
    .forEach((opt) => {
      opt.classList.remove('selected');
      opt.style.borderColor = '';
    });

  el.classList.add('selected');
  el.querySelector('input[type=radio]').checked = true;

  updateNavigationDot(questionId);
  updateProgressBar();
}

function updateNavigationDot() {
  document.querySelectorAll('.q-dot').forEach((dot, idx) => {
    const qCard = document.querySelector(`[data-question-index="${idx}"]`);
    const hasAnswer = isQuestionAnswered(qCard);
    dot.className = hasAnswer
      ? 'btn btn-sm btn-primary q-dot fw-bold'
      : 'btn btn-sm btn-outline-secondary q-dot';
  });
}

function updateProgressBar() {
  const total = document.querySelectorAll('.question-card').length;
  const answered = Array.from(document.querySelectorAll('.question-card'))
    .filter((card) => isQuestionAnswered(card)).length;
  const pct = total > 0 ? (answered / total * 100).toFixed(0) : 0;
  const bar = document.getElementById('progressBar');
  if (bar) {
    bar.style.width = `${pct}%`;
    bar.textContent = `${answered}/${total}`;
  }
}

function isQuestionAnswered(card) {
  if (!card) {
    return false;
  }

  const trueFalseGroups = new Set(
    Array.from(card.querySelectorAll('input[type=radio][name*="TruefalseAnswers"]'))
      .map((input) => input.name)
  );

  if (trueFalseGroups.size > 0) {
    return Array.from(trueFalseGroups)
      .every((name) => card.querySelector(`input[name="${CSS.escape(name)}"]:checked`));
  }

  return Boolean(card.querySelector('input[type=radio]:checked'));
}

function scrollToQuestion(index) {
  document.querySelector(`[data-question-index="${index}"]`)
    ?.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function confirmSubmit() {
  const total = document.querySelectorAll('.question-card').length;
  const answered = Array.from(document.querySelectorAll('.question-card'))
    .filter((card) => isQuestionAnswered(card)).length;
  const skipped = total - answered;

  const msg = skipped > 0
    ? `Bạn còn ${skipped} câu chưa trả lời. Bạn có chắc muốn nộp bài không?`
    : 'Bạn có chắc muốn nộp bài không?';

  if (confirm(msg)) {
    clearInterval(timerInterval);
    document.getElementById('exerciseForm').submit();
  }
}

document.addEventListener('DOMContentLoaded', () => {
  const form = document.getElementById('exerciseForm');
  if (!form) {
    return;
  }

  const timeLimit = Number(form.getAttribute('data-time-limit') || '0');
  if (timeLimit > 0) {
    startTimer(timeLimit);
  }

  form.querySelectorAll('[data-answer-option]').forEach((option) => {
    option.addEventListener('click', () => {
      selectAnswer(option);
    });
  });

  form.querySelectorAll('input[type=radio]').forEach((radio) => {
    radio.addEventListener('change', () => {
      updateProgressBar();
      updateNavigationDot();
    });
  });

  const submitButton = document.getElementById('submitExerciseButton');
  if (submitButton) {
    submitButton.addEventListener('click', confirmSubmit);
  }

  document.querySelectorAll('[data-question-dot-index]').forEach((dot) => {
    dot.addEventListener('click', () => {
      const idx = Number(dot.getAttribute('data-question-dot-index') || '0');
      scrollToQuestion(idx);
    });
  });

  updateProgressBar();
  updateNavigationDot();
});
