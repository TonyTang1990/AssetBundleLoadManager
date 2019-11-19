/*
 * Description:             ResourceManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
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
    /// 模块名
    /// </summary>
    public string ModuleName
    {
        get
        {
            return this.GetType().ToString();
        }
    }

    /// <summary>
    /// 加载所有Shader
    /// </summary>
    /// <param name="resname">资源名</param>
    /// <param name="callback">资源会动啊</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public void loadAllShader(string resname, Action callback, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        ResourceModuleManager.Singleton.requstResource(
        resname,
        (abi) =>
        {
            abi.loadAllAsset<Shader>();
            callback?.Invoke();
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 获取一个UI实例资源对象
    /// </summary>
    /// <param name="resname">资源名</param>
    /// <param name="assetname">Asset名</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void getUIInstance(string resname, string assetname = null, Action<GameObject> callback = null, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        if (string.IsNullOrEmpty(assetname))
        {
            assetname = resname;
        }
        ResourceModuleManager.Singleton.requstResource(resname,
        (abi) =>
        {
            var uiinstance = abi.instantiateAsset(assetname);
#if UNITY_EDITOR
            ResourceUtility.FindMeshRenderShaderBack(uiinstance);
#endif
            callback?.Invoke(uiinstance);
        },
        loadtype,
        loadmethod);
    }

    /// <summary>
    /// 获取一个共享的材质
    /// </summary>
    /// <param name="owner">资源绑定对象</param>
    /// <param name="resname">资源名</param>
    /// <param name="assetname">Asset名</param>
    /// <param name="callback">资源回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    /// <returns></returns>
    public void getShareMaterial(UnityEngine.Object owner, string resname, string assetname = null, Action<Material> callback = null, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        if(string.IsNullOrEmpty(assetname))
        {
            assetname = resname;
        }
        ResourceModuleManager.Singleton.requstResource(resname,
        (abi) =>
        {
            var material = abi.getAsset<Material>(owner, assetname);
#if UNITY_EDITOR
            ResourceUtility.FindMaterialShaderBack(material);
#endif
            callback?.Invoke(material);
        },
        loadtype,
        loadmethod);
    }
}