/*
 * Description:             AbstractResourceModule.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/24
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 设计与功能支持介绍:
// 1.面向Asset级别加载管理，支持Asset和AssetBundle级别的同步异步加载。
// 2.支持资源导入后AssetDatabase模式马上就能配置全路径加载
// 3.资源加载类型只提供普通和常驻两种(且不支持运行时切换相同Asset或AssetBundle的加载类型，意味着一旦第一次加载设定了类型，就再也不能改变，同时第一次因为加载Asset而加载某个AssetBundle的加载类型和Asset一致)，同时提供统一的加载管理策略，细节管理策略由上层自己设计(比如对象池，预加载)
// 4.新版异步加载准备采用监听回调的方式来实现，保证流程清晰易懂
// 5.新版设计请求UID的概念来支持加载打断设计(仅逻辑层面的打断，资源加载不会打断，当所有逻辑回调都取消时，加载完成时会返还索引计数确保资源正确卸载)
// 6.设计上支持动态AB下载(TODO:未来填坑)
// 7.保留索引计数(Asset和AssetBundle级别) + 对象绑定的设计(Asset和AssetBundle级别) + 按AssetBundle级别卸载(依赖还原的Asset无法准确得知所以无法直接卸载Asset) + 加载触发就提前计数(避免异步加载或异步加载打断情况下资源管理异常)
// 8.支持非回调式的同步加载返回(通过抽象Loader支持LoadImmediately的方式实现)

// 加载器流程介绍:
// 1. load                      -- 触发资源加载
// 2. loadImmediately           -- 触发立刻加载资源完成
// 3. doLoad                    -- 真正触发加载
// 4. cancelRequest             -- 取消指定请求
// 5. onLoad                    -- 响应资源加载
// 6. failed                    -- 触发资源加载失败
// 7. onFailed                  -- 响应资源加载失败
// 8. cancel                    -- 触发资源加载取消
// 9. onCancel                  -- 响应资源加载取消
// 10. complete                 -- 触发资源加载完成
// 11. onComplete               -- 响应资源加载完成

// 索引计数设计:
// - 上层调用Asset或AssetBundel资源加载接口时就会创建相关Asset和AssetBundle信息并添加计数
// - 加载Asset时会添加Asset自身和所属AssetBundle以及依赖AssetBundle的计数，
//   在Asset加载完成后会返回Asset和所属AssetBundle以及依赖AB的计数，最后把计数和绑定交由上层决定
// - 加载AssetBundle时会添加AssetBundle自身以及依赖AB的计数,
//   在AssetBundle加载完成后返还AssetBundle计数(不返还依赖AssetBundle计数，确保依赖计数只加一次)，最后把计数和绑定交由上层决定
// - 相同AssetBundle里的不同Asset加载会给所属AssetBundle的依赖AssetBundle只添加一次计数
// - 

namespace TResource
{
    /// <summary>
    /// AbstractResourceModule.cs
    /// 资源模块抽象类
    /// </summary>
    public abstract class AbstractResourceModule
    {
        #region 资源加载管理部分
        /// <summary>
        /// 资源加载模式
        /// </summary>
        public ResourceLoadMode ResLoadMode
        {
            get;
            protected set;
        }

        /// <summary>
        /// 是否开启资源回收检测(有些情况下不适合频繁回收创建，比如战斗场景)
        /// </summary>
        public bool EnableResourceRecyclingUnloadUnsed
        {
            get;
            set;
        }

        /// <summary>
        /// 普通加载方式所有已加载的Asset资源的信息映射Map
        /// Key为Asset路径，Value为Asset路径已经加载的资源信息映射Map(Key为Asset路径，Value为Asset资源加载信息)
        /// </summary>
        protected Dictionary<string, AssetInfo> mAllLoadedNormalAssetInfoMap;

        /// <summary>
        /// 常驻加载方式所有已加载的Asset资源的信息映射Map
        /// Key为Asset路径，Value为Asset路径已经加载的资源信息映射Map(Key为Asset路径，Value为Asset资源加载信息)
        /// </summary>
        protected Dictionary<string, AssetInfo> mAllLoadedPermanentAssetInfoMap;

        /// <summary>
        /// 普通加载方式所有已加载的资源的信息映射Map
        /// Key为AB路径，Value为AB路径已加载的资源信息映射Map(Key为AB路径，Value为AssetBundle资源加载信息)
        /// </summary>
        protected Dictionary<string, AssetBundleInfo> mAllLoadedNormalAssetBundleInfoMap;

        /// <summary>
        /// 常驻加载方式所有已加载的资源的信息映射Map
        /// Key为AB路径，Value为AB路径已加载的资源信息映射Map(Key为AB路径，Value为AssetBundle资源加载信息)
        /// </summary>
        protected Dictionary<string, AssetBundleInfo> mAllLoadedPermanentAssetBundleInfoMap;

        /// <summary> 检测未使用AB时间间隔(在请求队列为空时才检测未使用AB) /// </summary>
        protected float mCheckUnsedABTimeInterval;

        /// <summary>
        /// 单帧卸载的AB最大数量
        /// 避免单帧卸载过多AB导致卡顿
        /// </summary>
        protected int mMaxUnloadABNumberPerFrame;

        /// <summary>
        /// AB最短的有效生存时间
        /// 用于避免短时间内频繁删除卸载同一个AB的情况(比如同一个窗口AB资源不断重复打开关闭)
        /// </summary>
        protected float mABMinimumLifeTime;

        /// <summary>
        /// 资源回收帧率门槛(避免帧率过低的时候回收资源造成过卡)
        /// </summary>
        protected int mResourceRecycleFPSThreshold;

        /// <summary>
        /// AB资源卸载经历的时间
        /// </summary>
        private float mUnloadUnsedABTotalDeltaTime;
        #endregion

        #region AB模式相关信息获取(采用延迟绑定的方式来解决AB模式下特有的AB信息获取)
        /// <summary>
        /// 获取指定Asset路径的AB路径委托
        /// </summary>
        protected Func<string, string> GetAssetPathBundleNameDelegate;

        /// <summary>
        /// 获取指定AB路径的依赖AB路径信息委托
        /// </summary>
        protected Func<string, string[]> GetAssetBundleDpInfoDelegate;
        #endregion

        #region FSP计算部分
        /// <summary>
        /// 当前FPS
        /// </summary>
        public int CurrentFPS
        {
            get
            {
                return mCurrentFPS;
            }
        }
        protected int mCurrentFPS;

        /// <summary>
        /// 经历的时间
        /// </summary>
        private float mTotalDeltaTime;

        /// <summary>
        /// 经历的帧数
        /// </summary>
        private int mFrameCount;

        /// <summary>
        /// FPS更新间隔频率
        /// </summary>
        private float mFPSUpdateInterval;
        #endregion

        /// <summary>
        /// 资源加载模块初始化
        /// </summary>
        public virtual void init()
        {
            ResLoadMode = ResourceLoadMode.Invalide;
            EnableResourceRecyclingUnloadUnsed = true;
            mAllLoadedNormalAssetInfoMap = new Dictionary<string, AssetInfo>();
            mAllLoadedPermanentAssetInfoMap = new Dictionary<string, AssetInfo>();
            mAllLoadedNormalAssetBundleInfoMap = new Dictionary<string, AssetBundleInfo>();
            mAllLoadedPermanentAssetBundleInfoMap = new Dictionary<string, AssetBundleInfo>();

            // TODO: 根据设备设定相关参数，改成读表控制
            mCheckUnsedABTimeInterval = 10.0f;
            mMaxUnloadABNumberPerFrame = 5;
            mABMinimumLifeTime = 40.0f;
            mResourceRecycleFPSThreshold = 20;

            mFPSUpdateInterval = 1.0f;
        }

        #region 资源加载信息部分
        /// <summary>
        /// 获取或创建指定Asset路径的Asset信息
        /// </summary>
        /// <param name="assetPath">Asset路径</param>
        /// <param name="ownerAssetBundlePath">所属AB路径</param>
        /// <param name="loadType">加载类型</param>
        /// <returns></returns>
        public AssetInfo getOrCreateAssetInfo<T>(string assetPath, string ownerAssetBundlePath = null, ResourceLoadType loadType = ResourceLoadType.NormalLoad) where T : UnityEngine.Object
        {
            AssetInfo assetInfo = getAssetInfo(assetPath);
            if(assetInfo == null)
            {
                assetInfo = createAssetInfo<T>(assetPath, ownerAssetBundlePath, loadType);
                addAssetInfo(assetInfo);
            }
            return assetInfo;
        }

        /// <summary>
        /// 获取指定Asset路径的Asset信息
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public AssetInfo getAssetInfo(string assetPath)
        {
            AssetInfo assetInfo;
            if (mAllLoadedNormalAssetInfoMap.TryGetValue(assetPath, out assetInfo))
            {
                return assetInfo;
            }
            if (mAllLoadedPermanentAssetInfoMap.TryGetValue(assetPath, out assetInfo))
            {
                return assetInfo;
            }
            return null;
        }

        /// <summary>
        /// 获取指定Asset路径的Asset信息
        /// </summary>
        /// <param name="assetPath">Asset路径</param>
        /// <param name="ownerAssetBundlePath">所属AB路径</param>
        /// <param name="loadType">加载类型</param>
        /// <returns></returns>
        protected AssetInfo createAssetInfo<T>(string assetPath, string ownerAssetBundlePath = null, ResourceLoadType loadType = ResourceLoadType.NormalLoad) where T : UnityEngine.Object
        {
            AssetInfo assetInfo = ObjectPool.Singleton.pop<AssetInfo>();
            assetInfo.init(assetPath, typeof(T), ownerAssetBundlePath, loadType);
            return assetInfo;
        }

        /// <summary>
        /// 添加指定Asset路径的Asset信息
        /// </summary>
        /// <param name="assetInfo"></param>
        /// <returns></returns>
        protected bool addAssetInfo(AssetInfo assetInfo)
        {
            if (!mAllLoadedNormalAssetInfoMap.ContainsKey(assetInfo.ResourcePath) && !mAllLoadedPermanentAssetInfoMap.ContainsKey(assetInfo.ResourcePath))
            {
                if (assetInfo.LoadType == ResourceLoadType.NormalLoad)
                {
                    mAllLoadedNormalAssetInfoMap.Add(assetInfo.ResourcePath, assetInfo);
                    return true;
                }
                else if (assetInfo.LoadType == ResourceLoadType.PermanentLoad)
                {
                    mAllLoadedPermanentAssetInfoMap.Add(assetInfo.ResourcePath, assetInfo);
                    return true;
                }
                else
                {
                    Debug.LogError($"不支持的加载类型:{assetInfo.LoadType}");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"重复添加Asset:{assetInfo.ResourcePath}的信息,添加失败，请检查代码!");
                return false;
            }
        }

        /// <summary>
        /// 删除指定Asset路径的Asset信息
        /// Note:
        /// 只支持删除NormalLoad方式的Asset信息
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public bool deleteAssetInfo(string assetPath)
        {
            if(mAllLoadedNormalAssetInfoMap.Remove(assetPath))
            {
                return true;
            }
            else
            {
                Debug.LogError($"不存在NormalLoad的AssetPath信息,删除AssetPath:{assetPath}信息失败!");
                return false;
            }
        }

        /// <summary>
        /// 获取或创建指定AssetBundle路径的AssetBundle信息
        /// </summary>
        /// <param name="assetBundlePath">AB路径</param>
        /// <param name="loadType">加载类型</param>
        /// <returns></returns>
        public AssetBundleInfo getOrCreateAssetBundleInfo(string assetBundlePath, ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            AssetBundleInfo assetBundleInfo = getAssetBundleInfo(assetBundlePath);
            if (assetBundleInfo == null)
            {
                var depAssetBundlePaths = GetAssetBundleDpInfoDelegate(assetBundlePath);
                assetBundleInfo = createAssetBundleInfo(assetBundlePath, depAssetBundlePaths, loadType);
                addAssetBundleInfo(assetBundleInfo);
            }
            return assetBundleInfo;
        }

        /// <summary>
        /// 获取指定AssetBundle路径的AssetBundle信息
        /// </summary>
        /// <param name="assetBundlePath"></param>
        /// <returns></returns>
        public AssetBundleInfo getAssetBundleInfo(string assetBundlePath)
        {
            AssetBundleInfo assetBundleInfo;
            if (mAllLoadedNormalAssetBundleInfoMap.TryGetValue(assetBundlePath, out assetBundleInfo))
            {
                return assetBundleInfo;
            }
            if (mAllLoadedPermanentAssetBundleInfoMap.TryGetValue(assetBundlePath, out assetBundleInfo))
            {
                return assetBundleInfo;
            }
            return null;
        }

        /// <summary>
        /// 创建指定AssetBundle路径的AssetBundle信息
        /// </summary>
        /// <param name="assetBundlePath"></param>
        /// <param name="depAssetBundlePaths">依赖AB路径列表</param>
        /// <param name="loadType">加载类型</param>
        /// <returns></returns>
        protected AssetBundleInfo createAssetBundleInfo(string assetBundlePath, string[] depAssetBundlePaths, ResourceLoadType loadType = ResourceLoadType.NormalLoad)
        {
            AssetBundleInfo assetBundleInfo = ObjectPool.Singleton.pop<AssetBundleInfo>();
            assetBundleInfo.init(assetBundlePath, depAssetBundlePaths, loadType);
            return assetBundleInfo;
        }

        /// <summary>
        /// 添加指定AssetBundle路径的AssetBundle信息
        /// </summary>
        /// <param name="assetBundleInfo"></param>
        /// <returns></returns>
        protected bool addAssetBundleInfo(AssetBundleInfo assetBundleInfo)
        {
            if (!mAllLoadedNormalAssetBundleInfoMap.ContainsKey(assetBundleInfo.ResourcePath) && !mAllLoadedPermanentAssetBundleInfoMap.ContainsKey(assetBundleInfo.ResourcePath))
            {
                if (assetBundleInfo.LoadType == ResourceLoadType.NormalLoad)
                {
                    mAllLoadedNormalAssetBundleInfoMap.Add(assetBundleInfo.ResourcePath, assetBundleInfo);
                    return true;
                }
                else if (assetBundleInfo.LoadType == ResourceLoadType.PermanentLoad)
                {
                    mAllLoadedPermanentAssetBundleInfoMap.Add(assetBundleInfo.ResourcePath, assetBundleInfo);
                    return true;
                }
                else
                {
                    Debug.LogError($"不支持的加载类型:{assetBundleInfo.LoadType}");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"重复添加AssetBundle:{assetBundleInfo.ResourcePath}的信息,添加失败，请检查代码!");
                return false;
            }
        }

        /// <summary>
        /// 删除指定AssetBundle路径的AssetBundle信息
        /// Note:
        /// 只支持删除NormalLoad方式的AB信息
        /// </summary>
        /// <param name="assetBundlePath"></param>
        /// <returns></returns>
        public bool deleteAssetBundleInfo(string assetBundlePath)
        {
            if(mAllLoadedNormalAssetBundleInfoMap.Remove(assetBundlePath))
            {
                return true;
            }
            else
            {
                Debug.LogError($"不存在NormalLoad的AssetBundlePath信息,删除AssetBundlePath:{assetBundlePath}信息失败!");
                return false;
            }
        }

        /// <summary>
        /// 获取指定加载方式的Asset已加载信息映射Map
        /// </summary>
        /// <param name="loadtype"></param>
        /// <returns></returns>
        public Dictionary<string, AssetInfo> getSpecificLoadTypeAssetInfoMap(ResourceLoadType loadtype)
        {
            if(loadtype == ResourceLoadType.NormalLoad)
            {
                return mAllLoadedNormalAssetInfoMap;
            }
            else if(loadtype == ResourceLoadType.PermanentLoad)
            {
                return mAllLoadedPermanentAssetInfoMap;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取指定加载方式的AssetBundle已加载信息映射Map
        /// </summary>
        /// <param name="loadtype"></param>
        /// <returns></returns>
        public Dictionary<string, AssetBundleInfo> getSpecificLoadTypeAssetBundleInfoMap(ResourceLoadType loadtype)
        {
            if (loadtype == ResourceLoadType.NormalLoad)
            {
                return mAllLoadedNormalAssetBundleInfoMap;
            }
            else if (loadtype == ResourceLoadType.PermanentLoad)
            {
                return mAllLoadedPermanentAssetBundleInfoMap;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 释放指定AssetPath的计数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool releaseAsset(string assetPath)
        {
            var assetInfo = getAssetInfo(assetPath);
            assetInfo?.release();
            return assetInfo != null;
        }

        /// <summary>
        /// 为AssetPath解除指定owner的引用
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public bool unbindAsset(string assetPath, UnityEngine.Object owner)
        {
            var assetInfo = getAssetInfo(assetPath);
            assetInfo?.releaseOwner(owner);
            return assetInfo != null;
        }

        /// <summary>
        /// 释放指定AssetBundlePath的计数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool releaseAssetBundle(string assetBundlePath)
        {
            var assetBundleInfo = getAssetBundleInfo(assetBundlePath);
            assetBundleInfo?.release();
            return assetBundleInfo != null;
        }

        /// <summary>
        /// 为AssetBundlePath解除指定owner的引用
        /// </summary>
        /// <param name="assetBundlePath"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public bool unbindAssetBundle(string assetBundlePath, UnityEngine.Object owner)
        {
            var assetBundleInfo = getAssetBundleInfo(assetBundlePath);
            assetBundleInfo?.releaseOwner(owner);
            return assetBundleInfo != null;
        }
        #endregion

        /// <summary>
        /// 请求Asset资源
        /// 上层资源加载统一入口
        /// </summary>
        /// <param name="assetPath">Asset资源路径(带后缀)</param>
        /// <param name="assetLoader">Asset资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        public int requstAsset<T>(string assetPath, out AssetLoader assetLoader, Action<AssetLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync) where T : UnityEngine.Object
        {
            // 统一小写
            assetPath = assetPath.ToLower();
            assetLoader = LoaderManager.Singleton.getAssetLoader(assetPath);
            if (assetLoader != null)
            {
                if(assetLoader.IsDone)
                {
                    completeHandler.Invoke(assetLoader, 0);
                    return 0;
                }
                else
                {
                    var requestUID = LoaderManager.Singleton.GetNextRequestUID();
                    assetLoader.addLoadAssetCompleteCallBack(requestUID, completeHandler);
                    // 异步转同步加载的情况
                    if (loadMethod == ResourceLoadMethod.Sync)
                    {
                        assetLoader.loadImmediately();
                    }
                    return requestUID;
                }
            }
            else
            {
                return realRequestAsset<T>(assetPath, out assetLoader, completeHandler, loadType, loadMethod);
            }
        }

        /// <summary>
        /// 真正的请求Asset资源(由不同的资源模块去实现)
        /// </summary>
        /// <param name="assetPath">Asset资源路径(带后缀)</param>
        /// <param name="assetLoader">Asset资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        protected abstract int realRequestAsset<T>(string assetPath, out AssetLoader assetLoader, Action<AssetLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync) where T : UnityEngine.Object;

        /// <summary>
        /// 请求AssetBundle
        /// 上层AssetBundle加载统一入口
        /// </summary>
        /// <param name="abPath">AssetBundle资源路径</param>
        /// <param name="abLoader">AB资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        public int requstAssetBundle(string abPath, out BundleLoader abLoader, Action<BundleLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            // 统一小写
            abPath = abPath.ToLower();
            abLoader = LoaderManager.Singleton.getAssetBundleLoader(abPath);
            if (abLoader != null)
            {
                if (abLoader.IsDone)
                {
                    completeHandler.Invoke(abLoader, 0);
                    return 0;
                }
                else
                {
                    var requestUID = LoaderManager.Singleton.GetNextRequestUID();
                    abLoader.addLoadABCompleteCallBack(requestUID, completeHandler);
                    // 异步转同步加载的情况
                    if (loadMethod == ResourceLoadMethod.Sync)
                    {
                        abLoader.loadImmediately();
                    }
                    return requestUID;
                }
            }
            else
            {
                return realRequestAssetBundle(abPath, out abLoader, completeHandler, loadType, loadMethod);
            }
        }

        /// <summary>
        /// 真正的请求AssetBundle资源(由不同的资源模块去实现)
        /// </summary>
        /// <param name="abPath">AssetBundle资源路径</param>
        /// <param name="abLoader">AB资源加载器</param>
        /// <param name="completeHandler">加载完成上层回调</param>
        /// <param name="loadType">资源加载类型</param>
        /// <param name="loadMethod">资源加载方式</param>
        /// <returns>请求UID</returns>
        protected abstract int realRequestAssetBundle(string abPath, out BundleLoader abLoader, Action<BundleLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync);

        /// <summary>
        /// 更新入口
        /// </summary>
        public virtual void update()
        {
            LoaderManager.Singleton.update();
            mTotalDeltaTime += Time.deltaTime;
            mFrameCount++;
            if (mTotalDeltaTime >= mFPSUpdateInterval)
            {
                mCurrentFPS = (int)(mFrameCount / mTotalDeltaTime);
                mTotalDeltaTime = 0f;
                mFrameCount = 0;
            }
            checkUnusedResource();
        }

        /// <summary>
        /// 提供给外部的触发卸载所有正常加载不再使用的资源(递归判定，不限制回收数量)
        /// Note:
        /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
        /// </summary>
        public void unloadAllUnusedResources()
        {
            doUnloadAllUnusedResources();
        }

        /// <summary>
        /// 执行卸载所有不再使用的资源
        /// </summary>
        protected virtual void doUnloadAllUnusedResources()
        {

        }

        /// <summary>
        /// 队列里不再有资源需要加载时检查不再使用的资源
        /// </summary>
        protected void checkUnusedResource()
        {
            if (!EnableResourceRecyclingUnloadUnsed)
            {
                return;
            }

            mUnloadUnsedABTotalDeltaTime += Time.deltaTime;
            if(mCurrentFPS >= mResourceRecycleFPSThreshold 
                && mUnloadUnsedABTotalDeltaTime > mCheckUnsedABTimeInterval 
                && LoaderManager.Singleton.HasLoadingTask)
            {
                // 因为Asset依赖加载是无法准确得知的，所以无法做到Asset级别准确卸载
                // 所以只对AssetBundle进行卸载检测判定
                // 而AssetBundle的卸载判定里包含了相关Asset使用计数信息为0和无有效绑定对象
                mUnloadUnsedABTotalDeltaTime = 0f;
                doCheckUnusedResource();
            }
        }

        /// <summary>
        /// 执行不再使用资源监察
        /// </summary>
        protected virtual void doCheckUnusedResource()
        {

        }

        #region 调试开发工具
        /// <summary>
        /// 获取正常已加载不可用的Asset数量
        /// </summary>
        public int getNormalUnsedAssetNumber()
        {
            var unsednumber = 0;
            // 检查回收不再使用的AB
            foreach (var loadedab in mAllLoadedNormalAssetInfoMap)
            {
                if (loadedab.Value.IsUnsed)
                {
                    unsednumber++;
                }
            }
            return unsednumber;
        }

        /// <summary>
        /// 获取正常已加载不可用的Asset数量
        /// </summary>
        public int getNormalUnsedABNumber()
        {
            var unsednumber = 0;
            // 检查回收不再使用的AB
            foreach (var loadedab in mAllLoadedNormalAssetBundleInfoMap)
            {
                if (loadedab.Value.IsUnsed)
                {
                    unsednumber++;
                }
            }
            return unsednumber;
        }
        #endregion
    }
}