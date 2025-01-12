/*
 * Description:             LoaderManager.cs
 * Author:                  TONYTANG
 * Create Date:             2021/10/13
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// LoaderManager.cs
    /// 资源加载器管理单例类
    /// </summary>
    public class LoaderManager : SingletonTemplate<LoaderManager>
    {
        /// Note:
        /// 同步加载是单帧完成
        /// 异步加载会限制单帧加载Asset和AB数量
        /// 限制单帧过卡时不进一步触发异步加载，避免单帧过卡

        /// <summary>
        /// 单帧资源加载的数量限制(含Asset和AssetBundle)
        /// </summary>
        private const int RESOURCE_LOAD_NUMBER_PER_FRAME = 5;

        /// <summary>
        /// 单帧资源加载时长限制
        /// </summary>
        private const float RESOURCE_LOAD_TIME_LIMIT_PER_FRAME = 50f;

        /// <summary>
        /// 请求UID循环值(避免请求UID后期过大问题)
        /// </summary>
        private const int REQUEST_UID_LOOP_VALUE = 10000;

        /// <summary>
        /// 是否有加载任务(含Asset和AssetBundle)
        /// </summary>
        public bool HasLoadingTask
        {
            get
            {
                return mAllWaitLoadLoaderList.Count > 0;
            }
        }

        /// <summary>
        /// 下一个有效资源请求UID
        /// </summary>
        private int mNextRequestUID;

        /// <summary>
        /// 所有资源加载器(含Asset和AssetBundle)
        /// </summary>
        private Dictionary<string, Loadable> mAllLoaderMap;

        /// <summary>
        /// 所有正在等待加载的加载器列表
        /// </summary>
        private List<Loadable> mAllWaitLoadLoaderList;

        /// <summary>
        /// Asset资源请求UID Map<资源请求UID,Asset路径>
        /// </summary>
        private Dictionary<int, string> mAssetRequestUIDMap;

        /// <summary>
        /// AssetBundle资源请求UID Map<资源请求UID,AssetBundle路径>
        /// </summary>
        private Dictionary<int, string> mAssetBundleRequestUIDMap;

        /// <summary>
        /// 单帧资源加载个数
        /// </summary>
        private int mResourceLoadCountPerFrame;

        /// <summary>
        /// 单帧资源加载开始时间
        /// </summary>
        private float mResourceLoadStartTime;

        /// <summary>
        /// 单帧资源加载经历时长
        /// </summary>
        private float mResourceLoadTimePassed;

        /// <summary>
        /// 资源加载是否忙
        /// </summary>
        private bool IsResourceLoadBusy
        {
            get
            {
                return mResourceLoadTimePassed >= RESOURCE_LOAD_TIME_LIMIT_PER_FRAME;
            }
        }

        public LoaderManager()
        {
            mNextRequestUID = 1;
            mAllLoaderMap = new Dictionary<string, Loadable>();
            mAllWaitLoadLoaderList = new List<Loadable>();
            mAssetRequestUIDMap = new Dictionary<int, string>();
            mAssetBundleRequestUIDMap = new Dictionary<int, string>();
            mResourceLoadCountPerFrame = 0;
            mResourceLoadStartTime = 0f;
            mResourceLoadTimePassed = 0f;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
           if (HasLoadingTask)
           {
                mResourceLoadCountPerFrame = 0;
                mResourceLoadStartTime = Time.time;
                mResourceLoadTimePassed = 0;
                for (int i = 0; i < mAllWaitLoadLoaderList.Count; i++)
                {
                    mAllWaitLoadLoaderList[i].DoLoad();
                    i--;
                    mResourceLoadCountPerFrame++;
                    mResourceLoadTimePassed = Time.time - mResourceLoadStartTime;
                    if (mResourceLoadCountPerFrame >= RESOURCE_LOAD_NUMBER_PER_FRAME || IsResourceLoadBusy)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 获取下一个有效请求UID
        /// </summary>
        /// <returns></returns>
        public int GetNextRequestUID()
        {
            if(mNextRequestUID <= REQUEST_UID_LOOP_VALUE)
            {
                return mNextRequestUID++;
            }
            else
            {
                mNextRequestUID = mNextRequestUID % REQUEST_UID_LOOP_VALUE;
                return mNextRequestUID++;
            }
        }

        /// <summary>
        /// 创建AssetDatabase模式指定Asset路径的Asset加载器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath">Asset路径</param>
        /// <param name="ownerAssetBundlePath">AssetBundle路径</param>
        /// <returns></returns>
        public AssetDatabaseLoader createAssetDatabaseLoader<T>(string assetPath, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync) where T : UnityEngine.Object
        {
            AssetDatabaseLoader assetDatabaseLoader = ObjectPool.Singleton.Pop<AssetDatabaseLoader>();
            AssetInfo assetInfo = ResourceModuleManager.Singleton.CurrentResourceModule.GetOrCreateAssetInfo<T>(assetPath);
            assetDatabaseLoader.Init(assetPath, typeof(T), assetInfo, loadType, loadMethod);
            return assetDatabaseLoader;
        }

        /// <summary>
        /// 创建AssetBundle模式指定Asset路径的Asset加载器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath">Asset路径</param>
        /// <param name="ownerAssetBundlePath">所属AB路径</param>
        /// <param name="loadType">加载类型</param>
        /// <param name="loadMethod">加载方式</param>
        /// <returns></returns>
        public AssetLoader createBundleAssetLoader<T>(string assetPath, string ownerAssetBundlePath = null, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync) where T : UnityEngine.Object
        {
            BundleAssetLoader assetLoader = ObjectPool.Singleton.Pop<BundleAssetLoader>();
            AssetInfo assetInfo = ResourceModuleManager.Singleton.CurrentResourceModule.GetOrCreateAssetInfo<T>(assetPath, ownerAssetBundlePath, loadType);
            assetLoader.Init(assetPath, typeof(T), assetInfo, loadType, loadMethod);
            return assetLoader;
        }

        /// <summary>
        /// 创建指定AssetBundle路径的AssetBundle加载器
        /// </summary>
        /// <param name="assetBundlePath">AB路径</param>
        /// <param name="depABPaths">依赖AB路径组</param>
        /// <param name="loadType">加载类型</param>
        /// <returns></returns>
        public BundleLoader CreateAssetBundleLoader<T>(string assetBundlePath, string[] depABPaths, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync) where T : BundleLoader
        {
            BundleLoader bundleLoader = ObjectPool.Singleton.Pop<T>();
            AssetBundleInfo assetBundleInfo = ResourceModuleManager.Singleton.CurrentResourceModule.GetOrCreateAssetBundleInfo(assetBundlePath, loadType);
            bundleLoader.Init(assetBundlePath, assetBundleInfo, depABPaths, loadType, loadMethod);
            return bundleLoader;
        }

        /// <summary>
        /// 添加Bundle加载器任务
        /// </summary>
        /// <param name="loader"></param>
        /// <returns></returns>
        public bool AddLoadTask(Loadable loader)
        {
            if (mAllLoaderMap.ContainsKey(loader.ResourcePath))
            {
                Debug.LogError($"Frame:{AbstractResourceModule.Frame}已经存在资源:{loader.ResourcePath}的加载器,添加资源加载器任务失败,不应该进入这里!");
                return false;
            }
            mAllLoaderMap.Add(loader.ResourcePath, loader);
            ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}添加资源:{loader.ResourcePath}加载器任务!");
            mAllWaitLoadLoaderList.Add(loader);
            return true;
        }

        /// <summary>
        /// 移除Bundle加载器任务
        /// </summary>
        /// <param name="loader"></param>
        /// <returns></returns>
        public bool RemoveLoadTask(Loadable loader)
        {
            var result = mAllWaitLoadLoaderList.Remove(loader);
            if (!result)
            {
                Debug.LogError($"Frame:{AbstractResourceModule.Frame}找不到资源:{loader.ResourcePath}的加载器任务,移除失败,请检查代码流程!");
            }
            else
            {
                ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}移除资源:{loader.ResourcePath}的加载器任务成功!");
            }
            return result;
        }

        /// <summary>
        /// 删除指定资源加载器信息
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public bool DeleteLoaderByPath(string resourcePath)
        {
            var loader = GetLoader(resourcePath);
            return DeleteLoader(loader);
        }

        /// <summary>
        /// 删除指定资源加载器信息
        /// </summary>
        /// <param name="loader"></param>
        /// <returns></returns>
        public bool DeleteLoader(Loadable loader)
        {
            // 未加载完成的加载器不应该被移除
            if(!loader.IsDone)
            {
                Debug.LogError($"资源:{loader.ResourcePath}加载器未加载完成,不允许删除!");
                return false;
            }
            var result = mAllLoaderMap.Remove(loader.ResourcePath);
            if (!result)
            {
                Debug.LogError($"找不到资源:{loader.ResourcePath}的加载器信息,删除资源加载器信息失败,请检查代码流程!");
            }
            else
            {
                if (loader is BundleAssetLoader)
                {
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}删除Asset资源:{loader.ResourcePath}的加载器信息成功!");
                    ObjectPool.Singleton.Push<BundleAssetLoader>(loader as BundleAssetLoader);
                }
                else if(loader is AssetDatabaseLoader)
                {
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}删除AssetDatabase资源:{loader.ResourcePath}的加载器信息成功!");
                    ObjectPool.Singleton.Push<AssetDatabaseLoader>(loader as AssetDatabaseLoader);
                }
                else if (loader is AssetBundleLoader)
                {
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}删除AssetBundle资源:{loader.ResourcePath}的加载器信息成功!");
                    ObjectPool.Singleton.Push<AssetBundleLoader>(loader as AssetBundleLoader);
                }
                else
                {
                    Debug.LogError($"Frame:{AbstractResourceModule.Frame}不支持的加载类类型:{loader.GetType().ToString()},进池失败!");
                }
            }
            return result;
        }

        /// <summary>
        /// 获取指定Loader
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public Loadable GetLoader(string resourcePath)
        {
            Loadable loader;
            if (mAllLoaderMap.TryGetValue(resourcePath, out loader))
            {
                return loader;
            }
            return loader;
        }

        /// <summary>
        /// 获取指定Asset路径的加载器
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public AssetLoader GetAssetLoader(string assetPath)
        {
            Loadable assetLoader;
            if (mAllLoaderMap.TryGetValue(assetPath, out assetLoader))
            {
                return assetLoader as AssetLoader;
            }
            return null;
        }

        /// <summary>
        /// 获取指定AssetBundle路径的加载器
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public BundleLoader GetAssetBundleLoader(string assetBundlePath)
        {
            Loadable bundleLoader;
            if (mAllLoaderMap.TryGetValue(assetBundlePath, out bundleLoader))
            {
                return bundleLoader as BundleLoader;
            }
            return null;
        }

        /// <summary>
        /// 获取指定请求UID的Asset路径
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        private string GetAssetByRequestUID(int requestUID)
        {
            string assetPath;
            if (mAssetRequestUIDMap.TryGetValue(requestUID, out assetPath))
            {
                return assetPath;
            }
            else
            {
                Debug.LogError($"找不到请求UID:{requestUID}的Asset路径,获取请求UID的AssetBundle路径失败!");
                return null;
            }
        }

        /// <summary>
        /// 添加Asset路径资源请求UID
        /// </summary>
        /// <param name="requestUID"></param>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public bool AddAssetRequestUID(int requestUID, string assetPath)
        {
            if (!mAssetRequestUIDMap.ContainsKey(requestUID))
            {
                mAssetRequestUIDMap.Add(requestUID, assetPath);
                return true;
            }
            else
            {
                Debug.LogError($"添加Asset:{assetPath}资源请求UID:{requestUID}成功!");
                return false;
            }
        }

        /// <summary>
        /// 移除指定请求UID的Asset加载
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        public bool RemoveAssetRequestUID(int requestUID)
        {
            if (mAssetRequestUIDMap.Remove(requestUID))
            {
                return true;
            }
            else
            {
                Debug.LogError($"找不到Asset请求UID:{requestUID},移除Asset请求UID失败!");
                return false;
            }
        }

        /// <summary>
        /// 取消指定请求UID的Asset加载
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        public bool CancelAssetRequest<T>(int requestUID) where T : UnityEngine.Object
        {
            string assetPath = GetAssetByRequestUID(requestUID);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"找不到请求UID:{requestUID}的Asset路径,取消Asset请求失败!");
                return false;
            }
            var assetLoader = GetAssetLoader(assetPath);
            return assetLoader != null ? assetLoader.CancelRequest(requestUID) : false;
        }

        /// <summary>
        /// 获取指定请求UID的AssetBundle路径
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        private string GetAssetBundleByRequestUID(int requestUID)
        {
            string assetBundlePath;
            if (mAssetBundleRequestUIDMap.TryGetValue(requestUID, out assetBundlePath))
            {
                return assetBundlePath;
            }
            else
            {
                Debug.LogError($"找不到请求UID:{requestUID}的AssetBundle路径,获取请求UID的AssetBundle路径失败!");
                return null;
            }
        }

        /// <summary>
        /// 添加AssetBundle路径资源请求UID
        /// </summary>
        /// <param name="requestUID"></param>
        /// <param name="assetBundlePath"></param>
        /// <returns></returns>
        public bool AddAssetBundleRequestUID(int requestUID, string assetBundlePath)
        {
            if (!mAssetBundleRequestUIDMap.ContainsKey(requestUID))
            {
                mAssetBundleRequestUIDMap.Add(requestUID, assetBundlePath);
                return true;
            }
            else
            {
                Debug.LogError($"添加AssetBundle:{assetBundlePath}资源请求UID:{requestUID}成功!");
                return false;
            }
        }

        /// <summary>
        /// 移除指定请求UID的AssetBundle加载
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        public bool RemoveAssetBundleRequestUID(int requestUID)
        {
            if (mAssetBundleRequestUIDMap.Remove(requestUID))
            {
                return true;
            }
            else
            {
                Debug.LogError($"找不到AssetBundle请求UID:{requestUID},移除AssetBundle请求UID失败!");
                return false;
            }
        }

        /// <summary>
        /// 取消指定请求UID的AssetBundle加载
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        public bool CancelAssetBundleRequest(int requestUID)
        {
            string assetBundlePath = GetAssetBundleByRequestUID(requestUID);
            if (string.IsNullOrEmpty(assetBundlePath))
            {
                Debug.LogError($"找不到请求UID:{requestUID}的AssetBundle路径,取消AssetBundle请求失败!");
                return false;
            }
            var assetBundleLoader = GetAssetBundleLoader(assetBundlePath);
            return assetBundleLoader != null ? assetBundleLoader.CancelRequest(requestUID) : false;
        }

        #region 调试用
        /// <summary>
        /// 获取所有AssetBundle加载器
        /// </summary>
        /// <returns></returns>
        public void GetAllAssetBundleLoader(ref List<BundleLoader> allAssetBundleLoader)
        {
            allAssetBundleLoader.Clear();
            foreach (var loader in mAllLoaderMap)
            {
                if(loader.Value is BundleLoader)
                {
                    allAssetBundleLoader.Add(loader.Value as BundleLoader);
                }
            }
        }

        /// <summary>
        /// 获取所有Asset加载器
        /// </summary>
        /// <returns></returns>
        public void GetAllAssetLoader(ref List<AssetLoader> allAssetLoader)
        {
            allAssetLoader.Clear();
            foreach (var loader in mAllLoaderMap)
            {
                if (loader.Value is AssetLoader)
                {
                    allAssetLoader.Add(loader.Value as AssetLoader);
                }
            }
        }

        /// <summary>
        /// 获取所有等待加载的AssetBundle加载器
        /// </summary>
        /// <returns></returns>
        public void GetAllWaitLoadedBundleLoader(ref List<BundleLoader> waitLoadedAssetBundleLoader)
        {
            waitLoadedAssetBundleLoader.Clear();
            foreach (var waitLoadedLoader in mAllWaitLoadLoaderList)
            {
                if (waitLoadedLoader is BundleLoader)
                {
                    waitLoadedAssetBundleLoader.Add(waitLoadedLoader as BundleLoader);
                }
            }
        }

        /// <summary>
        /// 获取所有等待加载的Asset加载器
        /// </summary>
        /// <returns></returns>
        public void GetAllWaitLoadedAssetLoader(ref List<AssetLoader> waitLoadedAssetLoader)
        {
            waitLoadedAssetLoader.Clear();
            foreach (var waitLoadedLoader in mAllWaitLoadLoaderList)
            {
                if (waitLoadedLoader is AssetLoader)
                {
                    waitLoadedAssetLoader.Add(waitLoadedLoader as AssetLoader);
                }
            }
        }
        #endregion
    }
}