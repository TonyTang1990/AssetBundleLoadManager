/*
 * Description:             BuildPreprocess.cs
 * Author:                  TANGHUAN
 * Create Date:             2019/12/13
 */

using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

#if UNITY_2018
/// <summary>
/// 打包编译前的预处理
/// </summary>
public class BuildPreprocess : IPreprocessBuildWithReport
{
    public int callbackOrder
    {
        get
        {
            return 0;
        }
    }

    /// <summary>
    /// 打包编译前的预处理接口
    /// </summary>
    /// <param name="report"></param>
    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("OnPreprocessBuild()");
    }
}
#endif