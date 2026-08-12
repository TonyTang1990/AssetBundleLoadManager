/*
 * Description:             AssetBundleBuildPurpose.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/11
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundleBuildPurpose.cs
    /// AB打包用途枚举
    /// </summary>
    public enum AssetBundleBuildPurpose
    {
        /// <summary>
        /// 构建母包
        /// </summary>
        BuildPlayerBaseLine = 0,

        /// <summary>
        /// 构建热更包
        /// </summary>
        BuildHotUpdate,
    }
}