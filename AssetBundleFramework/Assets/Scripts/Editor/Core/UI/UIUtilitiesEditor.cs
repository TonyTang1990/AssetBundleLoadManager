/*
 * Description:             UIUtilitiesEditor.cs
 * Author:                  TANGHUAN
 * Create Date:             2020/10/16
 */

using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// UI编辑器工具
/// </summary>
public static class UIUtilitiesEditor
{
    /// <summary>
    /// 添加指定组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <returns></returns>
    public static T AddComponent<T>(GameObject target) where T : Component
    {
        GameObject newObj = new GameObject();
        if (target)
            newObj.transform.SetParent(target.transform);
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localScale = Vector3.one;

        T com = newObj.AddComponent<T>();
        newObj.layer = target.layer;

        Selection.activeTransform = com.transform;
        return com;
    }
}