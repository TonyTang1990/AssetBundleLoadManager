/*
 * Description:             AssetBundleCollectSetting.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/25
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// 单个搜集打包设定
/// </summary>
[Serializable]
public class Collector
{
    /// <summary>
    /// 搜集设定相对目录路径
    /// </summary>
    public string CollectFolderPath;

    /// <summary>
    /// 搜集打包规则
    /// </summary>
    public AssetBundleCollectRule BuildRule;

    private Collector()
    {

    }

    public Collector(string collectrelativefolderpath, AssetBundleCollectRule buildrule = AssetBundleCollectRule.LoadByFilePath)
    {
        CollectFolderPath = collectrelativefolderpath;
        BuildRule = buildrule;
    }
}

/// <summary>
/// AssetBundleCollectSetting.cs
/// AB打包搜集规则数据序列化类
/// </summary>
public class AssetBundleCollectSetting : ScriptableObject
{
    /// <summary>
    /// 所有的AB搜集信息
    /// </summary>
    public List<Collector> AssetBundleCollectors = new List<Collector>();
    
    /// <summary>
    /// 是否拥有无效的搜集目录
    /// </summary>
    /// <returns></returns>
    public bool HasInvalideCollectFolderPath()
    {
        var invalidecollectorlist = new List<Collector>();
        var fullfolderpathprefix = Application.dataPath.Replace("Assets", string.Empty);
        foreach (var collector in AssetBundleCollectors)
        {
            var collectfolderfullpath = fullfolderpathprefix + collector.CollectFolderPath;
            if(!Directory.Exists(collectfolderfullpath))
            {
                invalidecollectorlist.Add(collector);
            }
        }
        foreach(var invalidecollector in invalidecollectorlist)
        {
            Debug.Log($"无效的资源搜集路径:{invalidecollector.CollectFolderPath},请检查资源搜集设置!");
        }
        return invalidecollectorlist.Count > 0;
    }
    
    /// <summary>
    /// 添加指定Collector
    /// </summary>
    /// <param name="collector"></param>
    public bool AddAssetBundleCollector(string folderpath)
    {
        if(IsValideCollectFolderPath(folderpath))
        {
            var relativefolderpath = GetRelativeFolderPath(folderpath);
            var collector = new Collector(relativefolderpath);
            AssetBundleCollectors.Add(collector);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 移除指定Collector
    /// </summary>
    /// <param name="collector"></param>
    /// <returns></returns>
    public bool RemoveAssetBundleCollector(Collector collector)
    {
        return AssetBundleCollectors.Remove(collector);
    }

    /// <summary>
    /// 获取资源搜集相对路径
    /// </summary>
    /// <param name="folderfullpath"></param>
    /// <returns></returns>
    public string GetRelativeFolderPath(string folderfullpath)
    {
        var projectpathprefix = Application.dataPath.Replace("Assets", string.Empty);
        if (folderfullpath.StartsWith(projectpathprefix))
        {
            var relativefolderpath = folderfullpath.Replace(projectpathprefix, string.Empty);
            return relativefolderpath;
        }
        else
        {
            Debug.LogError("目录:{folderfullpath}不是项目有效路径,获取相对路径失败!");
            return string.Empty;
        }
    }

    /// <summary>
    /// 是否是有效的可收集目录
    /// </summary>
    /// <param name="folderfullpath"></param>
    /// <param name="relativepath"></param>
    /// <returns></returns>
    private bool IsValideCollectFolderPath(string folderfullpath)
    {
        var relativefolderpath = GetRelativeFolderPath(folderfullpath);
        if(!relativefolderpath.Equals(string.Empty))
        {
            Debug.Log($"relativefolderpath:{relativefolderpath}");
            return AssetBundleCollectors.Find((collector) =>
            {
                return collector.CollectFolderPath.Equals(relativefolderpath);
            }) == null;
        }
        else
        {
            Debug.LogError("目录:{folderfullpath}不是项目有效路径!");
            return false;
        }
    }
}