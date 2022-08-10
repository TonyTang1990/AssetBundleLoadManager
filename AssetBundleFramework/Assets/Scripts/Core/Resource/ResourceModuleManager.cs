/*
 * Description:             ResourceModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/24
 */

using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
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
        public ResourceLoadMode ResLoadMode
        {
            get
            {
                return mResLoadMode;
            }
            set
            {
#if UNITY_EDITOR
                mResLoadMode = value;
                PlayerPrefs.SetInt(ResLoadModeKey, (int)mResLoadMode);
                Debug.Log(string.Format("切换资源加载模式到 : {0},重新运行Editor后生效!", mResLoadMode));
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }
#else
                //非编辑器只支持AssetBundle模式
                mResLoadMode = ResourceLoadMode.AssetBundle;
                Debug.Log("真机模式只支持AssetBundle模式，不允许切换!");
#endif
            }
        }
        private ResourceLoadMode mResLoadMode;

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
            mResLoadMode = (ResourceLoadMode)PlayerPrefs.GetInt(ResLoadModeKey, (int)ResourceLoadMode.AssetBundle);
            if (mResLoadMode == ResourceLoadMode.AssetBundle)
            {
                CurrentResourceModule = new AssetBundleModule();
            }
            else if (mResLoadMode == ResourceLoadMode.AssetDatabase)
            {
                CurrentResourceModule = new AssetDatabaseModule();
            }
#else
            //非编辑器只支持AssetBundle模式
            mResLoadMode = ResourceLoadMode.AssetBundle;
            CurrentResourceModule = new AssetBundleModule();
#endif
            Debug.Log(string.Format("当前资源加载模式 : {0}", mResLoadMode));
            CurrentResourceModule.init();
        }

        /// <summary>
        /// 重新加载数据(针对热更流程后需要重新加载部分数据的情况)
        /// </summary>
        public void reloadData()
        {
            CurrentResourceModule.reloadData();
        }

        /// <summary>
        /// 同步请求Asset资源(为未来支持动态资源加载做准备)
        /// 同步Asset资源加载统一入口
        /// </summary>
        /// <param name="assetPath">资源路径(带后缀)</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        public int requstAssetSync<T>(string assetPath, out AssetLoader assetLoader, Action<AssetLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad) where T : UnityEngine.Object
        {
            return CurrentResourceModule.requstAsset<T>(assetPath, out assetLoader, completeHandler, loadType, ResourceLoadMethod.Sync);
        }

        /// <summary>
        /// 异步请求Asset资源
        /// 异步Asset资源加载统一入口
        /// </summary>
        /// <param name="assetPath">资源路径(带后缀)</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        public int requstAssetAsync<T>(string assetPath, out AssetLoader assetLoader, Action<AssetLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad) where T : UnityEngine.Object
        {
            return CurrentResourceModule.requstAsset<T>(assetPath, out assetLoader, completeHandler, loadType, ResourceLoadMethod.Async);
        }

        /// <summary>
        /// 同步请求AssetBundle资源(回调式--为未来支持动态资源加载做准备)
        /// 同步AssetBundle资源加载统一入口
        /// </summary>
        /// <param name="assetBundlePath">AB资源路径</param>
        /// <param name="loadType">资源加载类型</param>
        public int requstAssetBundleSync(string assetBundlePath, out BundleLoader assetBundleLoader, Action<BundleLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            return CurrentResourceModule.requstAssetBundle(assetBundlePath, out assetBundleLoader, completeHandler, loadType, ResourceLoadMethod.Sync);
        }

        /// <summary>
        /// 异步请求AssetBundle资源
        /// 异步AssetBundle资源加载统一入口
        /// </summary>
        /// <param name="assetBundlePath">资源路径</param>
        /// <param name="assetBundleLoader">AssetBundle资源加载请求</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        public int requstAssetBundleAsync(string assetBundlePath, out BundleLoader assetBundleLoader, Action<BundleLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            return CurrentResourceModule.requstAssetBundle(assetBundlePath, out assetBundleLoader, completeHandler, loadType, ResourceLoadMethod.Async);
        }

        public void Update()
        {
            CurrentResourceModule.update();
        }

        /// <summary>
        /// 提供给外部的触发卸载所有正常加载不再使用的资源资源(递归判定，不限制回收数量)
        /// Note:
        /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
        /// </summary>
        public void unloadAllUnsedNormalLoadedResources()
        {
            CurrentResourceModule.unloadAllUnusedResources();
        }

        /// <summary>
        /// 强制卸载所有资源(只在特定情况下用 e.g. 热更后卸载所有已加载资源后重新初始化加载AB资源)***慎用***
        /// </summary>
        public void forceUnloadAllResources()
        {
            CurrentResourceModule.forceUnloadAllResources();
        }
    }
}