/*
 * Description:             AssetBundleInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundleInfo.cs
    /// AssetBundle加载信息
    /// </summary>
    public class AssetBundleInfo : AbstractResourceInfo
    {
        /// <summary>
        /// 当前AB依赖AB路径信息组
        /// </summary>
        private string[] mDepAssetBundlePaths;

        /// <summary>
        /// 所有已加载的Asset信息
        /// </summary>
        public Dictionary<string, AssetInfo> AllLoadedAssetInfoMap
        {
            get;
            protected set;
        }

        /// <summary>
        /// 是否不再有人使用
        /// </summary>
        public override bool IsUnsed
        {
            get 
            {
                return IsReady && RefCount <= 0 && UpdateOwnerReference() == 0 && IsAllAssetsUnsed;
            }
        }

        /// <summary>
        /// 是否所有的子Asset没人使用
        /// </summary>
        protected bool IsAllAssetsUnsed
        {
            get
            {
                foreach(var assetInfo in AllLoadedAssetInfoMap)
                {
                    if(!assetInfo.Value.IsUnsed)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public AssetBundleInfo()
        {
            AllLoadedAssetInfoMap = new Dictionary<string, AssetInfo>();
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
            mDepAssetBundlePaths = null;
            AllLoadedAssetInfoMap.Clear();
        }

        public void Init(string assetBundlePath, string[] depAssetBundlePaths, ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            ResourcePath = assetBundlePath;
            mDepAssetBundlePaths = depAssetBundlePaths;
            LoadType = loadType;
        }

        /// <summary>
        /// 添加AssetBundle内的Asset信息
        /// </summary>
        /// <param name="assetInfo"></param>
        /// <returns></returns>
        public bool AddAssetInfo(AssetInfo assetInfo)
        {
            if(assetInfo.OwnerAsestBundlePath == ResourcePath)
            {
                if(!AllLoadedAssetInfoMap.ContainsKey(assetInfo.ResourcePath))
                {
                    AllLoadedAssetInfoMap.Add(assetInfo.ResourcePath, assetInfo);
                    return true;
                }
                else
                {
                    Debug.LogError($"AssetBundlePath:{ResourcePath}不允许重复添加AssetPath:{assetInfo.ResourcePath}的Asset信息,添加失败,请检查代码!");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"AssetBundlePath:{ResourcePath}不允许添加属于其他AssetBundlePath:{assetInfo.OwnerAsestBundlePath}的Asset信息,添加失败,请检查代码!");
                return false;
            }
        }

        public override void Dispose()
        {
            if(LoadType != ResourceLoadType.NormalLoad)
            {
                Debug.LogWarning($"正在卸载非NormalLoad的AssetBundlePath:{ResourcePath}的AssetBundleInfo信息!");
            }
            // AB模式释放指定AB时，需要减少依赖AB信息的索引计数
            for (int i = 0, length = mDepAssetBundlePaths.Length; i < length; i++)
            {
                var depAssetBundleInfo = ResourceModuleManager.Singleton.CurrentResourceModule.GetAssetBundleInfo(mDepAssetBundlePaths[i]);
                depAssetBundleInfo?.Release();
            }
            mDepAssetBundlePaths = null;
            // AssetBundleLoader和AssetBundleInfo是一一对应，
            // 在AssetBundleInfo回收时,AssetBundleLoader也应该得到回收
            // 同时回收所有应加载的AssetInfo信息
            foreach (var assetInfo in AllLoadedAssetInfoMap)
            {
                ResourceModuleManager.Singleton.CurrentResourceModule.DeleteAssetInfo(assetInfo.Key);
            }
            AllLoadedAssetInfoMap.Clear();
            LoaderManager.Singleton.DeleteLoaderByPath(ResourcePath);
            var assetBundle = GetResource<AssetBundle>();
            assetBundle.Unload(true);
            //AB卸载数据统计
            if (ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch)
            {
                ResourceLoadAnalyse.Singleton.addResourceUnloadedTime(ResourcePath);
            }
            base.Dispose();
        }
    }
}