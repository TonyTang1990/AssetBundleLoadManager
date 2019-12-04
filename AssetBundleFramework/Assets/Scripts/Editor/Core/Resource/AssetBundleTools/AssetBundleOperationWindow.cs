/*
 * Description:             AssetBundleOperationWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2019//12/01
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AssetBundleOperationWindow.cs
/// AB操作处理窗口
/// </summary>
public class AssetBundleOperationWindow : EditorWindow
{
    /// <summary>
    /// AB目录存储Key
    /// </summary>
    private const string ABOT_ABFolderPathPreferenceKey = "ABOT_ABFolderPathKey";

    /// <summary>
    /// AB目录
    /// </summary>
    private string ABFolderPath
    {
        get
        {
            return mABFolderPath;
        }
        set
        {
            mABFolderPath = value;
        }
    }
    private string mABFolderPath;

    /// <summary>
    /// 整体UI滚动位置
    /// </summary>
    private Vector2 mWindowUiScrollPos;

    /// <summary>
    /// 删除的AB文件名列表
    /// </summary>
    private List<KeyValuePair<string, string>> mNeedDeleteABFileNameList = new List<KeyValuePair<string, string>>();

    /// <summary>
    /// 已删除的AB文件名列表
    /// </summary>
    private List<KeyValuePair<string, string>> mDeletedABFileNameList = new List<KeyValuePair<string, string>>();

    [MenuItem("Tools/AssetBundle/AssetBundle操作工具", false, 102)]
    public static void assetBundleOpterationWindow()
    {
        var assetbundleoperationwindow = EditorWindow.GetWindow<AssetBundleOperationWindow>();
        assetbundleoperationwindow.Show();
    }

    private void OnEnable()
    {
        InitData();
    }

    private void OnDisable()
    {
        SaveData();
    }

    private void OnDestroy()
    {
        SaveData();
    }

    /// <summary>
    /// 初始化窗口数据
    /// </summary>
    private void InitData()
    {
        ABFolderPath = PlayerPrefs.GetString(ABOT_ABFolderPathPreferenceKey);
        Debug.Log("AssetBundle操作窗口读取配置:");
        Debug.Log("AB目录:" + ABFolderPath);
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    private void SaveData()
    {
        PlayerPrefs.SetString(ABOT_ABFolderPathPreferenceKey, ABFolderPath);
        Debug.Log("AssetBundle操作窗口保存配置:");
        Debug.Log("AB目录:" + ABFolderPath);
    }

    public void OnGUI()
    {
        mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("AB目录:", GUILayout.Width(50.0f));
        EditorGUILayout.TextField("", ABFolderPath);
        if (GUILayout.Button("选择AB目录", GUILayout.Width(150.0f)))
        {
            ABFolderPath = EditorUtility.OpenFolderPanel("AB目录", "请选择需要分析的AB所在目录!", "");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("分析并删除已删除AB", GUILayout.Width(150.0f)))
        {
            doAnalyzeAndDeleteDeletedABFiles();
        }
        GUILayout.EndHorizontal();
        displayDeletedABResult();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 统计分析需要删除的AB文件
    /// </summary>
    private void doAnalyzeNeedDeleteAB()
    {
        if (Directory.Exists(ABFolderPath))
        {
            mNeedDeleteABFileNameList.Clear();
            var foldername = new DirectoryInfo(ABFolderPath).Name;
            var ab = AssetBundle.LoadFromFile(ABFolderPath + Path.DirectorySeparatorChar + foldername);
            if (ab != null)
            {
                var abmanifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                var valideallabnames = abmanifest.GetAllAssetBundles();
                var existabfilespath = Directory.GetFiles(ABFolderPath, "*.*", SearchOption.TopDirectoryOnly).Where(f =>
                    !f.EndsWith(".meta") && !f.EndsWith(".manifest")
                ).ToList<string>();
                foreach (var existabfilepath in existabfilespath)
                {
                    var existabfilename = Path.GetFileName(existabfilepath);
                    // GetAllAssetBundles得不到依赖信息AB自身，目录下的同名依赖信息文件需要单独排除
                    if (!valideallabnames.Contains(existabfilename) && !existabfilename.Equals(foldername))
                    {
                        mNeedDeleteABFileNameList.Add(new KeyValuePair<string, string>(existabfilename, existabfilepath));
                        Debug.Log($"需要删除的AB文件:{existabfilepath}!");
                    }
                }
                ab.Unload(true);
            }
            else
            {
                Debug.LogError($"找不到AB目录:{ABFolderPath}下的Manifest:{foldername}文件!");
            }
        }
        else
        {
            Debug.LogError($"AB目录:{ABFolderPath}不存在,无法分析需要删除的AB!");
        }
    }

    /// <summary>
    /// 分析并删除需要移除的AB文件
    /// </summary>
    private bool doAnalyzeAndDeleteDeletedABFiles()
    {
        mDeletedABFileNameList.Clear();
        doAnalyzeNeedDeleteAB();
        if (mNeedDeleteABFileNameList != null)
        {
            if (mNeedDeleteABFileNameList.Count > 0)
            {
                foreach (var deleteabfilename in mNeedDeleteABFileNameList)
                {
                    //连带Meta和Manifest文件一起删除
                    if (File.Exists(deleteabfilename.Value))
                    {
                        var abmetafilename = deleteabfilename.Value + ".meta";
                        var abmanifestfilename = deleteabfilename.Value + ".manifest";
                        var abmanifestmetafilename = abmanifestfilename + ".meta";
                        File.Delete(deleteabfilename.Value);
                        File.Delete(abmetafilename);
                        File.Delete(abmanifestfilename);
                        File.Delete(abmanifestmetafilename);
                        mDeletedABFileNameList.Add(deleteabfilename);
                    }
                    else
                    {
                        Debug.LogError($"AB文件不存在:{deleteabfilename.Value}，删除失败!");
                        return false;
                    }
                }
                Debug.Log("分析并删除需要移除的AB文件操作完成!");
                return true;
            }
            else
            {
                Debug.Log("没有需要删除的AB!");
                return true;
            }
        }
        else
        {
            Debug.Log("请先执行AB文件删除分析!");
            return false;
        }
    }

    /// <summary>
    /// 显示删除的AB结果
    /// </summary>
    private void displayDeletedABResult()
    {
        GUILayout.BeginVertical();
        if (mDeletedABFileNameList.Count > 0)
        {
            GUILayout.Label("已删除AB文件信息:", GUILayout.Width(100.0f));
            foreach (var deleteabfilename in mDeletedABFileNameList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("文件名:" + deleteabfilename.Key, GUILayout.Width(250.0f));
                GUILayout.Label("全路径:" + deleteabfilename.Value, GUILayout.Width(1200.0f));
                GUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("未删除任何AB!");
        }
        GUILayout.EndVertical();
    }
}