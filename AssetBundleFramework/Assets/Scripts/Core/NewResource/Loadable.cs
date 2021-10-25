/*
 * Description:             Loadable.cs
 * Author:                  TONYTANG
 * Create Date:             2021/10/13
 */

// 加载器流程设计:
// 1. load                      -- 触发资源加载
// 2. loadImmediately           -- 触发立刻加载资源完成
// 3. cancelRequest             -- 取消指定请求
// 4. onLoad                    -- 响应资源加载
// 5. failed                    -- 触发资源加载失败
// 6. onFailed                  -- 响应资源加载失败
// 7. cancel                    -- 触发资源加载取消
// 8. onCancel                  -- 响应资源加载取消
// 9. complete                  -- 触发资源加载完成
// 10. onComplete               -- 响应资源加载完成

namespace TResource
{
    /// <summary>
    /// Loadable.cs
    /// 加载器基类抽象
    /// </summary>
    public abstract class Loadable : IRecycle
    {
        /// <summary>
        /// 资源加载方式
        /// </summary>
        public ResourceLoadMethod LoadMethod
        {
            get;
            set;
        }

        /// <summary>
        /// 资源加载类型
        /// </summary>
        public ResourceLoadType LoadType
        {
            get;
            set;
        }

        /// <summary> AB资源自身加载任务状态 /// </summary>
        public ResourceLoadState LoadState
        {
            get;
            set;
        }

        /// <summary>
        /// 是否加载完成
        /// </summary>
        public bool IsDone
        {
            get
            {
                return LoadState == ResourceLoadState.Complete || LoadState == ResourceLoadState.Error || LoadState == ResourceLoadState.Cancel;
            }
        }

        /// <summary>
        /// 是否在等待
        /// </summary>
        public bool IsWaiting
        {
            get
            {
                return LoadState == ResourceLoadState.Waiting;
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

        public virtual void onCreate()
        {

        }

        public virtual void onDispose()
        {

        }

        /// <summary>
        /// 触发资源加载
        /// </summary>
        public virtual void load()
        {

        }

        /// <summary>
        /// 触发立刻加载资源完成
        /// </summary>
        public virtual void loadImmediately()
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
        /// 响应资源加载
        /// </summary>
        protected virtual void onLoad()
        {

        }

        /// <summary>
        /// 触发资源加载失败
        /// </summary>
        protected virtual void failed()
        {

        }

        /// <summary>
        /// 响应资源加载失败
        /// </summary>
        protected virtual void onFailed()
        {

        }

        /// <summary>
        /// 触发资源加载取消
        /// </summary>
        protected virtual void cancel()
        {

        }

        /// <summary>
        /// 响应资源加载取消
        /// </summary>
        protected virtual void onCancel()
        {

        }

        /// <summary>
        /// 触发资源加载完成
        /// </summary>
        protected virtual void complete()
        {

        }

        /// <summary>
        /// 响应资源加载完成
        /// </summary>
        protected virtual void onComplete()
        {

        }
    }
}