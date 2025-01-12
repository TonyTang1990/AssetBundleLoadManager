/*
 * Description:             ResourceManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.IO;
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
    /// <summary>
    /// 加载所有Shader
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <param name="callback">资源会动啊</param>
    /// <param name="loadtype">加载方式</param>
    public int loadAllShader(string respath, Action callback, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.PermanentLoad)
    {
        TResource.BundleLoader bundleLoader;
        return TResource.ResourceModuleManager.Singleton.RequstAssetBundleSync(
        respath,
        out bundleLoader,
        (loader, requestUid) =>
        {
            var bundle = loader?.GetAssetBundle();
            var allAssetNames = bundle?.GetAllAssetNames();
            if(allAssetNames != null)
            {
                TResource.AssetLoader assetLoader;
                for (int i = 0, length = allAssetNames.Length; i < length; i++)
                {
                    if (!allAssetNames[i].EndsWith(".shadervariants"))
                    {
                        TResource.ResourceModuleManager.Singleton.RequstAssetSync<Shader>(
                        allAssetNames[i],
                        out assetLoader,
                        (loader2, requestUid2) =>
                        {
                            // SVC的WarmUp就会触发相关Shader的预编译，触发预编译之后再加载Shader Asset即可
                            loader2.ObtainAsset<Shader>();
                        },
                        loadtype);
                    }
                    else
                    {
                        TResource.ResourceModuleManager.Singleton.RequstAssetSync<ShaderVariantCollection>(
                        allAssetNames[i],
                        out assetLoader,
                        (loader3, requestUid3) =>
                        {
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
    /// <param name="respath">资源路径</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int getPrefabInstance(string respath, Action<GameObject, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        TResource.AssetLoader assetLoader;
        return TResource.ResourceModuleManager.Singleton.RequstAssetSync<GameObject>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var prefab = loader.ObtainAsset<GameObject>();
                var prefabinstance = UnityEngine.Object.Instantiate<GameObject>(prefab);
                //不修改实例化后的名字，避免上层逻辑名字对不上
                //goinstance.name = goasset.name;
                // 绑定owner对象，用于判定是否还有有效对象引用AB资源
                loader.BindAsset<GameObject>(prefabinstance);
    #if UNITY_EDITOR
                ResourceUtility.FindMeshRenderShaderBack(prefabinstance);
    #endif
                callback?.Invoke(prefabinstance, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步获取一个实例资源对象
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int getPrefabInstanceAsync(string respath, out TResource.AssetLoader assetLoader, Action<GameObject, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        return TResource.ResourceModuleManager.Singleton.RequstAssetAsync<GameObject>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var prefab = loader.ObtainAsset<GameObject>();
                var prefabinstance = UnityEngine.Object.Instantiate<GameObject>(prefab);
                //不修改实例化后的名字，避免上层逻辑名字对不上
                //goinstance.name = goasset.name;
                // 绑定owner对象，用于判定是否还有有效对象引用AB资源
                loader.BindAsset<GameObject>(prefabinstance);
    #if UNITY_EDITOR
                ResourceUtility.FindMeshRenderShaderBack(prefabinstance);
    #endif
                callback?.Invoke(prefabinstance, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 获取一个材质
    /// </summary>
    /// <param name="owner">资源绑定对象</param>
    /// <param name="respath">资源路径</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int getMaterial(UnityEngine.Object owner, string respath, Action<Material, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        TResource.AssetLoader assetLoader;
        return TResource.ResourceModuleManager.Singleton.RequstAssetSync<Material>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var material = loader.BindAsset<Material>(owner);
    #if UNITY_EDITOR
                ResourceUtility.FindMaterialShaderBack(material);
    #endif
                callback?.Invoke(material, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步获取一个材质
    /// </summary>
    /// <param name="owner">资源绑定对象</param>
    /// <param name="respath">资源路径</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public int getMaterialAsync(UnityEngine.Object owner, string respath, out TResource.AssetLoader assetLoader, Action<Material, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        return TResource.ResourceModuleManager.Singleton.RequstAssetAsync<Material>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var material = loader.BindAsset<Material>(owner);
#if UNITY_EDITOR
                ResourceUtility.FindMaterialShaderBack(material);
#endif
                callback?.Invoke(material, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 获取指定音效
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="respath"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    public int getAudioClip(UnityEngine.Object owner, string respath, Action<AudioClip, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        TResource.AssetLoader assetLoader;
        return TResource.ResourceModuleManager.Singleton.RequstAssetSync<AudioClip>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var audioClip = loader.BindAsset<AudioClip>(owner);
                callback?.Invoke(audioClip, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 异步获取指定音效
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="respath"></param>
    /// <param name="assetLoader"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    public int getAudioClipAsync(UnityEngine.Object owner, string respath, out TResource.AssetLoader assetLoader, Action<AudioClip, int> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        return TResource.ResourceModuleManager.Singleton.RequstAssetSync<AudioClip>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var audioClip = loader.BindAsset<AudioClip>(owner);
                callback?.Invoke(audioClip, requestUid);
            },
            loadtype
        );
    }

    /// <summary>
    /// 获取视频Clip
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="videoPath"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    /// <returns></returns>
    public VideoClip getVideoClip(UnityEngine.Object owner, string videoPath, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        TResource.AssetLoader assetLoader;
        TResource.ResourceModuleManager.Singleton.RequstAssetSync<VideoClip>(
            videoPath,
            out assetLoader,
            null,
            loadtype
        );
        var videoClip = assetLoader.BindAsset<VideoClip>(owner);
        return videoClip;
    }
}