/*
 * Description:             AssetBundlePath.cs
 * Author:                  TONYTANG
 * Create Date:             2018//09/28
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetBundlePath.cs
/// AB资源路径相关静态类，处理多平台路径问题
/// </summary>
public static class AssetBundlePath {

    #region AssetBundle
#if UNITY_STANDALONE
    /// <summary> AB包内资源路径 /// </summary>
    private static string ABBuildinPath = Application.streamingAssetsPath + "/PC/";
    /// <summary> AB热更新资源路径 /// </summary>
    private static string ABHotUpdatePath = Application.persistentDataPath + "/PC/";
    /// <summary> 依赖信息文件名 /// </summary>
    public const string DependencyFileName = "PC";
#elif UNITY_ANDROID
    /// <summary> AB包内资源路径 /// </summary>
    private static string ABBuildinPath = Application.streamingAssetsPath + "/Android/";
    /// <summary> AB热更新资源路径 /// </summary>
    private static string ABHotUpdatePath = Application.persistentDataPath + "/Android/";
    /// <summary> 依赖信息文件名 /// </summary>
    public const string DependencyFileName = "Android";
#elif UNITY_IOS
    /// <summary> AB包内资源路径 /// </summary>
    private static string ABBuildinPath = Application.streamingAssetsPath + "/IOS/";
    /// <summary> AB热更新资源路径 /// </summary>
    private static string ABHotUpdatePath = Application.persistentDataPath + "/IOS/";
    /// <summary> 依赖信息文件名 /// </summary>
    public const string DependencyFileName = "IOS";
#endif

    /// <summary> 依赖信息Asset名 /// </summary>
    public const string DependencyAssetName = "AssetBundleManifest";

    /// <summary>
    /// 获取AB包内加载路径
    /// </summary>
    /// <param name="resName"></param>
    /// <param name="wwwPath"></param>
    /// <returns></returns>
    public static string GetABInnerPath()
    {
        return ABBuildinPath;
    }

    /// <summary>
    /// 获取AB加载全路径(含热更加载逻辑判定)
    /// </summary>
    /// <param name="abname"></param>
    /// <returns></returns>
    public static string GetABLoadFullPath(string abname)
    {
        //TODO:
        //热更逻辑路径判定
        //暂时默认返回包内路径
        return ABBuildinPath + abname;
    }
    #endregion
}
