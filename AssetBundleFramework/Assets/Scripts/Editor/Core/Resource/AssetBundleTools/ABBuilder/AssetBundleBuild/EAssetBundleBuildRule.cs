/*
 * Description:             EAssetBundleCollectRule.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/25
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// EAssetBundleCollectRule.cs
/// AB资源搜集规则(针对文件夹而言的)
/// </summary>
[Serializable]
public enum EAssetBundleBuildRule
{
    ByFilePath = 1,                 // 按文件加载策略
    ByFolderPath,                   // 按目录加载策略
    ByConstName,                    // 按固定名字加载策略(AB不含路径，但Asset含路径)
    ByFileOrSubFolder,              // 同层按文件，其他按下一层目录路径
    Ignore,                         // 不参与打包策略
}