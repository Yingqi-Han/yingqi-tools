# Yingqi Tools v2.0.0

Yingqi Tools v2 是一次完整的 WPF Fluent 重构。

## Highlights

- 全新 PowerToys 风格的 FluentWindow、Mica、NavigationView 和响应式页面。
- 新增概览页，以及跟随系统/浅色/深色主题设置。
- 键盘锁新增 Fluent 锁定窗口、倒计时、进度、延长 5 分钟和显眼的鼠标解锁入口。
- 合盖模式改用原生 ToggleSwitch、InfoBar 和异步启用/恢复流程。
- PowerGuard 的安全边界、固定命令、关闭/崩溃/切换计划/开机残留恢复逻辑保持兼容。
- 发布包为 Windows 10/11 x64 self-contained，无需预装 .NET 运行库。

## Safety

- 合盖模式每次启动默认关闭，不会跨重启保持启用。
- 未明确点击启用并确认 UAC 时，不会修改电源设置。
- 合盖运行时请始终将电脑放在通风、坚硬的表面。

## Package

发布包未签名，包含 `YingqiTools.exe`、独立 `PowerGuard.exe`、MIT License 和第三方许可证说明。
