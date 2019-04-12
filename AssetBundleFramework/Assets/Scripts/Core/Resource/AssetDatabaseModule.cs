/*
 * Description:             AssetDatabaseModule.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/07
 */

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// 所有已加载的资源的信息映射Map
    /// Key为AB加载类型，Value为该加载类型已经加载的资源信息映射Map(Key为AB名字，Value为资源加载信息)
    /// </summary>
    private Dictionary<ResourceLoadType, Dictionary<string, AssetDatabaseInfo>> mAllLoadedResourceInfoMap;

    /// <summary>
    /// 已加载资源里不再有有效引用的资源信息列表
    /// </summary>
    private List<AssetDatabaseInfo> mUnsedResourceInfoList;

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

    /// <summary>
    /// 资源白名单映射Map(用Dictionary只是为了快速访问)
    /// Key为资源名，Value也为资源名
    /// 在白名单里的资源，只要上层加载该资源，一律以PreLoad形式加载资源
    /// 这里主要是为了避免因为上层逻辑代码导致错误的资源被频繁加载卸载
    /// 比如: Res.cs    public bool SetImageSprite(UnityEngine.UI.Image image, string spName)
    /// </summary>
    private Dictionary<string, string> mResourceWhileListMap;
    #endregion

    /// <summary>
    /// 资源加载模块初始化
    /// </summary>
    public override void init()
    {
        ResLoadMode = ResourceLoadMode.AssetDatabase;
        EnableResourceRecyclingUnloadUnsed = true;

        mResourceRequestTaskMap = new Dictionary<string, AssetDatabaseLoader>();
        mAllLoadedResourceInfoMap = new Dictionary<ResourceLoadType, Dictionary<string, AssetDatabaseInfo>>();
        mAllLoadedResourceInfoMap.Add(ResourceLoadType.NormalLoad, new Dictionary<string, AssetDatabaseInfo>());
        mAllLoadedResourceInfoMap.Add(ResourceLoadType.Preload, new Dictionary<string, AssetDatabaseInfo>());
        mAllLoadedResourceInfoMap.Add(ResourceLoadType.PermanentLoad, new Dictionary<string, AssetDatabaseInfo>());
        mUnsedResourceInfoList = new List<AssetDatabaseInfo>();
        mCheckUnsedResourceTimeInterval = 5.0f;
        mWaitForCheckUnsedResourceInterval = new WaitForSeconds(mCheckUnsedResourceTimeInterval);
        mMaxUnloadResourceNumberPerFrame = 10;
        mResourceMinimumLifeTime = 20.0f;
        mResourceRecycleFPSThreshold = 20;
        mResourceWhileListMap = new Dictionary<string, string>(); ;

        AssetDatabaseLoaderFactory.initialize(20);             // 考虑到大部分都是采用同步加载，所以AssetDatabaseLoader并不需要初始化太多
        AssetDatabaseInfoFactory.initialize(200);
    }

    /// <summary>
    /// 开启资源不再使用回收检测
    /// </summary>
    public override void startResourceRecyclingTask()
    {
        CoroutineManager.Singleton.startCoroutine(checkUnsedResource());
    }

    /// <summary>
    /// 添加指定资源到白名单
    /// </summary>
    /// <param name="resname">资源名(既AB名)</param>
    public override void addToWhiteList(string resname)
    {
        if (!mResourceWhileListMap.ContainsKey(resname))
        {
            mResourceWhileListMap.Add(resname, resname);
        }
        else
        {
            ResourceLogger.logErr(string.Format("资源 : {0}重复添加白名单!", resname));
        }
    }

    /// <summary>
    /// 请求资源
    /// 资源加载统一入口
    /// </summary>
    /// <param name="resname">资源AB名</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public override void requstResource(string resname, LoadResourceCompleteHandler completehandler, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        // 在白名单里的资源一律以预加载形式加载，
        // 避免因为上层逻辑错误加载后被频繁加载卸载
        if (mResourceWhileListMap.ContainsKey(resname))
        {
            loadtype = ResourceLoadType.Preload;
        }

        // 如果资源已经加载完成，直接返回
        if (mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad].ContainsKey(resname))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad][resname]);
            if (loadtype > ResourceLoadType.NormalLoad)
            {
                updateLoadedResourceILoadType(resname, ResourceLoadType.NormalLoad, loadtype);
            }
        }
        else if (mAllLoadedResourceInfoMap[ResourceLoadType.Preload].ContainsKey(resname))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.Preload][resname]);
            if (loadtype > ResourceLoadType.Preload)
            {
                updateLoadedResourceILoadType(resname, ResourceLoadType.Preload, loadtype);
            }
        }
        else if (mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad].ContainsKey(resname))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad][resname]);
        }
        else
        {
            AssetDatabaseLoader adloader = createADLoader(resname);
            //暂时默认都当同步加载，不支持异步模拟
            adloader.LoadMethod = loadmethod;
            adloader.LoadType = loadtype;
            adloader.LoadResourceCompleteCallBack = completehandler;
            adloader.LoadSelfResourceCompleteNotifier = onResourceLoadCompleteNotifier;
            mResourceRequestTaskMap.Add(resname, adloader);
            adloader.startLoad();
            /*
            // 确保同一个资源加载的Loader是同一个
            // 保证同一个资源加载完成时上层所有加载该资源的回调正确
            AssetBundleLoader abloader = null;
            if (mABRequestTaskMap.ContainsKey(resname))
            {
                abloader = mABRequestTaskMap[resname];
                // 之前有请求resname资源，但还未完成
                // 比如先异步请求resname，在异步完成前来了一个同步请求resname
                // 修改加载方式并添加回调，调用同步加载方式，异步加载会在同步加载完成时一起回调
                abloader.LoadMethod = loadmethod;
                abloader.LoadType = loadtype;
                abloader.LoadABCompleteCallBack += completehandler;
                abloader.LoadSelfABCompleteNotifier = onABLoadCompleteNotifier;
                if (loadmethod == ResourceLoadMethod.Sync)
                {
                    ResourceLogger.log(string.Format("请求同步加载一个正在异步加载的资源 : {0}", abloader.AssetBundleName));
                    //重置AB加载状态，走同步加载模式
                    abloader.LoadState = ResourceLoadState.None;
                    abloader.startLoad();
                }
            }
            else
            {
                abloader = createABLoader(resname);
                abloader.LoadMethod = loadmethod;
                abloader.LoadType = loadtype;
                abloader.LoadABCompleteCallBack = completehandler;
                abloader.LoadSelfABCompleteNotifier = onABLoadCompleteNotifier;
                mABRequestTaskMap.Add(resname, abloader);
                abloader.startLoad();
            }
            */
        }

    }

    /// <summary>
    /// 更新入口
    /// </summary>
    public override void Update()
    {

    }


    /// <summary>
    /// 释放可释放的预加载资源(递归判定，不限制回收数量)
    /// Note:
    /// 切场景前调用，确保所有预加载资源正确释放
    /// </summary>
    public override void unloadAllUnsedPreloadLoadedResources()
    {
        unloadSpecificLoadTypeUnsedResourceRecursively(ResourceLoadType.Preload);
    }

    /// <summary>
    /// 提供给外部的触发卸载所有正常加载不再使用的资源(递归判定，不限制回收数量)
    /// Note:
    /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
    /// </summary>
    public override void unloadAllUnsedNormalLoadedResources()
    {
        unloadSpecificLoadTypeUnsedResourceRecursively(ResourceLoadType.NormalLoad);
    }

    /// <summary>
    /// 递归卸载指定类型不再使用的资源(Note:不支持卸载常驻资源类型)
    /// </summary>
    /// <param name="resourceloadtype">AB资源加载类型</param>
    private void unloadSpecificLoadTypeUnsedResourceRecursively(ResourceLoadType resourceloadtype)
    {
        if (resourceloadtype == ResourceLoadType.PermanentLoad)
        {
            ResourceLogger.logErr("不允许卸载常驻AB资源!");
            return;
        }

        // 递归判定卸载所有不再可用的正常加载资源
        bool hasunsedres = true;
        while (hasunsedres)
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
                hasunsedres = false;
            }
            else
            {
                // 有可卸载的组他吧
                for (int i = 0; i < mUnsedResourceInfoList.Count; i++)
                {
                    mAllLoadedResourceInfoMap[resourceloadtype].Remove(mUnsedResourceInfoList[i].AssetBundleName);
                    mUnsedResourceInfoList[i].dispose();
                }
                mUnsedResourceInfoList.Clear();
            }
        }
    }

    /// <summary>
    /// 打印当前资源所有使用者信息以及索引计数(开发用)
    /// </summary>
    public override void printAllLoadedResourceOwnersAndRefCount()
    {
        ResourceLogger.log("Normal Loaded AssetDatabase Info:");
        foreach (var abi in mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad])
        {
            abi.Value.printAllOwnersNameAndRefCount();
        }

        ResourceLogger.log("Preload Loaded AssetDatabase Info:");
        foreach (var abi in mAllLoadedResourceInfoMap[ResourceLoadType.Preload])
        {
            abi.Value.printAllOwnersNameAndRefCount();
        }

        ResourceLogger.log("Permanent Loaded AssetDatabase Info:");
        foreach (var abi in mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad])
        {
            abi.Value.printAllOwnersNameAndRefCount();
        }
    }

    /// <summary>
    /// 创建AssetDatabase资源加载对象
    /// </summary>
    /// <param name="resname">资源名</param>
    /// <returns></returns>
    private AssetDatabaseLoader createADLoader(string resname)
    {
        var adl = AssetDatabaseLoaderFactory.create();
        adl.AssetBundleName = resname;
        return adl;
    }

    /// <summary>
    /// 队列里不再有资源需要加载时检查不再使用的资源
    /// </summary>
    private IEnumerator checkUnsedResource()
    {
        while (true)
        {
            if (EnableResourceRecyclingUnloadUnsed && mResourceRequestTaskMap.Count == 0)
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
                            mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad].Remove(mUnsedResourceInfoList[i].AssetBundleName);
                            mUnsedResourceInfoList[i].dispose();
                        }
                        else
                        {
                            break;
                        }
                    }
                    mUnsedResourceInfoList.Clear();
                }
            }
            yield return mWaitForCheckUnsedResourceInterval;
        }
    }

    /// <summary> 资源加载完成通知(用于更新资源加载管理) /// </summary>
    /// <param name="adl">资源加载任务信息</param>
    private void onResourceLoadCompleteNotifier(AssetDatabaseLoader adl)
    {
        var abname = adl.AssetBundleName;
        if (mResourceRequestTaskMap.ContainsKey(abname))
        {
            mResourceRequestTaskMap.Remove(abname);
            mAllLoadedResourceInfoMap[adl.LoadType].Add(abname, adl.ResourceInfo);
            //资源加载数据统计
            if (ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch)
            {
                ResourceLoadAnalyse.Singleton.addResourceLoadedTime(abname);
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("收到不在加载任务请求队列里的AB:{0}加载完成回调!", abname));
        }
    }

    /// <summary>
    /// AassetDatabase信息根据最近使用时间排序
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int ADILastUsedTimeSort(AssetDatabaseInfo a, AssetDatabaseInfo b)
    {
        return a.LastUsedTime.CompareTo(b.LastUsedTime);
    }

    /// <summary>
    /// 更新已加载资源的加载类型
    /// </summary>
    /// <param name="resname">资源名</param>
    /// <param name="oldloadtype">旧的加载类型</param>
    /// <param name="newloadtype">新的家在类型</param>
    private void updateLoadedResourceILoadType(string resname, ResourceLoadType oldloadtype, ResourceLoadType newloadtype)
    {
        if (mAllLoadedResourceInfoMap[oldloadtype].ContainsKey(resname))
        {
            var abi = mAllLoadedResourceInfoMap[oldloadtype][resname];
            mAllLoadedResourceInfoMap[newloadtype].Add(resname, abi);
            mAllLoadedResourceInfoMap[oldloadtype].Remove(resname);
            ResourceLogger.log(string.Format("已加载的资源 : {0}从资源类型 : {1}更新到资源类型 : {2}！", resname, oldloadtype, newloadtype));
        }
        else
        {
            ResourceLogger.logErr(string.Format("资源类型 : {0}里找不到已加载的资源 : {1}，无法更新该资源的加载类型！", oldloadtype, resname));
        }
    }
}
#endif