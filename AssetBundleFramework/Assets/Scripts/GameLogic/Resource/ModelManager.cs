/*
 * Description:             ModelManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// ModelManager.cs
/// 模型管理单例类
/// </summary>
public class ModelManager : SingletonTemplate<ModelManager>
{
    /// <summary>
    /// 获取模型实例对象
    /// </summary>
    /// <param name="respath"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    /// <param name="loadmethod"></param>
    public void getModelInstance(string respath, Action<GameObject> callback, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
#if !NEW_RESOURCE
        ResourceModuleManager.Singleton.requstResource(
        respath,
        (abi) =>
        {
            var assetname = Path.GetFileName(respath);
            var modelinstance = abi.instantiateAsset(assetname);
#if UNITY_EDITOR
            ResourceUtility.FindMeshRenderShaderBack(modelinstance);
#endif
            callback(modelinstance);
        });
#else

#endif
    }
}