/*
 * Description:             TImage.cs
 * Author:                  TONYTANG
 * Create Date:             2020//02/05
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
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
    [AddComponentMenu("UI/TUI/TImage", 1)]
    public class TImage : Image
    {
        /// <summary>
        /// 是否开启反向遮罩
        /// </summary>
        [Header("是否开启反向遮罩")]
        public bool EnableInvertMask = false;
        
        /// <summary>
        /// 是否激活透明Alpha透明可点击阈值
        /// </summary>
        [Header("是否激活透明Alpha透明可点击阈值")]
        public bool EnableAlphaHitTestMinimusThreshold = false;

        /// <summary>
        /// 透明Alpha可点击阈值(<=0表示全部可点击，>1表示全不可点击，其他值表示小于该值不可点击)
        /// Note：
        /// 仅当EnableAlphaHitTestMinimusThreshold=true时有效
        /// </summary>
        [Header("透明Alpha可点击阈值")]
        [Tooltip("<=0表示全部可点击，>1表示全不可点击，其他值表示小于该值不可点击")]
        public float AlphaHitTestMinimumThreshold = 0.1f;

        /// <summary>
        /// 资源加载器(默认采用对象绑定所以不需要再OnDestroy时返还计数)
        /// </summary>
        public TResource.AssetLoader Loader
        {
            get;
            set;
        }

        /// <summary>
        /// 当前图片名
        /// </summary>
        [HideInInspector]
        public string SpritePath;

        protected override void Start()
        {
            base.Start();
            UpdateAlphaHitTestMinimumThreshold();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public bool ReleaseRes()
        {
            if(string.IsNullOrEmpty(SpritePath))
            {
                return true;
            }
            if(Loader != null)
            {
                var result = Loader.ReleaseOwner(this);
                Loader = null;
                if(!result)
                {
                    DIYLog.LogError($"TImage.ReleaseRes失败，SpritePath = {SpritePath}");
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 更新透明Alpha可点击阈值
        /// Note:
        /// 外部修改EnableAlphaHitTestMinimusThreshold或AlphaHitTestMinimumThreshold值后请调用此方法确保更新成功
        /// alphaHitTestMinimumThreshold值需要EnableAlphaHitTestMinimusThreshold为true的情况下设置才有效
        /// </summary>
        public void UpdateAlphaHitTestMinimumThreshold()
        {
            if(EnableAlphaHitTestMinimusThreshold)
            {
                alphaHitTestMinimumThreshold = AlphaHitTestMinimumThreshold;
            }
        }
        
        /// <summary>
        /// See IMaterialModifier.GetModifiedMaterial
        /// </summary>
        public override Material GetModifiedMaterial(Material baseMaterial)
        {
            if(!EnableInvertMask)
            {
                return base.GetModifiedMaterial(baseMaterial);
            }

            return GetInvertMaskModifiedMaterial(baseMaterial);
        }

        /// <summary>
        /// 获取反向遮罩修改材质球
        /// </summary>
        /// <param name="baseMaterial"></param>
        /// <returns></returns>
        protected Material GetInvertMaskModifiedMaterial(Material baseMaterial)
        {
            var toUse = baseMaterial;

            if (m_ShouldRecalculateStencil)
            {
                var rootCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
                m_StencilValue = maskable ? MaskUtilities.GetStencilDepth(transform, rootCanvas) : 0;
                m_ShouldRecalculateStencil = false;
            }

            Mask maskComponent = GetComponent<Mask>();
            if (m_StencilValue > 0 && (maskComponent == null || !maskComponent.IsActive()))
            {
                var maskMat = StencilMaterial.Add(toUse, (1 << m_StencilValue) - 1, StencilOp.Keep, CompareFunction.NotEqual, ColorWriteMask.All, (1 << m_StencilValue) - 1, 0);
                StencilMaterial.Remove(m_MaskMaterial);
                m_MaskMaterial = maskMat;
                toUse = m_MaskMaterial;
            }
            return toUse;
        }
        
        /// <summary>
        /// 打印当前TImage图集使用信息
        /// </summary>
        public void PrintTImageInfo()
        {
            DIYLog.Log($"SpritePath = {SpritePath}");
            var refcount = Loader != null ? Loader.GetReferenceCount().ToString() : "无";
            DIYLog.Log($"SpritePath引用计数 = {refcount}");
        }
    }
}