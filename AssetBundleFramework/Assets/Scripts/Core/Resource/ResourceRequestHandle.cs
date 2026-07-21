/*
 * Description:             ResourceRequestHandle.cs
 * Author:                  TONYTANG
 * Create Date:             2026/7/21
 */

using System;

namespace TResource
{
    /// <summary>
    /// 资源请求状态
    /// </summary>
    public enum ResourceRequestState
    {
        Pending,
        Completed,
        Cancelled,
        Failed
    }

    /// <summary>
    /// 资源请求句柄基类
    /// 句柄表示一次上层请求，而不是一个资源加载器。
    /// </summary>
    public abstract class ResourceRequestHandle
    {
        /// <summary>
        /// 取消回调
        /// </summary>
        private readonly Func<int, bool> mCancelHandler;

        /// <summary>
        /// 请求UID
        /// </summary>
        public int RequestUID { get; private set; }

        /// <summary>
        /// 资源请求状态
        /// </summary>
        public ResourceRequestState State { get; private set; }

        /// <summary>
        /// 是否处于等待状态
        /// </summary>
        public bool IsPending => State == ResourceRequestState.Pending;

        /// <summary>
        /// 是否处于完成状态
        /// </summary>
        public bool IsDone => State != ResourceRequestState.Pending;

        /// <summary>
        /// 是否处于完成状态
        /// </summary>
        public bool IsComplete => State == ResourceRequestState.Completed;

        protected ResourceRequestHandle(int requestUID, Func<int, bool> cancelHandler)
        {
            RequestUID = requestUID;
            mCancelHandler = cancelHandler;
            State = ResourceRequestState.Pending;
        }

        /// <summary>
        /// 取消本次请求。请求已进入终态时安全返回false。
        /// </summary>
        public bool Cancel()
        {
            if (!IsPending || mCancelHandler == null)
            {
                return false;
            }
            return mCancelHandler(RequestUID);
        }

        /// <summary>
        /// 标记本次请求为完成状态。请求已进入完成时安全返回false。
        /// </summary>
        /// <returns></returns>
        internal bool MarkCompleted()
        {
            return TrySetTerminalState(ResourceRequestState.Completed);
        }

        /// <summary>
        /// 标记本次请求为取消状态。请求已进入完成时安全返回false。
        /// </summary>
        /// <returns></returns>
        internal bool MarkCancelled()
        {
            return TrySetTerminalState(ResourceRequestState.Cancelled);
        }

        /// <summary>
        /// 标记本次请求为失败状态。请求已进入完成时安全返回false。
        /// </summary>
        /// <returns></returns>
        internal bool MarkFailed()
        {
            return TrySetTerminalState(ResourceRequestState.Failed);
        }

        /// <summary>
        /// 尝试设置请求状态为完成。请求已进入完成时安全返回false。
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool TrySetTerminalState(ResourceRequestState state)
        {
            if (!IsPending)
            {
                return false;
            }
            State = state;
            return true;
        }
    }

    /// <summary>
    /// Asset请求句柄
    /// </summary>
    public sealed class AssetRequestHandle : ResourceRequestHandle
    {
        internal AssetRequestHandle(int requestUID, Func<int, bool> cancelHandler)
            : base(requestUID, cancelHandler)
        {
        }
    }

    /// <summary>
    /// AssetBundle请求句柄
    /// </summary>
    public sealed class AssetBundleRequestHandle : ResourceRequestHandle
    {
        internal AssetBundleRequestHandle(int requestUID, Func<int, bool> cancelHandler)
            : base(requestUID, cancelHandler)
        {
        }
    }
}
