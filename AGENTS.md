# AGENTS.md — LittleBigMouse

## 交互要求
- Thinking 思考过程用中文表述
- Reply 回答也要用中文回复

## 项目概述
Windows 多显示器鼠标精度工具。.NET 8.0 (C#) + 原生 C++ 钩子 DLL。Avalonia 11 UI。仅限 Windows。

## 构建要求
需要安装 Visual Studio（含 C++/桌面开发工作负载）才能构建 `LittleBigMouse.Hook.vcxproj`。仅使用 `dotnet build` 无法完成构建。

### 构建命令
```bash
# 使用 MSBuild（通过开发者命令提示符）
msbuild LittleBigMouse.sln /t:Restore
msbuild LittleBigMouse.sln /p:Configuration=Debug

# 或使用 dotnet（仅限 C# 项目，Hook 项目会失败）
dotnet restore LittleBigMouse.sln
dotnet build LittleBigMouse.sln
```

## Git 子模块
克隆后必须初始化子模块 — `HLab.Core` 和 `HLab.Avalonia` 是子模块：
```bash
git submodule update --init --recursive
```

## 解决方案结构
```
LittleBigMouse.sln
├── LittleBigMouse.Core/
│   ├── LittleBigMouse.DisplayLayout/   ← 核心显示布局逻辑（类库）
│   └── LittleBigMouse.Zones/           ← 区域计算
├── LittleBigMouse.Hook/                ← C++ 原生鼠标钩子 DLL (vcxproj)
├── LittleBigMouse.Ui/
│   ├── LittleBigMouse.Ui.Avalonia/     ← 主 Avalonia GUI (WinExe) — 入口点
│   └── LittleBigMouse.Ui.Core/         ← 共享 UI 逻辑
├── LittleBigMouse.Ui.Loader/           ← 启动器：互斥锁单实例，然后启动 Ui.Avalonia
├── LittleBigMouse.Plugins/             ← 插件系统（Layout, Vcp, Core）
├── LittleBigMouse.Setup/               ← NSIS + InnoSetup 安装程序脚本
├── HLab.Core/                          ← 子模块：基础库（Mvvm, DI, Remote 等）
├── HLab.Avalonia/                      ← 子模块：Avalonia 封装（Base, Mvvm, Icons）
└── HLab.Sys/                           ← 系统级库（Windows 显示器, VCP, Argyll）
```

**入口点**：`LittleBigMouse.Ui.Loader` → 启动 `LittleBigMouse.Ui.Avalonia.exe`。

**关键依赖**：`LittleBigMouse.Ui.Avalonia` 对 `LittleBigMouse.Hook` 有 `ProjectDependency` — C++ DLL 必须先构建。

## 平台和配置
- **目标框架**：net8.0（C# `LangVersion=preview`）
- **平台**：x64、x86、AnyCPU
- **配置**：Debug、Release、DebugRelease
- CI 在 `windows-latest` 上构建 `Debug` 和 `Release`

## 测试
- **框架**：xunit
- **运行所有测试**：`dotnet test LittleBigMouse.sln`
- **运行特定测试项目**：`dotnet test LittleBigMouse.Core\LittleBigMouse.DisplayLayout.Tests\LittleBigMouse.DisplayLayout.Tests.csproj`
- **注意**：`LittleBigMouse.DisplayLayout.Tests` 目标为 net7.0 — 可能需要更新到 net8.0
- HLab 子模块包含自己的单元测试项目

## 关键依赖
- Avalonia 11.2.3（UI 框架）
- ReactiveUI（MVVM）
- Grace（DI 容器）
- H.Pipes（命名管道 IPC）
- TaskScheduler（Windows 任务计划程序集成）
- SixLabors.ImageSharp

## 注意事项
- **C++ Hook 项目**：需要 MSVC 构建工具。无法仅使用 `dotnet build` 构建。
- **子模块是必需的**：`HLab.Core/` 和 `HLab.Avalonia/` 必须检出，否则解决方案无法编译。
- **仅限 Windows**：使用 Win32 API、Windows 任务计划程序、命名管道、注册表。尽管使用 Avalonia，但不是跨平台的。
- **Loader 互斥锁**：`LittleBigMouse.Ui.Loader` 使用全局互斥锁（GUID `51B5711E-1A7F-436E-B3DD-B598901B3FD2`）强制单实例。只能运行一个实例。
- **备份 csproj 文件**：树中有多个 `*.csproj` 文件的 `Backup` 变体 — 忽略它们，它们不属于构建的一部分。
- **Avalonia 版本不匹配**：`HLab.Avalonia/Directory.Build.props` 设置 AvaloniaVersion=11.1.0，但主 UI 使用 11.2.3。子模块可能滞后。
- **无 CI 测试步骤**：GitHub Actions 工作流中 `dotnet test` 被注释掉了。

## 安装程序
`LittleBigMouse.Setup/` 包含 NSIS (`LittleBigMouse.nsi`) 和 InnoSetup (`LittleBigMouse.iss`) 脚本，用于构建 Windows 安装程序。