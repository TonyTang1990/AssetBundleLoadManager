/*
 * Description:             AssetDatabaseModule.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/07
 */

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// TODO:
// 支持Assetdabase模式正确的以来加载和卸载管理(仅在确保所有资源都正确标记AB名字时可用)

/// <summary>
/// AssetDatabaseModule.cs
/// AssetDatabase资源加载模块
/// </summary>
public class AssetDatabaseModule : AbstractResourceModule
{
    #region 资源加载管理部分
    /// <summary>
    /// 资源请求任务映射map
    /// Key为资源名，Value为资源加载任务对象
    /// </summary>
    public Dictionary<string, AssetDatabaseLoader> ResourceRequestTaskMap
    {
        get
        {
            return mResourceRequestTaskMap;
        }
    }
    private Dictionary<string, AssetDatabaseLoader> mResourceRequestTaskMap;

    /// <summary>
    /// 已加载资源里不再有有效引用的资源信息列表
    /// </summary>
    private List<AbstractResourceInfo> mUnsedResourceInfoList;

    /// <summary> 检测未使用资源时间间隔(在请求队列为空时才检测未使用资源) /// </summary>
    private float mCheckUnsedResourceTimeInterval;

    /// <summary> 检测未使用资源等待间隔 /// </summary>
    private WaitForSeconds mWaitForCheckUnsedResourceInterval;

    /// <summary>
    /// 单帧卸载的资源最大数量
    /// 避免单帧卸载过多AB导致卡顿
    /// </summary>
    private int mMaxUnloadResourceNumberPerFrame;

    /// <summary>
    /// 资源最短的有效生存时间
    /// 用于避免短时间内频繁删除卸载同一个AB的情况(比如同一个窗口AB资源不断重复打开关闭)
    /// </summary>
    private float mResourceMinimumLifeTime;

    /// <summary>
    /// 资源回收帧率门槛(避免帧率过低的时候回收AB造成过卡)
    /// </summary>
    private int mResourceRecycleFPSThreshold;
    #endregion

    /// <summary>
    /// 资源加载模块初始化
    /// </summary>
    public override void init()
    {
        base.init();
        ResLoadMode = ResourceLoadMode.AssetDatabase;

        mResourceRequestTaskMap = new Dictionary<string, AssetDatabaseLoader>();
        mUnsedResourceInfoList = new List<AbstractResourceInfo>();
        mCheckUnsedResourceTimeInterval = 5.0f;
        mWaitForCheckUnsedResourceInterval = new WaitForSeconds(mCheckUnsedResourceTimeInterval);
        mMaxUnloadResourceNumberPerFrame = 10;
        mResourceMinimumLifeTime = 20.0f;
        mResourceRecycleFPSThreshold = 20;

        AssetDatabaseLoaderFactory.initialize(20);             // 考虑到大部分都是采用同步加载，所以AssetDatabaseLoader并不需要初始化太多
        AssetDatabaseInfoFactory.initialize(200);
    }

    /// <summary>
    /// 真正的请求资源
    /// </summary>
    /// <param name="respath">资源AB路径</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    protected override void realRequestResource(string respath, LoadResourceCompleteHandler completehandler, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        // 如果资源已经加载完成，直接返回
        if (mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad].ContainsKey(respath))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad][respath]);
            if (loadtype > ResourceLoadType.NormalLoad)
            {
                updateLoadedResourceInfoLoadType(respath, ResourceLoadType.NormalLoad, loadtype);
            }
        }
        else if (mAllLoadedResourceInfoMap[ResourceLoadType.Preload].ContainsKey(respath))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.Preload][respath]);
            if (loadtype > ResourceLoadType.Preload)
            {
                updateLoadedResourceInfoLoadType(respath, ResourceLoadType.Preload, loadtype);
            }
        }
        else if (mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad].ContainsKey(respath))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad][respath]);
        }
        else
        {
            AssetDatabaseLoader adloader = createADLoader(respath);
            //暂时默认都当同步加载，不支持异步模拟
            adloader.LoadMethod = loadmethod;
            adloader.LoadType = loadtype;
            adloader.LoadResourceCompleteCallBack = completehandler;
            adloader.LoadSelfResourceCompleteNotifier = onResourceLoadCompleteNotifier;
            mResourceRequestTaskMap.Add(respath, adloader);
            adloader.startLoad();
        }
    }

    /// <summary>
    /// 更新入口
    /// </summary>
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// 真正执行递归卸载指定类型不再使用的资源接口
    /// </summary>
    /// <param name="resourceloadtype">资源加载类型</param>
    protected override void doUnloadSpecificLoadTypeUnsedResource(ResourceLoadType resourceloadtype)
    {
        // Assetdatabase模式下为了开发方便没有正确添加依赖计数，
        // 所以AssetDatabase不允许资源回收，避免错误的资源回收
        if (ResourceModuleManager.Singleton.ResLoadMode != ResourceLoadMode.AssetDatabase)
        {
            // 递归判定卸载所有不再可用的正常加载资源
            bool iscomplete = false;
            bool hasunsedres = false;
            while (!iscomplete)
            {
                // 检查回收不再使用正常已加载的AB
                foreach (var loadedab in mAllLoadedResourceInfoMap[resourceloadtype])
                {
                    if (loadedab.Value.IsUnsed)
                    {
                        mUnsedResourceInfoList.Add(loadedab.Value);
                    }
                }

                if (mUnsedResourceInfoList.Count == 0)
                {
                    //不再有可卸载的资源
                    iscomplete = true;
                }
                else
                {
                    hasunsedres = true;
                    // 有可卸载的组他吧
                    for (int i = 0; i < mUnsedResourceInfoList.Count; i++)
                    {
                        mAllLoadedResourceInfoMap[resourceloadtype].Remove(mUnsedResourceInfoList[i].ResourcePath);
                        mUnsedResourceInfoList[i].dispose();
                    }
                    mUnsedResourceInfoList.Clear();
                }
            }

            if (iscomplete && hasunsedres)
            {
                //Resources.UnloadAsset()只是标记该资源能被卸载,并不能真正卸载资源
                //同时AssetDatabase模式下，没有依赖资源的概念，
                //为了确保不再使用的资源被正确卸载(模拟AssetBundle模式的加载管理环境)
                //通过调用Resources.UnloadUnusedAssets()方法来测试卸载资源
                Resources.UnloadUnusedAssets();
            }
        }
    }

    /// <summary>
    /// 创建AssetDatabase资源加载对象
    /// </summary>
    /// <param name="resPath">资源路径</param>
    /// <returns></returns>
    private AssetDatabaseLoader createADLoader(string resPath)
    {
        var adl = AssetDatabaseLoaderFactory.create();
        adl.AssetPath = resPath;
        return adl;
    }

    /// <summary>
    /// 队列里不再有资源需要加载时检查不再使用的资源
    /// </summary>
    protected override IEnumerator checkUnsedResource()
    {
        while (true)
        {
            // Assetdatabase模式下为了开发方便没有正确添加依赖计数，
            // 所以AssetDatabase不允许资源回收，避免错误的资源回收
            if (ResourceModuleManager.Singleton.ResLoadMode != ResourceLoadMode.AssetDatabase)
            {
                if (EnableResourceRecyclingUnloadUnsed && mCurrentFPS >= mResourceRecycleFPSThreshold && mResourceRequestTaskMap.Count == 0)
                {
                    float time = Time.time;
                    // 检查正常加载的资源，回收不再使用的资源
                    foreach (var loadedres in mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad])
                    {
                        if (loadedres.Value.IsUnsed)
                        {
                            if ((time - loadedres.Value.LastUsedTime) > mResourceMinimumLifeTime)
                            {
                                mUnsedResourceInfoList.Add(loadedres.Value);
                            }
                        }
                    }

                    if (mUnsedResourceInfoList.Count > 0)
                    {
                        // 根据最近使用时间升序排列
                        mUnsedResourceInfoList.Sort(ADILastUsedTimeSort);

                        for (int i = 0; i < mUnsedResourceInfoList.Count; i++)
                        {
                            if (i < mMaxUnloadResourceNumberPerFrame)
                            {
                                mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad].Remove(mUnsedResourceInfoList[i].ResourcePath);
                                mUnsedResourceInfoList[i].dispose();
                            }
                            else
                            {
                                break;
                            }
                        }
                        mUnsedResourceInfoList.Clear();
                        //Resources.UnloadAsset()只是标记该资源能被卸载,并不能真正卸载资源
                        //同时AssetDatabase模式下，没有依赖资源的概念，
                        //为了确保不再使用的资源被正确卸载(模拟AssetBundle模式的加载管理环境)
                        //通过调用Resources.UnloadUnusedAssets()方法来测试卸载资源
                        Resources.UnloadUnusedAssets();
                    }
                }
            }
            yield return mWaitForCheckUnsedResourceInterval;
        }
    }

    /// <summary> 资源加载完成通知(用于更新资源加载管理) /// </summary>
    /// <param name="adl">资源加载任务信息</param>
    private void onResourceLoadCompleteNotifier(AssetDatabaseLoader adl)
    {
        var assetPath = adl.AssetPath;
        if (mResourceRequestTaskMap.ContainsKey(assetPath))
        {
            mResourceRequestTaskMap.Remove(assetPath);
            mAllLoadedResourceInfoMap[adl.LoadType].Add(assetPath, adl.ResourceInfo);
            //资源加载数据统计
            if (ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch)
            {
                ResourceLoadAnalyse.Singleton.addResourceLoadedTime(assetPath);
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("收到不在加载任务请求队列里的Asset:{0}加载完成回调!", assetPath));
        }
    }

    /// <summary>
    /// AassetDatabase信息根据最近使用时间排序
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int ADILastUsedTimeSort(AbstractResourceInfo a, AbstractResourceInfo b)
    {
        return a.LastUsedTime.CompareTo(b.LastUsedTime);
    }
}
#endif