'use strict';

const { app, BrowserWindow, ipcMain, dialog } = require('electron');
const path = require('path');
const fs = require('fs');

const { overturn: overturnX } = require('./lib/overturnX');
const { overturn: overturnY } = require('./lib/overturnY');
const { rotate90 } = require('./lib/rotate');
const { move } = require('./lib/moveCoordinate');
const { mergeCad } = require('./lib/merge');

const CAD_FILTERS = [
  { name: 'CAD 文件 (*.CAD)', extensions: ['cad'] },
  { name: '所有文件', extensions: ['*'] }
];

let mainWindow = null;

function createWindow() {
  mainWindow = new BrowserWindow({
    width: 470,
    height: 620,
    resizable: false,
    title: 'GenCAD 文件合并工具',
    backgroundColor: '#f5f5f5',
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
      sandbox: false
    }
  });

  mainWindow.setMenuBarVisibility(false);
  mainWindow.loadFile(path.join(__dirname, 'renderer', 'index.html'));
}

app.whenReady().then(() => {
  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) createWindow();
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') app.quit();
});

// ---------- IPC：文件选择 ----------

ipcMain.handle('dialog:openFile', async () => {
  const win = BrowserWindow.getFocusedWindow() || mainWindow;
  const r = await dialog.showOpenDialog(win, {
    title: '选择 CAD 文件',
    filters: CAD_FILTERS,
    properties: ['openFile']
  });
  if (r.canceled || r.filePaths.length === 0) return null;
  return r.filePaths[0];
});

ipcMain.handle('dialog:saveFile', async () => {
  const win = BrowserWindow.getFocusedWindow() || mainWindow;
  const r = await dialog.showSaveDialog(win, {
    title: '保存 CAD 文件',
    defaultPath: 'merged.CAD',
    filters: [{ name: 'CAD 文件 (*.CAD)', extensions: ['cad'] }]
  });
  if (r.canceled || !r.filePath) return null;
  return r.filePath;
});

// ---------- IPC：执行处理任务 ----------

function runTask(taskType, params) {
  const t0 = Date.now();

  switch (taskType) {
    case 'overturnX':
      assertFile(params[0], '小板文件');
      requireParam(params[1], '输出文件');
      overturnX(params[0], params[1]);
      break;

    case 'overturnY':
      assertFile(params[0], '小板文件');
      requireParam(params[1], '输出文件');
      overturnY(params[0], params[1]);
      break;

    case 'rotate90':
      assertFile(params[0], '小板文件');
      requireParam(params[1], '输出文件');
      rotate90(params[0], params[1]);
      break;

    case 'move': {
      for (let i = 0; i < 4; i++) requireParam(params[i], ['X 偏移量', 'Y 偏移量', 'PIN 偏移量', '后缀名'][i]);
      assertFile(params[4], '小板文件');
      requireParam(params[5], '输出文件');
      move(params[0], params[1], params[2], params[3], params[4], params[5]);
      break;
    }

    case 'merge':
      assertFile(params[0], '小板文件');
      assertFile(params[1], '主板文件');
      requireParam(params[2], '输出文件');
      mergeCad(params[0], params[1], params[2]);
      break;

    default:
      throw new Error('未知任务类型：' + taskType);
  }

  return {
    ok: true,
    elapsedSec: ((Date.now() - t0) / 1000).toFixed(2)
  };
}

function requireParam(value, name) {
  if (value === undefined || value === null || value === '') {
    throw new Error(name + '参数不能为空');
  }
}

function assertFile(value, name) {
  requireParam(value, name);
  if (value === ':' || !fs.existsSync(value)) {
    throw new Error(name + '文件不存在：' + value);
  }
}

ipcMain.handle('task:run', (event, payload) => {
  try {
    return runTask(payload.type, payload.params);
  } catch (e) {
    return { ok: false, error: e.message, elapsedSec: '0' };
  }
});
