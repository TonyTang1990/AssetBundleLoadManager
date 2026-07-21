/*
 * Description:             AssetInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/13
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetInfo.cs
    /// </summary>
    public class AssetInfo : AbstractResourceInfo
    {
        /// <summary>
        /// Asset类型
        /// </summary>
        public Type AssetType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Asset名
        /// </summary>
        public string AssetName
        {
            get;
            protected set;
        }

        /// <summary>
        /// 所属AB路径信息(仅在AB模式下有值)
        /// </summary>
        public string OwnerAsestBundlePath
        {
            get;
            protected set;
        }

        /// <summary>
        /// SubAsset缓存Map<SubAsset类型, Map<SubAsset名, SubAsset>>
        /// SubAsset的引用计数和Owner统一由当前主AssetInfo管理
        /// </summary>
        protected Dictionary<Type, Dictionary<string, UnityEngine.Object>> mSubAssetMap;

        public AssetInfo()
        {
            AssetType = null;
            AssetName = null;
            OwnerAsestBundlePath = null;
            mSubAssetMap = new Dictionary<Type, Dictionary<string, UnityEngine.Object>>();
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }

        public override void OnDispose()
        {
            base.OnDispose();
        }

        /// <summary>
        /// 重置数据
        /// </summary>
        protected override void ResetDatas()
        {
            base.ResetDatas();
            AssetType = null;
            AssetName = null;
            OwnerAsestBundlePath = null;
            mSubAssetMap.Clear();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="assetPath">Asset路径</param>
        /// <param name="assetType">Asset类型</param>
        /// <param name="assetBundlePath">所属AB路径</param>
        /// <param name="loadType">加载类型</param>
        public void Init(string assetPath, Type assetType, string assetBundlePath = null,
                         ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            ResourcePath = assetPath;
            AssetType = assetType;
            AssetName = Path.GetFileNameWithoutExtension(ResourcePath);
            OwnerAsestBundlePath = assetBundlePath;
            LoadType = loadType;
        }

        public override void Dispose()
        {
            if (LoadType != ResourceLoadType.NormalLoad)
            {
                Debug.LogWarning($"正在卸载非NormalLoad的AssetPath:{ResourcePath}的AssetInfo信息!");
            }
            // AssetLoader和AssetInfo是一一对应，
            // 在AssetInfo回收时,AssetLoader也应该得到回收
            LoaderManager.Singleton.DeleteLoaderByPath(ResourcePath);
            base.Dispose();
        }
        
        /// <summary>
        /// 缓存单个SubAsset
        /// </summary>
        /// <param name="subAsset"></param>
        /// <returns></returns>
        public bool CacheSubAsset(UnityEngine.Object subAsset)
        {
            // AssetBundle.LoadAssetWithSubAssets会同时返回主Asset，这里只缓存SubAsset
            if (subAsset == null || subAsset == mResource || string.IsNullOrEmpty(subAsset.name))
            {
                return false;
            }

            var subAssetType = subAsset.GetType();
            Dictionary<string, UnityEngine.Object> typeAssetMap;
            if (!mSubAssetMap.TryGetValue(subAssetType, out typeAssetMap))
            {
                typeAssetMap = new Dictionary<string, UnityEngine.Object>(StringComparer.Ordinal);
                mSubAssetMap.Add(subAssetType, typeAssetMap);
            }
            typeAssetMap[subAsset.name] = subAsset;
            return true;
        }

        /// <summary>
        /// 缓存多个SubAsset
        /// </summary>
        /// <param name="subAssets"></param>
        public void CacheSubAssets(UnityEngine.Object[] subAssets)
        {
            if (subAssets == null)
            {
                return;
            }

            for (int i = 0, length = subAssets.Length; i < length; i++)
            {
                CacheSubAsset(subAssets[i]);
            }
        }

        /// <summary>
        /// 尝试获取指定类型和名称的SubAsset缓存
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="subAssetName"></param>
        /// <param name="subAsset"></param>
        /// <returns></returns>
        public bool TryGetSubAsset<T>(string subAssetName, out T subAsset) where T : UnityEngine.Object
        {
            subAsset = null;
            if (string.IsNullOrEmpty(subAssetName))
            {
                return false;
            }

            Dictionary<string, UnityEngine.Object> typeAssetMap;
            UnityEngine.Object cachedSubAsset;
            if (!mSubAssetMap.TryGetValue(typeof(T), out typeAssetMap) ||
                !typeAssetMap.TryGetValue(subAssetName, out cachedSubAsset))
            {
                return false;
            }

            subAsset = cachedSubAsset as T;
            if (subAsset != null)
            {
                UpdateLastUsedTime();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取所有SubAsset缓存
        /// </summary>
        /// <param name="subAssetList"></param>
        public void GetAllSubAssetsOut(ref List<UnityEngine.Object> subAssetList)
        {
            subAssetList.Clear();
            foreach (var typeAssetMap in mSubAssetMap.Values)
            {
                foreach (var subAsset in typeAssetMap.Values)
                {
                    subAssetList.Add(subAsset);
                }
            }
        }
    }
}
