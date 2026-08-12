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
        /// Note:
        /// Asset是Asset名(含后缀)
        /// AssetBundle是AssetBundle路径
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
        private Dictionary<int, string> mABRequestUIDMap;

        /// <summary>
        /// 单帧资源加载个数
        /// </summary>
        private int mResLoadCountPerFrame;

        /// <summary>
        /// 单帧资源加载开始时间
        /// </summary>
        private float mResLoadStartTime;

        /// <summary>
        /// 单帧资源加载经历时长
        /// </summary>
        private float mResLoadTimePassed;

        /// <summary>
        /// 资源加载是否忙
        /// </summary>
        private bool IsResourceLoadBusy
        {
            get
            {
                return mResLoadTimePassed >= RESOURCE_LOAD_TIME_LIMIT_PER_FRAME;
            }
        }

        public LoaderManager()
        {
            mNextRequestUID = 1;
            mAllLoaderMap = new Dictionary<string, Loadable>();
            mAllWaitLoadLoaderList = new List<Loadable>();
            mAssetRequestUIDMap = new Dictionary<int, string>();
            mABRequestUIDMap = new Dictionary<int, string>();
            mResLoadCountPerFrame = 0;
            mResLoadStartTime = 0f;
            mResLoadTimePassed = 0f;
        }

        /// <summary>
        /// 更新
        /// </summary>
        public void Update()
        {
           if (HasLoadingTask)
           {
                mResLoadCountPerFrame = 0;
                mResLoadStartTime = Time.time;
                mResLoadTimePassed = 0;
                for (int i = 0; i < mAllWaitLoadLoaderList.Count; i++)
                {
                    mAllWaitLoadLoaderList[i].DoLoad();
                    i--;
                    mResLoadCountPerFrame++;
                    mResLoadTimePassed = Time.time - mResLoadStartTime;
                    if (mResLoadCountPerFrame >= RESOURCE_LOAD_NUMBER_PER_FRAME || IsResourceLoadBusy)
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
        /// 创建Asset请求句柄
        /// </summary>
        /// <returns></returns>
        public AssetRequestHandle CreateAssetRequestHandle()
        {
            return new AssetRequestHandle(GetNextRequestUID(), CancelAssetRequest, LoadAssetRequestImmediately);
        }

        /// <summary>
        /// 创建AssetBundle请求句柄
        /// </summary>
        /// <returns></returns>
        public AssetBundleRequestHandle CreateAssetBundleRequestHandle()
        {
            return new AssetBundleRequestHandle(GetNextRequestUID(), CancelABRequest, LoadABRequestImmediately);
        }

        /// <summary>
        /// 创建AssetDatabase模式指定Asset路径的Asset加载器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath">Asset路径</param>
        /// <param name="ownerAssetBundlePath">AssetBundle路径</param>
        /// <returns></returns>
        public AssetDatabaseLoader CreateAssetDatabaseLoader<T>(string assetPath, ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                                ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
                                                                where T : UnityEngine.Object
        {
            AssetDatabaseLoader assetDatabaseLoader = ObjectPool.Singleton.Pop<AssetDatabaseLoader>();
            AssetInfo assetInfo = ResourceModuleManager.Singleton.GetOrCreateAssetInfo<T>(assetPath);
            assetDatabaseLoader.Init(assetPath, typeof(T), assetInfo, loadType, loadMethod);
            return assetDatabaseLoader;
        }

        /// <summary>
        /// 创建AssetBundle模式指定Asset路径的Asset加载器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath">Asset路径(含后缀)</param>
        /// <param name="ownerABPath">所属AB路径</param>
        /// <param name="loadType">加载类型</param>
        /// <param name="loadMethod">加载方式</param>
        /// <returns></returns>
        public AssetLoader CreateBundleAssetLoader<T>(string assetPath, string ownerABPath = null,
                                                      ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                      ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
                                                      where T : UnityEngine.Object
        {
            BundleAssetLoader assetLoader = ObjectPool.Singleton.Pop<BundleAssetLoader>();
            AssetInfo assetInfo = ResourceModuleManager.Singleton.GetOrCreateAssetInfo<T>(assetPath, ownerABPath, loadType);
            assetLoader.Init(assetPath, typeof(T), assetInfo, loadType, loadMethod);
            return assetLoader;
        }

        /// <summary>
        /// 创建指定AssetBundle路径的AssetBundle加载器
        /// </summary>
        /// <param name="abPath">AB路径</param>
        /// <param name="depABPaths">依赖AB路径组</param>
        /// <param name="getABRealRelativePathDelegate">获取AB真实相对路径委托</param>
        /// <param name="loadType">加载类型</param>
        /// <param name="loadMethod">加载方式</param>
        /// <returns></returns>
        public BundleLoader CreateAssetBundleLoader<T>(string abPath, string[] depABPaths,
                                                       Func<string, string> getABRealRelativePathDelegate,
                                                       ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                       ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
                                                       where T : BundleLoader, new()
        {
            BundleLoader bundleLoader = ObjectPool.Singleton.Pop<T>();
            AssetBundleInfo assetBundleInfo = ResourceModuleManager.Singleton.GetOrCreateAssetBundleInfo(abPath, loadType);
            bundleLoader.Init(abPath, assetBundleInfo, depABPaths, getABRealRelativePathDelegate, loadType, loadMethod);
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
                if (loader is BundleAssetLoader bundleAssetLoader)
                {
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}删除Asset资源:{loader.ResourcePath}的加载器信息成功!");
                    ObjectPool.Singleton.Push(bundleAssetLoader);
                }
                else if(loader is AssetDatabaseLoader assetDatabaseLoader)
                {
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}删除AssetDatabase资源:{loader.ResourcePath}的加载器信息成功!");
                    ObjectPool.Singleton.Push(assetDatabaseLoader);
                }
                else if (loader is AssetBundleLoader assetBundleLoader)
                {
                    ResourceLogger.log($"Frame:{AbstractResourceModule.Frame}删除AssetBundle资源:{loader.ResourcePath}的加载器信息成功!");
                    ObjectPool.Singleton.Push(assetBundleLoader);
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
        /// <param name="assetName"></param>
        /// <returns></returns>
        public AssetLoader GetAssetLoader(string assetName)
        {
            Loadable assetLoader;
            if (mAllLoaderMap.TryGetValue(assetName, out assetLoader))
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
        public bool CancelAssetRequest(int requestUID)
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
        /// 将指定请求UID的Asset加载任务转为同步加载
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        private bool LoadAssetRequestImmediately(int requestUID)
        {
            string assetPath = GetAssetByRequestUID(requestUID);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"找不到请求UID:{requestUID}的Asset路径,Asset请求异步转同步失败!");
                return false;
            }
            var assetLoader = GetAssetLoader(assetPath);
            if (assetLoader == null || assetLoader.IsDone)
            {
                return false;
            }
            assetLoader.LoadImmediately();
            return true;
        }

        /// <summary>
        /// 获取指定请求UID的AssetBundle路径
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        private string GetABByRequestUID(int requestUID)
        {
            string assetBundlePath;
            if (mABRequestUIDMap.TryGetValue(requestUID, out assetBundlePath))
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
        public bool AddABRequestUID(int requestUID, string assetBundlePath)
        {
            if (!mABRequestUIDMap.ContainsKey(requestUID))
            {
                mABRequestUIDMap.Add(requestUID, assetBundlePath);
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
        public bool RemoveABRequestUID(int requestUID)
        {
            if (mABRequestUIDMap.Remove(requestUID))
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
        public bool CancelABRequest(int requestUID)
        {
            string assetBundlePath = GetABByRequestUID(requestUID);
            if (string.IsNullOrEmpty(assetBundlePath))
            {
                Debug.LogError($"找不到请求UID:{requestUID}的AssetBundle路径,取消AssetBundle请求失败!");
                return false;
            }
            var assetBundleLoader = GetAssetBundleLoader(assetBundlePath);
            return assetBundleLoader != null ? assetBundleLoader.CancelRequest(requestUID) : false;
        }

        /// <summary>
        /// 将指定请求UID的AssetBundle加载任务转为同步加载
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        private bool LoadABRequestImmediately(int requestUID)
        {
            string assetBundlePath = GetABByRequestUID(requestUID);
            if (string.IsNullOrEmpty(assetBundlePath))
            {
                Debug.LogError($"找不到请求UID:{requestUID}的AssetBundle路径,AssetBundle请求异步转同步失败!");
                return false;
            }
            var assetBundleLoader = GetAssetBundleLoader(assetBundlePath);
            if (assetBundleLoader == null || assetBundleLoader.IsDone)
            {
                return false;
            }
            assetBundleLoader.LoadImmediately();
            return true;
        }

        #region 调试用
        /// <summary>
        /// 获取所有AssetBundle加载器
        /// </summary>
        /// <returns></returns>
        public void GetAllAssetBundleLoader(ref List<BundleLoader> allABLoader)
        {
            allABLoader.Clear();
            foreach (var loader in mAllLoaderMap)
            {
                if(loader.Value is BundleLoader)
                {
                    allABLoader.Add(loader.Value as BundleLoader);
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
        public void GetAllWaitLoadedABLoader(ref List<BundleLoader> waitLoadedABLoader)
        {
            waitLoadedABLoader.Clear();
            foreach (var waitLoadedLoader in mAllWaitLoadLoaderList)
            {
                if (waitLoadedLoader is BundleLoader)
                {
                    waitLoadedABLoader.Add(waitLoadedLoader as BundleLoader);
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
