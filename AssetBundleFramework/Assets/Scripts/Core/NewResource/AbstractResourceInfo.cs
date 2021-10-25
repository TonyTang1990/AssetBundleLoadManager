/*
 * Description:             AbstractResourceInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/24
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AbstractResourceInfo.cs
    /// 资源信息抽象类
    /// </summary>
    public abstract class AbstractResourceInfo : IRecycle
    {
        /// <summary>
        /// 资源卸载委托
        /// </summary>
        /// <param name="abi"></param>
        public delegate void OnResourceUnloadedHandler(AbstractResourceInfo abi);

        /// <summary>
        /// 资源卸载回调(用于通知资源对应Loader切换状态)
        /// </summary>
        public OnResourceUnloadedHandler onResourceUnloadedCallback;

        /// <summary>
        /// 资源加载类型
        /// Note:
        /// 已加载的资源的加载类型不允许更改
        /// </summary>
        public ResourceLoadType LoadType
        {
            get;
            protected set;
        }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string ResourcePath
        {
            get;
            protected set;
        }

        /// <summary>
        /// 上一次使用的有效时间(用于回收策略判定，比如越久未使用先回收，0.0f表示被回收或者未被使用)
        /// </summary>
        public float LastUsedTime
        {
            get;
            protected set;
        }

        /// <summary>
        /// 是否不再有人使用
        /// </summary>
        public virtual bool IsUnsed
        {
            get { return mIsReady && RefCount <= 0; }
        }

        /// <summary>
        /// 资源是否已经加载完成
        /// </summary>
        public bool mIsReady
        {
            get;
            set;
        }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount
        {
            get;
            protected set;
        }

        /// <summary>
        /// 资源
        /// </summary>
        protected Object mResource;

        public virtual void onCreate()
        {
            onResourceUnloadedCallback = null;
            ResourcePath = null;
            LastUsedTime = 0f;
            mIsReady = false;
            RefCount = 0;
            mResource = null;
        }

        public virtual void onDispose()
        {
            onResourceUnloadedCallback = null;
            ResourcePath = null;
            LastUsedTime = 0f;
            mIsReady = false;
            RefCount = 0;
            mResource = null;
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <returns></returns>
        public T getResource<T>() where T : Object
        {
            return mResource as T;
        }

        /// <summary>
        /// 添加引用，引用计数+1
        /// </summary>
        public virtual void retain()
        {
            RefCount++;
        }

        /// <summary>
        /// 释放引用，引用计数-1
        /// </summary>
        public virtual void release()
        {
            RefCount = Mathf.Max(0, RefCount - 1);
        }

        /// <summary>
        /// 更新最近使用时间
        /// </summary>
        public void updateLastUsedTime()
        {
            LastUsedTime = Time.time;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public abstract void dispose();

    }
}