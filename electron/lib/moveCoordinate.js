'use strict';

// 坐标移动：平移 X/Y、给焊盘/网络等名称追加后缀（用于拼板）
// 移植自 Function/Move_coordinate.cs，重建字符串的拼接方式与原版完全一致

const { ismatchs, getmatch } = require('./genutil');
const { add } = require('./dutil');
const { readFileText, splitLines, writeLines } = require('./io');

/**
 * 移动坐标
 * @param {string} x        X 偏移量
 * @param {string} y        Y 偏移量
 * @param {string} pin      PIN(测试针编号) 偏移量
 * @param {string} stuf     名称后缀
 * @param {string} oldName  源文件
 * @param {string} newName  输出文件
 */
function move(x, y, pin, stuf, oldName, newName) {
  const text = readFileText(oldName);
  const lines = splitLines(text);
  const out = [];

  let boardFlag = false;
  let padFlag = false;
  let padstackFlag = false;
  let signalFlag = false;
  let shapeFlag = false;
  let compFlag = false;
  let routFlag = false;
  let testFlag = false;
  let powerFlag = false;

  for (let raw of lines) {
    let line = raw;

    // ORIGIN
    if (ismatchs('ORIGIN\\s(\\S+)\\s(\\S+)$', line)) {
      line = 'ORIGIN' + ' ' + x + ' ' + y + '\r\n';
    }

    if (ismatchs('\\$BOARD', line)) {
      boardFlag = true;
    }

    if (boardFlag) {
      if (ismatchs('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line)) {
        const o = getmatch('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line);
        line = 'LINE' + ' ' + add(o[1], x) + ' ' + add(o[2], y) + ' ' +
          add(o[3], x) + ' ' + add(o[4], y);
      }
    }

    if (ismatchs('\\$ENDBOARD', line)) {
      boardFlag = false;
    }

    if (ismatchs('\\$PADS$', line)) {
      padFlag = true;
    }

    if (padFlag) {
      if (ismatchs('PAD\\s(\\S+)(\\s.+)', line)) {
        const o = getmatch('PAD\\s(\\S+)(\\s.+)', line);
        line = 'PAD' + ' ' + o[1] + stuf + o[2];
      }
    }

    if (ismatchs('\\$ENDPADS$', line)) {
      padFlag = false;
    }

    if (ismatchs('\\$PADSTACKS', line)) {
      padstackFlag = true;
    }

    if (padstackFlag) {
      if (ismatchs('PAD\\s(\\S+)(\\s.+)', line)) {
        const o = getmatch('PAD\\s(\\S+)(\\s.+)', line);
        line = 'PAD' + ' ' + o[1] + stuf + o[2];
      }

      if (ismatchs('PADSTACK\\s(\\S+)(\\s.+)', line)) {
        const o = getmatch('PADSTACK\\s(\\S+)(\\s.+)', line);
        line = 'PADSTACK' + ' ' + o[1] + stuf + o[2];
      }
    }

    if (ismatchs('\\$ENDPADSTACKS', line)) {
      padstackFlag = false;
    }

    if (ismatchs('\\$SHAPES', line)) {
      shapeFlag = true;
    }

    if (shapeFlag) {
      if (ismatchs('(PIN\\s\\d+\\s)(\\S+)(\\s.+)', line)) {
        const o = getmatch('(PIN\\s\\d+\\s)(\\S+)(\\s.+)', line);
        line = o[1] + o[2] + stuf + o[3];
      }
    }

    if (ismatchs('\\$ENDSHAPES', line)) {
      shapeFlag = false;
    }

    if (ismatchs('\\$COMPONENTS', line)) {
      compFlag = true;
    }

    if (compFlag) {
      if (ismatchs('COMPONENT\\s(\\S+)', line)) {
        const o = getmatch('COMPONENT\\s(\\S+)', line);
        line = 'COMPONENT' + ' ' + o[1] + stuf;
      }

      if (ismatchs('PLACE\\s(\\S+)\\s(\\S+)', line)) {
        const o = getmatch('PLACE\\s(\\S+)\\s(\\S+)', line);
        line = 'PLACE' + ' ' + add(o[1], x) + ' ' + add(o[2], y);
      }
    }

    if (ismatchs('\\$ENDCOMPONENTS', line)) {
      compFlag = false;
    }

    if (ismatchs('\\$SIGNALS$', line)) {
      signalFlag = true;
    }

    if (signalFlag) {
      if (ismatchs('SIGNAL\\s(\\S+)$', line)) {
        const o = getmatch('SIGNAL\\s(\\S+)$', line);
        line = 'SIGNAL' + ' ' + o[1] + stuf;
      }

      if (ismatchs('NODE\\s(\\S+)(\\s.+)', line)) {
        const o = getmatch('NODE\\s(\\S+)(\\s.+)', line);
        line = 'NODE' + ' ' + o[1] + stuf + o[2];
      }
    }

    if (ismatchs('\\$ENDSIGNALS$', line)) {
      signalFlag = false;
    }

    if (ismatchs('\\$ROUTES', line)) {
      routFlag = true;
    }

    if (routFlag) {
      if (ismatchs('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line)) {
        const o = getmatch('LINE\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line);
        line = 'LINE' + ' ' + add(o[1], x) + ' ' + add(o[2], y) + ' ' +
          add(o[3], x) + ' ' + add(o[4], y);
      }

      if (ismatchs('ARC\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line)) {
        const o = getmatch('ARC\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)', line);
        line = 'ARC' + ' ' + add(o[1], x) + ' ' + add(o[2], y) + ' ' +
          add(o[3], x) + ' ' + add(o[4], y) + ' ' +
          add(o[5], x) + ' ' + add(o[6], y);
      }

      if (ismatchs('ROUTE\\s(\\S+)', line)) {
        const o = getmatch('ROUTE\\s(\\S+)', line);
        line = 'ROUTE' + ' ' + o[1] + stuf;
      }

      if (ismatchs('(VIA\\s\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)\\s(VIA\\d+)', line)) {
        const o = getmatch('(VIA\\s\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)\\s(VIA\\d+)', line);
        line = o[1] + stuf + ' ' + add(o[2], x) + ' ' + add(o[3], y) +
          ' ' + o[4] + ' ' + o[5] + stuf;
      }

      if (ismatchs('NAILLOC\\s(\\w+)\\s(\\d+)\\s(\\S+)\\s(\\S+)\\s(.*)', line)) {
        const o = getmatch('NAILLOC\\s(\\w+)\\s(\\d+)\\s(\\S+)\\s(\\S+)\\s(.*)', line);
        line = 'NAILLOC' + ' ' + o[1] + stuf + ' ' + o[2] +
          add(o[3], x) + ' ' + add(o[4], y) + ' ' + o[5];
      }
    }

    if (ismatchs('\\$ENDROUTES', line)) {
      routFlag = false;
    }

    if (ismatchs('\\$TESTPINS', line)) {
      testFlag = true;
    }

    if (testFlag) {
      if (ismatchs('(TESTPIN\\s)(\\d+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(.*)', line)) {
        const o = getmatch('(TESTPIN\\s)(\\d+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(.*)', line);
        let tp = add(pin, o[2]);
        let a = add(x, o[3]);
        let b = add(y, o[4]);
        if (o[3] === '-32767') {
          a = '-32767';
          b = '-32767';
        }
        line = o[1] + tp + ' ' + a + ' ' + b + ' ' + o[5] + stuf + ' ' + o[6];
      }
    }

    if (ismatchs('\\$ENDTESTPINS', line)) {
      testFlag = false;
    }

    if (ismatchs('\\$POWERPINS', line)) {
      powerFlag = true;
    }

    if (powerFlag) {
      if (ismatchs('POWERPIN\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)', line)) {
        const o = getmatch('POWERPIN\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(\\S+)\\s(.+)', line);
        let a = add(x, o[2]);
        let b = add(y, o[3]);
        if (o[3] === '-32767') {
          a = '-32767';
          b = '-32767';
        }
        line = 'POWERPIN' + ' ' + o[1] + stuf + ' ' + a + ' ' + b + ' ' + o[4] + stuf + ' ' + o[5];
      }
    }

    if (ismatchs('\\$ENDPOWERPINS', line)) {
      powerFlag = false;
    }

    out.push(line);
  }

  writeLines(newName, out);
}

module.exports = { move };
