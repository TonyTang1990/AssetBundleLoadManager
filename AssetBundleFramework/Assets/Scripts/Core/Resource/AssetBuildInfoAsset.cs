/*
 * Description:             AssetBundleBuildInfoAsset.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/17
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 打包Asset信息
/// </summary>
[Serializable]
public class BuildAssetInfo
{
    /// <summary>
    /// Asset路径(不含后缀)
    /// </summary>
    public string AssetPath;

    /// <summary>
    /// AB路径
    /// </summary>
    public string ABPath;

    /// <summary>
    /// AB变体路径(暂未支持)
    /// </summary>
    public string ABVariantPath;

    public BuildAssetInfo(string assetPath, string abPath, string abVariantPath)
    {
        AssetPath = assetPath;
        ABPath = abPath;
        ABVariantPath = abVariantPath;
    }
}

/// <summary>
/// 打包AssetBundle信息
/// </summary>
[Serializable]
public class BuildAssetBundleInfo
{
    /// <summary>
    /// AB路径信息
    /// </summary>
    public string ABPath;

    /// <summary>
    /// 依赖的AB路径列表
    /// </summary>
    public List<string> DepABPathList;

    public BuildAssetBundleInfo(string abpath)
    {
        ABPath = abpath;
        DepABPathList = new List<string>();
    }

    public BuildAssetBundleInfo(string abpath, List<string> depabpathlist)
    {
        ABPath = abpath;
        DepABPathList = depabpathlist;
    }
}

/// <summary>
/// AssetBundleBuildInfoAsset.cs
/// AssetBundle打包信息Asset
/// </summary>
public class AssetBuildInfoAsset : ScriptableObject
{
    /// <summary>
    /// 打包Asset信息列表
    /// </summary>
    [Header("打包Asset信息列表")]
    public List<BuildAssetInfo> BuildAssetInfoList;

    /// <summary>
    /// 打包AssetBundle信息信息列表(现有设计采用AB打包出来的Manifest文件访问依赖信息)
    /// </summary>
    //[Header("AB打包信息信息列表")]
    //public List<AssetBundleBuildInfo> AssetBundleBuildInfoList;

    /// <summary>
    /// Asset AB打包信息映射Map(Key为Asset路径，Value为对应Asset打包信息)
    /// </summary>
    private Dictionary<string, BuildAssetInfo> mBuildAssetInfoMap;

    /// <summary>
    /// AB路径依赖信息映射Map(Key为AB路径，Value为对应AB路径对应的依赖信息)
    /// </summary>
    //public Dictionary<string, string[]> ABPathDepMap
    //{
    //    get;
    //    private set;
    //}

    public AssetBuildInfoAsset()
    {
        BuildAssetInfoList = new List<BuildAssetInfo>();
        //AssetBundleBuildInfoList = new List<BuildAssetBundleInfo>();
        mBuildAssetInfoMap = new Dictionary<string, BuildAssetInfo>();
        //ABPathDepMap = new Dictionary<string, string[]>();
    }

    /// <summary>
    /// 初始化相关数据
    /// </summary>
    public void init()
    {
        mBuildAssetInfoMap.Clear();
        for (int i = 0, length = BuildAssetInfoList.Count; i < length; i++)
        {
            if(!mBuildAssetInfoMap.ContainsKey(BuildAssetInfoList[i].AssetPath))
            {
                mBuildAssetInfoMap.Add(BuildAssetInfoList[i].AssetPath, BuildAssetInfoList[i]);
            }
            else
            {
                // 忽略重复AssetPath理论上没有关系，无论是LoadByFile还是LoadByFolder还是LoadByConstName
                // 同目录同名文件理论上都在同一个AB里，最后通过泛型匹配加载即可
                Debug.LogWarning($"同目录下有同名文件:{BuildAssetInfoList[i].AssetPath},忽略同名文件避免重复Key问题!");
            }
        }
        //ABPathDepMap.Clear();
        //for (int i = 0, length = AssetBundleBuildInfoList.Count; i < length; i++)
        //{
        //    if (!ABPathDepMap.ContainsKey(AssetBundleBuildInfoList[i].ABPath))
        //    {
        //        ABPathDepMap.Add(AssetBundleBuildInfoList[i].ABPath, AssetBundleBuildInfoList[i].DepABPathList.ToArray());
        //    }
        //}
    }

    /// <summary>
    /// 获取指定Asset路径的AB路径
    /// </summary>
    /// <param name="assetpath"></param>
    /// <returns></returns>
    public string getAssetABPath(string assetpath)
    {
        BuildAssetInfo buildAssetInfo;
        if(mBuildAssetInfoMap.TryGetValue(assetpath, out buildAssetInfo))
        {
            return buildAssetInfo.ABPath;
        }
        else
        {
            Debug.LogError($"找不到Asset路径:{assetpath}的AB名字信息!");
            return null;
        }
    }

    /// <summary>
    /// 获取指定Asset路径的AB变体路径
    /// </summary>
    /// <param name="assetpath"></param>
    /// <returns></returns>
    public string getAssetABVariantPath(string assetpath)
    {
        BuildAssetInfo buildAssetInfo;
        if (mBuildAssetInfoMap.TryGetValue(assetpath, out buildAssetInfo))
        {
            return buildAssetInfo.ABVariantPath;
        }
        else
        {
            Debug.LogError($"找不到Asset路径:{assetpath}的AB变体名字信息!");
            return null;
        }
    }

    ///// <summary>
    ///// 获取AB路径所依赖的AB路径信息数组
    ///// </summary>
    ///// <param name="abpath"></param>
    ///// <returns></returns>
    //public string[] getABPathDepPaths(string abpath)
    //{
    //    string[] abdeppaths = null;
    //    if (ABPathDepMap.TryGetValue(abpath, out abdeppaths))
    //    {
    //        return abdeppaths;
    //    }
    //    else
    //    {
    //        Debug.LogError($"找不到AB路径:{abpath}的AB依赖路径信息!");
    //        return null;
    //    }
    //}

    ///// <summary>
    ///// 指定路径是否是AB路径
    ///// </summary>
    ///// <param name="respath"></param>
    ///// <returns></returns>
    //public bool isABPath(string respath)
    //{
    //    return ABPathDepMap.ContainsKey(respath);
    //}
}