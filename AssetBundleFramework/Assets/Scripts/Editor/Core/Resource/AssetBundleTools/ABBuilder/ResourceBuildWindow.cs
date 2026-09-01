/*
 * Description:             ResourceBuildWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/25
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// ResourceBuildWindow.cs
    /// 资源打包工具
    /// </summary>
    public class ResourceBuildWindow : EditorWindow
    {
        /// <summary>
        /// 操作类型
        /// </summary>
        public enum EOperationType
        {
            /// <summary>
            /// 资源打包窗口
            /// </summary>
            ResourceBuild = 1,
            /// <summary>
            /// 资源搜集窗口
            /// </summary>
            ResourceCollect,
            /// <summary>
            /// 热更打包窗口
            /// </summary>
            HotUpdateBuild,
        }

        /// <summary>
        /// 整体UI滚动位置
        /// </summary>
        private Vector2 mWindowUiScrollPos;

        /// <summary>
        /// 当前窗口操作类型
        /// </summary>
        private EOperationType CurrentOperationType = EOperationType.ResourceBuild;

        /// <summary>
        /// 窗口操作类型名字数组
        /// </summary>
        private string[] mOperationTypeNameArray;

        /// <summary>
        /// 操作面板
        /// </summary>
        private string[] mToolBarStrings = { "资源打包", "资源搜集", "热更打包" };

        /// <summary>
        /// 操作面板选择索引
        /// </summary>
        private int mToolBarSelectIndex;

        /// <summary>
        /// 上次打开的文件夹路径
        /// </summary>
        private string LastOpenFolderPath = "Assets/";

        [MenuItem("Tools/AssetBundle/资源打包窗口", priority = 200)]
        static void ShowWindow()
        {
            var resourceBuildWindow = EditorWindow.GetWindow<ResourceBuildWindow>(false, "资源打包窗口");
            resourceBuildWindow.Show();
        }

        protected void OnEnable()
        {
            LoadAllData();
        }

        protected void OnDisable()
        {
            SaveAllData();
        }

        protected void OnDestroy()
        {
            SaveAllData();
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        protected void LoadAllData()
        {
            mOperationTypeNameArray = Enum.GetNames(typeof(EOperationType));
            mToolBarSelectIndex = 0;
            CurrentOperationType = (EOperationType)Enum.Parse(typeof(EOperationType), mOperationTypeNameArray[mToolBarSelectIndex]);


            mFoldMap = new Dictionary<EFoldType, bool>();
            foreach (var foldType in Enum.GetValues(typeof(EFoldType)))
            {
                mFoldMap.Add((EFoldType)foldType, false);
            }

            InitCommonData();
            InitResourceData();
            InitHotUpdateData();
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        protected void SaveAllData()
        {
            SaveResourceData();
        }

        public void OnGUI()
        {
            mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
            GUILayout.BeginVertical();
            DisplayTagArea();
            if (CurrentOperationType == EOperationType.ResourceBuild)
            {
                DisplayResourceBuildArea();
            }
            else if (CurrentOperationType == EOperationType.ResourceCollect)
            {
                DisplayResourceCollectArea();
                DisplayBlackListArea();
                DisplayCommonArea();
            }
            else if(CurrentOperationType == EOperationType.HotUpdateBuild)
            {
                DisplayHotUpdateBuildArea();
            }
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// 显示标签部分
        /// </summary>
        private void DisplayTagArea()
        {
            GUILayout.BeginHorizontal();
            var pretollbarselectindex = mToolBarSelectIndex;
            mToolBarSelectIndex = GUILayout.Toolbar(mToolBarSelectIndex, mToolBarStrings, EditorStyles.toolbarButton, GUILayout.ExpandWidth(true));
            if (pretollbarselectindex != mToolBarSelectIndex)
            {
                CurrentOperationType = (EOperationType)Enum.Parse(typeof(EOperationType), mOperationTypeNameArray[mToolBarSelectIndex]);
            }
            GUILayout.EndHorizontal();
        }

        #region 公共部分
        /// <summary>
        /// 初始化公共数据
        /// </summary>
        private void InitCommonData()
        {
            
        }
        #endregion

        #region 资源打包部分
        /// <summary>
        /// 资源打包器
        /// </summary>
        private AssetBundleBuilder mAssetBundleBuilder;

        /// <summary>
        /// 是否展开设置
        /// </summary>
        private bool mShowSettingFoldout = true;

        /// <summary>
        /// 初始化资源数据
        /// </summary>
        private void InitResourceData()
        {
            Debug.Log($"ResourceBuildWindow:InitResourceData()");
            // 创建资源打包器
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            mAssetBundleBuilder = ResourceBuildTool.GetTargetAssetBundleBuilder(buildTarget);
        }

        /// <summary>
        /// 保存打包数据
        /// </summary>
        private void SaveResourceData()
        {

        }

        /// <summary>
        /// 显示打包区域
        /// </summary>
        private void DisplayResourceBuildArea()
        {
            // 标题
            EditorGUILayout.LabelField("Build setup");
            EditorGUILayout.Space();

            // 输出路径
            EditorGUILayout.LabelField("Build Output", mAssetBundleBuilder.AssetBundleBuildParams.BuildTargetOutputFolder);

            // 构建选项
            EditorGUILayout.Space();
            mAssetBundleBuilder.AssetBundleBuildParams.IsForceRebuild = GUILayout.Toggle(mAssetBundleBuilder.AssetBundleBuildParams.IsForceRebuild, "Froce Rebuild", GUILayout.MaxWidth(120));

            // 高级选项
            using (new EditorGUI.DisabledScope(false))
            {
                EditorGUILayout.Space();
                mShowSettingFoldout = EditorGUILayout.Foldout(mShowSettingFoldout, "Advanced Settings");
                if (mShowSettingFoldout)
                {
                    int indent = EditorGUI.indentLevel;
                    EditorGUI.indentLevel = 1;
                    mAssetBundleBuilder.AssetBundleBuildParams.CompressOption = (ABCompressOption)EditorGUILayout.EnumPopup("Compression", mAssetBundleBuilder.AssetBundleBuildParams.CompressOption);
                    mAssetBundleBuilder.AssetBundleBuildParams.IsAppendHash = EditorGUILayout.ToggleLeft("Append Hash", mAssetBundleBuilder.AssetBundleBuildParams.IsAppendHash, GUILayout.MaxWidth(120));
                    mAssetBundleBuilder.AssetBundleBuildParams.IsDisableWriteTypeTree = EditorGUILayout.ToggleLeft("Disable Write Type Tree", mAssetBundleBuilder.AssetBundleBuildParams.IsDisableWriteTypeTree, GUILayout.MaxWidth(200));
                    mAssetBundleBuilder.AssetBundleBuildParams.IsIgnoreTypeTreeChanges = EditorGUILayout.ToggleLeft("Ignore Type Tree Changes(SBP不支持)", mAssetBundleBuilder.AssetBundleBuildParams.IsIgnoreTypeTreeChanges, GUILayout.MaxWidth(300));
                    EditorGUI.indentLevel = indent;
                }
            }

            // 构建按钮
            EditorGUILayout.Space();
            if (GUILayout.Button("Build", GUILayout.MaxHeight(40)))
            {
                string title;
                string content;
                if (mAssetBundleBuilder.AssetBundleBuildParams.IsForceRebuild)
                {
                    title = "警告";
                    content = "确定开始强制构建吗，这样会删除所有已有构建的文件";
                }
                else
                {
                    title = "提示";
                    content = "确定开始增量构建吗";
                }
                if (EditorUtility.DisplayDialog(title, content, "Yes", "No"))
                {
                    // 清空控制台
                    EditorUtilities.ClearUnityConsole();
                    //var appVersion = float.Parse(Application.version);
                    //mAssetBundleBuilder.AssetBundleBuildParams.BuildVersion = Application.version;

                    // 存储配置
                    ResourceBuildTool.SaveSettingsToPlayerPrefs(mAssetBundleBuilder);

                    mAssetBundleBuilder.AssetBundleBuildParams.AssetBundleBuildPurpose = AssetBundleBuildPurpose.BuildPlayerBaseLine;
                    EditorApplication.delayCall += ExecuteBuild;
                }
                else
                {
                    Debug.LogWarning("[Build] 打包已经取消");
                }
            }
        }

        /// <summary>
        /// 执行构建
        /// </summary>
        private void ExecuteBuild()
        {
            mAssetBundleBuilder.AssetBundleBuildParams.PrintAllParams();
            var buildABResuilt = ResourceBuildTool.DoBuildAssetBundleByBuilder(mAssetBundleBuilder);
            if(!buildABResuilt)
            {
                Debug.LogError($"[Build] 资源打包失败!");
            }
            else
            {
                Debug.Log($"[Build] 资源打包成功!");
            }
        }
        #endregion

        #region 资源搜集部分
        /// <summary>
        /// 折叠类型
        /// </summary>
        public enum EFoldType
        {
            BuildRule = 1,          // 打包策略
            BlackList,              // 黑名单
            PostFixBlackList,       // 后缀名黑名单
            FileNameBlackList,      // 文件名黑名单
        }

        /// <summary>
        /// 重写EFoldType比较相关接口函数，避免EFoldType作为Dictionary Key时，
        /// 底层调用默认Equals(object obj)和DefaultCompare.GetHashCode()导致额外的堆内存分配
        /// 参考:
        /// http://gad.qq.com/program/translateview/7194373
        /// </summary>
        public class EFoldTypeComparer : IEqualityComparer<EFoldType>
        {
            public bool Equals(EFoldType x, EFoldType y)
            {
                return x == y;
            }

            public int GetHashCode(EFoldType x)
            {
                return (int)x;
            }
        }

        /// <summary>
        /// 折叠Map
        /// </summary>
        private Dictionary<EFoldType, bool> mFoldMap;

        /// <summary>
        /// 后缀名黑名单每行显示个数
        /// </summary>
        private const int POST_FIX_NUM_PER_ROW = 10;

        /// <summary>
        /// 文件名黑名单每行显示个数
        /// </summary>
        private const int FILE_NAME_NUM_PER_ROW = 5;

        /// <summary>
        /// 单行显示高度
        /// </summary>
        private const float SINGLE_LINE_DISPLAY_HEIGHT = 20f;

        /// <summary>
        /// 收集目录路径显示宽度
        /// </summary>
        private const float COLLECT_FOLDER_PATH_DISPLAY_WIDTH = 800f;

        /// <summary>
        /// 收集规则显示宽度
        /// </summary>
        private const float COLLECT_RULE_DISPLAY_WIDTH = 120f;

        /// <summary>
        /// 打包规则显示宽度
        /// </summary>
        private const float BUILD_RULE_DISPLAY_WIDTH = 150f;

        /// <summary>
        /// 固定AB名字显示宽度
        /// </summary>
        private const float CONST_NAME_DISPLAY_WIDTH = 120f;

        /// <summary>
        /// 压缩格式显示宽度
        /// </summary>
        private const float COMPRESSION_TYPE_DISPLAY_WIDTH = 120f;

        /// <summary>
        /// 是否允许从脚本加载宽度
        /// </summary>
        private const float ALLOW_LOAD_FROM_SCRIPT_WIDTH = 80f;

        /// <summary>
        /// 删除按钮显示宽度
        /// </summary>
        private const float DELETE_BUTTON_DISPLAY_WIDTH = 40f;

        /// <summary>
        /// 显示资源搜集区域
        /// </summary>
        private void DisplayResourceCollectArea()
        {
            EditorGUILayout.BeginVertical();
            mFoldMap[EFoldType.BuildRule] = EditorGUILayout.Foldout(mFoldMap[EFoldType.BuildRule], "AB打包资源搜集");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(AssetBundleBuildConstData.INDENTATION);
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("搜集目录路径", ResourceBuildStyles.ButtonMidStyle, GUILayout.Width(COLLECT_FOLDER_PATH_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            EditorGUILayout.LabelField("搜集策略", ResourceBuildStyles.ButtonMidStyle, GUILayout.Width(COLLECT_RULE_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            EditorGUILayout.LabelField("打包策略", ResourceBuildStyles.ButtonMidStyle, GUILayout.Width(BUILD_RULE_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            EditorGUILayout.LabelField("固定AB名", ResourceBuildStyles.ButtonMidStyle, GUILayout.Width(CONST_NAME_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            EditorGUILayout.LabelField("压缩格式", ResourceBuildStyles.ButtonMidStyle, GUILayout.Width(COMPRESSION_TYPE_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            EditorGUILayout.LabelField("代码加载", ResourceBuildStyles.ButtonMidStyle, GUILayout.Width(ALLOW_LOAD_FROM_SCRIPT_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            EditorGUILayout.LabelField("删除", ResourceBuildStyles.ButtonMidStyle, GUILayout.Width(DELETE_BUTTON_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            EditorGUILayout.EndHorizontal();
            if (!mFoldMap[EFoldType.BuildRule])
            {
                for (int i = 0; i < AssetBundleCollectSettingData.Setting.AssetBundleCollectors.Count; i++)
                {
                    DisplayOneCollect(AssetBundleCollectSettingData.Setting.AssetBundleCollectors[i]);
                }
                if (AssetBundleCollectSettingData.Setting.AssetBundleCollectors.Count == 0)
                {
                    EditorGUILayout.LabelField("无打包策略配置", GUILayout.ExpandWidth(true), GUILayout.Height(20f));
                }
                if (GUILayout.Button("+", GUILayout.ExpandWidth(true), GUILayout.Height(20.0f)))
                {
                    var chosenfolderpath = EditorUtility.OpenFolderPanel("选择搜集目录", LastOpenFolderPath, "");
                    if (string.IsNullOrEmpty(chosenfolderpath) == false && AssetBundleCollectSettingData.AddAssetBundleCollector(chosenfolderpath))
                    {
                        var relativefolderpath = PathUtilities.GetAssetsRelativeFolderPath(chosenfolderpath);
                        LastOpenFolderPath = relativefolderpath;
                        Debug.Log($"添加资源搜集目录:{chosenfolderpath}成功!");
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 显示单个搜集信息
        /// </summary>
        /// <param name="collector"></param>
        private void DisplayOneCollect(Collector collector)
        {
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField(collector.CollectFolderPath, GUILayout.Width(COLLECT_FOLDER_PATH_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            collector.CollectRule = (AssetBundleCollectRule)EditorGUILayout.EnumPopup(collector.CollectRule, GUILayout.Width(COLLECT_RULE_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            // 强制Igore规则的目录打包规则为Ignore
            if (collector.CollectRule == AssetBundleCollectRule.Ignore)
            {
                collector.BuildRule = AssetBundleBuildRule.Ignore;
            }
            collector.BuildRule = (AssetBundleBuildRule)EditorGUILayout.EnumPopup(collector.BuildRule, GUILayout.Width(BUILD_RULE_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            if (collector.BuildRule == AssetBundleBuildRule.ByConstName)
            {
                collector.ConstName = EditorGUILayout.TextField(collector.ConstName, GUILayout.Width(CONST_NAME_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            }
            else
            {
                collector.ConstName = string.Empty;
                EditorGUILayout.TextField(collector.ConstName, GUILayout.Width(CONST_NAME_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            }
            collector.Compression = (CompressionType)EditorGUILayout.EnumPopup(collector.Compression, GUILayout.Width(COMPRESSION_TYPE_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            collector.AllowLoadFromScript = EditorGUILayout.Toggle(collector.AllowLoadFromScript, GUILayout.Width(ALLOW_LOAD_FROM_SCRIPT_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT));
            if (GUILayout.Button("-", GUILayout.Width(DELETE_BUTTON_DISPLAY_WIDTH), GUILayout.Height(SINGLE_LINE_DISPLAY_HEIGHT)))
            {
                if (AssetBundleCollectSettingData.RemoveAssetBundleCollector(collector))
                {
                    Debug.Log($"移除资源搜集目录:{collector.CollectFolderPath}成功!");
                }
                else
                {
                    Debug.LogError($"移除资源搜集目录:{collector.CollectFolderPath}失败!");
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示黑名单区域
        /// </summary>
        private void DisplayBlackListArea()
        {
            EditorGUILayout.BeginVertical();
            mFoldMap[EFoldType.BlackList] = EditorGUILayout.Foldout(mFoldMap[EFoldType.BlackList], "AB打包黑名单");
            if (!mFoldMap[EFoldType.BlackList])
            {
                DisplayPostFixBlackListArea();
                DisplayFileNameBlackListArea();
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 显示后缀名黑名单区域
        /// </summary>
        private void DisplayPostFixBlackListArea()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(AssetBundleBuildConstData.INDENTATION);
            EditorGUILayout.BeginVertical();
            mFoldMap[EFoldType.PostFixBlackList] = EditorGUILayout.Foldout(mFoldMap[EFoldType.PostFixBlackList], "后缀名黑名单");
            if (!mFoldMap[EFoldType.PostFixBlackList])
            {
                if (AssetBundleCollectSettingData.Setting.BlackListInfo.PostFixBlackList.Count != 0)
                {
                    int mod;
                    for (int i = 0; i < AssetBundleCollectSettingData.Setting.BlackListInfo.PostFixBlackList.Count; i++)
                    {
                        mod = i % POST_FIX_NUM_PER_ROW;
                        if (mod == 0)
                        {
                            EditorGUILayout.BeginHorizontal("box");
                        }
                        DisplayOnePostFix(AssetBundleCollectSettingData.Setting.BlackListInfo.PostFixBlackList, i);
                        if (mod == (POST_FIX_NUM_PER_ROW - 1) || (i == AssetBundleCollectSettingData.Setting.BlackListInfo.PostFixBlackList.Count - 1))
                        {
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("无后缀名黑名单配置", GUILayout.ExpandWidth(true), GUILayout.Height(20f));
                }

                if (GUILayout.Button("+", GUILayout.ExpandWidth(true), GUILayout.Height(20f)))
                {
                    AssetBundleCollectSettingData.AddPostFixBlackList();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示单个后缀名黑名单
        /// </summary>
        /// <param name="postFixBlackList"></param>
        /// <param name="index"></param>
        private void DisplayOnePostFix(List<string> postFixBlackList, int index)
        {
            EditorGUI.BeginChangeCheck();
            postFixBlackList[index] = EditorGUILayout.TextField(postFixBlackList[index], GUILayout.Width(70f), GUILayout.Height(20f));
            if (EditorGUI.EndChangeCheck())
            {
                if (!postFixBlackList[index].StartsWith("."))
                {
                    Debug.LogWarning($"后缀名必须已.开头!");
                }
            }
            if (GUILayout.Button("-", GUILayout.Width(30.0f), GUILayout.Height(20.0f)))
            {
                var postFix = postFixBlackList[index];
                if (AssetBundleCollectSettingData.RemovePostFixBlackList(index))
                {
                    Debug.Log($"移除后缀名黑名单索引:{index}后缀名:{postFix}成功!");
                }
                else
                {
                    Debug.LogError($"移除后缀名黑名单索引:{index}后缀名:{postFix}失败!");
                }
            }
        }

        /// <summary>
        /// 显示文件名黑名单区域
        /// </summary>
        private void DisplayFileNameBlackListArea()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(AssetBundleBuildConstData.INDENTATION);
            EditorGUILayout.BeginVertical();
            mFoldMap[EFoldType.FileNameBlackList] = EditorGUILayout.Foldout(mFoldMap[EFoldType.FileNameBlackList], "文件名黑名单");
            if (!mFoldMap[EFoldType.FileNameBlackList])
            {
                if (AssetBundleCollectSettingData.Setting.BlackListInfo.FileNameBlackList.Count != 0)
                {
                    int mod;
                    for (int i = 0; i < AssetBundleCollectSettingData.Setting.BlackListInfo.FileNameBlackList.Count; i++)
                    {
                        mod = i % FILE_NAME_NUM_PER_ROW;
                        if (mod == 0)
                        {
                            EditorGUILayout.BeginHorizontal("box");
                        }
                        DisplayOneFileName(AssetBundleCollectSettingData.Setting.BlackListInfo.FileNameBlackList, i);
                        if (mod == (FILE_NAME_NUM_PER_ROW - 1) || (i == AssetBundleCollectSettingData.Setting.BlackListInfo.FileNameBlackList.Count - 1))
                        {
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("无文件名黑名单配置", GUILayout.ExpandWidth(true), GUILayout.Height(20f));
                }

                if (GUILayout.Button("+", GUILayout.ExpandWidth(true), GUILayout.Height(20f)))
                {
                    AssetBundleCollectSettingData.AddFileNameBlackList();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示单个名字黑名单
        /// </summary>
        /// <param name="fileNameBlackList"></param>
        /// <param name="index"></param>
        private void DisplayOneFileName(List<string> fileNameBlackList, int index)
        {
            EditorGUI.BeginChangeCheck();
            fileNameBlackList[index] = EditorGUILayout.TextField(fileNameBlackList[index], GUILayout.Width(140f), GUILayout.Height(20f));
            if (EditorGUI.EndChangeCheck())
            {
                if (fileNameBlackList[index].Equals(string.Empty))
                {
                    Debug.LogWarning($"文件名不能为空!");
                }
            }
            if (GUILayout.Button("-", GUILayout.Width(30.0f), GUILayout.Height(20.0f)))
            {
                var fileName = fileNameBlackList[index];
                if (AssetBundleCollectSettingData.RemoveFileNameBlackList(index))
                {
                    Debug.Log($"移除文件名黑名单索引:{index}后缀名:{fileName}成功!");
                }
                else
                {
                    Debug.LogError($"移除文件名黑名单索引:{index}后缀名:{fileName}失败!");
                }
            }
        }

        /// <summary>
        /// 显示公共区域
        /// </summary>
        private void DisplayCommonArea()
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("保存", GUILayout.ExpandWidth(true), GUILayout.Height(20.0f)))
            {
                AssetBundleCollectSettingData.SaveFile();
            }
            EditorGUILayout.EndVertical();
        }
        #endregion

        #region 热更打包部分
        /// <summary>
        /// 热更新版本号本地存储Key
        /// </summary>
        private const string HotUpdateVersionCodeKey = "HotUpdateVersionCode";

        /// <summary>
        /// 热更新资源版本号本地存储Key
        /// </summary>
        private const string HotUpdateResourceVersionCodeKey = "HotUpdateResourceVersionCode";

        /// <summary>
        /// 热更新版本号(热更新准备任务使用)
        /// </summary>
        private float mHotUpdateVersion;

        /// <summary>
        /// 热更新资源版本号(热更新准备任务使用)
        /// </summary>
        private int mHotUpdateResourceVersion;

        /// <summary>
        /// 热更新UI滚动位置
        /// </summary>
        private Vector2 mHotUpdateUiScrollPos;

        /// <summary>
        /// 包内版本配置信息
        /// </summary>
        private VersionConfig mInnerVersionConfig;

        /// <summary>
        /// 包外版本配置信息
        /// </summary>
        private VersionConfig mOutterVersionConfig;

        /// <summary>
        /// 初始化热更打包数据
        /// </summary>
        private void InitHotUpdateData()
        {
            Debug.Log($"ResourceBuildWindow:InitHotUpdateData()");
            InitVerisonConfigDatas();
            LoadHotUpdateSettingsFromPlayerPrefs(mAssetBundleBuilder);
        }

        /// <summary>
        /// 初始化版本配置信息数据
        /// </summary>
        private void InitVerisonConfigDatas()
        {
            mInnerVersionConfig = VersionUtilities.ReadInnerVersionConfig();
            mOutterVersionConfig = VersionUtilities.ReadOutterVersionConfig();
        }

        /// <summary>
        /// 存储热更新设置配置
        /// </summary>
        private void SaveHotUpdateSettingsFromPlayerPrefs(AssetBundleBuilder abBuilder)
        {
            var hotUpdateVersionCodeKey = ResourceBuildTool.GetProjectPlayerPrefKey(HotUpdateVersionCodeKey);
            PlayerPrefs.SetString(hotUpdateVersionCodeKey, mHotUpdateVersion.ToString());
            var hotUpdateResourceVersionCodeKey = ResourceBuildTool.GetProjectPlayerPrefKey(HotUpdateResourceVersionCodeKey);
            PlayerPrefs.SetInt(hotUpdateResourceVersionCodeKey, mHotUpdateResourceVersion);
        }

        /// <summary>
        /// 读取热更新设置配置
        /// </summary>
        private void LoadHotUpdateSettingsFromPlayerPrefs(AssetBundleBuilder abBuilder)
        {
            // 默认值以包外版本信息优先
            var gameVersionConfig = mOutterVersionConfig != null ? mOutterVersionConfig : mInnerVersionConfig;
            var hotUpdateVersionCodeKey = ResourceBuildTool.GetProjectPlayerPrefKey(HotUpdateVersionCodeKey);
            mHotUpdateVersion = PlayerPrefs.GetFloat(hotUpdateVersionCodeKey, (float)gameVersionConfig.VersionCode);
            var hotUpdateResourceVersionCodeKey = ResourceBuildTool.GetProjectPlayerPrefKey(HotUpdateResourceVersionCodeKey);
            mHotUpdateResourceVersion = PlayerPrefs.GetInt(hotUpdateResourceVersionCodeKey, gameVersionConfig.ResourceVersionCode);
        }

        /// <summary>
        /// 显示热更打包区域
        /// </summary>
        private void DisplayHotUpdateBuildArea()
        {
            mHotUpdateUiScrollPos = GUILayout.BeginScrollView(mHotUpdateUiScrollPos);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("包内版本号:", GUILayout.Width(100f));
            GUILayout.Label($"{mInnerVersionConfig.VersionCode}", "box", GUILayout.Width(120f));
            EditorGUILayout.LabelField("包内资源版本号:", GUILayout.Width(120f));
            GUILayout.Label($"{mInnerVersionConfig.ResourceVersionCode}", "box", GUILayout.Width(120f));
            GUILayout.EndHorizontal();
            if(mOutterVersionConfig != null)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("包外版本号:", GUILayout.Width(100f));
                GUILayout.Label($"{mOutterVersionConfig.VersionCode}", "box", GUILayout.Width(120f));
                EditorGUILayout.LabelField("包外资源版本号:", GUILayout.Width(120f));
                GUILayout.Label($"{mOutterVersionConfig.ResourceVersionCode}", "box", GUILayout.Width(120f));
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("热更新版本号:", GUILayout.Width(80f));
            mHotUpdateVersion = EditorGUILayout.FloatField(mHotUpdateVersion, GUILayout.Width(50.0f));
            // 限制版本号只保留1位小数
            mHotUpdateVersion = MathF.Truncate(mHotUpdateVersion * 100f) / 100f;
            EditorGUILayout.LabelField("热更新资源版本号:", GUILayout.Width(100f));
            mHotUpdateResourceVersion = EditorGUILayout.IntField(mHotUpdateResourceVersion, GUILayout.Width(100.0f));
            mHotUpdateResourceVersion = mHotUpdateResourceVersion > 0 ? mHotUpdateResourceVersion : 1;
            GUILayout.EndHorizontal();
            if (GUILayout.Button("执行热更打包", GUILayout.ExpandWidth(true)))
            {
                SaveHotUpdateSettingsFromPlayerPrefs(mAssetBundleBuilder);
                DoHotUpdateABBuildTask();
            }
            if(GUILayout.Button("清空包外热更资源", GUILayout.ExpandWidth(true)))
            {
                DoClearOutterHotUpdateResources();
            }
            DisplayNotice();
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
        }
        
        /// <summary>
        /// 执行热更打包任务
        /// </summary>
        private void DoHotUpdateABBuildTask()
        {
            mAssetBundleBuilder.AssetBundleBuildParams.VersionCode = mHotUpdateVersion;
            mAssetBundleBuilder.AssetBundleBuildParams.ResourceVersionCode = mHotUpdateResourceVersion;
            mAssetBundleBuilder.AssetBundleBuildParams.AssetBundleBuildPurpose = AssetBundleBuildPurpose.BuildHotUpdate;
            ExecuteBuild();
        }

        /// <summary>
        /// 执行清空包外热更资源(含热更资源，临时资源，包外资源版本文件等所有资源)
        /// </summary>
        private void DoClearOutterHotUpdateResources()
        {
            HotUpdateUtilities.DeleteAllOutterHotUpdateResources();
            InitVerisonConfigDatas();
            LoadHotUpdateSettingsFromPlayerPrefs(mAssetBundleBuilder);
        }

        /// <summary>
        /// 显示提示信息
        /// </summary>
        private void DisplayNotice()
        {
            GUILayout.Space(10);
            GUILayout.BeginVertical();
            GUI.color = Color.yellow;
            GUILayout.Label("注意事项:", "Box");
            GUILayout.Label($"1. 选择热更新打包的版本和资源版本号，注意热更资源版本号要大于包内资源版本号!", "Box");
            GUILayout.Label($"2. 版本号默认只允许保留最多2位小数!", "Box");
            GUI.color = Color.white;
            GUILayout.EndVertical();
        }
        #endregion
    }
}