//--------------------------------------------------
// Motion Framework
// Copyright©2019-2020 何冠峰
// Licensed under the MIT license
//--------------------------------------------------
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MotionFramework.Editor
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
        /// 收集器类型集合
        /// </summary>
        private static readonly Dictionary<string, System.Type> _cacheTypes = new Dictionary<string, System.Type>();

		/// <summary>
		/// 收集器实例集合
		/// </summary>
		private static readonly Dictionary<string, IAssetCollector> _cacheCollector = new Dictionary<string, IAssetCollector>();

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
        private static void LoadSettingData()
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

			// 清空缓存集合
			_cacheTypes.Clear();
			_cacheCollector.Clear();

            // 获取所有资源收集器类型
            List<Type> types = new List<Type>();
			types.Add(typeof(LabelNone));
			types.Add(typeof(LabelByFilePath));
            types.Add(typeof(LabelByFolderPath));
            types.Add(typeof(LableByConstName));
            types.Add(typeof(LabelByFileAndSubFolderPath));
            for (int i = 0; i < types.Count; i++)
            {
                Type type = types[i];
                if (_cacheTypes.ContainsKey(type.Name) == false)
                {
                    _cacheTypes.Add(type.Name, type);
                }
            }

            CheckCollectorSettingValidation();
        }

        /// <summary>
        /// 检查资源搜集有效性
        /// </summary>
        public static bool CheckCollectorSettingValidation()
        {
            // 检查是否有无效的资源搜集设定
            var result = AssetBundleCollectSettingData.HasInvalideCollectFolderPath();
            if (result)
            {
                Debug.LogError($"有无效的资源搜集设置!");
            }
            return result;
        }

        /// <summary>
        /// 获取所有收集器名称列表
        /// </summary>
        public static List<string> GetCollectorNames()
		{
			if (mSetting == null)
				LoadSettingData();

			List<string> names = new List<string>();
			foreach (var pair in _cacheTypes)
			{
				names.Add(pair.Key);
			}
			return names;
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
                    invalidecollectorlist.Add(collector);
                }
            }
            foreach (var invalidecollector in invalidecollectorlist)
            {
                Debug.Log($"无效的资源搜集路径:{invalidecollector.CollectFolderPath},请检查资源搜集设置!");
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
        /// 是否收集该资源
        /// </summary>
        public static bool IsCollectAsset(string assetPath)
        {
            for (int i = 0; i < Setting.AssetBundleCollectors.Count; i++)
            {
                Collector wrapper = Setting.AssetBundleCollectors[i];
                if (wrapper.CollectRule == EAssetBundleCollectRule.Collect)
                {
                    if (assetPath.StartsWith(wrapper.CollectFolderPath))
                    {
                        return true;
                    }
                }
            }
            return false;
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
				if (wrapper.CollectRule == EAssetBundleCollectRule.Collect)
                {
                    result.Add(wrapper.CollectFolderPath);
                }
            }
			return result;
		}
        
		/// <summary>
		/// 是否忽略该资源
		/// </summary>
		public static bool IsIgnoreAsset(string assetpath)
		{
			for (int i = 0; i < Setting.AssetBundleCollectors.Count; i++)
			{
				Collector wrapper = Setting.AssetBundleCollectors[i];
				if (wrapper.CollectRule == EAssetBundleCollectRule.Ignore)
				{
					if (assetpath.StartsWith(wrapper.CollectFolderPath))
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 获取资源的打包标签
		/// </summary>
		public static string GetAssetBundleLabel(string assetpath)
		{
			// 注意：一个资源有可能被多个收集器覆盖
			List<Collector> filterWrappers = new List<Collector>();
			for (int i = 0; i < Setting.AssetBundleCollectors.Count; i++)
			{
				Collector wrapper = Setting.AssetBundleCollectors[i];
				if (assetpath.StartsWith(wrapper.CollectFolderPath))
				{
					filterWrappers.Add(wrapper);
				}
			}

			// 我们使用路径最深层的收集器
			Collector findWrapper = null;
			for (int i = 0; i < filterWrappers.Count; i++)
			{
				Collector wrapper = filterWrappers[i];
				if (findWrapper == null)
				{
					findWrapper = wrapper;
					continue;
				}
				if (wrapper.CollectFolderPath.Length > findWrapper.CollectFolderPath.Length)
					findWrapper = wrapper;
			}

			// 如果没有找到收集器
			if (findWrapper == null)
			{
				IAssetCollector defaultCollector = new LabelByFilePath();
				return defaultCollector.GetAssetBundleLabel(assetpath);
			}

			// 根据规则设置获取标签名称
			IAssetCollector collector = GetCollectorInstance(findWrapper.GetCollectorClassName());
			return collector.GetAssetBundleLabel(assetpath, findWrapper);
		}

        /// <summary>
        /// 获取收集器实例对象
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
		private static IAssetCollector GetCollectorInstance(string classname)
		{
			if (_cacheCollector.TryGetValue(classname, out IAssetCollector instance))
				return instance;

			// 如果不存在创建类的实例
			if (_cacheTypes.TryGetValue(classname, out Type type))
			{
				instance = (IAssetCollector)Activator.CreateInstance(type);
				_cacheCollector.Add(classname, instance);
				return instance;
			}
			else
			{
				throw new Exception($"资源收集器类型无效：{classname}");
			}
		}
	}
}