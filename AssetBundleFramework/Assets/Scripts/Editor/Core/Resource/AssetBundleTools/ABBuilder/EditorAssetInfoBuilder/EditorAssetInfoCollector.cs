/*
 * Description:             EditorAssetInfoCollector.cs
 * Author:                  TONYTANG
 * Create Date:             2026//07/13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// EditorAssetInfoCollector.cs
    /// 编辑器Asset信息(Asset名字(含后缀)和Asset路径)的搜集器静态类
    /// </summary>
    public static class EditorAssetInfoCollector
    {
        /// <summary>
        /// 更新EditorAssetInfoAsset
        /// </summary>
        [MenuItem("Tools/AssetBundle/更新EditorAssetInfoAsset", priority = 100)]
        public static void DoCollectEditorAssetInfo()
        {
            var editorAssetInfoBuilder = new EditorAssetInfoBuilder();
            editorAssetInfoBuilder.DoCollectEditorAssetInfo();
        }
    }
}