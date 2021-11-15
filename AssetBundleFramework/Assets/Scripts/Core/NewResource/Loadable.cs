/*
 * Description:             Loadable.cs
 * Author:                  TONYTANG
 * Create Date:             2021/10/13
 */

// 加载器流程设计:
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

using UnityEngine;

namespace TResource
{
    /// <summary>
    /// Loadable.cs
    /// 加载器基类抽象
    /// </summary>
    public abstract class Loadable : IRecycle
    {
        /// <summary>
        /// 加载任务对应的资源路径
        /// </summary>
        public string ResourcePath
        {
            get;
            protected set;
        }

        /// <summary>
        /// 资源加载方式
        /// </summary>
        public ResourceLoadMethod LoadMethod
        {
            get;
            protected set;
        }

        /// <summary>
        /// 资源加载类型
        /// </summary>
        public ResourceLoadType LoadType
        {
            get;
            protected set;
        }

        /// <summary> AB资源自身加载任务状态 /// </summary>
        public ResourceLoadState LoadState
        {
            get;
            protected set;
        }

        /// <summary>
        /// 是否加载完成
        /// </summary>
        public bool IsDone
        {
            get
            {
                return LoadState == ResourceLoadState.Complete || LoadState == ResourceLoadState.Error;
            }
        }

        /// <summary>
        /// 是否在等待加载
        /// </summary>
        public bool IsWaiting
        {
            get
            {
                return LoadState == ResourceLoadState.Waiting;
            }
        }

        /// <summary>
        /// 是否在空状态
        /// </summary>
        public bool IsNoneState
        {
            get
            {
                return LoadState == ResourceLoadState.None;
            }
        }

        /// <summary>
        /// 是否在加载中
        /// </summary>
        public bool IsLoading
        {
            get
            {
                return LoadState == ResourceLoadState.Loading;
            }
        }

        /// <summary>
        /// 是否加载失败
        /// </summary>
        public bool IsError
        {
            get
            {
                return LoadState == ResourceLoadState.Error;
            }
        }

        /// <summary>
        /// 是否处于加载取消状态
        /// </summary>
        public bool IsCancel
        {
            get
            {
                return LoadState == ResourceLoadState.Cancel;
            }
        }

        public Loadable()
        {
            ResourcePath = null;
            LoadState = ResourceLoadState.None;
        }

        public virtual void onCreate()
        {
            ResourcePath = null;
            LoadState = ResourceLoadState.None;

        }

        public virtual void onDispose()
        {
            ResourcePath = null;
            LoadState = ResourceLoadState.None;
        }

        /// <summary>
        /// 触发资源加载
        /// </summary>
        public virtual void load()
        {
            if (!IsNoneState)
            {
                Debug.LogError($"ResourcePath:{ResourcePath}未处于空状态,不应该触发加载!");
                return;
            }
            LoadState = ResourceLoadState.Waiting;
            LoaderManager.Singleton.addLoadTask(this);
            // 同步加载就立刻触发，异步加载等待排队
            if (LoadMethod == ResourceLoadMethod.Sync)
            {
                doLoad();
            }
        }

        /// <summary>
        /// 触发立刻加载资源完成
        /// </summary>
        public virtual void loadImmediately()
        {
            if(IsDone)
            {
                Debug.LogError($"ResourcePath:{ResourcePath}已加载完成不应该触发立刻加载!");
                return;
            }
            if(IsWaiting)
            {
                Debug.LogError($"ResourcePath:{ResourcePath}未处于等待加载状态,不应该触发立刻加载!");
                return;
            }
            LoadMethod = ResourceLoadMethod.Sync;
            doLoad();
        }

        /// <summary>
        /// 执行资源加载
        /// </summary>
        public void doLoad()
        {
            if (IsDone)
            {
                Debug.LogError($"ResourcePath:{ResourcePath}已加载完成不应该触发加载!");
                return;
            }
            LoadState = ResourceLoadState.Loading;
            LoaderManager.Singleton.removeLoadTask(this);
            onLoad();
        }

        /// <summary>
        /// 响应资源加载
        /// </summary>
        protected virtual void onLoad()
        {

        }

        /// <summary>
        /// 取消指定请求
        /// </summary>
        /// <param name="requiestUid"></param>
        /// <returns></returns>
        public virtual bool cancelRequest(int requiestUid)
        {
            return true;
        }

        /// <summary>
        /// 触发资源加载失败
        /// </summary>
        protected virtual void failed()
        {
            if(!IsLoading)
            {
                Debug.LogError($"ResourcePath:{ResourcePath}未处于加载中,不应该触发加载失败!");
                return;
            }
            LoadState = ResourceLoadState.Error;
            onFailed();
            // 加载失败要通知完成，确保上层逻辑走完
            onComplete();
        }

        /// <summary>
        /// 响应资源加载失败(处理加载失败的情况)
        /// </summary>
        protected virtual void onFailed()
        {
            ResourceLogger.log($"资源:{ResourcePath}加载失败!");
        }

        /// <summary>
        /// 触发资源加载取消(逻辑层加载取消)
        /// Note:
        /// 取消仅代表逻辑层面的加载请求取消，不代表资源加载的取消
        /// 资源在加载完成后依然能进入完成流程，最后会因为不满足索引计数和对象绑定要求而被卸载
        /// </summary>
        protected virtual void cancel()
        {
            if (IsDone)
            {
                Debug.LogError($"ResourcePath:{ResourcePath}已加载完成不允许取消!");
                return;
            }
            LoadState = ResourceLoadState.Cancel;
            onCancel();
        }

        /// <summary>
        /// 响应资源加载取消(处理资源加载取消的情况)
        /// </summary>
        protected virtual void onCancel()
        {
            ResourceLogger.log($"资源:{ResourcePath}加载请求取消!");
        }

        /// <summary>
        /// 触发资源加载完成
        /// </summary>
        protected virtual void complete()
        {
            ResourceLogger.log($"加载资源:{ResourcePath}完成!");
            LoadState = ResourceLoadState.Complete;
            onComplete();
        }

        /// <summary>
        /// 响应资源加载完成(一些数据清理和逻辑层回调)
        /// </summary>
        protected virtual void onComplete()
        {

        }
    }
}