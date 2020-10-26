/*
 * Description:             TRawImageEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/08
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TUI
{
    /// <summary>
    /// TRawImageEditor.cs
    /// TRawImage組件的自定义编辑器界面
    /// </summary>
    [CustomEditor(typeof(TRawImage), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom editor for RawImage.
    ///   Extend this class to write a custom editor for a RawImage-derived component.
    /// </summary>
    public class TRawImageEditor : GraphicEditor
    {
        SerializedProperty m_Texture;
        SerializedProperty m_UVRect;
        GUIContent m_UVRectContent;

        /// <summary>
        /// 图片名字属性
        /// </summary>
        SerializedProperty m_TextureName;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Note we have precedence for calling rectangle for just rect, even in the Inspector.
            // For example in the Camera component's Viewport Rect.
            // Hence sticking with Rect here to be consistent with corresponding property in the API.
            m_UVRectContent = EditorGUIUtility.TrTextContent("UV Rect");

            m_Texture = serializedObject.FindProperty("m_Texture");
            m_UVRect = serializedObject.FindProperty("m_UVRect");

            m_TextureName = serializedObject.FindProperty("TextureName");

            SetShowNativeSize(true);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_Texture);
            if (EditorGUI.EndChangeCheck())
            {
                // 检测设置变化，记录最新的纹理引用名
                m_TextureName.stringValue = m_Texture.objectReferenceValue != null ? m_Texture.objectReferenceValue.name : string.Empty;
                Debug.Log($"TRawImage:{this.target.name}图片名设置有变化，最新图片名:{m_TextureName.stringValue}");
            }

            AppearanceControlsGUI();
            RaycastControlsGUI();
            EditorGUILayout.PropertyField(m_UVRect, m_UVRectContent);
            SetShowNativeSize(false);
            NativeSizeButtonGUI();

            serializedObject.ApplyModifiedProperties();
        }

        void SetShowNativeSize(bool instant)
        {
            base.SetShowNativeSize(m_Texture.objectReferenceValue != null, instant);
        }

        private static Rect Outer(RawImage rawImage)
        {
            Rect outer = rawImage.uvRect;
            outer.xMin *= rawImage.rectTransform.rect.width;
            outer.xMax *= rawImage.rectTransform.rect.width;
            outer.yMin *= rawImage.rectTransform.rect.height;
            outer.yMax *= rawImage.rectTransform.rect.height;
            return outer;
        }

        /// <summary>
        /// Allow the texture to be previewed.
        /// </summary>

        public override bool HasPreviewGUI()
        {
            RawImage rawImage = target as RawImage;
            if (rawImage == null)
                return false;

            var outer = Outer(rawImage);
            return outer.width > 0 && outer.height > 0;
        }

        /// <summary>
        /// Draw the Image preview.
        /// </summary>

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            RawImage rawImage = target as RawImage;
            Texture tex = rawImage.mainTexture;

            if (tex == null)
                return;

            var outer = Outer(rawImage);
            SpriteDrawUtility.DrawSprite(tex, rect, outer, rawImage.uvRect, rawImage.canvasRenderer.GetColor());
        }

        /// <summary>
        /// Info String drawn at the bottom of the Preview
        /// </summary>

        public override string GetInfoString()
        {
            RawImage rawImage = target as RawImage;

            // Image size Text
            string text = string.Format("RawImage Size: {0}x{1}",
                Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.width)),
                Mathf.RoundToInt(Mathf.Abs(rawImage.rectTransform.rect.height)));

            return text;
        }
    }
}
