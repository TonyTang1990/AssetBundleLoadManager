/*
 * Description:             AssetBundleCollectRule.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/11
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AB收集策略
    /// </summary>
    [Serializable]
    public enum AssetBundleCollectRule
    {
        /// <summary>
        /// 收集该文件夹
        /// </summary>
        Collect,

        /// <summary>
        /// 忽略该文件夹
        /// </summary>
        Ignore,
    }
}