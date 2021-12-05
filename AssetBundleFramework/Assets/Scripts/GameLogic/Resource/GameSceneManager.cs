/*
 * Description:             GameSceneManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameSceneManager.cs
/// 游戏场景管理单例类
/// </summary>
public class GameSceneManager : SingletonTemplate<GameSceneManager>
{
#if !NEW_RESOURCE
    /// <summary>
    /// 当前场景的AssetBundle信息
    /// </summary>
    private AbstractResourceInfo mCurrentSceneARI;
#else
    /// <summary>
    /// 当前场景的Asset加载器信息
    /// </summary>
    private TResource.BundleLoader mCurrentSceneAssetLoader;
#endif

    /// <summary>
    /// 初始化
    /// </summary>
    public void init()
    {
        // hook场景加载与切换回调
        SceneManager.sceneLoaded += onSceneLoaded;
        SceneManager.sceneUnloaded += onSceneUnloaded;
    }

#if !NEW_RESOURCE
    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="scenePath"></param>
    public void loadSceneSync(string scenePath)
    {
        // 预加载资源类型需要在切换场景前卸载掉，切换场景后可能有新的预加载资源加载进来
        ResourceModuleManager.Singleton.unloadAllUnsedPreloadLoadedResources();

        // 场景资源计数采用手动管理计数的方式
        // 切场景时手动计数减1
        // 加载时手动计数加1，不绑定对象
        if (mCurrentSceneARI != null)
        {
            mCurrentSceneARI.release();
            mCurrentSceneARI = null;
        }

        ResourceModuleManager.Singleton.requstResource(
        scenePath,
        (abi) =>
        {
            mCurrentSceneARI = abi;
            mCurrentSceneARI.retain();
            // 减掉场景计数后，切换场景前强制卸载所有不再使用的正常加载的Unsed资源(递归判定释放)
            // 在新场景加载后再回收资源是为了避免不同场景引用相同资源导致频繁加载卸载
            ResourceModuleManager.Singleton.unloadAllUnsedNormalLoadedResources();
        });
        var scenename = Path.GetFileNameWithoutExtension(scenePath);
        SceneManager.LoadScene(scenename);
    }

    /// <summary>
    /// 异步加载场景
    /// </summary>
    /// <param name="scenePath"></param>
    public void loadSceneAync(string scenePath)
    {
        // 预加载资源类型需要在切换场景前卸载掉，切换场景后可能有新的预加载资源加载进来
        ResourceModuleManager.Singleton.unloadAllUnsedPreloadLoadedResources();

        // 场景资源计数采用手动管理计数的方式
        // 切场景时手动计数减1
        // 加载时手动计数加1，不绑定对象
        if (mCurrentSceneARI != null)
        {
            mCurrentSceneARI.release();
            mCurrentSceneARI = null;
        }

        ResourceModuleManager.Singleton.requstResource(
        scenePath,
        (abi) =>
        {
            mCurrentSceneARI = abi;
            mCurrentSceneARI.retain();
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            SceneManager.LoadSceneAsync(sceneName);
            // 减掉场景计数后，切换场景前强制卸载所有不再使用的正常加载的Unsed资源(递归判定释放)
            // 在新场景加载后再回收资源是为了避免不同场景引用相同资源导致频繁加载卸载
            ResourceModuleManager.Singleton.unloadAllUnsedNormalLoadedResources();
        },
        ResourceLoadType.NormalLoad,
        ResourceLoadMethod.Async);
    }

    /// <summary>
    /// 场景加载回调
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(string.Format("场景:{0}被加载!", scene.name));
        //新场景加载后DO Something

    }

    /// <summary>
    /// 场景卸载回调
    /// </summary>
    /// <param name="scene"></param>
    private void onSceneUnloaded(Scene scene)
    {
        Debug.Log(string.Format("场景:{0}被卸载!", scene.name));
        if(!scene.name.Equals("Preview Scene"))
        {
            // 场景卸载后，递归释放所有不再使用的正常加载的资源
            // 确保所有上一个场景不再使用的正常加载AB资源正确释放
            ResourceModuleManager.Singleton.unloadAllUnsedNormalLoadedResources();
        }
    }
#else
    /// <summary>
    /// 同步加载场景
    /// </summary>
    /// <param name="scenePath"></param>
    public void loadSceneSync(string scenePath)
    {
        // 场景资源计数采用手动管理计数的方式
        // 切场景时手动计数减1
        // 加载时手动计数加1，不绑定对象
        // 减掉场景计数后，切换场景完成后再强制卸载所有不再使用的正常加载的Unsed资源(递归判定释放)
        if (mCurrentSceneAssetLoader != null)
        {
            mCurrentSceneAssetLoader.releaseAssetBundle();
            mCurrentSceneAssetLoader = null;
        }

        var sceneAssetBundlePath = scenePath.Replace(".unity", string.Empty);
        TResource.BundleLoader bundleLoader;
        // 场景Asset比较特别，不是作为Asset加载，所以这里只加载所在AssetBundle
        TResource.ResourceModuleManager.Singleton.requstAssetBundleSync(
        sceneAssetBundlePath,
        out bundleLoader,
        (loader, requestUid) =>
        {
            mCurrentSceneAssetLoader = loader;
            mCurrentSceneAssetLoader?.retainAssetBundle();
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            SceneManager.LoadScene(sceneName);
        },
        TResource.ResourceLoadType.NormalLoad);
    }

    /// <summary>
    /// 异步加载场景
    /// TODO:
    /// 异步加载完成回调
    /// </summary>
    /// <param name="scenePath"></param>
    public void loadSceneAsync(string scenePath)
    {
        // 场景资源计数采用手动管理计数的方式
        // 切场景时手动计数减1
        // 加载时手动计数加1，不绑定对象
        // 减掉场景计数后，切换场景完成后再强制卸载所有不再使用的正常加载的Unsed资源(递归判定释放)
        if (mCurrentSceneAssetLoader != null)
        {
            mCurrentSceneAssetLoader.releaseAssetBundle();
            mCurrentSceneAssetLoader = null;
        }

        var sceneAssetBundlePath = Path.GetPathRoot(scenePath);
        TResource.BundleLoader bundleLoader;
        // 场景Asset比较特别，不是作为Asset加载，所以这里只加载所在AssetBundle
        TResource.ResourceModuleManager.Singleton.requstAssetBundleAsync(
        sceneAssetBundlePath,
        out bundleLoader,
        (loader, requestUid) =>
        {
            mCurrentSceneAssetLoader = loader;
            mCurrentSceneAssetLoader.retainAssetBundle();
            var sceneName = Path.GetFileNameWithoutExtension(scenePath);
            SceneManager.LoadSceneAsync(sceneName);
        },
        TResource.ResourceLoadType.NormalLoad);
    }

    /// <summary>
    /// 场景加载回调
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void onSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log(string.Format("场景:{0}被加载!", scene.name));
        //新场景加载后DO Something
#if UNITY_EDITOR
        var rootGameObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0, length = rootGameObjects.Length; i < length; i++)
        {
            ResourceUtility.FindMeshRenderShaderBack(rootGameObjects[i]);
        }
        if(RenderSettings.skybox != null && RenderSettings.skybox.shader != null)
        {
            RenderSettings.skybox.shader = Shader.Find(RenderSettings.skybox.shader.name);
        }
#endif
        // 在新场景加载后再回收资源是为了避免不同场景引用相同资源导致频繁加载卸载
        TResource.ResourceModuleManager.Singleton.unloadAllUnsedNormalLoadedResources();
    }

    /// <summary>
    /// 场景卸载回调
    /// </summary>
    /// <param name="scene"></param>
    private void onSceneUnloaded(Scene scene)
    {
        Debug.Log(string.Format("场景:{0}被卸载!", scene.name));
        if (!scene.name.Equals("Preview Scene"))
        {
            // 场景卸载后做一些事
        }
    }
#endif
}