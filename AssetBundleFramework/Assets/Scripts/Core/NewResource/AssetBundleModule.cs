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
            if (mAssetBuildInfo.ABPathDepMap.ContainsKey(abpath))
            {
                return mAssetBuildInfo.ABPathDepMap[abpath];
            }
            else
            {
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
        /// 资源加载模块初始化
        /// </summary>
        public override void init()
        {
            base.init();
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
        protected override int realRequestAsset<T>(string assetPath, out AssetLoader assetLoader, Action<AssetLoader> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            var requestUID = LoaderManager.Singleton.GetNextRequestUID();
            var ownerAssetBundlePath = GetAssetPathBundleNameDelegate(assetPath);
            var depABPaths = GetAssetBundleDpInfoDelegate(ownerAssetBundlePath);
            var bundleAssetLoader = LoaderManager.Singleton. createBundleAssetLoader<T>(assetPath, ownerAssetBundlePath, loadType, loadMethod) as BundleAssetLoader;
            bundleAssetLoader.initBundleInfo(ownerAssetBundlePath, depABPaths);
            bundleAssetLoader.addLoadAssetCompleteCallBack(requestUID, completeHandler);
            bundleAssetLoader.load();
            assetLoader = bundleAssetLoader;
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
        protected override int realRequestAssetBundle(string abPath, out BundleLoader bundleLoader, Action<BundleLoader> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            // TODO: 支持动态AB资源下载
            var requestUID = LoaderManager.Singleton.GetNextRequestUID();
            var depABPaths = GetAssetBundleDpInfoDelegate(abPath);
            bundleLoader = LoaderManager.Singleton.createAssetBundleLoader<AssetBundleLoader>(abPath, depABPaths, loadType, loadMethod);
            bundleLoader.addLoadABCompleteCallBack(requestUID, completeHandler);
            bundleLoader.load();
            return requestUID;
        }
    }
}