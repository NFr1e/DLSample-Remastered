# DLSample

![Engine](https://img.shields.io/badge/Unity-2022-green?logo=unity&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows|macOS-yellow)
![License](https://img.shields.io/badge/License-MIT-blue)

这是一个基于 [Unity2022.3 LTS](https://unity.com/) 开发的，以游戏 [跳舞的线](https://www.cheetahgames.com/)为蓝本的项目，项目包含基础的 Gameplay 运行逻辑，也内置了关卡创建、节拍数据承载、路径生成与编辑辅助等配套工具，旨在形成一套从关卡数据配置到运行时驱动表现的完整样板。

## 一.开始使用

克隆仓库
```bash
git clone https://github.com/NFr1e/DLSample-Rematered.git
```

项目使用 [InputSystem](https://docs.unity3d.com/Manual/com.unity.inputsystem.html), [UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)等较新特性，不同版本可能出现API不兼容等情况。

- 最低版本 Unity 2019.4

- **推荐使用Unity2022.3 LTS**


## 二.项目内容

### 项目结构

```
DLSample/
├─ Levels/              # 关卡目录
├─ Resources/           # 资源
└─ Scripts/             # 项目代码
   ├─ Runtime/          # 运行时代码，负责应用入口、Gameplay 流程与基础设施
   ├─ Editor/           # 编辑器拓展
   └─ Shared/           # 共享的数据结构、常量等
```

### DLSample工作流

#### 节拍驱动的关卡组织方式
项目使用 `BeatmapData` 记录节拍时间点，再结合 `PathGrapherAsset` 生成和可视化关卡路径。

#### PathGrapher 路径可视化工具
`PathGrapher` 依据：
- 初始位置
- 初始速度
- 重力参数
- 方向信息
- Beatmap 节拍点
- 全局路径事件

推导出关卡路径中的 Waypoint 与 Segment，并在编辑阶段辅助设计者进行可视化调整。  
这使得节拍时间轴与角色移动路径之间建立了直观映射，能显著降低节奏关卡的制作成本。

#### 路径事件与 Gameplay 同步机制
PathGrapher 中配置的事件可以通过 `PathGrapherEventsSyncer` 同步到运行时 Gameplay 中，并注册到 `GameplayTimer` 的时间刻事件里。  
这意味着速度变化、重力变化、跳跃、传送等行为，都可以作为“时间事件”统一管理，具备较好的扩展性和一致性。

#### 一键创建关卡资源
项目提供了 LevelCreator 编辑器工具，可快速创建一整套关卡资源，包括：
- 场景文件
- `LevelData`
- `BeatmapData`
- `PathGrapherAsset`
- 关卡资源目录

这使得关卡的创建流程标准化，减少重复操作。

#### 模块化 Gameplay 生命周期管理
项目在运行时采用了基于 `IModule + ModulesManager` 的模块管理机制，将输入、计时器、角色控制、音频、结果结算等系统拆成独立模块，并通过优先级统一初始化与更新顺序。  
这种组织方式非常适合后续扩展新玩法或替换某个子系统。

#### 输入、UI、事件总线等基础设施齐备
项目已经搭建了简单的的基础层设施：
- `EventBus / AsyncEventBus`
- `InputSystem + InputManager`
- `UIElementManager`
- `ServiceLocator`
- 持久化、对象池等通用能力

## 三.文档✨
[DLSample文档站](https://nfr1e.github.io/docs/dl-sample/)