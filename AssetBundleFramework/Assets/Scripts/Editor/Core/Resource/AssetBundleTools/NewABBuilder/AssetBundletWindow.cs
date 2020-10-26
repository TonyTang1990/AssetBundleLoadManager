/*
 * Description:             AssetBundletWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/25
 */

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
    /// AB搜集设置信息
    /// </summary>
    public static AssetBundleCollectSetting CollectSetting
    {
        get
        {
            if (mCollectSetting == null)
            {
                LoadCollectSettingData();
            }
            return mCollectSetting;
        }
    }
    private static AssetBundleCollectSetting mCollectSetting = null;

    /// <summary>
    /// AB搜集设置信息存储目录相对路径
    /// </summary>
    private static string AssetBundleCollectSettingSaveFolderRelativePath = "/AssetBundleCollectSetting";

    /// <summary>
    /// AB搜集设置文件名
    /// </summary>
    private static string AssetBundleCollectSettingFileName = "AssetBundleCollectSetting.asset";

    /// <summary>
    /// AB搜集设置信息文件存储相对路径
    /// </summary>
    private static string AssetBundleCollectSettingFileRelativePath = $"Assets{AssetBundleCollectSettingSaveFolderRelativePath}/{AssetBundleCollectSettingFileName}";

    [MenuItem("Tools/New AssetBundle/AB资源搜集窗口", priority = 200)]
    static void ShowWindow()
    {
        var assetbundlewindow = EditorWindow.GetWindow<AssetBundletWindow>(false, "AB资源搜集");
        assetbundlewindow.Show();
    }

    private void OnEnable()
    {
        ABOperationTypeNameArray = Enum.GetNames(typeof(EABOperationType));
        mToolBarSelectIndex = 0;
        CurrentOperationType = (EABOperationType)Enum.Parse(typeof(EABOperationType), ABOperationTypeNameArray[mToolBarSelectIndex]);
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
    /// 显示打包区域
    /// </summary>
    private void DisplayBuildArea()
    {

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
        for (int i = 0, length = CollectSetting.AssetBundleCollectors.Count; i < length; i++)
        {
            DisplayOneCollect(CollectSetting.AssetBundleCollectors[i]);
        }
        if (GUILayout.Button("+", GUILayout.ExpandWidth(true), GUILayout.Height(20.0f)))
        {
            var chosenfolderpath = EditorUtility.OpenFolderPanel("选择搜集目录", "", "");
            if (CollectSetting.AddAssetBundleCollector(chosenfolderpath))
            {
                Debug.Log($"添加资源搜集目录:{chosenfolderpath}成功!");
            }
            else
            {
                Debug.LogError($"添加资源搜集目录:{chosenfolderpath}失败!");
            }
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
        collector.BuildRule = (AssetBundleCollectRule)EditorGUILayout.EnumPopup(collector.BuildRule, GUILayout.Width(120.0f), GUILayout.Height(20.0f));
        if (GUILayout.Button("-", GUILayout.Width(30.0f), GUILayout.Height(20.0f)))
        {
            if (CollectSetting.RemoveAssetBundleCollector(collector))
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

    /// <summary>
    /// 加载配置文件
    /// </summary>
    private static void LoadCollectSettingData()
    {
        var assetbundlecollectsettingfolderpath = Application.dataPath + AssetBundleCollectSettingSaveFolderRelativePath;
        if (!Directory.Exists(assetbundlecollectsettingfolderpath))
        {
            Directory.CreateDirectory(assetbundlecollectsettingfolderpath);
        }
        var assetbundlecollectsetting = AssetDatabase.LoadAssetAtPath<AssetBundleCollectSetting>(AssetBundleCollectSettingFileRelativePath);
        if(assetbundlecollectsetting == null)
        {
            assetbundlecollectsetting = new AssetBundleCollectSetting();
            AssetDatabase.CreateAsset(assetbundlecollectsetting, AssetBundleCollectSettingFileRelativePath);
            AssetDatabase.SaveAssets();
        }
        mCollectSetting = assetbundlecollectsetting;
        CheckCollectorSettingValidation();
    }

    /// <summary>
    /// 检查资源搜集有效性
    /// </summary>
    public static bool CheckCollectorSettingValidation()
    {
        // 检查是否有无效的资源搜集设定
        var result = CollectSetting.HasInvalideCollectFolderPath();
        if(result)
        {
            Debug.LogError($"有无效的资源搜集设置!");
        }
        return result;
    }
    #endregion
}