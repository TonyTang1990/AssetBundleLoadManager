/*
 * Description:             HotUpdateModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/14
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
// 14. 拉去服务器对应版本最新的资源信息校验文件(VerifyABInfo.json)校验是否需要下载最新的资源信息文件(ABInfo.json)
// 15. 下载最新的资源信息文件(ABInfo.json)会存储到Stage临时目录
// 16. 使用资源信息文件ABInfo.json数据与本地资源数据比较，得出需要热更的资源列表
// 17. 热更资源先下载到Stage临时目录，然后利用ABInfo.json里的数据校(文件大小，SHA256等)验热更资源的完整性和正确性
// 18. 热更资源完整性和正确性校验通过后移动到包外热更资源目录
// 19. 所有资源热更新完成后，将资源信息文件(ABInfo.json)移动到包外，同时更新本地资源版本号到包外，至此完成热更(等玩家第二次进游戏生效)
// 20. 不需要资源热更
// 21. 进入游戏

/// 热更包外目录结构:
/// PersistentAsset/HotUpdate/Platform(资源热更新目录)
/// PersistentAsset/Config/VersionConfig.json(包外版本信息--用于进游戏前强更和热更判定)
/// PersistentAsset/HotUpdate/Donwload/版本强更包
/// PersistentAsset/Stage/(资源热更新临时目录)

namespace TResource
{
    /// <summary>
    /// HotUpdateModuleManager.cs
    /// 热更模块(含资源和版本更新)
    /// Note:
    /// 服务器那一块待开发，所以先默认有资源可以更新，暂时只做资源热更下载这一块
    /// </summary>
    public class HotUpdateModuleManager : SingletonBase<HotUpdateModuleManager>
    {
        /// <summary>
        /// 热更新资源目录名
        /// </summary>
        private const string HotUpdateFolderName = "HotUpdate";

        /// <summary>
        /// 版本强更目录名
        /// </summary>
        private const string ForceUpdateFolderName = "Download";

        /// <summary>
        /// 热更开关
        /// </summary>
        public bool HotUpdateSwitch
        {
            get;
            set;
        }

        /// <summary>
        /// 热更新状态
        /// </summary>
        public HotUpdateStatus HotUpdateStatus
        {
            get;
            private set;
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
        /// 热更缓存目录
        /// </summary>
        public string VersionHotUpdateFolderPath
        {
            get;
            private set;
        }

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
        private string mVersionHotUpdateFileName = string.Empty;

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
        public float ResHotUpdateProgress
        {
            get
            {
                return mResHotUpdateRequest.CurrentProgress;
            }
        }

        /// <summary>
        /// 需要资源热更的单个AB信息Map
        /// Key为资源路径，Value为该资源单个AB最新热更信息
        /// </summary>
        private Dictionary<string, HotUpdateSingleABInfo> mNeedHotUpdateSingleABInfoMap;

        /// <summary>
        /// 需要资源热更的资源总数
        /// </summary>
        private int mHotUpdateResTotalNumber;

        /// <summary>
        /// 资源热更失败的资源个数
        /// </summary>
        private int mHotUpdateResFailedNumber;

        /// <summary>
        /// 资源更新完成回调
        /// </summary>
        private Action<HotUpdateResult> mResHotUpdateCompleteCB;

        /// <summary>
        /// 资源热更版本号
        /// </summary>
        private double mResHotUpdateVersionCode;
        
        /// <summary>
        /// 资源热更版本号
        /// </summary>
        private int mResHotUpdateResourceVersionCode;

        /// <summary>
        /// 热更校验资源信息
        /// </summary>
        private HotUpdateVerifyABInfo mHotUpdateVerifyABInfo;

        /// <summary>
        /// 热更资源信息
        /// </summary>
        private HotUpdateABInfo mHotUpdateABInfo;

        /// <summary>
        /// 临时ABInfo.json文件绝对路径
        /// </summary>
        private string mABInfoFileStageFullPath;

        /// <summary>
        /// 本地AB热更信息Map
        /// Key为资源路径，Value为该资源当前热更最新资源信息(优先包外)
        /// </summary>
        private Dictionary<string, HotUpdateSingleABInfo> mLocalHotUpdateSingleABInfoMap;

        /// <summary>
        /// 资源热更请求
        /// </summary>
        private TWebRequest mResHotUpdateRequest;
        
        /// <summary>
        /// UTF8编码
        /// </summary>
        private UTF8Encoding mUTF8Encoding = new UTF8Encoding(true);

        /// <summary>
        /// Sha256哈希算法
        /// </summary>
        private SHA256 mSha256Hash = SHA256.Create();

        public HotUpdateModuleManager()
        {
            HotUpdateSwitch = true;

            HotUpdateStatus = HotUpdateStatus.NotStart;

            mHotUpdateConfigFilePath = Path.Combine(ConfigFolderPath, mVersionConfigFileName);
            mHotUpdateConfig = null;
            mHotUpdateURL = string.Empty;

            mHotUpdateResTotalNumber = 0;
            mHotUpdateResFailedNumber = 0;
            VersionHotUpdateFolderPath = Path.Combine(Application.persistentDataPath, HotUpdateFolderName);
            VersionHotUpdateCacheFolderPath = Path.Combine(VersionHotUpdateFolderPath, ForceUpdateFolderName);
            
            mVersionHotUpdateFileName = string.Empty;
            HotVersionUpdateRequest = new TWebRequest();
            mVersionHotUpdateCompleteCB = null;

            mNeedHotUpdateSingleABInfoMap = new Dictionary<string, HotUpdateSingleABInfo>();
            mResHotUpdateCompleteCB = null;
            mResHotUpdateVersionCode = 0;
            mResHotUpdateResourceVersionCode = 0;
            mLocalHotUpdateSingleABInfoMap = new Dictionary<string, HotUpdateSingleABInfo>();
            mResHotUpdateRequest = new TWebRequest();
        }

        /// <summary>
        /// 热更模块初始化
        /// </summary>
        public void Init()
        {
            InitHotUpdateConfig();
            InitLocalABHotUpdateInfo();
        }

        /// <summary>
        /// 初始化热更新地址信息
        /// </summary>
        private void InitHotUpdateConfig()
        {
            mHotUpdateConfig = null;
            Debug.Log($"mHotUpdateConfigFilePath:{mHotUpdateConfigFilePath}");
            //读取包内的热更信息
            var hotupdateconfigasset = Resources.Load<TextAsset>(mHotUpdateConfigFilePath);
            if (hotupdateconfigasset != null)
            {
                Debug.Log("热更新地址信息信息:");
                var content = hotupdateconfigasset.text;
                Debug.Log($"content:{content}");
                mHotUpdateConfig = JsonUtility.FromJson<HotUpdateConfig>(content);
                Debug.Log($"APKName:{mHotUpdateConfig.APKName} HotUpdateURL:{mHotUpdateConfig.HotUpdateURL}");
                mVersionHotUpdateFileName = mHotUpdateConfig.APKName;
                VersionHotUpdateCacheFilePath = VersionHotUpdateCacheFolderPath + mVersionHotUpdateFileName;
                if(GameConfigModuleManager.Singleton.IsInnerDevelopMode())
                {
    #if UNITY_EDITOR
                    var localHotUpdateFolderPath = HotUpdatePath.GetLocalHotUpdateFolderPath();
                    mHotUpdateURL = $"file://{localHotUpdateFolderPath}/";
    #endif
                }
                else if(GameConfigModuleManager.Singleton.IsReleaseMode())
                {
                    mHotUpdateURL = mHotUpdateConfig.HotUpdateURL;
                }
            }
            else
            {
                Debug.LogError(string.Format("包内热更地址信息文件 : {0}不存在!", mHotUpdateConfigFilePath));
            }
        }

        /// <summary>
        /// 初始化本地以资源热更信息
        /// </summary>
        private void InitLocalABHotUpdateInfo()
        {
            Debug.Log("初始化本地资源热更列表!");
            // 但是为了性能考虑，不采用实时算包外Sha256，直接信任包外ABInfo.json
            // 后续每次热更还会单独拉去最新的ABInfo.json，即使被串改，也会根据最新的ABInfo.json进行热更新覆盖
            mLocalHotUpdateSingleABInfoMap.Clear();
            var outterABInfoFileFullPath = ResourcePath.GetOutterABInfoFilePath();
            HotUpdateABInfo hotUpdateABInfo;
            if(!File.Exists(outterABInfoFileFullPath))
            {
                Debug.Log($"使用包内ABInfo.json文件初始化本地热更信息!");
                var innerABInfoFileResRelativePathNoE = ResourcePath.GetInnerABInfoFileResRelativePathNoE();
                var innerABInfoAsset = Resources.Load<TextAsset>(innerABInfoFileResRelativePathNoE);
                var innerABInfoFileContent = innerABInfoAsset.text;
                hotUpdateABInfo = JsonUtility.FromJson<HotUpdateABInfo>(innerABInfoFileContent);
            }
            else
            {
                Debug.Log($"使用包外ABInfo.json文件初始化本地热更信息!");
                var outterABInfoFileContent = File.ReadAllText(outterABInfoFileFullPath);
                hotUpdateABInfo = JsonUtility.FromJson<HotUpdateABInfo>(outterABInfoFileContent);
            }
            if(hotUpdateABInfo == null)
            {
                Debug.LogError($"初始化本地资源热更列表失败,hotUpdateABInfo为null!");
                return;
            }
            foreach (var hotUpdateSingleABInfo in hotUpdateABInfo.HotUpdateSingleABInfoList)
            {
                if(hotUpdateSingleABInfo == null)
                {
                    Debug.LogWarning($"初始化热更信息失败,hotUpdateSingleABInfo为null!跳过该条信息!");
                    continue;
                }
                AddOrUpdateHotUpdateSimpleABInfo(hotUpdateSingleABInfo);
            }
            var hotUpdateSingleABInfoNum = hotUpdateABInfo.HotUpdateSingleABInfoList?.Count ?? 0;
            Debug.Log($"AB信息文件数量:{hotUpdateSingleABInfoNum}加载成功!");
        }

        /// <summary>
        /// 添加单个AB的热更AB信息
        /// Note:
        /// 后写入的同名Key的Value覆盖之前的值
        /// </summary>
        /// <param name="hotUpdateSingleABInfo">单个AB热更信息</param>
        /// <param name="md5"></param>
        private bool AddOrUpdateHotUpdateSimpleABInfo(HotUpdateSingleABInfo hotUpdateSingleABInfo)
        {
            if(hotUpdateSingleABInfo == null)
            {
                Debug.LogError($"添加热更单个AB简要信息失败,hotUpdateSimpleABInfo为null!");
                return false;
            }
            var abRelativePath = hotUpdateSingleABInfo.ABRelativePath;
            if (!mLocalHotUpdateSingleABInfoMap.ContainsKey(abRelativePath))
            {
                mLocalHotUpdateSingleABInfoMap.Add(abRelativePath, hotUpdateSingleABInfo);
            }
            else
            {
                mLocalHotUpdateSingleABInfoMap[abRelativePath] = hotUpdateSingleABInfo;
            }
            return true;
        }

        /// <summary>
        /// 是否处于下载状态中
        /// </summary>
        /// <returns></returns>
        private bool IsUnderHotUpdatingStatus()
        {
            return HotUpdateStatus == HotUpdateStatus.VersionDownloading ||
                   HotUpdateStatus == HotUpdateStatus.VerifyABInfoDownloading ||
                   HotUpdateStatus == HotUpdateStatus.ABInfoDownloading ||
                   HotUpdateStatus == HotUpdateStatus.ResourceDownloading;
        }

    #region 服务器版本资源信息拉去部分
        /// <summary>
        /// 执行获取服务器版本信息
        /// </summary>
        /// <returns></returns>
        public void DoObtainServerVersionConfig(Action<bool> completecallback)
        {
            mServerVersionConfigHotUpdateCompleteCB = completecallback;
            //拉取服务器热更资源信息与本地资源热更信息进行比较
            TWebRequest twr = new TWebRequest();
            //URL = 热更新URL + 服务器版本信息文件名(ServerVersionConfig.json)
            var url = Utilities.CombineUrl(mHotUpdateURL, ServerVersionConfigFileName);
            twr.Enqueue(url, null, ServerVersionConfigCompleteCB);
            twr.StartRequest();
        }

        /// <summary>
        /// 服务器版本信息拉去回调
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="fileMd5">文件MD5</param>
        /// <param name="downloadhandler">下载结果数据</param>
        /// <param name="requeststatus">下载状态</param>
        /// <param name="customData">自定义数据</param>
        private void ServerVersionConfigCompleteCB(string url, string fileMd5, DownloadHandler downloadhandler,
                                                TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus,
                                                object customData)
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
        public bool CheckHasVersionHotUpdate()
        {
            Debug.Log("checkHasVersionHotUpdate()");
            if (VersionConfigModuleManager.Singleton.HasVersionHotUpdate())
            {
                Debug.Log("强更过版本！清除包外目录！");
                // 清空包外目录
                HotUpdateUtilities.DeleteAllOutterHotUpdateResources();
                VersionConfigModuleManager.Singleton.InitVerisonConfigData();
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
        public bool CheckVersionHotUpdate(double newhotupdateversioncode)
        {
            Debug.Log("checkVersionHotUpdate()");
            if (HotUpdateSwitch)
            {
                if (VersionConfigModuleManager.Singleton.NeedVersionHotUpdate(newhotupdateversioncode))
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
        /// <param name="newHotUpdateVersionCode">新的强更版本号</param>
        /// <param name="completecallback">强更完成回调</param>
        public void DoNewVersionHotUpdate(double newHotUpdateVersionCode, Action<bool> completecallback)
        {
            Debug.Log("doNewVersionHotUpdate()");
            if(IsUnderHotUpdatingStatus())
            {
                Debug.Log($"当前处于热更新下载状态:{HotUpdateStatus}，不允许进行版本强更！");
                return;
            }
            HotUpdateStatus = HotUpdateStatus.VersionDownloading;
            //写入当前版本信息到包外
            VersionConfigModuleManager.Singleton.SaveNewVersionCodeOuterConfig(VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode);
            //引导版本强更
            mVersionHotUpdateCompleteCB = completecallback;
            HotVersionUpdateRequest.ResetRequest();
            var versionHotUpdateFilePath = Utilities.CombineUrl(mHotUpdateURL, newHotUpdateVersionCode.ToString(), mVersionHotUpdateFileName);
            HotVersionUpdateRequest.Enqueue(versionHotUpdateFilePath, null, VersionHotUpdateCompleteCB, 1800);
            HotVersionUpdateRequest.StartRequest();
        }

        /// <summary>
        /// 版本强更下载完成回调
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="fileMd5">文件MD5</param>
        /// <param name="downloadhandler">下载结果数据</param>
        /// <param name="requeststatus">下载状态</param>
        /// <param name="customData">自定义数据</param>
        private void VersionHotUpdateCompleteCB(string url, string fileMd5, DownloadHandler downloadhandler,
                                                TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus,
                                                object customData)
        {
            Debug.Log(string.Format("版本强更资源地址 : {0}", url));
            if (requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
            {
                Debug.Log("版本强更资源下载成功!版本强更资源信息");
                try
                {
                    // TODO: 校验版本强更的完整性
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
                        OnVersionHotUpdateSuccess();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(string.Format("版本强更资源异常 : {0}！", e.Message));
                    Debug.LogError("版本强更失败!");
                    OnVersionHotUpdateFailed();
                }
                finally
                {
                    mVersionHotUpdateCompleteCB = null;
                }
            }
            else
            {
                OnVersionHotUpdateFailed();
            }
        }

        /// <summary>
        /// 版本强更下载成功
        /// </summary>
        private void OnVersionHotUpdateSuccess()
        {
            Debug.Log($"版本强更下载成功!安装包缓存路径:{VersionHotUpdateCacheFilePath}");
            HotUpdateStatus = HotUpdateStatus.VersionDownloadSuccess;
            mVersionHotUpdateCompleteCB(true);
            mVersionHotUpdateCompleteCB = null;
        }

        /// <summary>
        /// 版本强更下载失败
        /// </summary>
        private void OnVersionHotUpdateFailed()
        {
            Debug.LogError("版本强更下载失败!");
            HotUpdateStatus = HotUpdateStatus.VersionDownloadFailed;
            mVersionHotUpdateCompleteCB(false);
            mVersionHotUpdateCompleteCB = null;
        }
    #endregion

    #region 资源热更新部分
        /// <summary>
        /// 尝试资源热更新
        /// </summary>
        /// <param name="completeCb">资源热更新完成回调(不需要热更也会回调)</param>
        /// <returns></returns>
        public bool TryResourceHotUpdate(Action<HotUpdateResult> completeCb = null)
        {
            if (ServerVersionConfig != null)
            {
                if (CheckResourceHotUpdate(ServerVersionConfig.ResourceVersionCode))
                {
                    DoResourceHotUpdate(ServerVersionConfig.VersionCode,
                                        ServerVersionConfig.ResourceVersionCode,
                                        completeCb);
                    return true;
                }
                HotUpdateStatus = HotUpdateStatus.ResourceHotUpdateSuccess;
                completeCb?.Invoke(HotUpdateResult.NoNeedHotUpdate);
                return false;
            }
            DIYLog.LogError("资源热更新前，请先获取服务器版本信息!");
            HotUpdateStatus = HotUpdateStatus.ResourceDownloadFailed;
            completeCb?.Invoke(HotUpdateResult.UnderError);
            return false;
        }

        /// <summary>
        /// 检查资源热更
        /// </summary>
        /// <param name="newHotUpdateResourceCode">新的资源版本号</param>
        private bool CheckResourceHotUpdate(int newHotUpdateResourceCode)
        {
            Debug.Log("CheckResourceHotUpdate()");
            if (HotUpdateSwitch)
            {
                if (VersionConfigModuleManager.Singleton.NeedResourceHotUpdate(newHotUpdateResourceCode))
                {
                    Debug.Log(string.Format("服务器资源版本号 : {0}大于本地资源版本号 : {1}，需要资源热更！", newHotUpdateResourceCode, VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode));
                    return true;
                }
                else
                {
                    Debug.Log(string.Format("服务器资源版本号 : {0}小于或等于本地资源版本号 : {1}，不需要资源热更！", newHotUpdateResourceCode, VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode));
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
        /// <param name="versionCode"></param>
        /// <param name="resourceVersionCode"></param>
        /// <param name="completecallback"></param>
        private void DoResourceHotUpdate(double versionCode, int resourceVersionCode, Action<HotUpdateResult> completecallback)
        {
            Debug.Log("doResourceHotUpdate()");
            // 先拉去SimpleABInfo.json文件
            // 然后读取包外热更资源ABInfo.json文件比较Sha256看是否需要重新拉去ABInfo.json文件
            // 如果需要拉ABInfo.json(先存储到临时目录)并验证
            // ABInfo.json验证通过后移动到包外热更资源目录，然后再开始热更新判定
            // 如果不需要拉ABInfo.json则直接使用包外ABInfo.json进行后续热更新判定
            mResHotUpdateCompleteCB = completecallback;
            mResHotUpdateVersionCode = versionCode;
            mResHotUpdateResourceVersionCode = resourceVersionCode;
            DoVerifyABInfoHotUpdate();
            //拉取服务器热更资源信息与本地资源热更信息进行比较
        }

        /// <summary>
        /// 执行资源热更校验资源信息文件拉取
        /// </summary>
        /// <param name="versionCode"></param>
        /// <param name="resourceVersionCode"></param>
        private void DoVerifyABInfoHotUpdate()
        {
            Debug.Log("doVerifyABInfoHotUpdate()");
            HotUpdateStatus = HotUpdateStatus.VerifyABInfoDownloading;
            TWebRequest twr = new TWebRequest();
            //URL = 基础URL + 当前版本号 + "/" + 热更资源信息文件名(VerifyABInfo.json)
            var url = GetResHotUpdateURL(mResHotUpdateVersionCode, mResHotUpdateResourceVersionCode, ResourceConstData.VerifyABInfoFileName);
            mResHotUpdateRequest.ResetRequest();
            twr.Enqueue(url, null, OnVerifyABInfoHotUpdateCompleteCB);
            twr.StartRequest();
        }

        /// <summary>
        /// 响应热更校验资源信息文件下载完成回调
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="fileMd5">文件MD5</param>
        /// <param name="downloadhandler">下载结果数据</param>
        /// <param name="requeststatus">下载状态</param>
        private void OnVerifyABInfoHotUpdateCompleteCB(string url, string fileMd5, DownloadHandler downloadhandler,
                                                       TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus,
                                                       object customData)
        {
            Debug.Log(string.Format("热更校验资源信息地址 : {0}", url));
            if (requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
            {
                Debug.Log(string.Format("热更校验资源信息文件下载成功!热更校验资源信息文件大小: {0}KB", downloadhandler.data.Length / 1024));
                // VerifyABInfo.json每次都重新下载确保验证本地ABInfo.json的完整性和正确性
                var hotUpdateVerifyABInfoFileContent = downloadhandler.text;
                mHotUpdateVerifyABInfo = JsonUtility.FromJson<HotUpdateVerifyABInfo>(hotUpdateVerifyABInfoFileContent);
                try
                {
                    if(mHotUpdateVerifyABInfo != null)
                    {
                        Debug.Log("热更校验资源信息文件解析成功!");
                        OnVerifyABInfoDownloadSuccess();
                    }
                    else
                    {
                        Debug.LogError("热更校验资源信息文件解析失败!");
                        OnVerifyABInfoDownloadFailed();
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(string.Format("热更资源异常 : {0}！", e.Message));
                    Debug.LogError("热更校验资源信息文件失败!");
                    OnVerifyABInfoDownloadFailed();
                }
                finally
                {

                }
            }
            else
            {
                Debug.LogError("热更校验资源信息文件下载失败!");
                OnVerifyABInfoDownloadFailed();
            }
        }
        
        /// <summary>
        /// 响应热更校验资源信息文件下载成功回调
        /// </summary>
        private void OnVerifyABInfoDownloadSuccess()
        {
            Debug.Log("热更校验资源信息文件下载成功!");
            HotUpdateStatus = HotUpdateStatus.VerifyABInfoDownloadSuccess;
            DoABInfoHotUpdate();
        }

        /// <summary>
        /// 响应热更校验资源信息文件下载失败回调
        /// </summary>
        private void OnVerifyABInfoDownloadFailed()
        {
            Debug.LogError("热更校验资源信息文件下载失败!");
            HotUpdateStatus = HotUpdateStatus.VerifyABInfoDownloadFailed;
            mHotUpdateVerifyABInfo = null;
            mResHotUpdateCompleteCB?.Invoke(HotUpdateResult.Failed);
            mResHotUpdateCompleteCB = null;
        }
    
        /// <summary>
        /// 执行资源热更资源信息文件拉取
        /// </summary>
        /// <param name="versionCode"></param>
        /// <param name="resourceVersionCode"></param>
        private void DoABInfoHotUpdate()
        {
            Debug.Log("DoABInfoHotUpdate()");
            HotUpdateStatus = HotUpdateStatus.ABInfoDownloading;
            TWebRequest twr = new TWebRequest();
            //URL = 基础URL + 当前版本号 + "/" + 热更资源信息文件名(ABInfo.json)
            var url = GetResHotUpdateURL(mResHotUpdateVersionCode, mResHotUpdateResourceVersionCode, ResourceConstData.ABInfoFileName);
            mResHotUpdateRequest.ResetRequest();
            twr.Enqueue(url, mHotUpdateVerifyABInfo.ABInfoFileSha256, OnABInfoHotUpdateCompleteCB, ResourceConstData.ABInfoFileName);
            twr.StartRequest();
        }

        /// <summary>
        /// 响应热更资源信息文件下载完成回调
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="fileSha256">文件Sha256</param>
        /// <param name="downloadhandler">下载结果数据</param>
        /// <param name="requeststatus">下载状态</param>
        private void OnABInfoHotUpdateCompleteCB(string url, string fileSha256, DownloadHandler downloadhandler,
                                                TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus,
                                                object customData)
        {
            Debug.Log(string.Format("热更资源信息地址 : {0}", url));
            if (requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
            {
                Debug.Log(string.Format("热更资源MD5信息文件下载成功!热更资源MD5信息文件大小: {0}KB", downloadhandler.data.Length / 1024));
                // 存储ABInfo.json数据到临时目录
                // 然后验证ABInfo.json文件正确性和完整性
                // 验证通过移动开始资源热更新
                // 等所有资源热更新完成后再将ABInfo.json移动到包外热更目录
                var hotUpdateABInfoFileContent = downloadhandler.text;
                mHotUpdateABInfo = JsonUtility.FromJson<HotUpdateABInfo>(hotUpdateABInfoFileContent);
                try
                {
                    if(mHotUpdateABInfo != null)
                    {
                        Debug.Log("热更资源信息文件解析成功!");
                        var abInfoFileRelativePath = customData as string;
                        // 存储热更资源信息到包外临时资源目录
                        mABInfoFileStageFullPath = SaveHotUpdateResToStageFolder(abInfoFileRelativePath, downloadhandler.data);
                        // 校验热更资源文件正确性
                        (var result, var resSha256) = FileUtilities.CheckFileSha256(mABInfoFileStageFullPath, fileSha256, mSha256Hash);
                        if(!result)
                        {
                            Debug.LogError($"热更资源信息 : {mABInfoFileStageFullPath}的文件Sha256校验失败，下载资源Sha256:{resSha256}，目标Sha256:{fileSha256}！");
                            OnABInfoHotUpdateFailed();
                            return;
                        }
                        OnABInfoHotUpdateSuccess();
                    }
                    else
                    {
                        Debug.LogError("热更资源信息文件解析失败!");
                        OnABInfoHotUpdateFailed();
                    }
                }
                catch(Exception e)
                {
                    Debug.LogError(string.Format("热更资源异常 : {0}！", e.Message));
                    Debug.LogError("热更资源失败!");
                    OnABInfoHotUpdateFailed();
                }
                finally
                {

                }
            }
            else
            {
                Debug.LogError("热更资源信息下载失败!");
                OnABInfoHotUpdateFailed();
            }
        }

        /// <summary>
        /// 响应热更资源信息文件下载成功回调
        /// </summary>
        private void OnABInfoHotUpdateSuccess()
        {
            Debug.Log("热更资源信息文件下载成功!");
            HotUpdateStatus = HotUpdateStatus.ABInfoDownloadSuccess;
            DoResourceNeedHotUpdate();
        }

        /// <summary>
        /// 响应热更资源信息文件下载失败回调
        /// </summary>
        private void OnABInfoHotUpdateFailed()
        {
            Debug.LogError("热更资源信息文件下载失败!");
            HotUpdateStatus = HotUpdateStatus.ABInfoDownloadFailed;
            mABInfoFileStageFullPath = null;
            mResHotUpdateCompleteCB?.Invoke(HotUpdateResult.Failed);
            mResHotUpdateCompleteCB = null;
        }

        /// <summary>
        /// 执行资源需要的热更
        /// </summary>
        private void DoResourceNeedHotUpdate()
        {
            Debug.Log("开始资源热更新！");
            if(mHotUpdateABInfo == null)
            {
                Debug.LogError("热更资源信息文件异常为空，无法进行资源热更!");
                OnResourceHotUpdateFailed();
                return;
            }
            HotUpdateStatus = HotUpdateStatus.ResourceDownloading;
            mNeedHotUpdateSingleABInfoMap.Clear();
            mHotUpdateResTotalNumber = 0;
            mHotUpdateResFailedNumber = 0;
            var hotUpdateSingleABInfoMap = new Dictionary<string, HotUpdateSingleABInfo>(); 
            foreach (var hotUpdateSingleABInfo in mHotUpdateABInfo.HotUpdateSingleABInfoList)
            {
                if(hotUpdateSingleABInfo == null)
                {
                    Debug.LogWarning("热更资源信息文件中有空的单个AB热更信息，跳过!");
                    continue;
                }
                var abRelativePath = hotUpdateSingleABInfo.ABRelativePath;
                if (!hotUpdateSingleABInfoMap.ContainsKey(abRelativePath))
                {
                    hotUpdateSingleABInfoMap.Add(abRelativePath, hotUpdateSingleABInfo);
                }
                else
                {
                    Debug.LogError($"重复的资源热更信息!资源路径:{abRelativePath}，请检查流程！");
                }
            }
            //根据返回的热更资源数据结合已经热更下载的资源计算出剩下需要热更的资源数据
            foreach (var hotUpdateSingleABInfoPairs in hotUpdateSingleABInfoMap)
            {
                var hotUpdateSingleABInfo = hotUpdateSingleABInfoPairs.Value;
                if (IsAssetBundleNeedHotUpdate(hotUpdateSingleABInfo))
                {
                    var abRelativePath = hotUpdateSingleABInfo.ABRelativePath;
                    if (!mNeedHotUpdateSingleABInfoMap.ContainsKey(abRelativePath))
                    {
                        mNeedHotUpdateSingleABInfoMap.Add(abRelativePath, hotUpdateSingleABInfo);
                        mHotUpdateResTotalNumber++;
                    }
                    else
                    {
                        Debug.LogError($"有重复的热更新资源路径:{abRelativePath}，检查流程!");
                    }
                }
            }                
            Debug.Log(string.Format("需要热更的资源数 : {0}", mHotUpdateResTotalNumber));
            foreach (var needHotUpdateSingleABInfoPairs in mNeedHotUpdateSingleABInfoMap)
            {
                var needHotUpdateSingleABInfo = needHotUpdateSingleABInfoPairs.Value;
                Debug.Log($"需要热更的资源路径:{needHotUpdateSingleABInfo.ABRelativePath}资源Sha256:{needHotUpdateSingleABInfo.ABSha256}");
            }

            if(mHotUpdateResTotalNumber > 0)
            {
                //开始资源热更
                //检查资源热更目录和临时资源目录，不存在就创建一个
                ResourcePath.CheckAndCreateABHotUpdateFolder();
                ResourcePath.CheckAndCreateABHotUpdateStageFolder();
                foreach (var needHotUpdateSingleABInfoPairs in mNeedHotUpdateSingleABInfoMap)
                {
                    //URL = 基础URL + 当前版本号 + "/" + 需要热更的资源版本号 + "/" + 需要热更的资源名
                    var needHotUpdateSingleABInfo = needHotUpdateSingleABInfoPairs.Value;
                    var abRealRelativePath = needHotUpdateSingleABInfo.GetABRelativePathWithMD5();
                    var finalurl = GetResHotUpdateURL(ServerVersionConfig.VersionCode, ServerVersionConfig.ResourceVersionCode, abRealRelativePath);
                    mResHotUpdateRequest.Enqueue(finalurl, needHotUpdateSingleABInfo.ABSha256,
                                                OnSingleResHotUpdateCompleteCB, needHotUpdateSingleABInfo);
                }
                mResHotUpdateRequest.StartRequest();
            }
            else
            {
                Debug.Log("没有资源需要热更，直接进入游戏!");
                OnResourceHotUpdateComplete();
            }
        }

        /// <summary>
        /// 指定单个AB信息是否需要热更
        /// </summary>
        /// <param name="hotUpdateSingleABInfo"></param>
        /// <returns></returns>
        private bool IsAssetBundleNeedHotUpdate(HotUpdateSingleABInfo hotUpdateSingleABInfo)
        {
            if(hotUpdateSingleABInfo == null)
            {
                Debug.LogError("判定单个AB是否需要热更失败,hotUpdateSingleABInfo为null!");
                return false;
            }
            var abRelativePath = hotUpdateSingleABInfo.ABRelativePath;
            if(mLocalHotUpdateSingleABInfoMap.TryGetValue(abRelativePath, out var localHotUpdateABSimpleInfo))
            {
                return !string.Equals(localHotUpdateABSimpleInfo.ABSha256, hotUpdateSingleABInfo.ABSha256);
            }
            // 找不到说明是新资源文件需要热更新
            return true;
        }

        /// <summary>
        /// 响应单个资源热更下载完成回调
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="fileSha256">文件Sha256</param>
        /// <param name="downloadhandler">下载结果信息</param>
        /// <param name="requeststatus">请求结果</param>
        /// <param name="customData">自定义数据</param>
        private void OnSingleResHotUpdateCompleteCB(string url, string fileSha256, DownloadHandler downloadhandler,
                                                    TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus,
                                                    object customData)
        {
            Debug.Log(string.Format("资源 : {0}下载返回！", url));
            Debug.Log(string.Format("当前资源热更进度 : {0}", ResHotUpdateProgress));
            var hotUpdateSingleABInfo = customData as HotUpdateSingleABInfo;
            var resRelativePath = hotUpdateSingleABInfo.ABRelativePath;
            var resRelativePathWithMD5 = hotUpdateSingleABInfo.GetABRelativePathWithMD5();
            if(requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
            {
                Debug.Log($"资源路径:{resRelativePathWithMD5}热更下载成功！");
                // 存储热更资源到包外临时资源目录
                var stageResPath = SaveHotUpdateResToStageFolder(resRelativePathWithMD5, downloadhandler.data);
                // 校验热更资源文件正确性
                (var result, var resSha256) = FileUtilities.CheckFileSha256(stageResPath, fileSha256, mSha256Hash);
                if(!result)
                {
                    Debug.LogError($"热更资源 : {resRelativePathWithMD5}的文件Sha256校验失败，下载资源Sha256:{resSha256}，目标Sha256:{fileSha256}！");
                    OnSingleResHotUpdateFailed(resRelativePath);
                    return;
                }
                // 校验成功后移动到包外资源热更目录
                var moveResult = MoveStageResToOuterResFolder(stageResPath);
                if(!moveResult)
                {
                    Debug.LogError($"热更资源 : {resRelativePathWithMD5}的文件移动到包外资源热更目录失败！");
                    OnSingleResHotUpdateFailed(resRelativePath);
                    return;
                }
                OnSingleResHotUpdateSuccess(resRelativePath);
            }
            else
            {
                OnSingleResHotUpdateFailed(resRelativePath);
            }
        }

        /// <summary>
        /// 移动Stage目录资源到包外热更资源目录
        /// </summary>
        /// <param name="stageResFullPath"></param>
        /// <returns></returns>
        private bool MoveStageResToOuterResFolder(string stageResFullPath)
        {
            var stageFileInfo = new FileInfo(stageResFullPath);
            if(!stageFileInfo.Exists)
            {
                Debug.LogError($"移动Stage目录资源到包外热更资源目录失败，Stage目录资源:{stageResFullPath}不存在！");
                return false;
            }
            var regularStageResFullPath = PathUtilities.GetRegularPath(stageResFullPath);
            var regularStageFolderFullPath = PathUtilities.GetRegularPath(ResourcePath.ABHotUpdateStagePath);
            if(!regularStageResFullPath.StartsWith(regularStageFolderFullPath))
            {
                Debug.LogError($"移动Stage目录资源到包外热更资源目录失败，Stage目录资源:{regularStageResFullPath}不在Stage目录:{regularStageFolderFullPath}下！");
                return false;
            }
            var stageResRelativePath = Path.GetRelativePath(regularStageFolderFullPath, regularStageResFullPath);
            var outterResFullPath = ResourcePath.GetOutterFileFullPath(stageResRelativePath);
            try
            {
                var outterResFolderFullPath = Path.GetDirectoryName(outterResFullPath);
                FolderUtilities.CheckAndCreateSpecificFolder(outterResFolderFullPath);
                // 如果目标文件存在先删除
                if(File.Exists(outterResFullPath))
                {
                    File.Delete(outterResFullPath);
                }
                File.Move(regularStageResFullPath, outterResFullPath);
                Debug.Log($"文件移动成功：{regularStageResFullPath} -> {outterResFullPath}");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"文件移动失败：{regularStageResFullPath} -> {outterResFullPath}\n{exception}");
                return false;
            }
        }

        /// <summary>
        /// 响应单个资源热更新成功
        /// </summary>
        /// <param name="resRelativePath"></param>
        private void OnSingleResHotUpdateSuccess(string resRelativePath)
        {
            Debug.Log(string.Format("资源 : {0}热更下载成功！", resRelativePath));
            mNeedHotUpdateSingleABInfoMap.Remove(resRelativePath);
            // 检查热更新完成
            CheckResHotUpdateAllComplete();
        }

        /// <summary>
        /// 响应单个资源热更新失败
        /// </summary>
        /// <param name="resRelativePath"></param>
        private void OnSingleResHotUpdateFailed(string resRelativePath)
        {
            Debug.LogError(string.Format("资源 : {0}热更下载失败！", resRelativePath));
            // 如果有资源下载失败先默认当做下载完成，下一次进游戏重新触发下载
            // TODO: 未来可以做N次重新下载机制，这里为了简单先默认等第二次进游戏再重新触发下载
            mNeedHotUpdateSingleABInfoMap.Remove(resRelativePath);
            mHotUpdateResFailedNumber++;
            // 检查热更新完成
            CheckResHotUpdateAllComplete();
        }

        /// <summary>
        /// 响应资源热更新完成
        /// </summary>
        private void OnResourceHotUpdateComplete()
        {
            Debug.Log($"资源热更完成，最新版本号:{ServerVersionConfig.VersionCode}，资源版本号:{ServerVersionConfig.ResourceVersionCode}");
            Debug.LogWarning($"资源热更完成后要重启游戏才生效！");
            if(mHotUpdateResFailedNumber > 0)
            {
                Debug.LogError($"热更资源下载失败数量:{mHotUpdateResFailedNumber}");
                HotUpdateStatus = HotUpdateStatus.ResourceDownloadFailed;
            }
            else
            {
                // 只有所有热更资源都成功才能算作热更正确完成
                Debug.Log($"所有资源热更成功，最新资源版本号:{ServerVersionConfig.ResourceVersionCode}");
                HotUpdateStatus = HotUpdateStatus.ResourceHotUpdateSuccess;
                // 只有所有热更资源都成功ABInfo.json才允许移动到包外资源热更目录
                var moveResult = MoveStageResToOuterResFolder(mABInfoFileStageFullPath);
                if(!moveResult)
                {
                    Debug.LogError($"热更资源信息 : {mABInfoFileStageFullPath}的文件移动到包外资源热更目录失败！");
                }
                else
                {
                    // 所有资源热更都成功+ABInfo.json移动成功才敢切换版本号启用最新资源
                    VersionConfigModuleManager.Singleton.SaveNewResoueceCodeOuterConfig(ServerVersionConfig.ResourceVersionCode);
                }
            }
            // 有资源下载失败也要算完成，不能影响玩家进游戏，下一次进游戏再尝试下载即可
            mNeedHotUpdateSingleABInfoMap?.Clear();
            mHotUpdateResTotalNumber = 0;
            mHotUpdateResFailedNumber = 0;
            mResHotUpdateVersionCode = 0;
            mResHotUpdateResourceVersionCode = 0;
            mHotUpdateVerifyABInfo = null;
            mHotUpdateABInfo = null;
            mABInfoFileStageFullPath = null;
            mABInfoFileStageFullPath = null;
            mResHotUpdateRequest?.StopRequest();
            mResHotUpdateCompleteCB?.Invoke(HotUpdateResult.Complete);
            mResHotUpdateCompleteCB = null;
        }

        /// <summary>
        /// 响应资源热更新失败
        /// </summary>
        private void OnResourceHotUpdateFailed()
        {
            Debug.Log($"新版本号:{ServerVersionConfig.VersionCode}，资源版本号:{ServerVersionConfig.ResourceVersionCode}的资源热更失败！");
            HotUpdateStatus = HotUpdateStatus.ResourceHotUpdateFailed;
            mResHotUpdateRequest?.StopRequest();
            mResHotUpdateCompleteCB?.Invoke(HotUpdateResult.Failed);
            mResHotUpdateCompleteCB = null;
        }
        
        /// <summary>
        /// 检查资源热更是否全部完成
        /// </summary>
        /// <returns></returns>
        private bool CheckResHotUpdateAllComplete()
        {
            if(mNeedHotUpdateSingleABInfoMap.Count == 0)
            {
                Debug.Log("所有资源热更完成!");
                OnResourceHotUpdateComplete();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 存储热更资源并记录热更资源信息
        /// </summary>
        /// <param name="resPath">资源名</param>
        /// <param name="data">资源二进制数据</param>
        private string SaveHotUpdateResToStageFolder(string resPath, byte[] data)
        {
            //检查包外临时目录是否存在同名资源，存在的话需要先删除再存储最新到包外临时目录
            var resStageFullPath = Path.Combine(ResourcePath.ABHotUpdateStagePath, resPath);
            if (ResourcePath.IsABExitInOutterStagePath(resPath))
            {
                Debug.Log(string.Format("删除包外临时目录资源 : {0}", resPath));
                File.Delete(resStageFullPath);
            }
            var fileFolderPath = Path.GetDirectoryName(resStageFullPath);
            FolderUtilities.CheckAndCreateSpecificFolder(fileFolderPath);
            File.WriteAllBytes(resStageFullPath, data);
            Debug.Log($"热更资源:{resStageFullPath}保存完毕！");
            return resStageFullPath;
        }

        /// <summary>
        /// 获取指定版本号和资源版本号的热更URL
        /// </summary>
        /// <param name="versionCode"></param>
        /// <param name="resVersionCode"></param>
        /// <returns></returns>
        private string GetHotUpdateURL(double versionCode, int resVersionCode)
        {
            var versionCodeS = versionCode.ToString();
            var resourceVersionCodeS = resVersionCode.ToString();
            return Utilities.CombineUrl(mHotUpdateURL, versionCodeS, resourceVersionCodeS);
        }

        /// <summary>
        /// 获取指定版本号，资源版本号和资源相对路径的热更URL
        /// </summary>
        /// <param name="versionCode"></param>
        /// <param name="resVersionCode"></param>
        /// <param name="resRelativePath"></param>
        /// <returns></returns>
        private string GetResHotUpdateURL(double versionCode, int resVersionCode, string resRelativePath)
        {
            var hotUpdateURL = GetHotUpdateURL(versionCode, resVersionCode);
            return Utilities.CombineUrl(hotUpdateURL, resRelativePath);
        }
    #endregion
    }
}