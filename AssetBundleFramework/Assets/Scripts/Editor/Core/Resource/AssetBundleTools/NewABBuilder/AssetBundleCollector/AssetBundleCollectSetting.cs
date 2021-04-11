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
using MotionFramework.Editor;

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
    /// 收集规则
    /// </summary>
    public EAssetBundleCollectRule CollectRule = EAssetBundleCollectRule.Collect;

    /// <summary>
    /// 搜集打包规则
    /// </summary>
    public EAssetBundleBuildRule BuildRule;

    public Collector()
    {

    }

    public Collector(string collectrelativefolderpath, EAssetBundleCollectRule collectrule = EAssetBundleCollectRule.Collect, EAssetBundleBuildRule buildrule = EAssetBundleBuildRule.LoadByFilePath)
    {
        CollectFolderPath = collectrelativefolderpath;
        CollectRule = collectrule;
        BuildRule = buildrule;
    }

    /// <summary>
    /// 获取当前Collect对应的搜集类名
    /// </summary>
    /// <returns></returns>
    public string GetCollectorClassName()
    {
        if(BuildRule == EAssetBundleBuildRule.LoadByFilePath)
        {
            return typeof(LabelByFilePath).Name;
        }
        else if(BuildRule == EAssetBundleBuildRule.LoadByFolderPath)
        {
            return typeof(LabelByFolderPath).Name;
        }
        else
        {
            return typeof(LabelNone).Name;
        }
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
}