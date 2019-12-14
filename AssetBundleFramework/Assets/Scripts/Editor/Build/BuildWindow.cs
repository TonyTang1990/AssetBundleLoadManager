/*
 * Description:             BuildWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2019/12/14
 */

using System;
using System.Collections;
using System.Collections.Generic;
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
        BuildOutputPath = PlayerPrefs.GetString($"{mProjectPathHashValue}_{BuildOutputPathKey}");
        Debug.Log("打包窗口读取配置:");
        Debug.Log("版本号设置:" + BuildVersion);
        Debug.Log("资源版本号设置:" + BuildResourceVersion);
        Debug.Log("打包平台:" + Enum.GetName(typeof(BuildTarget), BuildTarget));
        Debug.Log("打包输出路径:" + BuildOutputPath);
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
        PlayerPrefs.SetString($"{mProjectPathHashValue}_{BuildOutputPathKey}", BuildOutputPath);
        Debug.Log("打包窗口保存配置:");
        Debug.Log("版本号设置:" + BuildVersion);
        Debug.Log("资源版本号设置:" + BuildResourceVersion);
        Debug.Log("打包平台:" + Enum.GetName(typeof(BuildTarget), BuildTarget));
        Debug.Log("打包输出路径:" + BuildOutputPath);
    }

    public void OnGUI()
    {
        mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("打包版本号:", GUILayout.Width(70.0f));
        BuildVersion = EditorGUILayout.TextField(BuildVersion, GUILayout.Width(50.0f));
        EditorGUILayout.LabelField("打包资源版本号:", GUILayout.Width(90.0f));
        BuildResourceVersion = EditorGUILayout.IntField(BuildResourceVersion, GUILayout.Width(50.0f));
        EditorGUILayout.LabelField("打包平台:", GUILayout.Width(60.0f));
        BuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(BuildTarget, GUILayout.Width(100.0f));
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
    /// 执行打包
    /// </summary>
    private void DoBuild()
    {
        Debug.Log("DoBuild()");
        if(!string.IsNullOrEmpty(BuildOutputPath))
        {
            if(!Directory.Exists(BuildOutputPath))
            {
                Directory.CreateDirectory(BuildOutputPath);
            }
            var buildtargetgroup = GetCorrespondingBuildTaregtGroup(BuildTarget);
            Debug.Log($"打包分组:{Enum.GetName(typeof(BuildTargetGroup), buildtargetgroup)}");
            if (buildtargetgroup != BuildTargetGroup.Unknown)
            {
                double newversioncode;
                if(double.TryParse(BuildVersion, out newversioncode))
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
        switch(buildtarget)
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
}