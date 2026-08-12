/*
 * Description:             HotUpdateResult.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/12
 */

namespace TResource
{
    /// <summary>
    /// HotUpdateResult.cs
    /// 热更新结果枚举
    /// </summary>
    public enum HotUpdateResult
    {
        /// <summary>
        /// 热更新完成
        /// </summary>
        Complete = 1,
        /// <summary>
        /// 热更新失败
        /// </summary>
        Failed,
        /// <summary>
        /// 不需要热更新
        /// </summary>
        NoNeedHotUpdate,
        /// <summary>
        /// 有错误状态
        /// </summary>
        UnderError,
    }
}