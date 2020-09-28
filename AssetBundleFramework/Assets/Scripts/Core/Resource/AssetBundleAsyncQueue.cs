/*
 * Description:             AssetBundleAsyncQueue.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/02
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetBundleAsyncQueue.cs
/// AB异步加载队列
/// 目的：
/// 优化原来的每一个异步AB加载都是一个携程的问题
/// 改成限定AB加载携程数量，模拟队列的形式进行AB异步加载
/// </summary>
public class AssetBundleAsyncQueue {

    /// <summary>
    /// 异步AB加载队列(全局唯一)
    /// </summary>
    public static Queue<AssetBundleLoader> ABAsyncQueue = new Queue<AssetBundleLoader>();

    /// <summary>
    /// 是否开启了AB加载任务携程
    /// </summary>
    public bool IsLoadStart
    {
        get;
        private set;
    }

    /// <summary>
    /// 当前正在加载的AB加载器
    /// </summary>
    public AssetBundleLoader CurrentLoadingAssetBundleLoader
    {
        get;
        private set;
    }
    
    public AssetBundleAsyncQueue()
    {
        IsLoadStart = false;
    }

    /// <summary>
    /// 启动AB异步加载任务携程
    /// </summary>
    public void startABAsyncLoad()
    {
        if(IsLoadStart == false)
        {
            CoroutineManager.Singleton.StartCoroutine(assetBundleLoadAsync());
            IsLoadStart = true;
        }
        else
        {
            ResourceLogger.logErr("AB异步加载任务携程已经启动！不能重复开启！");
        }
    }

    /// <summary>
    /// 异步加载任务入队列
    /// </summary>
    /// <param name="abl"></param>
    public static void enqueue(AssetBundleLoader abl)
    {
        if(abl.LoadMethod == ResourceLoadMethod.Async)
        {
            ABAsyncQueue.Enqueue(abl);
        }
        else
        {
            ResourceLogger.logErr(string.Format("严重错误，同步加载资源 : {0} 不应该添加到异步加载队列里！", abl.AssetBundleName));
        }
    }

    /// <summary>
    /// AB加载携程
    /// </summary>
    /// <returns></returns>
    private IEnumerator assetBundleLoadAsync()
    {
        while (true)
        {
            if (ABAsyncQueue.Count > 0)
            {
                CurrentLoadingAssetBundleLoader = ABAsyncQueue.Dequeue();
                //检查是否已经同步加载完成
                //如果异步加载AB时，同步请求来了，打断异步后续逻辑
                //LoadState == ResourceLoadState.None表明同步加载该资源已经完成，无需再异步返回
                if (CurrentLoadingAssetBundleLoader.LoadState == ResourceLoadState.None)
                {
                    ResourceLogger.logWar("有资源还未开始异步加载就被同步加载打断!");
                }
                else
                {
                    CurrentLoadingAssetBundleLoader.LoadState = ResourceLoadState.Loading;
                    var abname = CurrentLoadingAssetBundleLoader.AssetBundleName;
                    var abpath = AssetBundlePath.GetABLoadFullPath(abname);
                    AssetBundleCreateRequest abrequest = null;
#if UNITY_EDITOR
                    //因为资源不全，很多资源丢失，导致直接报错
                    //这里临时先在Editor模式下判定下文件是否存在，避免AssetBundle.LoadFromFileAsync()直接报错
                    if (System.IO.File.Exists(abpath))
                    {
                        Debug.Log(string.Format("开始异步加载AB : {0}！", CurrentLoadingAssetBundleLoader.AssetBundleName));
                        abrequest = AssetBundle.LoadFromFileAsync(abpath);
                    }
                    else
                    {
                        Debug.LogError(string.Format("AB : {0}文件不存在！", CurrentLoadingAssetBundleLoader.AssetBundleName));
                    }
#else
                    abrequest = AssetBundle.LoadFromFileAsync(abpath);
#endif
                    yield return abrequest;
                    Debug.Log(string.Format("等待异步加载AB : {0}！", abname));
                    //如果异步加载AB时，同步请求来了，打断异步后续逻辑
                    //LoadState == ResourceLoadState.None表明同步加载该资源已经完成，无需再异步返回
                    if (CurrentLoadingAssetBundleLoader.LoadState == ResourceLoadState.None)
                    {
                        ResourceLogger.log(string.Format("资源 : {0}加载已完成，异步加载被打断!", abname));
                    }
                    else
                    {
                        var assetbundle = abrequest.assetBundle;
                        if (assetbundle == null)
                        {
                            ResourceLogger.logErr(string.Format("Failed to load AssetBundle : {0}!", CurrentLoadingAssetBundleLoader.AssetBundleName));
                        }
                        CurrentLoadingAssetBundleLoader.onSelfABLoadComplete(assetbundle);
                    }
                }
                CurrentLoadingAssetBundleLoader = null;
            }
            else
            {
                yield return null;
            }
        }
    }
}