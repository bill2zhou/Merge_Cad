'use strict';

// 合并 CAD 文件（子文件各段内容插入到主文件对应段结束标记之前）
// 移植自 Function/Merge.cs

const { ismatchs, getmatch } = require('./genutil');
const { readFileText, splitLines, writeLines } = require('./io');

// 段配置（C# SectionConfig 数组）
const SECTION_KEYS = ['$BOARD', '$PADS', '$PADSTACKS', '$SHAPES', '$DEVICES',
  '$COMPONENTS', '$SIGNALS', '$ROUTES', '$TESTPINS', '$POWERPINS'];

const END_KEYS = ['$ENDBOARD', '$ENDPADS', '$ENDPADSTACKS', '$ENDSHAPES', '$ENDDEVICES',
  '$ENDCOMPONENTS', '$ENDSIGNALS', '$ENDROUTES', '$ENDTESTPINS', '$ENDPOWERPINS'];

function startRegex(key) { return new RegExp('\\$' + key.slice(1) + '$'); } // \$BOARD$
function endRegex(key) { return new RegExp('\\$END' + key.slice(1) + '$'); }
function sectionOfStart(line) {
  for (let i = 0; i < SECTION_KEYS.length; i++) {
    if (ismatchs(startRegex(SECTION_KEYS[i]), line)) return i;
  }
  return -1;
}

/**
 * 解析子文件的各个段内容（不含开始/结束标记）
 */
function parseSections(lines) {
  const sections = {}; // key: SECTION_KEYS 下标对应段名，如 '$BOARD'
  for (let i = 0; i < SECTION_KEYS.length; i++) {
    const name = SECTION_KEYS[i];
    // BOARD 段跳过 THICKNESS 行
    const skipPattern = name === '$BOARD' ? /THICKNESS.+/ : null;
    const collected = [];
    let inSection = false;
    for (const line of lines) {
      if (ismatchs(startRegex(name), line)) { inSection = true; continue; }
      if (!inSection) continue;
      if (skipPattern && ismatchs(skipPattern, line)) continue;
      if (ismatchs(endRegex(name), line)) break;
      collected.push(line);
    }
    if (collected.length > 0) sections[name] = collected;
  }
  return sections;
}

/**
 * 插入子文件的段内容（BOARD 段跳过 THICKNESS 行）
 */
function insertSubSection(outLines, subSections, sectionName) {
  const list = subSections[sectionName];
  if (!list) return;
  for (const subLine of list) {
    if (sectionName === '$BOARD' && ismatchs(/THICKNESS.+/, subLine)) continue;
    outLines.push(subLine);
  }
}

/**
 * 合并：sub_name 子文件 → 插入到 mb_name 主文件对应段，输出 new_name
 */
function mergeCad(subName, mbName, newName) {
  const subLines = splitLines(readFileText(subName));
  const subSections = parseSections(subLines);

  const mbLines = splitLines(readFileText(mbName));
  const out = [];

  let inBoard = false;        // 对应 C# context.InBoard（一旦进入不再复位）
  let currentIndex = -1;      // 当前所在段在 SECTION_KEYS 中的下标，-1 表示不在段内

  for (const line of mbLines) {
    // 段开始
    const si = sectionOfStart(line);
    if (si >= 0) {
      currentIndex = si;
      if (si === 0) inBoard = true; // $BOARD
    }

    // BOARD 段内跳过 ATTRIBUTE /GRA3/ 行
    if (inBoard && ismatchs(/ATTRIBUTE\s+\/GRA3\/.+/, line)) {
      continue;
    }

    // 段结束：仅在处于对应段内时插入子文件内容
    let ei = -1;
    if (currentIndex >= 0 && ismatchs(endRegex(SECTION_KEYS[currentIndex]), line)) {
      ei = currentIndex;
    }
    if (ei >= 0) {
      insertSubSection(out, subSections, SECTION_KEYS[ei]);
      currentIndex = -1;
    }

    out.push(line);
  }

  writeLines(newName, out);
}

module.exports = { mergeCad };
