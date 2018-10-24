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
    private static string ABBuildinPath = Application.streamingAssetsPath + "/PC/";
#elif UNITY_ANDROID
    private static string ABBuildinPath = Application.streamingAssetsPath + "/Android/";
#elif UNITY_IOS
    private static string ABBuildinPath = Application.streamingAssetsPath + "/IOS/";
#endif

    /// <summary> 依赖信息文件名 /// </summary>
    public const string DependencyFileName = "allabdep";

    /// <summary>
    /// 获取AB加载路径
    /// </summary>
    /// <param name="resName"></param>
    /// <param name="wwwPath"></param>
    /// <returns></returns>
    public static string GetABPath()
    {
        return ABBuildinPath;
    }
    #endregion
}
