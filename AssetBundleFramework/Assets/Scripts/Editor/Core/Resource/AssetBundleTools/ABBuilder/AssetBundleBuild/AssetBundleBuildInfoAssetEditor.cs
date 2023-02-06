/*
 * Description:             AssetBundleBuildInfoAssetEditor.cs
 * Author:                  TONYTANG
 * Create Date:             2021//04/17
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundleBuildInfoAssetEditor.cs
    /// AssetBundle编译信息Asset自定义编辑器
    /// </summary>
    [CustomEditor(typeof(AssetBuildInfoAsset))]
    public class AssetBundleBuildInfoAssetEditor : Editor
    {
        /// <summary>
        /// TextArea Style
        /// </summary>
        private GUIStyle mTextAreaStyle;

        /// <summary>
        /// 打包Asset信息列表成员属性
        /// </summary>
        private SerializedProperty BuildAssetInfoListProperty;

        /// <summary>
        /// 打包AssetBundle信息列表成员属性
        /// </summary>
        //private SerializedProperty BuildAssetBuildInfoListProperty;

        ///滚动位置
        private Vector2 mScrollPos;

        void OnEnable()
        {
            BuildAssetInfoListProperty = serializedObject.FindProperty("BuildAssetInfoList");
            //BuildAssetBuildInfoListProperty = serializedObject.FindProperty("AssetBundleBuildInfoList");
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
            EditorGUILayout.LabelField("Asset打包信息:", GUILayout.Width(150.0f), GUILayout.Height(20.0f));
            for (int i = 0; i < BuildAssetInfoListProperty.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal("box");
                var assetbuildinfomemberproperty = BuildAssetInfoListProperty.GetArrayElementAtIndex(i);
                var assetpathmemberproperty = assetbuildinfomemberproperty.FindPropertyRelative("AssetPath");
                var abnamememberproperty = assetbuildinfomemberproperty.FindPropertyRelative("ABPath");
                var abvariantnamememberproperty = assetbuildinfomemberproperty.FindPropertyRelative("ABVariantPath");
                EditorGUILayout.LabelField("Asset路径:", GUILayout.Width(70.0f), GUILayout.Height(20.0f));
                EditorGUILayout.LabelField(assetpathmemberproperty.stringValue, mTextAreaStyle, GUILayout.Width(600.0f), GUILayout.Height(20.0f));
                EditorGUILayout.LabelField("AB名:", GUILayout.Width(70.0f), GUILayout.Height(20.0f));
                EditorGUILayout.LabelField(abnamememberproperty.stringValue, mTextAreaStyle, GUILayout.Width(600.0f), GUILayout.Height(20.0f));
                EditorGUILayout.LabelField("AB变体名:", GUILayout.Width(70.0f), GUILayout.Height(20.0f));
                EditorGUILayout.LabelField(abvariantnamememberproperty.stringValue, mTextAreaStyle, GUILayout.Width(150.0f), GUILayout.Height(20.0f));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();


            //EditorGUILayout.BeginVertical("box");
            //EditorGUILayout.LabelField("AssetBundle打包信息:", GUILayout.Width(150.0f), GUILayout.Height(20.0f));
            //for (int i = 0; i < AssetBuildBuildInfoListProperty.arraySize; i++)
            //{
            //    EditorGUILayout.BeginVertical("box");
            //    var assetbundlebuildinfomemberproperty = AssetBuildBuildInfoListProperty.GetArrayElementAtIndex(i);
            //    var abpathmemberproperty = assetbundlebuildinfomemberproperty.FindPropertyRelative("ABPath");
            //    var depabpathmemberproperty = assetbundlebuildinfomemberproperty.FindPropertyRelative("DepABPathList");
            //    EditorGUILayout.LabelField("AB路径:", GUILayout.Width(70.0f), GUILayout.Height(20.0f));
            //    EditorGUILayout.LabelField(abpathmemberproperty.stringValue, mTextAreaStyle, GUILayout.Width(600.0f), GUILayout.Height(20.0f));
            //    if(depabpathmemberproperty.arraySize > 0)
            //    {
            //        EditorGUILayout.LabelField("依赖AB路径:", GUILayout.Width(100.0f), GUILayout.Height(20.0f));
            //        for (int j = 0; j < depabpathmemberproperty.arraySize; j++)
            //        {
            //            var depabpathmemberindexproperty = depabpathmemberproperty.GetArrayElementAtIndex(j);
            //            EditorGUILayout.LabelField(depabpathmemberindexproperty.stringValue, mTextAreaStyle, GUILayout.Width(600.0f), GUILayout.Height(20.0f));
            //        }
            //    }
            //    else
            //    {
            //        EditorGUILayout.LabelField("无依赖AB路径", GUILayout.Width(100.0f), GUILayout.Height(20.0f));
            //    }
            //    EditorGUILayout.EndVertical();
            //}
            //EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
