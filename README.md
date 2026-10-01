# GenCAD 文件处理与拼板合并工具

用于处理 GenCAD（`.CAD`）格式 PCB 文件的桌面工具：支持小板 X/Y 轴翻转、90 度旋转、坐标移动（可加名称后缀），以及将小板 CAD 合并进主板 CAD，用于拼板场景。

仓库同时提供两套实现，功能与交互保持一致：

| 实现 | 目录 | 技术栈 | 状态 |
| ---- | ---- | ------ | ---- |
| WinForms 版 | 仓库根目录 | C# / .NET Framework 4.8 / WinForms | 原始版本 |
| Electron 版 | `electron/` | Electron 33 + 原生 JS | 由 C# 版逐行移植 |

## 功能说明

| 操作 | 行为 | 输出文件 |
| ---- | ---- | -------- |
| X 轴翻转 | Y 坐标取反，交换 TOP/BOTTOM 等层信息 | `原文件_X.CAD` |
| Y 轴翻转 | X 坐标取反，交换 TOP/BOTTOM 等层信息 | `原文件_Y.CAD` |
| 旋转 90 度 | `x' = -y`，`y' = x`，旋转角 +90 | `原文件_R.CAD` |
| 位置移动 | 平移 X/Y/PIN，名称追加指定后缀 | `原文件_O.CAD` |
| 合并 CAD | 板二（小板）各段内容并入板一（主板）对应段 | 由“保存 CAD”指定 |

合并时会处理的 GenCAD 段：`$BOARD`、`$PADS`、`$PADSTACKS`、`$SHAPES`、`$DEVICES`、`$COMPONENTS`、`$SIGNALS`、`$ROUTES`、`$TESTPINS`、`$POWERPINS`；其中 `THICKNESS` 行与 `ATTRIBUTE /GRA3/` 行会被跳过，避免与主板定义冲突。

## 目录结构

```
GenCAD_Merge/
├─ Form1.cs / Form1.Designer.cs   # 主界面
├─ Program.cs
├─ Function/                      # C# 核心算法
│  ├─ Overturn_x.cs               # X 轴翻转
│  ├─ Overturn_y.cs               # Y 轴翻转
│  ├─ Rotate.cs                   # 90 度旋转
│  ├─ Move_coordinate.cs          # 坐标移动 + 名称后缀
│  ├─ Merge.cs                    # 多板合并
│  ├─ proces.cs / Mproces.cs      # 通用处理流程
│  └─ Progress.cs                 # 进度窗口
├─ Properties/                    # 程序集信息与资源
├─ Resources/                     # 图标等资源
├─ merge_cad.csproj               # WinForms 工程（.NET Framework 4.8）
└─ electron/                      # Electron 版（见 electron/README.md）
   ├─ main.js / preload.js
   ├─ renderer/
   └─ lib/                        # 算法层（纯 Node 模块）
```

## 使用方式

### WinForms 版（Windows）

- 环境：Visual Studio（支持 .NET Framework 4.8 的版本）
- 打开 `merge_cad.csproj`，还原后以 Debug/Release 生成并运行
- 输出的可执行文件位于 `bin\Debug\` 或 `bin\Release\`

界面操作流程：

1. “选择小板” 载入待处理的 `.CAD` 文件
2. 按需点击 “X 轴翻转” / “Y 轴翻转” / “旋转 90 度”
3. “位置移动” 时填写 X、Y、PIN 偏移量与名称后缀
4. 合并时分别选择板一（主板）、板二（小板）与保存路径，点击 “合并 CAD”

### Electron 版（Windows / macOS / Linux）

```bash
cd electron
npm install     # .npmrc 已配置国内镜像
npm start
```

打包：`npm run dist`（electron-builder，产出 NSIS 安装包到 `electron/dist`）。

若 Electron 二进制下载失败，可手动执行：

```bash
$env:ELECTRON_MIRROR="https://npmmirror.com/mirrors/electron/"
node node_modules/electron/install.js
```

## 注意事项

- 处理前请备份原始 `.CAD` 文件，工具会直接写出新文件而不修改源文件
- 合并时板一为主板，板二为小板，顺序颠倒会导致坐标基准错误
- 数值计算使用十进制精确运算（C# `decimal` / Node 侧等效实现），避免浮点误差

## 许可

Electron 版工程声明为 MIT，WinForms 版沿用仓库原有授权。
