/*
 * Description:             HotUpdateVerifyABInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/09
 */

using System;

namespace TResource
{
    /// <summary>
    /// HotUpdateVerifyABInfo.cs
    /// 热更新校验AB信息类(对应VerifyABInfo.json的数据结构)
    /// </summary>
    [Serializable]
    public class HotUpdateVerifyABInfo
    {
        /// <summary>
        /// 热更新AB资源信息记录文件大小
        /// </summary>
        public long ABInfoFileSize;

        /// <summary>
        /// 热更新AB资源信息记录文件的Sha256
        /// </summary>
        public string ABInfoFileSha256;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="abInfoFileSize"></param>
        /// <param name="abInfoFileSha256"></param>
        public HotUpdateVerifyABInfo(long abInfoFileSize, string abInfoFileSha256)
        {
            ABInfoFileSize = abInfoFileSize;
            ABInfoFileSha256 = abInfoFileSha256;
        }
    }
}