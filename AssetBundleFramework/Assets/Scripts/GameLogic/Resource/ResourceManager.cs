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
}