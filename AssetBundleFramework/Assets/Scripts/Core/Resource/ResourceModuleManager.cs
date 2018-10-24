/*
 * Description:             ResourceModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/12
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 资源加载方案1：
// 1. 加载AB
// 2. 判定AB是否有依赖AB，有依赖AB先加载依赖AB，没有则直接加载自身AB
// 3. 有依赖AB等依赖AB加载完再加载自身AB
// 4. 自身AB加载完回调上层通知

// 方案1问题:
// 当依赖AB互相依赖时，会导致死锁

// 解决方案:
// 资源加载方案2:
// 修改成先加载自己，自己加载完成再加载依赖AB
// 然后等所有依赖AB加载完成再往上回调

// AB加载管理相关概念：
// 1. 依赖AB与被依赖者采用同样的加载方式(ABLoadMethod)，但加载方式依赖AB统一采用ABLoadType.NormalLoad
// 2. 依赖AB通过索引计数管理，只要原始AB不被卸载，依赖AB就不会被卸载
// 3. 已加载的AB资源加载类型只允许从低往高变(NormalLoad -> Preload -> PermanentLoad)，不允许从高往低(PermanentLoad -> Preload -> NormalLoad)

/// <summary>
/// AB资源加载方式
/// </summary>
public enum ABLoadMethod
{
    Sync = 1,          // 同步
    Async = 2          // 异步
}

/// <summary>
/// AB资源加载类型
/// Note:
/// 已加载的资源加载类型允许更改，但只允许从低往高变(NormalLoad -> Preload -> PermanentLoad)，不允许从高往低(PermanentLoad -> Preload -> NormalLoad)
/// </summary>
public enum ABLoadType
{
    NormalLoad = 1,         // 正常加载(可通过Tick检测判定正常卸载)
    Preload = 2,            // 预加载(切场景才会卸载)
    PermanentLoad = 3,      // 永久加载(常驻内存永不卸载)
}

/// <summary>
/// 重写ABLoadType比较相关接口函数，避免ABLoadType作为Dictionary Key时，
/// 底层调用默认Equals(object obj)和DefaultCompare.GetHashCode()导致额外的堆内存分配
/// 参考:
/// http://gad.qq.com/program/translateview/7194373
/// </summary>
public class ABLoadTypeComparer : IEqualityComparer<ABLoadType>
{
    public bool Equals(ABLoadType x, ABLoadType y)
    {
        return x == y;
    }

    public int GetHashCode(ABLoadType x)
    {
        return (int)x;
    }
}

/// <summary>
/// ResourceModuleManager.cs
/// 资源加载管理类
/// TODO: 支持预加载资源
/// </summary>
public class ResourceModuleManager : SingletonMonoBehaviourTemplate<ResourceModuleManager>, IModuleInterface {
    
    /// <summary>
    /// 真实的模块名
    /// </summary>
    public string ModuleName
    {
        get
        {
            return this.GetType().ToString();
        }
    }

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
        private set
        {
            mAssetBundleDpMap = value;
        }
    }
    private Dictionary<string, string[]> mAssetBundleDpMap;

    /// <summary>
    /// 加载所有AB依赖信息
    /// </summary>
    /// <returns></returns>
    private void loadAllDpInfo()
    {
        // 依赖信息加载完即可释放，不需要常驻
        requstResource(AssetBundlePath.DependencyFileName, onLoadAllDpInfoComplete, ABLoadType.NormalLoad, ABLoadMethod.Sync);
    }

    /// <summary>
    /// 加载AB依赖文件信息完成回调
    /// </summary>
    /// <param name="abi"></param>
    private void onLoadAllDpInfoComplete(AssetBundleInfo abi)
    {
        mAssetBundleDpMap.Clear();

        if (abi.Bundle == null)
        {
            Debug.LogError("找不到ab依赖配置文件");
            return;
        }
        var ta = abi.loadAsset<TextAsset>(AssetBundlePath.DependencyFileName.ToLower());
        if (ta == null)
        {
            Debug.LogError("ab依赖配置文件内容为空");
            return;
        }
        //解析
        using (var stream = new System.IO.MemoryStream(ta.bytes))
        {
            using (var br = new System.IO.BinaryReader(stream))
            {
                while (true)
                {
                    if (stream.Position == stream.Length)
                        break;
                    string name = br.ReadString();
                    int len = br.ReadInt16();
                    string[] deps = new string[len];
                    for (int i = 0; i < len; ++i)
                    {
                        deps[i] = br.ReadString();
                    }
                    mAssetBundleDpMap[name] = deps;
                }
            }
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
    public void printAllABDpInfo()
    {
        foreach (var abinfo in mAssetBundleDpMap)
        {
            Debug.Log(string.Format("AB Name:{0}", abinfo.Key));
            foreach (var dpfile in abinfo.Value)
            {
                Debug.Log(string.Format("       DP AB Name:{0}", dpfile));
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
        private set
        {
            mCurrentFPS = value;
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

    void Update()
    {
        // FPS计算部分
        mTotalDeltaTime += Time.deltaTime;
        mFrameCount++;
        if (mTotalDeltaTime >= mFPSUpdateInterval)
        {
            mCurrentFPS = (int)(mFrameCount / mTotalDeltaTime);
            mTotalDeltaTime = 0f;
            mFrameCount = 0;
        }

        //Update分帧加载的方案导致上层同步逻辑也必须按异步的方式写，这里暂时放弃此方案
        // AB加载请求部分
        // 限制每帧最大加载数量
        //while (mABRequestTaskList.Count > 0)
        //{
        //    var currentloadingablnumber = mABLoadingTaskList.Count;
        //    if (currentloadingablnumber < mMaxmumABLoadingNumberPerFrame)
        //    {
        //        var requesttask = mABRequestTaskList[0];
        //        mABRequestTaskList.RemoveAt(0);
        //        mABLoadingTaskList.Add(requesttask);
        //        requesttask.startLoad();
        //    }
        //    else
        //    {
        //        break;
        //    }
        //}
    }
    #endregion

    #region AB加载管理部分
    /// <summary>
    /// 是否开启AB回收检测(有些情况下不适合频繁回收创建，比如战斗场景)
    /// </summary>
    public bool EnableABUnloadUnsed
    {
        get;
        set;
    }

    /// <summary>
    /// AB资源请求任务列表
    /// </summary>
    public List<AssetBundleLoader> ABRequestTaskList
    {
        get
        {
            return mABRequestTaskList;
        }
        private set
        {
            mABRequestTaskList = value;
        }
    }
    private List<AssetBundleLoader> mABRequestTaskList;

    //Update分帧加载的方案导致上层同步逻辑也必须按异步的方式写，这里暂时放弃此方案
    /// <summary>
    /// AB资源加载中任务列表
    /// </summary>
    //public List<AssetBundleLoader> ABLoadingTaskList
    //{
    //    get
    //    {
    //        return mABLoadingTaskList;
    //    }
    //    private set
    //    {
    //        mABLoadingTaskList = value;
    //    }
    //}
    //private List<AssetBundleLoader> mABLoadingTaskList;

    /// <summary>
    /// AB资源加载任务映射map(仅用于快速访问AB加载任务，真正的任务列表是mABRequestTaskList)
    /// Key为资源名，Value为资源加载任务对象
    /// </summary>
    public Dictionary<string, AssetBundleLoader> AssetBundleLoaderTaskMap
    {
        get
        {
            return mAssetBundleLoaderTaskMap;
        }
        private set
        {
            mAssetBundleLoaderTaskMap = value;
        }
    }
    private Dictionary<string, AssetBundleLoader> mAssetBundleLoaderTaskMap;

    /// <summary>
    /// 所有已加载的AB的信息映射Map
    /// Key为AB加载类型，Value为该AB加载类型已经加载的AB资源信息映射Map(Key为AB名字，Value为AB加载信息)
    /// </summary>
    public Dictionary<ABLoadType, Dictionary<string, AssetBundleInfo>> AllLoadedABInfoMap
    {
        get
        {
            return mAllLoadedABInfoMap;
        }
        private set
        {
            mAllLoadedABInfoMap = value;
        }
    }
    private Dictionary<ABLoadType, Dictionary<string, AssetBundleInfo>> mAllLoadedABInfoMap;

    /// <summary>
    /// 已加载AB里不再有有效引用的AB信息列表
    /// </summary>
    private List<AssetBundleInfo> mUnsedABInfoList;

    //Update分帧加载的方案导致上层同步逻辑也必须按异步的方式写，这里暂时放弃此方案
    /// <summary>
    /// 单帧最大资源加载请求数量(不区分同步还是异步)
    /// Note:
    /// 这里的资源数量不是AB数量，加载指定资源AB与其所有依赖AB算一个资源加载请求(AssetBundleLoader算一个)
    /// </summary>
    //private int mMaxmumABLoadingNumberPerFrame;

    /// <summary> 检测未使用AB时间间隔(在请求队列为空时才检测未使用AB) /// </summary>
    private float mCheckUnsedABTimeInterval;

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

    /// <summary> 逻辑层AB加载完成委托 /// </summary>
    /// <param name="abinfo"></param>
    public delegate void LoadABCompleteHandler(AssetBundleInfo abinfo);

    /// <summary> 资源管理层AB加载完成委托 /// </summary>
    /// <param name="abl">AB加载任务信息</param>
    public delegate void LoadABCompleteNotifier(AssetBundleLoader abl);

    protected void Awake()
    {

    }

    /// <summary>
    /// 初始化资源加载相关的
    /// </summary>
    public void init()
    {
        // 初始化ABLoader和ABInfo可重用的基础数量
        AssetBundleLoaderFactory.initialize(5);     // 考虑到大部分时候都是用同步，所以AssetBundleLoader不需要太多可直接重用之前的
        AssetBundleInfoFactory.initialize(200);

        mAssetBundleDpMap = new Dictionary<string, string[]>();
        EnableABUnloadUnsed = true;
        mABRequestTaskList = new List<AssetBundleLoader>();
        //mABLoadingTaskList = new List<AssetBundleLoader>();
        mAssetBundleLoaderTaskMap = new Dictionary<string, AssetBundleLoader>();
        mAllLoadedABInfoMap = new Dictionary<ABLoadType, Dictionary<string, AssetBundleInfo>>(new ABLoadTypeComparer());
        mAllLoadedABInfoMap.Add(ABLoadType.NormalLoad, new Dictionary<string, AssetBundleInfo>());
        mAllLoadedABInfoMap.Add(ABLoadType.Preload, new Dictionary<string, AssetBundleInfo>());
        mAllLoadedABInfoMap.Add(ABLoadType.PermanentLoad, new Dictionary<string, AssetBundleInfo>());
        mUnsedABInfoList = new List<AssetBundleInfo>();
        
        //mMaxmumABLoadingNumberPerFrame = 5;
        mCheckUnsedABTimeInterval = 5.0f;
        mMaxUnloadABNumberPerFrame = 10;
        mABMinimumLifeTime = 10.0f;
        mABRecycleFPSThreshold = 20;

        mFPSUpdateInterval = 1.0f;

        // TODO: 根据设备设定相关参数

        loadAllDpInfo();
    }

    /// <summary>
    /// 开启AB不再使用回收检测
    /// </summary>
    public void startUnsedABRecyclingTask()
    {
        InvokeRepeating("checkUnsedAssetBundle", 0.0f, mCheckUnsedABTimeInterval);
    }

    /// <summary>
    /// 请求资源
    /// 资源加载统一入口
    /// </summary>
    /// <param name="resname">资源AB名</param>
    /// <param name="completehandler">加载完成上层回调</param>
    /// <param name="loadtype">资源加载类型</param>
    /// <param name="loadmethod">资源加载方式</param>
    public void requstResource(string resname, LoadABCompleteHandler completehandler, ABLoadType loadtype = ABLoadType.NormalLoad, ABLoadMethod loadmethod = ABLoadMethod.Sync)
    {
        resname = resname.ToLower();
        // 如果资源已经加载完成，直接返回
        if (mAllLoadedABInfoMap[ABLoadType.NormalLoad].ContainsKey(resname))
        {
            completehandler(mAllLoadedABInfoMap[ABLoadType.NormalLoad][resname]);
            if (loadtype > ABLoadType.NormalLoad)
            {
                updateLoadedABILoadType(resname, ABLoadType.NormalLoad, loadtype);
            }
        }
        else if (mAllLoadedABInfoMap[ABLoadType.Preload].ContainsKey(resname))
        {
            completehandler(mAllLoadedABInfoMap[ABLoadType.Preload][resname]);
            if (loadtype > ABLoadType.Preload)
            {
                updateLoadedABILoadType(resname, ABLoadType.Preload, loadtype);
            }
        }
        else if(mAllLoadedABInfoMap[ABLoadType.PermanentLoad].ContainsKey(resname))
        {
            completehandler(mAllLoadedABInfoMap[ABLoadType.PermanentLoad][resname]);
        }
        else
        {
            // 确保同一个资源加载的Loader是同一个
            // 保证同一个资源加载完成时上层所有加载该资源的回调正确
            AssetBundleLoader abloader = null;
            if(mAssetBundleLoaderTaskMap.ContainsKey(resname))
            {
                abloader = mAssetBundleLoaderTaskMap[resname];
                // 之前有请求resname资源，但还未完成
                // 比如先异步请求resname，在异步完成前来了一个同步请求resname
                // 修改加载方式并添加回调，异步会在同步加载完成时一起回调
                abloader.LoadMethod = loadmethod;
                abloader.LoadType = loadtype;
                abloader.LoadABCompleteCallBack += completehandler;
                abloader.LoadABCompleteNotifier = onABLoadCompleteNotifier;
            }
            else
            {
                abloader = createABLoader(resname);
                abloader.LoadMethod = loadmethod;
                abloader.LoadType = loadtype;
                abloader.LoadABCompleteCallBack = completehandler;
                abloader.LoadABCompleteNotifier = onABLoadCompleteNotifier;
                mAssetBundleLoaderTaskMap.Add(resname, abloader);
                mABRequestTaskList.Add(abloader);
                abloader.startLoad();
            }
        }
    }

    /// <summary>
    /// 创建AssetBundleInfo对象信息
    /// </summary>
    /// <param name="abname"></param>
    /// <param name="ab"></param>
    /// <returns></returns>
    public AssetBundleInfo createAssetBundleInfo(string abname, AssetBundle ab)
    {
        var abi = AssetBundleInfoFactory.create();
        abi.AssetBundleName = abname;
        abi.Bundle = ab;
        abi.onUnloadedCallback = onABUnloaded;
        return abi;
    }

    /// <summary>
    /// 释放可释放的预加载资源AB(递归判定，不限制回收数量)
    /// Note:
    /// 切场景前调用，确保所有预加载资源正确释放
    /// </summary>
    public void unloadAllUnsedPreloadLoadedAB()
    {
        unloadSpecificLoadTypeUnsedABRecursively(ABLoadType.Preload);
    }

    /// <summary>
    /// 提供给外部的触发卸载所有正常加载不再使用的AB接口(递归判定，不限制回收数量)
    /// Note:
    /// 同步接口，回收数量会比较大，只建议切场景时场景卸载后调用一次
    /// </summary>
    public void unloadAllUnsedNormalLoadedAB()
    {
        unloadSpecificLoadTypeUnsedABRecursively(ABLoadType.NormalLoad);
    }

    /// <summary>
    /// 获取指定加载类型的已加载AB信息映射Map
    /// </summary>
    /// <param name="loadtype">资源加载类型</param>
    /// <returns></returns>
    public Dictionary<string, AssetBundleInfo> getSpecificLoadTypeABIMap(ABLoadType loadtype)
    {
        if(mAllLoadedABInfoMap.ContainsKey(loadtype))
        {
            return mAllLoadedABInfoMap[loadtype];
        }
        else
        {
            Debug.LogError(string.Format("找不到资源类型 : {0}的已加载AB信息!", loadtype));
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
        foreach (var loadedab in mAllLoadedABInfoMap[ABLoadType.NormalLoad])
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
        foreach (var loadedab in mAllLoadedABInfoMap[ABLoadType.Preload])
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
        Debug.Log("Normal Loaded AB Info:");
        foreach(var abi in mAllLoadedABInfoMap[ABLoadType.NormalLoad])
        {
            abi.Value.printAllOwnersNameAndRefCount();
        }

        Debug.Log("Preload Loaded AB Info:");
        foreach (var abi in mAllLoadedABInfoMap[ABLoadType.Preload])
        {
            abi.Value.printAllOwnersNameAndRefCount();
        }

        Debug.Log("Permanent Loaded AB Info:");
        foreach (var abi in mAllLoadedABInfoMap[ABLoadType.PermanentLoad])
        {
            abi.Value.printAllOwnersNameAndRefCount();
        }
    }
    #endregion

    /// <summary>
    /// 创建AB资源加载对象
    /// </summary>
    /// <param name="resname">资源名</param>
    /// <returns></returns>
    private AssetBundleLoader createABLoader(string resname)
    {
        var depabnames = getAssetBundleDpInfo(resname);
        var loader = AssetBundleLoaderFactory.create();
        loader.ABName = resname;
        loader.DepABNames = depabnames;
        return loader;
#if UNITY_EDITOR
        //return new EditorModeAssetBundleLoader();
#else
        //return new MobileAssetBundleLoader();
#endif
    }

    /// <summary>
    /// 对应AB卸载回调
    /// </summary>
    /// <param name="abi"></param>
    private void onABUnloaded(AssetBundleInfo abi)
    {
        // AB卸载时ABAssetBundleInfo回收时回收重用
        AssetBundleInfoFactory.recycle(abi);
    }

    /// <summary>
    /// 队列里不再有AB需要加载时检查不再使用的AB
    /// </summary>
    private void checkUnsedAssetBundle()
    {
        if(EnableABUnloadUnsed && mCurrentFPS >= mABRecycleFPSThreshold && isABRequestQueueEmpty())
        {
            float time = Time.time;
            // 检查正常加载的资源AB，回收不再使用的AB
            foreach (var loadedab in mAllLoadedABInfoMap[ABLoadType.NormalLoad])
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

                // 卸载AB并重用Loader
                for (int i = 0; i < mUnsedABInfoList.Count; i++)
                {
                    if (i < mMaxUnloadABNumberPerFrame)
                    {
                        mAllLoadedABInfoMap[ABLoadType.NormalLoad].Remove(mUnsedABInfoList[i].AssetBundleName);
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
    }

    /// <summary> AB加载完成通知(用于更新AB加载管理) /// </summary>
    /// <param name="abl">AB加载任务信息</param>
    private void onABLoadCompleteNotifier(AssetBundleLoader abl)
    {
        var abname = abl.ABName;
        if (mAssetBundleLoaderTaskMap.ContainsKey(abname))
        {
            mAssetBundleLoaderTaskMap.Remove(abname);
            mABRequestTaskList.Remove(abl);
            //mABLoadingTaskList.Remove(abl);
            mAllLoadedABInfoMap[abl.LoadType].Add(abname, abl.ABInfo);
        }
        else
        {
            Debug.LogError(string.Format("收到不在加载任务请求队列里的AB:{0}加载完成回调!", abname));
        }
    }

    /// <summary>
    /// AB请求队列是否为空
    /// </summary>
    /// <returns></returns>
    private bool isABRequestQueueEmpty()
    {
        return mABRequestTaskList.Count == 0;
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
    private void updateLoadedABILoadType(string resname, ABLoadType oldloadtype, ABLoadType newloadtype)
    {
        if(mAllLoadedABInfoMap[oldloadtype].ContainsKey(resname))
        {
            var abi = mAllLoadedABInfoMap[oldloadtype][resname];
            mAllLoadedABInfoMap[newloadtype].Add(resname, abi);
            mAllLoadedABInfoMap[oldloadtype].Remove(resname);
            Debug.Log(string.Format("已加载的资源 : {0}从资源类型 : {1}更新到资源类型 : {2}！", resname, oldloadtype, newloadtype));
        }
        else
        {
            Debug.LogError(string.Format("资源类型 : {0}里找不到已加载的资源 : {1}，无法更新该资源的加载类型！", oldloadtype, resname));
        }
    }

    /// <summary>
    /// 递归卸载指定类型不再使用的AB(Note:不支持卸载常驻资源类型)
    /// </summary>
    /// <param name="abloadtype">AB资源加载类型</param>
    private void unloadSpecificLoadTypeUnsedABRecursively(ABLoadType abloadtype)
    {
        if (abloadtype == ABLoadType.PermanentLoad)
        {
            Debug.LogError("不允许卸载常驻AB资源!");
            return;
        }

        // 递归判定卸载所有不再可用的正常加载AB
        bool hasunsedab = true;
        while (hasunsedab)
        {
            // 检查回收不再使用正常已加载的AB
            foreach (var loadedab in mAllLoadedABInfoMap[abloadtype])
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
                    mAllLoadedABInfoMap[abloadtype].Remove(mUnsedABInfoList[i].AssetBundleName);
                    mUnsedABInfoList[i].dispose();
                }
                mUnsedABInfoList.Clear();
            }
        }
    }
    #endregion
}