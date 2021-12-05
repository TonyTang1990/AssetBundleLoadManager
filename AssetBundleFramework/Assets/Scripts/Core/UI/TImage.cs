/*
 * Description:             TImage.cs
 * Author:                  TONYTANG
 * Create Date:             2020//02/05
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TUI
{
    /// <summary>
    /// TImage.cs
    /// 重写Image组件，方便资源管理和自定义一些特性
    /// Note:
    /// 默认在编辑器赋值的通过依赖加载的不提供资源信息接口(ABI),
    /// 避免依赖当前TImage组件卸载时发现资源被提前卸载
    /// </summary>
    public class TImage : Image
    {
#if !NEW_RESOURCE
        /// <summary>
        /// 资源引用信息
        /// </summary>
        public AbstractResourceInfo ABI
        {
            get;
            set;
        }
#else
        /// <summary>
        /// 资源加载器(默认采用对象绑定所以不需要再OnDestroy时返还计数)
        /// </summary>
        public TResource.AssetLoader Loader
        {
            get;
            set;
        }
#endif

        /// <summary>
        /// 当前图片名
        /// </summary>
        [HideInInspector]
        public string SpritePath;

        /// <summary>
        /// 打印当前TImage图集使用信息
        /// </summary>
        public void printTImageInfo()
        {
            DIYLog.Log($"SpritePath = {SpritePath}");
#if !NEW_RESOURCE
            var refcount = ABI != null ? ABI.RefCount.ToString() : "无";
            DIYLog.Log($"SpritePath引用计数 = {refcount}");
#else
            var refcount = Loader != null ? Loader.getReferenceCount().ToString() : "无";
            DIYLog.Log($"SpritePath引用计数 = {refcount}");
#endif
        }
    }
}