/*
 * Description:             AssetDatabaseAsyncRequest.cs
 * Author:                  TONYTANG
 * Create Date:             2021//12/05
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetDatabaseAsyncRequest.cs
    /// AssetDatabase异步Asset资源请求模拟
    /// </summary>
    public class AssetDatabaseAsyncRequest
    {
        /// <summary>
        /// Asset路径
        /// </summary>
        public string AssetPath
        {
            get;
            protected set;
        }

        /// <summary>
        /// Asset类型
        /// </summary>
        public Type AssetType
        {
            get;
            protected set;
        }

        /// <summary>
        /// 资源Asset
        /// </summary>
        public UnityEngine.Object Asset
        {
            get;
            protected set;
        }

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsDone 
        {
            get;
            protected set;
        }

        /// <summary>
        /// 进度
        /// </summary>
        public float Progress 
        {
            get
            {
                return Mathf.Clamp(mTimePassed, 0f, mLoadAssetTime);
            }
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public Action<AssetDatabaseAsyncRequest> completed;

        /// <summary>
        /// 加载Asset时长(随机时长用于模拟异步加载)
        /// </summary>
        protected float mLoadAssetTime;

        /// <summary>
        /// 经历时长
        /// </summary>
        protected float mTimePassed;

        private AssetDatabaseAsyncRequest()
        {

        }

        public AssetDatabaseAsyncRequest(string assetPath, Type assetType)
        {
            Debug.Assert(!string.IsNullOrEmpty(assetPath), "不允许传空AssetPath!");
            Debug.Assert(assetType != null, "不允许传空AssetType!");
            AssetPath = assetPath;
            AssetType = assetType;
            mLoadAssetTime = UnityEngine.Random.Range(0f, 3f);
            UpdateManager.Singleton.registerFixedUpdate(OnFixedUpdate);
            ResourceLogger.log($"AssetDatabase:{AssetPath}资源异步加载时长:{mLoadAssetTime}!");
        }

        /// <summary>
        /// 固定频率更新
        /// </summary>
        /// <param name="fixedDeltTime"></param>
        protected void OnFixedUpdate(float fixedDeltTime)
        {
            mTimePassed += fixedDeltTime;
            if(mTimePassed >= mLoadAssetTime)
            {
                OnAssetLoadComplete();
            }
        }

        /// <summary>
        /// Asset加载完成
        /// </summary>
        protected void OnAssetLoadComplete()
        {
            UpdateManager.Singleton.unregisterFixedUpdate(OnFixedUpdate);
            IsDone = true;
#if UNITY_EDITOR
            Asset = AssetDatabase.LoadAssetAtPath(AssetPath, AssetType);
#else
            Asset = null;
#endif
            completed?.Invoke(this);
        }
    }
}
