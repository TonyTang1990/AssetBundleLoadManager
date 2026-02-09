/*
 * Description:             TRawImage.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/08
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace TUI
{
    /// <summary>
    /// TRawImage.cs
    /// 重写RawImage组件，方便资源管理和自定义一些特性
    /// Note:
    /// 默认在编辑器赋值的通过依赖加载的不提供资源信息接口(ABI),
    /// 避免依赖当前TRawImage组件卸载时发现资源被提前卸载
    /// </summary>
    [AddComponentMenu("UI/TUI/TRawImage", 4)]
    public class TRawImage : RawImage
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
        public float AlphaHitTestMinimumThreshold = 0f;

        /// <summary>
        /// Asset加载器
        /// </summary>
        public TResource.AssetLoader Loader
        {
            get;
            set;
        }

        /// <summary>
        /// 当前图片路径
        /// </summary>
        [HideInInspector]
        public string TexturePath;

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
        /// 重写射线检测，实现透明穿透功能
        /// </summary>
        public bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if(!EnableAlphaHitTestMinimusThreshold)
            {
                return true;
            }

            if (AlphaHitTestMinimumThreshold <= 0f)
                return true;

            if (AlphaHitTestMinimumThreshold > 1f)
                return false;

            Texture tex = texture;
            if (tex == null)
                return true;

            Texture2D tex2D = tex as Texture2D;
            if (tex2D == null)
                return true;

            if (!tex2D.isReadable)
            {
                Debug.LogError($"TRawImage Alpha穿透检测需要纹理开启Read/Write Enabled: {tex2D.name}", this);
                return true;
            }

            // 将屏幕坐标转换为RectTransform的本地坐标
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint,
                                                                         eventCamera, out Vector2 localPoint))
            {
                return false;

            }

            Rect rect = GetPixelAdjustedRect();

            // 计算UV坐标 (考虑uvRect偏移和缩放)
            float u = (localPoint.x - rect.x) / rect.width;
            float v = (localPoint.y - rect.y) / rect.height;

            // 应用uvRect的偏移和缩放
            Rect uv = uvRect;
            u = uv.x + u * uv.width;
            v = uv.y + v * uv.height;

            // 确保UV在有效范围内
            u = Mathf.Clamp01(u);
            v = Mathf.Clamp01(v);

            try
            {
                return tex2D.GetPixelBilinear(u, v).a >= AlphaHitTestMinimumThreshold;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"TRawImage Alpha穿透检测读取纹理像素失败: {ex.Message}", this);
                return true;
            }
        }
        
        /// <summary>
        /// 打印当前TImage资源使用信息
        /// </summary>
        public void PrintTRawImageInfo()
        {
            DIYLog.Log($"TexturePath = {TexturePath}");
            var refcount = Loader != null ? Loader.GetReferenceCount().ToString() : "无";
            DIYLog.Log($"TexturePath引用计数 = {refcount}");
        }
    }
}