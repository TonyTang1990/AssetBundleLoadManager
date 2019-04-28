/*
 * Description:             AssetBundlePath.cs
 * Author:                  TONYTANG
 * Create Date:             2018//09/28
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// AssetBundlePath.cs
/// AB资源路径相关静态类，处理多平台路径问题
/// </summary>
public static class AssetBundlePath {

    #region AssetBundle
#if UNITY_STANDALONE
    /// <summary> AB包内资源路径 /// </summary>
    public readonly static string ABBuildinPath = Application.streamingAssetsPath + "/PC/";
    /// <summary> AB热更新资源路径 /// </summary>
    public readonly static string ABHotUpdatePath = Application.persistentDataPath + "/PC/";
    /// <summary> 依赖信息文件名 /// </summary>
    public const string DependencyFileName = "PC";
#elif UNITY_ANDROID
    /// <summary> AB包内资源路径 /// </summary>
    public readonly static string ABBuildinPath = Application.streamingAssetsPath + "/Android/";
    /// <summary> AB热更新资源路径 /// </summary>
    public readonly static string ABHotUpdatePath = Application.persistentDataPath + "/Android/";
    /// <summary> 依赖信息文件名 /// </summary>
    public const string DependencyFileName = "Android";
#elif UNITY_IOS
    /// <summary> AB包内资源路径 /// </summary>
    public readonly static string ABBuildinPath = Application.streamingAssetsPath + "/IOS/";
    /// <summary> AB热更新资源路径 /// </summary>
    public readonly static string ABHotUpdatePath = Application.persistentDataPath + "/IOS/";
    /// <summary> 依赖信息文件名 /// </summary>
    public const string DependencyFileName = "IOS";
#endif

    /// <summary> 依赖信息Asset名 /// </summary>
    public const string DependencyAssetName = "AssetBundleManifest";

    /// <summary>
    /// 打印所有路径信息
    /// </summary>
    public static void PrintAllPathInfo()
    {
        DIYLog.Log(string.Format("ABBuildinPath : {0}", ABBuildinPath));
        DIYLog.Log(string.Format("ABHotUpdatePath : {0}", ABHotUpdatePath));
        DIYLog.Log(string.Format("DependencyFileName : {0}", DependencyFileName));
        DIYLog.Log(string.Format("DependencyAssetName : {0}", DependencyAssetName));
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
        //if(包外有)        // Application.persistentDataPath
        //{ 
        //    返回包外资源路径
        //}
        //else              // Application.streamingAssetsPath
        //{ 
        //    返回包内资源路径
        //}
        var outterabfullpath = ABHotUpdatePath + abname;
        if (IsABExitInOutterPath(abname))
        {
            ResourceLogger.log(string.Format("使用包外资源 : {0}", abname));
            return outterabfullpath;
        }
        else
        {
            ResourceLogger.log(string.Format("使用包内资源 : {0}", abname));
            return ABBuildinPath + abname;
        }
    }

    /// <summary>
    /// 判定指定AB是否存在包外
    /// </summary>
    /// <param name="abname"></param>
    /// <returns></returns>
    public static bool IsABExitInOutterPath(string abname)
    {
        var outterabfullpath = ABHotUpdatePath + abname;
        if (File.Exists(outterabfullpath))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 检查AB包外目录，不存在则创建一个
    /// </summary>
    public static void CheckAndCreateABOutterPathFolder()
    {
        if(Directory.Exists(ABHotUpdatePath))
        {
            ResourceLogger.log(string.Format("AB包外目录:{0}已存在!", ABHotUpdatePath));
        }
        else
        {
            ResourceLogger.log(string.Format("AB包外目录:{0}不存在，新创建一个!", ABHotUpdatePath));
            Directory.CreateDirectory(ABHotUpdatePath);
        }
    }
    #endregion
}
