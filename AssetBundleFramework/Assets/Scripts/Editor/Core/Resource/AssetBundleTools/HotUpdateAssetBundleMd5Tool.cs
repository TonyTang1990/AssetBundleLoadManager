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
    /// 热更新AB准备目录路径存储Key
    /// </summary>
    private const string HotUpdateABPreparationPreferenceKey = "HotUpdateABPreparationABListKey";

    /// <summary>
    /// 热更新目录路径存储Key
    /// </summary>
    private const string HotUpdatePreparationPreferenceKey = "HotUpdatePreparationABListKey";

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
    private string ABFolderPath
    {
        get
        {
            return mABFolderPath;
        }
        set
        {
            mABFolderPath = value;
            doAnalyzeNeedDeleteAB();
        }
    }
    private string mABFolderPath;

    /// <summary>
    /// AB的Md5信息输出目录
    /// </summary>
    private string ABMd5OutputFolderPath
    {
        get;
        set;
    }

    /// <summary>
    /// AB的Md5对比数据旧文件路径
    /// </summary>
    private string ABMd5CompareSourceFilePath
    {
        get
        {
            return mABMd5CompareSourceFilePath;
        }
        set
        {
            mABMd5CompareSourceFilePath = value;
        }
    }
    private string mABMd5CompareSourceFilePath;

    /// <summary>
    /// AB的Md5对比数据新文件路径
    /// </summary>
    private string ABMd5CompareTargetFilePath
    {
        get
        {
            return mABMd5CompareTargetFilePath;
        }
        set
        {
            mABMd5CompareTargetFilePath = value;
        }
    }
    private string mABMd5CompareTargetFilePath;

    /// <summary>
    /// 热更新AB拷贝目录路径
    /// </summary>
    private string mHotUpdateABOutputFolderPath;

    /// <summary>
    /// 热更新目录路径
    /// </summary>
    private string mHotUpdateOutputFolderPath;

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
    private string mHotUpdateSourceVersion;

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
        ABFolderPath = PlayerPrefs.GetString(ABFolderPathPreferenceKey);
        ABMd5OutputFolderPath = PlayerPrefs.GetString(MD5OutputFolderPathPreferenceKey);
        mABMd5CompareSourceFilePath = PlayerPrefs.GetString(MD5ComparisonSourceFilePathPreferenceKey);
        mABMd5CompareTargetFilePath = PlayerPrefs.GetString(MD5ComparisonTargetFilePathPreferenceKey);
        mHotUpdateABOutputFolderPath = PlayerPrefs.GetString(HotUpdateABPreparationPreferenceKey);
        mHotUpdateOutputFolderPath = PlayerPrefs.GetString(HotUpdatePreparationPreferenceKey);
        Debug.Log("读取配置:");
        Debug.Log("AB目录:" + ABFolderPath);
        Debug.Log("MD5输出目录:" + ABMd5OutputFolderPath);
        Debug.Log("Md5对比旧文件路径:" + mABMd5CompareSourceFilePath);
        Debug.Log("Md5对比新文件路径:" + mABMd5CompareTargetFilePath);
        Debug.Log("热更新AB准备目录:" + mHotUpdateABOutputFolderPath);
        Debug.Log("热更新目录:" + mHotUpdateOutputFolderPath);
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    private void SaveData()
    {
        PlayerPrefs.SetString(ABFolderPathPreferenceKey, ABFolderPath);
        PlayerPrefs.SetString(MD5OutputFolderPathPreferenceKey, ABMd5OutputFolderPath);
        PlayerPrefs.SetString(MD5ComparisonSourceFilePathPreferenceKey, mABMd5CompareSourceFilePath);
        PlayerPrefs.SetString(MD5ComparisonTargetFilePathPreferenceKey, mABMd5CompareTargetFilePath);
        PlayerPrefs.SetString(HotUpdateABPreparationPreferenceKey, mHotUpdateABOutputFolderPath);
        PlayerPrefs.SetString(HotUpdatePreparationPreferenceKey, mHotUpdateOutputFolderPath);
        Debug.Log("保存配置:");
        Debug.Log("AB目录:" + ABFolderPath);
        Debug.Log("MD5输出目录:" + ABMd5OutputFolderPath);
        Debug.Log("Md5对比旧文件路径:" + mABMd5CompareSourceFilePath);
        Debug.Log("Md5对比新文件路径:" + mABMd5CompareTargetFilePath);
        Debug.Log("热更新AB准备目录:" + mHotUpdateABOutputFolderPath);
        Debug.Log("热更新AB拷贝目录:" + mHotUpdateOutputFolderPath);
    }

    public void OnGUI()
    {
        mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择AB目录", GUILayout.Width(150.0f)))
        {
            ABFolderPath = EditorUtility.OpenFolderPanel("AB目录", "请选择需要分析的AB所在目录!", "");
        }
        GUILayout.Label("AB目录:" + ABFolderPath);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择MD5输出目录", GUILayout.Width(150.0f)))
        {
            ABMd5OutputFolderPath = EditorUtility.OpenFolderPanel("MD5输出目录", "请选择需要AB的MD5分析输出目录!", "");
        }
        GUILayout.Label("MD5输出目录:" + ABMd5OutputFolderPath);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("计算目标目录AB的Md5", GUILayout.Width(150.0f)))
        {
            doAssetBundleMd5Caculation();
        }
        if (GUILayout.Button("删除已删除的AB", GUILayout.Width(150.0f)))
        {
            doDeleteDeletedABFiles();
        }
        GUILayout.EndHorizontal();
        displayDeletedABResult();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择MD5对比旧文件", GUILayout.Width(150.0f)))
        {
            mABMd5CompareSourceFilePath = EditorUtility.OpenFilePanel("MD5对比旧文件", "请选择需要对比的旧MD5分析文件路径!", "txt");
        }
        GUILayout.Label("MD5对比旧文件:" + mABMd5CompareSourceFilePath);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择MD5对比新文件", GUILayout.Width(150.0f)))
        {
            mABMd5CompareTargetFilePath = EditorUtility.OpenFilePanel("MD5对比新文件", "请选择需要对比的新MD5分析文件路径!", "txt");
        }
        GUILayout.Label("MD5对比新文件:" + mABMd5CompareTargetFilePath);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("对比新老版本的MD5", GUILayout.Width(150.0f)))
        {
            doAssetBundleMd5Comparison();
        }
        GUILayout.EndHorizontal();
        displayComparisonResult();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择热更新AB准备目录", GUILayout.Width(150.0f)))
        {
            mHotUpdateABOutputFolderPath = EditorUtility.OpenFolderPanel("热更新AB准备目录", "请选择热更新AB准备目录!", "");
        }
        GUILayout.Label("热更新AB准备目录:" + mHotUpdateABOutputFolderPath);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("执行热更新AB准备任务", GUILayout.Width(150.0f)))
        {
            doHotUpdateABPreparationTask();
        }
        GUILayout.EndHorizontal();
        displayHotUpdateABPreparationResult();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("选择热更新目录", GUILayout.Width(150.0f)))
        {
            mHotUpdateOutputFolderPath = EditorUtility.OpenFolderPanel("热更新目录", "请选择热更新目录!", "");
        }
        GUILayout.Label("热更新目录:" + mHotUpdateOutputFolderPath);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("执行热更新准备任务", GUILayout.Width(150.0f)))
        {
            doHotUpdatePreparationTask();
        }
        GUILayout.EndHorizontal();
        displayHotUpdatePreparationResult();
        displayNotice();
        GUILayout.EndVertical();
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 执行AB的MD5分析
    /// </summary>
    private void doAssetBundleMd5Caculation()
    {
        if(Directory.Exists(ABFolderPath))
        {
            if(Directory.Exists(ABMd5OutputFolderPath))
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
                if(!File.Exists(md5filefullpath))
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
    /// 统计分析需要删除的AB文件
    /// </summary>
    private void doAnalyzeNeedDeleteAB()
    {
        if (Directory.Exists(ABFolderPath))
        {
            mNeedDeleteABFileNameList.Clear();
            var foldername = new DirectoryInfo(ABFolderPath).Name;
            var ab = AssetBundle.LoadFromFile(ABFolderPath + Path.DirectorySeparatorChar + foldername);          
            if(ab != null)
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
                        Debug.LogError($"需要删除的AB文件:{existabfilepath}!");
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
    /// 显示删除的AB结果
    /// </summary>
    private void displayDeletedABResult()
    {
        GUILayout.BeginVertical();
        if (mDeletedABFileNameList.Count > 0)
        {
            foreach (var deleteabfilename in mDeletedABFileNameList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("已删除AB文件名:" + deleteabfilename.Key, GUILayout.Width(300.0f));
                GUILayout.Label("已删除AB全路径:" + deleteabfilename.Value, GUILayout.Width(600.0f));
                GUILayout.EndHorizontal();
            }
        }
        else
        {
            GUILayout.Label("未删除任何AB!");
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 删除需要移除的AB文件
    /// </summary>
    private void doDeleteDeletedABFiles()
    {
        mDeletedABFileNameList.Clear();
        if (mNeedDeleteABFileNameList.Count > 0)
        {
            foreach (var deleteabfilename in mNeedDeleteABFileNameList)
            {
                if(File.Exists(deleteabfilename.Value))
                {
                    File.Delete(deleteabfilename.Value);
                    mDeletedABFileNameList.Add(deleteabfilename);
                }
                else
                {
                    Debug.LogError($"AB文件不存在:{deleteabfilename.Value}，删除失败!");
                }
            }
        }
        else
        {
            Debug.Log("没有需要删除的AB!");
        }
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
                        //第一行是版本号信息
                        //第二行是资源版本号信息
                        //后面是AB详细(AB名+":"+MD5值+":"AB全路径)
                        mABMD5ComparisonSourceVersion = md51sr.ReadLine();
                        mABMD5ComparisonSourceResourceVersion = md51sr.ReadLine();
                        while (!md51sr.EndOfStream)
                        {
                            var lineinfo = md51sr.ReadLine().Split(SeparaterKeyChar);
                            md51map.Add(lineinfo[0],new KeyValuePair<string, string>(lineinfo[1], lineinfo[2]));
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
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 执行热更新AB准备任务
    /// </summary>
    private void doHotUpdateABPreparationTask()
    {
        if (Directory.Exists(mHotUpdateABOutputFolderPath))
        {
            mHotUpdateABFileNameList.Clear();
            // 创建版本号以及资源版本号对应输出目录
            // 热更详细信息文件以及APK目录
            var versionupdatefolderpath = mHotUpdateABOutputFolderPath + Path.DirectorySeparatorChar + mABMD5ComparisonTargetVersion;
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
                    File.Copy(changedabfile.Value.Key, abfiledestination);
                    Debug.Log($"拷贝文件:{changedabfile.Value.Key}到{abfiledestination}");
                }
            }
        }
        else
        {
            Debug.LogError("热更新AB准备目录不存在，请选择有效目录路径!");
        }
    }

    /// <summary>
    /// 显示热更AB拷贝结果
    /// </summary>
    private void displayHotUpdateABPreparationResult()
    {
        GUILayout.BeginVertical();
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
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 执行热更新准备任务
    /// </summary>
    private void doHotUpdatePreparationTask()
    {
        if (Directory.Exists(mHotUpdateOutputFolderPath))
        {
            mHotUpdateABFileNameList.Clear();
            // 创建版本号以及资源版本号对应输出目录
            // 对应版本的热更新目录
            var versionupdatefilefolderpath = mHotUpdateOutputFolderPath + Path.DirectorySeparatorChar + mABMD5ComparisonTargetVersion;
            // 确保热更新目录存在
            if (!Directory.Exists(mHotUpdateOutputFolderPath))
            {
                Directory.CreateDirectory(mHotUpdateOutputFolderPath);
            }
            // 分析热更列表文件(根据热更目录的所有热更记录以及最新热更AB信息分析)
            // 热更信息格式示例:
            // 资源版本号:资源AB名;资源版本号:资源AB名;
            var resourceupdatefilefullname = HotUpdateModuleManager.ResourceUpdateListFileName;
            using (var sw = new StreamWriter(resourceupdatefilefullname, false))
            {
                //默认所有的带数字的子目录都算成资源版本号对应目录
                var subfolders = Directory.GetDirectories(versionupdatefilefolderpath);
                bool isfirstabfile = true;
                foreach(var subfolder in subfolders)
                {
                    if (int.TryParse(subfolder, out int resourceversion))
                    {
                        var abfiles = Directory.GetFiles(subfolder);
                        foreach(var abfile in abfiles)
                        {
                            if(!isfirstabfile)
                            {
                                sw.Write($"{resourceversion}:{abfile}");
                            }
                            else
                            {
                                isfirstabfile = false;
                                sw.Write($";{resourceversion}:{abfile}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError($"热更目录:{versionupdatefilefolderpath}有非法目录{subfolder}");
                    }
                }
                Debug.Log($"热更新最新数据:{sw.ToString()}");
            }
        }
        else
        {
            Debug.LogError("热更新准备目录不存在，请选择有效目录路径!");
        }
    }

    /// <summary>
    /// 显示热更新准备的结果
    /// </summary>
    private void displayHotUpdatePreparationResult()
    {
        GUILayout.BeginVertical();
        GUILayout.Label("逻辑待添加!");
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示提示信息
    /// </summary>
    private void displayNotice()
    {
        GUILayout.BeginVertical();
        GUI.color = Color.yellow;
        GUILayout.Label("注意事项:", "Box");
        GUILayout.Label("请先执行热更新AB准备任务后手动拷贝相关版本热更资源(APK和AB)到热更新目录，然后再执行热更新准备任务!", "Box");
        GUILayout.Label($"热更新准备任务不会自动拷贝对应版本APK和AB文件(需要手动拷贝),只会自动分析生成热更新信息文件({HotUpdateModuleManager.ResourceUpdateListFileName})!", "Box");
        GUI.color = Color.white;
        GUILayout.EndVertical();
    }
}