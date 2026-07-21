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
    /// 加载所有Shader
    /// </summary>
    /// <param name="callback">资源会动啊</param>
    /// <param name="loadtype">加载方式</param>
    public AssetBundleRequestHandle LoadAllShader(Action callback, ResourceLoadType loadtype = ResourceLoadType.PermanentLoad)
    {
        BundleLoader bundleLoader;
        return ResourceModuleManager.Singleton.RequstABSync(
        ResourceConstData.ShaderABName,
        out bundleLoader,
        (loader, assetRequestHandle) =>
        {
            // 非AB模式会返回null
            DIYLog.Log($"LoadAllShader加载完成!");
            if (loader == null || !assetRequestHandle.IsComplete)
            {
                callback?.Invoke();
                return;
            }
            var bundle = loader?.GetAssetBundle();
            var allAssetNames = bundle?.GetAllAssetNames();
            if(allAssetNames != null)
            {
                AssetLoader assetLoader;
                for (int i = 0, length = allAssetNames.Length; i < length; i++)
                {
                    var assetName = Path.GetFileName(allAssetNames[i]);
                    if (!assetName.EndsWith(".shadervariants"))
                    {
                        ResourceModuleManager.Singleton.RequstAssetSync<Shader>(
                        assetName,
                        out assetLoader,
                        (loader2, assetRequestHandle2) =>
                        {
                            DIYLog.Log($"LoadAllShader加载assetName:{assetName}完成!");
                            if (loader2 == null || !assetRequestHandle2.IsComplete)
                            {
                                return;
                            }
                            // SVC的WarmUp就会触发相关Shader的预编译，触发预编译之后再加载Shader Asset即可
                            loader2.ObtainAsset<Shader>();
                        },
                        loadtype);
                    }
                    else
                    {
                        ResourceModuleManager.Singleton.RequstAssetSync<ShaderVariantCollection>(
                        assetName,
                        out assetLoader,
                        (loader3, assetRequestHandle3) =>
                        {
                            DIYLog.Log($"LoadAllShader加载assetName:{assetName}完成!");
                            if (loader3 == null || !assetRequestHandle3.IsComplete)
                            {
                                return;
                            }
                            var shaderVariants = loader3.GetAsset<ShaderVariantCollection>();
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
    }

    /// <summary>
    /// 获取一个实例资源对象
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle GetPrefabInstance(string resName, Action<GameObject, AssetRequestHandle> callback = null,
                                                ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        return ResourceModuleManager.Singleton.RequstAssetSync<GameObject>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetPrefabInstance加载resName:{resName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var prefab = loader.ObtainAsset<GameObject>();
                var prefabInstance = UnityEngine.Object.Instantiate<GameObject>(prefab);
                //不修改实例化后的名字，避免上层逻辑名字对不上
                //goinstance.name = goasset.name;
                // 绑定owner对象，用于判定是否还有有效对象引用AB资源
                loader.BindAsset<GameObject>(prefabInstance);
    #if UNITY_EDITOR
                // ResourceUtility.FindMeshRenderShaderBack(prefabinstance);
    #endif
                callback?.Invoke(prefabInstance, assetRequestHandle);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步获取一个实例资源对象
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle GetPrefabInstanceAsync(string resName, out AssetLoader assetLoader,
                                                     Action<GameObject, AssetRequestHandle> callback = null,
                                                     ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        return ResourceModuleManager.Singleton.RequstAssetAsync<GameObject>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetPrefabInstanceAsync异步加载resName:{resName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var prefab = loader.ObtainAsset<GameObject>();
                var prefabInstance = UnityEngine.Object.Instantiate<GameObject>(prefab);
                //不修改实例化后的名字，避免上层逻辑名字对不上
                //goinstance.name = goasset.name;
                // 绑定owner对象，用于判定是否还有有效对象引用AB资源
                loader.BindAsset<GameObject>(prefabInstance);
    #if UNITY_EDITOR
                // ResourceUtility.FindMeshRenderShaderBack(prefabinstance);
    #endif
                callback?.Invoke(prefabInstance, assetRequestHandle);
            },
            loadtype
        );
    }

    /// <summary>
    /// 获取一个材质
    /// </summary>
    /// <param name="owner">资源绑定对象</param>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle GetMaterial(UnityEngine.Object owner, string resName,
                                          Action<Material, AssetRequestHandle> callback = null,
                                          ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        return ResourceModuleManager.Singleton.RequstAssetSync<Material>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetMaterial加载resName:{resName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var material = loader.BindAsset<Material>(owner);
    #if UNITY_EDITOR
                // ResourceUtility.FindMaterialShaderBack(material);
    #endif
                callback?.Invoke(material, assetRequestHandle);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步获取一个材质
    /// </summary>
    /// <param name="owner">资源绑定对象</param>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle GetMaterialAsync(UnityEngine.Object owner, string resName,
                                               out AssetLoader assetLoader,
                                               Action<Material, AssetRequestHandle> callback = null,
                                               ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        return ResourceModuleManager.Singleton.RequstAssetAsync<Material>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetMaterialAsync异步加载resName:{resName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var material = loader.BindAsset<Material>(owner);
#if UNITY_EDITOR
                // ResourceUtility.FindMaterialShaderBack(material);
#endif
                callback?.Invoke(material, assetRequestHandle);
            },
            loadtype
        );
    }

    /// <summary>
    /// 获取指定音效
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    public AssetRequestHandle GetAudioClip(UnityEngine.Object owner, string resName,
                                           Action<AudioClip, AssetRequestHandle> callback = null,
                                           ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        return ResourceModuleManager.Singleton.RequstAssetSync<AudioClip>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetAudioClip加载resName:{resName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var audioClip = loader.BindAsset<AudioClip>(owner);
                callback?.Invoke(audioClip, assetRequestHandle);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步获取指定音效
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="assetLoader"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    public AssetRequestHandle GetAudioClipAsync(UnityEngine.Object owner, string resName, out AssetLoader assetLoader,
                                                Action<AudioClip, AssetRequestHandle> callback = null,
                                                ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        return ResourceModuleManager.Singleton.RequstAssetAsync<AudioClip>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"GetAudioClipAsync异步加载resName:{resName}完成!");
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callback?.Invoke(null, assetRequestHandle);
                    return;
                }
                var audioClip = loader.BindAsset<AudioClip>(owner);
                callback?.Invoke(audioClip, assetRequestHandle);
            },
            loadtype
        );
    }

    /// <summary>
    /// 获取视频Clip
    /// </summary>
    /// <param name="videoName"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    /// <returns></returns>
    public VideoClip GetVideoClip(string videoName, ResourceLoadType loadtype = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        ResourceModuleManager.Singleton.RequstAssetSync<VideoClip>(
            videoName,
            out assetLoader,
            null,
            loadtype
        );
        VideoClip videoClip = null;
        if(assetLoader != null)
        {
            videoClip = assetLoader.GetAsset<VideoClip>();
        }
        return videoClip;
    }
}
