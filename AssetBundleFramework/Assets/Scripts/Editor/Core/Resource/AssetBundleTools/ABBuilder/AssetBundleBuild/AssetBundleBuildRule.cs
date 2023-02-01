/*
 * Description:             AssetBundleCollectRule.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/25
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TResource
{
    /// <summary>
    /// AssetBundleCollectRule.cs
    /// AB资源打包规则(针对文件夹而言的)
    /// </summary>
    [Serializable]
    public enum AssetBundleBuildRule
    {
        ByFilePath = 1,                 // 按文件路径打包策略
        ByFolderPath,                   // 按所在目录路径打包策略
        ByConstName,                    // 按固定名字打包策略(AB不含路径，但Asset含路径)
        ByFileOrSubFolder,              // 同层按文件路径，其他按下一层目录路径打包策略
        Ignore,                         // 不参与打包策略
    }
}