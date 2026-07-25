/*
 * Description:             ResourceScope.cs
 * Author:                  TONYTANG
 * Create Date:             2026/7/23
 */

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// ResourceScope.cs
    /// 指定逻辑作用域内的资源计数释放+请求打断管理器
    /// Note：
    ///     功能：
    ///         1. 负责资源计数统计和资源释放
    ///         2. 负责异步未加载完成的资源请求取消
    ///     设计：
    ///         1. 只记录通过索引计数持有的资源，不记录Owner绑定的资源
    ///         2. 只保存资源路径，释放时重新从LoaderManager获取Loader
    ///         3. Asset和AssetBundle路径约定不会重合，因此统一使用资源路径作为Key
    ///         4. ReleaseAll后仍可继续获取和记录资源
    /// </summary>
    public sealed class ResourceScope
    {
        /// <summary>
        /// 资源路径和当前Scope持有计数Map
        /// </summary>
        private readonly Dictionary<string, int> mResourceCountMap = new Dictionary<string, int>(StringComparer.Ordinal);

        /// <summary>
        /// 当前Scope未完成的资源请求Handle集合
        /// </summary>
        private readonly HashSet<ResourceRequestHandle> mRequestHandleSet = new HashSet<ResourceRequestHandle>();

        /// <summary>
        /// 当前记录的资源数量
        /// </summary>
        public int ResourceCount
        {
            get
            {
                return mResourceCountMap.Count;
            }
        }

        /// <summary>
        /// 当前记录的未完成资源请求数量
        /// </summary>
        public int RequestCount
        {
            get
            {
                return mRequestHandleSet.Count;
            }
        }

        /// <summary>
        /// 当前记录的资源引用总数
        /// </summary>
        public int TotalReferenceCount
        {
            get
            {
                int totalReferenceCount = 0;
                foreach (var resourceCount in mResourceCountMap.Values)
                {
                    totalReferenceCount += resourceCount;
                }
                return totalReferenceCount;
            }
        }

        /// <summary>
        /// 获取指定Asset并记录本次增加的索引计数
        /// 同时兼容BundleAssetLoader和AssetDatabaseLoader
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetLoader"></param>
        /// <returns></returns>
        public T GetAsset<T>(AssetLoader assetLoader) where T : UnityEngine.Object
        {
            if (assetLoader == null)
            {
                Debug.LogError("ResourceScope获取Asset失败，AssetLoader为空!");
                return null;
            }

            var asset = assetLoader.GetAsset<T>();
            if (asset != null)
            {
                RecordResource(assetLoader.ResourcePath);
            }
            return asset;
        }

        /// <summary>
        /// 获取指定Asset(不增加索引计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetLoader"></param>
        /// <returns></returns>
        public T ObtainAsset<T>(AssetLoader assetLoader) where T : UnityEngine.Object
        {
            if (assetLoader == null)
            {
                Debug.LogError("ResourceScope获取Asset失败，AssetLoader为空!");
                return null;
            }

            var asset = assetLoader.ObtainAsset<T>();
            return asset;
        }
        
        /// <summary>
        /// 为Asset添加指定owner的对象绑定并返回该Asset
        /// 所有owner都销毁且所属ab引用计数归零可回收
        /// Note:
        /// ResourceScope只是为了统一资源使用相关接口，实际上并不负责对象绑定的清理
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetLoader"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public T BindAsset<T>(AssetLoader assetLoader, UnityEngine.Object owner) where T : UnityEngine.Object
        {
            if(assetLoader == null)
            {
                Debug.LogError("ResourceScope绑定Asset失败，AssetLoader为空!");
                return null;
            }
            return assetLoader.BindAsset<T>(owner);
        }

        /// <summary>
        /// 获取指定SubAsset并记录主Asset本次增加的索引计数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetLoader"></param>
        /// <param name="subAssetName"></param>
        /// <returns></returns>
        public T GetSubAsset<T>(AssetLoader assetLoader, string subAssetName) where T : UnityEngine.Object
        {
            if (assetLoader == null)
            {
                Debug.LogError("ResourceScope获取SubAsset失败，AssetLoader为空!");
                return null;
            }

            var subAsset = assetLoader.GetSubAsset<T>(subAssetName);
            if (subAsset != null)
            {
                RecordResource(assetLoader.ResourcePath);
            }
            return subAsset;
        }

        /// <summary>
        /// 获取指定SubAsset(不增加索引计数)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetLoader"></param>
        /// <param name="subAssetName"></param>
        /// <returns></returns>
        public T ObtainSubAsset<T>(AssetLoader assetLoader, string subAssetName) where T : UnityEngine.Object
        {
            if (assetLoader == null)
            {
                Debug.LogError("ResourceScope获取SubAsset失败，AssetLoader为空!");
                return null;
            }

            var subAsset = assetLoader.ObtainSubAsset<T>(subAssetName);
            return subAsset;
        }

        /// <summary>
        /// 获取指定AssetBundle并记录本次增加的索引计数
        /// </summary>
        /// <param name="bundleLoader"></param>
        /// <returns></returns>
        public AssetBundle GetAssetBundle(BundleLoader bundleLoader)
        {
            if (bundleLoader == null)
            {
                Debug.LogError("ResourceScope获取AssetBundle失败，BundleLoader为空!");
                return null;
            }

            var assetBundle = bundleLoader.GetAssetBundle();
            if (assetBundle != null)
            {
                RecordResource(bundleLoader.ResourcePath);
            }
            return assetBundle;
        }

        /// <summary>
        /// 获取指定资源在当前Scope中记录的索引计数
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public int GetReferenceCount(string resourcePath)
        {
            if(string.IsNullOrEmpty(resourcePath))
            {
                Debug.LogError("ResourceScope不允许获取空资源路径的索引计数!");
                return 0;
            }
            if(!mResourceCountMap.TryGetValue(resourcePath, out var resourceCount))
            {
                Debug.LogError($"ResourceScope未记录资源:{resourcePath}的索引计数，无法获取索引计数!");
                return 0;
            }
            return resourceCount;
        }

        /// <summary>
        /// 记录一次已经由外部增加的资源索引计数
        /// Note:
        /// 本方法只记录计数，不会为资源实际增加索引计数
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        private bool RecordResource(string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                Debug.LogError("ResourceScope不允许记录空资源路径!");
                return false;
            }

            int resourceCount;
            if (mResourceCountMap.TryGetValue(resourcePath, out resourceCount))
            {
                mResourceCountMap[resourcePath] = resourceCount + 1;
            }
            else
            {
                mResourceCountMap.Add(resourcePath, 1);
            }
            return true;
        }

        /// <summary>
        /// 释放指定资源名(含后缀)在当前Scope中记录的指定次数索引计数
        /// </summary>
        /// <param name="resourceName"></param>
        /// <param name="releaseCount"></param>
        /// <returns></returns>
        public bool ReleaseResourceByName(string resourceName, int releaseCount = 1)
        {
            if (string.IsNullOrEmpty(resourceName))
            {
                Debug.LogError("ResourceScope不允许释放空资源名称!");
                return false;
            }

            var resourcePath = ResourceModuleManager.Singleton.GetAssetPath(resourceName);
            return ReleaseResource(resourcePath, releaseCount);
        }

        /// <summary>
        /// 释放指定资源在当前Scope中记录的指定次数索引计数
        /// 释放次数超过已记录数量时只释放已记录数量
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <param name="releaseCount"></param>
        /// <returns></returns>
        public bool ReleaseResource(string resourcePath, int releaseCount = 1)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                Debug.LogError("ResourceScope不允许释放空资源路径!");
                return false;
            }
            if (releaseCount <= 0)
            {
                Debug.LogError($"资源:{resourcePath}释放次数:{releaseCount}无效，释放次数必须大于0!");
                return false;
            }

            int recordedCount;
            if (!mResourceCountMap.TryGetValue(resourcePath, out recordedCount))
            {
                Debug.LogWarning($"ResourceScope未记录资源:{resourcePath}的索引计数，释放失败!");
                return false;
            }

            var loader = LoaderManager.Singleton.GetLoader(resourcePath);
            // 有三种情况可能出现以下loader为null：
            // 1. 传入错误(不归属当前ResourceScope统计的)的资源卸载
            // 2. 资源被错误卸载(比如被错误减计数)
            // 3. 资源被强制卸载
            if (loader == null)
            {
                Debug.LogWarning($"ResourceScope释放资源:{resourcePath}失败，找不到对应Loader，强制清理该资源计数!");
                mResourceCountMap.Remove(resourcePath);
                return false;
            }

            var actualReleaseCount = Math.Min(releaseCount, recordedCount);
            if (!ReleaseResourceByLoader(loader, actualReleaseCount))
            {
                return false;
            }

            var remainCount = recordedCount - actualReleaseCount;
            if (remainCount > 0)
            {
                mResourceCountMap[resourcePath] = remainCount;
            }
            else
            {
                mResourceCountMap.Remove(resourcePath);
            }
            return true;
        }

        /// <summary>
        /// 释放指定资源在当前Scope中记录的所有索引计数
        /// </summary>
        /// <param name="resourcePath"></param>
        /// <returns></returns>
        public bool ReleaseResourceAll(string resourcePath)
        {
            int recordedCount;
            if (!string.IsNullOrEmpty(resourcePath) &&
                mResourceCountMap.TryGetValue(resourcePath, out recordedCount))
            {
                return ReleaseResource(resourcePath, recordedCount);
            }
            return false;
        }

        /// <summary>
        /// 释放当前Scope记录的所有资源索引计数
        /// 释放失败的资源记录会被保留，当前Scope后续仍可继续使用
        /// </summary>
        /// <returns></returns>
        private bool ReleaseAll()
        {
            if(mResourceCountMap.Count == 0)
            {
                return true;
            }
            var resourcePaths = mResourceCountMap.Keys.ToList();
            var result = true;
            foreach (var resourcePath in resourcePaths)
            {
                if (!ReleaseResourceAll(resourcePath))
                {
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// 记录指定未完成资源请求
        /// 已经进入完成、取消或失败状态的同步请求不会被记录
        /// </summary>
        /// <param name="requestHandle"></param>
        /// <returns></returns>
        public bool RecordRequest(ResourceRequestHandle requestHandle)
        {
            if (requestHandle == null || requestHandle.IsDone)
            {
                return false;
            }
            return mRequestHandleSet.Add(requestHandle);
        }

        /// <summary>
        /// 移除指定资源请求记录
        /// 异步请求进入完成、取消或失败状态时应调用本接口
        /// </summary>
        /// <param name="requestHandle"></param>
        /// <returns></returns>
        public bool RemoveRequest(ResourceRequestHandle requestHandle)
        {
            return requestHandle != null && mRequestHandleSet.Remove(requestHandle);
        }

        /// <summary>
        /// 取消当前Scope记录的所有未完成资源请求
        /// 当前Scope后续仍可继续记录新的资源请求
        /// </summary>
        private void CancelAllRequests()
        {
            if(mRequestHandleSet.Count == 0)
            {
                return;
            }
            var requestHandles = mRequestHandleSet.ToList();
            // Handle.Cancel可能同步触发完成回调并移除请求，因此遍历前先清空原集合
            mRequestHandleSet.Clear();
            foreach (var requestHandle in requestHandles)
            {
                if (requestHandle == null || requestHandle.IsDone)
                {
                    continue;
                }
                requestHandle.Cancel();
            }
        }

        /// <summary>
        /// 将当前Scope记录的所有未完成资源请求转为同步加载
        /// 同一个Loader上的多个请求可能会一起完成
        /// </summary>
        /// <returns></returns>
        public bool LoadAllRequestsImmediately()
        {
            var requestHandles = mRequestHandleSet.ToList();
            var result = true;
            foreach (var requestHandle in requestHandles)
            {
                if (requestHandle == null || requestHandle.IsDone)
                {
                    mRequestHandleSet.Remove(requestHandle);
                    continue;
                }
                if (!requestHandle.LoadImmediately())
                {
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// 清理当前Scope持有的所有资源请求和资源索引计数
        /// Note:
        /// 必须先取消未完成请求，避免请求完成后继续产生新的资源引用
        /// 当前Scope清理后仍可继续使用
        /// </summary>
        /// <returns></returns>
        public bool Clear()
        {
            CancelAllRequests();
            return ReleaseAll();
        }

        /// <summary>
        /// 根据Loader类型释放指定次数资源索引计数
        /// </summary>
        /// <param name="loader"></param>
        /// <param name="releaseCount"></param>
        /// <returns></returns>
        private bool ReleaseResourceByLoader(Loadable loader, int releaseCount = 1)
        {
            if (loader == null || releaseCount <= 0)
            {
                return false;
            }

            var assetLoader = loader as AssetLoader;
            if (assetLoader != null)
            {
                for (int i = 0; i < releaseCount; i++)
                {
                    assetLoader.ReleaseAsset();
                }
                return true;
            }

            var bundleLoader = loader as BundleLoader;
            if (bundleLoader != null)
            {
                for (int i = 0; i < releaseCount; i++)
                {
                    bundleLoader.ReleaseAssetBundle();
                }
                return true;
            }

            Debug.LogError($"ResourceScope不支持释放Loader类型:{loader.GetType().FullName}!");
            return false;
        }
    }
}
