/*
 * Description:             AssetBundleModule.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/07
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    /// Asset打包信息
    /// </summary>
    public AssetBuildInfoAsset AssetBuildInfo
    {
        get
        {
            return mAssetBuildInfo;
        }
    }
    protected AssetBuildInfoAsset mAssetBuildInfo;

    /// <summary>
    /// 加载Asset打包信息
    /// </summary>
    private void loadAssetBuildInfo()
    {
        // 确保之前加载的AB打包信息卸载彻底
        if(mAssetBuildInfo != null)
        {
            Resources.UnloadAsset(mAssetBuildInfo);
            mAssetBuildInfo = null;
        }
        // AssetBundle打包信息比较特殊，在未加载完成前拿不到AB名字映射
        // 所以这里单独特殊加载,不走正常流程
        var abpath = AssetBundlePath.GetABLoadFullPath(ResourceConstData.AssetBuildInfoAssetName);
        AssetBundle ab = null;
        ab = AssetBundle.LoadFromFile(abpath);
        if(ab != null)
        {
            mAssetBuildInfo = ab.LoadAsset<AssetBuildInfoAsset>(ResourceConstData.AssetBuildInfoAssetName);
            mAssetBuildInfo.init();
            ab.Unload(false);
            Debug.Log("Asset打包信息文件加载成功!");
        }
        else
        {
            Debug.LogError($"找不到Asset打包信息文件:{ResourceConstData.AssetBuildInfoAssetName}");
        }
    }
    
    /// <summary>
    /// 获取AssetBundle所依赖的AB信息
    /// </summary>
    /// <param name="abpath"></param>
    /// <returns></returns>
    private string[] getAssetBundleDpInfo(string abpath)
    {
        if (mAssetBuildInfo.ABPathDepMap.ContainsKey(abpath))
        {
            return mAssetBuildInfo.ABPathDepMap[abpath];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// 获取指定Asset路径的AB名
    /// </summary>
    /// <param name="assetpath"></param>
    /// <returns></returns>
    private string getAssetPathAssetBundleName(string assetpath)
    {
        var abpath = mAssetBuildInfo.getAssetABPath(assetpath);
        if(!string.IsNullOrEmpty(abpath))
        {
            return abpath;
        }
        else
        {
            Debug.LogError($"找不到Asset路径:{assetpath}的AB路径信息!");
            return null;
        }
    }

    /// <summary>
    /// 打印所有AB依赖信息
    /// </summary>
    public void printAllResourceDpInfo()
    {
        foreach (var abinfo in mAssetBuildInfo.ABPathDepMap)
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
        // 加载Asset打包信息
        loadAssetBuildInfo();
    }

    /// <summary>
    /// 真正的请求资源
    /// </summary>
    /// <param name="respath">资源路径(带后缀)</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    protected override void realRequestResource(string respath, LoadResourceCompleteHandler completehandler, ResourceLoadType loadtype = ResourceLoadType.NormalLoad, ResourceLoadMethod loadmethod = ResourceLoadMethod.Sync)
    {
        // AB运行时统一转成小写,避免和AB打包那方输出的信息不一致
        respath = respath.ToLower();
        var abpath = string.Empty;
        // 因为依赖AB加载也是走统一入口，所以要区分是AB路径还是Asset路径
        if (!mAssetBuildInfo.isABPath(respath))
        {
            // AB依赖信息文件和AB打包
            abpath = getAssetPathAssetBundleName(respath);
        }
        else
        {
            abpath = respath;
        }
        if(string.IsNullOrEmpty(abpath))
        {
            Debug.LogError($"找不到资源:{respath}的AB名信息, 加载失败!");
            return;
        }
        // 如果资源已经加载完成，直接返回
        if (mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad].ContainsKey(abpath))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad][abpath]);
            if (loadtype > ResourceLoadType.NormalLoad)
            {
                updateLoadedResourceInfoLoadType(abpath, ResourceLoadType.NormalLoad, loadtype);
            }
        }
        else if (mAllLoadedResourceInfoMap[ResourceLoadType.Preload].ContainsKey(abpath))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.Preload][abpath]);
            if (loadtype > ResourceLoadType.Preload)
            {
                updateLoadedResourceInfoLoadType(abpath, ResourceLoadType.Preload, loadtype);
            }
        }
        else if (mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad].ContainsKey(abpath))
        {
            completehandler(mAllLoadedResourceInfoMap[ResourceLoadType.PermanentLoad][abpath]);
        }
        else
        {
            // 确保同一个资源加载的Loader是同一个
            // 保证同一个资源加载完成时上层所有加载该资源的回调正确
            AssetBundleLoader abloader = null;
            if (mABRequestTaskMap.ContainsKey(abpath))
            {
                abloader = mABRequestTaskMap[abpath];
                // 之前有请求resname资源，但还未完成
                // 比如先异步请求resname，在异步完成前来了一个同步请求resname
                // 修改加载方式并添加回调，调用同步加载方式，异步加载会在同步加载完成时一起回调
                abloader.LoadMethod = loadmethod;
                abloader.LoadType = loadtype;
                abloader.LoadABCompleteCallBack += completehandler;
                abloader.LoadSelfABCompleteNotifier = onABLoadCompleteNotifier;
                if (loadmethod == ResourceLoadMethod.Sync)
                {
                    ResourceLogger.log(string.Format("请求同步加载一个异步加载状态:{0}的资源 : {1}", abloader.LoadState.ToString(), abloader.AssetBundlePath));
                    //重置AB加载状态，走同步加载模式
                    abloader.LoadState = ResourceLoadState.None;
                    abloader.startLoad();
                }
            }
            else
            {
                abloader = createABLoader(abpath);
                abloader.LoadMethod = loadmethod;
                abloader.LoadType = loadtype;
                abloader.LoadABCompleteCallBack = completehandler;
                abloader.LoadSelfABCompleteNotifier = onABLoadCompleteNotifier;
                mABRequestTaskMap.Add(abpath, abloader);
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
    /// <param name="abpath">AB路径</param>
    /// <returns></returns>
    private AssetBundleLoader createABLoader(string abpath)
    {
        var depabnames = getAssetBundleDpInfo(abpath);
        var loader = AssetBundleLoaderFactory.create();
        loader.AssetBundlePath = abpath;
        loader.DepABPaths = depabnames;
        if(depabnames != null)
        {
            DIYLog.Log($"{abpath}资源依赖的AB资源如下:");
            foreach (var depabname in depabnames)
            {
                DIYLog.Log($"{depabname}");
            }
        }
        return loader;
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
                            mAllLoadedResourceInfoMap[ResourceLoadType.NormalLoad].Remove(mUnsedABInfoList[i].ResourcePath);
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
        var abname = abl.AssetBundlePath;
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
                    mAllLoadedResourceInfoMap[resourceloadtype].Remove(mUnsedABInfoList[i].ResourcePath);
                    mUnsedABInfoList[i].dispose();
                }
                mUnsedABInfoList.Clear();
            }
        }
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
        abi.ResourcePath = abname;
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
            ResourceLoadAnalyse.Singleton.addResourceUnloadedTime(abi.ResourcePath);
        }
        // AB卸载时ABAssetBundleInfo回收时回收重用
        AssetBundleInfoFactory.recycle(abi);
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
            mAllLoadedResourceInfoMap[resourceloadtype].Remove(arinfo.ResourcePath);
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