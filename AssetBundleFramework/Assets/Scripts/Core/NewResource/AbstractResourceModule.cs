/*
 * Description:             AbstractResourceModule.cs
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
    /// AbstractResourceModule.cs
    /// 资源模块抽象类
    /// </summary>
    public abstract class AbstractResourceModule
    {
        #region 资源加载管理部分
        /// <summary> 逻辑层资源加载完成委托 /// </summary>
        /// <param name="abinfo"></param>
        public delegate void LoadResourceCompleteHandler(AbstractResourceInfo abinfo);

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

        /// <summary>
        /// Asset资源请求任务映射map
        /// Key为Asset路径，Value为Asset资源加载任务对象
        /// </summary>
        public Dictionary<string, AssetLoader> AssetRequestTaskMap
        {
            get
            {
                return mAssetRequestTaskMap;
            }
        }
        private Dictionary<string, AssetLoader> mAssetRequestTaskMap;

        /// <summary>
        /// AB资源请求任务映射map
        /// Key为AB路径，Value为AB资源加载任务对象
        /// </summary>
        public Dictionary<string, AssetBundleLoader> ABRequestTaskMap
        {
            get
            {
                return mABRequestTaskMap;
            }
        }
        private Dictionary<string, AssetBundleLoader> mABRequestTaskMap;

        /// <summary>
        /// 已加载AB里不再有有效引用的AB信息列表
        /// </summary>
        protected List<AssetBundleInfo> mUnsedAssetBundleInfoList;

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

            mAssetRequestTaskMap = new Dictionary<string, AssetLoader>();
            mABRequestTaskMap = new Dictionary<string, AssetBundleLoader>();
            mUnsedAssetBundleInfoList = new List<AssetBundleInfo>();

            // TODO: 根据设备设定相关参数，改成读表控制
            mCheckUnsedABTimeInterval = 10.0f;
            mMaxUnloadABNumberPerFrame = 5;
            mABMinimumLifeTime = 40.0f;
            mResourceRecycleFPSThreshold = 20;

            mFPSUpdateInterval = 1.0f;
        }

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
        public int requstAsset<T>(string assetPath, out AssetLoader assetLoader, LoadResourceCompleteHandler completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync) where T : UnityEngine.Object
        {
            return realRequestAsset<T>(assetPath, out assetLoader, completeHandler, loadType, loadMethod);
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
        protected abstract int realRequestAsset<T>(string assetPath, out AssetLoader assetLoader, LoadResourceCompleteHandler completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync) where T : UnityEngine.Object;

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
        public int requstAssetBundle(string abPath, out AssetBundleLoader abLoader, LoadResourceCompleteHandler completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            return realRequestAssetBundle(abPath, out abLoader, completeHandler, loadType, loadMethod);
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
        protected abstract int realRequestAssetBundle(string abPath, out AssetBundleLoader abLoader, LoadResourceCompleteHandler completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync);

        /// <summary>
        /// 更新入口
        /// </summary>
        public virtual void Update()
        {
            mTotalDeltaTime += Time.deltaTime;
            mFrameCount++;
            if (mTotalDeltaTime >= mFPSUpdateInterval)
            {
                mCurrentFPS = (int)(mFrameCount / mTotalDeltaTime);
                mTotalDeltaTime = 0f;
                mFrameCount = 0;
            }
            checkUnsedResource();
        }

        /// <summary>
        /// 提供给外部的触发卸载所有正常加载不再使用的资源(递归判定，不限制回收数量)
        /// Note:
        /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
        /// </summary>
        public void unloadAllUnsedResources()
        {
            checkUnsedAssetBundleResources();
            doUnloadUnsedAssetBundleWithLimit(false);
        }

        /// <summary>
        /// 队列里不再有资源需要加载时检查不再使用的资源
        /// </summary>
        protected void checkUnsedResource()
        {
            if (!EnableResourceRecyclingUnloadUnsed)
            {
                return;
            }

            mUnloadUnsedABTotalDeltaTime += Time.deltaTime;
            if(mCurrentFPS >= mResourceRecycleFPSThreshold 
                && mUnloadUnsedABTotalDeltaTime > mCheckUnsedABTimeInterval 
                && mAssetRequestTaskMap.Count == 0 && mABRequestTaskMap.Count == 0)
            {
                // 因为Asset依赖加载是无法准确得知的，所以无法做到Asset级别准确卸载
                // 所以只对AssetBundle进行卸载检测判定
                // 而AssetBundle的卸载判定里包含了相关Asset使用计数信息为0和无有效绑定对象
                mUnloadUnsedABTotalDeltaTime = 0f;
                checkUnsedAssetBundleResources();
                doUnloadUnsedAssetBundleWithLimit(true);
            }
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
                    if ((time - loadedAssetBundleInfo.Value.LastUsedTime) > mABMinimumLifeTime)
                    {
                        mUnsedAssetBundleInfoList.Add(loadedAssetBundleInfo.Value);
                    }
                }
            }

            if (mUnsedAssetBundleInfoList.Count > 0)
            {
                // 根据最近使用时间升序排列
                mUnsedAssetBundleInfoList.Sort(ABILastUsedTimeSort);
            }
        }

        /// <summary>
        /// 卸载未使用的AssetBundle
        /// </summary>
        /// <param name="withLimitNumber">是否限制卸载数量</param>
        protected void doUnloadUnsedAssetBundleWithLimit(bool withLimitNumber = false)
        {
            for (int i = 0; i < mUnsedAssetBundleInfoList.Count; i++)
            {
                if (withLimitNumber == false || (withLimitNumber && i < mMaxUnloadABNumberPerFrame))
                {
                    mAllLoadedNormalAssetBundleInfoMap.Remove(mUnsedAssetBundleInfoList[i].ResourcePath);
                    mUnsedAssetBundleInfoList[i].dispose();
                }
                else
                {
                    break;
                }
            }
            mUnsedAssetBundleInfoList.Clear();
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