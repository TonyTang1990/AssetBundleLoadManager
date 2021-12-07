/*
 * Description:             AssetBundleModule.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/24
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundleModule.cs
    /// AB模式的资源模块管理类
    /// </summary>
    public class AssetBundleModule : AbstractResourceModule
    {

        #region AB依赖信息部分
        /// <summary>
        /// Asset打包信息
        /// </summary>
        public AssetBuildInfoAsset AssetBuildInfo
        {
            get
            {
                return mAssetBuildInfo;
            }
        }
        protected AssetBuildInfoAsset mAssetBuildInfo;

        /// <summary>
        /// 加载Asset打包信息
        /// </summary>
        private void loadAssetBuildInfo()
        {
            // 确保之前加载的AB打包信息卸载彻底
            if (mAssetBuildInfo != null)
            {
                Resources.UnloadAsset(mAssetBuildInfo);
                mAssetBuildInfo = null;
            }
            // AssetBundle打包信息比较特殊，在未加载完成前拿不到AB名字映射
            // 所以这里单独特殊加载,不走正常流程
            var abpath = AssetBundlePath.GetABLoadFullPath(ResourceConstData.AssetBuildInfoAssetName.ToLower());
            AssetBundle ab = null;
            ab = AssetBundle.LoadFromFile(abpath);
            if (ab != null)
            {
                mAssetBuildInfo = ab.LoadAsset<AssetBuildInfoAsset>(ResourceConstData.AssetBuildInfoAssetName);
                mAssetBuildInfo.init();
                ab.Unload(false);
                Debug.Log("Asset打包信息文件加载成功!");
            }
            else
            {
                Debug.LogError($"找不到Asset打包信息文件:{ResourceConstData.AssetBuildInfoAssetName}");
            }
        }

        /// <summary>
        /// 获取AssetBundle所依赖的AB信息
        /// </summary>
        /// <param name="abpath"></param>
        /// <returns></returns>
        private string[] getAssetBundleDpInfo(string abpath)
        {
            if(string.IsNullOrEmpty(abpath))
            {
                return null;
            }
            if (mAssetBuildInfo.ABPathDepMap.ContainsKey(abpath))
            {
                return mAssetBuildInfo.ABPathDepMap[abpath];
            }
            else
            {
                Debug.LogError($"找不到AssetBundle:{abpath}的AssetBundle依赖信息,请检查是否传递了正确的AssetBundle路径!");
                return null;
            }
        }

        /// <summary>
        /// 获取指定Asset路径的AB名
        /// </summary>
        /// <param name="assetpath"></param>
        /// <returns></returns>
        private string getAssetPathAssetBundleName(string assetpath)
        {
            var abpath = mAssetBuildInfo.getAssetABPath(assetpath);
            if (!string.IsNullOrEmpty(abpath))
            {
                return abpath;
            }
            else
            {
                Debug.LogError($"找不到Asset路径:{assetpath}的AB路径信息!");
                return null;
            }
        }

        /// <summary>
        /// 打印所有AB依赖信息
        /// </summary>
        public void printAllResourceDpInfo()
        {
            foreach (var abinfo in mAssetBuildInfo.ABPathDepMap)
            {
                ResourceLogger.log(string.Format("AB Name:{0}", abinfo.Key));
                foreach (var dpfile in abinfo.Value)
                {
                    ResourceLogger.log(string.Format("       DP AB Name:{0}", dpfile));
                }
            }
        }
        #endregion

        /// <summary>
        /// 已加载AB里不再有有效引用的AB信息列表
        /// </summary>
        protected List<AssetBundleInfo> mUnsedAssetBundleInfoList;

        /// <summary>
        /// 资源加载模块初始化
        /// </summary>
        public override void init()
        {
            base.init();

            mUnsedAssetBundleInfoList = new List<AssetBundleInfo>();

            ResLoadMode = ResourceLoadMode.AssetBundle;
            // 加载Asset打包信息
            loadAssetBuildInfo();
            // 延迟绑定AB相关信息获取委托
            GetAssetPathBundleNameDelegate = getAssetPathAssetBundleName;
            GetAssetBundleDpInfoDelegate = getAssetBundleDpInfo;
        }

        /// <summary>
        /// 真正的请求Asset资源
        /// </summary>
        /// <param name="assetPath">Asset资源路径(带后缀)</param>
        /// <param name="assetLoader">Asset资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        protected override int realRequestAsset<T>(string assetPath, out AssetLoader assetLoader, Action<AssetLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            var requestUID = LoaderManager.Singleton.GetNextRequestUID();
            var ownerAssetBundlePath = GetAssetPathBundleNameDelegate(assetPath);
            var depABPaths = GetAssetBundleDpInfoDelegate(ownerAssetBundlePath);
            var bundleAssetLoader = LoaderManager.Singleton. createBundleAssetLoader<T>(assetPath, ownerAssetBundlePath, loadType, loadMethod) as BundleAssetLoader;
            bundleAssetLoader.initBundleInfo(ownerAssetBundlePath, depABPaths);
            bundleAssetLoader.addRequest(requestUID, completeHandler);
            assetLoader = bundleAssetLoader as AssetLoader;
            bundleAssetLoader.load();
            return requestUID;
        }

        /// <summary>
        /// 真正的请求AssetBundle资源
        /// </summary>
        /// <param name="abPath">AssetBundle资源路径</param>
        /// <param name="bundleLoader">AB资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        protected override int realRequestAssetBundle(string abPath, out BundleLoader bundleLoader, Action<BundleLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            // TODO: 支持动态AB资源下载
            var requestUID = LoaderManager.Singleton.GetNextRequestUID();
            var depABPaths = GetAssetBundleDpInfoDelegate(abPath);
            bundleLoader = LoaderManager.Singleton.createAssetBundleLoader<AssetBundleLoader>(abPath, depABPaths, loadType, loadMethod);
            bundleLoader.addRequest(requestUID, completeHandler);
            bundleLoader.load();
            return requestUID;
        }

        /// <summary>
        /// 真正执行资源卸载指定类型不再使用的资源接口
        /// </summary>
        /// <param name="resourceloadtype"></param>
        protected override void doUnloadSpecificLoadTypeUnsedResource(ResourceLoadType resourceloadtype)
        {
            // 递归判定卸载所有不再可用的正常加载AB
            bool hasunsedAssetBundle = true;
            while (hasunsedAssetBundle)
            {
                // 检查回收不再使用正常已加载的AB
                checkUnsedAssetBundleResources();

                if (mUnsedAssetBundleInfoList.Count == 0)
                {
                    //不再有可卸载的AB
                    hasunsedAssetBundle = false;
                }
                else
                {
                    // 有可卸载的AB
                    doUnloadUnsedAssetBundleWithLimit(true);
                }
            }
        }

        /// <summary>
        /// 执行不再使用资源监察
        /// </summary>
        protected override void doCheckUnusedResource()
        {
            base.doCheckUnusedResource();
            checkUnsedAssetBundleResources();
            doUnloadUnsedAssetBundleWithLimit(true);
        }

        /// <summary>
        /// 执行卸载所有不再使用的资源
        /// </summary>
        protected override void doUnloadAllUnusedResources()
        {
            base.doUnloadAllUnusedResources();
            checkUnsedAssetBundleResources();
            doUnloadUnsedAssetBundleWithLimit(false);
        }

        /// <summary>
        /// 检查未使用AssetBundle
        /// </summary>
        protected void checkUnsedAssetBundleResources()
        {
            mUnsedAssetBundleInfoList.Clear();
            var time = Time.time;
            // 检查正常加载的资源AssetBundle，回收不再使用的AssetBundle
            foreach (var loadedAssetBundleInfo in mAllLoadedNormalAssetBundleInfoMap)
            {
                if (loadedAssetBundleInfo.Value.IsUnsed)
                {
                    // 强制卸载不需要判定有效资源生命时长
                    //if ((time - loadedAssetBundleInfo.Value.LastUsedTime) > ResourceMinimumLifeTime)
                    mUnsedAssetBundleInfoList.Add(loadedAssetBundleInfo.Value);
                }
            }

            if (mUnsedAssetBundleInfoList.Count > 0)
            {
                // 根据最近使用时间升序排列
                mUnsedAssetBundleInfoList.Sort(ABILastUsedTimeSort);
            }
        }

        /// <summary>
        /// 资源信息根据最近使用时间排序
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int ABILastUsedTimeSort(AbstractResourceInfo a, AbstractResourceInfo b)
        {
            return a.LastUsedTime.CompareTo(b.LastUsedTime);
        }

        /// <summary>
        /// 卸载未使用的AssetBundle
        /// </summary>
        /// <param name="withLimitNumber">是否限制卸载数量</param>
        protected void doUnloadUnsedAssetBundleWithLimit(bool withLimitNumber = false)
        {
            for (int i = 0; i < mUnsedAssetBundleInfoList.Count; i++)
            {
                if (withLimitNumber == false || (withLimitNumber && i < MaxUnloadABNumberPerFrame))
                {
                    deleteAssetBundleInfo(mUnsedAssetBundleInfoList[i].ResourcePath);
                }
                else
                {
                    break;
                }
            }
            mUnsedAssetBundleInfoList.Clear();
        }

        #region 资源调试辅助功能
        /// <summary>
        /// 强制卸载指定AB(只支持NormalLoad的AB资源强制卸载)
        /// </summary>
        /// <param name="assetBundelPath"></param>
        public void forceUnloadSpecificAssetBundle(string assetBundelPath)
        {
            AssetBundleInfo assetBundleInfo = getAssetBundleInfo(assetBundelPath);
            if(assetBundleInfo != null)
            {
                if (mAllLoadedNormalAssetBundleInfoMap.TryGetValue(assetBundelPath, out assetBundleInfo))
                {
                    deleteAssetBundleInfo(assetBundelPath);
                    assetBundleInfo.dispose();
                }
                else
                {
                    ResourceLogger.logErr(string.Format("AssetBundle资源 : {0}属于非NormalLoad资源，不允许强制卸载!", assetBundelPath));
                }
            }
            else
            {
                ResourceLogger.logErr(string.Format("AssetBundle资源 : {0}未被加载，无法强制卸载!", assetBundelPath));
            }
        }
        #endregion
    }
}