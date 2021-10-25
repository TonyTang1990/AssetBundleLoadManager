/*
 * Description:             ResourceModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/24
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// ResourceModuleManager.cs
    /// 资源管理模块单例类
    /// </summary>
    public class ResourceModuleManager : SingletonTemplate<ResourceModuleManager>
    {
        /// <summary>
        /// 资源加载模式
        /// </summary>
        public ResourceLoadMode LoadMode
        {
            get;
            private set;
        }

        /// <summary>
        /// 当前资源加载模块
        /// </summary>
        public AbstractResourceModule CurrentResourceModule
        {
            get;
            private set;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 资源加载模式Key
        /// </summary>
        private const string ResLoadModeKey = "ResLoadModeKey";
#endif

        public ResourceModuleManager()
        {

        }

        /// <summary>
        /// 初始化资源加载相关的
        /// </summary>
        public void init()
        {
#if UNITY_EDITOR
            LoadMode = (ResourceLoadMode)PlayerPrefs.GetInt(ResLoadModeKey, (int)ResourceLoadMode.AssetBundle);
            if (LoadMode == ResourceLoadMode.AssetBundle)
            {
                CurrentResourceModule = new AssetBundleModule();
            }
            else if (LoadMode == ResourceLoadMode.AssetDatabase)
            {
                CurrentResourceModule = new AssetDatabaseModule();
            }
#else
            //非编辑器只支持AssetBundle模式
            mResLoadMode = ResourceLoadMode.AssetBundle;
            CurrentResourceModule = new AssetBundleModule();
#endif
            Debug.Log(string.Format("当前资源加载模式 : {0}", LoadMode));
            CurrentResourceModule.init();
        }

        /// <summary>
        /// 同步请求Asset资源
        /// 同步Asset资源加载统一入口
        /// </summary>
        /// <param name="assetPath">资源路径(带后缀)</param>
        /// <param name="loadType">资源加载类型</param>
        public T requstAssetSync<T>(string assetPath, ResourceLoadType loadType = ResourceLoadType.NormalLoad) where T : Object
        {
            AssetLoader assetLoader;
            CurrentResourceModule.requstAsset<T>(assetPath, out assetLoader, null, loadType, ResourceLoadMethod.Sync);
            return assetLoader.getAsset<T>();
        }

        /// <summary>
        /// 异步请求Asset资源
        /// 异步Asset资源加载统一入口
        /// </summary>
        /// <param name="assetPath">资源路径(带后缀)</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        public int requstAssetAsync<T>(string assetPath, AbstractResourceModule.LoadResourceCompleteHandler completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad) where T : Object
        {
            AssetLoader assetLoader;
            return CurrentResourceModule.requstAsset<T>(assetPath, out assetLoader, completeHandler, loadType, ResourceLoadMethod.Async);
        }

        /// <summary>
        /// 同步请求AssetBundle资源
        /// 同步AssetBundle资源加载统一入口
        /// </summary>
        /// <param name="assetBundlePath">AB资源路径</param>
        /// <param name="loadType">资源加载类型</param>
        public AssetBundle requstAssetBundleSync(string assetBundlePath, ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            AssetBundleLoader assetBundleLoader;
            CurrentResourceModule.requstAssetBundle(assetBundlePath, out assetBundleLoader, null, loadType, ResourceLoadMethod.Sync);
            return assetBundleLoader.getAsset();
        }

        /// <summary>
        /// 异步请求AssetBundle资源
        /// 异步AssetBundle资源加载统一入口
        /// </summary>
        /// <param name="assetBundlePath">资源路径</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        public int requstAssetBundleAsync(string assetBundlePath, AbstractResourceModule.LoadResourceCompleteHandler completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            AssetBundleLoader assetBundleLoader;
            return CurrentResourceModule.requstAssetBundle(assetBundlePath, out assetBundleLoader, completeHandler, loadType, ResourceLoadMethod.Async);
        }

        public void Update()
        {
            CurrentResourceModule.Update();
        }

        /// <summary>
        /// 提供给外部的触发卸载所有正常加载不再使用的资源资源(递归判定，不限制回收数量)
        /// Note:
        /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
        /// </summary>
        public void unloadAllUnsedNormalLoadedResources()
        {
            CurrentResourceModule.unloadAllUnsedResources();
        }
    }
}