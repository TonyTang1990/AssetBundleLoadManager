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
    LoadByFilePath = 1,                 // 按文件加载策略
    LoadByFolderPath,                   // 按目录加载策略
    Ignore,                             // 不参与打包策略
}