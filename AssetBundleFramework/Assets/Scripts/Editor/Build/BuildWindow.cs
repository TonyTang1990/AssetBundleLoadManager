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
    /// 开发模式Key
    /// </summary>
    private const string DevelopModeKey = "DevelopModeKey";

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
    /// 游戏开发模式
    /// </summary>
    public GameDevelopMode DevelopMode
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

    [MenuItem("Tools/DevelopMode/切换本地开发模式", false, 1)]
    public static void changeToInnerDevelopMode()
    {
        BuildTool.ModifyInnerGameConfig(GameDevelopMode.InnerDevelop);
    }


    [MenuItem("Tools/DevelopMode/切换发布模式", false, 2)]
    public static void changeToReleaseMode()
    {
        BuildTool.ModifyInnerGameConfig(GameDevelopMode.Release);
    }

    /// <summary>
    /// 初始化窗口数据
    /// </summary>
    protected override void InitData()
    {
        Debug.Log("BuildWindow:InitData()");
        mProjectPathHashValue = Application.dataPath.GetHashCode();
        BuildVersion = PlayerPrefs.GetString(GetProjectBuildVersionKey());
        BuildResourceVersion = PlayerPrefs.GetInt(GetProjectBuildResourceVersionKey());
        BuildTarget = (BuildTarget)PlayerPrefs.GetInt(GetProjectBuildTargetKey());
        IsDevelopment = PlayerPrefs.GetInt(GetProjectBuildDevelopmentKey()) != 0;
        DevelopMode = (GameDevelopMode)PlayerPrefs.GetInt(GetProjectBuildDevelopModeKey(), (int)GameDevelopMode.Release);
        BuildOutputPath = PlayerPrefs.GetString(GetProjectBuildOutputPathKey());
        Debug.Log($"打包窗口读取配置:");
        Debug.Log($"版本号设置:{BuildVersion}");
        Debug.Log($"资源版本号设置:{BuildResourceVersion}");
        Debug.Log($"打包平台:{Enum.GetName(typeof(BuildTarget), BuildTarget)}");
        Debug.Log($"打包开发版本:{IsDevelopment}");
        Debug.Log($"游戏开发模式:{DevelopMode}");
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
        PlayerPrefs.SetString(GetProjectBuildVersionKey(), BuildVersion);
        PlayerPrefs.SetInt(GetProjectBuildResourceVersionKey(), BuildResourceVersion);
        PlayerPrefs.SetInt(GetProjectBuildTargetKey(), (int)BuildTarget);
        PlayerPrefs.SetInt(GetProjectBuildDevelopmentKey(), IsDevelopment ? 1 : 0);
        PlayerPrefs.SetInt(GetProjectBuildDevelopModeKey(), (int)DevelopMode);
        PlayerPrefs.SetString(GetProjectBuildOutputPathKey(), BuildOutputPath);
        Debug.Log("打包窗口保存配置:");
        Debug.Log("版本号设置:" + BuildVersion);
        Debug.Log("资源版本号设置:" + BuildResourceVersion);
        Debug.Log("打包平台:" + Enum.GetName(typeof(BuildTarget), BuildTarget));
        Debug.Log($"打包开发版本:{IsDevelopment}");
        Debug.Log($"游戏开发模式:{DevelopMode}");
        Debug.Log("打包输出路径:" + BuildOutputPath);
    }

    /// <summary>
    /// 获取项目打包版本Key
    /// </summary>
    /// <returns></returns>
    private string GetProjectBuildVersionKey()
    {
        return $"{mProjectPathHashValue}_{BuildVersionKey}";
    }

    /// <summary>
    /// 获取项目打包资源版本Key
    /// </summary>
    /// <returns></returns>
    private string GetProjectBuildResourceVersionKey()
    {
        return $"{mProjectPathHashValue}_{BuildResourceVersionKey}";
    }

    /// <summary>
    /// 获取项目打包平台Key
    /// </summary>
    /// <returns></returns>
    private string GetProjectBuildTargetKey()
    {
        return $"{mProjectPathHashValue}_{BuildTargetKey}";
    }

    /// <summary>
    /// 获取项目打包开发版本Key
    /// </summary>
    /// <returns></returns>
    private string GetProjectBuildDevelopmentKey()
    {
        return $"{mProjectPathHashValue}_{BuildDevelopmentKey}";
    }

    /// <summary>
    /// 获取项目打包开发模式Key
    /// </summary>
    /// <returns></returns>
    private string GetProjectBuildDevelopModeKey()
    {
        return $"{mProjectPathHashValue}_{DevelopModeKey}";
    }

    /// <summary>
    /// 获取项目打包输出目录Key
    /// </summary>
    /// <returns></returns>
    private string GetProjectBuildOutputPathKey()
    {
        return $"{mProjectPathHashValue}_{BuildOutputPathKey}";
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
        IsDevelopment = EditorGUILayout.Toggle(IsDevelopment, GUILayout.Width(20f));
        EditorGUILayout.LabelField($"开发模式:", GUILayout.Width(60f), GUILayout.Height(20f));
        DevelopMode = (GameDevelopMode)EditorGUILayout.EnumPopup(DevelopMode, GUILayout.Width(100f));
        if (GUILayout.Button("修改包内版本信息", GUILayout.Width(120f), GUILayout.Height(20f)))
        {
            DoModifyInnerVersionConfig();
        }
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
    /// 执行修改包内版本信息
    /// </summary>
    private void DoModifyInnerVersionConfig()
    {
        double buildVersion = 0;
        if (!double.TryParse(BuildVersion, out buildVersion))
        {
            Debug.LogError($"解析版本号:{BuildVersion}失败,格式无效!");
            return;
        }
        BuildTool.ModifyInnerVersionConfig(buildVersion, BuildResourceVersion);
    }


    /// <summary>
    /// 执行修改包内游戏配置信息
    /// </summary>
    private void DoModifyInnerGameConfig()
    {
        if (DevelopMode == GameDevelopMode.Invalide)
        {
            Debug.LogError($"不允许修改游戏开发模式到:{DevelopMode}，格式无效，修改失败!");
            return;
        }
        BuildTool.ModifyInnerGameConfig(DevelopMode);
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