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
/// AssetBundleMd5Tool.cs
/// AB的Md5对比分析工具
/// </summary>
public class AssetBundleMd5Tool : EditorWindow
{
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
    /// AB的Md5对比数据文件1路径
    /// </summary>
    private string mABMd5Compare1FilePath;

    /// <summary>
    /// AB的Md5对比数据文件2路径
    /// </summary>
    private string mABMd5Compare2FilePath;

    /// <summary>
    /// MD5值有改变的文件名列表
    /// </summary>
    private List<KeyValuePair<string, EChangedFileStatus>> mMD5ChangedABFileNameList = new List<KeyValuePair<string, EChangedFileStatus>>();

    /// <summary>
    /// 滚动位置
    /// </summary>
    private Vector2 uiScrollPos;

    [MenuItem("Tools/Assetbundle/AB MD5计算分析", false)]
    public static void abMd5Analyzing()
    {
        var assetbundlemd5analyzewindow = EditorWindow.GetWindow<AssetBundleMd5Tool>();
        assetbundlemd5analyzewindow.Show();
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        if (GUILayout.Button("选择AB目录"))
        {
            mABFolderPath = EditorUtility.OpenFolderPanel("AB目录", "请选择需要分析的AB所在目录!", "OK");
        }
        GUILayout.Label("AB目录:");
        GUILayout.Label(mABFolderPath);
        if(GUILayout.Button("选择MD5输出目录"))
        {
            mABMd5OutputFolderPath = EditorUtility.OpenFolderPanel("MD5输出目录", "请选择需要AB的MD5分析输出目录!", "OK");
        }
        GUILayout.Label("MD5输出目录:");
        GUILayout.Label(mABMd5OutputFolderPath);
        if (GUILayout.Button("计算目标目录AB的Md5", GUILayout.MaxWidth(150.0f)))
        {
            doAssetBundleMd5Caculation();
        }
        if (GUILayout.Button("选择MD5对比旧文件"))
        {
            mABMd5Compare1FilePath = EditorUtility.OpenFilePanel("MD5对比旧文件", "请选择需要对比的旧MD5分析文件路径!", "OK");
        }
        GUILayout.Label("MD5对比旧文件:");
        GUILayout.Label(mABMd5Compare1FilePath);
        if (GUILayout.Button("选择MD5对比新文件"))
        {
            mABMd5Compare2FilePath = EditorUtility.OpenFilePanel("MD5对比新文件", "请选择需要对比的新MD5分析文件路径!", "OK");
        }
        GUILayout.Label("MD5对比新文件:");
        GUILayout.Label(mABMd5Compare2FilePath);
        if (GUILayout.Button("对比新老版本的MD5", GUILayout.MaxWidth(150.0f)))
        {
            doAssetBundleMd5Comparison();
        }
        GUILayout.EndVertical();
        displayComparisonResult();
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
                            md5sw.WriteLine(abfilename + ":" + sb.ToString());
                        }
                    }
                }
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
    /// 执行MD5对比
    /// </summary>
    private void doAssetBundleMd5Comparison()
    {
        if (File.Exists(mABMd5Compare1FilePath))
        {
            if (File.Exists(mABMd5Compare2FilePath))
            {
                mMD5ChangedABFileNameList.Clear();
                var md51map = new Dictionary<string, string>();
                var md52map = new Dictionary<string, string>();
                using (var md51sr = new StreamReader(mABMd5Compare1FilePath))
                {
                    using (var md52sr = new StreamReader(mABMd5Compare2FilePath))
                    {
                        while(!md51sr.EndOfStream)
                        {
                            var lineinfo = md51sr.ReadLine().Split(':');
                            md51map.Add(lineinfo[0], lineinfo[1]);
                        }
                        while (!md52sr.EndOfStream)
                        {
                            var lineinfo = md52sr.ReadLine().Split(':');
                            md52map.Add(lineinfo[0], lineinfo[1]);
                        }
                    }
                }
                // 进行对比
                foreach(var md51 in md51map)
                {
                    if(md52map.ContainsKey(md51.Key) && !md52map[md51.Key].Equals(md51.Value))
                    {
                        mMD5ChangedABFileNameList.Add(new KeyValuePair<string, EChangedFileStatus>(md51.Key, EChangedFileStatus.Changed));
                    }
                    else
                    {
                        mMD5ChangedABFileNameList.Add(new KeyValuePair<string, EChangedFileStatus>(md51.Key, EChangedFileStatus.Delete));
                    }
                }
                foreach (var md52 in md52map)
                {
                    if (!md51map.ContainsKey(md52.Key))
                    {
                        mMD5ChangedABFileNameList.Add(new KeyValuePair<string, EChangedFileStatus>(md52.Key, EChangedFileStatus.Add));
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
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos);
        foreach (var mdchangedabfilename in mMD5ChangedABFileNameList)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("文件名:" + mdchangedabfilename);
            GUILayout.Space(10);
            GUILayout.Label("状态:" + mdchangedabfilename);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
}