/*
 * Description:             BuildWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2019/12/14
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BuildWindow.cs
/// 打包工具窗口
/// </summary>
public class BuildWindow : BaseEditorWindow
{
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
    /// 打包开发版本Key
    /// </summary>
    private const string BuildDevelopmentKey = "BuildDevelopmentKey";

    /// <summary>
    /// 打包输出路径存储Key
    /// </summary>
    private const string BuildOutputPathKey = "BuildOutputPathKey";
    #endregion

    /// <summary>
    /// 项目路径Hash值(用于使得PlayerPrefs存储的Key值唯一)
    /// </summary>
    private int mProjectPathHashValue;

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
    /// 是否是开发版本
    /// </summary>
    public bool IsDevelopment
    {
        get;
        set;
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
    /// 整体UI滚动位置
    /// </summary>
    private Vector2 mWindowUiScrollPos;

    [MenuItem("Tools/Build/打包工具", false, 1)]
    public static void buildWindow()
    {
        var buildwindow = EditorWindow.GetWindow<BuildWindow>(false, "打包工具");
        buildwindow.Show();
    }

    /// <summary>
    /// 初始化窗口数据
    /// </summary>
    protected override void InitData()
    {
        Debug.Log("BuildWindow:InitData()");
        mProjectPathHashValue = Application.dataPath.GetHashCode();
        BuildVersion = PlayerPrefs.GetString($"{mProjectPathHashValue}_{BuildVersionKey}");
        BuildResourceVersion = PlayerPrefs.GetInt($"{mProjectPathHashValue}_{BuildResourceVersionKey}");
        BuildTarget = (BuildTarget)PlayerPrefs.GetInt($"{mProjectPathHashValue}_{BuildTargetKey}");
        IsDevelopment = PlayerPrefs.GetInt($"{mProjectPathHashValue}_{BuildDevelopmentKey}") != 0;
        BuildOutputPath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{BuildOutputPathKey}");
        Debug.Log($"打包窗口读取配置:");
        Debug.Log($"版本号设置:{BuildVersion}");
        Debug.Log($"资源版本号设置:{BuildResourceVersion}");
        Debug.Log($"打包平台:{Enum.GetName(typeof(BuildTarget), BuildTarget)}");
        Debug.Log($"打包开发版本:{IsDevelopment}");
        Debug.Log($"打包输出路径:{BuildOutputPath}");
        VersionConfigModuleManager.Singleton.initVerisonConfigData();
        Debug.Log($"包内版本号:{VersionConfigModuleManager.Singleton.InnerGameVersionConfig.VersionCode}");
        Debug.Log($"包内资源版本号:{VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode}");
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    protected override void SaveData()
    {
        Debug.Log("BuildWindow:SaveData()");
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{BuildVersionKey}", BuildVersion);
        PlayerPrefs.SetInt($"{mProjectPathHashValue}_{BuildResourceVersionKey}", BuildResourceVersion);
        PlayerPrefs.SetInt($"{mProjectPathHashValue}_{BuildTargetKey}", (int)BuildTarget);
        PlayerPrefs.SetInt($"{mProjectPathHashValue}_{BuildDevelopmentKey}", IsDevelopment ? 1 : 0);
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{BuildOutputPathKey}", BuildOutputPath);
        Debug.Log("打包窗口保存配置:");
        Debug.Log("版本号设置:" + BuildVersion);
        Debug.Log("资源版本号设置:" + BuildResourceVersion);
        Debug.Log("打包平台:" + Enum.GetName(typeof(BuildTarget), BuildTarget));
        Debug.Log($"打包开发版本:{IsDevelopment}");
        Debug.Log("打包输出路径:" + BuildOutputPath);
    }

    public void OnGUI()
    {
        mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
        GUILayout.BeginVertical();
        DisplayInnerVersionAndResourceVersionInfoArea();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("打包版本号:", GUILayout.Width(70.0f));
        EditorGUI.BeginChangeCheck();
        BuildVersion = EditorGUILayout.TextField(BuildVersion, GUILayout.Width(50.0f));
        if(EditorGUI.EndChangeCheck())
        {
            double buildVersion = 0;
            if (!double.TryParse(BuildVersion, out buildVersion))
            {
                Debug.Log($"不支持的版本格式:{BuildVersion},请输入有效版本号值!");
                BuildVersion = "1.0";
            }
            else
            {
                BuildVersion = buildVersion.ToString("N1", CultureInfo.CreateSpecificCulture("en-US"));
            }
        }
        BuildVersion = string.IsNullOrEmpty(BuildVersion) ? "1.0" : BuildVersion;
        EditorGUILayout.LabelField("打包资源版本号:", GUILayout.Width(90.0f));
        BuildResourceVersion = EditorGUILayout.IntField(BuildResourceVersion, GUILayout.Width(50.0f));
        BuildResourceVersion = BuildResourceVersion > 0 ? BuildResourceVersion : 1;
        EditorGUILayout.LabelField("打包平台:", GUILayout.Width(60.0f));
        BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(BuildTarget, GUILayout.Width(100.0f));
        EditorGUILayout.LabelField($"开发版本:", GUILayout.Width(60f), GUILayout.Height(20f));
        IsDevelopment = EditorGUILayout.Toggle(IsDevelopment, GUILayout.Width(100.0f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("打包输出目录:", GUILayout.Width(80.0f));
        BuildOutputPath = EditorGUILayout.TextField("", BuildOutputPath);
        if (GUILayout.Button("选择打包输出目录", GUILayout.Width(150.0f)))
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
        GUILayout.EndScrollView();
    }

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
    /// 执行打包
    /// </summary>
    private void DoBuild()
    {
        Debug.Log("DoBuild()");
        double buildVersion = 0;
        if (!double.TryParse(BuildVersion, out buildVersion))
        {
            Debug.LogError($"解析版本号:{BuildVersion}失败,格式无效!");
            return;
        }
        BuildVersion = buildVersion.ToString("N1", CultureInfo.CreateSpecificCulture("en-US"));
        buildVersion = double.Parse(BuildVersion);
        BuildTool.DoBuild(BuildOutputPath, BuildTarget, buildVersion, BuildResourceVersion, IsDevelopment);
    }
}