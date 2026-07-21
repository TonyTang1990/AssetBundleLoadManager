/*
 * Description:             ModelManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TResource;
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
    public AssetRequestHandle GetModelInstance(string resName, Action<GameObject, AssetRequestHandle> callBack = null,
                                               ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        return ResourceModuleManager.Singleton.RequstAssetSync<GameObject>(
        resName,
        out assetLoader,
        (loader, assetRequestHandle) =>
        {
            var modelPrefab = loader.ObtainAsset<GameObject>();
            var modelinstance = UnityEngine.Object.Instantiate(modelPrefab);
            loader.BindAsset<GameObject>(modelinstance);
#if UNITY_EDITOR
            // ResourceUtility.FindMeshRenderShaderBack(modelinstance);
#endif
            callBack?.Invoke(modelinstance, assetRequestHandle);
        },
        loadType);
    }

    /// <summary>
    /// 异步获取模型实例对象
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callBack"></param>
    /// <param name="loadType"></param>
    public AssetRequestHandle GetModelInstanceAsync(string resName, Action<GameObject, AssetRequestHandle> callBack = null,
                                                    ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        return ResourceModuleManager.Singleton.RequstAssetAsync<GameObject>(
        resName,
        out assetLoader,
        (loader, assetRequestHandle) =>
        {
            var modelPrefab = loader.ObtainAsset<GameObject>();
            var modelinstance = UnityEngine.Object.Instantiate(modelPrefab);
            loader.BindAsset<GameObject>(modelinstance);
#if UNITY_EDITOR
            // ResourceUtility.FindMeshRenderShaderBack(modelinstance);
#endif
            callBack?.Invoke(modelinstance, assetRequestHandle);
        },
        loadType);
    }
}
