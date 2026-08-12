/*
 * Description:             ResourcePath.cs
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
    /// ResourcePath.cs
    /// 资源路径相关静态类
    /// 处理资源加载多平台路径问题
    /// </summary>
    public static class ResourcePath
    {
        #region AssetBundle
        /// <summary>
        /// AB热更新资源路径
        /// </summary>
        public readonly static string ABHotUpdatePath = Path.Combine(Application.persistentDataPath, "HotUpdate");

#if UNITY_STANDALONE
        /// <summary>
        /// AB包内资源路径
        /// </summary>
        public readonly static string ABBuildinPath = Path.Combine(Application.streamingAssetsPath, "StandaloneWindows64");

        /// <summary>
        /// AB热更新资源路径
        /// </summary>
        public readonly static string ABHotUpdateResourcesPath = Path.Combine(ABHotUpdatePath, "StandaloneWindows64");

        /// <summary>
        /// 依赖信息文件名
        /// </summary>
        public const string DependencyFileName = "StandaloneWindows64";

#elif UNITY_ANDROID
        /// <summary>
        /// AB包内资源路径
        /// </summary>
        public readonly static string ABBuildinPath = Path.Combine(Application.streamingAssetsPath, "Android");

        /// <summary>
        /// AB热更新资源路径
        /// </summary>
        public readonly static string ABHotUpdateResourcesPath = Path.Combine(ABHotUpdatePath, "Android");

        /// <summary>
        /// AB热更新资源缓存路径
        /// </summary>
        public readonly static string ABHotUpdateStagePath = Path.Combine(Application.persistentDataPath, "HotUpdate/Stage");

        /// <summary>
        /// 依赖信息文件名
        /// </summary>
        public const string DependencyFileName = "Android";

#elif UNITY_IOS
        /// <summary>
        /// AB包内资源路径
        /// </summary>
        public readonly static string ABBuildinPath = Path.Combine(Application.streamingAssetsPath, "IOS");

        /// <summary>
        /// AB热更新资源路径
        /// </summary>
        public readonly static string ABHotUpdateResourcesPath = Path.Combine(ABHotUpdatePath, "IOS");

        /// <summary>
        /// AB热更新资源缓存路径
        /// </summary>
        public readonly static string ABHotUpdateStagePath = Application.persistentDataPath + "HotUpdate/Stage";

        /// <summary>
        /// 依赖信息文件名
        /// </summary>
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
        /// 配置文件目录路径(比如版本信息文件)
        /// </summary>
        public const string ConfigFolderPath = "Config/";

        /// <summary>
        /// 游戏版本信息配置文件名
        /// </summary>
        public const string VersionConfigFileName = "VersionConfig";

        /// <summary>
        /// 打印所有路径信息
        /// </summary>
        public static void PrintAllPathInfo()
        {
            DIYLog.Log($"ABBuildinPath : {ABBuildinPath}");
            DIYLog.Log($"ABHotUpdateResourcesPath : {ABHotUpdateResourcesPath}");
            DIYLog.Log($"ABHotUpdateStagePath : {ABHotUpdateStagePath}");
            DIYLog.Log($"DependencyFileName : {DependencyFileName}");
            DIYLog.Log($"ConfigFolderPath : {ConfigFolderPath}");
            DIYLog.Log($"VersionConfigFileName : {VersionConfigFileName}");
        }

        /// <summary>
        /// 获取包内版本配置文件相对路径(无后缀)
        /// </summary>
        /// <returns></returns>
        public static string GetInnerVersionConfigRelativePath()
        {
            return Path.Combine(ConfigFolderPath, VersionConfigFileName);
        }
        
        /// <summary>
        /// 获取包内版本配置文件全路径(含后缀)
        /// </summary>
        /// <returns></returns>
        public static string GetInnerVersionConfigFullPath()
        {
            var versionConfigRelativePath = GetInnerVersionConfigRelativePath();
            return Path.Combine(Application.dataPath, "Resources", $"{versionConfigRelativePath}.json");;
        }

        /// <summary>
        /// 获取包外版本配置文件全路径
        /// </summary>
        /// <returns></returns>
        public static string GetOutterVersionConfigFolderPath()
        {
            return Path.Combine(Application.persistentDataPath, ConfigFolderPath);
        }

        /// <summary>
        /// 获取包外版本配置文件全路径(含后缀)
        /// </summary>
        /// <returns></returns>
        public static string GetOtterVersionConfigFullPath()
        {
            var outterVersionConfigFolderPath = GetOutterVersionConfigFolderPath();
            return Path.Combine(outterVersionConfigFolderPath, $"{VersionConfigFileName}.json");
        }

        /// <summary>
        /// 获取项目目录下的Resources目录全路径
        /// </summary>
        /// <returns></returns>
        public static string GetProjectResourcesFullPath()
        {
            return Path.Combine(Application.dataPath, "Resources");
        }

        /// <summary>
        /// 转换Asset路径到AB路径(一般情况下用不上此方法，慎用)
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns>AB相对路径(带后缀)</returns>
        public static string ChangeAssetPathToABPath(string assetPath)
        {
            var assetPathNoE = PathUtilities.GetPathWithoutPostFix(assetPath);
            return GetABPathWithPostFix(assetPathNoE);
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
        /// <param name="abPath">AB相对路径(带后缀)</param>
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
            if (IsABExitInOutterPath(abPath))
            {
                ResourceLogger.log(string.Format("使用包外资源 : {0}", abPath));
                return Path.Combine(ABHotUpdateResourcesPath, abPath);
            }
            else
            {
                ResourceLogger.log(string.Format("使用包内资源 : {0}", abPath));
                return Path.Combine(ABBuildinPath, abPath);
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
                return Path.Combine(ABHotUpdateResourcesPath, abPath);
            }
            else
            {
                ResourceLogger.log(string.Format("使用包内资源 : {0}", abPath));
                return Path.Combine(ABBuildinPath, abPath);
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
            return $"{Application.dataPath}/{ResourceConstData.AssetBuildInfoAssetRelativeFolderPath}";
        }

        /// <summary>
        /// 获取Asset打包信息文件相对路径
        /// </summary>
        /// <returns></returns>
        public static string GetAssetBuildInfoFileRelativePath()
        {
            return $"Assets/{ResourceConstData.AssetBuildInfoAssetRelativeFolderPath}/{GetAssetBuildInfoAssetName()}.asset";
        }
        
		/// <summary>
		/// 获取Asset打包信息文件AB名
		/// </summary>
		public static string GetAssetBuildInfoABName()
		{
			var assetBuildInfoRelativePath = GetAssetBuildInfoFileRelativePath();
			var assetBuildInfoABPath = ChangeAssetPathToABPath(assetBuildInfoRelativePath);
			return PathUtilities.GetRegularPath(assetBuildInfoABPath.ToLower());
		}

        /// <summary>
        /// 判定指定AB是否存在包外
        /// </summary>
        /// <param name="abPath"></param>
        /// <returns></returns>
        public static bool IsABExitInOutterPath(string abPath)
        {
            var outterABFullPath = Path.Combine(ABHotUpdateResourcesPath, abPath);
            return File.Exists(outterABFullPath);
        }

        /// <summary>
        /// 判定指定AB是否存在包外缓存目录
        /// </summary>
        /// <param name="abPath"></param>
        /// <returns></returns>
        public static bool IsABExitInOutterStagePath(string abPath)
        {
            var outterABFullPath = Path.Combine(ABHotUpdateStagePath, abPath);
            return File.Exists(outterABFullPath);
        }

        /// <summary>
        /// 检查AB包外热更新目录，不存在则创建一个
        /// </summary>
        public static void CheckAndCreateABHotUpdateFolder()
        {
            if (Directory.Exists(ABHotUpdateResourcesPath))
            {
                ResourceLogger.log(string.Format("AB包外目录:{0}已存在!", ABHotUpdateResourcesPath));
                return;
            }
            ResourceLogger.log(string.Format("AB包外目录:{0}不存在，新创建一个!", ABHotUpdateResourcesPath));
            Directory.CreateDirectory(ABHotUpdateResourcesPath);
        }

        /// <summary>
        /// 检查AB包外热更资源缓存目录，不存在则创建一个
        /// </summary>
        public static void CheckAndCreateABHotUpdateStageFolder()
        {
            if (Directory.Exists(ABHotUpdateStagePath))
            {
                ResourceLogger.log(string.Format("AB包外缓存目录:{0}已存在!", ABHotUpdateStagePath));
                return;
            }
            ResourceLogger.log(string.Format("AB包外缓存目录:{0}不存在，新创建一个!", ABHotUpdateStagePath));
            Directory.CreateDirectory(ABHotUpdateStagePath);
        }

        /// <summary>
        /// 获取包内VerifyABInfo.json信息文件相对Resources路径
        /// </summary>
        /// <returns></returns>
        public static string GetVerifyABInfoResRelativePath()
        {
            return ResourceConstData.VerifyABInfoFileName;
        }

        /// <summary>
        /// 获取包内VerifyABInfo.json信息文件路径
        /// </summary>
        /// <returns></returns>
        public static string GetInnerVerifyABInfoFilePath()
        {
            var innerVerifyABInfoFileResRelativePath = GetVerifyABInfoResRelativePath();
            var innerVerifyABInfoFileResPath = Path.Combine("Resources", innerVerifyABInfoFileResRelativePath);
            return Path.Combine(Application.dataPath, innerVerifyABInfoFileResPath);
        }

        /// <summary>
        /// 获取包内ABInfo.json信息文件相对Resources路径
        /// </summary>
        /// <returns></returns>
        public static string GetABInfoResRelativePath()
        {
            return ResourceConstData.ABInfoFileName;
        }

        /// <summary>
        /// 获取包内ABInfo.json信息文件相对Resources路径(不带后缀)
        /// </summary>
        /// <returns></returns>
        public static string GetInnerABInfoFileResRelativePathNoE()
        {
            var innerABInfoFileResRelativePath = GetABInfoResRelativePath();
            return PathUtilities.GetPathWithoutPostFix(innerABInfoFileResRelativePath);
        }

        /// <summary>
        /// 获取包内ABInfo.json信息文件路径
        /// </summary>
        /// <returns></returns>
        public static string GetInnerABInfoFilePath()
        {
            var innerABInfoFileResRelativePath = GetABInfoResRelativePath();
            var innerABInfoFileResPath = Path.Combine("Resources", innerABInfoFileResRelativePath);
            return Path.Combine(Application.dataPath, innerABInfoFileResPath);
        }

        /// <summary>
        /// 获取指定相对路径的包外全路径
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string GetOutterFileFullPath(string relativePath)
        {
            return Path.Combine(ABHotUpdateResourcesPath, relativePath);
        }

        /// <summary>
        /// 获取包外ABInfo.json信息文件路径
        /// </summary>
        /// <returns></returns>
        public static string GetOutterABInfoFilePath()
        {
            return GetOutterFileFullPath(ResourceConstData.ABInfoFileName);
        }

        /// <summary>
        /// 获取指定相对路径的包外缓存全路径
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public static string GetOutterStageFileFullPath(string relativePath)
        {
            return Path.Combine(ABHotUpdateStagePath, relativePath);
        }

        /// <summary>
        /// 获取包外缓存ABInfo.json信息文件路径
        /// </summary>
        /// <returns></returns>
        public static string GetOutterStageABInfoFilePath()
        {
            return GetOutterStageFileFullPath(ResourceConstData.ABInfoFileName);
        }
        #endregion
    }
}