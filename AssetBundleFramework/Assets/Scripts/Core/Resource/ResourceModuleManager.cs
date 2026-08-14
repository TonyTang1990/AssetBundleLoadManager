/*
 * Description:             ResourceModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/24
 */

using System;
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
    public class ResourceModuleManager : SingletonBase<ResourceModuleManager>
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
        /// 初始化
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public override void Shutdown()
        {
            base.Shutdown();
            CurrentResourceModule?.Shutdown();
        }

        /// <summary>
        /// 自定义初始化
        /// </summary>
        public void Init()
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
            CurrentResourceModule.Init();
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            CurrentResourceModule.Update(deltaTime);
        }

        /// <summary>
        /// 重新加载数据(针对热更流程后需要重新加载部分数据的情况)
        /// </summary>
        public void ReloadData()
        {
            CurrentResourceModule.ReloadData();
        }

        /// <summary>
        /// 同步请求Asset资源(为未来支持动态资源加载做准备)
        /// 同步Asset资源加载统一入口
        /// </summary>
        /// <param name="assetName">资源名(带后缀)</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        public AssetRequestHandle RequstAssetSync<T>(string assetName, out AssetLoader assetLoader,
                                                     Action<AssetLoader, AssetRequestHandle> completeHandler,
                                                     ResourceLoadType loadType = ResourceLoadType.NormalLoad)
                                                     where T : UnityEngine.Object
        {
            return CurrentResourceModule.RequstAsset<T>(assetName, out assetLoader, completeHandler,
                                                        loadType, ResourceLoadMethod.Sync);
        }

        /// <summary>
        /// 异步请求Asset资源
        /// 异步Asset资源加载统一入口
        /// </summary>
        /// <param name="assetName">资源名(带后缀)</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        public AssetRequestHandle RequstAssetAsync<T>(string assetName, out AssetLoader assetLoader,
                                                      Action<AssetLoader, AssetRequestHandle> completeHandler,
                                                      ResourceLoadType loadType = ResourceLoadType.NormalLoad)
                                                      where T : UnityEngine.Object
        {
            return CurrentResourceModule.RequstAsset<T>(assetName, out assetLoader, completeHandler,
                                                        loadType, ResourceLoadMethod.Async);
        }

        /// <summary>
        /// 同步指定Asset所在AssetBundle资源(回调式--为未来支持动态资源加载做准备)
        /// 同步AssetBundle资源加载统一入口
        /// Note:
        /// 仅AssetBundle模式生效，AssetDatabase模式下直接返回null和回调null
        /// </summary>
        /// <param name="assetName">Asset名(含后缀)</param>
        /// <param name="assetBundleLoader">AssetBundle资源加载请求</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        public AssetBundleRequestHandle RequstAssetABSync(string assetName, out BundleLoader assetBundleLoader,
                                                          Action<BundleLoader, AssetBundleRequestHandle> completeHandler,
                                                          ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            return CurrentResourceModule.RequstAssetAB(assetName, out assetBundleLoader, completeHandler,
                                                       loadType, ResourceLoadMethod.Sync);
        }

        /// <summary>
        /// 异步指定Asset所在AssetBundle资源
        /// 异步AssetBundle资源加载统一入口
        /// Note:
        /// 仅AssetBundle模式生效，AssetDatabase模式下直接返回null和回调null
        /// </summary>
        /// <param name="assetName">Asset名(含后缀)</param>
        /// <param name="assetBundleLoader">AssetBundle资源加载请求</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        public AssetBundleRequestHandle RequstAssetABAsync(string assetName, out BundleLoader assetBundleLoader,
                                                           Action<BundleLoader, AssetBundleRequestHandle> completeHandler,
                                                           ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            return CurrentResourceModule.RequstAssetAB(assetName, out assetBundleLoader, completeHandler,
                                                       loadType, ResourceLoadMethod.Async);
        }

        /// <summary>
        /// 同步请求AssetBundle资源(回调式--为未来支持动态资源加载做准备)
        /// 同步AssetBundle资源加载统一入口
        /// Note:
        /// 仅AssetBundle模式生效，AssetDatabase模式下直接返回null和回调null
        /// </summary>
        /// <param name="assetBundlePath">AB资源路径(带平台后缀(e.g. .android))</param>
        /// <param name="assetBundleLoader">AssetBundle资源加载请求</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        public AssetBundleRequestHandle RequstABSync(string assetBundlePath, out BundleLoader assetBundleLoader,
                                                     Action<BundleLoader, AssetBundleRequestHandle> completeHandler,
                                                     ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            return CurrentResourceModule.RequstAssetBundle(assetBundlePath, out assetBundleLoader,
                                                           completeHandler, loadType, ResourceLoadMethod.Sync);
        }

        /// <summary>
        /// 异步请求AssetBundle资源
        /// Note:
        /// 仅AssetBundle模式生效，AssetDatabase模式下直接返回null和回调null
        /// </summary>
        /// <param name="assetBundlePath">AB资源路径(带平台后缀(e.g. .android))</param>
        /// <param name="assetBundleLoader">AssetBundle资源加载请求</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        public AssetBundleRequestHandle RequstABAsync(string assetBundlePath, out BundleLoader assetBundleLoader,
                                                      Action<BundleLoader, AssetBundleRequestHandle> completeHandler,
                                                      ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            return CurrentResourceModule.RequstAssetBundle(assetBundlePath, out assetBundleLoader,
                                                           completeHandler, loadType, ResourceLoadMethod.Async);
        }

        /// <summary>
        /// 释放指定Asset名(含后缀)(索引计数-1)
        /// </summary>
        /// <param name="assetName">资源名(带后缀)</param>
        public void ReleaseAsset(string assetName)
        {
            CurrentResourceModule.ReleaseAsset(assetName);
        }

        /// <summary>
        /// 获取或创建指定AssetBundle路径的AssetBundle信息
        /// </summary>
        /// <param name="assetBundlePath">AB路径</param>
        /// <param name="loadType">加载类型</param>
        /// <returns></returns>
        public AssetBundleInfo GetOrCreateAssetBundleInfo(string assetBundlePath, ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            return CurrentResourceModule.GetOrCreateAssetBundleInfo(assetBundlePath, loadType);
        }

        /// <summary>
        /// 获取或创建指定Asset路径的Asset信息
        /// </summary>
        /// <param name="assetPath">Asset路径</param>
        /// <param name="ownerAssetBundlePath">所属AB路径</param>
        /// <param name="loadType">加载类型</param>
        /// <returns></returns>
        public AssetInfo GetOrCreateAssetInfo<T>(string assetPath, string ownerAssetBundlePath = null,
                                                 ResourceLoadType loadType = ResourceLoadType.NormalLoad)
                                                 where T : UnityEngine.Object
        {
            return CurrentResourceModule.GetOrCreateAssetInfo<T>(assetPath, ownerAssetBundlePath, loadType);
        }

        /// <summary>
        /// 获取指定AssetBundle路径的AssetBundle信息
        /// </summary>
        /// <param name="assetBundlePath"></param>
        /// <returns></returns>
        public AssetBundleInfo GetAssetBundleInfo(string assetBundlePath)
        {
            return CurrentResourceModule.GetAssetBundleInfo(assetBundlePath);
        }

        /// <summary>
        /// 删除指定Asset路径的Asset信息(任何加载方式)
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public bool DeleteAssetInfo(string assetPath)
        {
            return CurrentResourceModule.DeleteAssetInfo(assetPath);
        }

        /// <summary>
        /// 提供给外部的触发卸载所有正常加载不再使用的资源资源(递归判定，不限制回收数量)
        /// Note:
        /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
        /// </summary>
        public void UnloadAllUnsedNormalLoadedResources()
        {
            CurrentResourceModule.UnloadAllUnusedResources();
        }

        /// <summary>
        /// 获取Asset名(含后缀名)的Asset路径
        /// Note:
        /// 仅支持主动加载的Asset才能获取到有效Asset路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public string GetAssetPath(string assetName)
        {
            return CurrentResourceModule.GetAssetPath(assetName);
        }

        /// <summary>
        /// 强制卸载所有资源(只在特定情况下用 e.g. 热更后卸载所有已加载资源后重新初始化加载AB资源)***慎用***
        /// </summary>
        public void ForceUnloadAllResources()
        {
            CurrentResourceModule.ForceUnloadAllResources();
        }
    }
}
