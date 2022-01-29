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
            loader.bindAsset<GameObject>(modelinstance);
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
}