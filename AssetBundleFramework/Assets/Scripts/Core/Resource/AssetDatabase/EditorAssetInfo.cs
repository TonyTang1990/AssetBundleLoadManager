/*
 * Description:             EditorAssetInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2026//07/13
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// EditorAssetInfo.cs
    /// EditorAssetInfo类,用于存储编辑器Asset信息(Asset名字(含后缀)和Asset路径)
    /// </summary>
    [Serializable]
    public class EditorAssetInfo
    {
        /// <summary>
        /// Asset路径
        /// </summary>
        [Header("Asset路径")]
        public string AssetPath;

        /// <summary>
        /// Asset名(含后缀)
        /// </summary>
        [Header("Asset名(含后缀)")]
        public string AssetName;

        public EditorAssetInfo(string assetPath)
        {
            AssetPath = assetPath;
            AssetName = Path.GetFileName(assetPath);
        }
    }
}