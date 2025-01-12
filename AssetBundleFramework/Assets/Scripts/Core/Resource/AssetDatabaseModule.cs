﻿/*
 * Description:             AssetDatabaseModule.cs
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
    /// AssetDatabaseModule.cs
    /// Editor模式资源加载模块管理类
    /// </summary>
    public class AssetDatabaseModule : AbstractResourceModule
    {
        /// <summary>
        /// 已加载Asset里不再有有效引用的Asset信息列表
        /// </summary>
        protected List<AssetInfo> mUnsedAssetInfoList;

        /// <summary>
        /// 资源加载模块初始化
        /// </summary>
        public override void Init()
        {
            base.Init();

            mUnsedAssetInfoList = new List<AssetInfo>();
            ResLoadMode = ResourceLoadMode.AssetDatabase;
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
        protected override int RealRequestAsset<T>(string assetPath, out AssetLoader assetLoader, Action<AssetLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            var requestUID = LoaderManager.Singleton.GetNextRequestUID();
            var assetDatabaseLoader = LoaderManager.Singleton.createAssetDatabaseLoader<T>(assetPath, loadType, loadMethod) as AssetDatabaseLoader;
            assetDatabaseLoader.AddRequest(requestUID, completeHandler);
            assetLoader = assetDatabaseLoader as AssetDatabaseLoader;
            assetDatabaseLoader.Load();
            return requestUID;
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
        protected override int RealRequestAssetBundle(string abPath, out BundleLoader abLoader, Action<BundleLoader, int> completeHandler, ResourceLoadType loadType = ResourceLoadType.NormalLoad, ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
        {
            // AssetDatabase模式不支持AssetBundle加载，直接返回逻辑回调
            abLoader = null;
            completeHandler?.Invoke(abLoader, 0);
            return 0;
        }

        /// <summary>
        /// 真正执行资源卸载指定类型不再使用的资源接口
        /// </summary>
        /// <param name="resourceloadtype"></param>
        protected override void DoUnloadSpecificLoadTypeUnsedResource(ResourceLoadType resourceloadtype)
        {
            // 递归判定卸载所有不再可用的正常加载资源
            bool hasUnusedRes = true;
            while (hasUnusedRes)
            {
                // 检查回收不再使用正常已加载的AB
                CheckUnusedResource();

                if (mUnsedAssetInfoList.Count == 0)
                {
                    //不再有可卸载的资源
                    hasUnusedRes = true;
                }
                else
                {
                    DoUnloadAllUnusedResources();
                }
            }

            // AssetDatabase模式无法得知依赖加载还原的Asset信息，所以无法使用Resources.UnloadAsset接口
            // 通过Resources.UnloadUnsedAssets回收逻辑层面满足可卸载的Asset资源
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 执行不再使用资源监察
        /// </summary>
        protected override void DoCheckUnusedResource()
        {
            base.DoCheckUnusedResource();
            CheckUnsedAssetResources();
            DoUnloadUnsedAssetWithLimit(true);
        }

        /// <summary>
        /// 检查未使用Asset
        /// </summary>
        protected void CheckUnsedAssetResources()
        {
            mUnsedAssetInfoList.Clear();
            var time = Time.time;
            // 检查正常加载的资源Asset，回收不再使用的Asset
            foreach (var loadedAssetInfo in mAllLoadedNormalAssetInfoMap)
            {
                if (loadedAssetInfo.Value.IsUnsed)
                {
                    // 强制卸载不需要判定有效资源生命时长
                    //if ((time - loadedAssetInfo.Value.LastUsedTime) > ResourceMinimumLifeTime)
                    mUnsedAssetInfoList.Add(loadedAssetInfo.Value);
                }
            }

            if (mUnsedAssetInfoList.Count > 0)
            {
                // 根据最近使用时间升序排列
                mUnsedAssetInfoList.Sort(AssetILastUsedTimeSort);
            }
        }

        /// <summary>
        /// 资源信息根据最近使用时间排序
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int AssetILastUsedTimeSort(AbstractResourceInfo a, AbstractResourceInfo b)
        {
            return a.LastUsedTime.CompareTo(b.LastUsedTime);
        }

        /// <summary>
        /// 卸载未使用的Asset
        /// </summary>
        /// <param name="withLimitNumber">是否限制卸载数量</param>
        protected void DoUnloadUnsedAssetWithLimit(bool withLimitNumber = false)
        {
            for (int i = 0; i < mUnsedAssetInfoList.Count; i++)
            {
                if (withLimitNumber == false || (withLimitNumber && i < MaxUnloadABNumberPerFrame))
                {
                    DeleteAssetInfo(mUnsedAssetInfoList[i].ResourcePath);
                }
                else
                {
                    break;
                }
            }
            mUnsedAssetInfoList.Clear();
        }

        /// <summary>
        /// 强制卸载所有资源(只在特定情况下用 e.g. 热更后卸载所有已加载资源后重新初始化加载AB资源)***慎用***
        /// </summary>
        public override void ForceUnloadAllResources()
        {
            // 强制清除待卸载AssetInfo信息，避免强制清除后依然触发清理报错
            mUnsedAssetInfoList.Clear();
            var assetPathList = new List<string>(mAllLoadedNormalAssetInfoMap.Keys);
            assetPathList.AddRange(mAllLoadedPermanentAssetInfoMap.Keys);
            foreach (var assetPath in assetPathList)
            {
                if (mAllLoadedNormalAssetInfoMap.ContainsKey(assetPath) ||
                    mAllLoadedPermanentAssetInfoMap.ContainsKey(assetPath))
                {
                    DeleteAssetInfo(assetPath);
                }
            }
        }
    }
}