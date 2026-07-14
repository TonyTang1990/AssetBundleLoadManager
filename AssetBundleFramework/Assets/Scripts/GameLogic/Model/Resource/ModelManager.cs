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
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callBack"></param>
    /// <param name="loadType"></param>
    public int GetModelInstance(string resName, Action<GameObject, int> callBack = null,
                                TResource.ResourceLoadType loadType = TResource.ResourceLoadType.NormalLoad)
    {
        TResource.AssetLoader assetLoader;
        return TResource.ResourceModuleManager.Singleton.RequstAssetSync<GameObject>(
        resName,
        out assetLoader,
        (loader, requestUid) =>
        {
            var modelPrefab = loader.ObtainAsset<GameObject>();
            var modelinstance = UnityEngine.Object.Instantiate(modelPrefab);
            loader.BindAsset<GameObject>(modelinstance);
#if UNITY_EDITOR
            // ResourceUtility.FindMeshRenderShaderBack(modelinstance);
#endif
            callBack?.Invoke(modelinstance, requestUid);
        },
        loadType);
    }

    /// <summary>
    /// 异步获取模型实例对象
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callBack"></param>
    /// <param name="loadType"></param>
    public int GetModelInstanceAsync(string resName, Action<GameObject, int> callBack = null,
                                     TResource.ResourceLoadType loadType = TResource.ResourceLoadType.NormalLoad)
    {
        TResource.AssetLoader assetLoader;
        return TResource.ResourceModuleManager.Singleton.RequstAssetAsync<GameObject>(
        resName,
        out assetLoader,
        (loader, requestUid) =>
        {
            var modelPrefab = loader.ObtainAsset<GameObject>();
            var modelinstance = UnityEngine.Object.Instantiate(modelPrefab);
            loader.BindAsset<GameObject>(modelinstance);
#if UNITY_EDITOR
            // ResourceUtility.FindMeshRenderShaderBack(modelinstance);
#endif
            callBack?.Invoke(modelinstance, requestUid);
        },
        loadType);
    }
}