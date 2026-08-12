# Yingqi Tools

一个模块化的 Windows 个人小工具箱。当前包含：

- [Keyboard Cooldown Lock](https://github.com/Yingqi-Han/keyboard-cooldown-lock)：临时锁定键盘，保留鼠标恢复界面。
- [Lid Work Mode](https://github.com/Yingqi-Han/lid-work-mode)：当次工具箱会话内合盖继续运行，退出、崩溃或下次开机恢复原电源值。

## Architecture

- 总仓库通过 Git submodule 锁定各组件版本。
- 组件保留独立仓库、独立构建和独立测试。
- Yingqi Tools 本体使用普通用户权限，不常驻托盘、不开机启动、不联网、不收集遥测。
- 只有修改和恢复 Windows 电源计划的 PowerGuard 需要 UAC。

## Build

```powershell
git clone --recurse-submodules https://github.com/Yingqi-Han/yingqi-tools.git
cd yingqi-tools
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\build.ps1
```

## Install

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\install.ps1
```

安装 PowerGuard 时需要一次 UAC。安装完成后桌面只保留 `Yingqi Tools` 快捷方式。

MIT licensed.
