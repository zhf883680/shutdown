# Windows一键关机服务

## 简介

这个项目实现了一个Windows服务,可以通过外部发送指令,实现Windows系统的一键关机。

该服务使用C#在.NET Framework上开发,可以安装成为系统服务,在后台运行,接收外部请求执行关机操作。

## 设计目的

此项目设计的初衷是与HomeAssistant智能家居平台集成,让HomeAssistant可以通过调用该服务实现Windows设备的远程关机。

用户只需在HomeAssistant中简单配置,就可以实现语音控制或一键控制Windows设备开关机。

## 使用方法

1. 下载release中的安装包,运行安装程序  环境需求 .NET Framework上开发4.8  
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\installutil.exe ShutDown.exe
```
2. 在系统服务管理中启动WindowsShutdown服务
```
net start ShutdownService
```
3. 在HomeAssistant中配置WindowsShutdown服务的调用  
在 configuration.yaml   添加如下内容 mac,名称name自行修改  ip地址xxxxxxxx自行修改 delay=120 自行修改想要的时间
```
switch:
 - platform: wake_on_lan
   mac: 11:22:33:44:55:66
   name: xxxxxxxx
   turn_off:
     service: shell_command.turn_off_desktop
shell_command:
  turn_off_desktop: 'curl -X POST http://xxxxxxxx:8058/shutdown?delay=120 -d "" ' 

device_tracker:
  - platform: ping
    hosts:
      homewindows: xxxxxxxx
```
4. 通过HomeAssistant界面或终端发送调用指令,执行系统关机

## 开源信息

本项目使用MIT开源许可证,欢迎自由使用、共同维护与协作开发。

代码仓库地址:[https://github.com/zhf883680/shutdown](https://github.com/zhf883680/shutdown)

欢迎提出建议、报告bug或贡献代码!

