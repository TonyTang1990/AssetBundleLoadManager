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
    /// 已加载AB里不再有有效引用的AB信息列表
    /// </summary>
    private List<AbstractResourceInfo> mUnsedABInfoList;

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

    /// <summary>
    /// 资源加载模块初始化
    /// </summary>
    public override void init()
    {
        base.init();
        // 初始化ABLoader和ABInfo可重用的基础数量
        AssetBundleLoaderFactory.initialize(20);             // 考虑到大部分都是采用同步加载，所以AssetBundleLoader并不需要初始化太多
        AssetBundleInfoFactory.initialize(200);
        mAssetBundleDpMap = new Dictionary<string, string[]>();

        ResLoadMode = ResourceLoadMode.AssetBundle;

        mABRequestTaskMap = new Dictionary<string, AssetBundleLoader>();
        mUnsedABInfoList = new List<AbstractResourceInfo>();

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
    /// 真正的请求资源
    /// </summary>
    /// <param name="resname">资源AB名</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    protected override void realRequestResource(string resname, LoadResourceCompleteHandler completehandler, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        // 如果资源已经加载完成，直接返回
        if (mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad].ContainsKey(resname))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad][resname]);
            if (loadtype > ResourceLoadType.NormalLoad)
            {
                updateLoadedResourceInfoLoadType(resname, ResourceLoadType.NormalLoad, loadtype);
            }
        }
        else if (mAllLoadedResourceInfoMap[ResourceLoadType.Preload].ContainsKey(resname))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.Preload][resname]);
            if (loadtype > ResourceLoadType.Preload)
            {
                updateLoadedResourceInfoLoadType(resname, ResourceLoadType.Preload, loadtype);
            }
        }
        else if (mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad].ContainsKey(resname))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad][resname]);
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
        base.Update();
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
        if(depabnames != null)
        {
            DIYLog.Log($"{resname}资源依赖的AB资源如下:");
            foreach (var depabname in depabnames)
            {
                DIYLog.Log($"{depabname}");
            }
        }
        return loader;
    }    
    
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
        abi.onResourceUnloadedCallback = onABUnloaded;
        return abi;
    }

    /// <summary>
    /// 对应AB卸载回调
    /// </summary>
    /// <param name="ari"></param>
    private static void onABUnloaded(AbstractResourceInfo ari)
    {
        var abi = ari as AssetBundleInfo;
        //AB卸载数据统计
        if (ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch)
        {
            ResourceLoadAnalyse.Singleton.addResourceUnloadedTime(abi.AssetBundleName);
        }
        // AB卸载时ABAssetBundleInfo回收时回收重用
        AssetBundleInfoFactory.recycle(abi);
    }

    /// <summary>
    /// 队列里不再有AB需要加载时检查不再使用的AB
    /// </summary>
    protected override IEnumerator checkUnsedResource()
    {
        while(true)
        {
            if (EnableResourceRecyclingUnloadUnsed && mCurrentFPS >= mABRecycleFPSThreshold && mABRequestTaskMap.Count == 0)
            {
                float time = Time.time;
                // 检查正常加载的资源AB，回收不再使用的AB
                foreach (var loadedab in mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad])
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
                            mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad].Remove(mUnsedABInfoList[i].AssetBundleName);
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
            mAllLoadedResourceInfoMap[abl.LoadType].Add(abname, abl.ABInfo);
            //AB加载数据统计
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
    /// AB信息根据最近使用时间排序
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int ABILastUsedTimeSort(AbstractResourceInfo a, AbstractResourceInfo b)
    {
        return a.LastUsedTime.CompareTo(b.LastUsedTime);
    }

    /// <summary>
    /// 真正执行资源卸载指定类型不再使用的资源接口
    /// </summary>
    /// <param name="resourceloadtype">资源加载类型</param>
    protected override void doUnloadSpecificLoadTypeUnsedResource(ResourceLoadType resourceloadtype)
    {
        // 递归判定卸载所有不再可用的正常加载AB
        bool hasunsedab = true;
        while (hasunsedab)
        {
            // 检查回收不再使用正常已加载的AB
            foreach (var loadedab in mAllLoadedResourceInfoMap[resourceloadtype])
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
                    mAllLoadedResourceInfoMap[resourceloadtype].Remove(mUnsedABInfoList[i].AssetBundleName);
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
        AbstractResourceInfo arinfo = null;
        foreach (var loadedabinfomap in mAllLoadedResourceInfoMap)
        {
            if (arinfo != null)
            {
                break;
            }

            foreach (var loadedabinfo in loadedabinfomap.Value)
            {
                if (loadedabinfo.Key.Equals(abname))
                {
                    arinfo = loadedabinfo.Value;
                    resourceloadtype = loadedabinfomap.Key;
                    break;
                }
            }
        }

        if (arinfo != null)
        {
            mAllLoadedResourceInfoMap[resourceloadtype].Remove(arinfo.AssetBundleName);
            arinfo.dispose();
            ResourceLogger.log(string.Format("AB资源 : {0}已强制卸载！", abname));
        }
        else
        {
            ResourceLogger.logErr(string.Format("AB资源 : {0}未被加载，无法强制卸载!", abname));
        }
    }
    #endregion
}