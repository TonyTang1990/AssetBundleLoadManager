/*
 * Description:             NewBuildWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/25
 */

using MotionFramework.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// NewBuildWindow.cs
/// 新版打包工具(整合Android和IOS打包,资源打包以及热更新功能)
/// </summary>
public class NewBuildWindow : BaseEditorWindow
{
    /// <summary>
    /// 操作类型
    /// </summary>
    public enum EOperationType
    {
        Build = 1,                  // 打包窗口
        ResourceBuild,              // 资源打包窗口
        ResourceCollect,            // 资源搜集窗口
        HotUpdate,                  // 热更新窗口
    }

    /// <summary>
    /// 整体UI滚动位置
    /// </summary>
    private Vector2 mWindowUiScrollPos;

    /// <summary>
    /// 当前窗口操作类型
    /// </summary>
    private EOperationType CurrentOperationType = EOperationType.Build;

    /// <summary>
    /// 窗口操作类型名字数组
    /// </summary>
    private string[] mOperationTypeNameArray;

    /// <summary>
    /// 操作面板
    /// </summary>
    private string[] mToolBarStrings = { "版本打包", "资源打包", "资源搜集", "热更新" };

    /// <summary>
    /// 操作面板选择索引
    /// </summary>
    private int mToolBarSelectIndex;

    /// <summary>
    /// 上次打开的文件夹路径
    /// </summary>
    private string LastOpenFolderPath = "Assets/";

    /// <summary>
    /// 项目路径Hash值(用于使得PlayerPrefs存储的Key值唯一)
    /// </summary>
    private int mProjectPathHashValue;

    [MenuItem("Tools/New AssetBundle/打包窗口", priority = 200)]
    static void ShowWindow()
    {
        var newBuildWindow = EditorWindow.GetWindow<NewBuildWindow>(false, "打包窗口");
        newBuildWindow.Show();
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    protected override void InitData()
    {
        base.InitData();
        mOperationTypeNameArray = Enum.GetNames(typeof(EOperationType));
        mToolBarSelectIndex = 0;
        CurrentOperationType = (EOperationType)Enum.Parse(typeof(EOperationType), mOperationTypeNameArray[mToolBarSelectIndex]);

        #region 打包部分
        InitBuildData();
        #endregion

        #region 资源打包部分
        InitResourceData();
        #endregion

        #region 热更新部分
        InitHotUpdateData();
        #endregion
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    protected override void SaveData()
    {
        base.SaveData();
        #region 打包部分
        SaveBuildData();
        #endregion

        #region 资源打包部分
        SaveResourceData();
        #endregion

        #region 热更新部分
        SaveHotUpdateData();
        #endregion
    }

    public void OnGUI()
    {
        mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
        GUILayout.BeginVertical();
        DisplayTagArea();
        if (CurrentOperationType == EOperationType.Build)
        {
            DisplayBuildArea();
        }
        else if (CurrentOperationType == EOperationType.ResourceBuild)
        {
            DisplayResourceBuildArea();
        }
        else if (CurrentOperationType == EOperationType.ResourceCollect)
        {
            DisplayResourceCollectArea();
        }
        else if (CurrentOperationType == EOperationType.HotUpdate)
        {
            DisplayHotUpdateArea();
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
        if(pretollbarselectindex != mToolBarSelectIndex)
        {
            CurrentOperationType = (EOperationType)Enum.Parse(typeof(EOperationType), mOperationTypeNameArray[mToolBarSelectIndex]);
        }
        GUILayout.EndHorizontal();
    }

    #region 公共部分
    /// <summary>
    /// 显示包内版本和资源版本信息区域
    /// </summary>
    private void DisplayInnerVersionAndResourceVersionInfoArea()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("包内版本号:", GUILayout.Width(100f));
        GUILayout.Label($"{VersionConfigModuleManager.Singleton.InnerGameVersionConfig.VersionCode}", "box", GUILayout.Width(100f));
        EditorGUILayout.LabelField("包内资源版本号:", GUILayout.Width(100f));
        GUILayout.Label($"{VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode}", "box", GUILayout.Width(100f));
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 显示打包信息区域
    /// </summary>
    private void DisplayBuildInfoArea()
    {
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("打包版本号:", GUILayout.Width(100f));
        BuildVersion = EditorGUILayout.TextField(BuildVersion, GUILayout.Width(100f));
        BuildVersion = string.IsNullOrEmpty(BuildVersion) ? "1.0" : BuildVersion;
        EditorGUILayout.LabelField("打包资源版本号:", GUILayout.Width(100f));
        BuildResourceVersion = EditorGUILayout.IntField(BuildResourceVersion, GUILayout.Width(100f));
        BuildResourceVersion = BuildResourceVersion > 0 ? BuildResourceVersion : 1;
        GUILayout.EndHorizontal();
    }
    #endregion

    #region 打包部分
    #region 存储相关Key
    /// <summary>
    /// 打包版本号Key
    /// </summary>
    private const string BuildVersionKey = "BuildVersionKey";

    /// <summary>
    /// 打包资源版本号Key
    /// </summary>
    private const string BuildResourceVersionKey = "BuildResourceVersionKey";

    /// <summary>
    /// 打包平台Key
    /// </summary>
    private const string BuildTargetKey = "BuildTargetKey";

    /// <summary>
    /// 打包输出路径存储Key
    /// </summary>
    private const string BuildOutputPathKey = "BuildOutputPathKey";
    #endregion

    /// <summary>
    /// 打包版本号
    /// </summary>
    public string BuildVersion
    {
        get;
        private set;
    }

    /// <summary>
    /// 打包资源版本号
    /// </summary>
    public int BuildResourceVersion
    {
        get;
        private set;
    }

    /// <summary>
    /// 打包平台
    /// </summary>
    public BuildTarget BuildTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// 打包输出路径
    /// </summary>
    public string BuildOutputPath
    {
        get;
        private set;
    }

    /// <summary>
    /// 初始化打包数据
    /// </summary>
    private void InitBuildData()
    {
        Debug.Log("NewBuildWindow:InitBuildData()");
        mProjectPathHashValue = Application.dataPath.GetHashCode();
        BuildVersion = PlayerPrefs.GetString($"{mProjectPathHashValue}_{BuildVersionKey}");
        BuildResourceVersion = PlayerPrefs.GetInt($"{mProjectPathHashValue}_{BuildResourceVersionKey}");
        BuildTarget = (BuildTarget)PlayerPrefs.GetInt($"{mProjectPathHashValue}_{BuildTargetKey}", (int)EditorUserBuildSettings.activeBuildTarget);
        BuildOutputPath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{BuildOutputPathKey}");
        Debug.Log("打包窗口读取配置:");
        Debug.Log("版本号设置:" + BuildVersion);
        Debug.Log("资源版本号设置:" + BuildResourceVersion);
        Debug.Log("打包平台:" + Enum.GetName(typeof(BuildTarget), BuildTarget));
        Debug.Log("打包输出路径:" + BuildOutputPath);
    }

    /// <summary>
    /// 保存打包数据
    /// </summary>
    private void SaveBuildData()
    {
        Debug.Log("NewBuildWindow:SaveBuildData()");
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{BuildVersionKey}", BuildVersion);
        PlayerPrefs.SetInt($"{mProjectPathHashValue}_{BuildResourceVersionKey}", BuildResourceVersion);
        PlayerPrefs.SetInt($"{mProjectPathHashValue}_{BuildTargetKey}", (int)BuildTarget);
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{BuildOutputPathKey}", BuildOutputPath);
        Debug.Log("打包窗口保存配置:");
        Debug.Log("版本号设置:" + BuildVersion);
        Debug.Log("资源版本号设置:" + BuildResourceVersion);
        Debug.Log("打包平台:" + Enum.GetName(typeof(BuildTarget), BuildTarget));
        Debug.Log("打包输出路径:" + BuildOutputPath);
    }

    /// <summary>
    /// 显示打包部分
    /// </summary>
    private void DisplayBuildArea()
    {
        GUILayout.BeginVertical();
        DisplayInnerVersionAndResourceVersionInfoArea();
        DisplayBuildInfoArea();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("打包平台:", GUILayout.Width(100f));
        BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(BuildTarget, GUILayout.Width(100f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("打包输出目录:", GUILayout.Width(100f));
        BuildOutputPath = EditorGUILayout.TextField("", BuildOutputPath);
        if (GUILayout.Button("选择打包输出目录", GUILayout.Width(200f)))
        {
            BuildOutputPath = EditorUtility.OpenFolderPanel("打包输出目录", "请选择打包输出目录!", "");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("打包", GUILayout.ExpandWidth(true)))
        {
            DoBuild();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }


    /// <summary>
    /// 执行打包
    /// </summary>
    private void DoBuild()
    {
        Debug.Log("DoBuild()");
        if (!string.IsNullOrEmpty(BuildOutputPath))
        {
            if (!Directory.Exists(BuildOutputPath))
            {
                Directory.CreateDirectory(BuildOutputPath);
            }
            var buildtargetgroup = GetCorrespondingBuildTaregtGroup(BuildTarget);
            Debug.Log($"打包分组:{Enum.GetName(typeof(BuildTargetGroup), buildtargetgroup)}");
            if (buildtargetgroup != BuildTargetGroup.Unknown)
            {
                double newversioncode;
                if (double.TryParse(BuildVersion, out newversioncode))
                {
                    VersionConfigModuleManager.Singleton.initVerisonConfigData();
                    var innerversioncode = VersionConfigModuleManager.Singleton.InnerGameVersionConfig.VersionCode;
                    var innerresourceversioncode = VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode;
                    Debug.Log("打包版本信息:");
                    Debug.Log($"版本号:{newversioncode} 资源版本号:{BuildResourceVersion}");
                    Debug.Log($"包内VersionConfig信息:");
                    Debug.Log($"版本号:{innerversioncode} 资源版本号:{innerresourceversioncode}");
                    var prebundleversion = PlayerSettings.bundleVersion;
                    PlayerSettings.bundleVersion = newversioncode.ToString();
                    Debug.Log($"打包修改版本号从:{prebundleversion}到{PlayerSettings.bundleVersion}");
                    Debug.Log($"打包修改VersionConfig从:Version:{innerversioncode}到{newversioncode} ResourceVersion:{innerresourceversioncode}到{BuildResourceVersion}");
                    VersionConfigModuleManager.Singleton.saveNewVersionCodeInnerConfig(newversioncode);
                    VersionConfigModuleManager.Singleton.saveNewResoueceCodeInnerConfig(BuildResourceVersion);
                    BuildPlayerOptions buildplayeroptions = new BuildPlayerOptions();
                    buildplayeroptions.locationPathName = BuildOutputPath + Path.DirectorySeparatorChar + PlayerSettings.productName + GetCorrespondingBuildFilePostfix(BuildTarget);
                    buildplayeroptions.scenes = GetBuildSceneArray();
                    buildplayeroptions.target = BuildTarget;
                    Debug.Log($"打包平台:{Enum.GetName(typeof(BuildTarget), BuildTarget)}");
                    Debug.Log($"打包输出路径:{buildplayeroptions.locationPathName}");
                    buildplayeroptions.targetGroup = buildtargetgroup;
                    EditorUserBuildSettings.SwitchActiveBuildTarget(buildtargetgroup, BuildTarget);
                    BuildPipeline.BuildPlayer(buildplayeroptions);
                }
                else
                {
                    Debug.LogError($"输入的版本号:{BuildVersion}无效,打包失败!");
                }
            }
            else
            {
                Debug.LogError("不支持的打包平台选择,打包失败!");
            }
        }
        else
        {
            Debug.LogError("打包输出目录为空或不存在,打包失败!");
        }
    }

    /// <summary>
    /// 获取需要打包的场景数组
    /// </summary>
    /// <returns></returns>
    private string[] GetBuildSceneArray()
    {
        //暂时默认BuildSetting里设置的场景才是要进包的场景
        List<string> editorscenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;
            editorscenes.Add(scene.path);
            Debug.Log($"需要打包的场景:{scene.path}");
        }
        return editorscenes.ToArray();
    }

    /// <summary>
    /// 获取对应的打包分组
    /// </summary>
    /// <param name="buildtarget"></param>
    /// <returns></returns>
    private BuildTargetGroup GetCorrespondingBuildTaregtGroup(BuildTarget buildtarget)
    {
        switch (buildtarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return BuildTargetGroup.Standalone;
            case BuildTarget.Android:
                return BuildTargetGroup.Android;
            case BuildTarget.iOS:
                return BuildTargetGroup.iOS;
            default:
                return BuildTargetGroup.Unknown;
        }
    }

    /// <summary>
    /// 获取对应的打包分组的打包文件后缀
    /// </summary>
    /// <param name="buildtarget"></param>
    /// <returns></returns>
    private string GetCorrespondingBuildFilePostfix(BuildTarget buildtarget)
    {
        switch (buildtarget)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return ".exe";
            case BuildTarget.Android:
                return ".apk";
            case BuildTarget.iOS:
                return "";
            default:
                return "";
        }
    }
    #endregion

    #region 资源打包部分
    /// <summary>
    /// 压缩格式设置本地存储Key
    /// </summary>
    private const string ABBuildSettingCompressOptionKey = "ABBuildSettingCompressOption";

    /// <summary>
    /// 是否强制重新打包设置本地存储Key
    /// </summary>
    private const string ABBuildSettingIsForceRebuildKey = "ABBuildSettingIsForceRebuild";

    /// <summary>
    /// 是否AppendHash设置本地存储Key
    /// </summary>
    private const string ABBuildSettingIsAppendHashKey = "ABBuildSettingIsAppendHash";

    /// <summary>
    /// 是否Disable Write Type Tree设置本地存储Key
    /// </summary>
    private const string ABBuildSettingIsDisableWriteTypeTreeKey = "ABBuildSettingIsDisableWriteTypeTree";

    /// <summary>
    /// 是否Ignore Type Tree Change设置本地存储Key
    /// </summary>
    private const string ABBuildSettingIsIgnoreTypeTreeChangesKey = "ABBuildSettingIsIgnoreTypeTreeChanges";

    /// <summary>
    /// 是否受用PlayerSettingVersion设置本地存储Key
    /// </summary>
    private const string ABBuildSettingIsUsePlayerSettingVersionKey = "ABBuildSettingIsUsePlayerSettingVersion";

    /// <summary>
    /// 初始化资源数据
    /// </summary>
    private void InitResourceData()
    {
        Debug.Log($"NewBuildWindow:InitResourceData()");
        // 创建资源打包器
        var appVersion = new Version(Application.version);
        var buildVersion = appVersion.Revision;
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        mAssetBuilder = new AssetBundleBuilder(buildTarget, buildVersion);

        // 读取配置
        LoadSettingsFromPlayerPrefs(mAssetBuilder);
    }

    /// <summary>
    /// 保存打包数据
    /// </summary>
    private void SaveResourceData()
    {

    }

    /// <summary>
    /// 存储配置
    /// </summary>
    private static void SaveSettingsToPlayerPrefs(AssetBundleBuilder builder)
    {
        PlayerPrefs.SetString(ABBuildSettingCompressOptionKey, builder.CompressOption.ToString());
        PlayerPrefs.SetInt(ABBuildSettingIsForceRebuildKey, builder.IsForceRebuild ? 1 : 0);
        PlayerPrefs.SetInt(ABBuildSettingIsAppendHashKey, builder.IsAppendHash ? 1 : 0);
        PlayerPrefs.SetInt(ABBuildSettingIsDisableWriteTypeTreeKey, builder.IsDisableWriteTypeTree ? 1 : 0);
        PlayerPrefs.SetInt(ABBuildSettingIsIgnoreTypeTreeChangesKey, builder.IsIgnoreTypeTreeChanges ? 1 : 0);
    }

    /// <summary>
    /// 读取配置
    /// </summary>
    private static void LoadSettingsFromPlayerPrefs(AssetBundleBuilder builder)
    {
        builder.CompressOption = (AssetBundleBuilder.ECompressOption)Enum.Parse(typeof(AssetBundleBuilder.ECompressOption), PlayerPrefs.GetString(ABBuildSettingCompressOptionKey, AssetBundleBuilder.ECompressOption.Uncompressed.ToString()));
        builder.IsForceRebuild = PlayerPrefs.GetInt(ABBuildSettingIsForceRebuildKey, 0) != 0;
        builder.IsAppendHash = PlayerPrefs.GetInt(ABBuildSettingIsAppendHashKey, 0) != 0;
        builder.IsDisableWriteTypeTree = PlayerPrefs.GetInt(ABBuildSettingIsDisableWriteTypeTreeKey, 0) != 0;
        builder.IsIgnoreTypeTreeChanges = PlayerPrefs.GetInt(ABBuildSettingIsIgnoreTypeTreeChangesKey, 0) != 0;
    }

    /// <summary>
    /// 构建器
    /// </summary>
    private AssetBundleBuilder mAssetBuilder = null;

    /// <summary>
    /// 是否展开设置
    /// </summary>
    private bool mShowSettingFoldout = true;

    /// <summary>
    /// 显示打包区域
    /// </summary>
    private void DisplayResourceBuildArea()
    {
        // 标题
        EditorGUILayout.LabelField("Build setup");
        EditorGUILayout.Space();

        DisplayBuildInfoArea();
        // 构建版本
        //mAssetBuilder.BuildVersion = EditorGUILayout.IntField("Build Version", mAssetBuilder.BuildVersion, GUILayout.MaxWidth(250));

        // 输出路径
        EditorGUILayout.LabelField("Build Output", mAssetBuilder.OutputDirectory);

        // 构建选项
        EditorGUILayout.Space();
        mAssetBuilder.IsForceRebuild = GUILayout.Toggle(mAssetBuilder.IsForceRebuild, "Froce Rebuild", GUILayout.MaxWidth(120));

        // 高级选项
        using (new EditorGUI.DisabledScope(false))
        {
            EditorGUILayout.Space();
            mShowSettingFoldout = EditorGUILayout.Foldout(mShowSettingFoldout, "Advanced Settings");
            if (mShowSettingFoldout)
            {
                int indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 1;
                mAssetBuilder.CompressOption = (AssetBundleBuilder.ECompressOption)EditorGUILayout.EnumPopup("Compression", mAssetBuilder.CompressOption);
                mAssetBuilder.IsAppendHash = EditorGUILayout.ToggleLeft("Append Hash", mAssetBuilder.IsAppendHash, GUILayout.MaxWidth(120));
                mAssetBuilder.IsDisableWriteTypeTree = EditorGUILayout.ToggleLeft("Disable Write Type Tree", mAssetBuilder.IsDisableWriteTypeTree, GUILayout.MaxWidth(200));
                mAssetBuilder.IsIgnoreTypeTreeChanges = EditorGUILayout.ToggleLeft("Ignore Type Tree Changes", mAssetBuilder.IsIgnoreTypeTreeChanges, GUILayout.MaxWidth(200));
                EditorGUI.indentLevel = indent;
            }
        }

        // 构建按钮
        EditorGUILayout.Space();
        if (GUILayout.Button("Build", GUILayout.MaxHeight(40)))
        {
            string title;
            string content;
            if (mAssetBuilder.IsForceRebuild)
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

                // 存储配置
                SaveSettingsToPlayerPrefs(mAssetBuilder);

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
        var timecounter = new TimeCounter();
        timecounter.Start("AssetBundleBuild");
        mAssetBuilder.PreAssetBuild();
        mAssetBuilder.PostAssetBuild();
        timecounter.End();
    }
    #endregion

    #region 资源搜集部分
    /// <summary>
    /// 显示资源搜集区域
    /// </summary>
    private void DisplayResourceCollectArea()
    {
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("AB打包资源搜集:", GUILayout.ExpandWidth(true), GUILayout.Height(20.0f));
        for (int i = 0; i < AssetBundleCollectSettingData.Setting.AssetBundleCollectors.Count; i++)
        {
            DisplayOneCollect(AssetBundleCollectSettingData.Setting.AssetBundleCollectors[i]);
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
        if (GUILayout.Button("保存", GUILayout.ExpandWidth(true), GUILayout.Height(20.0f)))
        {
            AssetBundleCollectSettingData.SaveFile();
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示单个搜集信息
    /// </summary>
    /// <param name="collector"></param>
    private void DisplayOneCollect(Collector collector)
    {
        GUILayout.BeginHorizontal("Box");
        EditorGUILayout.LabelField(collector.CollectFolderPath, GUILayout.ExpandWidth(true), GUILayout.Height(20.0f));
        collector.CollectRule = (EAssetBundleCollectRule)EditorGUILayout.EnumPopup(collector.CollectRule, GUILayout.Width(120.0f), GUILayout.Height(20.0f));
        collector.BuildRule = (EAssetBundleBuildRule)EditorGUILayout.EnumPopup(collector.BuildRule, GUILayout.Width(120.0f), GUILayout.Height(20.0f));
        // 强制Igore规则的目录打包规则为Ignore
        if (collector.CollectRule == EAssetBundleCollectRule.Ignore)
        {
            collector.BuildRule = EAssetBundleBuildRule.Ignore;
        }
        if(collector.BuildRule == EAssetBundleBuildRule.LoadByConstName)
        {
            collector.ConstName = EditorGUILayout.TextField(collector.ConstName, GUILayout.Width(120.0f), GUILayout.Height(20.0f));
        }
        else
        {
            collector.ConstName = string.Empty;
        }
        if (GUILayout.Button("-", GUILayout.Width(30.0f), GUILayout.Height(20.0f)))
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
        GUILayout.EndHorizontal();
    }
    #endregion

    #region 热更新部分
    #region 存储相关Key
    /// <summary>
    /// AB目录存储Key
    /// </summary>
    private const string ABFolderPathPreferenceKey = "ABFolderPathKey";

    /// <summary>
    /// MD5输出目录存储Key
    /// </summary>
    private const string MD5OutputFolderPathPreferenceKey = "MD5OutputFolderPathKey";

    /// <summary>
    /// AB的Md5对比数据旧文件路径存储Key
    /// </summary>
    private const string MD5ComparisonSourceFilePathPreferenceKey = "MD5ComparisonSourceFilePathKey";

    /// <summary>
    /// AB的Md5对比数据旧文件路径存储Key
    /// </summary>
    private const string MD5ComparisonTargetFilePathPreferenceKey = "MD5ComparisonTargetFilePathKey";

    /// <summary>
    /// 热更新AB准备目录路径存储Key
    /// </summary>
    private const string HotUpdateABPreparationPreferenceKey = "HotUpdateABPreparationABListKey";

    /// <summary>
    /// 热更新目录路径存储Key
    /// </summary>
    private const string HotUpdatePreparationPreferenceKey = "HotUpdatePreparationABListKey";
    #endregion

    /// <summary>
    /// 文件改变状态
    /// </summary>
    private enum EChangedFileStatus
    {
        Changed = 1,            // 改变
        Delete,                 // 移除
        Add,                    // 增加
    }

    /// <summary>
    /// AB目录
    /// </summary>
    public string ABFolderPath
    {
        get;
        private set;
    }

    /// <summary>
    /// AB的Md5信息输出目录
    /// </summary>
    public string ABMd5OutputFolderPath
    {
        get;
        private set;
    }

    /// <summary>
    /// AB的Md5对比数据旧文件路径
    /// </summary>
    public string ABMd5CompareSourceFilePath
    {
        get;
        private set;
    }

    /// <summary>
    /// AB的Md5对比数据新文件路径
    /// </summary>
    public string ABMd5CompareTargetFilePath
    {
        get;
        private set;
    }

    /// <summary>
    /// 热更新AB拷贝目录路径
    /// </summary>
    public string HotUpdateABOutputFolderPath
    {
        get;
        private set;
    }

    /// <summary>
    /// 热更新目录路径
    /// </summary>
    public string HotUpdateOutputFolderPath
    {
        get;
        private set;
    }

    /// <summary>
    /// MD5值有改变的文件名列表
    /// </summary>
    private List<KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>> mMD5ChangedABFileNameList;

    /// <summary>
    /// AB的MD5计算结果
    /// </summary>
    private string mAssetBundleMD5CaculationResult = "未触发MD5计算!";

    /// <summary>
    /// 热更拷贝的AB文件名列表
    /// </summary>
    private List<string> mHotUpdateABFileNameList;

    /// <summary>
    /// 删除的AB文件名列表
    /// </summary>
    private List<KeyValuePair<string, string>> mNeedDeleteABFileNameList = new List<KeyValuePair<string, string>>();

    /// <summary>
    /// 已删除的AB文件名列表
    /// </summary>
    private List<KeyValuePair<string, string>> mDeletedABFileNameList = new List<KeyValuePair<string, string>>();

    /// <summary>
    /// AB的MD5对比旧文件版本号
    /// </summary>
    private string mABMD5ComparisonSourceVersion;

    /// <summary>
    /// AB的MD5对比旧文件资源版本号
    /// </summary>
    private string mABMD5ComparisonSourceResourceVersion;

    /// <summary>
    /// AB的MD5对比新文件版本号
    /// </summary>
    private string mABMD5ComparisonTargetVersion;

    /// <summary>
    /// AB的MD5对比新文件资源版本号
    /// </summary>
    private string mABMD5ComparisonTargetResourceVersion;

    /// <summary>
    /// 热更新版本号(热更新准备任务使用)
    /// </summary>
    private string mHotUpdateVersion;

    /// <summary>
    /// 热更新资源版本号(热更新准备任务使用)
    /// </summary>
    private int mHotUpdateResourceVersion;

    /// <summary>
    /// 热更新准备操作结果
    /// </summary>
    private string mHotUpdatePreparationResult = "未触发热更新准备操作!";

    /// <summary>
    /// 分隔符
    /// </summary>
    private const char SeparaterKeyChar = ' ';

    /// <summary>
    /// 初始化热更新数据
    /// </summary>
    private void InitHotUpdateData()
    {
        Debug.Log($"NewBuildWindow:InitHotUpdateData()");
        mProjectPathHashValue = Application.dataPath.GetHashCode();
        ABFolderPath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{ABFolderPathPreferenceKey}");
        ABMd5OutputFolderPath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{MD5OutputFolderPathPreferenceKey}");
        ABMd5CompareSourceFilePath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{MD5ComparisonSourceFilePathPreferenceKey}");
        ABMd5CompareTargetFilePath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{MD5ComparisonTargetFilePathPreferenceKey}");
        HotUpdateABOutputFolderPath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{HotUpdateABPreparationPreferenceKey}");
        HotUpdateOutputFolderPath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{HotUpdatePreparationPreferenceKey}");
        Debug.Log("热更新操作窗口读取配置:");
        Debug.Log("AB目录:" + ABFolderPath);
        Debug.Log("MD5输出目录:" + ABMd5OutputFolderPath);
        Debug.Log("Md5对比旧文件路径:" + ABMd5CompareSourceFilePath);
        Debug.Log("Md5对比新文件路径:" + ABMd5CompareTargetFilePath);
        Debug.Log("热更新AB准备目录:" + HotUpdateABOutputFolderPath);
        Debug.Log("热更新目录:" + HotUpdateOutputFolderPath);
        VersionConfigModuleManager.Singleton.initVerisonConfigData();
    }

    /// <summary>
    /// 保存热更新数据
    /// </summary>
    private void SaveHotUpdateData()
    {
        Debug.Log($"NewBuildWindow:SaveHotUpdateData()");
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{ABFolderPathPreferenceKey}", ABFolderPath);
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{MD5OutputFolderPathPreferenceKey}", ABMd5OutputFolderPath);
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{MD5ComparisonSourceFilePathPreferenceKey}", ABMd5CompareSourceFilePath);
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{MD5ComparisonTargetFilePathPreferenceKey}", ABMd5CompareTargetFilePath);
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{HotUpdateABPreparationPreferenceKey}", HotUpdateABOutputFolderPath);
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{HotUpdatePreparationPreferenceKey}", HotUpdateOutputFolderPath);
        Debug.Log("热更新操作窗口保存配置:");
        Debug.Log("AB目录:" + ABFolderPath);
        Debug.Log("MD5输出目录:" + ABMd5OutputFolderPath);
        Debug.Log("Md5对比旧文件路径:" + ABMd5CompareSourceFilePath);
        Debug.Log("Md5对比新文件路径:" + ABMd5CompareTargetFilePath);
        Debug.Log("热更新AB准备目录:" + HotUpdateABOutputFolderPath);
        Debug.Log("热更新AB拷贝目录:" + HotUpdateOutputFolderPath);
    }

    /// <summary>
    /// 显示热更新区域
    /// </summary>
    private void DisplayHotUpdateArea()
    {
        GUILayout.BeginVertical();
        DisplayInnerVersionAndResourceVersionInfoArea();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("AB目录:", GUILayout.Width(120f));
        ABFolderPath = EditorGUILayout.TextField("", ABFolderPath);
        if (GUILayout.Button("选择AB目录", GUILayout.Width(200f)))
        {
            ABFolderPath = EditorUtility.OpenFolderPanel("AB目录", "请选择需要分析的AB所在目录!", "");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("MD5输出目录:", GUILayout.Width(120f));
        ABMd5OutputFolderPath = EditorGUILayout.TextField("", ABMd5OutputFolderPath);
        if (GUILayout.Button("选择MD5输出目录", GUILayout.Width(200f)))
        {
            ABMd5OutputFolderPath = EditorUtility.OpenFolderPanel("MD5输出目录", "请选择需要AB的MD5分析输出目录!", "");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("计算目标目录AB的MD5", GUILayout.Width(200f)))
        {
            doAssetBundleMd5Caculation();
        }
        GUILayout.EndHorizontal();
        displayAssetBundleMd5CaculationResult();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("MD5对比旧文件路径:", GUILayout.Width(120f));
        ABMd5CompareSourceFilePath = EditorGUILayout.TextField("", ABMd5CompareSourceFilePath);
        if (GUILayout.Button("选择MD5对比旧文件", GUILayout.Width(200f)))
        {
            ABMd5CompareSourceFilePath = EditorUtility.OpenFilePanel("MD5对比旧文件", "请选择需要对比的旧MD5分析文件路径!", "txt");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("MD5对比新文件路径:", GUILayout.Width(120f));
        ABMd5CompareTargetFilePath = EditorGUILayout.TextField("", ABMd5CompareTargetFilePath);
        if (GUILayout.Button("选择MD5对比新文件", GUILayout.Width(200f)))
        {
            ABMd5CompareTargetFilePath = EditorUtility.OpenFilePanel("MD5对比新文件", "请选择需要对比的新MD5分析文件路径!", "txt");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("对比新老版本的MD5", GUILayout.Width(200f)))
        {
            doAssetBundleMd5Comparison();
        }
        GUILayout.EndHorizontal();
        displayComparisonResult();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("热更新AB准备目录:", GUILayout.Width(120f));
        HotUpdateABOutputFolderPath = EditorGUILayout.TextField("", HotUpdateABOutputFolderPath);
        if (GUILayout.Button("选择热更新AB准备目录", GUILayout.Width(200f)))
        {
            HotUpdateABOutputFolderPath = EditorUtility.OpenFolderPanel("热更新AB准备目录", "请选择热更新AB准备目录!", "");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("执行热更新AB准备任务", GUILayout.Width(200f)))
        {
            doHotUpdateABPreparationTask();
        }
        GUILayout.EndHorizontal();
        displayHotUpdateABPreparationResult();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("热更新版本号:", GUILayout.Width(80f));
        mHotUpdateVersion = EditorGUILayout.TextField(mHotUpdateVersion, GUILayout.Width(100.0f));
        mHotUpdateVersion = string.Format("{0:C1}", mHotUpdateVersion);
        EditorGUILayout.LabelField("热更新资源版本号:", GUILayout.Width(100f));
        mHotUpdateResourceVersion = EditorGUILayout.IntField(mHotUpdateResourceVersion, GUILayout.Width(100.0f));
        mHotUpdateResourceVersion = mHotUpdateResourceVersion > 0 ? mHotUpdateResourceVersion : 1;
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("热更新目录:", GUILayout.Width(120f));
        HotUpdateOutputFolderPath = EditorGUILayout.TextField("", HotUpdateOutputFolderPath);
        if (GUILayout.Button("选择热更新目录", GUILayout.Width(200f)))
        {
            HotUpdateOutputFolderPath = EditorUtility.OpenFolderPanel("热更新目录", "请选择热更新目录!", "");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("执行热更新准备任务", GUILayout.Width(200f)))
        {
            doHotUpdatePreparationTask();
        }
        GUILayout.EndHorizontal();
        displayHotUpdatePreparationResult();
        displayNotice();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 执行AB的MD5分析
    /// </summary>
    private void doAssetBundleMd5Caculation()
    {
        if (Directory.Exists(ABFolderPath))
        {
            if (Directory.Exists(ABMd5OutputFolderPath))
            {
                VersionConfigModuleManager.Singleton.initVerisonConfigData();
                var resourceversion = VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode;
                var targetplatform = EditorUserBuildSettings.activeBuildTarget;
                // 文件名格式: MD5+版本号+资源版本号+平台+时间戳(年_月_日_时_分_秒)+.txt
                var nowdate = DateTime.Now;
                var md5filename = $"MD5-{Application.version}-{resourceversion}-{targetplatform}-{nowdate.Year}_{nowdate.Month}_{nowdate.Day}_{nowdate.Hour}_{nowdate.Minute}_{nowdate.Second}.txt";
                var md5filefullpath = ABMd5OutputFolderPath + Path.DirectorySeparatorChar + md5filename;
                var abfilespath = Directory.GetFiles(ABFolderPath, "*.*", SearchOption.TopDirectoryOnly).Where(f =>
                    !f.EndsWith(".meta") && !f.EndsWith(".manifest")
                );
                if (!File.Exists(md5filefullpath))
                {
                    using (File.Create(md5filefullpath))
                    {

                    }
                }
                using (var md5sw = new StreamWriter(md5filefullpath, false, Encoding.UTF8))
                {
                    var md5hash = MD5.Create();
                    //第一行是版本号信息
                    //第二行是资源版本号信息
                    //后面是AB详细(AB名+":"+MD5值+":"AB全路径)
                    md5sw.WriteLine(Application.version);
                    md5sw.WriteLine(resourceversion);
                    var sb = new StringBuilder();
                    foreach (var abfilepath in abfilespath)
                    {
                        using (var abfilefs = File.OpenRead(abfilepath))
                        {
                            sb.Clear();
                            var abfilename = Path.GetFileName(abfilepath);
                            var md5value = md5hash.ComputeHash(abfilefs);
                            foreach (var md5byte in md5value)
                            {
                                sb.Append(md5byte.ToString("x2"));
                            }
                            md5sw.WriteLine(abfilename + SeparaterKeyChar + sb.ToString() + SeparaterKeyChar + abfilepath);
                        }
                    }
                }
                Debug.Log("AB的MD5计算完毕!");
                Debug.Log($"AB的MD5文件路径:{md5filefullpath}");
                mAssetBundleMD5CaculationResult = "AB的MD5计算完毕!";
            }
            else
            {
                Debug.LogError("MD5输出目录不存在，请选择有效AB的MD5分析输出目录!");
                mAssetBundleMD5CaculationResult = "MD5输出目录不存在，请选择有效AB的MD5分析输出目录!!";
            }
        }
        else
        {
            Debug.LogError("目标AB目录不存在，请选择有效目录!");
            mAssetBundleMD5CaculationResult = "目标AB目录不存在，请选择有效目录!!";
        }
    }

    /// <summary>
    /// 执行MD5对比
    /// </summary>
    private bool doAssetBundleMd5Comparison()
    {
        if (File.Exists(ABMd5CompareSourceFilePath))
        {
            if (File.Exists(ABMd5CompareTargetFilePath))
            {
                if (mMD5ChangedABFileNameList == null)
                {
                    mMD5ChangedABFileNameList = new List<KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>>();
                }
                mMD5ChangedABFileNameList.Clear();
                mABMD5ComparisonSourceVersion = string.Empty;
                mABMD5ComparisonTargetVersion = string.Empty;
                var md51map = new Dictionary<string, KeyValuePair<string, string>>();
                var md52map = new Dictionary<string, KeyValuePair<string, string>>();
                using (var md51sr = new StreamReader(ABMd5CompareSourceFilePath))
                {
                    using (var md52sr = new StreamReader(ABMd5CompareTargetFilePath))
                    {
                        //第一行是版本号信息
                        //第二行是资源版本号信息
                        //后面是AB详细(AB名+":"+MD5值+":"AB全路径)
                        mABMD5ComparisonSourceVersion = md51sr.ReadLine();
                        mABMD5ComparisonSourceResourceVersion = md51sr.ReadLine();
                        while (!md51sr.EndOfStream)
                        {
                            var lineinfo = md51sr.ReadLine().Split(SeparaterKeyChar);
                            md51map.Add(lineinfo[0], new KeyValuePair<string, string>(lineinfo[1], lineinfo[2]));
                        }
                        mABMD5ComparisonTargetVersion = md52sr.ReadLine();
                        mABMD5ComparisonTargetResourceVersion = md52sr.ReadLine();
                        while (!md52sr.EndOfStream)
                        {
                            var lineinfo = md52sr.ReadLine().Split(SeparaterKeyChar);
                            md52map.Add(lineinfo[0], new KeyValuePair<string, string>(lineinfo[1], lineinfo[2]));
                        }
                    }
                }
                // 进行对比
                foreach (var md51 in md51map)
                {
                    if (md52map.ContainsKey(md51.Key))
                    {
                        //如果写入老版MD5的路径可能会因为老版MD5生成不是在同一台电脑上，绝对路径不对
                        //所以这里采用写入最新的MD5的路径确保能得到正确的AB路径
                        if (!md52map[md51.Key].Key.Equals(md51.Value.Key))
                        {
                            //mMD5ChangedABFileNameList.Add(new KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>(md51.Key, new KeyValuePair<string, EChangedFileStatus>(md51.Value.Value, EChangedFileStatus.Changed)));
                            mMD5ChangedABFileNameList.Add(new KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>(md51.Key, new KeyValuePair<string, EChangedFileStatus>(md52map[md51.Key].Value, EChangedFileStatus.Changed)));
                        }
                    }
                    else
                    {
                        //mMD5ChangedABFileNameList.Add(new KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>(md51.Key, new KeyValuePair<string, EChangedFileStatus>(md51.Value.Value, EChangedFileStatus.Delete)));
                        mMD5ChangedABFileNameList.Add(new KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>(md51.Key, new KeyValuePair<string, EChangedFileStatus>(md52map[md51.Key].Value, EChangedFileStatus.Delete)));
                    }
                }
                foreach (var md52 in md52map)
                {
                    if (!md51map.ContainsKey(md52.Key))
                    {
                        mMD5ChangedABFileNameList.Add(new KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>(md52.Key, new KeyValuePair<string, EChangedFileStatus>(md52.Value.Value, EChangedFileStatus.Add)));
                    }
                }
                Debug.Log("MD5对比操作完成!");
                return true;
            }
            else
            {
                Debug.LogError("对比目标文件2不存在，请选择有效文件路径!");
                return false;
            }
        }
        else
        {
            Debug.LogError("对比目标文件1不存在，请选择有效文件路径!");
            return false;
        }
    }

    /// <summary>
    /// 显示AB M5计算结果
    /// </summary>
    private void displayAssetBundleMd5CaculationResult()
    {
        GUILayout.BeginVertical();
        GUILayout.Label(mAssetBundleMD5CaculationResult);
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示MD5对比分析结果
    /// </summary>
    private void displayComparisonResult()
    {
        GUILayout.BeginVertical();
        if (mMD5ChangedABFileNameList != null)
        {
            if (mMD5ChangedABFileNameList.Count > 0)
            {
                foreach (var mdchangedabfilename in mMD5ChangedABFileNameList)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("文件名:" + mdchangedabfilename.Key, GUILayout.Width(300.0f));
                    GUILayout.Label("状态:" + mdchangedabfilename.Value.Value, GUILayout.Width(200.0f));
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("无变化!");
            }
        }
        else
        {
            GUILayout.Label("未触发MD5比较操作!");
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 执行热更新AB准备任务
    /// </summary>
    private bool doHotUpdateABPreparationTask()
    {
        if (Directory.Exists(HotUpdateABOutputFolderPath))
        {
            if (mMD5ChangedABFileNameList == null)
            {
                Debug.Log($"请先执行MD5对比操作!");
                return false;
            }
            if (mHotUpdateABFileNameList == null)
            {
                mHotUpdateABFileNameList = new List<string>();
            }
            mHotUpdateABFileNameList.Clear();
            // 创建版本号以及资源版本号对应输出目录
            // 热更详细信息文件以及APK目录
            var versionupdatefolderpath = HotUpdateABOutputFolderPath + Path.DirectorySeparatorChar + mABMD5ComparisonTargetVersion;
            // 热更AB拷贝目录+版本号目录+资源版本号目录
            var resourceupdatefolderpath = versionupdatefolderpath + Path.DirectorySeparatorChar + mABMD5ComparisonTargetResourceVersion;
            if (Directory.Exists(resourceupdatefolderpath))
            {
                Directory.Delete(resourceupdatefolderpath, true);
            }
            Directory.CreateDirectory(resourceupdatefolderpath);
            // 拷贝需要热更的AB文件
            foreach (var changedabfile in mMD5ChangedABFileNameList)
            {
                if (changedabfile.Value.Value == EChangedFileStatus.Changed || changedabfile.Value.Value == EChangedFileStatus.Add)
                {
                    mHotUpdateABFileNameList.Add(changedabfile.Key);
                    var abfiledestination = resourceupdatefolderpath + Path.DirectorySeparatorChar + changedabfile.Key;
                    if (File.Exists(changedabfile.Value.Key))
                    {
                        File.Copy(changedabfile.Value.Key, abfiledestination);
                        Debug.Log($"拷贝文件:{changedabfile.Value.Key}到{abfiledestination}");
                    }
                    else
                    {
                        Debug.Log($"目标文件:{changedabfile.Value.Key}不存在，拷贝失败!");
                    }
                }
            }
            Debug.Log($"热更新AB准备任务操作完成!");
            return true;
        }
        else
        {
            Debug.LogError("热更新AB准备目录不存在，请选择有效目录路径!");
            return false;
        }
    }

    /// <summary>
    /// 显示热更AB拷贝结果
    /// </summary>
    private void displayHotUpdateABPreparationResult()
    {
        GUILayout.BeginVertical();
        if (mHotUpdateABFileNameList != null)
        {
            if (mHotUpdateABFileNameList.Count > 0)
            {
                foreach (var hotupdateabfilename in mHotUpdateABFileNameList)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("已拷贝AB文件:" + hotupdateabfilename, GUILayout.Width(300.0f));
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                GUILayout.Label("无需拷贝!");
            }
        }
        else
        {
            GUILayout.Label("未触发热更新AB准备操作!");
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 执行热更新准备任务
    /// </summary>
    private bool doHotUpdatePreparationTask()
    {
        if (Directory.Exists(HotUpdateOutputFolderPath))
        {
            double versionnumber = 0f;
            int resourcenumber = mHotUpdateResourceVersion;
            if (!double.TryParse(mHotUpdateVersion, out versionnumber))
            {
                Debug.LogError($"填写的版本号:{mHotUpdateVersion}无效，请填写有效的版本号!");
                return false;
            }
            if (versionnumber <= 0)
            {
                Debug.LogError($"填写的版本号:{versionnumber}小于等于0无效，请填写有效的版本号!");
                return false;
            }
            // 创建版本号以及资源版本号对应输出目录
            // 对应版本的热更新目录
            var versionupdatefilefolderpath = HotUpdateOutputFolderPath + Path.DirectorySeparatorChar + versionnumber;
            // 确保热更新目录存在
            if (!Directory.Exists(versionupdatefilefolderpath))
            {
                Directory.CreateDirectory(versionupdatefilefolderpath);
            }
            // 分析热更列表文件(根据热更目录的所有热更记录以及最新热更AB信息分析)
            // 热更信息格式示例:
            // 资源版本号:资源AB名;资源版本号:资源AB名;
            var resourceupdatefilefullname = HotUpdateOutputFolderPath + Path.DirectorySeparatorChar + versionnumber + Path.DirectorySeparatorChar + HotUpdateModuleManager.ResourceUpdateListFileName;
            Debug.Log($"资源热更新文件:{resourceupdatefilefullname}");
            using (var sw = new StreamWriter(resourceupdatefilefullname, false))
            {
                //默认所有的带数字的子目录都算成资源版本号对应目录
                var subfolders = Directory.GetDirectories(versionupdatefilefolderpath, "*", SearchOption.TopDirectoryOnly);
                bool isfirstabfile = true;
                var updatefilecontent = string.Empty;
                foreach (var subfolder in subfolders)
                {
                    var resourcefoldername = new DirectoryInfo(subfolder).Name;
                    if (int.TryParse(resourcefoldername, out int resourceversion))
                    {
                        var abfiles = Directory.GetFiles(subfolder);
                        foreach (var abfile in abfiles)
                        {
                            var abfilename = Path.GetFileName(abfile);
                            if (!isfirstabfile)
                            {
                                updatefilecontent += $";{resourceversion}:{abfilename}";
                            }
                            else
                            {
                                isfirstabfile = false;
                                updatefilecontent += $"{resourceversion}:{abfilename}";
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"热更目录:{versionupdatefilefolderpath}有非法目录{subfolder}");
                        mHotUpdatePreparationResult = $"热更目录:{versionupdatefilefolderpath}有非法目录{subfolder}";
                        return false;
                    }
                }
                sw.Write(updatefilecontent);
                Debug.Log($"热更新准备任务操作完成!");
                Debug.Log($"热更新最新数据:{updatefilecontent}");
                mHotUpdatePreparationResult = $"热更新最新数据:{updatefilecontent}\n";
            }
            var serverversionfilefullname = HotUpdateOutputFolderPath + Path.DirectorySeparatorChar + HotUpdateModuleManager.ServerVersionConfigFileName;
            using (var sw = new StreamWriter(serverversionfilefullname, false))
            {
                var serverversionconfigcontent = "{" + "\"VersionCode\":" + versionnumber + ",\"ResourceVersionCode\":" + resourcenumber + "}";
                sw.WriteLine(serverversionconfigcontent);
                Debug.Log($"热更新最新版本号数据:{serverversionconfigcontent}");
                mHotUpdatePreparationResult += $"热更新最新版本号数据:{serverversionconfigcontent}";
            }
            return true;
        }
        else
        {
            Debug.LogError("热更新准备目录不存在，请选择有效目录路径!");
            mHotUpdatePreparationResult = "热更新准备目录不存在，请选择有效目录路径!";
            return false;
        }
    }

    /// <summary>
    /// 显示热更新准备的结果
    /// </summary>
    private void displayHotUpdatePreparationResult()
    {
        GUILayout.BeginVertical();
        GUILayout.Label(mHotUpdatePreparationResult);
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示提示信息
    /// </summary>
    private void displayNotice()
    {
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        GUI.color = Color.yellow;
        GUILayout.Label("注意事项:", "Box");
        GUILayout.Label($"1. 请先执行热更新AB准备任务后手动拷贝相关版本热更资源(APK和AB)到热更新目录，然后再执行热更新准备任务!", "Box");
        GUILayout.Label($"2. 热更新准备任务不会自动拷贝对应版本APK和AB文件(需要手动拷贝),只会自动分析生成资源热更新信息文件({HotUpdateModuleManager.ResourceUpdateListFileName})和服务器最新版本信息文件({HotUpdateModuleManager.ServerVersionConfigFileName})!", "Box");
        GUI.color = Color.white;
        GUILayout.EndVertical();
    }
    #endregion
}