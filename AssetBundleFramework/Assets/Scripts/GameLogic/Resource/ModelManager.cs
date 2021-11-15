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
#if !NEW_RESOURCE
    /// <summary>
    /// 获取模型实例对象
    /// </summary>
    /// <param name="respath"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    /// <param name="loadmethod"></param>
    public void getModelInstance(string respath, Action<GameObject> callback, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
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
    }
#else
    /// <summary>
    /// 获取模型实例对象
    /// </summary>
    /// <param name="respath"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    public int getModelInstance(string respath, Action<GameObject, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        TResource.AssetLoader assetLoader;
        return TResource.ResourceModuleManager.Singleton.requstAssetSync<GameObject>(
        respath,
        out assetLoader,
        (loader, requestUid) =>
        {
            var modelPrefab = loader.obtainAsset<GameObject>();
            var modelinstance = UnityEngine.Object.Instantiate(modelPrefab);
#if UNITY_EDITOR
            ResourceUtility.FindMeshRenderShaderBack(modelinstance);
#endif
            callback?.Invoke(modelinstance, requestUid);
        },
        loadtype);
    }

    /// <summary>
    /// 异步获取模型实例对象
    /// </summary>
    /// <param name="respath"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    public int getModelInstanceAsync(string respath, Action<GameObject, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        TResource.AssetLoader assetLoader;
        return TResource.ResourceModuleManager.Singleton.requstAssetAsync<GameObject>(
        respath,
        out assetLoader,
        (loader, requestUid) =>
        {
            var modelPrefab = loader.obtainAsset<GameObject>();
            var modelinstance = UnityEngine.Object.Instantiate(modelPrefab);
            loader.bindAsset<GameObject>(modelinstance);
#if UNITY_EDITOR
            ResourceUtility.FindMeshRenderShaderBack(modelinstance);
#endif
            callback?.Invoke(modelinstance, requestUid);
        },
        loadtype);
    }
#endif
}