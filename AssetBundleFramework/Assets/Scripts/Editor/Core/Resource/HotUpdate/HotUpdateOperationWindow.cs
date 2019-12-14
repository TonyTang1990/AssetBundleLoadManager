/*
 * Description:             HotUpdateOperationWindow.cs
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
/// HotUpdateOperationWindow.cs
/// 热更新操作窗口
/// </summary>
public class HotUpdateOperationWindow : BaseEditorWindow
{
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
    /// 项目路径Hash值(用于使得PlayerPrefs存储的Key值唯一)
    /// </summary>
    private int mProjectPathHashValue;

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
    /// 整体UI滚动位置
    /// </summary>
    private Vector2 mWindowUiScrollPos;

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
    private string mHotUpdateResourceVersion;

    /// <summary>
    /// 热更新准备操作结果
    /// </summary>
    private string mHotUpdatePreparationResult = "未触发热更新准备操作!";

    /// <summary>
    /// 分隔符
    /// </summary>
    private const char SeparaterKeyChar = ' ';

    [MenuItem("Tools/HotUpdate/热更新操作工具", false, 102)]
    public static void hotUpdateOpterationWindow()
    {
        var hotupdateoperationwindow = EditorWindow.GetWindow<HotUpdateOperationWindow>(false, "热更新工具");
        hotupdateoperationwindow.Show();
    }
    
    /// <summary>
    /// 初始化窗口数据
    /// </summary>
    protected override void InitData()
    {
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
    }

    /// <summary>
    /// 保存数据
    /// </summary>
    protected override void SaveData()
    {
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

    public void OnGUI()
    {
        mWindowUiScrollPos = GUILayout.BeginScrollView(mWindowUiScrollPos);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("AB目录:", GUILayout.Width(50.0f));
        ABFolderPath = EditorGUILayout.TextField("", ABFolderPath);
        if (GUILayout.Button("选择AB目录", GUILayout.Width(150.0f)))
        {
            ABFolderPath = EditorUtility.OpenFolderPanel("AB目录", "请选择需要分析的AB所在目录!", "");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("MD5输出目录:", GUILayout.Width(80.0f));
        ABMd5OutputFolderPath = EditorGUILayout.TextField("", ABMd5OutputFolderPath);
        if (GUILayout.Button("选择MD5输出目录", GUILayout.Width(150.0f)))
        {
            ABMd5OutputFolderPath = EditorUtility.OpenFolderPanel("MD5输出目录", "请选择需要AB的MD5分析输出目录!", "");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("计算目标目录AB的MD5", GUILayout.Width(150.0f)))
        {
            doAssetBundleMd5Caculation();
        }
        GUILayout.EndHorizontal();
        displayAssetBundleMd5CaculationResult();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("MD5对比旧文件路径:", GUILayout.Width(120.0f));
        ABMd5CompareSourceFilePath = EditorGUILayout.TextField("", ABMd5CompareSourceFilePath);
        if (GUILayout.Button("选择MD5对比旧文件", GUILayout.Width(150.0f)))
        {
            ABMd5CompareSourceFilePath = EditorUtility.OpenFilePanel("MD5对比旧文件", "请选择需要对比的旧MD5分析文件路径!", "txt");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("MD5对比新文件路径:", GUILayout.Width(120.0f));
        ABMd5CompareTargetFilePath = EditorGUILayout.TextField("", ABMd5CompareTargetFilePath);
        if (GUILayout.Button("选择MD5对比新文件", GUILayout.Width(150.0f)))
        {
            ABMd5CompareTargetFilePath = EditorUtility.OpenFilePanel("MD5对比新文件", "请选择需要对比的新MD5分析文件路径!", "txt");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("对比新老版本的MD5", GUILayout.Width(150.0f)))
        {
            doAssetBundleMd5Comparison();
        }
        GUILayout.EndHorizontal();
        displayComparisonResult();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("热更新AB准备目录:", GUILayout.Width(100.0f));
        HotUpdateABOutputFolderPath = EditorGUILayout.TextField("", HotUpdateABOutputFolderPath);
        if (GUILayout.Button("选择热更新AB准备目录", GUILayout.Width(150.0f)))
        {
            HotUpdateABOutputFolderPath = EditorUtility.OpenFolderPanel("热更新AB准备目录", "请选择热更新AB准备目录!", "");
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("执行热更新AB准备任务", GUILayout.Width(150.0f)))
        {
            doHotUpdateABPreparationTask();
        }
        GUILayout.EndHorizontal();
        displayHotUpdateABPreparationResult();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("热更新版本号:", GUILayout.Width(100.0f));
        mHotUpdateVersion = EditorGUILayout.TextField(mHotUpdateVersion, GUILayout.Width(100.0f));
        EditorGUILayout.LabelField("热更新资源版本号:", GUILayout.Width(100.0f));
        mHotUpdateResourceVersion = EditorGUILayout.TextField(mHotUpdateResourceVersion, GUILayout.Width(100.0f));
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("热更新目录:", GUILayout.Width(100.0f));
        HotUpdateOutputFolderPath = EditorGUILayout.TextField("", HotUpdateOutputFolderPath);
        if (GUILayout.Button("选择热更新目录", GUILayout.Width(150.0f)))
        {
            HotUpdateOutputFolderPath = EditorUtility.OpenFolderPanel("热更新目录", "请选择热更新目录!", "");
        }
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
            int resourcenumber = 0;
            if (!double.TryParse(mHotUpdateVersion, out versionnumber))
            {
                Debug.LogError($"填写的版本号:{mHotUpdateVersion}无效，请填写有效的版本号!");
                return false;
            }
            if (!int.TryParse(mHotUpdateResourceVersion, out resourcenumber))
            {
                Debug.LogError($"填写的资源版本号:{mHotUpdateResourceVersion}无效，请填写有效的资源版本号!");
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
}