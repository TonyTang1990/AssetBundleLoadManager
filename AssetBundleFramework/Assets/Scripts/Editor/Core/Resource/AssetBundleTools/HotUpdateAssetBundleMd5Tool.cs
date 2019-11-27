/*
 * Description:             AssetBundleMd5Tool.cs
 * Author:                  TONYTANG
 * Create Date:             2019//11/26
 */

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
/// HotUpdateAssetBundleMd5Tool.cs
/// 热更新AB的Md5对比分析工具
/// </summary>
public class HotUpdateAssetBundleMd5Tool : EditorWindow
{
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
    /// 需要热更的AB拷贝输出目录
    /// </summary>
    private const string NeedHotUpdateABListPreferenceKey = "NeedHotUpdateABListKey";

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
    private string mABFolderPath;

    /// <summary>
    /// AB的Md5信息输出目录
    /// </summary>
    private string mABMd5OutputFolderPath;

    /// <summary>
    /// AB的Md5对比数据旧文件路径
    /// </summary>
    private string mABMd5CompareSourceFilePath;

    /// <summary>
    /// AB的Md5对比数据新文件路径
    /// </summary>
    private string mABMd5CompareTargetFilePath;

    /// <summary>
    /// 热更新AB拷贝目录路径
    /// </summary>
    private string mHotUpdateABOutputFolderPath;

    /// <summary>
    /// MD5值有改变的文件名列表
    /// </summary>
    private List<KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>> mMD5ChangedABFileNameList = new List<KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>>();

    /// <summary>
    /// 整体UI滚动位置
    /// </summary>
    private Vector2 mWindowUiScrollPos;

    /// <summary>
    /// 热更拷贝的AB文件名列表
    /// </summary>
    private List<string> mHotUpdateABFileNameList = new List<string>();

    /// <summary>
    /// MD5比较结果滚动位置
    /// </summary>
    private Vector2 mMD5ResultUiScrollPos;

    /// <summary>
    /// 删除的AB文件名列表
    /// </summary>
    private List<KeyValuePair<string, string>> mDeleteABFileNameList = new List<KeyValuePair<string, string>>();

    /// <summary>
    /// 删除的AB滚动位置
    /// </summary>
    private Vector2 mDeleteABUiScrollPos;

    /// <summary>
    /// 热更AB拷贝结果滚动位置
    /// </summary>
    private Vector2 mHotUpdateABResultUiScrollPos;

    /// <summary>
    /// AB的MD5对比旧文件版本
    /// </summary>
    private string mABMD5ComparisonSourceVersion;

    /// <summary>
    /// AB的MD5对比新文件版本
    /// </summary>
    private string mABMD5ComparisonTargetVersion;

    /// <summary>
    /// 分隔符
    /// </summary>
    private const char SeparaterKeyChar = ' ';

    [MenuItem("Tools/Assetbundle/热更新AB辅助工具", false)]
    public static void hotUpdateABMd5Analyzing()
    {
        var hotupdateabmd5analyzewindow = EditorWindow.GetWindow<HotUpdateAssetBundleMd5Tool>();
        hotupdateabmd5analyzewindow.Show();
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
        mABFolderPath = PlayerPrefs.GetString(ABFolderPathPreferenceKey);
        mABMd5OutputFolderPath = PlayerPrefs.GetString(MD5OutputFolderPathPreferenceKey);
        mABMd5CompareSourceFilePath = PlayerPrefs.GetString(MD5ComparisonSourceFilePathPreferenceKey);
        mABMd5CompareTargetFilePath = PlayerPrefs.GetString(MD5ComparisonTargetFilePathPreferenceKey);
        mHotUpdateABOutputFolderPath = PlayerPrefs.GetString(NeedHotUpdateABListPreferenceKey);
        Debug.Log("读取配置:");
        Debug.Log("AB目录:" + mABFolderPath);
        Debug.Log("MD5输出目录:" + mABMd5OutputFolderPath);
        Debug.Log("Md5对比旧文件路径:" + mABMd5CompareSourceFilePath);
        Debug.Log("Md5对比新文件路径:" + mABMd5CompareTargetFilePath);
        Debug.Log("热更新AB拷贝目录:" + mHotUpdateABOutputFolderPath);
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    private void SaveData()
    {
        PlayerPrefs.SetString(ABFolderPathPreferenceKey, mABFolderPath);
        PlayerPrefs.SetString(MD5OutputFolderPathPreferenceKey, mABMd5OutputFolderPath);
        PlayerPrefs.SetString(MD5ComparisonSourceFilePathPreferenceKey, mABMd5CompareSourceFilePath);
        PlayerPrefs.SetString(MD5ComparisonTargetFilePathPreferenceKey, mABMd5CompareTargetFilePath);
        PlayerPrefs.SetString(NeedHotUpdateABListPreferenceKey, mHotUpdateABOutputFolderPath);
        Debug.Log("保存配置:");
        Debug.Log("AB目录:" + mABFolderPath);
        Debug.Log("MD5输出目录:" + mABMd5OutputFolderPath);
        Debug.Log("Md5对比旧文件路径:" + mABMd5CompareSourceFilePath);
        Debug.Log("Md5对比新文件路径:" + mABMd5CompareTargetFilePath);
        Debug.Log("热更新AB拷贝目录:" + mHotUpdateABOutputFolderPath);
    }

    public void OnGUI()
    {
        mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
        GUILayout.BeginVertical();
        if (GUILayout.Button("选择AB目录"))
        {
            mABFolderPath = EditorUtility.OpenFolderPanel("AB目录", "请选择需要分析的AB所在目录!", "");
        }
        GUILayout.Label("AB目录:" + mABFolderPath);
        if(GUILayout.Button("选择MD5输出目录"))
        {
            mABMd5OutputFolderPath = EditorUtility.OpenFolderPanel("MD5输出目录", "请选择需要AB的MD5分析输出目录!", "");
        }
        GUILayout.Label("MD5输出目录:" + mABMd5OutputFolderPath);
        if (GUILayout.Button("计算目标目录AB的Md5"))
        {
            doAssetBundleMd5Caculation();
        }
        if (GUILayout.Button("删除已删除的AB"))
        {
            doDeleteRemovedAB();
        }
        displayDeleteABResult();
        if (GUILayout.Button("选择MD5对比旧文件"))
        {
            mABMd5CompareSourceFilePath = EditorUtility.OpenFilePanel("MD5对比旧文件", "请选择需要对比的旧MD5分析文件路径!", "txt");
        }
        GUILayout.Label("MD5对比旧文件:" + mABMd5CompareSourceFilePath);
        if (GUILayout.Button("选择MD5对比新文件"))
        {
            mABMd5CompareTargetFilePath = EditorUtility.OpenFilePanel("MD5对比新文件", "请选择需要对比的新MD5分析文件路径!", "txt");
        }
        GUILayout.Label("MD5对比新文件:" + mABMd5CompareTargetFilePath);
        if (GUILayout.Button("对比新老版本的MD5"))
        {
            doAssetBundleMd5Comparison();
        }
        displayComparisonResult();
        if (GUILayout.Button("选择热更AB拷贝目录"))
        {
            mHotUpdateABOutputFolderPath = EditorUtility.OpenFolderPanel("热更AB拷贝目录", "请选择热更AB拷贝目录!", "");
        }
        GUILayout.Label("热更AB拷贝目录:" + mHotUpdateABOutputFolderPath);
        if (GUILayout.Button("拷贝需要热更的AB"))
        {
            doCopyHotUpdateAB();
        }
        displayHotUpdateABCopyResult();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 执行AB的MD5分析
    /// </summary>
    private void doAssetBundleMd5Caculation()
    {
        if(Directory.Exists(mABFolderPath))
        {
            if(Directory.Exists(mABMd5OutputFolderPath))
            {
                var targetplatform = EditorUserBuildSettings.activeBuildTarget;
                var md5filename = "ABMD5-" + Application.version + "-" + targetplatform + ".txt";
                var md5filefullpath = mABMd5OutputFolderPath + Path.DirectorySeparatorChar + md5filename;
                var abfilespath = Directory.GetFiles(mABFolderPath, "*.*", SearchOption.TopDirectoryOnly).Where(f =>
                    !f.EndsWith(".meta") && !f.EndsWith(".manifest")
                );
                if(!File.Exists(md5filefullpath))
                {
                    using (File.Create(md5filefullpath))
                    {

                    }
                }
                using (var md5sw = new StreamWriter(md5filefullpath, false, Encoding.UTF8))
                {
                    var md5hash = MD5.Create();
                    //第一行是版本信息，后面才是AB详细(AB名+":"+MD5值+":"AB全路径)
                    md5sw.WriteLine(Application.version);
                    var sb = new StringBuilder();
                    foreach(var abfilepath in abfilespath)
                    {
                        using (var abfilefs = File.OpenRead(abfilepath))
                        {
                            sb.Clear();
                            var abfilename = Path.GetFileName(abfilepath);
                            var md5value = md5hash.ComputeHash(abfilefs);
                            foreach(var md5byte in md5value)
                            {
                                sb.Append(md5byte.ToString("x2"));
                            }
                            md5sw.WriteLine(abfilename + SeparaterKeyChar + sb.ToString() + SeparaterKeyChar + abfilepath);
                        }
                    }
                }
                Debug.Log("AB的MD5计算完毕!");
            }
            else
            {
                Debug.LogError("MD5输出目录不存在，请选择有效AB的MD5分析输出目录!");
            }
        }
        else
        {
            Debug.LogError("目标AB目录不存在，请选择有效目录!");
        }
    }

    /// <summary>
    /// 删除已经移除的AB文件
    /// </summary>
    private void doDeleteRemovedAB()
    {
        if (Directory.Exists(mABFolderPath))
        {
            mDeleteABFileNameList.Clear();
            var foldername = new DirectoryInfo(mABFolderPath).Name;
            var ab = AssetDatabase.LoadAssetAtPath<AssetBundle>(mABFolderPath + Path.DirectorySeparatorChar + foldername);          
            if(ab != null)
            {
                var abmanifest = ab.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                var valideallabnames = abmanifest.GetAllAssetBundles();
                var existabfilespath = Directory.GetFiles(mABFolderPath, "*.*", SearchOption.TopDirectoryOnly).Where(f =>
                    !f.EndsWith(".meta") && !f.EndsWith(".manifest")
                );
                foreach (var existabfilepath in existabfilespath)
                {
                    var existabfilename = Path.GetFileName(existabfilepath);
                    if (!valideallabnames.Contains(existabfilename))
                    {
                        mDeleteABFileNameList.Add(new KeyValuePair<string, string>(existabfilename, existabfilepath));
                        Debug.LogError($"删除{existabfilepath}的AB文件!");
                    }
                }
            }
            else
            {
                Debug.LogError($"找不到AB目录:{mABFolderPath}下的Manifest:{foldername}文件!");
            }
        }
        else
        {
            Debug.LogError($"AB目录:{mABFolderPath}不存在,无法分析需要删除的AB!");
        }
    }

    /// <summary>
    /// 显示删除的AB结果
    /// </summary>
    private void displayDeleteABResult()
    {
        GUILayout.BeginVertical();
        mDeleteABUiScrollPos = GUILayout.BeginScrollView(mDeleteABUiScrollPos);
        if (mDeleteABFileNameList.Count > 0)
        {
            foreach (var deleteabfilename in mDeleteABFileNameList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("已删除AB文件:" + deleteabfilename, GUILayout.Width(300.0f));
                GUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("没有需要删除的AB!");
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 执行MD5对比
    /// </summary>
    private void doAssetBundleMd5Comparison()
    {
        if (File.Exists(mABMd5CompareSourceFilePath))
        {
            if (File.Exists(mABMd5CompareTargetFilePath))
            {
                mMD5ChangedABFileNameList.Clear();
                mABMD5ComparisonSourceVersion = string.Empty;
                mABMD5ComparisonTargetVersion = string.Empty;
                var md51map = new Dictionary<string, KeyValuePair<string, string>>();
                var md52map = new Dictionary<string, KeyValuePair<string, string>>();
                using (var md51sr = new StreamReader(mABMd5CompareSourceFilePath))
                {
                    using (var md52sr = new StreamReader(mABMd5CompareTargetFilePath))
                    {
                        mABMD5ComparisonSourceVersion = md51sr.ReadLine();
                        while (!md51sr.EndOfStream)
                        {
                            var lineinfo = md51sr.ReadLine().Split(SeparaterKeyChar);
                            md51map.Add(lineinfo[0],new KeyValuePair<string, string>(lineinfo[1], lineinfo[2]));
                        }
                        mABMD5ComparisonTargetVersion = md52sr.ReadLine();
                        while (!md52sr.EndOfStream)
                        {
                            var lineinfo = md52sr.ReadLine().Split(SeparaterKeyChar);
                            md52map.Add(lineinfo[0], new KeyValuePair<string, string>(lineinfo[1], lineinfo[2]));
                        }
                    }
                }
                // 进行对比
                foreach(var md51 in md51map)
                {
                    if (md52map.ContainsKey(md51.Key))
                    {
                        if (!md52map[md51.Key].Key.Equals(md51.Value.Key))
                        {
                            mMD5ChangedABFileNameList.Add(new KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>(md51.Key, new KeyValuePair<string, EChangedFileStatus>(md51.Value.Value, EChangedFileStatus.Changed)));
                        }
                    }
                    else
                    {
                        mMD5ChangedABFileNameList.Add(new KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>(md51.Key, new KeyValuePair<string, EChangedFileStatus>(md51.Value.Value, EChangedFileStatus.Delete)));
                    }
                }
                foreach (var md52 in md52map)
                {
                    if (!md51map.ContainsKey(md52.Key))
                    {
                        mMD5ChangedABFileNameList.Add(new KeyValuePair<string, KeyValuePair<string, EChangedFileStatus>>(md52.Key, new KeyValuePair<string, EChangedFileStatus>(md52.Value.Value, EChangedFileStatus.Add)));
                    }
                }
            }
            else
            {
                Debug.LogError("对比目标文件2不存在，请选择有效文件路径!");
            }
        }
        else
        {
            Debug.LogError("对比目标文件1不存在，请选择有效文件路径!");
        }
    }

    /// <summary>
    /// 显示MD5对比分析结果
    /// </summary>
    private void displayComparisonResult()
    {
        GUILayout.BeginVertical();
        mMD5ResultUiScrollPos = GUILayout.BeginScrollView(mMD5ResultUiScrollPos);
        if(mMD5ChangedABFileNameList.Count > 0)
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
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 执行拷贝热更新AB操作
    /// </summary>
    private void doCopyHotUpdateAB()
    {
        if (Directory.Exists(mHotUpdateABOutputFolderPath))
        {
            mHotUpdateABFileNameList.Clear();
            // 创建对应版本的目录
            var destinationfolderpath = mHotUpdateABOutputFolderPath + Path.DirectorySeparatorChar + mABMD5ComparisonTargetVersion;
            if(Directory.Exists(destinationfolderpath))
            {
                Directory.Delete(destinationfolderpath);
            }
            Directory.CreateDirectory(destinationfolderpath);
            foreach (var changedabfile in mMD5ChangedABFileNameList)
            {
                if (changedabfile.Value.Value == EChangedFileStatus.Changed || changedabfile.Value.Value == EChangedFileStatus.Add)
                {
                    mHotUpdateABFileNameList.Add(changedabfile.Key);
                    var abfiledestination = destinationfolderpath + Path.DirectorySeparatorChar + changedabfile.Key;
                    File.Copy(changedabfile.Value.Key, abfiledestination);
                    Debug.Log($"拷贝文件:{changedabfile.Value.Key}到{abfiledestination}");
                }
            }
        }
        else
        {
            Debug.LogError("热更AB拷贝目录不存在，请选择有效目录路径!");
        }
    }

    /// <summary>
    /// 显示热更AB拷贝结果
    /// </summary>
    private void displayHotUpdateABCopyResult()
    {
        GUILayout.BeginVertical();
        mHotUpdateABResultUiScrollPos = GUILayout.BeginScrollView(mHotUpdateABResultUiScrollPos);
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
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

    }
}