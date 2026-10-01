'use strict';

// 正则表达式缓存（对应 C# 的 _regexCache）
const cache = new Map();

function getRegex(pattern) {
  const source = pattern instanceof RegExp ? pattern.source : String(pattern);
  let re = cache.get(source);
  if (!re) {
    re = new RegExp(source);
    cache.set(source, re);
  }
  return re;
}

/**
 * 判断字符串是否匹配（对应 Regex.IsMatch）
 * @param {string|RegExp} pattern
 * @param {string} str
 */
function ismatchs(pattern, str) {
  const re = getRegex(pattern);
  re.lastIndex = 0;
  return re.test(str);
}

/**
 * 返回首个匹配及所有分组（对应 Regex.Matches + Group 遍历）
 * 返回数组：第 0 项为完整匹配，之后为各分组
 * @param {string|RegExp} pattern
 * @param {string} str
 */
function getmatch(pattern, str) {
  const re = getRegex(pattern);
  re.lastIndex = 0;
  const m = re.exec(str);
  if (!m) return [];
  return Array.from(m, (x) => (x === undefined ? '' : x));
}

module.exports = { ismatchs, getmatch, getRegex };
