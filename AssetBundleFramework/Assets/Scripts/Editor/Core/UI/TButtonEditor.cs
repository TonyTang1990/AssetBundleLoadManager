/*
 * Description:             TButtonEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/08
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace TUI
{
    [CustomEditor(typeof(TButton), true)]
    [CanEditMultipleObjects]
    /// <summary>
    ///   Custom Editor for the Button Component.
    ///   Extend this class to write a custom editor for an Button-derived component.
    /// </summary>
    public class TButtonEditor : SelectableEditor
    {
        SerializedProperty m_OnClickProperty;

        /// <summary>
        /// 长按点击开关
        /// </summary>
        SerializedProperty m_EnableLongtimePress;

        /// <summary>
        /// 长按点击是否只有一次(反之无数次)
        /// </summary>
        SerializedProperty m_IsLongtimePressOnlyOnce;

        /// <summary>
        /// 长按点击时长定义
        /// </summary>
        SerializedProperty m_LongtimePressTimeInterval;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_OnClickProperty = serializedObject.FindProperty("m_OnClick");
            m_EnableLongtimePress = serializedObject.FindProperty("EnableLongtimePress");
            m_IsLongtimePressOnlyOnce = serializedObject.FindProperty("IsLongtimePressOnlyOnce");
            m_LongtimePressTimeInterval = serializedObject.FindProperty("LongtimePressTimeInterval");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_EnableLongtimePress);
            EditorGUILayout.PropertyField(m_IsLongtimePressOnlyOnce);
            EditorGUILayout.PropertyField(m_LongtimePressTimeInterval);
            EditorGUILayout.PropertyField(m_OnClickProperty);
            serializedObject.ApplyModifiedProperties();
        }
    }
}