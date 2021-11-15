/*
 * Description:             ResourceManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.IO;
using UnityEngine;

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
#if !NEW_RESOURCE
    /// <summary>
    /// 加载所有Shader
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <param name="callback">资源会动啊</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public void loadAllShader(string respath, Action callback, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        ResourceModuleManager.Singleton.requstResource(
        respath,
        (abi) =>
        {
            var svc = abi.loadAsset<ShaderVariantCollection>(ResourceConstData.ShaderVariantsAssetName);
            // Shader通过预加载ShaderVariantsCollection里指定的Shader来进行预编译
            svc?.WarmUp();
            // SVC的WarmUp就会触发相关Shader的预编译，触发预编译之后再加载Shader Asset即可
            abi.loadAllAsset<Shader>();
            callback?.Invoke();
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 获取一个实例资源对象
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void getPrefabInstance(string respath, Action<GameObject> callback = null, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        ResourceModuleManager.Singleton.requstResource(respath,
        (abi) =>
        {
            var assetname = Path.GetFileName(respath);
            var prefabinstance = abi.instantiateAsset(assetname);
#if UNITY_EDITOR
            ResourceUtility.FindMeshRenderShaderBack(prefabinstance);
#endif
            callback?.Invoke(prefabinstance);
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 获取一个材质
    /// </summary>
    /// <param name="owner">资源绑定对象</param>
    /// <param name="respath">资源路径</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void getMaterial(UnityEngine.Object owner, string respath, Action<Material> callback = null, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        ResourceModuleManager.Singleton.requstResource(respath,
        (abi) =>
        {
            var assetname = Path.GetFileName(respath);
            var material = abi.getAsset<Material>(owner, assetname);
#if UNITY_EDITOR
            ResourceUtility.FindMaterialShaderBack(material);
#endif
            callback?.Invoke(material);
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 获取指定音效
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="respath"></param>
    /// <param name="callback"></param>
    /// <param name="loadtype"></param>
    /// <param name="loadmethod"></param>
    public void getAudioClip(UnityEngine.Object owner, string respath, Action<AudioClip> callback = null, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        ResourceModuleManager.Singleton.requstResource(respath,
        (ari) =>
        {
            var assetname = Path.GetFileName(respath);
            var clip = ari.getAsset<AudioClip>(owner, assetname);
            callback?.Invoke(clip);
        },
        loadtype,
        loadmethod);
    }
#else
    /// <summary>
    /// 加载所有Shader
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <param name="callback">资源会动啊</param>
    /// <param name="loadtype">加载方式</param>
    public int loadAllShader(string respath, Action callback, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.PermanentLoad)
    {
        TResource.BundleLoader bundleLoader;
        return TResource.ResourceModuleManager.Singleton.requstAssetBundleSync(
        respath,
        out bundleLoader,
        (loader, requestUid) =>
        {
            var bundle = loader.getAssetBundle();
            var allAssetNames = bundle.GetAllAssetNames();
            TResource.AssetLoader assetLoader;
            for (int i = 0, length = allAssetNames.Length; i < length; i++)
            {
                if(!allAssetNames[i].EndsWith(".shadervariants"))
                {
                    TResource.ResourceModuleManager.Singleton.requstAssetSync<Shader>(
                    allAssetNames[i],
                    out assetLoader,
                    (loader2, requestUid2) =>
                    {
                        // SVC的WarmUp就会触发相关Shader的预编译，触发预编译之后再加载Shader Asset即可
                        loader2.getAsset<Shader>();
                    },
                    loadtype);
                }
                else
                {
                    TResource.ResourceModuleManager.Singleton.requstAssetSync<ShaderVariantCollection>(
                    allAssetNames[i],
                    out assetLoader,
                    (loader3, requestUid3) =>
                    {
                        var shaderVariants = loader3.getAsset<ShaderVariantCollection>();
                        // Shader通过预加载ShaderVariantsCollection里指定的Shader来进行预编译
                        shaderVariants?.WarmUp();
                    },
                    loadtype);
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
        return TResource.ResourceModuleManager.Singleton.requstAssetSync<GameObject>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var prefab = loader.obtainAsset<GameObject>();
                var prefabinstance = UnityEngine.Object.Instantiate<GameObject>(prefab);
                //不修改实例化后的名字，避免上层逻辑名字对不上
                //goinstance.name = goasset.name;
                // 绑定owner对象，用于判定是否还有有效对象引用AB资源
                loader.bindAsset<GameObject>(prefabinstance);
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
    public int getPrefabInstanceAsync(string respath, out TResource.AssetLoader assetLoader, Action<GameObject> callback = null, TResource.ResourceLoadType loadtype = TResource.ResourceLoadType.NormalLoad)
    {
        return TResource.ResourceModuleManager.Singleton.requstAssetAsync<GameObject>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var prefab = loader.obtainAsset<GameObject>();
                var prefabinstance = UnityEngine.Object.Instantiate<GameObject>(prefab);
                //不修改实例化后的名字，避免上层逻辑名字对不上
                //goinstance.name = goasset.name;
                // 绑定owner对象，用于判定是否还有有效对象引用AB资源
                loader.bindAsset<GameObject>(prefabinstance);
    #if UNITY_EDITOR
                ResourceUtility.FindMeshRenderShaderBack(prefabinstance);
    #endif
                callback?.Invoke(prefabinstance);
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
        return TResource.ResourceModuleManager.Singleton.requstAssetSync<Material>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var material = loader.bindAsset<Material>(owner);
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
        return TResource.ResourceModuleManager.Singleton.requstAssetAsync<Material>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var material = loader.bindAsset<Material>(owner);
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
        return TResource.ResourceModuleManager.Singleton.requstAssetSync<AudioClip>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var audioClip = loader.bindAsset<AudioClip>(owner);
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
        return TResource.ResourceModuleManager.Singleton.requstAssetSync<AudioClip>(
            respath,
            out assetLoader,
            (loader, requestUid) =>
            {
                var audioClip = loader.bindAsset<AudioClip>(owner);
                callback?.Invoke(audioClip, requestUid);
            },
            loadtype
        );
    }
#endif
}