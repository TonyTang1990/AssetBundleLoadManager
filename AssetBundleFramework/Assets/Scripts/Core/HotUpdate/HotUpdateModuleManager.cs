/*
 * Description:             HotUpdateModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/14
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// 版本强更和资源热更流程：
// 1. 重进游戏
// 2. 初始化本地版本信息
// 3. 检测是否强更过版本
// 4. 强更过版本                                                                                         -> 5
// 5. 清除包外目录(1. 热更资源目录 2. 包外版本信息目录 3. 资源热更记录目录)                              -> 1
// 6. 没有强更过版本                                                                                     -> 7
// 7. 拉去服务器列表信息(含最新版本以及资源版本信息)
// 8. 检测是否需要版本强更
// 9. 需要版本强更                                                                                       -> 10
// 10. 存储本地版本号到包外。引导版本强更。
// 11. 版本强更完成                                                                                      -> 1
// 11. 不需要版本强更                                                                                    -> 12
// 12. 判定是否需要资源热更(比较服务器资源版本号和本地资源版本号)
// 13. 需要资源热更
// 14. 拉去服务器对应版本最新的资源MD5信息文件与本地资源MD5信息文件比较，得出需要热更的资源列表  -> 1
// 15. 不需要资源热更
// 16. 进入游戏

/// <summary>
/// HotUpdateModuleManager.cs
/// 热更模块(含资源和版本更新)
/// Note:
/// 服务器那一块待开发，所以先默认有资源可以更新，暂时只做资源热更下载这一块
/// </summary>
public class HotUpdateModuleManager : SingletonTemplate<HotUpdateModuleManager>
{
    /// <summary>
    /// 热更新资源目录名
    /// </summary>
    private const string HotUpdateFolderName = "HotUpdate";

    /// <summary>
    /// 热更开关
    /// </summary>
    public bool HotUpdateSwitch
    {
        get;
        set;
    }

    /// <summary>
    /// 服务器端版本信息(暂时无后端，服务器版本信息通过静态资源更新访问)
    /// </summary>
    public VersionConfig ServerVersionConfig
    {
        get;
        private set;
    }

    /// <summary>
    /// 版本强更进度
    /// </summary>
    public float HotVersionUpdateProgress
    {
        get
        {
            return HotVersionUpdateRequest.CurrentProgress;
        }
    }

    /// <summary>
    /// 服务器版本文件名
    /// </summary>
    public const string ServerVersionConfigFileName = "ServerVersionConfig.json";

    /// <summary>
    /// 服务器版本信息资源更新完成回调
    /// </summary>
    private Action<bool> mServerVersionConfigHotUpdateCompleteCB;

    /// <summary>
    /// 热更新地址信息配置文件名
    /// </summary>
    private const string mVersionConfigFileName = "HotUpdateConfig";

    /// <summary>
    /// 配置文件目录路径
    /// </summary>
    private const string ConfigFolderPath = "Config/";
    
    /// <summary>
    /// 热更新文件存储路径
    /// </summary>
    private string mHotUpdateConfigFilePath;

    /// <summary>
    /// 热更新信息
    /// </summary>
    private HotUpdateConfig mHotUpdateConfig;

    /// <summary>
    /// 资源热更地址
    /// </summary>
    private string mHotUpdateURL;

    /// <summary>
    /// 版本强更缓存目录
    /// </summary>
    public string VersionHotUpdateCacheFolderPath
    {
        get;
        private set;
    }

    /// <summary>
    /// 版本强更文件缓存目录
    /// </summary>
    public string VersionHotUpdateCacheFilePath
    {
        get;
        private set;
    }

    /// <summary>
    /// 热更APK文件名
    /// </summary>
    private string mVersionHotUpdateFileName = string.Empty;//"AssetBundleFramework.apk";

    /// <summary>
    /// 版本强更请求
    /// </summary>
    public TWebRequest HotVersionUpdateRequest
    {
        get;
        private set;
    }

    /// <summary>
    /// 版本强更完成回调
    /// </summary>
    private Action<bool> mVersionHotUpdateCompleteCB;

    /// <summary>
    /// 资源热更进度
    /// </summary>
    public float HotResourceUpdateProgress
    {
        get
        {
            return mHotResourceUpdateRequest.CurrentProgress;
        }
    }

    /// <summary>
    /// 需要资源热更的信息Map
    /// Key为资源路径，Value为该资源最新热更信息
    /// </summary>
    private Dictionary<string, HotUpdateAssetBundleInfo> mNeedHotUpdateAssetBundleInfoMap;

    /// <summary>
    /// 需要资源热更的资源总数
    /// </summary>
    private int mHotUpdateResourceTotalNumber;
    
    /// <summary>
    /// 资源更新完成回调
    /// </summary>
    private Action<bool> mResourceHotUpdateCompleteCB;

    /// <summary>
    /// 服务器资源热更列表映射Map
    /// Key为资源版本号，Value为该资源版本号需要热更的资源
    /// </summary>
    private Dictionary<int, List<string>> mResourceUpdateListMap;

    /// <summary>
    /// 本地已经热更到的资源列表(热更完毕后会清空)
    /// Key为资源版本号，Value为该资源版本号已经热更到的资源列表信息
    /// </summary>
    private SortedDictionary<int, List<string>> mLocalUpdatedResourceMap;

    /// <summary>
    /// 本地AssetBundle热更信息Map
    /// Key为资源路径，Value为该资源本地热更信息
    /// </summary>
    private Dictionary<string, HotUpdateAssetBundleInfo> mLocalHotUpdateAssetBundleInfoMap;

    /// <summary>
    /// 资源热更资源MD5信息文件名
    /// </summary>
    public const string AssetBundleMD5FileName = ResourceConstData.AssetBundleMd5InfoFileName;

    /// <summary>
    /// 本地资源热更记录文件目录路径
    /// </summary>
    private string LocalAssetBundleMD5FilFolderPath;

    /// <summary>
    /// 本地资源热更记录文件路径
    /// Note:
    /// 所有资源热更完成后内容置空
    /// 没有热更完的话会记录已经更到的资源列表
    /// </summary>
    private string LocalAssetBundleMD5FilePath;

    /// <summary>
    /// 资源热更请求
    /// </summary>
    private TWebRequest mHotResourceUpdateRequest;
    
    /// <summary>
    /// UTF8编码
    /// </summary>
    private UTF8Encoding mUTF8Encoding = new UTF8Encoding(true);

    public HotUpdateModuleManager()
    {
        HotUpdateSwitch = true;

        mHotUpdateConfigFilePath = ConfigFolderPath + mVersionConfigFileName;
        mHotUpdateConfig = null;
        mHotUpdateURL = string.Empty;

        mHotUpdateResourceTotalNumber = 0;
        VersionHotUpdateCacheFolderPath = Path.Combine(Application.persistentDataPath, HotUpdateFolderName);
        
        mVersionHotUpdateFileName = string.Empty;
        HotVersionUpdateRequest = new TWebRequest();
        mVersionHotUpdateCompleteCB = null;

        mNeedHotUpdateAssetBundleInfoMap = new Dictionary<string, HotUpdateAssetBundleInfo>();
        mResourceHotUpdateCompleteCB = null;
        mResourceUpdateListMap = new Dictionary<int, List<string>>();
        mLocalUpdatedResourceMap = new SortedDictionary<int, List<string>>();
        mLocalHotUpdateAssetBundleInfoMap = new Dictionary<string, HotUpdateAssetBundleInfo>();
        LocalAssetBundleMD5FilFolderPath = VersionHotUpdateCacheFolderPath;
        LocalAssetBundleMD5FilePath = Path.Combine(LocalAssetBundleMD5FilFolderPath, AssetBundleMD5FileName);
        Debug.Log($"本地资源MD5信息文件:{LocalAssetBundleMD5FilePath}");
        mHotResourceUpdateRequest = new TWebRequest();
    }

    /// <summary>
    /// 热更模块初始化
    /// </summary>
    public void init()
    {
        initHotUpdateConfig();
        initLocalAssetBundleMD5Info();
    }

    /// <summary>
    /// 初始化热更新地址信息
    /// </summary>
    private void initHotUpdateConfig()
    {
        mHotUpdateConfig = null;
        Debug.Log(string.Format("mHotUpdateConfigFilePath : {0}", mHotUpdateConfigFilePath));
        //读取包内的热更信息
        var hotupdateconfigasset = Resources.Load<TextAsset>(mHotUpdateConfigFilePath);
        if (hotupdateconfigasset != null)
        {
            Debug.Log("热更新地址信息信息:");
            var content = mUTF8Encoding.GetString(hotupdateconfigasset.bytes);
            Debug.Log(string.Format("content : {0}", content));
            mHotUpdateConfig = JsonUtility.FromJson<HotUpdateConfig>(content);
            Debug.Log(string.Format("APKName : {0} HotUpdateLocalURL : {1} HotUpdateURL : {2}", mHotUpdateConfig.APKName, mHotUpdateConfig.HotUpdateLocalURL, mHotUpdateConfig.HotUpdateURL));
            mVersionHotUpdateFileName = mHotUpdateConfig.APKName;
            VersionHotUpdateCacheFilePath = VersionHotUpdateCacheFolderPath + mVersionHotUpdateFileName;
#if DEVELOPMENT
            mHotUpdateURL = mHotUpdateConfig.HotUpdateLocalURL;
#else
            mHotUpdateURL = mHotUpdateConfig.HotUpdateURL;
#endif
        }
        else
        {
            Debug.LogError(string.Format("包内热更地址信息文件 : {0}不存在!", mHotUpdateConfigFilePath));
        }
    }

    /// <summary>
    /// 初始化本地以资源MD5信息
    /// </summary>
    private void initLocalAssetBundleMD5Info()
    {
        Debug.Log("初始化本地资源热更列表!");
        mLocalHotUpdateAssetBundleInfoMap.Clear();
        // 包外没有读包内的AssetBundleMD5.txt
        string[] resourceupdateinfo = null;
        if (File.Exists(LocalAssetBundleMD5FilePath))
        {
            resourceupdateinfo = File.ReadAllLines(LocalAssetBundleMD5FilePath);
        }
        else
        {
            var innerAssetBundleMD5Asset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(AssetBundleMD5FileName));
            resourceupdateinfo = innerAssetBundleMD5Asset.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }
        foreach (var updatedresource in resourceupdateinfo)
        {
            //避免因为最后一个空行导致报错
            if(updatedresource.IsNullOrEmpty())
            {
                continue;
            }
            var resourceinfo = updatedresource.Split(ResourceConstData.AssetBundlleInfoSeparater);
            var resPath = resourceinfo[0];
            var resMD5 = resourceinfo[1];
            if (!mLocalHotUpdateAssetBundleInfoMap.ContainsKey(resPath))
            {
                mLocalHotUpdateAssetBundleInfoMap.Add(resPath, new HotUpdateAssetBundleInfo(resPath, resMD5));
            }
            else
            {
                Debug.LogError($"有相同Asset路径资源:{resPath},请检查流程!");
            }
        }
    }

    #region 服务器版本资源信息拉去部分
    /// <summary>
    /// 执行获取服务器版本信息
    /// </summary>
    /// <returns></returns>
    public void doObtainServerVersionConfig(Action<bool> completecallback)
    {
        mServerVersionConfigHotUpdateCompleteCB = completecallback;
        //拉取服务器热更资源信息与本地资源热更信息进行比较
        TWebRequest twr = new TWebRequest();
        //URL = 热更新URL + 服务器版本信息文件名(ServerVersionConfig.json)
        var url = mHotUpdateURL + ServerVersionConfigFileName; ;
        twr.enqueue(url, null, serverVersionConfigCompleteCB);
        twr.startRequest();
    }

    /// <summary>
    /// 服务器版本信息拉去回调
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="fileMd5">文件MD5</param>
    /// <param name="downloadhandler">下载结果数据</param>
    /// <param name="requeststatus">下载状态</param>
    private void serverVersionConfigCompleteCB(string url, string fileMd5, DownloadHandler downloadhandler, TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus)
    {
        Debug.Log($"服务器版本信息资源列表地址:{url}");
        if (requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
        {
            Debug.Log(string.Format("服务器版本信息资源下载成功!服务器版本信息 : {0}", downloadhandler.text));
            ServerVersionConfig = JsonUtility.FromJson<VersionConfig>(downloadhandler.text);
            Debug.Log($"服务器版本信息:VersionCode : {ServerVersionConfig.VersionCode} ResourceVersionCode: {ServerVersionConfig.ResourceVersionCode}");
            mServerVersionConfigHotUpdateCompleteCB?.Invoke(true);
        }
        else
        {
            Debug.LogError("服务器版本信息资源下载失败!");
            mServerVersionConfigHotUpdateCompleteCB?.Invoke(false);
            mServerVersionConfigHotUpdateCompleteCB = null;
        }
    }
    #endregion

    #region 版本强更部分
    /// <summary>
    /// 检查是否已经版本强更完成
    /// </summary>
    /// <returns></returns>
    public bool checkHasVersionHotUpdate()
    {
        Debug.Log("checkHasVersionHotUpdate()");
        if (VersionConfigModuleManager.Singleton.hasVersionHotUpdate())
        {
            Debug.Log("强更过版本！清除包外目录！");
            // 清空包外目录
            // 含如下几个目录:
            // 1. 包外资源热更存储目录(AssetBundlePath.ABHotUpdatePath)
            // 2. 包外包外热更资源信息目录(LocalResourceUpdateListFilFolderPath)
            // 3. 包外包外版本信息存储目录(VersionConfigModuleManager.Singleton.OutterVersionConfigSaveFileFolderPath)
            // 4. 包外版本强更缓存目录(Application.persistentDataPath + "/download/")
            if (Directory.Exists(AssetBundlePath.ABHotUpdatePath))
            {
                Directory.Delete(AssetBundlePath.ABHotUpdatePath, true);
                Debug.Log(string.Format("删除包外资源热更存储目录 : {0}", AssetBundlePath.ABHotUpdatePath));
            }
            if (Directory.Exists(LocalAssetBundleMD5FilFolderPath))
            {
                Directory.Delete(LocalAssetBundleMD5FilFolderPath, true);
                Debug.Log(string.Format("删除包外热更资源信息目录 : {0}", LocalAssetBundleMD5FilFolderPath));
            }
            if (Directory.Exists(VersionConfigModuleManager.Singleton.OutterVersionConfigSaveFileFolderPath))
            {
                Directory.Delete(VersionConfigModuleManager.Singleton.OutterVersionConfigSaveFileFolderPath, true);
                Debug.Log(string.Format("删除包外版本信息存储目录 : {0}", VersionConfigModuleManager.Singleton.OutterVersionConfigSaveFileFolderPath));
            }
            if (Directory.Exists(VersionHotUpdateCacheFolderPath))
            {
                Directory.Delete(VersionHotUpdateCacheFolderPath, true);
                Debug.Log(string.Format("删除包外版本强更包存储目录 : {0}", VersionHotUpdateCacheFolderPath));
            }
            VersionConfigModuleManager.Singleton.initVerisonConfigData();
            return true;
        }
        else
        {
            Debug.Log("未强更过版本！");
            return false;
        }
    }

    /// <summary>
    /// 检查版本强更
    /// </summary>
    /// <param name="newhotupdateversioncode">新的强更版本号</param>
    public bool checkVersionHotUpdate(double newhotupdateversioncode)
    {
        Debug.Log("checkVersionHotUpdate()");
        if (HotUpdateSwitch)
        {
            if (VersionConfigModuleManager.Singleton.needVersionHotUpdate(newhotupdateversioncode))
            {
                Debug.Log(string.Format("服务器版本号 : {0}高于本地版本号 : {1}，需要强更！", newhotupdateversioncode, VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode));
                return true;
            }
            else
            {
                Debug.Log(string.Format("服务器版本号 : {0}小于或等于本地版本号 : {1}，不需要强更！", newhotupdateversioncode, VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode));
                return false;
            }
        }
        else
        {
            Debug.Log("热更开关未打开，不允许热更！");
            return false;
        }
    }

    /// <summary>
    /// 执行新版本强更
    /// </summary>
    /// <param name="newhotupdateversioncode">新的强更版本号</param>
    /// <param name="completecallback">强更完成回调</param>
    public void doNewVersionHotUpdate(double newhotupdateversioncode, Action<bool> completecallback)
    {
        Debug.Log("doNewVersionHotUpdate()");
        //写入当前版本信息到包外
        VersionConfigModuleManager.Singleton.saveNewVersionCodeOuterConfig(VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode);
        //引导版本强更
        mVersionHotUpdateCompleteCB = completecallback;
        HotVersionUpdateRequest.resetRequest();
        var versionhotupdatefilepath = mHotUpdateURL + newhotupdateversioncode + "/" + mVersionHotUpdateFileName;
        HotVersionUpdateRequest.enqueue(versionhotupdatefilepath, null, versionHotUpdateCompleteCB, 1800);
        HotVersionUpdateRequest.startRequest();
    }

    /// <summary>
    /// 版本强更下载完成回调
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="fileMd5">文件MD5</param>
    /// <param name="downloadhandler">下载结果数据</param>
    /// <param name="requeststatus">下载状态</param>
    private void versionHotUpdateCompleteCB(string url, string fileMd5, DownloadHandler downloadhandler, TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus)
    {
        Debug.Log(string.Format("版本强更资源地址 : {0}", url));
        if (requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
        {
            Debug.Log("版本强更资源下载成功!版本强更资源信息");
            try
            {
                Debug.Log("安装包下载完成!");
                if (!Directory.Exists(VersionHotUpdateCacheFolderPath))
                {
                    Directory.CreateDirectory(VersionHotUpdateCacheFolderPath);
                }
                if (File.Exists(VersionHotUpdateCacheFilePath))
                {
                    File.Delete(VersionHotUpdateCacheFilePath);
                }
                using (var fs = File.Create(VersionHotUpdateCacheFilePath))
                {
                    fs.Write(downloadhandler.data, 0, downloadhandler.data.Length);
                    fs.Flush();
                    fs.Close();
                    Debug.Log(VersionHotUpdateCacheFilePath + "文件写入完成!");
                    mVersionHotUpdateCompleteCB(true);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(string.Format("版本强更资源异常 : {0}！", e.Message));
                Debug.LogError("版本强更失败!");
                mVersionHotUpdateCompleteCB(false);
            }
            finally
            {
                mVersionHotUpdateCompleteCB = null;
            }
        }
        else
        {
            Debug.LogError("版本强更资源下载失败!");
            mVersionHotUpdateCompleteCB(false);
            mVersionHotUpdateCompleteCB = null;
        }
    }
#endregion

#region 资源热更新部分
    /// <summary>
    /// 检查资源热更
    /// </summary>
    /// <param name="newhotupdateresourcecode">新的资源版本号</param>
    public bool checkResourceHotUpdate(int newhotupdateresourcecode)
    {
        Debug.Log("checkResourceHotUpdate()");
        if (HotUpdateSwitch)
        {
            if (VersionConfigModuleManager.Singleton.needResourceHotUpdate(newhotupdateresourcecode))
            {
                Debug.Log(string.Format("服务器资源版本号 : {0}大于本地资源版本号 : {1}，需要资源热更！", newhotupdateresourcecode, VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode));
                return true;
            }
            else
            {
                Debug.Log(string.Format("服务器资源版本号 : {0}小于或等于本地资源版本号 : {1}，不需要资源热更！", newhotupdateresourcecode, VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode));
                return false;
            }
        }
        else
        {
            Debug.Log("热更开关未打开，不允许热更资源！");
            return false;
        }
    }

    /// <summary>
    /// 执行资源热更新
    /// </summary>
    /// <param name="newresourcehotupdate"></param>
    /// <param name="completecallback"></param>
    public void doResourceHotUpdate(int newresourcehotupdate, Action<bool> completecallback)
    {
        Debug.Log("doResourceHotUpdate()");
        mResourceHotUpdateCompleteCB = completecallback;
        //拉取服务器热更资源信息与本地资源热更信息进行比较
        TWebRequest twr = new TWebRequest();
        //URL = 基础URL + 当前版本号 + "/" + 热更资源MD5信息文件名(AssetBundleMD5.txt)
        var url = mHotUpdateURL + ServerVersionConfig.VersionCode + "/" + ServerVersionConfig.ResourceVersionCode + "/" + AssetBundleMD5FileName;
        mHotResourceUpdateRequest.resetRequest();
        twr.enqueue(url, null, resourceMD5InfoHotUpdateCompleteCB);
        twr.startRequest();
    }

    /// <summary>
    /// 热更资源列表下载完成回调
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="fileMd5">文件MD5</param>
    /// <param name="downloadhandler">下载结果数据</param>
    /// <param name="requeststatus">下载状态</param>
    private void resourceMD5InfoHotUpdateCompleteCB(string url, string fileMd5, DownloadHandler downloadhandler, TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus)
    {
        Debug.Log(string.Format("热更资源热更信息地址 : {0}", url));
        if (requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
        {
            Debug.Log(string.Format("热更资源MD5信息文件下载成功!热更资源MD5信息文件大小: {0}KB", downloadhandler.data.Length / 1024));
            mNeedHotUpdateAssetBundleInfoMap.Clear();
            mHotUpdateResourceTotalNumber = 0;
            //TODO:
            //结合服务器上的资源MD5信息做资源验证，确保资源下对以及未被修改
            var hotUpdateAssetBundleInfoMap = new Dictionary<string, HotUpdateAssetBundleInfo>();
            var hotupdateresourcesinfo = downloadhandler.text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                foreach (var hotupdatereousrceinfo in hotupdateresourcesinfo)
                {
                    //避免最后添加;后导致多一个空数据报错
                    if(hotupdatereousrceinfo.IsNullOrEmpty())
                    {
                        continue;
                    }
                    var resourceinfo = hotupdatereousrceinfo.Split(ResourceConstData.AssetBundlleInfoSeparater);
                    var resPath = resourceinfo[0];
                    var resMD5 = resourceinfo[1];
                    if (!hotUpdateAssetBundleInfoMap.ContainsKey(resPath))
                    {
                        hotUpdateAssetBundleInfoMap.Add(resPath, new HotUpdateAssetBundleInfo(resPath, resMD5));
                    }
                    else
                    {
                        Debug.LogError($"重复的资源热更信息!资源路径:{resPath}资源MD5:{resMD5}，请检查流程！");
                    }
                }
                //根据返回的热更资源数据结合本地资源版本号以及已经热更下载的资源计算出剩下需要热更的资源数据
                var currentresversion = VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode;
                foreach (var hotUpdateAssetBunfleInfo in hotUpdateAssetBundleInfoMap)
                {
                    var assetBundlePath = hotUpdateAssetBunfleInfo.Value.AssetBundlePath;
                    var hotUpdateAssetBundleInfo = hotUpdateAssetBunfleInfo.Value;
                    if (isAssetBundleNeedHotUpdate(hotUpdateAssetBundleInfo))
                    {
                        if (!mNeedHotUpdateAssetBundleInfoMap.ContainsKey(assetBundlePath))
                        {
                            mNeedHotUpdateAssetBundleInfoMap.Add(assetBundlePath, hotUpdateAssetBundleInfo);
                            mHotUpdateResourceTotalNumber++;
                        }
                        else
                        {
                            Debug.LogError($"有重复的热更新资源路径:{assetBundlePath}，检查流程!");
                        }
                    }
                }                
                Debug.Log(string.Format("需要热更的资源数 : {0}", mHotUpdateResourceTotalNumber));
                foreach (var needhotupdateresinfo in mNeedHotUpdateAssetBundleInfoMap)
                {
                    Debug.Log($"需要热更的资源路径:{needhotupdateresinfo.Value.AssetBundlePath}资源MD5:{needhotupdateresinfo.Value.AssetBundleMD5}");
                }

                if(mHotUpdateResourceTotalNumber > 0)
                {
                    //开始资源热更
                    //检查资源热更目录，不存在就创建一个
                    AssetBundlePath.CheckAndCreateABOutterPathFolder();
                    //检查资源热更列表信息目录
                    if (!Directory.Exists(LocalAssetBundleMD5FilFolderPath))
                    {
                        Directory.CreateDirectory(LocalAssetBundleMD5FilFolderPath);
                        Debug.Log(string.Format("创建目录 : {0}", LocalAssetBundleMD5FilFolderPath));
                    }
                    foreach (var resinfo in mNeedHotUpdateAssetBundleInfoMap)
                    {
                        //URL = 基础URL + 当前版本号 + "/" + 需要热更的资源版本号 + "/" + 需要热更的资源名
                        var finalurl = mHotUpdateURL + ServerVersionConfig.VersionCode + "/" + ServerVersionConfig.ResourceVersionCode + "/" + resinfo.Value.AssetBundlePath;
                        mHotResourceUpdateRequest.enqueue(finalurl, resinfo.Value.AssetBundleMD5, singleResourceHotUpdateCompleteCB);
                    }
                    mHotResourceUpdateRequest.startRequest();
                }
                else
                {
                    Debug.Log("没有资源需要热更，直接进入游戏!");
                    mResourceHotUpdateCompleteCB(true);
                    mResourceHotUpdateCompleteCB = null;
                }
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("热更资源异常 : {0}！", e.Message));
                Debug.LogError("热更资源失败!");
                mHotResourceUpdateRequest.stopRequest();
                mResourceHotUpdateCompleteCB(false);
                mResourceHotUpdateCompleteCB = null;
            }
            finally
            {

            }
        }
        else
        {
            Debug.LogError("热更资源列表下载失败!");
            mResourceHotUpdateCompleteCB(false);
            mResourceHotUpdateCompleteCB = null;
        }
    }

    /// <summary>
    /// 指定AssetBundle热更信息是否需要热更
    /// </summary>
    /// <param name="hotUpdateAssetBundleInfo"></param>
    /// <returns></returns>
    private bool isAssetBundleNeedHotUpdate(HotUpdateAssetBundleInfo hotUpdateAssetBundleInfo)
    {
        if (!mLocalHotUpdateAssetBundleInfoMap.ContainsKey(hotUpdateAssetBundleInfo.AssetBundlePath)
            || (mLocalHotUpdateAssetBundleInfoMap.ContainsKey(hotUpdateAssetBundleInfo.AssetBundlePath) && mLocalHotUpdateAssetBundleInfoMap[hotUpdateAssetBundleInfo.AssetBundlePath].AssetBundleMD5 != hotUpdateAssetBundleInfo.AssetBundleMD5))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 单个资源热更下载完成回调
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="fileMd5">文件MD5</param>
    /// <param name="downloadhandler">下载结果信息</param>
    /// <param name="requeststatus">请求结果</param>
    private void singleResourceHotUpdateCompleteCB(string url, string fileMd5, DownloadHandler downloadhandler, TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus)
    {
        Debug.Log(string.Format("资源 : {0}下载返回！", url));
        var hotUpdateVersionCode = ServerVersionConfig.VersionCode;
        var hotUpdateResourceVersionCode = ServerVersionConfig.ResourceVersionCode;
        var hotUpdatePreURL = $"{mHotUpdateURL}{hotUpdateVersionCode}/{hotUpdateResourceVersionCode}/";
        var resPath = url.Replace(hotUpdatePreURL, string.Empty);
        if(requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
        {
            Debug.Log($"资源版本号:{hotUpdateResourceVersionCode}资源路径:{resPath}热更下载成功！");
            // 存储热更资源到包外并记录热更资源信息
            saveHotResourceUpdate(resPath, fileMd5, downloadhandler.data);
            mNeedHotUpdateAssetBundleInfoMap.Remove(resPath);
            Debug.Log(string.Format("当前资源热更进度 : {0}", HotResourceUpdateProgress));
            if(mNeedHotUpdateAssetBundleInfoMap.Count == 0)
            {
                Debug.Log("资源热更完成!");
                mResourceHotUpdateCompleteCB(true);
                mResourceHotUpdateCompleteCB = null;
                VersionConfigModuleManager.Singleton.saveNewResoueceCodeOuterConfig(ServerVersionConfig.ResourceVersionCode);
            }
        }
        else
        {
            Debug.LogError(string.Format("资源 : {0}热更下载失败！", resPath));
            mHotResourceUpdateRequest.stopRequest();
            mResourceHotUpdateCompleteCB(false);
            mResourceHotUpdateCompleteCB = null;
        }
    }

    /// <summary>
    /// 存储热更资源并记录热更资源信息
    /// </summary>
    /// <param name="resPath">资源名</param>
    /// <param name="resMd5">资源MD5</param>
    /// <param name="data">资源二进制数据</param>
    private void saveHotResourceUpdate(string resPath, string resMd5, byte[] data)
    {
        //检查包外是否存在同名资源，存在的话需要先删除再存储最新到包外
        var resfullpath = AssetBundlePath.ABHotUpdatePath + resPath;
        if (AssetBundlePath.IsABExitInOutterPath(resPath))
        {
            Debug.Log(string.Format("删除包外资源 : {0}", resPath));
            File.Delete(resfullpath);
        }
        var fileFolderPath = Path.GetDirectoryName(resfullpath);
        FolderUtilities.CheckAndCreateSpecificFolder(fileFolderPath);
        File.WriteAllBytes(resfullpath, data);

        if(!File.Exists(LocalAssetBundleMD5FilePath))
        {
            Debug.Log(string.Format("创建文件 : {0}", LocalAssetBundleMD5FilePath));
            using (var fs = File.Create(LocalAssetBundleMD5FilePath))
            {
                fs.Close();
            }
        }
        using (var sw = File.AppendText(LocalAssetBundleMD5FilePath))
        {
            sw.WriteLine($"{resPath}{ResourceConstData.AssetBundlleInfoSeparater}{resMd5}");
            sw.Close();
        }
        Debug.Log(string.Format("写入已更资源 : {0}", resfullpath));
    }
#endregion
}