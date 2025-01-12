/*
 * Description:             AbstractResourceInfo.cs
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
            get { return IsReady && RefCount <= 0 && UpdateOwnerReference() == 0 && LoadType == ResourceLoadType.NormalLoad; }
        }

        /// <summary>
        /// 资源是否已经加载完成
        /// </summary>
        public bool IsReady
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
        /// 引用对象列表
        /// 用于判定引用AB的对象是否依然有效(还在使用未销毁)
        /// </summary>
        public List<System.WeakReference> ReferenceOwnerList
        {
            get
            {
                return mReferenceOwnerList;
            }
        }
        protected List<System.WeakReference> mReferenceOwnerList;

        /// <summary>
        /// 资源
        /// </summary>
        protected UnityEngine.Object mResource;

        public AbstractResourceInfo()
        {
            ResourcePath = null;
            LastUsedTime = 0f;
            IsReady = false;
            RefCount = 0;
            mReferenceOwnerList = new List<WeakReference>();
        }

        public virtual void OnCreate()
        {
            ResourcePath = null;
            LastUsedTime = 0f;
            IsReady = false;
            RefCount = 0;
            mResource = null;
            mReferenceOwnerList.Clear();
        }

        public virtual void OnDispose()
        {
            ResourcePath = null;
            LastUsedTime = 0f;
            IsReady = false;
            RefCount = 0;
            mResource = null;
            mReferenceOwnerList.Clear();
        }

        /// <summary>
        /// 设置资源
        /// </summary>
        /// <returns></returns>
        public void SetResource(UnityEngine.Object asset)
        {
            mResource = asset;
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <returns></returns>
        public T GetResource<T>() where T : UnityEngine.Object
        {
            UpdateLastUsedTime();
            return mResource as T;
        }

        /// <summary>
        /// 添加引用，引用计数+1
        /// </summary>
        public virtual void Retain()
        {
            RefCount++;
        }

        /// <summary>
        /// 释放引用，引用计数-1
        /// </summary>
        public virtual void Release()
        {
            var newRefCount = RefCount - 1;
            if(newRefCount < 0)
            {
                Debug.LogError($"资源:{ResourcePath}的索引计数信息小于了0,请检查代码!");
            }
            RefCount = Mathf.Max(0, newRefCount);
        }

        /// <summary>
        /// 更新最近使用时间
        /// </summary>
        public void UpdateLastUsedTime()
        {
            LastUsedTime = Time.time;
        }

        /// <summary>
        /// 为Asset添加指定owner的引用
        /// 所有owner都销毁且所属ab引用计数归零可回收
        /// </summary>
        /// <param name="owner"></param>
        public void RetainOwner(UnityEngine.Object owner)
        {
            if (owner == null)
            {
                ResourceLogger.logErr(string.Format("引用对象不能为空!无法为资源:{0}添加引用!", ResourcePath));
                return;
            }

            // Asset对象绑定的情况下无需添加所属AB的计数信息
            foreach (var referenceowner in mReferenceOwnerList)
            {
                if (owner.Equals(referenceowner))
                {
                    return;
                }
            }

            System.WeakReference wr = new System.WeakReference(owner);
            mReferenceOwnerList.Add(wr);
        }

        /// <summary>
        /// 移除指定拥有者绑定(用于解决上层绑定对象一直存在导致资源无法释放的问题)
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        public bool ReleaseOwner(UnityEngine.Object owner)
        {
            if (owner == null)
            {
                ResourceLogger.logErr(string.Format("引用对象不能为空!无法为资源:{0}解除绑定!", ResourcePath));
                return false;
            }

            var ownerindex = mReferenceOwnerList.FindIndex((ow) => ow.Target.Equals(owner));
            if (ownerindex != -1)
            {
                ResourceLogger.log(string.Format("资源:{0}找到指定绑定对象:{1},解除绑定!", ResourcePath, owner));
                mReferenceOwnerList.RemoveAt(ownerindex);
                return true;
            }
            else
            {
                ResourceLogger.log(string.Format("资源:{0}找不到指定绑定对象:{1},解除绑定失败!", ResourcePath, owner));
                return false;
            }
        }

        /// <summary>
        /// 更新Asset有效的绑定对象计数
        /// </summary>
        /// <returns></returns>
        protected int UpdateOwnerReference()
        {
            for (int i = 0; i < mReferenceOwnerList.Count; i++)
            {
                UnityEngine.Object o = (UnityEngine.Object)mReferenceOwnerList[i].Target;
                if (!o)
                {
                    mReferenceOwnerList.RemoveAt(i);
                    i--;
                }
            }
            return mReferenceOwnerList.Count;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose()
        {
            ResourcePath = null;
            LastUsedTime = 0f;
            IsReady = false;
            RefCount = 0;
            mResource = null;
            mReferenceOwnerList.Clear();
        }

        #region 辅助调试工具
        /// <summary>
        /// 打印当前AB所有使用者信息以及索引计数(开发用)
        /// </summary>
        public void PrintAllOwnersNameAndRefCount()
        {
            ResourceLogger.log(string.Format("Resource Path: {0}", ResourcePath));
            ResourceLogger.log(string.Format("Ref Count: {0}", RefCount));
            if (mReferenceOwnerList.Count == 0)
            {
                ResourceLogger.log("    Owners Name : None");
            }
            else
            {
                ResourceLogger.log("    Owners Name :");
                for (int i = 0, length = mReferenceOwnerList.Count; i < length; i++)
                {
                    if (mReferenceOwnerList[i].Target != null)
                    {
                        ResourceLogger.log(string.Format("      owner[{0}] : {1}", i, mReferenceOwnerList[i].Target.ToString()));
                    }
                }
            }
            ResourceLogger.log(string.Format("  Last Used Time: {0}", LastUsedTime));
        }
        #endregion
    }
}