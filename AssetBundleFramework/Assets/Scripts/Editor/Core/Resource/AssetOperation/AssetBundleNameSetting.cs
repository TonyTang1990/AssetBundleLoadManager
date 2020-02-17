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
    /// <param name="abname">指定AB名字，为空表示使用Asset自身名字小写作为AB名字</param>
    public static void AutoSetAssetBundleName(Object obj, string abname = "")
    {
        if(obj != null)
        {
            var assetpath = AssetDatabase.GetAssetPath(obj);
            var assetimporter = AssetImporter.GetAtPath(assetpath);
            if (assetimporter != null)
            {
                var newabname = string.IsNullOrEmpty(abname) ? obj.name.ToLower() : abname;
                assetimporter.assetBundleName = newabname;
                DIYLog.Log(string.Format("设置资源:{0}的AB名字为:{1}", assetpath, newabname));
            }
        }
    }

    [MenuItem("Assets/Create/自动设置AB名 #s", true)]
    private static bool ValidateQuickSetABName()
    {
        return Selection.objects != null;
    }
}