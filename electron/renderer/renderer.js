'use strict';

/* ---------- DOM 工具 ---------- */
const $ = (id) => document.getElementById(id);

/* ---------- 路径状态 ---------- */
const state = {
  sub: '',    // 小板（用于翻转/旋转/移动）
  main: '',   // 板一 CAD（合并目标）
  child: '',  // 板二 CAD（待合并内容）
  out: ''     // 合并输出
};

/* ---------- 轻提示 ---------- */
function toast(message, isError = false) {
  let el = document.getElementById('toast');
  if (!el) {
    el = document.createElement('div');
    el.id = 'toast';
    el.style.cssText =
      'position:fixed;top:14px;left:50%;transform:translateX(-50%);z-index:200;' +
      'background:#333;color:#fff;padding:9px 18px;border-radius:6px;' +
      'font-size:13px;box-shadow:0 4px 14px rgba(0,0,0,.25);opacity:0;' +
      'transition:opacity .2s;max-width:86%;text-align:center;pointer-events:none;';
    document.body.appendChild(el);
  }
  el.style.background = isError ? '#c23b2e' : '#2d8f4e';
  el.textContent = message;
  requestAnimationFrame(() => { el.style.opacity = '1'; });
  clearTimeout(el._t);
  el._t = setTimeout(() => {
    el.style.opacity = '0';
    setTimeout(() => el.remove(), 250);
  }, 2600);
}

/* ---------- 数值输入过滤（仿原版 KeyPress 规则） ---------- */
function attachNumberFilter(el) {
  el.addEventListener('beforeinput', (e) => {
    if (e.data == null) return; // 删除/退格等
    const next = el.value.slice(0, el.selectionStart) + e.data + el.value.slice(el.selectionEnd);
    if (!/^-?\d*\.?\d*$/.test(next)) e.preventDefault();
  });
}
attachNumberFilter($('xOffset'));
attachNumberFilter($('yOffset'));
attachNumberFilter($('pinOffset'));

/* ---------- 输出路径生成（同目录 + 后缀） ---------- */
function outputFor(input, suffix) {
  const lastSlash = Math.max(input.lastIndexOf('\\'), input.lastIndexOf('/'));
  const lastDot = input.lastIndexOf('.');
  if (lastDot > lastSlash) {
    return input.slice(0, lastDot) + suffix + input.slice(lastDot);
  }
  return input + suffix;
}

/* ---------- 文件选择 ---------- */
function setPath(el, path) {
  if (!path) return;
  el.textContent = path;
  el.title = path;
}

$('btnPickSub').addEventListener('click', async () => {
  const p = await window.api.openFile();
  if (p) { state.sub = p; setPath($('subPath'), p); }
});

$('btnPickMain').addEventListener('click', async () => {
  const p = await window.api.openFile();
  if (p) { state.main = p; setPath($('mainPath'), p); }
});

$('btnPickChild').addEventListener('click', async () => {
  const p = await window.api.openFile();
  if (p) { state.child = p; setPath($('childPath'), p); }
});

$('btnPickOut').addEventListener('click', async () => {
  const p = await window.api.saveFile();
  if (p) { state.out = p; setPath($('outPath'), p); }
});

/* ---------- 校验 ---------- */
function validFile(path, name) {
  if (!path || path === ':') { toast(name + '文件不能为空，请先选择', true); return false; }
  return true;
}

function validNumber(el, name) {
  const v = el.value.trim();
  if (v === '') { toast(name + '不能为空', true); return false; }
  if (!/^-?\d+(\.\d+)?$/.test(v)) { toast(name + '必须是数字', true); return false; }
  return true;
}

/* ---------- 进度弹窗 ---------- */
function runTask(title, descLines, taskType, params) {
  return new Promise((resolve) => {
    const overlay = $('overlay');
    const status = $('dlgStatus');
    const fill = $('barFill');
    const doneBtn = $('btnDone');

    $('dlgTitle').textContent = title;
    $('dlgDesc').textContent = descLines.join('\n');
    status.textContent = '处理中...';
    status.className = 'dialog-status';
    fill.style.width = '0%';
    doneBtn.disabled = true;
    overlay.classList.remove('hidden');

    let current = 0;
    let finished = false;
    let result = null;

    const timer = setInterval(() => {
      if (!finished) {
        current = Math.min(90, current + Math.ceil(Math.random() * 3));
        const dots = '.'.repeat((Math.floor(current / 10) % 4));
        status.textContent = '处理中' + dots;
      } else {
        current = Math.min(100, current + 5);
        if (current >= 100) {
          clearInterval(timer);
          fill.style.width = '100%';
        }
      }
      fill.style.width = current + '%';
    }, 120);

    window.api.runTask(taskType, params)
      .then((r) => {
        result = r;
        if (r.ok) {
          status.textContent = '处理完成！耗时：' + r.elapsedSec + ' 秒';
          status.classList.add('ok');
        } else {
          status.textContent = '处理失败：' + r.error;
          status.classList.add('err');
        }
      })
      .catch((e) => {
        result = { ok: false, error: e.message || String(e) };
        status.textContent = '处理失败：' + result.error;
        status.classList.add('err');
      })
      .finally(() => {
        finished = true;
        doneBtn.disabled = false;
      });

    doneBtn.onclick = () => {
      clearInterval(timer);
      overlay.classList.add('hidden');
      resolve(result);
    };
  });
}

/* ---------- X 轴翻转 / Y 轴翻转 / 旋转 90 度 ---------- */
async function singleFileTask(taskType, suffix, title) {
  if (!validFile(state.sub, '小板')) return;
  const input = state.sub;
  const output = outputFor(input, suffix);
  const r = await runTask(title, ['源文件：' + input, '新文件：' + output], taskType, [input, output]);
  if (r && r.ok) toast('已生成：' + output);
}

$('btnOverturnX').addEventListener('click', () =>
  singleFileTask('overturnX', '_X', '正在 X 轴翻转处理...'));
$('btnOverturnY').addEventListener('click', () =>
  singleFileTask('overturnY', '_Y', '正在 Y 轴翻转处理...'));
$('btnRotate90').addEventListener('click', () =>
  singleFileTask('rotate90', '_R', '正在 90 度旋转处理...'));

/* ---------- 坐标移动 ---------- */
$('btnMove').addEventListener('click', async () => {
  if (!validFile(state.sub, '小板')) return;
  if (!validNumber($('xOffset'), 'X 偏移量')) return;
  if (!validNumber($('yOffset'), 'Y 偏移量')) return;
  if (!validNumber($('pinOffset'), 'PIN 偏移量')) return;
  const postfixEl = $('postfix');
  if (postfixEl.value.trim() === '') { toast('后缀名不能为空', true); return; }

  const input = state.sub;
  const output = outputFor(input, '_O');
  const params = [
    $('xOffset').value.trim(),
    $('yOffset').value.trim(),
    $('pinOffset').value.trim(),
    postfixEl.value.trim(),
    input,
    output
  ];
  const r = await runTask('正在坐标移动处理...', ['源文件：' + input, '新文件：' + output], 'move', params);
  if (r && r.ok) toast('已生成：' + output);
});

/* ---------- 文件合并 ---------- */
$('btnMerge').addEventListener('click', async () => {
  // 原版：板二 CAD（小板）内容并入板一 CAD（主板）
  if (!validFile(state.child, '小板(板二)')) return;
  if (!validFile(state.main, '主板(板一)')) return;
  if (!validFile(state.out, '输出')) return;

  const params = [state.child, state.main, state.out];
  const r = await runTask('正在文件合并处理...', [
    '文件 1（小板）：' + state.child,
    '文件 2（主板）：' + state.main,
    '输出：' + state.out
  ], 'merge', params);
  if (r && r.ok) toast('合并完成：' + state.out);
});
