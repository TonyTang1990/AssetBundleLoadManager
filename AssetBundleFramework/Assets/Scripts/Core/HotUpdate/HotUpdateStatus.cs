/*
 * Description:             HotUpdateStatus.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/12
 */

namespace TResource
{
    /// <summary>
    /// HotUpdateStatus.cs
    /// 热更新状态枚举
    /// </summary>
    public enum HotUpdateStatus
    {
        /// <summary>
        /// 热更新状态-未开始
        /// </summary>
        NotStart = 0,
        /// <summary>
        /// 版本强更下载中
        /// </summary>
        VersionDownloading,
        /// <summary>
        /// 版本强更下载完成
        /// </summary>
        VersionDownloadSuccess,
        /// <summary>
        /// 版本强更下载失败
        /// </summary>
        VersionDownloadFailed,
        /// <summary>
        /// 热更新校验资源信息下载中
        /// </summary>
        VerifyABInfoDownloading,
        /// <summary>
        /// 热更新校验资源信息下载成功
        /// </summary>
        VerifyABInfoDownloadSuccess,
        /// <summary>
        /// 热更新校验资源信息下载失败
        /// </summary>
        VerifyABInfoDownloadFailed,
        /// <summary>
        /// 热更新资源信息下载中
        /// </summary>
        ABInfoDownloading,
        /// <summary>
        /// 热更新资源信息下载成功
        /// </summary>
        ABInfoDownloadSuccess,
        /// <summary>
        /// 热更新资源信息下载失败
        /// </summary>
        ABInfoDownloadFailed,
        /// <summary>
        /// 热更新资源下载中
        /// </summary>
        ResourceDownloading,
        /// <summary>
        /// 资源热更成功
        /// </summary>
        ResourceHotUpdateSuccess,
        /// <summary>
        /// 资源热更失败
        /// </summary>
        ResourceHotUpdateFailed,
        /// <summary>
        /// 有热更新资源下载失败
        /// </summary>
        ResourceDownloadFailed,
    }
}