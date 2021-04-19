/*
 * Description:             AssetBundletWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/25
 */

using MotionFramework.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AssetBundletWindow.cs
/// AB窗口(含AB打包和资源搜集)
/// </summary>
public class AssetBundletWindow : EditorWindow
{
    /// <summary>
    /// AB操作类型
    /// </summary>
    public enum EABOperationType
    {
        Build = 1,              // 打包窗口
        Collect,                // 搜集窗口
    }

    /// <summary>
    /// 整体UI滚动位置
    /// </summary>
    private Vector2 mWindowUiScrollPos;

    /// <summary>
    /// 当前窗口操作类型
    /// </summary>
    private EABOperationType CurrentOperationType = EABOperationType.Build;

    /// <summary>
    /// 窗口操作类型名字数组
    /// </summary>
    private string[] ABOperationTypeNameArray;

    /// <summary>
    /// 操作面板
    /// </summary>
    private string[] mToolBarStrings = { "资源打包", "资源搜集" };

    /// <summary>
    /// 操作面板选择索引
    /// </summary>
    private int mToolBarSelectIndex;

    /// <summary>
    /// 上次打开的文件夹路径
    /// </summary>
    private string LastOpenFolderPath = "Assets/";

    [MenuItem("Tools/New AssetBundle/AB资源搜集打包窗口", priority = 200)]
    static void ShowWindow()
    {
        var assetbundlewindow = EditorWindow.GetWindow<AssetBundletWindow>(false, "AB资源搜集打包");
        assetbundlewindow.Show();
    }

    private void OnEnable()
    {
        ABOperationTypeNameArray = Enum.GetNames(typeof(EABOperationType));
        mToolBarSelectIndex = 0;
        CurrentOperationType = (EABOperationType)Enum.Parse(typeof(EABOperationType), ABOperationTypeNameArray[mToolBarSelectIndex]);
        
        // 创建资源打包器
        var appVersion = new Version(Application.version);
        var buildVersion = appVersion.Revision;
        var buildTarget = EditorUserBuildSettings.activeBuildTarget;
        mAssetBuilder = new AssetBundleBuilder(buildTarget, buildVersion);

        // 读取配置
        LoadSettingsFromPlayerPrefs(mAssetBuilder);
    }

    public void OnGUI()
    {
        mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
        GUILayout.BeginVertical();
        DisplayTagArea();
        if (CurrentOperationType == EABOperationType.Build)
        {
            DisplayBuildArea();
        }
        else if(CurrentOperationType == EABOperationType.Collect)
        {
            DisplayCollectArea();
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
            CurrentOperationType = (EABOperationType)Enum.Parse(typeof(EABOperationType), ABOperationTypeNameArray[mToolBarSelectIndex]);
        }
        GUILayout.EndHorizontal();
    }

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
    private void DisplayBuildArea()
    {
        // 标题
        EditorGUILayout.LabelField("Build setup");
        EditorGUILayout.Space();

        // 构建版本
        mAssetBuilder.BuildVersion = EditorGUILayout.IntField("Build Version", mAssetBuilder.BuildVersion, GUILayout.MaxWidth(250));

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
    /// 显示搜集区域
    /// </summary>
    private void DisplayCollectArea()
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
}