/*
 * Description:             AssetLoader.cs
 * Author:                  TONYTANG
 * Create Date:             2021/10/13
 */

using System;

namespace TResource
{
    /// <summary>
    /// AssetLoader.cs
    /// AssetBundle模式的Asset加载器
    /// </summary>
    public class AssetLoader : Loadable
    {
        /// <summary>
        /// Asset类型
        /// </summary>
        public Type AssetType
        {
            get;
            protected set;
        }

        /// <summary>
        /// Asset信息
        /// </summary>
        protected AssetInfo mAssetInfo;

        /// <summary>
        /// 获取指定Asset
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T getAsset<T>() where T : UnityEngine.Object
        {
            if (mAssetInfo == null)
            {
                
            }
            return mAssetInfo.getResource<T>();
        }
    }
}