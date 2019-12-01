/*
 * File Name:               LightMapDetailWindow.cs
 *
 * Description:             场景光照贴图信息查看工具
 * Author:                  tanghuan <435853363@qq.com>
 * Create Date:             2018/02/08
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEditor.SceneManagement;

public class LightMapDetailWindow : EditorWindow {

    [MenuItem("Tools/LightMap/LightMapDetail", false, 110)]
    public static void OpenLightMapDetailWindow()
    {
        LightMapDetailWindow lmdw = EditorWindow.GetWindow<LightMapDetailWindow>();
        lmdw.Show();
    }

    void OnGUI()
    {
        var lminfos = LightmapSettings.lightmaps;
        var activescene = EditorSceneManager.GetActiveScene();
        GUILayout.Label(string.Format("当前场景名:{0}", activescene.name), EditorStyles.boldLabel);
        GUILayout.Label("当前场景LightMap信息:", EditorStyles.boldLabel);
        GUILayout.Label(string.Format("LightmapSettings.lightmaps.Length:{0}", lminfos.Length), EditorStyles.boldLabel);
        for(int i = 0; i < lminfos.Length; i++)
        {
            GUILayout.Label(string.Format("索引号:{0}", i), EditorStyles.boldLabel);
            GUILayout.Label(string.Format("lightmapColor:{0}", lminfos[i].lightmapColor == null ? "无" : lminfos[i].lightmapColor.name), EditorStyles.boldLabel);
            GUILayout.Label(string.Format("lightmapDir:{0}", lminfos[i].lightmapDir == null ? "无" : lminfos[i].lightmapDir.name), EditorStyles.boldLabel);
        }
    }
}
