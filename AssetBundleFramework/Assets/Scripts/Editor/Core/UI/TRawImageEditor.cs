/*
 * Description:             TRawImageEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2020/10/08
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
    public class TRawImageEditor : RawImageEditor
    {
        /// <summary>
        /// 是否开启反向遮罩属性
        /// </summary>
        private SerializedProperty mEnableInvertMask;

        /// <summary>
        /// 是否激活透明Alpha透明可点击阈值属性
        /// </summary> <summary>
        private SerializedProperty mEnableAlphaHitTestMinimusThreshold;

        /// <summary>
        /// 透明Alpha可点击阈值属性
        /// </summary>
        private SerializedProperty mAlphaHitTestMinimumThreshold;

        /// <summary>
        /// Texture属性
        /// </summary>
        SerializedProperty m_Texture;

        /// <summary>
        /// 图片名字属性
        /// </summary>
        SerializedProperty m_TexturePath;

        [UnityEditor.MenuItem("GameObject/UI/TUI/TRawImage", priority = 4)]
        private static void AddTRawImage(MenuCommand command)
        {
            GameObject go = command.context as GameObject;
            var trawimage = UIUtilitiesEditor.AddComponent<TRawImage>(go);
            trawimage.name = "TRawImage";
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            mEnableInvertMask = serializedObject.FindProperty("EnableInvertMask");
            mEnableAlphaHitTestMinimusThreshold = serializedObject.FindProperty("EnableAlphaHitTestMinimusThreshold");
            mAlphaHitTestMinimumThreshold = serializedObject.FindProperty("AlphaHitTestMinimumThreshold");
            m_Texture = serializedObject.FindProperty("m_Texture");
            m_TexturePath = serializedObject.FindProperty("TexturePath");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(mEnableInvertMask);
            EditorGUILayout.PropertyField(mEnableAlphaHitTestMinimusThreshold);
            EditorGUILayout.PropertyField(mAlphaHitTestMinimumThreshold);

            DrawClearTextureButton();

            serializedObject.ApplyModifiedProperties();
        }
                
        /// <summary>
        /// 绘制清除Texture按钮
        /// Note:
        /// 运行时清除只会清除Texture引用，并不会解除资源绑定
        /// </summary>
        private void DrawClearTextureButton()
        {
            if(GUILayout.Button("清除Texture", GUILayout.ExpandWidth(true)))
            {
                m_Texture.objectReferenceValue = null;
            }
        }
    }
}