/*
 * Description:             GameSceneManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TResource;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameSceneManager.cs
/// 游戏场景管理单例类
/// </summary>
public class GameSceneManager : SingletonBase<GameSceneManager>
{
    /// <summary>
    /// 当前场景AB路径
    /// </summary>
    private string mCurrentSceneABPath;
    
    /// <summary>
    /// 资源计数释放+请求打断管理器
    /// </summary>
    private ResourceScope mResourceScope;

    public GameSceneManager()
    {
        mResourceScope = new ResourceScope();
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        // hook场景加载与切换回调
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    /// <summary>
    /// 释放
    /// </summary>
    public override void Shutdown()
    {
        base.Shutdown();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="sceneLoadProgressCb">场景加载进度回调</param>
    /// <param name="sceneLoadCompleteCb">场景加载完成回调</param>
    public void LoadSceneAsync(string sceneName, Action<float> sceneLoadProgressCb = null,
                              Action<AsyncOperation> sceneLoadCompleteCb = null)
    {
        Debug.Log($"开始异步加载场景:{sceneName}");
        BundleLoader bundleLoader;
        // 场景Asset比较特别，不是作为Asset加载，所以这里只加载所在AssetBundle
        var assetBundleRequestHandle = ResourceModuleManager.Singleton.RequstAssetABAsync(
        sceneName,
        out bundleLoader,
        (loader, assetBundleRequestHandle) =>
        {
            Debug.Log($"异步加载场景AB完成！");
            mResourceScope.RemoveRequest(assetBundleRequestHandle);
            // 非AB模式loader为null
            if(!assetBundleRequestHandle.IsSuccess)
            {
                Debug.LogError($"异步加载场景AB失败: {sceneName}");
                return;
            }
            // 非AB模式会返回null
            if(loader != null)
            {
                mResourceScope.GetAssetBundle(loader);
            }
            var coroutine = LoadSceneSyncCoroutine(sceneName, loader, sceneLoadProgressCb, sceneLoadCompleteCb);
            CoroutineManager.GetInstance().StartCoroutine(coroutine);
        },
        ResourceLoadType.NormalLoad);
        mResourceScope.RecordRequest(assetBundleRequestHandle);
    }

    /// <summary>
    /// 场景同步加载协程
    /// </summary>
    /// <param name="sceneName"></param>
    /// <param name="loader"></param>
    /// <param name="sceneLoadProgressCb"></param>
    /// <param name="sceneLoadCompleteCb"></param>
    /// <returns></returns>
    private IEnumerator LoadSceneSyncCoroutine(string sceneName, BundleLoader loader,
                                               Action<float> sceneLoadProgressCb = null,
                                               Action<AsyncOperation> sceneLoadCompleteCb = null)
    {
        Debug.Log($"开始加载场景:{sceneName}");
        sceneName = Path.GetFileNameWithoutExtension(sceneName);
        var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while(!asyncOperation.isDone)
        {
            var progress = asyncOperation.progress;
            Debug.Log($"场景:{sceneName}加载进度: {progress}");
            sceneLoadProgressCb?.Invoke(progress);
            yield return null;
        }
        Debug.Log($"场景:{sceneName}加载进度: {1}");
        Debug.Log($"场景:{sceneName}加载完成，开始释放之前场景资源");
        // 加载完成后再释放之前场景资源
        // 场景资源计数采用手动管理计数的方式
        // 切场景时手动计数减1
        // 加载时手动计数加1，不绑定对象
        // 减掉场景计数后，切换场景完成后再强制卸载所有不再使用的正常加载的Unsed资源(递归判定释放)
        ReleaseCurrentSceneRes();
        // 场景的计数是加载所在AB上的
        mCurrentSceneABPath = loader?.ResourcePath;
        sceneLoadProgressCb?.Invoke(1f);
        sceneLoadCompleteCb?.Invoke(asyncOperation);
        Debug.Log($"场景:{sceneName}加载结束！");
    }

    /// <summary>
    /// 释放当前场景资源
    /// </summary>
    private void ReleaseCurrentSceneRes()
    {
        if (!string.IsNullOrEmpty(mCurrentSceneABPath))
        {
            mResourceScope.ReleaseResource(mCurrentSceneABPath);
            mCurrentSceneABPath = null;
        }
    }

    /// <summary>
    /// 场景加载回调
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(string.Format("场景:{0}被加载!", scene.name));
        //新场景加载后DO Something
#if UNITY_EDITOR
        // var rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        // for (int i = 0, length = rootGameObjects.Length; i < length; i++)
        // {
        //     ResourceUtility.FindMeshRenderShaderBack(rootGameObjects[i]);
        // }
        // if(RenderSettings.skybox != null && RenderSettings.skybox.shader != null)
        // {
        //     RenderSettings.skybox.shader = Shader.Find(RenderSettings.skybox.shader.name);
        // }
#endif
        // 在新场景加载后再回收资源是为了避免不同场景引用相同资源导致频繁加载卸载
        ResourceModuleManager.Singleton.UnloadAllUnsedNormalLoadedResources();
    }

    /// <summary>
    /// 场景卸载回调
    /// </summary>
    /// <param name="scene"></param>
    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log(string.Format("场景:{0}被卸载!", scene.name));
        if (!scene.name.Equals("Preview Scene"))
        {
            // 场景卸载后做一些事
        }
    }
}
