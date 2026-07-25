/*
 * Description:             ResourceManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.IO;
using TResource;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// ResourceManager.cs
/// 上层资源请求单例管理类
/// Note:
/// 为了支持异步，统一回调的形式返回资源
/// load***表示加载不直接绑定使用，一般用于预加载或者加载常驻资源
/// get***表示加载并直接绑定使用，一般用于返回指定资源使用
/// 方法接口参数含assetname的表示该资源不是单独打包
/// 方法接口参数不含assetname的表示该资源是单独打包
/// </summary>
public class ResourceManager : SingletonTemplate<ResourceManager>
{
    // Note:
    // 取消Asset异步加载有两种方式:
    // 1. 返回给上层AssetRequestHandle，然后AssetRequestHandle.Cancel()
    // 2. 返回给上层AssetLoader和AssetRequestHandle,AssetLoader.CancelRequest(AssetRequestHandle.RequestUID)

    /// <summary>
    /// 全局资源计数释放+请求打断管理器
    /// Note:
    /// 用于管理全局常驻资源的加载和释放
    /// </summary>
    public ResourceScope GlobalResourceScope
    {
        get;
        private set;
    }

    public ResourceManager()
    {
        GlobalResourceScope = new ResourceScope();
    }

    /// <summary>
    /// 清理全局资源计数释放+请求打断管理器
    /// </summary>
    public void ClearGlobalResourceScope()
    {
        GlobalResourceScope.Clear();
    }

    /// <summary>
    /// 加载所有Shader
    /// </summary>
    /// <param name="callback">资源会动啊</param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="loadtype">加载方式</param>
    public AssetBundleRequestHandle LoadAllShader(Action callback, ResourceScope resourceScope,
                                                  ResourceLoadType loadtype = ResourceLoadType.PermanentLoad)
    {
        BundleLoader bundleLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstABSync(
        ResourceConstData.ShaderABName,
        out bundleLoader,
        (bundleLoader, assetRequestHandle) =>
        {
            // 非AB模式会返回null
            DIYLog.Log($"LoadAllShader加载完成!");
            resourceScope.RemoveRequest(assetRequestHandle);
            if (bundleLoader == null || !assetRequestHandle.IsComplete)
            {
                callback?.Invoke();
                return;
            }
            var bundle = resourceScope.GetAssetBundle(bundleLoader);
            var allAssetNames = bundle?.GetAllAssetNames();
            if(allAssetNames != null)
            {
                for (int i = 0, length = allAssetNames.Length; i < length; i++)
                {
                    var assetName = Path.GetFileName(allAssetNames[i]);
                    if (!assetName.EndsWith(".shadervariants"))
                    {
                        LoadShader<Shader>(assetName, resourceScope, null, loadtype);
                    }
                    else
                    {
                        LoadShader<ShaderVariantCollection>(assetName, resourceScope,
                        (shaderVariants) =>
                        {
                            // Shader通过预加载ShaderVariantsCollection里指定的Shader来进行预编译
                            shaderVariants?.WarmUp();
                        },
                        loadtype);
                    }
                }
            }
            callback?.Invoke();
        },
        loadtype);
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 加载指定Shader
    /// </summary>
    /// <param name="shaderName">Shader资源名(含后缀)</param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="callBack">加载完成回调</param>
    /// <param name="loadType">加载方式</param>
    public AssetRequestHandle LoadShader<T>(string shaderName, ResourceScope resourceScope,
                                         Action<T> callBack = null,
                                         ResourceLoadType loadType = ResourceLoadType.PermanentLoad)
                                         where T : UnityEngine.Object
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<T>(
        shaderName,
        out assetLoader,
        (loader, assetRequestHandle) =>
        {
            DIYLog.Log($"LoadShader加载shaderName:{shaderName}完成!");
            resourceScope.RemoveRequest(assetRequestHandle);
            if (loader == null || !assetRequestHandle.IsComplete)
            {
                return;
            }
            var asset = resourceScope.GetAsset<T>(loader);
            callBack?.Invoke(asset as T);
        },
        loadType);
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 获取一个实例资源对象
    /// Note:
    /// 像GameObject这类型资源直接走对象绑定GameObject，ResourceScrop只负责资源请求取消不负责资源计数清理
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callBack">资源回调</param>
    /// <param name="resourceScope">请求打断管理器(目前要求必传)</param>
    /// <param name="parent">实例化对象的父节点</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle GetPrefabInstance(string resName, Action<GameObject, AssetRequestHandle> callBack,
                                                ResourceScope resourceScope, Transform parent = null,
                                                ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<GameObject>(
        resName,
        out assetLoader,
        (loader, assetRequestHandle) =>
        {
            DIYLog.Log($"GetPrefabInstance加载resName:{resName}完成!");
            resourceScope.RemoveRequest(assetRequestHandle);
            if (loader == null || !assetRequestHandle.IsComplete)
            {
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var modelPrefab = loader.ObtainAsset<GameObject>();
            var modelinstance = UnityEngine.Object.Instantiate(modelPrefab, parent);
            resourceScope.BindAsset<GameObject>(loader, modelinstance);
#if UNITY_EDITOR
            // ResourceUtility.FindMeshRenderShaderBack(modelinstance);
#endif
            callBack?.Invoke(modelinstance, assetRequestHandle);
        },
        loadType);
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 异步获取一个实例资源对象
    /// Note:
    /// 像GameObject这类型资源直接走对象绑定GameObject，ResourceScrop只负责资源请求取消不负责资源计数清理
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callBack">资源回调</param>
    /// <param name="resourceScope">请求打断管理器(目前要求必传)</param>
    /// <param name="parent">实例化对象的父节点</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle GetPrefabInstanceAsync(string resName, Action<GameObject, AssetRequestHandle> callBack,
                                                     ResourceScope resourceScope, Transform parent = null,
                                                     ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetAsync<GameObject>(
        resName,
        out assetLoader,
        (loader, assetRequestHandle) =>
        {
            DIYLog.Log($"GetPrefabInstanceAsync异步加载resName:{resName}完成!");
            resourceScope.RemoveRequest(assetRequestHandle);
            if (loader == null || !assetRequestHandle.IsComplete)
            {
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var modelPrefab = resourceScope.ObtainAsset<GameObject>(loader);
            var modelinstance = UnityEngine.Object.Instantiate(modelPrefab, parent);
            resourceScope.BindAsset<GameObject>(loader, modelinstance);
#if UNITY_EDITOR
            // ResourceUtility.FindMeshRenderShaderBack(modelinstance);
#endif
            callBack?.Invoke(modelinstance, assetRequestHandle);
        },
        loadType);
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 获取一个材质
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callback">资源回调</param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle GetMaterial(string resName, Action<Material, AssetRequestHandle> callback,
                                          ResourceScope resourceScope,
                                          ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<Material>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetMaterial加载resName:{resName}完成!");
                resourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var material = resourceScope.GetAsset<Material>(loader);
    #if UNITY_EDITOR
                // ResourceUtility.FindMaterialShaderBack(material);
    #endif
                callback?.Invoke(material, assetRequestHandle);
            },
            loadtype
        );
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 异步获取一个材质
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callback">资源回调</param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle GetMaterialAsync(string resName, Action<Material, AssetRequestHandle> callback,
                                               ResourceScope resourceScope,
                                               ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetAsync<Material>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetMaterialAsync异步加载resName:{resName}完成!");
                resourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var material = resourceScope.GetAsset<Material>(loader);
#if UNITY_EDITOR
                // ResourceUtility.FindMaterialShaderBack(material);
#endif
                callback?.Invoke(material, assetRequestHandle);
            },
            loadtype
        );
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 获取指定音效
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callback"></param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="loadtype"></param>
    public AssetRequestHandle GetAudioClip(string resName, Action<AudioClip, AssetRequestHandle> callback,
                                           ResourceScope resourceScope,
                                           ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<AudioClip>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetAudioClip加载resName:{resName}完成!");
                resourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var audioClip = resourceScope.GetAsset<AudioClip>(null);
                callback?.Invoke(audioClip, assetRequestHandle);
            },
            loadtype
        );
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 异步获取指定音效
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callback"></param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="loadtype"></param>
    public AssetRequestHandle GetAudioClipAsync(string resName, Action<AudioClip, AssetRequestHandle> callback,
                                                ResourceScope resourceScope,
                                                ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetAsync<AudioClip>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetAudioClipAsync异步加载resName:{resName}完成!");
                resourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var audioClip = resourceScope.GetAsset<AudioClip>(loader);
                callback?.Invoke(audioClip, assetRequestHandle);
            },
            loadtype
        );
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 获取视频Clip
    /// </summary>
    /// <param name="videoName"></param>
    /// <param name="callback"></param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="loadtype"></param>
    /// <returns></returns>
    public AssetRequestHandle GetVideoClip(string videoName, Action<VideoClip, AssetRequestHandle> callback,
                                           ResourceScope resourceScope,
                                           ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<VideoClip>(
            videoName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetVideoClip加载videoName:{videoName}完成!");
                resourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var videoClip = resourceScope.GetAsset<VideoClip>(loader);
                callback?.Invoke(videoClip, assetRequestHandle);
            },
            loadtype
        );
        resourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }
}
