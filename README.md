# MDC

MDC（Music Desktop Controller）是一个基于 WinForms 和 Windows SMTC（System Media Transport Controls）的网易云音乐桌面控制器。它提供一个轻量、置顶、美观的小窗口，用来快速查看当前播放信息并控制播放。

## 功能

- 通过 Windows SMTC 获取网易云音乐的播放状态、歌曲名、歌手、专辑封面和进度信息。
- 支持播放/暂停、上一首、下一首控制。
- 窗口始终置顶，适合放在桌面角落当迷你播放器使用。
- 支持黑色、白色背景切换，也可以跟随系统主题。
- 显示歌曲进度、已播放时间和总时长。
- 支持点击或拖动进度条尝试跳转播放位置。

## 说明

进度跳转依赖播放器是否向 Windows SMTC 开放时间轴控制能力。网易云音乐部分版本可能只允许展示进度，不允许外部程序跳转，此时 MDC 会给出“不支持进度跳转”的提示。

## 运行环境

- Windows 10 1809 或更高版本
- .NET 8 Desktop Runtime 或 .NET 8 SDK
- 已安装并正在播放的网易云音乐客户端

## 构建和运行

```powershell
dotnet build .\MDC.sln
```

构建完成后可运行：

```powershell
.\MDC\bin\Debug\net8.0-windows10.0.19041.0\MDC.exe
```

## 技术栈

- C# / WinForms
- .NET 8
- Windows.Media.Control SMTC API
- 自定义 WinForms UI 控件
