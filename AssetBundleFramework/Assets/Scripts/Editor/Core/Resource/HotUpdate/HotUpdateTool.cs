/*
 * Description:             HotUpdateTool.cs
 * Author:                  TONYTANG
 * Create Date:             2021//12/19
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
/// HotUpdateTool.cs
/// 热更新工具
/// </summary>
public static class HotUpdateTool
{
    /// <summary>
    /// 文件改变状态
    /// </summary>
    public enum EChangedFileStatus
    {
        Changed = 1,            // 改变
        Delete,                 // 移除
        Add,                    // 增加
    }

    /// <summary>
    /// 执行AB的MD5分析
    /// </summary>
    /// <param name="targetFolderPath">目标分析目录路径</param>
    /// <param name="md5OutputFolderPath">MD5输出路径</param>
    public static bool DoAssetBundleMd5Caculation(string targetFolderPath, string md5OutputFolderPath)
    {
        if (Directory.Exists(targetFolderPath))
        {
            if (Directory.Exists(md5OutputFolderPath))
            {
                VersionConfigModuleManager.Singleton.initVerisonConfigData();
                var resourceversion = VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode;
                var targetplatform = EditorUserBuildSettings.activeBuildTarget;
                // 文件名格式: MD5+版本号+资源版本号+平台+时间戳(年_月_日_时_分_秒)+.txt
                var nowdate = DateTime.Now;
                var md5filename = $"MD5-{Application.version}-{resourceversion}-{targetplatform}-{nowdate.Year}_{nowdate.Month}_{nowdate.Day}_{nowdate.Hour}_{nowdate.Minute}_{nowdate.Second}.txt";
                var md5filefullpath = md5OutputFolderPath + Path.DirectorySeparatorChar + md5filename;
                var abfilespath = Directory.GetFiles(targetFolderPath, "*.*", SearchOption.TopDirectoryOnly).Where(f =>
                    !f.EndsWith(".meta") && !f.EndsWith(".manifest") && !f.Equals("readme.txt")
                );
                // 确保创建最新的
                FileUtilities.DeleteFile(md5filefullpath);
                using (var md5sw = new StreamWriter(md5filefullpath, false, Encoding.UTF8))
                {
                    var md5hash = MD5.Create();
                    //第一行是版本号信息
                    //第二行是资源版本号信息
                    //后面是AB详细(AB全路径+":"+MD5值)
                    md5sw.WriteLine(Application.version);
                    md5sw.WriteLine(resourceversion);
                    foreach (var abfilepath in abfilespath)
                    {
                        var fileMd5 = FileUtilities.GetFileMD5(abfilepath, md5hash);
                        md5sw.WriteLine($"{abfilepath}{ResourceConstData.AssetBundlleInfoSeparater}{fileMd5}");
                    }
                }
                Debug.Log("AB的MD5计算完毕!");
                Debug.Log($"AB的MD5文件路径:{md5filefullpath}");
                return true;
            }
            else
            {
                Debug.LogError("MD5输出目录不存在，请选择有效AB的MD5分析输出目录!");
                return false;
            }
        }
        else
        {
            Debug.LogError("目标AB目录不存在，请选择有效目录!");
            return false;
        }
    }

    /// <summary>
    /// 生成MD5热更新文件
    /// </summary>
    public static void CreateResourceHotUpdateListFile()
    {

    }
}