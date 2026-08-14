# Yingqi Tools

一个模块化的 Windows 个人小工具箱。当前包含：

- [Keyboard Cooldown Lock](https://github.com/Yingqi-Han/keyboard-cooldown-lock)：临时锁定键盘，保留鼠标恢复界面。
- [Lid Work Mode](https://github.com/Yingqi-Han/lid-work-mode)：当次工具箱会话内合盖继续运行，退出、崩溃或下次开机恢复原电源值。
- [Clipboard History](https://github.com/Yingqi-Han/clipboard-history)：从 Win+V 增量保存文本和图片，提供不会自动消失的完整页面与置顶小窗。

## v2 Fluent 界面

- `.NET 10 WPF` + `WPF UI 4.3.0` + `CommunityToolkit.Mvvm 8.4.2`。
- 原生 FluentWindow、Mica、NavigationView、系统主题色及 Segoe Fluent Icons。
- 概览、键盘锁、合盖继续运行、剪贴板历史和设置五个页面。
- 默认跟随 Windows 浅色/深色主题；只持久化主题和剪贴板小窗置顶偏好。
- `win-x64` self-contained 单文件发布，无需预装 .NET Desktop Runtime。

## Architecture

- 总仓库通过 Git submodule 锁定各组件版本。
- 组件保留独立仓库、独立构建和独立测试。
- Yingqi Tools 本体使用普通用户权限，不常驻托盘、不开机启动、不联网、不收集遥测。
- 剪贴板内容保存在安装目录的 `Data\ClipboardHistory`，使用 AES-GCM 加密，主密钥由当前用户 DPAPI 保护。
- 只有修改和恢复 Windows 电源计划的 PowerGuard 需要 UAC。

## Build

```powershell
git clone --recurse-submodules https://github.com/Yingqi-Han/yingqi-tools.git
cd yingqi-tools
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

需要 .NET 10 SDK；仓库通过 `global.json` 固定 10.0.101，并通过 `packages.lock.json` 锁定 NuGet 依赖。

## Install

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\install.ps1
```

默认安装到 `D:\Programs\YingqiTools`。也可以用 `-InstallDirectory` 指定其他位置：

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\install.ps1 -InstallDirectory 'D:\Programs\YingqiTools'
```

安装 PowerGuard 时需要一次 UAC。安装完成后桌面只保留 `Yingqi Tools` 快捷方式。

MIT licensed.
