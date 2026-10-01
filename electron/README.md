# GenCAD 文件合并工具（Electron 版）

将原 C# WinForms 版 `GenCAD_Merge` 用 Electron 重新实现，功能与交互保持一致：
小板 X/Y 轴翻转、90 度旋转、坐标移动（可加名称后缀）、多板 CAD 合并。

## 目录结构

```
electron/
├─ package.json            # electron ^33
├─ .npmrc                  # 国内镜像配置
├─ main.js                 # 主进程：窗口、文件对话框、任务执行
├─ preload.js              # contextBridge 暴露 API
├─ renderer/               # 渲染层 UI（原生 JS + CSS）
│  ├─ index.html
│  ├─ style.css
│  └─ renderer.js
└─ lib/                    # 核心算法（由 C# Function/*.cs 逐行移植）
   ├─ genutil.js           # 正则匹配/分组（Regex 缓存）
   ├─ dutil.js             # 十进制精确运算（模拟 decimal 的 scale 保留）
   ├─ io.js                # 文件读取/逐行写出（UTF-8/BOM、\r\n）
   ├─ overturnX.js         # X 轴翻转
   ├─ overturnY.js         # Y 轴翻转
   ├─ rotate.js            # 90 度旋转
   ├─ moveCoordinate.js    # 坐标移动 + 名称后缀
   └─ merge.js             # 合并（子文件各段并入主板对应段）
```

## 运行

```bash
cd electron
npm install        # 自动使用国内镜像（见 .npmrc）
npm start
```

> 若 Electron 二进制未下载成功（国内网络），可手动执行：
> ```bash
> $env:ELECTRON_MIRROR="https://npmmirror.com/mirrors/electron/"
> node node_modules/electron/install.js
> ```

## 国内镜像

`.npmrc` 已配置：

```ini
registry=https://registry.npmmirror.com
electron_mirror=https://npmmirror.com/mirrors/electron/
```

## 功能对应

| 按钮 | 行为 | 说明 |
| ---- | ---- | ---- |
| X 轴翻转 | 取反 Y 坐标、交换 TOP/BOTTOM 等 | 输出 `原文件_X.CAD` |
| Y 轴翻转 | 取反 X 坐标、交换 TOP/BOTTOM 等 | 输出 `原文件_Y.CAD` |
| 旋转 90 度 | x'=-y, y'=x，旋转角 +90 | 输出 `原文件_R.CAD` |
| 位置移动 | 平移 X/Y/PIN，名称追加后缀 | 输出 `原文件_O.CAD` |
| 合并 CAD | 板二（小板）内容并入板一（主板） | 输出由“保存 CAD”指定 |

算法层为纯 Node 模块（不依赖 Electron），便于单元测试。
