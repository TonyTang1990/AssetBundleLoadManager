/*
 * Description:             AssetBundlePath.cs
 * Author:                  TONYTANG
 * Create Date:             2018//09/28
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundlePath.cs
    /// 路径相关静态类
    /// 处理资源加载多平台路径问题
    /// </summary>
    public static class AssetBundlePath
    {

        #region AssetBundle
#if UNITY_STANDALONE
    /// <summary> AB包内资源路径 /// </summary>
    public readonly static string ABBuildinPath = Application.streamingAssetsPath + "/StandaloneWindows64/";
    /// <summary> AB热更新资源路径 /// </summary>
    public readonly static string ABHotUpdatePath = Application.persistentDataPath + "/HotUpdate/StandaloneWindows64/";
    /// <summary> 依赖信息文件名 /// </summary>
    public const string DependencyFileName = "StandaloneWindows64";
#elif UNITY_ANDROID
        /// <summary> AB包内资源路径 /// </summary>
        public readonly static string ABBuildinPath = Application.streamingAssetsPath + "/Android/";
        /// <summary> AB热更新资源路径 /// </summary>
        public readonly static string ABHotUpdatePath = Application.persistentDataPath + "/HotUpdate/Android/";
        /// <summary> 依赖信息文件名 /// </summary>
        public const string DependencyFileName = "Android";
#elif UNITY_IOS
    /// <summary> AB包内资源路径 /// </summary>
    public readonly static string ABBuildinPath = Application.streamingAssetsPath + "/IOS/";
    /// <summary> AB热更新资源路径 /// </summary>
    public readonly static string ABHotUpdatePath = Application.persistentDataPath + "/HotUpdate/IOS/";
    /// <summary> 依赖信息文件名 /// </summary>
    public const string DependencyFileName = "IOS";
#endif
        /// <summary>
        /// Windows的AssetBundle文件后缀名
        /// </summary>
        public const string WindowAssetBundlePostFix = "window";
        /// <summary>
        /// Android的AssetBundle文件后缀名
        /// </summary>
        public const string AndroidAssetBundlePostFix = "android";
        /// <summary>
        /// IOS的AssetBundle文件后缀名
        /// </summary>
        public const string IOSAssetBundlePostFix = "ios";

        /// <summary>
        /// Windows的Asset打包信息文件名
        /// </summary>
        public const string WindowAssetBuildInfoAssetName = "AssetBuildInfoWindow";
        /// <summary>
        /// Android的Asset打包信息文件名
        /// </summary>
        public const string AndroidAssetBuildInfoAssetName = "AssetBuildInfoAndroid";
        /// <summary>
        /// IOS的Asset打包信息文件名
        /// </summary>
        public const string IOSAssetBuildInfoAssetName = "AssetBuildInfoIOS";

        /// <summary>
        /// 打印所有路径信息
        /// </summary>
        public static void PrintAllPathInfo()
        {
            DIYLog.Log(string.Format("ABBuildinPath : {0}", ABBuildinPath));
            DIYLog.Log(string.Format("ABHotUpdatePath : {0}", ABHotUpdatePath));
            DIYLog.Log(string.Format("DependencyFileName : {0}", DependencyFileName));
        }

        /// <summary>
        /// 转换Asset路径到AB路径(一般情况下用不上此方法，慎用)
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static string ChangeAssetPathToABPath(string assetPath)
        {
            return PathUtilities.GetPathWithoutPostFix(assetPath);
        }

        /// <summary>
        /// 获取AB带后缀加载路径
        /// Note:
		/// 1. 因为Scriptable Build Pipeline不支持变体功能，所以这里打算统一不采用变体名功能，改为AB名自带后缀的方式
        /// </summary>
        /// <param name="abPath"></param>
        /// <returns></returns>
        public static string GetABPathWithPostFix(string abPath)
        {
            var assetBundlePostFix = GetAssetBundlePostFix();
            if (!string.IsNullOrEmpty(assetBundlePostFix))
            {
                return $"{abPath}.{assetBundlePostFix}";
            }
            return abPath;
        }

        /// <summary>
        /// 获取AB加载全路径(含热更加载逻辑判定)
        /// </summary>
        /// <param name="abPath"></param>
        /// <returns></returns>
        public static string GetABLoadFullPath(string abPath)
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
            var abPathWithPostFix = GetABPathWithPostFix(abPath);
            if (IsABExitInOutterPath(abPathWithPostFix))
            {
                ResourceLogger.log(string.Format("使用包外资源 : {0}", abPathWithPostFix));
                return $"{ABHotUpdatePath}{abPathWithPostFix}";
            }
            else
            {
                ResourceLogger.log(string.Format("使用包内资源 : {0}", abPathWithPostFix));
                return $"{ABBuildinPath}{abPathWithPostFix}";
            }
        }
        /// <summary>
        /// 获取AB加载全路径(不含AssetBundle后缀名,含热更加载逻辑判定)
        /// </summary>
        /// <param name="abPath"></param>
        /// <returns></returns>
        public static string GetABLoadFullPathNoPostFix(string abPath)
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
            if (IsABExitInOutterPath(abPath))
            {
                ResourceLogger.log(string.Format("使用包外资源 : {0}", abPath));
                return $"{ABHotUpdatePath}{abPath}"; ;
            }
            else
            {
                ResourceLogger.log(string.Format("使用包内资源 : {0}", abPath));
                return $"{ABBuildinPath}{abPath}";
            }
        }

        /// <summary>
        /// 获取AB后缀名
        /// </summary>
        /// <returns></returns>
        public static string GetAssetBundlePostFix()
        {
#if UNITY_STANDALONE_WIN
            return WindowAssetBundlePostFix;
#elif UNITY_ANDROID
            return AndroidAssetBundlePostFix;
#elif UNITY_IOS
            return IOSAssetBundlePostFix;
#endif
            Debug.LogError($"不支持的平台:{Application.platform},获取AB后缀名失败!");
            return string.Empty;
        }

        /// <summary>
        /// 获取Asset打包信息文件名
        /// </summary>
        /// <returns></returns>
        public static string GetAssetBuildInfoAssetName()
        {
#if UNITY_STANDALONE
            return WindowAssetBuildInfoAssetName;
#elif UNITY_ANDROID
            return AndroidAssetBuildInfoAssetName;
#elif UNITY_IOS
            return IOSAssetBuildInfoAssetName;
#endif
            Debug.LogError($"不支持的平台:{Application.platform},获取Asset打包信息文件名失败!");
            return string.Empty;
        }

        /// <summary>
        /// 获取Asset打包信息Asset所在目录全路径
        /// </summary>
        /// <returns></returns>
        public static string GetAssetBuildInfoFolderFullPath()
        {
            return $"{Application.dataPath}/{ResourceConstData.AssetBuildInfoAssetRelativePath}";
        }

        /// <summary>
        /// 获取Asset打包信息文件相对路径
        /// </summary>
        /// <returns></returns>
        public static string GetAssetBuildInfoFileRelativePath()
        {
            return $"Assets/{ResourceConstData.AssetBuildInfoAssetRelativePath}/{GetAssetBuildInfoAssetName()}.asset";
        }

        /// <summary>
        /// 判定指定AB是否存在包外
        /// </summary>
        /// <param name="abPath"></param>
        /// <returns></returns>
        public static bool IsABExitInOutterPath(string abPath)
        {
            var outterABFullPath = ABHotUpdatePath + abPath;
            if (File.Exists(outterABFullPath))
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
            if (Directory.Exists(ABHotUpdatePath))
            {
                ResourceLogger.log(string.Format("AB包外目录:{0}已存在!", ABHotUpdatePath));
            }
            else
            {
                ResourceLogger.log(string.Format("AB包外目录:{0}不存在，新创建一个!", ABHotUpdatePath));
                Directory.CreateDirectory(ABHotUpdatePath);
            }
        }

        /// <summary>
        /// 获取包内AssetBundle的MD5信息文件(AssetBundleMd5InfoFileName.txt)
        /// </summary>
        /// <returns></returns>
        public static string GetInnerAssetBundleMd5FilePath()
        {
            return Path.Combine(Application.dataPath, $"Resources/{ResourceConstData.AssetBundleMd5InfoFileName}");
        }
        #endregion
    }
}