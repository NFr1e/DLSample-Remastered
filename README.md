# DLSample

![Engine](https://img.shields.io/badge/Unity-2022-green?logo=unity&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows|macOS|Linux-yellow)
![License](https://img.shields.io/badge/License-MIT-blue)

这是一个基于 [Unity2022.3 LTS](https://unity.com/) 开发的，以游戏 [跳舞的线](https://www.cheetahgames.com/)为蓝本的项目，项目包含基础的 Gameplay 运行逻辑，也内置了关卡创建、节拍数据承载、路径生成与编辑辅助等配套工具，旨在形成一套从关卡数据配置到运行时驱动表现的完整样板。

## 一.开始使用💻

克隆仓库
```bash
git clone https://github.com/NFr1e/DLSample-Rematered.git
```

项目使用 [InputSystem](https://docs.unity3d.com/Manual/com.unity.inputsystem.html), [UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)等较新特性，不同版本可能出现API不兼容等情况。

- 最低版本 Unity 2019.4

- **推荐使用Unity2022.3 LTS**
  
##### 已知兼容问题
- `LevelRestarter`中使用的`UnityEngine.SceneManagement`中的`SceneManager.loadedSceneCount`API在旧版本中未被加入。
- 在旧版本Unit中打开项目，有时部分碰撞器会发生大小与预期不一致的问题。

#### 使用的插件
- [UniTask](https://github.com/Cysharp/UniTask) 异步编程插件
- [DOTween](https://dotween.demigiant.com/) 补间动画插件
- [Odin Inspector](https://odininspector.com/) 编辑器拓展

#### 默认输入
- `Mouse Left / Space / Enter`：玩家输入
- `P / Esc`：暂停

## 二.项目内容📜

### 📁 项目结构

```
DLSample/
├─ Levels/              # 关卡目录
├─ Resources/           # 资源
└─ Scripts/             # 项目代码
   ├─ Runtime/          # 运行时代码，负责应用入口、Gameplay 流程与基础设施
   ├─ Editor/           # 编辑器拓展
   └─ Shared/           # 共享的数据结构、常量等
```

### ✨ DLSample工作流

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

### 基础
#### [一. 创建关卡](./DLSampleDoc/1_CreateLevel/README.md)

1. 手动创建
2. 快捷创建(TODO)

<!-- #### [二. 关卡基础配置](./DLSampleDoc/2_LevelConfiguration/README.md)

1. LevelData配置
2. 如何填入BeatmapData？
3. PathGrapherAsset配置
4. 创建基础关卡 -->
   
<!-- ### [三. Player设置]()

1. 关卡基础配置
2. Player移动
3. Player死亡 -->

To be continued...

### 进阶
#### 一. 代码规范

#### 模块注册和访问:
1. 各模块实例需通过继承`GameplayObject`并覆写其`OnInit()`，`OnStart()`等方法，在`OnInit()`方法中创建模块和注册到`ServiceLocator`，在`OnStart()`将模块实例注册到ModulesManager(使用`GameplayEntry.Instance.ModulesManager.Register<T>(IModule module)`)
2. 必须模块，即Gameplay必不可少的模块(如GameplayPlayerController)通过其构造函数或其他绑定方法直接注入依赖的模块实例
3. 可选模块，即非必须的模块(如CameraFollowerController)，通过实现接口```IModuleRequire<T>```,由ModulesManager通过反射自动注入模块。
4. 其余杂项通过```GameplayEntry.Instance.ServiceLocator```的```Get<T>()```或```TryGet<T>(out service)```方法获取。