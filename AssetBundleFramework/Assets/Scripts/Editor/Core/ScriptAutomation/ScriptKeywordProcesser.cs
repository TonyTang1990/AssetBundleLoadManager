/*
 * Description:             脚本模板自定义Keyword处理脚本
 * Author:                  tanghuan
 * Create Date:             2018/04/01
 */

#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 脚本模板自定义Keyword处理脚本
/// </summary>
public class ScriptKeywordProcesser : UnityEditor.AssetModificationProcessor
{

    /// <summary>
    /// 非用户导入的Asset创建回调(e.g. .meta文件)
    /// </summary>
    /// <param name="assetpath"></param>
    public static void OnWillCreateAsset(string assetpath)
    {
        assetpath = assetpath.Replace(".meta", string.Empty);
        int index = assetpath.LastIndexOf(".");
        if (index <= 0)
        {
            return;
        }
        //判定是否是cs脚本
        string filepostfix = assetpath.Substring(index);
        if (!filepostfix.Equals(".cs"))
        {
            return;
        }
        //判定脚本文件是否存在
        index = Application.dataPath.LastIndexOf("Assets");
        assetpath = Application.dataPath.Substring(0, index) + assetpath;
        if (!File.Exists(assetpath))
        {
            return;
        }

        var filecontent = File.ReadAllText(assetpath);
        filecontent = replaceKeywords(filecontent);
        File.WriteAllText(assetpath, filecontent, Encoding.UTF8);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 替换文本Keyword
    /// Note:
    /// 自定义Keyword替换规则写在这里
    /// </summary>
    /// <param name="filecontent"></param>
    /// <returns></returns>
    private static string replaceKeywords(string filecontent)
    {
        string author = WindowsIdentity.GetCurrent().Name;
        // 只取最终用户名
        var splashindex = author.IndexOf("\\");
        if (splashindex > 0)
        {
            author = author.Substring(0, splashindex);
        }

        filecontent = filecontent.Replace("#AUTHOR#", author);
        filecontent = filecontent.Replace("#CREATEDATE#", DateTime.Now.ToString("yyyy//MM/dd"));
        return filecontent;
    }
}
 #endif