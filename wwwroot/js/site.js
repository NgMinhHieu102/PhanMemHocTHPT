'use strict';

/**
 * Tabs lọc theo khối ( Topics/Index ).
 */
window.initTopicGradeFilter = function initTopicGradeFilter() {
  var tabs = document.querySelectorAll('[data-topic-grade-tab],[data-topic-all]');
  var cells = document.querySelectorAll('[data-topic-card-cell][data-topic-grade]');
  if (!tabs.length || !cells.length) return;

  function applyFilter(isAll, gradeVal) {
    cells.forEach(function (cell) {
      var g = cell.getAttribute('data-topic-grade') || '';
      var show = isAll === true || g === gradeVal;
      cell.style.display = show ? '' : 'none';
    });

    tabs.forEach(function (t) {
      var ta = t.hasAttribute('data-topic-all');
      var tg = t.getAttribute('data-topic-grade-tab');
      var matchAll = !!isAll && ta;
      var matchGrade =
        !isAll && tg === gradeVal;
      t.classList.toggle('active', matchAll || matchGrade);
    });
  }

  tabs.forEach(function (tab) {
    tab.addEventListener('click', function () {
      var isAll = tab.hasAttribute('data-topic-all');
      var grade = tab.getAttribute('data-topic-grade-tab');
      applyFilter(isAll, grade || '');
    });
  });

  var firstAll = Array.prototype.slice.call(tabs).find(function (t) {
    return t.hasAttribute('data-topic-all');
  });
  if (firstAll) applyFilter(true, '');
};

document.addEventListener('DOMContentLoaded', function () {
  initTopicGradeFilter();

  /** Chào theo khung giờ ( Views/StudentPortal/Dashboard ) */
  var greetEl = document.getElementById('dashboard-time-greeting');
  if (greetEl && greetEl.dataset.hello) {
    var h = new Date().getHours();
    var part =
      h < 11 ? 'Chúc em một ngày học tốt!' :
      h < 17 ? 'Giữ nhịp luyện tập trong ngày nhé!' :
      'Nghỉ ngơi hợp lý và quay lại khi tinh thần sảng khoái nhất.';
    greetEl.textContent = part;
  }

  /** Thống kê — thời gian relative + format thời lượng */
  document.querySelectorAll('[data-utc]').forEach(function (el) {
    var utc = el.getAttribute('data-utc');
    if (!utc || !window.Intl || !Date.parse) return;
    var iso = utc.indexOf('Z') === utc.length - 1 ? utc : utc + 'Z';
    var dt = new Date(iso);
    if (Number.isNaN(+dt)) return;

    var now = new Date();
    var midToday = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    var midYesterday = new Date(midToday.getTime() - 86400000);
    var t = dt.getTime();

    function pad(n) {
      return n < 10 ? '0' + n : String(n);
    }

    var timeStr = pad(dt.getHours()) + ':' + pad(dt.getMinutes());
    var rel = '';

    if (t >= midToday.getTime()) {
      rel = 'Hôm nay ' + timeStr;
    } else if (t >= midYesterday.getTime() && t < midToday.getTime()) {
      rel = 'Hôm qua ' + timeStr;
    } else {
      var diffDays = Math.floor((midToday.getTime() - new Date(dt.getFullYear(), dt.getMonth(), dt.getDate()).getTime()) / 86400000);
      rel = diffDays <= 7 ? diffDays + ' ngày trước' : dt.toLocaleDateString('vi-VN');
    }

    var label = rel;
    var titleFallback = dt.toLocaleString('vi-VN');
    el.textContent = label;
    el.title = titleFallback;
  });

  document.querySelectorAll('[data-seconds]').forEach(function (el) {
    var raw = parseInt(el.getAttribute('data-seconds') || '', 10);
    if (Number.isNaN(raw)) return;
    var m = Math.floor(raw / 60);
    var s = raw % 60;
    el.textContent = m > 0 ? m + ' phút ' + s + ' giây' : s + ' giây';
  });

  document.querySelectorAll('form[data-confirm]').forEach(function (form) {
    form.addEventListener('submit', function (event) {
      var message = form.getAttribute('data-confirm') || 'Bạn có chắc chắn muốn thực hiện thao tác này?';
      if (!window.confirm(message)) {
        event.preventDefault();
      }
    });
  });
});
