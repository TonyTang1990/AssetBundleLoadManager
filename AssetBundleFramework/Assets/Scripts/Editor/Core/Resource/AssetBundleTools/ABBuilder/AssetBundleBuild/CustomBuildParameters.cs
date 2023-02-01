/*
 * Description:             CustomBuildParameters.cs
 * Author:                  TONYTANG
 * Create Date:             2023//01/31
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// CustomBuildParameters.cs
    /// </summary>
    class CustomBuildParameters : BundleBuildParameters
    {
        /// <summary>
        /// 自定义AB名压缩格式Map<AB名, 压缩格式>
        /// </summary>
        private Dictionary<string, BuildCompression> mCustomBundleCompression;
        
        public CustomBuildParameters(BuildTarget target, BuildTargetGroup group, string outputFolder) : base(target, group, outputFolder)
        {
            mCustomBundleCompression = new Dictionary<string, BuildCompression>();
        }

        /// <summary>
        /// 添加指定AB名的压缩格式设置
        /// </summary>
        /// <param name="assetBundleName"></param>
        /// <param name="compression"></param>
        /// <returns></returns>
        public bool AddAssetBundleCompression(string assetBundleName, BuildCompression compression)
        {
            if(mCustomBundleCompression.ContainsKey(assetBundleName))
            {
                Debug.LogError($"重复添加AB名:{assetBundleName}的压缩格式:{compression}，添加失败，请检查代码！");
                return false;
            }
            mCustomBundleCompression.Add(assetBundleName, compression);
            return true;
        }

        /// <summary>
        /// 获取自定义AB名压缩格式
        /// </summary>
        /// <param name="identifier"></param>
        /// <returns></returns>
        public override BuildCompression GetCompressionForIdentifier(string identifier)
        {
            BuildCompression compression;
            if (mCustomBundleCompression.TryGetValue(identifier, out compression))
            {
                return compression;
            }
            return base.GetCompressionForIdentifier(identifier);
        }
    }
}