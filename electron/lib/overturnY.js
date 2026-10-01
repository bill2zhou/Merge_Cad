'use strict';

// Y 轴翻转处理（镜像后还原）：取反 X 坐标、交换 TOP/BOTTOM 层
// 移植自 Function/Overturn_y.cs

const { ismatchs, getmatch } = require('./genutil');
const { add, sub, neg, cmp } = require('./dutil');
const { readFileText, splitLines, writeLines } = require('./io');

const MIRROR_NONE = 0;
const MIRROR_Y = 1;
const MIRROR_X = 2;

const LAYER_NONE = 0;
const LAYER_TOP = 1;
const LAYER_BOTTOM = 2;

function negateDecimal(value) {
  return neg(value);
}

/**
 * Y 轴翻转主入口
 */
function overturn(oldName, newName) {
  let mirrorType = MIRROR_NONE;
  const nailList = [];
  analyzeFile(oldName, (mt) => { mirrorType = mt; }, nailList);

  const viaPadList = [];
  collectViaPads(oldName, nailList, viaPadList);

  processAndWrite(oldName, newName, mirrorType, viaPadList);
}

function analyzeFile(fileName, setMirror, nailList) {
  const lines = splitLines(readFileText(fileName));
  for (const line of lines) {
    if (ismatchs('SHAPE\\s.+\\sMIRRORY\\s.+', line)) {
      setMirror(MIRROR_Y);
    } else if (ismatchs('SHAPE\\s.+\\sMIRRORX\\s.+', line)) {
      setMirror(MIRROR_X);
    }

    if (ismatchs('NAILLOC\\s(\\w+)\\s\\d+\\s\\S+\\s(\\S+)\\s.*\\s\\w+', line)) {
      const o = getmatch('NAILLOC\\s(\\w+)\\s\\d+\\s\\S+\\s(\\S+)\\s.*\\s\\w+', line);
      if (o.length > 1 && o[1] !== '') {
        nailList.push(o[1]);
      }
    }
  }
}

function collectViaPads(fileName, nailList, viaPadList) {
  const lines = splitLines(readFileText(fileName));
  for (const nail of nailList) {
    const pattern = new RegExp('VIA\\s(\\S+)\\s.*\\s' + nail + '$');
    for (const line of lines) {
      if (ismatchs(pattern, line)) {
        const o = getmatch(pattern, line);
        if (o.length > 1 && o[1] !== '') {
          viaPadList.push(o[1]);
        }
      }
    }
  }
}

function processAndWrite(oldName, newName, mirrorType, viaPadList) {
  const lines = splitLines(readFileText(oldName));
  const out = [];
  const flags = {
    board: false,
    pad: false,
    via: false,
    comp: false,
    rout: false,
    test: false,
    power: false,
    lay: LAYER_NONE
  };

  for (const raw of lines) {
    let line = raw;
    line = processLine(line, mirrorType, viaPadList, flags);
    out.push(line);
  }

  writeLines(newName, out);
}

function processLine(line, mirrorType, viaPadList, flags) {
  updateSectionFlags(line, flags);

  if (flags.board) line = processBoardLine(line);
  if (flags.pad) line = processPadLine(line, viaPadList, flags);
  if (flags.comp) line = processComponentLine(line, mirrorType, flags);
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
  flags.pad = toggleFlag(line, '\\$PADSTACKS', '\\$ENDPADSTACKS', flags.pad);
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
      line = 'LINE ' + negateDecimal(o[1]) + ' ' + o[2] + ' ' + negateDecimal(o[3]) + ' ' + o[4];
    }
  }
  return line;
}

function processPadLine(line, viaPadList, flags) {
  if (ismatchs('PADSTACK\\s(\\S+)\\s.*', line)) {
    const o = getmatch('PADSTACK\\s(\\S+)\\s.*', line);
    if (o.length > 1) {
      flags.via = viaPadList.includes(o[1]);
    }
  }

  if (flags.via && ismatchs('PAD (PAD\\d+)\\s(\\w+)\\s(.+)', line)) {
    const o = getmatch('PAD (PAD\\d+)\\s(\\w+)\\s(.+)', line);
    if (o.length >= 4) {
      const layer = o[2];
      let newLayer = layer;
      if (layer === 'SOLDERMASK_TOP') newLayer = 'SOLDERMASK_BOTTOM';
      else if (layer === 'SOLDERMASK_BOTTOM') newLayer = 'SOLDERMASK_TOP';
      else if (layer === 'TOP') newLayer = 'BOTTOM';
      else if (layer === 'BOTTOM') newLayer = 'TOP';
      else if (layer === 'FM_TOP') newLayer = 'FM_BOTTOM';
      else if (layer === 'FM_BOTTOM') newLayer = 'FM_TOP';

      if (newLayer !== layer) {
        line = 'PAD ' + o[1] + ' ' + newLayer + ' ' + o[3];
      }
    }
  }

  return line;
}

function calculateRotation(rotation, layerType, mirrorType) {
  if (layerType === LAYER_TOP) {
    if (mirrorType === MIRROR_X) {
      return cmp(rotation, 180) <= 0 ? sub('180', rotation) : sub('540', rotation);
    }
    if (mirrorType === MIRROR_Y) {
      return sub('360', rotation);
    }
  } else if (layerType === LAYER_BOTTOM) {
    if (mirrorType === MIRROR_Y) {
      return sub('360', rotation);
    }
    if (mirrorType === MIRROR_X) {
      return cmp(rotation, 180) <= 0 ? sub('180', rotation) : sub('540', rotation);
    }
  }
  return rotation;
}

function processComponentLine(line, mirrorType, flags) {
  if (ismatchs('LAYER\\s(\\S+)', line)) {
    const o = getmatch('LAYER\\s(\\S+)', line);
    if (o.length > 1) {
      if (o[1] === 'TOP') {
        flags.lay = LAYER_TOP;
        line = 'LAYER BOTTOM';
      } else if (o[1] === 'BOTTOM') {
        flags.lay = LAYER_BOTTOM;
        line = 'LAYER TOP';
      } else {
        flags.lay = LAYER_NONE;
      }
    }
  }

  if (ismatchs('ROTATION\\s(\\S+)', line)) {
    const o = getmatch('ROTATION\\s(\\S+)', line);
    if (o.length > 1) {
      const newRotation = calculateRotation(o[1], flags.lay, mirrorType);
      line = 'ROTATION ' + newRotation;
      flags.lay = LAYER_NONE;
    }
  }

  if (ismatchs('PLACE\\s(\\S+)\\s(\\S+)', line)) {
    const o = getmatch('PLACE\\s(\\S+)\\s(\\S+)', line);
    if (o.length >= 3) {
      line = 'PLACE ' + negateDecimal(o[1]) + ' ' + o[2];
    }
  }

  if (ismatchs('SHAPE\\s(.+)\\s(\\S+)\\s(\\S+)', line)) {
    const o = getmatch('SHAPE\\s(.+)\\s(\\S+)\\s(\\S+)', line);
    if (o.length >= 4) {
      line = processShapeLine(o, mirrorType);
    }
  }

  return line;
}

function processShapeLine(o, mirrorType) {
  const a = o[2];
  const b = o[3];
  let c = '0';
  let d = '0';

  if (a === '0' && b === '0') {
    if (mirrorType === MIRROR_Y) c = 'MIRRORY';
    else if (mirrorType === MIRROR_X) c = 'MIRRORX';
    d = 'FLIP';
  } else if (b === 'FLIP') {
    c = '0';
    d = '0';
  }

  return 'SHAPE ' + o[1] + ' ' + c + ' ' + d;
}

function processRouteLine(line) {
  // LAYER TOP/BOTTOM
  if (ismatchs('LAYER\\s+(TOP|BOTTOM).*', line)) {
    const o = getmatch('LAYER\\s+(TOP|BOTTOM).*', line);
    if (o.length > 1) {
      const newLayer = o[1] === 'TOP' ? 'BOTTOM' : 'TOP';
      line = 'LAYER ' + newLayer;
    }
  }

  // LAYER SOLDERMASK
  if (ismatchs('LAYER\\s+(SOLDERMASK_TOP|SOLDERMASK_BOTTOM)', line)) {
    const o = getmatch('LAYER\\s+(SOLDERMASK_TOP|SOLDERMASK_BOTTOM)', line);
    if (o.length > 1) {
      const newLayer = o[1] === 'SOLDERMASK_TOP' ? 'SOLDERMASK_BOTTOM' : 'SOLDERMASK_TOP';
      line = 'LAYER ' + newLayer;
    }
  }

  // LINE（取反 x1/x2）
  if (ismatchs('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line)) {
    const o = getmatch('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line);
    if (o.length >= 5) {
      line = 'LINE ' + negateDecimal(o[1]) + ' ' + o[2] + ' ' + negateDecimal(o[3]) + ' ' + o[4];
    }
  }

  // ARC（取反 x1/x2/x3）
  if (ismatchs('ARC\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line)) {
    const o = getmatch('ARC\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line);
    if (o.length >= 7) {
      line = 'ARC ' + negateDecimal(o[1]) + ' ' + o[2] + ' ' + negateDecimal(o[3]) + ' ' +
        o[4] + ' ' + negateDecimal(o[5]) + ' ' + o[6];
    }
  }

  // VIA（取反 x）
  if (ismatchs('(VIA\\s\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)\\s(VIA\\d+)', line)) {
    const o = getmatch('(VIA\\s\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)\\s(VIA\\d+)', line);
    if (o.length >= 6) {
      line = o[1] + ' ' + negateDecimal(o[2]) + ' ' + o[3] + ' ' + o[4] + ' ' + o[5];
    }
  }

  // NAILLOC（取反 x）
  if (ismatchs('NAILLOC\\s(\\w+)\\s(\\d+)\\s(\\S+)\\s(\\S+)\\s(.*)\\s(\\w+)', line)) {
    const o = getmatch('NAILLOC\\s(\\w+)\\s(\\d+)\\s(\\S+)\\s(\\S+)\\s(.*)\\s(\\w+)', line);
    if (o.length >= 7) {
      const newLayer = o[6] === 'TOP' ? 'BOTTOM' : o[6] === 'BOTTOM' ? 'TOP' : o[6];
      line = 'NAILLOC ' + o[1] + ' ' + o[2] + ' ' + negateDecimal(o[3]) + ' ' + o[4] +
        ' ' + o[5] + ' ' + newLayer;
    }
  }

  return line;
}

function processTestPinLine(line) {
  if (ismatchs('(TESTPIN\\s)(\\d+)\\s(\\S+\\.\\S+)\\s(\\S+\\.\\S+)\\s(.*)\\s(\\w+)', line)) {
    const o = getmatch('(TESTPIN\\s)(\\d+)\\s(\\S+\\.\\S+)\\s(\\S+\\.\\S+)\\s(.*)\\s(\\w+)', line);
    if (o.length >= 7) {
      const newLayer = o[6] === 'TOP' ? 'BOTTOM' : o[6] === 'BOTTOM' ? 'TOP' : o[6];
      line = o[1] + o[2] + ' ' + negateDecimal(o[3]) + ' ' + o[4] + ' ' + o[5] + ' ' + newLayer;
    }
  }
  return line;
}

function processPowerPinLine(line) {
  if (ismatchs('POWERPIN\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)\\s(\\w+)', line)) {
    const o = getmatch('POWERPIN\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)\\s(\\w+)', line);
    if (o.length >= 6) {
      const newLayer = o[5] === 'TOP' ? 'BOTTOM' : o[5] === 'BOTTOM' ? 'TOP' : o[5];
      line = 'POWERPIN ' + o[1] + ' ' + negateDecimal(o[2]) + ' ' + o[3] + ' ' + o[4] + ' ' + newLayer;
    }
  }
  return line;
}

module.exports = { overturn };
