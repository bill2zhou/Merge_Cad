'use strict';

const fs = require('fs');

/**
 * 读取文件文本（自动去除 UTF-8 BOM），对应 C# StreamReader 默认行为
 */
function readFileText(filePath) {
  let buf = fs.readFileSync(filePath);
  // 去掉 UTF-8 BOM
  if (buf.length >= 3 && buf[0] === 0xef && buf[1] === 0xbb && buf[2] === 0xbf) {
    buf = buf.slice(3);
  }
  return buf.toString('utf8');
}

/**
 * 按行拆分（兼容 \r\n、\n、\r），并剔除末尾分隔符产生的空元素
 */
function splitLines(text) {
  if (text === '') return [];
  const t = text.replace(/(?:\r\n|\n|\r)$/, '');
  if (t === '') return [];
  return t.split(/\r\n|\n|\r/);
}

/**
 * 逐行写出（等价于 C# 对每行调用 wr.WriteLine(line)，
 * 每行末尾追加 \r\n）
 */
function writeLines(filePath, lines) {
  const content = lines.map((l) => l + '\r\n').join('');
  fs.writeFileSync(filePath, content, 'utf8');
}

module.exports = { readFileText, splitLines, writeLines };
