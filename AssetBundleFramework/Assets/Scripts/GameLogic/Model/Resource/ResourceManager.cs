/*
 * Description:             ResourceManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.IO;
using Cysharp.Threading.Tasks;
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

    /// <summary>
    /// 初始化资源管理器和全局资源作用域
    /// </summary>
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

    #region CallBack接口
    /// <summary>
    /// 加载Shader AssetBundle并等待其中所有Shader资源加载完成
    /// Note:
    /// 返回的AssetBundleRequestHandle只能打断Shader的AB加载
    /// 如果Shader AB已经加载完成，后续的Shader Asset加载是无法通过返回的AssetBundleRequestHandle打断的
    /// </summary>
    /// <param name="callBack">所有Shader资源加载完成回调</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>Shader AssetBundle请求句柄</returns>
    public AssetBundleRequestHandle LoadAllShader(Action callBack, ResourceScope resourceScope,
                                                  ResourceLoadType loadType = ResourceLoadType.PermanentLoad,
                                                  ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        if (resourceScope == null)
        {
            Debug.LogError("加载所有Shader失败，ResourceScope为空!");
            callBack?.Invoke();
            return null;
        }
        BundleLoader bundleLoader;
        var shaderABName = ResourcePath.GetABPathWithPostFix(ResourceConstData.ShaderABName);
        Action<BundleLoader, AssetBundleRequestHandle> completeHandler =
        (loader, assetBundleRequestHandle) =>
        {
            DIYLog.Log("LoadAllShader加载完成!");
            resourceScope.RemoveRequest(assetBundleRequestHandle);
            if (loader == null || !assetBundleRequestHandle.IsSuccess)
            {
                callBack?.Invoke();
                return;
            }
            var bundle = resourceScope.GetAssetBundle(loader);
            var allAssetNames = bundle?.GetAllAssetNames();
            if (allAssetNames == null || allAssetNames.Length == 0)
            {
                callBack?.Invoke();
                return;
            }
            var uncompletedCount = allAssetNames.Length;
            Action assetCompleteHandler = () =>
            {
                uncompletedCount--;
                if (uncompletedCount == 0)
                {
                    callBack?.Invoke();
                }
            };
            for (int i = 0; i < allAssetNames.Length; i++)
            {
                var assetName = Path.GetFileName(allAssetNames[i]);
                if (!assetName.EndsWith(".shadervariants"))
                {
                    GetAssetByCb<Shader>(assetName, (shader, assetRequestHandle) =>
                    {
                        assetCompleteHandler();
                    }, resourceScope, loadType, loadMethod);
                }
                else
                {
                    GetAssetByCb<ShaderVariantCollection>(assetName, (shaderVariants, assetRequestHandle) =>
                    {
                        shaderVariants?.WarmUp();
                        assetCompleteHandler();
                    }, resourceScope, loadType, loadMethod);
                }
            }
        };
        var requestHandle = loadMethod == ResourceLoadMethod.Sync
                            ? ResourceModuleManager.Singleton.RequstABSync(shaderABName, out bundleLoader,
                            completeHandler, loadType)
                            : ResourceModuleManager.Singleton.RequstABAsync(shaderABName, out bundleLoader,
                            completeHandler, loadType);
        resourceScope.RecordRequest(requestHandle);
        return requestHandle;
    }

    /// <summary>
    /// 加载指定Shader资源并通过回调返回
    /// </summary>
    /// <typeparam name="T">Shader或ShaderVariantCollection资源类型</typeparam>
    /// <param name="shaderName">Shader资源名</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="callBack">资源加载完成回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle LoadShader<T>(string shaderName, ResourceScope resourceScope,
                                            Action<T> callBack = null,
                                            ResourceLoadType loadType = ResourceLoadType.PermanentLoad,
                                            ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
                                            where T : UnityEngine.Object
    {
        return GetAssetByCb<T>(shaderName, (asset, _) =>
        {
            if (asset != null)
            {
                callBack?.Invoke(asset);
            }
        }, resourceScope, loadType, loadMethod);
    }

    /// <summary>
    /// 加载Prefab并实例化后通过回调返回
    /// 实例与资源通过ResourceScope建立Owner绑定
    /// </summary>
    /// <param name="resName">Prefab资源名</param>
    /// <param name="callBack">实例创建完成回调</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="parent">实例父节点</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle GetPrefabInstance(string resName,
                                                Action<GameObject, AssetRequestHandle> callBack,
                                                ResourceScope resourceScope, Transform parent = null,
                                                ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        if (resourceScope == null)
        {
            Debug.LogError($"获取Prefab实例:{resName}失败，ResourceScope为空!");
            callBack?.Invoke(null, null);
            return null;
        }
        AssetLoader assetLoader;
        var requestHandle = RequestAssetAsync<GameObject>(resName, out assetLoader,
        (loader, assetRequestHandle) =>
        {
            DIYLog.Log($"GetPrefabInstance加载resName:{resName}完成!");
            resourceScope.RemoveRequest(assetRequestHandle);
            if (loader == null || !assetRequestHandle.IsSuccess)
            {
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var prefab = resourceScope.ObtainAsset<GameObject>(loader);
            if (prefab == null)
            {
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            resourceScope.BindAsset<GameObject>(loader, instance);
            callBack?.Invoke(instance, assetRequestHandle);
        }, loadType, loadMethod);
        resourceScope.RecordRequest(requestHandle);
        return requestHandle;
    }

    /// <summary>
    /// 加载材质资源并通过回调返回
    /// </summary>
    /// <param name="resName">材质资源名</param>
    /// <param name="callBack">资源加载完成回调</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle GetMaterial(string resName, Action<Material, AssetRequestHandle> callBack,
                                          ResourceScope resourceScope,
                                          ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                          ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        return GetAssetByCb(resName, callBack, resourceScope, loadType, loadMethod);
    }

    /// <summary>
    /// 加载音频资源并通过回调返回
    /// </summary>
    /// <param name="resName">音频资源名</param>
    /// <param name="callBack">资源加载完成回调</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle GetAudioClip(string resName, Action<AudioClip, AssetRequestHandle> callBack,
                                           ResourceScope resourceScope,
                                           ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                           ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        return GetAssetByCb(resName, callBack, resourceScope, loadType, loadMethod);
    }

    /// <summary>
    /// 加载视频资源并通过回调返回
    /// </summary>
    /// <param name="videoName">视频资源名</param>
    /// <param name="callBack">资源加载完成回调</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle GetVideoClip(string videoName, Action<VideoClip, AssetRequestHandle> callBack,
                                           ResourceScope resourceScope,
                                           ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                           ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        return GetAssetByCb(videoName, callBack, resourceScope, loadType, loadMethod);
    }

    /// <summary>
    /// 创建资源加载请求并在加载完成后执行回调
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resName">资源名</param>
    /// <param name="callBack">资源加载完成回调</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <param name="obtainAsset">是否仅获取资源而不增加引用计数</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle GetAssetByCb<T>(string resName,
                                              Action<T, AssetRequestHandle> callBack,
                                              ResourceScope resourceScope,
                                              ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                              ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync,
                                              bool obtainAsset = false)
                                              where T : UnityEngine.Object
    {
        if (resourceScope == null)
        {
            Debug.LogError($"获取资源:{resName}失败，ResourceScope为空!");
            callBack?.Invoke(null, null);
            return null;
        }
        AssetLoader assetLoader;
        var requestHandle = RequestAssetAsync<T>(resName, out assetLoader,
        (loader, assetRequestHandle) =>
        {
            DIYLog.Log($"GetAssetByCb加载resName:{resName}完成!");
            resourceScope.RemoveRequest(assetRequestHandle);
            if (loader == null || !assetRequestHandle.IsSuccess)
            {
                callBack?.Invoke(null, assetRequestHandle);
                return;
            }
            var asset = obtainAsset ? resourceScope.ObtainAsset<T>(loader) : resourceScope.GetAsset<T>(loader);
            callBack?.Invoke(asset, assetRequestHandle);
        }, loadType, loadMethod);
        resourceScope.RecordRequest(requestHandle);
        return requestHandle;
    }
    #endregion

    #region UniTask接口
    /// <summary>
    /// 获取指定Asset资源
    /// 加载失败或请求取消时返回null，不向上层抛出资源加载异常
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="resourceScope">资源计数释放+请求打断管理器(目前要求必传)</param>
    /// <param name="requestHandle">输出资源请求句柄</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>等待完成后返回资源对象的UniTask</returns>
    public UniTask<T> GetAssetAsync<T>(string resName, ResourceScope resourceScope,
                                       out ResourceRequestHandle requestHandle,
                                       ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                       ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
                                       where T : UnityEngine.Object
    {
        if (resourceScope == null)
        {
            Debug.LogError($"异步获取资源:{resName}失败，ResourceScope为空!");
            requestHandle = null;
            return UniTask.FromResult<T>(null);
        }
        AssetLoader assetLoader;
        var assetRequestHandle = RequestAssetAsync<T>(resName, out assetLoader, null, loadType, loadMethod);
        requestHandle = assetRequestHandle;
        return WaitAssetAsync<T>(assetLoader, assetRequestHandle, resourceScope, false);
    }

    /// <summary>
    /// 获取指定Asset资源但不增加资源引用计数
    /// 加载失败或请求取消时返回null，不向上层抛出资源加载异常
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resName">资源名</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="requestHandle">输出资源请求句柄</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>等待完成后返回资源对象的UniTask</returns>
    public UniTask<T> ObtainAssetAsync<T>(string resName, ResourceScope resourceScope,
                                          out ResourceRequestHandle requestHandle,
                                          ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                          ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
                                          where T : UnityEngine.Object
    {
        if (resourceScope == null)
        {
            Debug.LogError($"异步获取资源:{resName}失败，ResourceScope为空!");
            requestHandle = null;
            return UniTask.FromResult<T>(null);
        }
        AssetLoader assetLoader;
        var assetRequestHandle = RequestAssetAsync<T>(resName, out assetLoader, null, loadType, loadMethod);
        requestHandle = assetRequestHandle;
        return WaitAssetAsync<T>(assetLoader, assetRequestHandle, resourceScope, true);
    }

    /// <summary>
    /// 等待资源请求完成并根据请求结果从ResourceScope获取资源
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="assetLoader">资源加载器</param>
    /// <param name="requestHandle">Asset请求句柄</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="obtainAsset">是否仅获取资源而不增加引用计数</param>
    /// <returns>等待完成后返回资源对象的UniTask</returns>
    private async UniTask<T> WaitAssetAsync<T>(AssetLoader assetLoader,
                                                AssetRequestHandle requestHandle,
                                                ResourceScope resourceScope,
                                                bool obtainAsset)
                                                where T : UnityEngine.Object
    {
        resourceScope.RecordRequest(requestHandle);
        try
        {
            await requestHandle;
        }
        finally
        {
            resourceScope.RemoveRequest(requestHandle);
        }
        if (assetLoader == null || !requestHandle.IsSuccess)
        {
            return null;
        }
        return obtainAsset ? resourceScope.ObtainAsset<T>(assetLoader) : resourceScope.GetAsset<T>(assetLoader);
    }

    /// <summary>
    /// 异步加载指定Shader并返回资源
    /// </summary>
    /// <typeparam name="T">Shader或ShaderVariantCollection资源类型</typeparam>
    /// <param name="shaderName">Shader资源名</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="requestHandle">输出资源请求句柄</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>等待完成后返回Shader资源的UniTask</returns>
    public UniTask<T> LoadShaderAsync<T>(string shaderName, ResourceScope resourceScope,
                                         out ResourceRequestHandle requestHandle,
                                         ResourceLoadType loadType = ResourceLoadType.PermanentLoad,
                                         ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
                                         where T : UnityEngine.Object
    {
        return GetAssetAsync<T>(shaderName, resourceScope, out requestHandle, loadType, loadMethod);
    }

    /// <summary>
    /// UniTask方式加载所有Shader
    /// </summary>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="requestHandle">输出Shader AssetBundle请求句柄</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns>等待所有Shader加载和预热完成的UniTask</returns>
    public UniTask LoadAllShaderAsync(ResourceScope resourceScope,
                                      out ResourceRequestHandle requestHandle,
                                      ResourceLoadType loadType = ResourceLoadType.PermanentLoad)
    {
        if (resourceScope == null)
        {
            Debug.LogError("加载所有Shader失败，ResourceScope为空!");
            requestHandle = null;
            return UniTask.CompletedTask;
        }
        BundleLoader bundleLoader;
        var shaderABName = ResourcePath.GetABPathWithPostFix(ResourceConstData.ShaderABName);
        var assetBundleRequestHandle = ResourceModuleManager.Singleton.RequstABAsync(shaderABName, out bundleLoader, null, loadType);
        requestHandle = assetBundleRequestHandle;
        return WaitLoadAllShaderAsync(bundleLoader, assetBundleRequestHandle, resourceScope, loadType);
    }

    /// <summary>
    /// 等待Shader AssetBundle完成并逐个加载其中的Shader资源
    /// ShaderVariantCollection加载后会立即执行预热
    /// </summary>
    /// <param name="bundleLoader">Shader AssetBundle加载器</param>
    /// <param name="requestHandle">AssetBundle请求句柄</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="loadType">资源加载类型</param>
    /// <returns>等待所有Shader加载和预热完成的UniTask</returns>
    private async UniTask WaitLoadAllShaderAsync(BundleLoader bundleLoader,
                                                  AssetBundleRequestHandle requestHandle,
                                                  ResourceScope resourceScope,
                                                  ResourceLoadType loadType)
    {
        // 开始加载Shader AB
        Debug.Log($"开始加载Shader!");
        resourceScope.RecordRequest(requestHandle);
        try
        {
            await requestHandle;
        }
        finally
        {
            resourceScope.RemoveRequest(requestHandle);
        }

        if (bundleLoader == null || !requestHandle.IsSuccess)
        {
            return;
        }

        var bundle = resourceScope.GetAssetBundle(bundleLoader);
        var allAssetNames = bundle?.GetAllAssetNames();
        if (allAssetNames == null)
        {
            return;
        }

        var shaderTasks = new UniTask[allAssetNames.Length];
        for (int i = 0; i < allAssetNames.Length; i++)
        {
            var assetName = Path.GetFileName(allAssetNames[i]);
            if (!assetName.EndsWith(".shadervariants"))
            {
                shaderTasks[i] = LoadShaderAsync<Shader>(assetName, resourceScope, out _, loadType, ResourceLoadMethod.Async);
                Debug.Log($"开始加载Shader: {assetName}");
            }
            else
            {
                shaderTasks[i] = LoadAndWarmUpShaderVariantsAsync(assetName, resourceScope, loadType, ResourceLoadMethod.Async);
                Debug.Log($"开始加载ShaderVariantCollection: {assetName}");
            }
        }
        await UniTask.WhenAll(shaderTasks);
    }

    /// <summary>
    /// 加载ShaderVariantCollection并在加载完成后执行预热
    /// </summary>
    /// <param name="assetName">ShaderVariantCollection资源名</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>等待ShaderVariantCollection加载和预热完成的UniTask</returns>
    private async UniTask LoadAndWarmUpShaderVariantsAsync(string assetName, ResourceScope resourceScope,
                                                            ResourceLoadType loadType,
                                                            ResourceLoadMethod loadMethod)
    {
        var shaderVariants = await LoadShaderAsync<ShaderVariantCollection>(assetName, resourceScope,
                                                                            out _, loadType, loadMethod);
        shaderVariants?.WarmUp();
    }

    /// <summary>
    /// 异步获取一个实例资源对象
    /// 加载失败或请求取消时返回null
    /// </summary>
    /// <param name="resName">Prefab资源名</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="requestHandle">输出资源请求句柄</param>
    /// <param name="parent">实例父节点</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>等待完成后返回Prefab实例的UniTask</returns>
    public UniTask<GameObject> GetPrefabInstanceAsync(string resName, ResourceScope resourceScope,
                                                       out ResourceRequestHandle requestHandle,
                                                       Transform parent = null,
                                                       ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                       ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        if (resourceScope == null)
        {
            Debug.LogError($"异步获取Prefab实例:{resName}失败，ResourceScope为空!");
            requestHandle = null;
            return UniTask.FromResult<GameObject>(null);
        }
        AssetLoader assetLoader;
        var assetRequestHandle = RequestAssetAsync<GameObject>(resName, out assetLoader, null, loadType, loadMethod);
        requestHandle = assetRequestHandle;
        return WaitPrefabInstanceAsync(assetLoader, assetRequestHandle, resourceScope, parent);
    }

    /// <summary>
    /// 等待Prefab资源请求完成并创建绑定到ResourceScope的实例
    /// </summary>
    /// <param name="assetLoader">Prefab资源加载器</param>
    /// <param name="requestHandle">Asset请求句柄</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="parent">实例父节点</param>
    /// <returns>等待完成后返回Prefab实例的UniTask</returns>
    private async UniTask<GameObject> WaitPrefabInstanceAsync(AssetLoader assetLoader,
                                                               AssetRequestHandle requestHandle,
                                                               ResourceScope resourceScope,
                                                               Transform parent)
    {
        Debug.Log($"开始预制件加载！");
        resourceScope.RecordRequest(requestHandle);

        try
        {
            await requestHandle;
        }
        finally
        {
            resourceScope.RemoveRequest(requestHandle);
        }
        Debug.Log($"预制件加载完成！");

        if (assetLoader == null || !requestHandle.IsSuccess)
        {
            return null;
        }

        Debug.Log($"开始获取预制件Asset！");
        var prefab = resourceScope.ObtainAsset<GameObject>(assetLoader);
        if (prefab == null)
        {
            return null;
        }
        Debug.Log($"开始实例化预制件Asset！");
        var instance = UnityEngine.Object.Instantiate(prefab, parent);
        resourceScope.BindAsset<GameObject>(assetLoader, instance);
        return instance;
    }

    /// <summary>
    /// 异步获取一个材质，加载失败或请求取消时返回null
    /// </summary>
    /// <param name="resName">材质资源名</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="requestHandle">输出资源请求句柄</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>等待完成后返回材质资源的UniTask</returns>
    public UniTask<Material> GetMaterialAsync(string resName, ResourceScope resourceScope,
                                              out ResourceRequestHandle requestHandle,
                                              ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                              ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        return GetAssetAsync<Material>(resName, resourceScope, out requestHandle, loadType, loadMethod);
    }

    /// <summary>
    /// 异步获取指定音效，加载失败或请求取消时返回null
    /// </summary>
    /// <param name="resName">音频资源名</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="requestHandle">输出资源请求句柄</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>等待完成后返回音频资源的UniTask</returns>
    public UniTask<AudioClip> GetAudioClipAsync(string resName, ResourceScope resourceScope,
                                                out ResourceRequestHandle requestHandle,
                                                ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        return GetAssetAsync<AudioClip>(resName, resourceScope, out requestHandle, loadType, loadMethod);
    }

    /// <summary>
    /// 异步获取视频Clip，加载失败或请求取消时返回null
    /// </summary>
    /// <param name="videoName">视频资源名</param>
    /// <param name="resourceScope">资源计数释放和请求打断管理器</param>
    /// <param name="requestHandle">输出资源请求句柄</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>等待完成后返回视频资源的UniTask</returns>
    public UniTask<VideoClip> GetVideoClipAsync(string videoName, ResourceScope resourceScope,
                                                out ResourceRequestHandle requestHandle,
                                                ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
    {
        return GetAssetAsync<VideoClip>(videoName, resourceScope, out requestHandle, loadType, loadMethod);
    }
    #endregion

    #region 公共方法
    /// <summary>
    /// 根据加载方式创建同步或异步Asset请求
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="resName">资源名</param>
    /// <param name="assetLoader">输出资源加载器</param>
    /// <param name="completeHandler">加载完成回调</param>
    /// <param name="loadType">资源加载类型</param>
    /// <param name="loadMethod">底层资源加载方式</param>
    /// <returns>Asset请求句柄</returns>
    public AssetRequestHandle RequestAssetAsync<T>(string resName, out AssetLoader assetLoader,
                                                    Action<AssetLoader, AssetRequestHandle> completeHandler = null,
                                                    ResourceLoadType loadType = ResourceLoadType.NormalLoad,
                                                    ResourceLoadMethod loadMethod = ResourceLoadMethod.Sync)
                                                    where T : UnityEngine.Object
    {
        return loadMethod == ResourceLoadMethod.Sync
               ? ResourceModuleManager.Singleton.RequstAssetSync<T>(resName, out assetLoader, completeHandler, loadType)
               : ResourceModuleManager.Singleton.RequstAssetAsync<T>(resName, out assetLoader, completeHandler, loadType);
    }
    #endregion
}
