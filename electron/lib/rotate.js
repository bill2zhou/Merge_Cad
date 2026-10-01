'use strict';

// 90 度旋转：交换 X/Y 并取反新 X（即 x' = -y, y' = x）
// 移植自 Function/Rotate.cs

const { ismatchs, getmatch } = require('./genutil');
const { neg, normalizeRotation } = require('./dutil');
const { readFileText, splitLines, writeLines } = require('./io');

function negateDecimal(value) {
  return neg(value);
}

/**
 * 90 度旋转主入口
 */
function rotate90(oldName, newName) {
  const lines = splitLines(readFileText(oldName));
  const out = [];
  const flags = {
    board: false,
    comp: false,
    rout: false,
    test: false,
    power: false
  };

  for (const raw of lines) {
    let line = raw;
    line = processLine(line, flags);
    out.push(line);
  }

  writeLines(newName, out);
}

function processLine(line, flags) {
  updateSectionFlags(line, flags);

  if (flags.board) line = processBoardLine(line);
  if (flags.comp) line = processComponentLine(line);
  if (flags.rout) line = processRouteLine(line);
  if (flags.test) line = processTestPinLine(line);
  if (flags.power) line = processPowerPinLine(line);

  return line;
}

function toggleFlag(line, startPattern, endPattern, currentFlag) {
  if (ismatchs(startPattern, line)) return true;
  if (ismatchs(endPattern, line)) return false;
  return currentFlag;
}

function updateSectionFlags(line, flags) {
  flags.board = toggleFlag(line, '\\$BOARD', '\\$ENDBOARD', flags.board);
  flags.comp = toggleFlag(line, '\\$COMPONENTS', '\\$ENDCOMPONENTS', flags.comp);
  flags.rout = toggleFlag(line, '\\$ROUTES', '\\$ENDROUTES', flags.rout);
  flags.test = toggleFlag(line, '\\$TESTPINS', '\\$ENDTESTPINS', flags.test);
  flags.power = toggleFlag(line, '\\$POWERPINS', '\\$ENDPOWERPINS', flags.power);
}

// ---- 各段处理 ----

function processBoardLine(line) {
  if (ismatchs('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line)) {
    const o = getmatch('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line);
    if (o.length >= 5) {
      line = 'LINE ' + negateDecimal(o[2]) + ' ' + o[1] + ' ' + negateDecimal(o[4]) + ' ' + o[3];
    }
  }
  return line;
}

function processComponentLine(line) {
  if (ismatchs('ROTATION\\s(\\S+)', line)) {
    const o = getmatch('ROTATION\\s(\\S+)', line);
    if (o.length > 1) {
      const newRotation = normalizeRotation(rotateAdd(o[1], '90'));
      line = 'ROTATION ' + newRotation;
    }
  }

  if (ismatchs('PLACE\\s(\\S+)\\s(\\S+)', line)) {
    const o = getmatch('PLACE\\s(\\S+)\\s(\\S+)', line);
    if (o.length >= 3) {
      line = 'PLACE ' + negateDecimal(o[2]) + ' ' + o[1];
    }
  }

  return line;
}

function rotateAdd(rotation, delta) {
  const { add } = require('./dutil');
  return add(rotation, delta);
}

function processRouteLine(line) {
  // LINE
  if (ismatchs('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line)) {
    const o = getmatch('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line);
    if (o.length >= 5) {
      line = 'LINE ' + negateDecimal(o[2]) + ' ' + o[1] + ' ' + negateDecimal(o[4]) + ' ' + o[3];
    }
  }

  // ARC
  if (ismatchs('ARC\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line)) {
    const o = getmatch('ARC\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line);
    if (o.length >= 7) {
      line = 'ARC ' + negateDecimal(o[2]) + ' ' + o[1] + ' ' + negateDecimal(o[4]) + ' ' +
        o[3] + ' ' + negateDecimal(o[6]) + ' ' + o[5];
    }
  }

  // VIA
  if (ismatchs('(VIA\\s\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)\\s(VIA\\d+)', line)) {
    const o = getmatch('(VIA\\s\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)\\s(VIA\\d+)', line);
    if (o.length >= 6) {
      line = o[1] + ' ' + negateDecimal(o[3]) + ' ' + o[2] + ' ' + o[4] + ' ' + o[5];
    }
  }

  // NAILLOC
  if (ismatchs('(NAILLOC\\s\\w+\\s\\d+)\\s(\\S+)\\s(\\S+)\\s(.*)', line)) {
    const o = getmatch('(NAILLOC\\s\\w+\\s\\d+)\\s(\\S+)\\s(\\S+)\\s(.*)', line);
    if (o.length >= 5) {
      line = o[1] + ' ' + negateDecimal(o[3]) + ' ' + o[2] + ' ' + o[4];
    }
  }

  return line;
}

function processTestPinLine(line) {
  if (ismatchs('(TESTPIN\\s)(\\d+)\\s(\\S+\\.\\S+)\\s(\\S+\\.\\S+)\\s(\\S+)\\s(.*)', line)) {
    const o = getmatch('(TESTPIN\\s)(\\d+)\\s(\\S+\\.\\S+)\\s(\\S+\\.\\S+)\\s(\\S+)\\s(.*)', line);
    if (o.length >= 7) {
      line = o[1] + o[2] + ' ' + negateDecimal(o[4]) + ' ' + o[3] + ' ' + o[5] + ' ' + o[6];
    }
  }
  return line;
}

function processPowerPinLine(line) {
  if (ismatchs('POWERPIN\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)', line)) {
    const o = getmatch('POWERPIN\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)', line);
    if (o.length >= 6) {
      line = 'POWERPIN ' + o[1] + ' ' + negateDecimal(o[3]) + ' ' + o[2] + ' ' + o[4] + ' ' + o[5];
    }
  }
  return line;
}

module.exports = { rotate90 };
