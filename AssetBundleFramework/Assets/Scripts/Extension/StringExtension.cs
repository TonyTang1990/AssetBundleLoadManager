/*
 * Description:             StringExtension.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/08
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StringExtension.cs
/// String扩展方法
/// </summary>
public static class StringExtension {

    /// <summary>
    /// 判定字符串是否为null或者""
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string s)
    {
        return string.IsNullOrEmpty(s);
    }

    /// <summary>
    /// 移除首个字符
    /// </summary>
    public static string RemoveFirstChar(this System.String str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        return str.Substring(1);
    }

    /// <summary>
    /// 移除末尾字符
    /// </summary>
    public static string RemoveLastChar(this System.String str)
    {
        if (string.IsNullOrEmpty(str))
            return str;
        return str.Substring(0, str.Length - 1);
    }
}