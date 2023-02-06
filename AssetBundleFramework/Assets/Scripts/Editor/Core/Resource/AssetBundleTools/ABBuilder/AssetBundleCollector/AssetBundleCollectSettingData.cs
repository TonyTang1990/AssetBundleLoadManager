/*
 * Description:             AssetBundle搜集设置数据
 * Author:                  TonyTang
 * Create Date:             2023/01/23
 */
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

namespace TResource
{
    /// <summary>
    /// AssetBundle搜集设置数据
    /// </summary>
	public static class AssetBundleCollectSettingData
	{
        /// <summary>
        /// AB搜集设置信息存储目录相对路径
        /// </summary>
        public static string AssetBundleCollectSettingSaveFolderRelativePath = "/AssetBundleCollectSetting";

        /// <summary>
        /// AB搜集设置文件名
        /// </summary>
        public static string AssetBundleCollectSettingFileName = "AssetBundleCollectSetting.asset";

        /// <summary>
        /// AB搜集设置信息文件存储相对路径
        /// </summary>
        public static string AssetBundleCollectSettingFileRelativePath = $"Assets{AssetBundleCollectSettingSaveFolderRelativePath}/{AssetBundleCollectSettingFileName}";

        /// <summary>
        /// AB搜集设置
        /// </summary>
		public static AssetBundleCollectSetting Setting
		{
			get
			{
				if (mSetting == null)
                {
                    LoadSettingData();
                }
                return mSetting;
			}
		}
        /// <summary>
        /// AB搜集设置
        /// </summary>
		private static AssetBundleCollectSetting mSetting = null;

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public static void LoadSettingData()
		{
			// 加载配置文件
			mSetting = AssetDatabase.LoadAssetAtPath<AssetBundleCollectSetting>(AssetBundleCollectSettingFileRelativePath);
			if (mSetting == null)
			{
				Debug.LogWarning($"Create new {nameof(AssetBundleCollectSetting)}.asset : {AssetBundleCollectSettingFileRelativePath}");
				mSetting = ScriptableObject.CreateInstance<AssetBundleCollectSetting>();
                var assetbundlecollectsettingfolderpath = Application.dataPath + AssetBundleCollectSettingSaveFolderRelativePath;
                FolderUtilities.CheckAndCreateSpecificFolder(assetbundlecollectsettingfolderpath);
				AssetDatabase.CreateAsset(Setting, AssetBundleCollectSettingFileRelativePath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}
			else
			{
				Debug.Log($"Load {nameof(AssetBundleCollectSetting)}.asset ok");
			}
            mSetting.UpdateData();

            CheckCollectorSettingValidation();
        }

        /// <summary>
        /// 检查资源搜集有效性
        /// </summary>
        public static bool CheckCollectorSettingValidation()
        {
            // 检查是否有无效的资源搜集设定
            var result = HasInvalideCollectFolderPath();
            if (result)
            {
                Debug.LogError($"有无效的资源搜集设置!");
            }
            return !result;
        }

		/// <summary>
		/// 存储文件
		/// </summary>
		public static void SaveFile()
		{
			if (Setting != null)
			{
				EditorUtility.SetDirty(Setting);
				AssetDatabase.SaveAssets();
			}
		}

        /// <summary>
        /// 是否拥有无效的搜集目录
        /// </summary>
        /// <returns></returns>
        public static bool HasInvalideCollectFolderPath()
        {
            var invalidecollectorlist = new List<Collector>();
            var fullfolderpathprefix = Application.dataPath.Replace("Assets", string.Empty);
            foreach (var collector in Setting.AssetBundleCollectors)
            {
                var collectfolderfullpath = fullfolderpathprefix + collector.CollectFolderPath;
                if (!Directory.Exists(collectfolderfullpath))
                {
                    Debug.LogWarning($"收集路径目录:{collectfolderfullpath}已经不存在了！");
                    continue;
                }
                if(collector.CollectRule == AssetBundleCollectRule.Collect &&
                    (collector.BuildRule == AssetBundleBuildRule.ByConstName && string.IsNullOrEmpty(collector.ConstName)))
                {
                    invalidecollectorlist.Add(collector);
                    Debug.LogError($"资源搜集路径:{collector.CollectFolderPath}设置固定名字搜集策略但未设置有效AB名！");
                }
            }
            return invalidecollectorlist.Count > 0;
        }

        /// <summary>
        /// 是否是有效的可收集目录
        /// </summary>
        /// <param name="folderfullpath"></param>
        /// <param name="relativepath"></param>
        /// <returns></returns>
        public static bool IsValideCollectFolderPath(string folderfullpath)
        {
            var relativefolderpath = PathUtilities.GetAssetsRelativeFolderPath(folderfullpath);
            if (!relativefolderpath.Equals(string.Empty))
            {
                Debug.Log($"relativefolderpath:{relativefolderpath}");
                return Setting.AssetBundleCollectors.Find((collector) =>
                {
                    return collector.CollectFolderPath.Equals(relativefolderpath);
                }) == null;
            }
            else
            {
                Debug.LogError($"目录:{folderfullpath}不是项目有效路径!");
                return false;
            }
        }

        /// <summary>
        /// 添加指定Collector
        /// </summary>
        /// <param name="collector"></param>
        public static bool AddAssetBundleCollector(string folderpath)
        {
            if (IsValideCollectFolderPath(folderpath))
            {
                var relativefolderpath = PathUtilities.GetAssetsRelativeFolderPath(folderpath);
                var collector = new Collector(relativefolderpath);
                Setting.AssetBundleCollectors.Add(collector);
                Setting.SortCollector();
                SaveFile();
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
        public static bool RemoveAssetBundleCollector(Collector collector)
        {
            var result = Setting.AssetBundleCollectors.Remove(collector);
            SaveFile();
            return result;
        }
        
		/// <summary>
		/// 获取所有的打包路径
		/// </summary>
		public static List<string> GetAllCollectDirectory()
		{
			List<string> result = new List<string>();
			for (int i = 0; i < Setting.AssetBundleCollectors.Count; i++)
			{
				Collector wrapper = Setting.AssetBundleCollectors[i];
				if (wrapper.CollectRule == AssetBundleCollectRule.Collect)
                {
                    result.Add(wrapper.CollectFolderPath);
                }
            }
			return result;
		}
        
		/// <summary>
		/// 是否可收集资源
		/// </summary>
		public static bool IsCollectAsset(string assetPath)
		{
            var collector = GetCollectorByAssetPath(assetPath);
			return collector != null ? collector.CollectRule == AssetBundleCollectRule.Collect : false;
		}

		/// <summary>
		/// 获取资源的打包AB名
        /// Note:
        /// 1. 不满足参与打包的Asset返回空字符串
		/// </summary>
		public static string GetAssetBundleName(string assetPath)
		{
            var assetBundleName = string.Empty;
            var collector = GetCollectorByAssetPath(assetPath);
            if(collector == null)
            {
                Debug.LogError($"找不到Asset:{assetPath}的收集器数据，获取AB名失败，请检查打包配置!");
            }
            if(collector.CollectRule == AssetBundleCollectRule.Ignore)
            {
                Debug.LogError($"Asset:{assetPath}设置了不参与打包收集，不应该进入这里，请检查代码!");
            }
            if(collector.BuildRule == AssetBundleBuildRule.ByFilePath)
            {
                // 以文件路径作为打包策略
                // 例如："Assets/Config/test.txt"-- > "Assets/Config/test"
                var extension = Path.GetExtension(assetPath);
                assetBundleName = assetPath.Remove(assetPath.Length - extension.Length);
            }
            else if (collector.BuildRule == AssetBundleBuildRule.ByFolderPath)
            {
                // 以文件夹路径作为打包策略
                // 例如："Assets/Config/test.txt" --> "Assets/Config"
                assetBundleName = Path.GetDirectoryName(assetPath);
            }
            else if (collector.BuildRule == AssetBundleBuildRule.ByFileOrSubFolder)
            {
                // 同层以文件路径作为打包策略，其他以下层目录路径作为打包策略
                var assetFolderPath = Path.GetDirectoryName(assetPath);
                assetFolderPath = PathUtilities.GetRegularPath(assetFolderPath);
                // 在同层目录的文件(假设目标目录是Assets/Conif)
                if (assetFolderPath.Equals(collector.CollectFolderPath))
                {
                    // 例如："Assets/Config/test.txt" --> "Assets/Config/test"
                    assetBundleName = assetPath.Remove(assetPath.LastIndexOf("."));
                }
                else
                {
                    // 例如："Assets/Config/Test/test1.txt" --> "Assets/Config/Test"
                    // 例如："Assets/Config/Test/Test2/test2.txt" --> "Assets/Config/Test"
                    var regulationContent = $"({collector.CollectFolderPath}/)([^/]*)";
                    var regulation = new Regex(regulationContent);
                    var match = regulation.Match(assetPath);
                    var matchPath = match.Value;
                    //Debug.Log($"AssetPath:{assetPath}的MatchPath:{matchPath}");
                    assetBundleName = matchPath;
                }
            }
            else if (collector.BuildRule == AssetBundleBuildRule.ByConstName)
            {
                // 以固定名字作为打包策略
                // 例如："Assets/Config/test.txt" --> "ConstName"
                assetBundleName = collector.ConstName;
            }
            else if (collector.BuildRule == AssetBundleBuildRule.Ignore)
            {
                Debug.LogError($"Asset:{assetPath}设置了不参与打包，不应该进入这里，请检查代码!");
            }
            else
            {
                Debug.LogError($"未支持的打包规则:{collector.BuildRule}，获取Asset:{assetPath}的AB名失败，请检查代码!");
            }
            // AssetBundle打包后都是输出小写路径
            // 所以这里AssetBundle名统一转小写
            return PathUtilities.GetRegularPath(assetBundleName.ToLower());
        }

        /// <summary>
        /// 获取指定Asset路径的收集信息
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static Collector GetCollectorByAssetPath(string assetPath)
        {
            if(string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"传递了空Asset路径，获取AB名失败，请检查代码!");
                return null;
            }
            // AssetBundleCollectors那方是按字母排序的
            // 所以反向匹配是由里往外遍历，匹配第一个就是最里层符合打包策略的设定
            var assetFolderPath = Path.GetDirectoryName(assetPath);
            var regularAssetFolderPath = PathUtilities.GetRegularPath(assetFolderPath);
            for (int i = Setting.AssetBundleCollectors.Count - 1; i > 0; i--)
            {
                Collector wrapper = Setting.AssetBundleCollectors[i];
                if (regularAssetFolderPath.StartsWith(wrapper.CollectFolderPath))
                {
                    return wrapper;
                }
            }
            return null;
        }

        #region 黑名单部分
        /// <summary>
        /// 添加后缀名黑名单
        /// </summary>
        /// <returns></returns>
        public static bool AddPostFixBlackList()
        {
            Setting.BlackListInfo.PostFixBlackList.Add(string.Empty);
            return true;
        }

        /// <summary>
        /// 移除指定索引的后缀名黑名单
        /// </summary>
        /// <returns></returns>
        public static bool RemovePostFixBlackList(int index)
        {
            var exit = Setting.BlackListInfo.PostFixBlackList.Count > index;
            if (exit)
            {
                Setting.BlackListInfo.PostFixBlackList.RemoveAt(index);
            }
            return exit;
        }

        /// <summary>
        /// 添加文件名黑名单
        /// </summary>
        /// <returns></returns>
        public static bool AddFileNameBlackList()
        {
            Setting.BlackListInfo.FileNameBlackList.Add(string.Empty);
            return true;
        }

        /// <summary>
        /// 移除指定索引的文件名黑名单
        /// </summary>
        /// <returns></returns>
        public static bool RemoveFileNameBlackList(int index)
        {
            var exit = Setting.BlackListInfo.FileNameBlackList.Count > index;
            if (exit)
            {
                Setting.BlackListInfo.FileNameBlackList.RemoveAt(index);
            }
            return exit;
        }
        #endregion
    }
}