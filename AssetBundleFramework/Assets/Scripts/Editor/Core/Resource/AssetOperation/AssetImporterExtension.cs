/*
 * Description:             AssetImporterExtension.cs脚本导入设置扩展
 * Author:                  tanghuan
 * Create Date:             2018/04/10
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AssetImporterExtension.cs脚本导入设置扩展
/// </summary>
public class AssetImporterExtension : SingletonTemplate<AssetImporterExtension>{

    /// <summary>
    /// 修改指定asset的ab名字设置
    /// </summary>
    /// <param name="assetpath"></param>
    /// <param name="abname"></param>
    public void changeAssetBundleName(string assetpath, string abname)
    {
        var assetimporter = AssetImporter.GetAtPath(assetpath);
        if(assetimporter != null)
        {
            assetimporter.assetBundleName = abname;
        }
        else
        {
            Debug.LogError(string.Format("找不到路径为:{0}的Assetimporter!", assetpath));
        }
    }
}
