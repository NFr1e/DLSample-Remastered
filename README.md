# DLSample

这是一个基于 [Unity2022.3 LTS](https://unity.com/) 开发的，以游戏 [跳舞的线](https://www.cheetahgames.com/)为蓝本的项目，未来计划包含游戏Gameplay部分的完整游戏流程复刻实现。

## 一.环境配置💻

项目使用 [InputSystem](https://docs.unity3d.com/Manual/com.unity.inputsystem.html), [UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)等较新特性，不同版本可能出现API不兼容等情况。

- 最低版本 Unity 2019.4

- **推荐使用Unity2022.3 LTS**
  
#### 已知兼容问题
- ```LevelRestarter```中使用的```UnityEngine.SceneManagement```中的```SceneManager.loadedScenesCount```API在旧版本中不存在。请考虑替换为```SceneManager.sceneCount```。
- 在旧版本Unit中打开项目，有时部分碰撞器会发生大小与预期不一致的问题

### 使用的插件
- [UniTask](https://github.com/Cysharp/UniTask) 异步编程插件
- [DOTween](https://dotween.demigiant.com/) 补间动画插件

#### 付费插件 💵
- [Odin Inspector](https://odininspector.com/) 编辑器拓展

## 二.项目内容📜

### DLSample工作流
DLSample内置编辑器拓展，计划实现完整便捷的关卡设计工作流。

<!-- #### TrackGrapher (Rescripting)
- TrackGrapher是DLSample实现的音频可视化+踩音辅助工具，可以实现更方便且精确的关卡踩音工作，并将踩音结果保存到BeatmapData。 -->

#### PathGrapher
- PathGrapher是DLSample提供的通过BeatmapData传入时间数据源，并根据时间数据模拟出关卡标准路径的可视化工具。设计师可在路径时间点上增删改路径事件(PathEvent)实现路径变换等。路径事件包含点性事件和段性事件。点性事件如SpeedChangeEvent, GravityChangeEvent等，只需提供起点。段性事件如JumpEvent，需要提供起点和终点。
- PathGrapher上创建的事件可直接通过PathGrapherEventsSyncer同步到Gameplay。
- PathGrapher需要搭配OdinInspector插件使用

#### PathBuilder
- PathBuilder是DLSample提供的的通过读取PathGrapherAsset数据实现一键创建路径和引导线的工具

### 框架
 DLSample实现了一个简易的Gameplay生命周期管理框架。该框架将游戏中各模块抽象出```IModule```接口, 并由ModulesManager根据各模块的```Priority```值实现各模块的统一有序管理。模块管理器生命周期通过所在领域的Entry对象（如Gameplay -> GameplayEntry）桥接。
#### 模块注册和访问需要遵守以下开发规范:
1. 各模块实例需通过继承```GameplayObject```并覆写其```OnStart()```方法，在该方法中将模块实例注册到ModulesManager(使用```GameplayEntry.Instance.ModulesManager.Register<T>(IModule module)```)
2. 必须模块，即Gameplay必不可少的模块(如GameplayPlayerController)通过其构造函数或其他绑定方法直接注入依赖的模块实例
3. 可选模块，即非必须的模块(如CameraFollowerController)，通过实现接口```IModuleRequire<T>```,由ModulesManager通过反射自动注入模块。
4. 其余非模块杂项通过```GameplayEntry.Instance.ServiceLocator```的```Get<T>()```或```TryGet<T>(out service)```方法获取。

## 三.教程文档✨
### [一. 创建关卡](./DLSampleDoc/1_CreateLevel/README.md)

1. 手动创建
2. 快捷创建(TODO)

### [二. 关卡基础配置](./DLSampleDoc/2_LevelConfiguration/README.md)

1. LevelData配置
2. 如何填入BeatmapData？
3. PathGrapherAsset配置
4. 创建基础关卡

To be continued...

<!-- ### [三. Player设置]()

1. 关卡基础配置
2. Player移动
3. Player死亡 -->