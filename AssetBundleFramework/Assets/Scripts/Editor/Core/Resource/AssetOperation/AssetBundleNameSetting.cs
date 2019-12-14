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
        foreach(var obj in Selection.objects)
        {
            AutoSetAssetBundleName(obj);
        }
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 自动设置指定对象AssetBundle名
    /// </summary>
    /// <param name="obj"></param>
    private static void AutoSetAssetBundleName(Object obj)
    {
        if(obj != null)
        {
            var assetpath = AssetDatabase.GetAssetPath(obj);
            var assetimporter = AssetImporter.GetAtPath(assetpath);
            if (assetimporter != null)
            {
                assetimporter.assetBundleName = obj.name.ToLower();
                DIYLog.Log(string.Format("设置资源:{0}的AB名字为:{1}", assetpath, obj.name.ToLower()));
            }
        }
    }

    [MenuItem("Assets/Create/自动设置AB名 #s", true)]
    private static bool ValidateQuickSetABName()
    {
        return Selection.objects != null;
    }
}