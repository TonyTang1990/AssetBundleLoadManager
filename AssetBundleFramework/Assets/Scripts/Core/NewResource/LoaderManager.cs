/*
 * Description:             LoaderManager.cs
 * Author:                  TONYTANG
 * Create Date:             2021/10/13
 */

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
        public void update()
        {
           if (HasLoadingTask)
           {
                mResourceLoadCountPerFrame = 0;
                mResourceLoadStartTime = Time.time;
                mResourceLoadTimePassed = 0;
                for (int i = 0; i < mAllWaitLoadLoaderList.Count; i++)
                {
                    mAllWaitLoadLoaderList[i].doLoad();
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
            return mNextRequestUID++;
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
            AssetDatabaseLoader assetDatabaseLoader = ObjectPool.Singleton.pop<AssetDatabaseLoader>();
            AssetInfo assetInfo = ResourceModuleManager.Singleton.CurrentResourceModule.getOrCreateAssetInfo<T>(assetPath);
            assetDatabaseLoader.init(assetPath, typeof(T), assetInfo, loadType, loadMethod);
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
            BundleAssetLoader assetLoader = ObjectPool.Singleton.pop<BundleAssetLoader>();
            AssetInfo assetInfo = ResourceModuleManager.Singleton.CurrentResourceModule.getOrCreateAssetInfo<T>(assetPath);
            assetLoader.init(assetPath, typeof(T), assetInfo, loadType, loadMethod);
            return assetLoader;
        }

        /// <summary>
        /// 创建指定AssetBundle路径的AssetBundle加载器
        /// </summary>
        /// <param name="assetBundlePath">AB路径</param>
        /// <param name="depABPaths">依赖AB路径组</param>
        /// <param name="loadType">加载类型</param>
        /// <returns></returns>
        public BundleLoader createAssetBundleLoader<T>(string assetBundlePath, string[] depABPaths, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync) where T : BundleLoader
        {
            BundleLoader bundleLoader = ObjectPool.Singleton.pop<T>();
            AssetBundleInfo assetBundleInfo = ResourceModuleManager.Singleton.CurrentResourceModule.getOrCreateAssetBundleInfo(assetBundlePath, loadType);
            bundleLoader.init(assetBundlePath, assetBundleInfo, depABPaths, loadType, loadMethod);
            return bundleLoader;
        }

        /// <summary>
        /// 添加Bundle加载器任务
        /// </summary>
        /// <param name="loader"></param>
        /// <returns></returns>
        public bool addLoadTask(Loadable loader)
        {
            if (mAllLoaderMap.ContainsKey(loader.ResourcePath))
            {
                Debug.LogError($"已经存在资源:{loader.ResourcePath}的加载器,添加资源加载器任务失败,不应该进入这里!");
                return false;
            }
            mAllLoaderMap.Add(loader.ResourcePath, loader);
            ResourceLogger.log($"添加资源:{loader.ResourcePath}加载器任务!");
            mAllWaitLoadLoaderList.Add(loader);
            return true;
        }

        /// <summary>
        /// 移除Bundle加载器任务
        /// </summary>
        /// <param name="loader"></param>
        /// <returns></returns>
        public bool removeLoadTask(Loadable loader)
        {
            var result = mAllWaitLoadLoaderList.Remove(loader);
            if (!result)
            {
                Debug.LogError($"找不到资源:{loader.ResourcePath}的加载器任务,移除失败,请检查代码流程!");
            }
            else
            {
                ResourceLogger.log($"移除资源:{loader.ResourcePath}的加载器任务成功!");
            }
            return result;
        }

        /// <summary>
        /// 删除指定资源加载器信息
        /// </summary>
        /// <param name="assetLoader"></param>
        /// <returns></returns>
        public bool deleteAssetLoader(AssetLoader assetLoader)
        {
            var result = mAllLoaderMap.Remove(assetLoader.ResourcePath);
            if (!result)
            {
                Debug.LogError($"找不到资源:{assetLoader.ResourcePath}的加载器信息,删除资源加载器信息失败,请检查代码流程!");
            }
            else
            {
                ResourceLogger.log($"删除资源:{assetLoader.ResourcePath}的加载器信息成功!");
            }
            return result;
        }

        /// <summary>
        /// 获取指定Asset路径的加载器
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public AssetLoader getAssetLoader(string assetPath)
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
        public BundleLoader getAssetBundleLoader(string assetBundlePath)
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
        private string getAssetByRequestUID(int requestUID)
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
        public bool addAssetRequestUID(int requestUID, string assetPath)
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
        public bool removeAssetRequestUID(int requestUID)
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
        public bool cancelAssetRequest<T>(int requestUID) where T : UnityEngine.Object
        {
            string assetPath = getAssetByRequestUID(requestUID);
            if (string.IsNullOrEmpty(assetPath))
            {
                Debug.LogError($"找不到请求UID:{requestUID}的Asset路径,取消Asset请求失败!");
                return false;
            }
            var assetLoader = getAssetLoader(assetPath);
            return assetLoader != null ? assetLoader.cancelRequest(requestUID) : false;
        }

        /// <summary>
        /// 获取指定请求UID的AssetBundle路径
        /// </summary>
        /// <param name="requestUID"></param>
        /// <returns></returns>
        private string getAssetBundleByRequestUID(int requestUID)
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
        public bool addAssetBundleRequestUID(int requestUID, string assetBundlePath)
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
        public bool removeAssetBundleRequestUID(int requestUID)
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
        public bool cancelAssetBundleRequest(int requestUID)
        {
            string assetBundlePath = getAssetBundleByRequestUID(requestUID);
            if (string.IsNullOrEmpty(assetBundlePath))
            {
                Debug.LogError($"找不到请求UID:{requestUID}的AssetBundle路径,取消AssetBundle请求失败!");
                return false;
            }
            var assetBundleLoader = getAssetBundleLoader(assetBundlePath);
            return assetBundleLoader != null ? assetBundleLoader.cancelRequest(requestUID) : false;
        }

    }
}