/*
 * Description:             EditorAssetInfoAssetEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2026/07/13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// EditorAssetInfoAssetEditor.cs
    /// EditorAssetInfoAsset自定义编辑器
    /// </summary>
    [CustomEditor(typeof(EditorAssetInfoAsset))]
    public class EditorAssetInfoAssetEditor : Editor
    {
        /// <summary>
        /// TextArea Style
        /// </summary>
        private GUIStyle mTextAreaStyle;

        /// <summary>
        /// 打包Asset信息列表成员属性
        /// </summary>
        private SerializedProperty EditorAssetInfoListProperty;

        ///滚动位置
        private Vector2 mScrollPos;

        void OnEnable()
        {
            EditorAssetInfoListProperty = serializedObject.FindProperty("EditorAssetInfoList");
        }

        public override void OnInspectorGUI()
        {
            if (mTextAreaStyle == null)
            {
                mTextAreaStyle = new GUIStyle("textarea");
            }
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            mScrollPos = EditorGUILayout.BeginScrollView(mScrollPos);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("EditorAssetInfoAsset信息:", GUILayout.Width(150.0f), GUILayout.Height(20.0f));
            for (int i = 0; i < EditorAssetInfoListProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal("box");
                var editorAssetInfoMemberProperty = EditorAssetInfoListProperty.GetArrayElementAtIndex(i);
                var assetNameMemberProperty = editorAssetInfoMemberProperty.FindPropertyRelative("AssetName");
                var assetPathMemberProperty = editorAssetInfoMemberProperty.FindPropertyRelative("AssetPath");
                EditorGUILayout.LabelField("Asset名:", GUILayout.Width(70.0f), GUILayout.Height(20.0f));
                EditorGUILayout.LabelField(assetNameMemberProperty.stringValue, mTextAreaStyle, GUILayout.Width(250.0f), GUILayout.Height(20.0f));
                EditorGUILayout.LabelField("Asset路径:", GUILayout.Width(70.0f), GUILayout.Height(20.0f));
                EditorGUILayout.LabelField(assetPathMemberProperty.stringValue, mTextAreaStyle, GUILayout.Width(600.0f), GUILayout.Height(20.0f));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.EndScrollView();

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
