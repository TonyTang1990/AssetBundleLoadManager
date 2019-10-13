/*
 * Description:             AssetBundleNameSetting.cs
 * Author:                  TONYTANG
 * Create Date:             2019/10/08
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AssetBundleNameSetting.cs
/// 快速设置资源AB名字
/// </summary>
public class AssetBundleNameSetting
{
    [MenuItem("Assets/Create/自动设置AB名 #&s", false)]
    public static void QuickSetABName()
    {
        var assetpath = AssetDatabase.GetAssetPath(Selection.activeObject);
        var assetimporter = AssetImporter.GetAtPath(assetpath);
        if(assetimporter != null)
        {
            assetimporter.assetBundleName = Selection.activeObject.name.ToLower();
            DIYLog.Log(string.Format("设置资源:{0}的AB名字为:{1}", assetpath, Selection.activeObject.name.ToLower()));
            AssetDatabase.SaveAssets();
        }
    }

    [MenuItem("Assets/Create/自动设置AB名 #s", true)]
    private static bool ValidateQuickSetABName()
    {
        return Selection.activeObject != null;
    }
}