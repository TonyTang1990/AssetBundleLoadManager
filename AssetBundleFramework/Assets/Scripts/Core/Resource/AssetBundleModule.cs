/*
 * Description:             AssetBundleModule.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/07
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AssetBundleModule.cs
/// AssetBundle资源加载模块
/// </summary>
public class AssetBundleModule : AbstractResourceModule
{
    #region 资源加载管理部分
    /// <summary>
    /// AB资源请求任务映射map
    /// Key为资源名，Value为资源加载任务对象
    /// </summary>
    public Dictionary<string, AssetBundleLoader> ABRequestTaskMap
    {
        get
        {
            return mABRequestTaskMap;
        }
    }
    private Dictionary<string, AssetBundleLoader> mABRequestTaskMap;

    /// <summary>
    /// 所有已加载的AB的信息映射Map
    /// Key为AB加载类型，Value为该AB加载类型已经加载的AB资源信息映射Map(Key为AB名字，Value为AB加载信息)
    /// </summary>
    private Dictionary<ResourceLoadType, Dictionary<string, AssetBundleInfo>> mAllLoadedABInfoMap;

    /// <summary>
    /// 已加载AB里不再有有效引用的AB信息列表
    /// </summary>
    private List<AssetBundleInfo> mUnsedABInfoList;

    /// <summary> 检测未使用AB时间间隔(在请求队列为空时才检测未使用AB) /// </summary>
    private float mCheckUnsedABTimeInterval;

    /// <summary> 检测未使用AB等待间隔 /// </summary>
    private WaitForSeconds mWaitForCheckUnsedABInterval;

    /// <summary>
    /// 单帧卸载的AB最大数量
    /// 避免单帧卸载过多AB导致卡顿
    /// </summary>
    private int mMaxUnloadABNumberPerFrame;

    /// <summary>
    /// AB最短的有效生存时间
    /// 用于避免短时间内频繁删除卸载同一个AB的情况(比如同一个窗口AB资源不断重复打开关闭)
    /// </summary>
    private float mABMinimumLifeTime;

    /// <summary>
    /// AB回收帧率门槛(避免帧率过低的时候回收AB造成过卡)
    /// </summary>
    private int mABRecycleFPSThreshold;

    /// <summary>
    /// 资源白名单映射Map(用Dictionary只是为了快速访问)
    /// Key为资源名，Value也为资源名
    /// 在白名单里的资源，只要上层加载该资源，一律以PreLoad形式加载资源
    /// 这里主要是为了避免因为上层逻辑代码导致错误的资源被频繁加载卸载
    /// 比如: Res.cs    public bool SetImageSprite(UnityEngine.UI.Image image, string spName)
    /// </summary>
    private Dictionary<string, string> mABWhileListMap;
    #endregion

    #region AB异步加载队列部分
    /// <summary>
    /// 最大AB异步加载携程数量
    /// </summary>
    public int MaxMaximumAsyncCoroutine
    {
        get;
        private set;
    }

    /// <summary>
    /// AB异步加载队列列表
    /// </summary>
    public List<AssetBundleAsyncQueue> AssetBundleAsyncQueueList
    {
        get;
        private set;
    }
    #endregion

    #region AB依赖信息部分

    /// <summary>
    /// AB依赖信息映射map
    /// Key为AB名字，Value为该AB依赖的AB信息
    /// </summary>
    public Dictionary<string, string[]> AssetBundleDpMap
    {
        get
        {
            return mAssetBundleDpMap;
        }
    }
    private Dictionary<string, string[]> mAssetBundleDpMap;

    /// <summary>
    /// 加载所有AB依赖信息
    /// </summary>
    /// <returns></returns>
    public void loadAllDpInfo()
    {
        if (mAssetBundleDpMap.Count > 0)
        {
            //强制卸载一次老的，确保加载的是最新的依赖信息
            forceUnloadSpecificResource(AssetBundlePath.DependencyFileName);
            Debug.Log("重新加载依赖文件！");
        }
        // 依赖AB加载完即可卸载，所以作为NormalLoad方式加载
        requstResource(AssetBundlePath.DependencyFileName, onLoadAllDpInfoComplete, ResourceLoadType.NormalLoad, ResourceLoadMethod.Sync);
    }

    /// <summary>
    /// 加载AB依赖文件信息完成回调
    /// </summary>
    /// <param name="ari"></param>
    private void onLoadAllDpInfoComplete(AbstractResourceInfo ari)
    {
        mAssetBundleDpMap.Clear();
        var abi = ari as AssetBundleInfo;
        if (abi.Bundle == null)
        {
            Debug.LogError("找不到ab依赖配置文件");
            return;
        }
        //修改成读AssetBundleManifest方式
        AssetBundleManifest manifest = abi.loadAsset<AssetBundleManifest>(AssetBundlePath.DependencyAssetName);
        if (manifest == null)
        {
            Debug.Log(string.Format("Failed to load {0}.manifest!", AssetBundlePath.DependencyAssetName));
            return;
        }

        var allabnames = manifest.GetAllAssetBundles();

        foreach (var abname in allabnames)
        {
            var abdeparray = manifest.GetAllDependencies(abname);
            mAssetBundleDpMap[abname] = abdeparray;
        }
        Debug.Log("AB依赖文件信息加载成功!");
    }

    /// <summary>
    /// 获取AssetBundle锁依赖的AB信息
    /// </summary>
    /// <param name="abname"></param>
    /// <returns></returns>
    private string[] getAssetBundleDpInfo(string abname)
    {
        if (mAssetBundleDpMap.ContainsKey(abname))
        {
            return mAssetBundleDpMap[abname];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 打印所有AB依赖信息
    /// </summary>
    public void printAllResourceDpInfo()
    {
        foreach (var abinfo in mAssetBundleDpMap)
        {
            ResourceLogger.log(string.Format("AB Name:{0}", abinfo.Key));
            foreach (var dpfile in abinfo.Value)
            {
                ResourceLogger.log(string.Format("       DP AB Name:{0}", dpfile));
            }
        }
    }
    #endregion

    #region FSP计算部分

    /// <summary>
    /// 当前FPS
    /// </summary>
    public int CurrentFPS
    {
        get
        {
            return mCurrentFPS;
        }
    }
    private int mCurrentFPS;

    /// <summary>
    /// 经历的时间
    /// </summary>
    private float mTotalDeltaTime;

    /// <summary>
    /// 经历的帧数
    /// </summary>
    private int mFrameCount;

    /// <summary>
    /// FPS更新间隔频率
    /// </summary>
    private float mFPSUpdateInterval;
    #endregion

    /// <summary>
    /// 资源加载模块初始化
    /// </summary>
    public override void init()
    {
        // 初始化ABLoader和ABInfo可重用的基础数量
        AssetBundleLoaderFactory.initialize(20);             // 考虑到大部分都是采用同步加载，所以AssetBundleLoader并不需要初始化太多
        AssetBundleInfoFactory.initialize(200);
        mAssetBundleDpMap = new Dictionary<string, string[]>();

        ResLoadMode = ResourceLoadMode.AssetBundle;
        EnableResourceRecyclingUnloadUnsed = true;

        mABRequestTaskMap = new Dictionary<string, AssetBundleLoader>();
        mAllLoadedABInfoMap = new Dictionary<ResourceLoadType, Dictionary<string, AssetBundleInfo>>(new ResourceLoadTypeComparer());
        mAllLoadedABInfoMap.Add(ResourceLoadType.NormalLoad, new Dictionary<string, AssetBundleInfo>());
        mAllLoadedABInfoMap.Add(ResourceLoadType.Preload, new Dictionary<string, AssetBundleInfo>());
        mAllLoadedABInfoMap.Add(ResourceLoadType.PermanentLoad, new Dictionary<string, AssetBundleInfo>());
        mUnsedABInfoList = new List<AssetBundleInfo>();
        mABWhileListMap = new Dictionary<string, string>();

        mFPSUpdateInterval = 1.0f;

        // TODO: 根据设备设定相关参数，改成读表控制
        mCheckUnsedABTimeInterval = 5.0f;
        mWaitForCheckUnsedABInterval = new WaitForSeconds(mCheckUnsedABTimeInterval);
        mMaxUnloadABNumberPerFrame = 10;
        mABMinimumLifeTime = 20.0f;
        mABRecycleFPSThreshold = 20;

        MaxMaximumAsyncCoroutine = 2;
        AssetBundleAsyncQueueList = new List<AssetBundleAsyncQueue>();
        for (int i = 0; i < MaxMaximumAsyncCoroutine; i++)
        {
            var abaq = new AssetBundleAsyncQueue();
            AssetBundleAsyncQueueList.Add(abaq);
            abaq.startABAsyncLoad();
        }
        loadAllDpInfo();
    }

    /// <summary>
    /// 开启资源不再使用回收检测
    /// </summary>
    public override void startResourceRecyclingTask()
    {
        CoroutineManager.Singleton.startCoroutine(checkUnsedAssetBundle());
    }

    /// <summary>
    /// 添加指定资源到白名单
    /// </summary>
    /// <param name="resname">资源名(既AB名)</param>
    public override void addToWhiteList(string resname)
    {
        if (!mABWhileListMap.ContainsKey(resname))
        {
            mABWhileListMap.Add(resname, resname);
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
        if (mABWhileListMap.ContainsKey(resname))
        {
            loadtype = ResourceLoadType.Preload;
        }

        // 如果资源已经加载完成，直接返回
        if (mAllLoadedABInfoMap[ResourceLoadType.NormalLoad].ContainsKey(resname))
        {
            completehandler(mAllLoadedABInfoMap[ResourceLoadType.NormalLoad][resname]);
            if (loadtype > ResourceLoadType.NormalLoad)
            {
                updateLoadedABILoadType(resname, ResourceLoadType.NormalLoad, loadtype);
            }
        }
        else if (mAllLoadedABInfoMap[ResourceLoadType.Preload].ContainsKey(resname))
        {
            completehandler(mAllLoadedABInfoMap[ResourceLoadType.Preload][resname]);
            if (loadtype > ResourceLoadType.Preload)
            {
                updateLoadedABILoadType(resname, ResourceLoadType.Preload, loadtype);
            }
        }
        else if (mAllLoadedABInfoMap[ResourceLoadType.PermanentLoad].ContainsKey(resname))
        {
            completehandler(mAllLoadedABInfoMap[ResourceLoadType.PermanentLoad][resname]);
        }
        else
        {
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
        }
    }

    /// <summary>
    /// 更新入口
    /// </summary>
    public override void Update()
    {
        mTotalDeltaTime += Time.deltaTime;
        mFrameCount++;
        if (mTotalDeltaTime >= mFPSUpdateInterval)
        {
            mCurrentFPS = (int)(mFrameCount / mTotalDeltaTime);
            mTotalDeltaTime = 0f;
            mFrameCount = 0;
        }
    }

    /// <summary>
    /// 创建AB资源加载对象
    /// </summary>
    /// <param name="resname">资源名</param>
    /// <returns></returns>
    private AssetBundleLoader createABLoader(string resname)
    {
        var depabnames = getAssetBundleDpInfo(resname);
        var loader = AssetBundleLoaderFactory.create();
        loader.AssetBundleName = resname;
        loader.DepABNames = depabnames;
        return loader;
    }

    /// <summary>
    /// 释放可释放的预加载资源AB(递归判定，不限制回收数量)
    /// Note:
    /// 切场景前调用，确保所有预加载资源正确释放
    /// </summary>
    public override void unloadAllUnsedPreloadLoadedResources()
    {
        unloadSpecificLoadTypeUnsedABRecursively(ResourceLoadType.Preload);
    }

    /// <summary>
    /// 提供给外部的触发卸载所有正常加载不再使用的资源AB(递归判定，不限制回收数量)
    /// Note:
    /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
    /// </summary>
    public override void unloadAllUnsedNormalLoadedResources()
    {
        unloadSpecificLoadTypeUnsedABRecursively(ResourceLoadType.NormalLoad);
    }

    /// <summary>
    /// 获取指定加载类型的已加载AB信息映射Map
    /// </summary>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public Dictionary<string, AssetBundleInfo> getSpecificLoadTypeABIMap(ResourceLoadType loadtype)
    {
        if (mAllLoadedABInfoMap.ContainsKey(loadtype))
        {
            return mAllLoadedABInfoMap[loadtype];
        }
        else
        {
            ResourceLogger.logErr(string.Format("找不到资源类型 : {0}的已加载AB信息!", loadtype));
            return null;
        }
    }

    #region Debug
    /// <summary>
    /// 获取正常已加载不可用的AB数量(不包含常驻AB)
    /// </summary>
    public int getNormalUnsedABNumber()
    {
        var unsednumber = 0;
        // 检查回收不再使用的AB
        foreach (var loadedab in mAllLoadedABInfoMap[ResourceLoadType.NormalLoad])
        {
            if (loadedab.Value.IsUnsed)
            {
                unsednumber++;
            }
        }
        return unsednumber;
    }

    /// <summary>
    /// 获取预加载已加载不可用的AB数量(不包含常驻AB)
    /// </summary>
    public int getPreloadUnsedABNumber()
    {
        var unsednumber = 0;
        // 检查回收不再使用的AB
        foreach (var loadedab in mAllLoadedABInfoMap[ResourceLoadType.Preload])
        {
            if (loadedab.Value.IsUnsed)
            {
                unsednumber++;
            }
        }
        return unsednumber;
    }

    /// <summary>
    /// 打印当前AB所有使用者信息以及索引计数(开发用)
    /// </summary>
    public void printAllLoadedABOwnersAndRefCount()
    {
        ResourceLogger.log("Normal Loaded AB Info:");
        foreach (var abi in mAllLoadedABInfoMap[ResourceLoadType.NormalLoad])
        {
            abi.Value.printAllOwnersNameAndRefCount();
        }

        ResourceLogger.log("Preload Loaded AB Info:");
        foreach (var abi in mAllLoadedABInfoMap[ResourceLoadType.Preload])
        {
            abi.Value.printAllOwnersNameAndRefCount();
        }

        ResourceLogger.log("Permanent Loaded AB Info:");
        foreach (var abi in mAllLoadedABInfoMap[ResourceLoadType.PermanentLoad])
        {
            abi.Value.printAllOwnersNameAndRefCount();
        }
    }
    #endregion

    /// <summary>
    /// 创建AssetBundleInfo对象信息
    /// </summary>
    /// <param name="abname"></param>
    /// <param name="ab"></param>
    /// <returns></returns>
    public static AssetBundleInfo createAssetBundleInfo(string abname, AssetBundle ab)
    {
        var abi = AssetBundleInfoFactory.create();
        abi.AssetBundleName = abname;
        abi.Bundle = ab;
        abi.onUnloadedCallback = onABUnloaded;
        return abi;
    }

    /// <summary>
    /// 对应AB卸载回调
    /// </summary>
    /// <param name="abi"></param>
    private static void onABUnloaded(AssetBundleInfo abi)
    {
        //AB卸载数据统计
        if (AssetBundleLoadAnalyse.Singleton.ABLoadAnalyseSwitch)
        {
            AssetBundleLoadAnalyse.Singleton.addABUnloadedTime(abi.AssetBundleName);
        }
        // AB卸载时ABAssetBundleInfo回收时回收重用
        AssetBundleInfoFactory.recycle(abi);
    }

    /// <summary>
    /// 队列里不再有AB需要加载时检查不再使用的AB
    /// </summary>
    private IEnumerator checkUnsedAssetBundle()
    {
        while(true)
        {
            if (EnableResourceRecyclingUnloadUnsed && mCurrentFPS >= mABRecycleFPSThreshold && isABRequestQueueEmpty())
            {
                float time = Time.time;
                // 检查正常加载的资源AB，回收不再使用的AB
                foreach (var loadedab in mAllLoadedABInfoMap[ResourceLoadType.NormalLoad])
                {
                    if (loadedab.Value.IsUnsed)
                    {
                        if ((time - loadedab.Value.LastUsedTime) > mABMinimumLifeTime)
                        {
                            mUnsedABInfoList.Add(loadedab.Value);
                        }
                    }
                }

                if (mUnsedABInfoList.Count > 0)
                {
                    // 根据最近使用时间升序排列
                    mUnsedABInfoList.Sort(ABILastUsedTimeSort);

                    for (int i = 0; i < mUnsedABInfoList.Count; i++)
                    {
                        if (i < mMaxUnloadABNumberPerFrame)
                        {
                            mAllLoadedABInfoMap[ResourceLoadType.NormalLoad].Remove(mUnsedABInfoList[i].AssetBundleName);
                            mUnsedABInfoList[i].dispose();
                        }
                        else
                        {
                            break;
                        }
                    }
                    mUnsedABInfoList.Clear();
                }
            }
            yield return mWaitForCheckUnsedABInterval;
        }
    }

    /// <summary> AB加载完成通知(用于更新AB加载管理) /// </summary>
    /// <param name="abl">AB加载任务信息</param>
    private void onABLoadCompleteNotifier(AssetBundleLoader abl)
    {
        var abname = abl.AssetBundleName;
        if (mABRequestTaskMap.ContainsKey(abname))
        {
            mABRequestTaskMap.Remove(abname);
            mAllLoadedABInfoMap[abl.LoadType].Add(abname, abl.ABInfo);
            //AB加载数据统计
            if (AssetBundleLoadAnalyse.Singleton.ABLoadAnalyseSwitch)
            {
                AssetBundleLoadAnalyse.Singleton.addABLoadedTime(abname);
            }
        }
        else
        {
            ResourceLogger.logErr(string.Format("收到不在加载任务请求队列里的AB:{0}加载完成回调!", abname));
        }
    }

    /// <summary>
    /// AB请求队列是否为空
    /// </summary>
    /// <returns></returns>
    private bool isABRequestQueueEmpty()
    {
        return mABRequestTaskMap.Count == 0;
    }

    /// <summary>
    /// AB信息根据最近使用时间排序
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int ABILastUsedTimeSort(AssetBundleInfo a, AssetBundleInfo b)
    {
        return a.LastUsedTime.CompareTo(b.LastUsedTime);
    }

    /// <summary>
    /// 更新已加载AB的加载类型
    /// </summary>
    /// <param name="resname">资源名</param>
    /// <param name="oldloadtype">旧的加载类型</param>
    /// <param name="newloadtype">新的家在类型</param>
    private void updateLoadedABILoadType(string resname, ResourceLoadType oldloadtype, ResourceLoadType newloadtype)
    {
        if (mAllLoadedABInfoMap[oldloadtype].ContainsKey(resname))
        {
            var abi = mAllLoadedABInfoMap[oldloadtype][resname];
            mAllLoadedABInfoMap[newloadtype].Add(resname, abi);
            mAllLoadedABInfoMap[oldloadtype].Remove(resname);
            ResourceLogger.log(string.Format("已加载的资源 : {0}从资源类型 : {1}更新到资源类型 : {2}！", resname, oldloadtype, newloadtype));
        }
        else
        {
            ResourceLogger.logErr(string.Format("资源类型 : {0}里找不到已加载的资源 : {1}，无法更新该资源的加载类型！", oldloadtype, resname));
        }
    }

    /// <summary>
    /// 递归卸载指定类型不再使用的AB(Note:不支持卸载常驻资源类型)
    /// </summary>
    /// <param name="resourceloadtype">AB资源加载类型</param>
    private void unloadSpecificLoadTypeUnsedABRecursively(ResourceLoadType resourceloadtype)
    {
        if (resourceloadtype == ResourceLoadType.PermanentLoad)
        {
            ResourceLogger.logErr("不允许卸载常驻AB资源!");
            return;
        }

        // 递归判定卸载所有不再可用的正常加载AB
        bool hasunsedab = true;
        while (hasunsedab)
        {
            // 检查回收不再使用正常已加载的AB
            foreach (var loadedab in mAllLoadedABInfoMap[resourceloadtype])
            {
                if (loadedab.Value.IsUnsed)
                {
                    mUnsedABInfoList.Add(loadedab.Value);
                }
            }

            if (mUnsedABInfoList.Count == 0)
            {
                //不再有可卸载的AB
                hasunsedab = false;
            }
            else
            {
                // 有可卸载的AB
                for (int i = 0; i < mUnsedABInfoList.Count; i++)
                {
                    mAllLoadedABInfoMap[resourceloadtype].Remove(mUnsedABInfoList[i].AssetBundleName);
                    mUnsedABInfoList[i].dispose();
                }
                mUnsedABInfoList.Clear();
            }
        }
    }

    #region 资源调试辅助功能
    /// <summary>
    /// 强制卸载指定资源(不管AB加载后属于哪一种类型，强制卸载)
    /// </summary>
    /// <param name="abname"></param>
    public void forceUnloadSpecificResource(string abname)
    {
        ResourceLoadType resourceloadtype = ResourceLoadType.NormalLoad;
        AssetBundleInfo abinfo = null;
        foreach (var loadedabinfomap in mAllLoadedABInfoMap)
        {
            if (abinfo != null)
            {
                break;
            }

            foreach (var loadedabinfo in loadedabinfomap.Value)
            {
                if (loadedabinfo.Key.Equals(abname))
                {
                    abinfo = loadedabinfo.Value;
                    resourceloadtype = loadedabinfomap.Key;
                    break;
                }
            }
        }

        if (abinfo != null)
        {
            mAllLoadedABInfoMap[resourceloadtype].Remove(abinfo.AssetBundleName);
            abinfo.dispose();
            ResourceLogger.log(string.Format("AB资源 : {0}已强制卸载！", abname));
        }
        else
        {
            ResourceLogger.logErr(string.Format("AB资源 : {0}未被加载，无法强制卸载!", abname));
        }
    }
    #endregion
}