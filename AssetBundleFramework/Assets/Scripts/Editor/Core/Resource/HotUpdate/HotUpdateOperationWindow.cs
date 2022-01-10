/*
 * Description:             HotUpdateOperationWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2019//11/26
 */

using MotionFramework.Editor;
using System.Globalization;
using UnityEditor;
using UnityEngine;

/// <summary>
/// HotUpdateOperationWindow.cs
/// 热更新操作窗口
/// </summary>
public class HotUpdateOperationWindow : BaseEditorWindow
{
    #region 存储相关Key
    /// <summary>
    /// 热更新目录路径存储Key
    /// </summary>
    private const string HotUpdatePreparationPreferenceKey = "HotUpdatePreparationABListKey";
    #endregion

    /// <summary>
    /// 项目路径Hash值(用于使得PlayerPrefs存储的Key值唯一)
    /// </summary>
    private int mProjectPathHashValue;

    /// <summary>
    /// 热更新目录路径
    /// </summary>
    public string HotUpdateOutputFolderPath
    {
        get;
        private set;
    }

    /// <summary>
    /// 整体UI滚动位置
    /// </summary>
    private Vector2 mWindowUiScrollPos;

    /// <summary>
    /// 热更新版本号(热更新准备任务使用)
    /// </summary>
    private string mHotUpdateVersion;

    /// <summary>
    /// 热更新资源版本号(热更新准备任务使用)
    /// </summary>
    private int mHotUpdateResourceVersion;

    /// <summary>
    /// 热更新平台
    /// </summary>
    public BuildTarget BuildTarget
    {
        get;
        private set;
    }

    /// <summary>
    /// 热更新准备结果字符串
    /// </summary>
    private string mHotUpdatePreparationResult;

    [MenuItem("Tools/HotUpdate/热更新操作工具", false, 102)]
    public static void hotUpdateOpterationWindow()
    {
        var hotupdateoperationwindow = EditorWindow.GetWindow<HotUpdateOperationWindow>(false, "热更新工具");
        hotupdateoperationwindow.Show();
    }


    //[MenuItem("Tools/HotUpdate/关闭热更新操作工具", false, 103)]
    //public static void closeHotUpdateOpterationWindow()
    //{
    //    var hotupdateoperationwindow = EditorWindow.GetWindow<HotUpdateOperationWindow>(false, "热更新工具");
    //    hotupdateoperationwindow.Close();
    //}

    /// <summary>
    /// 初始化窗口数据
    /// </summary>
    protected override void InitData()
    {
        mProjectPathHashValue = Application.dataPath.GetHashCode();
        HotUpdateOutputFolderPath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{HotUpdatePreparationPreferenceKey}");
        Debug.Log("热更新操作窗口读取配置:");
        Debug.Log("热更新输出目录:" + HotUpdateOutputFolderPath);
        VersionConfigModuleManager.Singleton.initVerisonConfigData();
        BuildTarget = EditorUserBuildSettings.activeBuildTarget;
        // 默认热更新版本信息就是包内版本信息
        mHotUpdateVersion = VersionConfigModuleManager.Singleton.InnerGameVersionConfig.VersionCode.ToString();
        mHotUpdateResourceVersion = VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode;
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    protected override void SaveData()
    {
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{HotUpdatePreparationPreferenceKey}", HotUpdateOutputFolderPath);
        Debug.Log("热更新操作窗口保存配置:");
        Debug.Log("热更新输出目录:" + HotUpdateOutputFolderPath);
    }

    public void OnGUI()
    {
        mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("包内版本号:", GUILayout.Width(100f));
        GUILayout.Label($"{VersionConfigModuleManager.Singleton.InnerGameVersionConfig.VersionCode}", "box", GUILayout.Width(120f));
        EditorGUILayout.LabelField("包内资源版本号:", GUILayout.Width(120f));
        GUILayout.Label($"{VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode}", "box", GUILayout.Width(120f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.LabelField("热更新版本号:", GUILayout.Width(80f));
        mHotUpdateVersion = EditorGUILayout.TextField(mHotUpdateVersion, GUILayout.Width(50.0f));
        if (EditorGUI.EndChangeCheck())
        {
            double buildVersion = 0;
            if (!double.TryParse(mHotUpdateVersion, out buildVersion))
            {
                Debug.Log($"不支持的版本格式:{mHotUpdateVersion},请输入有效版本号值!");
                mHotUpdateVersion = "1.0";
            }
            else
            {
                mHotUpdateVersion = buildVersion.ToString("N1", CultureInfo.CreateSpecificCulture("en-US"));
            }
            mHotUpdateVersion = string.IsNullOrEmpty(mHotUpdateVersion) ? "1.0" : mHotUpdateVersion;
        }
        EditorGUILayout.LabelField("热更新资源版本号:", GUILayout.Width(100f));
        mHotUpdateResourceVersion = EditorGUILayout.IntField(mHotUpdateResourceVersion, GUILayout.Width(100.0f));
        mHotUpdateResourceVersion = mHotUpdateResourceVersion > 0 ? mHotUpdateResourceVersion : 1;
        EditorGUILayout.LabelField("打包平台:", GUILayout.Width(60.0f));
        BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(BuildTarget, GUILayout.Width(100.0f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("热更新目录:", GUILayout.Width(120f));
        HotUpdateOutputFolderPath = EditorGUILayout.TextField("", HotUpdateOutputFolderPath);
        if (GUILayout.Button("选择热更新目录", GUILayout.Width(200f)))
        {
            HotUpdateOutputFolderPath = EditorUtility.OpenFolderPanel("热更新目录", "请选择热更新目录!", "");
        }
        GUILayout.EndHorizontal();
        if (GUILayout.Button("执行热更新准备任务", GUILayout.ExpandWidth(true)))
        {
            doHotUpdatePreparationTask();
        }
        displayHotUpdatePreparationResult();
        displayNotice();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 执行热更新准备任务
    /// </summary>
    private bool doHotUpdatePreparationTask()
    {
        var assetBundleOutputPath = AssetBundleBuilderHelper.GetDefaultOutputRootPath();
        if (HotUpdateTool.DoHotUpdatePreparationTask(assetBundleOutputPath, HotUpdateOutputFolderPath, BuildTarget, mHotUpdateVersion, mHotUpdateResourceVersion))
        {
            mHotUpdatePreparationResult = $"资源热更新准备工作执行完成!";
            return true;
        }
        else
        {
            mHotUpdatePreparationResult = $"资源热更新准备工作执行失败!";
            return false;
        }
        
    }

    /// <summary>
    /// 显示热更新准备的结果
    /// </summary>
    private void displayHotUpdatePreparationResult()
    {
        if(!string.IsNullOrEmpty(mHotUpdatePreparationResult))
        {
            GUILayout.BeginVertical();
            GUILayout.Label(mHotUpdatePreparationResult);
            GUILayout.EndVertical();
        }
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
        GUILayout.Label($"1. 选择热更新的版本和资源版本号!", "Box");
        GUILayout.Label($"2. 执行热更新资源准备工作(复制最新AB资源和AssetBundleMD5.txt文件以及生成最新的ServerVersionConfig.json)到热更新目录!", "Box");
        GUI.color = Color.white;
        GUILayout.EndVertical();
    }
}