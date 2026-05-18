namespace DLSample.Shared
{
    /// <summary>
    /// DLSample 全局常量定义
    /// </summary>
    public struct DLSampleConsts
    {
        /// <summary>
        /// 编辑器相关常量
        /// </summary>
        public struct Editor
        {
            #region MenuItem
            public const string MENU_ITEM_CREATE_LEVEL = "DLSample/CreateLevel";
            public const int MENU_ITEM_CREATE_LEVEL_PRIORITY = 1;

            public const string MENU_ITEM_PATH_BUILDER = "DLSample/PathBuilder";
            public const int MENU_ITEM_PATH_BUILDER_PRIORITY = 1;

            public const string MENU_ITEM_CHART_READER = "DLSample/ChartReader";
            public const int MENU_ITEM_CHART_READER_PRIORITY = 1;

            public const string MENU_ITEM_TUTORIAL = "DLSample/Tutorial";
            public const int MENU_ITEM_TUTORIAL_PRIORITY = 12;
            #endregion

            #region CreateMenu
            // Level
            public const string CREATE_MENU_LEVELDATA_MENU_NAME = "DLSample/Level/LevelData";
            public const string CREATE_MENU_LEVELDATA_FILE_NAME = "LevelData";
            public const int CREATE_MENU_LEVELDATA_ORDER = 1;

            public const string CREATE_MENU_BEATMAPDATA_MENU_NAME = "DLSample/Level/BeatmapData";
            public const string CREATE_MENU_BEATMAPDATA_FILE_NAME = "BeatmapData";
            public const int CREATE_MENU_BEATMAPDATA_ORDER = 2;

            public const string CREATE_MENU_PATHGRAPHERASSET_MENU_NAME = "DLSample/Level/PathGrapherAsset";
            public const string CREATE_MENU_PATHGRAPHERASSET_FILE_NAME = "PathGrapherAsset";
            public const int CREATE_MENU_PATHGRAPHERASSET_ORDER = 3;

            // Global
            public const string CREATE_MENU_PANELS_MENU_NAME = "DLSample/Config/UI/PanelsData";
            public const string CREATE_MENU_PANELS_FILE_NAME = "UIPanelsData";
            public const int CREATE_MENU_PANELS_ORDER = 1;

            public const string CREATE_MENU_SKINDATA_MENU_NAME = "DLSample/Config/SkinData";
            public const string CREATE_MENU_SKINDATA_FILE_NAME = "SkinData";
            public const int CREATE_MENU_SKINDATA_ORDER = 1;
            #endregion
        }

        /// <summary>
        /// 游戏玩法相关常量
        /// </summary>
        public struct Gameplay
        {
            #region Module Priority
            // 模块优先级，数值越小优先级越高
            public const int PRIORITY_BACKTRACKABLES_HANDLER = 0;

            public const int PRIORITY_PLAYER_CONTROLLER = 1;
            public const int PRIORITY_INPUT_HANDLER = 1;
            public const int PRIORITY_STATE_HANDLER = 1;
            public const int PRIORITY_CHECKPOINT_HANDLER = 1;
            public const int PRIORITY_SOUNDTRACK_DIRECTOR = 1;
            public const int PRIORITY_READINESS_COORDINATOR = 1;
            public const int PRIORITY_GAMEPLAY_TIMER = 1;
            public const int PRIORITY_TIMER_DIRECTOR = 1;
            public const int PRIORITY_UI_HANDLER = 1;
            public const int PRIORITY_RESULTER = 1;

            public const int PRIORITY_HINT_LINE_CONTROLLER = 2;
            public const int PRIORITY_SKIN_HANDLER = 2;
            public const int PRIORITY_SKIN_CHANGER = 2;
            public const int PRIORITY_AUTO_PLAY = 2;

            public const int PRIORITY_INITIALIZER = 10;

            public const int PRIORITY_STAIR_CONTROLLER = 11;
            #endregion

            #region BacktrackPriority
            // 回溯优先级，数值越小优先级越高
            public const int BACKTRACK_PRIORITY_TIMER = 0;
            public const int BACKTRACK_PRIORITY_PLAYER_CONTROLLER = 0;
            public const int BACKTRACK_PRIORITY_SOUNDTRACK_DIRECTOR = 0;
            public const int BACKTRACK_PRIORITY_TIMER_DIRECTOR = 0;

            public const int BACKTRACK_PRIORITY_HINT_LINE_MANAGER = 5;
            public const int BACKTRACK_PRIORITY_COLLECTABLE = 10;

            public const int BACKTRACK_PRIORITY_CAMERA_FOLLOWER = 10;

            public const int BACKTRACK_PRIORITY_SKIN_ADAPTER = 20;
            #endregion

            public const float HINT_BOX_TRIGGER_INTERVAL = 0.1f;
        }

        /// <summary>
        /// 存档与读档相关常量
        /// </summary>
        public struct SaveAndLoad
        {
            public const string ID_SKIN = "SAVE_SKIN_ID";
            public const string ID_HINTLINE_STATE = "SAVE_HINTLINE_STATE";
            public const string ID_SYNC_DELAY = "SAVE_SYNC_DELAY";
        }

        /// <summary>
        /// 输入系统相关常量
        /// </summary>
        public struct Input
        {
            // 输入优先级，数值越大优先级越高
            public const int INPUT_PRIORITY_SYSTEM = 20;
            public const int INPUT_PRIORITY_UI = 10;
            public const int INPUT_PRIORITY_GAMEPLAY = 0;
        }

        /// <summary>
        /// 其他常量
        /// </summary>
        public struct Others
        {
            public const string URL_TUTORIAL = "https://nfr1e.github.io/docs/dl-sample/";
        }
    }
}
