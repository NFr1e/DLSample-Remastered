using DLSample.Shared;

namespace DLSample.Facility.Input
{
    /// <summary>
    /// 预定义输入层集合，包含 System、UI、Gameplay 三层
    /// </summary>
    public struct InputLayers
    {
        /// <summary>系统输入层，最低优先级，不阻断下层</summary>
        public struct SystemInputLayer : IInputLayer
        {
            public readonly string Name => "System";
            public readonly int Priority => DLSampleConsts.Input.INPUT_PRIORITY_SYSTEM;
            public readonly bool BlockLowerLayers => false;
        }

        /// <summary>UI 输入层，中优先级，阻断下层输入</summary>
        public struct UIInputLayer : IInputLayer
        {
            public readonly string Name => "UI";
            public readonly int Priority => DLSampleConsts.Input.INPUT_PRIORITY_UI;
            public readonly bool BlockLowerLayers => true;
        }

        /// <summary>Gameplay 输入层，最高优先级，阻断下层输入</summary>
        public struct GameplayInputLayer : IInputLayer
        {
            public readonly string Name => "Gameplay";
            public readonly int Priority => DLSampleConsts.Input.INPUT_PRIORITY_GAMEPLAY;
            public readonly bool BlockLowerLayers => true;
        }
    }
}

