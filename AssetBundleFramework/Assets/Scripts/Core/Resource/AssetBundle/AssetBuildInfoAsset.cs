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
    /// Asset名字(含后缀)
    /// </summary>
    public string AssetName;

    /// <summary>
    /// Asset路径(含后缀)
    /// </summary>
    public string AssetPath;

    /// <summary>
    /// AB路径(含后缀)
    /// </summary>
    public string ABPath;

    /// <summary>
    /// AB变体路径(暂未支持)
    /// </summary>
    public string ABVariantPath;

    public BuildAssetInfo(string assetPath, string abPath, string abVariantPath)
    {
        AssetPath = assetPath;
        AssetName = Path.GetFileName(assetPath);
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
    /// Note:
    /// 仅包含需要支持主动加载的资源Asset信息
    /// </summary>
    [Header("打包Asset信息列表")]
    public List<BuildAssetInfo> BuildAssetInfoList;

    /// <summary>
    /// Asset AB打包信息映射Map(Key为Asset路径(含后缀)，Value为对应Asset打包信息)
    /// </summary>
    private Dictionary<string, BuildAssetInfo> mBuildPathAssetInfoMap;

    /// <summary>
    /// Asset AB打包信息映射Map(Key为Asset名(含后缀)，Value为对应Asset路径)
    /// </summary>
    private Dictionary<string, string> mBuildAssetNameAndPathMap;

    public AssetBuildInfoAsset()
    {
        BuildAssetInfoList = new List<BuildAssetInfo>();
        mBuildPathAssetInfoMap = new Dictionary<string, BuildAssetInfo>();
        mBuildAssetNameAndPathMap = new Dictionary<string, string>();
    }

    /// <summary>
    /// 初始化相关数据
    /// </summary>
    public void Init()
    {
        mBuildPathAssetInfoMap.Clear();
        mBuildAssetNameAndPathMap.Clear();
        if(BuildAssetInfoList == null)
        {
            return;
        }
        for (int i = 0, length = BuildAssetInfoList.Count; i < length; i++)
        {
            var buildAssetInfo = BuildAssetInfoList[i];
            if(!mBuildPathAssetInfoMap.ContainsKey(buildAssetInfo.AssetPath))
            {
                mBuildPathAssetInfoMap.Add(buildAssetInfo.AssetPath, buildAssetInfo);
            }
            else
            {
                Debug.LogError($"打包AssetBundle信息里有同名Asset路径:{buildAssetInfo.AssetPath}，理论上打包测已经做了检测不应该发生，请检查代码!");
            }
            if(!mBuildAssetNameAndPathMap.TryGetValue(buildAssetInfo.AssetName, out var preAssetPath))
            {
                mBuildAssetNameAndPathMap.Add(buildAssetInfo.AssetName, buildAssetInfo.AssetPath);
            }
            else
            {
                Debug.LogError($"打包AssetBundle信息里有同名Asset名:{buildAssetInfo.AssetPath}和{preAssetPath}，理论上打包测已经做了检测不应该发生，请检查代码!");
            }
        }
    }

    /// <summary>
    /// 获取指定Asset路径的AB路径
    /// </summary>
    /// <param name="assetpath"></param>
    /// <returns></returns>
    public string GetAssetPathABPath(string assetpath)
    {
        BuildAssetInfo buildAssetInfo;
        if(mBuildPathAssetInfoMap.TryGetValue(assetpath, out buildAssetInfo))
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
    public string GetAssetPathABVariantPath(string assetpath)
    {
        BuildAssetInfo buildAssetInfo;
        if (mBuildPathAssetInfoMap.TryGetValue(assetpath, out buildAssetInfo))
        {
            return buildAssetInfo.ABVariantPath;
        }
        else
        {
            Debug.LogError($"找不到Asset路径:{assetpath}的AB变体名字信息!");
            return null;
        }
    }

    /// <summary>
    /// 获取指定Asset名(含后缀)的Asset路径
    /// </summary>
    /// <param name="assetName"></param>
    /// <returns></returns>
    public string GetAssetNamePath(string assetName)
    {
        if (mBuildAssetNameAndPathMap.TryGetValue(assetName, out var assetPath))
        {
            return assetPath;
        }
        else
        {
            Debug.LogError($"AB打包Asset信息里找不到Asset名:{assetName}的Asset路径信息!");
            return null;
        }
    }
}