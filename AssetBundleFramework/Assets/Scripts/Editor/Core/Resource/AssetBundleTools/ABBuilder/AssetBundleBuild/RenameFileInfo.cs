/*
 * Description:             RenameFileInfo.cs
 * Author:                  #AUTHOR#
 * Create Date:             #CREATEDATE#
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// RenameFileInfo.cs
    /// 重命名文件信息类
    /// </summary>
    public class RenameFileInfo
    {
        /// <summary>
        /// 文件全路径
        /// </summary>
        public string FileFullPath
        {
            get;
            private set;
        }

        /// <summary>
        /// 文件相对路径
        /// </summary>
        public string FileRelativePath
        {
            get;
            private set;
        }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long FileSize
        {
            get;
            private set;
        }

        /// <summary>
        /// 文件MD5值
        /// </summary>
        public string FileMd5
        {
            get;
            private set;
        }

        /// <summary>
        /// 文件Sha256值
        /// </summary>
        public string FileSha256
        {
            get;
            private set;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileFullPath"></param>
        /// <param name="fileRelativePath"></param>
        /// <param name="fileSize"></param>
        /// <param name="fileMd5"></param>
        /// <param name="fileSha256"></param>
        public RenameFileInfo(string fileFullPath, string fileRelativePath, long fileSize,
                              string fileMd5, string fileSha256)
        {
            FileFullPath = fileFullPath;
            FileRelativePath = fileRelativePath;
            FileSize = fileSize;
            FileMd5 = fileMd5;
            FileSha256 = fileSha256;
        }
    }
}