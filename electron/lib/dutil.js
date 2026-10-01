'use strict';

// 十进制精确运算工具，模拟 C# decimal 的加/减/取反/比较，
// 且保留小数位数（scale = max(参与运算数的 scale)），
// 使格式化结果与 C# decimal.ToString() 保持一致。

const TEN = 10n;

function parseNum(s) {
  s = String(s).trim();
  let neg = false;
  if (s[0] === '-') { neg = true; s = s.slice(1); }
  else if (s[0] === '+') { s = s.slice(1); }
  const dot = s.indexOf('.');
  let intp = dot === -1 ? s : s.slice(0, dot);
  let frac = dot === -1 ? '' : s.slice(dot + 1);
  if (intp === '' || intp === undefined) intp = '0';
  if (frac === '' || frac === undefined) frac = '';
  const scale = frac.length;
  const mag = BigInt((intp + frac) || '0');
  return { neg, mag, scale };
}

function format(mag, scale, neg) {
  const isZero = mag === 0n;
  if (scale === 0) {
    if (neg && !isZero) return '-' + mag.toString();
    return mag.toString();
  }
  let str = mag.toString().padStart(scale + 1, '0');
  const ip = str.slice(0, str.length - scale);
  const fp = str.slice(str.length - scale);
  let out = ip + '.' + fp;
  if (neg && !isZero) out = '-' + out;
  return out;
}

/**
 * 加法：返回 string，scale = max(scaleA, scaleB)
 */
function add(a, b) {
  const A = parseNum(a);
  const B = parseNum(b);
  const scale = Math.max(A.scale, B.scale);
  const sa = A.mag * TEN ** BigInt(scale - A.scale);
  const sb = B.mag * TEN ** BigInt(scale - B.scale);
  let sum = (A.neg ? -sa : sa) + (B.neg ? -sb : sb);
  const neg = sum < 0n;
  if (neg) sum = -sum;
  return format(sum, scale, neg);
}

/**
 * 取反
 */
function neg(a) {
  const A = parseNum(a);
  return format(A.mag, A.scale, A.mag !== 0n && !A.neg);
}

/**
 * 减法：a - b
 */
function sub(a, b) {
  return add(a, neg(b));
}

/**
 * 比较：返回 -1 / 0 / 1
 */
function cmp(a, b) {
  const A = parseNum(a);
  const B = parseNum(b);
  const scale = Math.max(A.scale, B.scale);
  const sa = (A.neg ? -A.mag : A.mag) * TEN ** BigInt(scale - A.scale);
  const sb = (B.neg ? -B.mag : B.mag) * TEN ** BigInt(scale - B.scale);
  return sa < sb ? -1 : sa > sb ? 1 : 0;
}

/**
 * 归一化角度到 [0,360)（保留小数位）
 */
function normalizeRotation(rotation) {
  let r = rotation;
  while (cmp(r, '0') < 0) r = add(r, '360');
  while (cmp(r, '360') >= 0) r = sub(r, '360');
  return r;
}

module.exports = { add, sub, neg, cmp, normalizeRotation };
