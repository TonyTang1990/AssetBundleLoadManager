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
        /// <summary>
        /// 下一个有效资源请求UID
        /// </summary>
        private int mNextRequestUID;

        /// <summary>
        /// 所有正在加载的Asset加载器
        /// </summary>
        private Dictionary<string, AssetLoader> mAllAssetLoaderMap;

        /// <summary>
        /// 所有正在加载的AssetBundle加载器
        /// </summary>
        private Dictionary<string, AssetBundleLoader> mAllAssetBundleLoaderMap;

        /// <summary>
        /// Asset资源请求UID Map<资源请求UID,Asset路径>
        /// </summary>
        private Dictionary<int, string> mAssetRequestUIDMap;

        /// <summary>
        /// AssetBundle资源请求UID Map<资源请求UID,AssetBundle路径>
        /// </summary>
        private Dictionary<int, string> mAssetBundleRequestUIDMap;

        public LoaderManager()
        {
            mNextRequestUID = 1;
            mAllAssetLoaderMap = new Dictionary<string, AssetLoader>();
            mAllAssetBundleLoaderMap = new Dictionary<string, AssetBundleLoader>();
            mAssetRequestUIDMap = new Dictionary<int, string>();
            mAssetBundleRequestUIDMap = new Dictionary<int, string>();
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
        /// 创建指定Asset路径的Asset加载器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public AssetLoader createAssetLoader<T>(string assetPath) where T : UnityEngine.Object
        {
            return null;
        }

        /// <summary>
        /// 创建指定AssetBundle路径的AssetBundle加载器
        /// </summary>
        /// <param name="assetBundlePath"></param>
        /// <returns></returns>
        public AssetBundleLoader createAssetBundleLoader(string assetBundlePath)
        {
            return null;
        }

        /// <summary>
        /// 获取指定Asset路径的加载器
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public AssetLoader getAssetLoader(string assetPath)
        {
            AssetLoader assetLoader;
            if (mAllAssetLoaderMap.TryGetValue(assetPath, out assetLoader))
            {
                return assetLoader;
            }
            return null;
        }

        /// <summary>
        /// 获取指定AssetBundle路径的加载器
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public AssetBundleLoader getAssetBundleLoader(string assetBundlePath)
        {
            AssetBundleLoader abLoader;
            if (mAllAssetBundleLoaderMap.TryGetValue(assetBundlePath, out abLoader))
            {
                return abLoader;
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