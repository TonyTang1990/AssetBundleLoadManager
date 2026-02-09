/*
 * Description:             TImageEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2020//02/05
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TUI
{
    /// <summary>
    /// TImageEditor.cs
    /// TImage組件的自定义编辑器界面
    /// </summary>
    [CustomEditor(typeof(TImage))]
    [CanEditMultipleObjects]
    public class TImageEditor : ImageEditor
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
        /// Sprite属性
        /// </summary>
        private SerializedProperty mSprite;

        /// <summary>
        /// 图片名字属性
        /// </summary>
        private SerializedProperty mSpritePath;

        [UnityEditor.MenuItem("GameObject/UI/TUI/TImage", priority = 1)]
        private static void AddTImage(MenuCommand command)
        {
            GameObject go = command.context as GameObject;
            var timage = UIUtilitiesEditor.AddComponent<TImage>(go);
            timage.name = "TImage";
        }

        
        protected override void OnEnable()
        {
            base.OnEnable();
            mEnableInvertMask = serializedObject.FindProperty("EnableInvertMask");
            mEnableAlphaHitTestMinimusThreshold = serializedObject.FindProperty("EnableAlphaHitTestMinimusThreshold");
            mAlphaHitTestMinimumThreshold = serializedObject.FindProperty("AlphaHitTestMinimumThreshold");
            mSprite = serializedObject.FindProperty("m_Sprite");
            mSpritePath = serializedObject.FindProperty("SpritePath");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.PropertyField(mEnableInvertMask);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(mEnableAlphaHitTestMinimusThreshold);
            bool enableChanged = EditorGUI.EndChangeCheck();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(mAlphaHitTestMinimumThreshold);
            bool thresholdChanged = EditorGUI.EndChangeCheck();

            DrawClearSpriteButton();

            serializedObject.ApplyModifiedProperties();

            if (enableChanged || thresholdChanged)
            {
                UpdateAllAlphaHitTextMinimusThresholdoTargets();
            }
        }
        
        /// <summary>
        /// 绘制清除Sprite按钮
        /// Note:
        /// 运行时清除只会清除Sprite引用，并不会解除资源绑定
        /// </summary>
        private void DrawClearSpriteButton()
        {
            if(GUILayout.Button("清除Sprite", GUILayout.ExpandWidth(true)))
            {
                mSprite.objectReferenceValue = null;
            }
        }

        /// <summary>
        /// 更新所有对象的透明Alpha穿透阈值
        /// </summary>
        private void UpdateAllAlphaHitTextMinimusThresholdoTargets()
        {
            if(targets == null)
            {
                return;
            }
            foreach (var targetObj in targets)
            {
                var tImage = targetObj as TImage;
                if (tImage != null)
                {
                    tImage.UpdateAlphaHitTestMinimumThreshold();
                    EditorUtility.SetDirty(targetObj);
                }
            }
        }
    }
}