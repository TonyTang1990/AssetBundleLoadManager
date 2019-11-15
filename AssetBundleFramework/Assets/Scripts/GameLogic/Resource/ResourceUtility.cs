/*
 * Description:             ResourceUtility.cs
 * Author:                  TANGHUAN
 * Create Date:             2019/11/12
 */

using System;
using UnityEngine;

/// <summary>
/// 资源静态工具类
/// </summary>
public static class ResourceUtility
{
    /// <summary>
    /// Editor模式下，找回MeshRender & SkinMeshRender的Shader显示
    /// </summary>
    /// <param name="go"></param>
    public static void FindMeshRenderShaderBack(GameObject go)
    {
#if UNITY_EDITOR
        var skinmeshrenders = go.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (var skr in skinmeshrenders)
        {
            var mts = skr.materials;
            foreach (var mt in mts)
            {
                if (mt.shader != null)
                {
                    mt.shader = Shader.Find(mt.shader.name);
                }
            }
        }
        var meshrenders = go.GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in meshrenders)
        {
            var mts = mr.materials;
            foreach (var mt in mts)
            {
                if (mt.shader != null)
                {
                    mt.shader = Shader.Find(mt.shader.name);
                    DIYLog.Log(string.Format("{0}对象找回Shader:{1}显示！", go.name, mt.shader.name));
                }
            }
        }
#endif
    }

    /// <summary>
    /// Editor模式下，找回材质Shader显示
    /// </summary>
    /// <param name="go"></param>
    public static void FindMaterialShaderBack(Material mt)
    {
#if UNITY_EDITOR
        if (mt.shader != null)
        {
            mt.shader = Shader.Find(mt.shader.name);
            DIYLog.Log(string.Format("{0}材质找回Shader:{1}显示！", mt.name, mt.shader.name));
        }
#endif
    }
}