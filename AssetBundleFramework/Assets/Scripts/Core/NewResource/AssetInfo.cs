/*
 * Description:             AssetInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2021//10/13
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetInfo.cs
    /// </summary>
    public class AssetInfo : AbstractResourceInfo
    {
        /// <summary>
        /// Asset类型
        /// </summary>
        public Type AssetType
        {
            get;
            protected set;
        }

        /// <summary>
        /// 所属AB信息(仅在AB模式下有值)
        /// </summary>
        public AssetBundleInfo OwnerAsestBundleInfo
        {
            get;
            protected set;
        }

        /// <summary>
        /// 是否不再有人使用
        /// </summary>
        public override bool IsUnsed
        {
            get { return mIsReady && RefCount <= 0 && updateOwnerReference() == 0; }
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

        public AssetInfo()
        {
            AssetType = null;
            OwnerAsestBundleInfo = null;
            mReferenceOwnerList = new List<WeakReference>();
        }


        public override void onCreate()
        {
            base.onCreate();
            AssetType = null;
            OwnerAsestBundleInfo = null;
            mReferenceOwnerList.Clear();
        }

        public override void onDispose()
        {
            base.onDispose();
            AssetType = null;
            OwnerAsestBundleInfo = null;
            mReferenceOwnerList.Clear();
        }

        public void init(string assetPath, Type assetType, AssetBundleInfo assetBundleInfo)
        {
            ResourcePath = assetPath;
            AssetType = assetType;
            OwnerAsestBundleInfo = assetBundleInfo;
            OwnerAsestBundleInfo?.addLoadedAssetInfo(this);
        }

        /// <summary>
        /// 添加引用，引用计数+1
        /// </summary>
        public override void retain()
        {
            base.retain();
            // 常规Asset引用计数添加需要添加所属AB的使用计数
            OwnerAsestBundleInfo?.retain();
        }

        /// <summary>
        /// 释放引用，引用计数-1
        /// </summary>
        public override void release()
        {
            base.release();
            // 常规Asset引用计数减少需要减少所属AB的使用计数
            OwnerAsestBundleInfo?.retain();
        }

        /// <summary>
        /// 为Asset添加指定owner的引用
        /// 所有owner都销毁且所属ab引用计数归零可回收
        /// </summary>
        /// <param name="owner"></param>
        protected void retainOwner(UnityEngine.Object owner)
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
        public bool releaseOwner(UnityEngine.Object owner)
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
        protected int updateOwnerReference()
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

        public override void dispose()
        {

        }
    }
}