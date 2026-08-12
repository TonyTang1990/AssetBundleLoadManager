/*
 * Description:             HotUpdateSingleABInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/09
 */

using System;

namespace TResource
{
    /// <summary>
    /// HotUpdateSingleABInfo.cs
    /// 单个热更新AB信息类
    /// </summary>
    [Serializable]
    public class HotUpdateSingleABInfo
    {
        /// <summary>
        /// AssetBundle文件相对路径(含后缀)
        /// </summary>
        public string ABRelativePath;

        /// <summary>
        /// AssetBundle的MD5
        /// </summary>
        public string ABMD5;

        /// <summary>
        /// AssetBundle文件大小
        /// </summary>
        public long ABSize;

        /// <summary>
        /// AssetBundle的Sha256(用于热更新AB文件的完整性和正确性验证)
        /// </summary>
        public string ABSha256;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="abRelativePath"></param>
        /// <param name="abMD5"></param>
        /// <param name="abRelativePathWithMD5"></param>
        /// <param name="abSize"></param>
        /// <param name="abSha256"></param>
        public HotUpdateSingleABInfo(string abRelativePath, string abMD5,
                                    long abSize, string abSha256)
        {
            ABRelativePath = abRelativePath;
            ABMD5 = abMD5;
            ABSize = abSize;
            ABSha256 = abSha256;
        }

        /// <summary>
        /// AssetBundle文件相对路径(带MD5)(含后缀)
        /// </summary>
        /// <returns></returns>
        public string GetABRelativePathWithMD5()
        {
            return PathUtilities.GetFilePathWithMD5(ABRelativePath, ABMD5);
        }
    }
}