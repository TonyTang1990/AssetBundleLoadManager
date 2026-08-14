/*
 * Description:             HotUpdateSingleABInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/09
 */

using System;
using UnityEngine;

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
        public string ABRelativePath => mABRelativePath;

        /// <summary>
        /// AssetBundle文件相对路径(含后缀)
        /// </summary>
        [SerializeField]
        private string mABRelativePath;

        /// <summary>
        /// AssetBundle的MD5
        /// </summary>
        public string ABMD5 => mABMD5;

        /// <summary>
        /// AssetBundle的MD5
        /// </summary>
        [SerializeField]
        private string mABMD5;

        /// <summary>
        /// AssetBundle文件大小
        /// </summary>
        public long ABSize => mABSize;

        /// <summary>
        /// AssetBundle文件大小
        /// </summary>
        [SerializeField]
        private long mABSize;

        /// <summary>
        /// AssetBundle的Sha256(用于热更新AB文件的完整性和正确性验证)
        /// </summary>
        public string ABSha256 => mABSha256;

        /// <summary>
        /// AssetBundle的Sha256(用于热更新AB文件的完整性和正确性验证)
        /// </summary>
        [SerializeField]
        private string mABSha256;

        /// <summary>
        /// AssetBundle文件相对路径(带MD5)(含后缀)
        /// </summary>
        private string ABRelativePathWithMD5;

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
            mABRelativePath = abRelativePath;
            mABMD5 = abMD5;
            mABSize = abSize;
            mABSha256 = abSha256;
        }

        /// <summary>
        /// AssetBundle文件相对路径(带MD5)(含后缀)
        /// </summary>
        /// <returns></returns>
        public string GetABRelativePathWithMD5()
        {
            // 做缓存优化，避免每次同一个AB反复加载获取真实加载路径都重新计算问题
            if(ABRelativePathWithMD5 == null)
            {
                ABRelativePathWithMD5 = PathUtilities.GetFilePathWithMD5(mABRelativePath, mABMD5);
            }
            return ABRelativePathWithMD5;
        }
    }
}