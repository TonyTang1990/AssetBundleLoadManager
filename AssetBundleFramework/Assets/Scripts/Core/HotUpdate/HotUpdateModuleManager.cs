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
// 14. 拉去服务器资源热更列表信息与本地资源版本号以及已经热更到的资源信息做比较，得出需要热更的资源列表  -> 1
// 15. 不需要资源热更
// 16. 进入游戏

/// <summary>
/// HotUpdateModuleManager.cs
/// 热更模块(含资源和版本更新)
/// Note:
/// 服务器那一块待开发，所以先默认有资源可以更新，暂时只做资源热更下载这一块
/// </summary>
public class HotUpdateModuleManager : SingletonTemplate<HotUpdateModuleManager>, IModuleInterface
{
    /// <summary>
    /// 真实模块名
    /// </summary>
    public string ModuleName
    {
        get
        {
            return this.GetType().ToString();
        }
    }

    /// <summary>
    /// 热更开关
    /// </summary>
    public bool HotUpdateSwitch
    {
        get;
        set;
    }

    /// <summary>
    /// 测试版本强更版本号
    /// 待添加网络模块后从服务器端读取
    /// </summary>
    public const double NewHotUpdateVersionCode = 2.0;

    /// <summary>
    /// 测试热更资源版本号
    /// 待添加网络模块后从服务器端读取
    /// </summary>
    public const int NewHotUpdateResourceCode = 6;

    /// <summary>
    /// 资源热更进度
    /// </summary>
    public float HotUpdateResourceProgress
    {
        get;
        private set;
    }

    /// <summary>
    /// 资源热更测试地址
    /// </summary>
    private const string TestResourceURL = "http://192.168.1.4/AssetBundleFramework/";

    /// <summary>
    /// 需要资源热更列表
    /// </summary>
    private List<string> mHotUpdateResourceList;

    /// <summary>
    /// 需要资源热更的资源总数
    /// </summary>
    private int mHotUpdateResourceTotalNumber;

    /// <summary>
    /// 资源热更结果映射Map
    /// Key为资源名，Value为资源热更结果
    /// </summary>
    private Dictionary<string, bool> mHotUpdateResourceResultMap;

    /// <summary>
    /// 资源更新完成回调
    /// </summary>
    private Action<int, bool> mResourceHotUpdateCompleteCB;

    /// <summary>
    /// 服务器资源热更列表映射Map
    /// Key为资源版本号，Value为该资源版本号需要热更的资源
    /// </summary>
    private Dictionary<int, List<string>> mResourceUpdateListMap;

    /// <summary>
    /// 本地已经热更到的资源列表(热更完毕后会清空)
    /// </summary>
    private List<string> mLocalResourceUpdateList;

    /// <summary>
    /// 资源热更列表文件名
    /// </summary>
    private const string ResourceUpdateListFileName = "ResourceUpdateList.txt";

    /// <summary>
    /// 本地资源热更记录文件目录路径
    /// </summary>
    private string LocalResourceUpdateListFilFolderPath;

    /// <summary>
    /// 本地资源热更记录文件路径
    /// Note:
    /// 所有资源热更完成后内容置空
    /// 没有热更完的话会记录已经更到的资源列表
    /// </summary>
    private readonly string LocalResourceUpdateListFilePath;

    /// <summary>
    /// 资源热更请求
    /// </summary>
    private TWebRequest mHotResourceUpdateRequest;

    public HotUpdateModuleManager()
    {
        HotUpdateSwitch = true;

        mHotUpdateResourceList = new List<string>();
        mHotUpdateResourceTotalNumber = 0;
        HotUpdateResourceProgress = 0.0f;
        mHotUpdateResourceResultMap = new Dictionary<string, bool>();
        mResourceHotUpdateCompleteCB = null;
        mResourceUpdateListMap = new Dictionary<int, List<string>>();
        mLocalResourceUpdateList = new List<string>();
        LocalResourceUpdateListFilFolderPath = Application.persistentDataPath + "/ResourceUpdateList/";
        LocalResourceUpdateListFilePath = LocalResourceUpdateListFilFolderPath + ResourceUpdateListFileName;
        mHotResourceUpdateRequest = new TWebRequest();
    }

    /// <summary>
    /// 热更模块初始化
    /// </summary>
    public void init()
    {
        initLocalResourceUpdateList();
    }

    /// <summary>
    /// 初始化本地资源热更列表
    /// </summary>
    private void initLocalResourceUpdateList()
    {
        Debug.Log("初始化本地资源热更列表!");
        mLocalResourceUpdateList.Clear();
        if (File.Exists(LocalResourceUpdateListFilePath))
        {
            var resourceupdateinfo = File.ReadAllLines(LocalResourceUpdateListFilePath);
            mLocalResourceUpdateList.AddRange(resourceupdateinfo);
            foreach (var updatedresource in mLocalResourceUpdateList)
            {
                Debug.Log(string.Format("已经更新到的资源 : {0}", updatedresource));
            }
        }
        else
        {
            Debug.Log(string.Format("{0}文件不存在，未更新任何资源!", LocalResourceUpdateListFilePath));
        }
    }

    /// <summary>
    /// 检查是否已经版本强更完成
    /// </summary>
    /// <returns></returns>
    public bool checkHasVersionHotUpdate()
    {
        if (VersionConfigModuleManager.Singleton.hasVersionHotUpdate())
        {
            Debug.Log("强更过版本！清除包外目录！");
            //清空包外目录
            if (Directory.Exists(AssetBundlePath.ABHotUpdatePath))
            {
                Directory.Delete(AssetBundlePath.ABHotUpdatePath, true);
                Debug.Log(string.Format("删除包外资源热更存储目录 : {0}", AssetBundlePath.ABHotUpdatePath));
            }
            if (Directory.Exists(LocalResourceUpdateListFilFolderPath))
            {
                Directory.Delete(LocalResourceUpdateListFilFolderPath, true);
                Debug.Log(string.Format("删除包外热更资源信息目录 : {0}", LocalResourceUpdateListFilFolderPath));
            }
            if (Directory.Exists(VersionConfigModuleManager.Singleton.OutterVersionConfigSaveFileFolderPath))
            {
                Directory.Delete(VersionConfigModuleManager.Singleton.OutterVersionConfigSaveFileFolderPath, true);
                Debug.Log(string.Format("删除包外版本信息存储目录 : {0}", VersionConfigModuleManager.Singleton.OutterVersionConfigSaveFileFolderPath));
            }
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
    /// <param name="completecallback">强更回调</param>
    public void checkVersionHotUpdate(double newhotupdateversioncode, Action<bool> completecallback)
    {
        if (HotUpdateSwitch)
        {
            if (VersionConfigModuleManager.Singleton.needVersionHotUpdate(newhotupdateversioncode))
            {
                Debug.Log(string.Format("服务器版本号 : {0}高于本地版本号 : {1}，需要强更！", newhotupdateversioncode, VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode));
                //TODO:
                //写入当前版本信息到包外
                VersionConfigModuleManager.Singleton.saveNewVersionCodeConfig(VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode);
                //引导版本强更
                //****
                completecallback(true);
            }
            else
            {
                Debug.Log(string.Format("服务器版本号 : {0}小于或等于本地版本号 : {1}，不需要强更！", newhotupdateversioncode, VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode));
                completecallback(false);
            }
        }
        else
        {
            Debug.Log("热更开关未打开，不允许版本强更！");
            completecallback(true);
        }
    }

    /// <summary>
    /// 检查资源热更
    /// </summary>
    /// <param name="newhotupdateresourcecode">新的资源版本号</param>
    /// <param name="completecallback">资源热更完成回调</param>
    public void checkResourceHotUpdate(int newhotupdateresourcecode, Action<int, bool> completecallback)
    {
        if(HotUpdateSwitch)
        {
            mResourceHotUpdateCompleteCB = completecallback;
            if (VersionConfigModuleManager.Singleton.needResourceHotUpdate(newhotupdateresourcecode))
            {
                //拉取服务器热更资源信息与本地资源热更信息进行比较
                TWebRequest twr = new TWebRequest();
                var url = TestResourceURL + ResourceUpdateListFileName;
                twr.enqueue(url, resourceListHotUpdateCompleteCB);
                twr.startRequest();
            }
            else
            {
                Debug.Log(string.Format("服务器资源版本号 : {0}小于或等于本地资源版本号 : {1}，不需要资源热更！", newhotupdateresourcecode, VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode));
                mResourceHotUpdateCompleteCB(VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode, true);
                mResourceHotUpdateCompleteCB = null;
            }
        }
        else
        {
            Debug.Log("热更开关未打开，不允许热更资源！");
            mResourceHotUpdateCompleteCB(VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode, false);
            mResourceHotUpdateCompleteCB = null;
        }
    }

    /// <summary>
    /// 热更资源列表下载完成回调
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="downloadhandler">下载结果数据</param>
    /// <param name="requeststatus">下载状态</param>
    private void resourceListHotUpdateCompleteCB(string url, DownloadHandler downloadhandler, TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus)
    {
        Debug.Log(string.Format("热更资源列表地址 : {0}", url));
        if (requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
        {
            Debug.Log(string.Format("热更资源列表下载成功!热更资源列表信息 : {0}", downloadhandler.text));
            mHotUpdateResourceList.Clear();
            mHotUpdateResourceTotalNumber = 0;
            mHotResourceUpdateRequest.resetRequest();

            var hotupdateresourcesmap = new SortedDictionary<int, List<string>>();
            var hotupdateresourcesinfo = downloadhandler.text.Split(';');
            try
            {
                foreach (var hotupdatereousrceinfo in hotupdateresourcesinfo)
                {
                    var resourceinfo = hotupdatereousrceinfo.Split(':');
                    var resversion = int.Parse(resourceinfo[0]);
                    var resname = resourceinfo[1];
                    List<string> reslist;
                    if (!hotupdateresourcesmap.TryGetValue(resversion, out reslist))
                    {
                        reslist = new List<string>();
                        hotupdateresourcesmap.Add(resversion, reslist);
                    }
                    if (!reslist.Contains(resname))
                    {
                        reslist.Add(resname);
                        Debug.Log(string.Format("添加需要热更的资源信息,版本号 : {0}, 资源名 : {1}", resversion, resname));
                    }
                    else
                    {
                        Debug.LogError(string.Format("重复的资源热更信息！版本号 : {0} 资源名 : {1}！", resversion, resname));
                    }
                }
                //根据返回的热更资源数据结合本地资源版本号以及已经热更下载的资源计算出剩下需要热更的资源数据
                var currentresversion = VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode;
                foreach (var hotupdateresourceinfo in hotupdateresourcesmap)
                {
                    if (hotupdateresourceinfo.Key > currentresversion)
                    {
                        mHotUpdateResourceList.AddRange(hotupdateresourceinfo.Value);
                    }
                }
                mHotUpdateResourceTotalNumber = mHotUpdateResourceList.Count;
                Debug.Log(string.Format("需要热更的资源数 : {0}", mHotUpdateResourceTotalNumber));
                foreach (var hotupdateres in mHotUpdateResourceList)
                {
                    Debug.Log(string.Format("需要热更的资源名 : {0}", hotupdateres));
                }

                //开始资源热更
                //检查资源热更目录，不存在就创建一个
                AssetBundlePath.CheckAndCreateABOutterPathFolder();
                //检查资源热更列表信息目录
                if (!Directory.Exists(LocalResourceUpdateListFilFolderPath))
                {
                    Directory.CreateDirectory(LocalResourceUpdateListFilFolderPath);
                    Debug.Log(string.Format("创建目录 : {0}", LocalResourceUpdateListFilFolderPath));
                }

                //StartCoroutine(resourcesRequest());
                foreach (var res in mHotUpdateResourceList)
                {
                    var finalurl = TestResourceURL + res;
                    mHotResourceUpdateRequest.enqueue(finalurl, singleResourceHotUpdateCompleteCB);
                }
                mHotResourceUpdateRequest.startRequest();
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("热更资源异常 : {0}！", e.Message));
                Debug.LogError("热更资源失败!");
                mHotResourceUpdateRequest.stopRequest();
                mResourceHotUpdateCompleteCB(NewHotUpdateResourceCode, false);
                mResourceHotUpdateCompleteCB = null;
            }
            finally
            {

            }
        }
        else
        {
            Debug.LogError("热更资源列表下载失败!");
            mResourceHotUpdateCompleteCB(NewHotUpdateResourceCode, false);
            mResourceHotUpdateCompleteCB = null;
        }
    }

    /// <summary>
    /// 单个资源热更下载完成回调
    /// </summary>
    /// <param name="url">下载地址</param>
    /// <param name="downloadhandler">下载结果信息</param>
    /// <param name="requeststatus">请求结果</param>
    private void singleResourceHotUpdateCompleteCB(string url, DownloadHandler downloadhandler, TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus)
    {
        Debug.Log(string.Format("资源 : {0}下载返回！", url));
        var resname = Path.GetFileName(url);
        if(requeststatus == TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete)
        {
            Debug.Log(string.Format("资源 : {0}热更下载成功！", resname));
            //存储热更资源到包外并记录热更资源信息
            saveHotResourceUpdate(resname, downloadhandler.data);
            mHotUpdateResourceList.Remove(resname);
            HotUpdateResourceProgress = (mHotUpdateResourceTotalNumber - mHotUpdateResourceList.Count) * 1.0f / mHotUpdateResourceTotalNumber;
            Debug.Log(string.Format("当前资源热更进度 : {0}", HotUpdateResourceProgress));
            if(mHotUpdateResourceList.Count == 0)
            {
                Debug.Log("资源热更完成!");
                mResourceHotUpdateCompleteCB(NewHotUpdateResourceCode, true);
                mResourceHotUpdateCompleteCB = null;
                VersionConfigModuleManager.Singleton.saveNewResoueceCodeConfig(NewHotUpdateResourceCode);
            }
        }
        else
        {
            Debug.LogError(string.Format("资源 : {0}热更下载失败！", resname));
            mHotResourceUpdateRequest.stopRequest();
            mResourceHotUpdateCompleteCB(NewHotUpdateResourceCode, false);
            mResourceHotUpdateCompleteCB = null;
        }
    }

    /// <summary>
    /// 存储热更资源并记录热更资源信息
    /// </summary>
    /// <param name="resname"></param>
    /// <param name="data"></param>
    private void saveHotResourceUpdate(string resname, byte[] data)
    {
        //检查包外是否存在同名资源，存在的话需要先删除再存储最新到包外
        var resfullpath = AssetBundlePath.ABHotUpdatePath + resname;
        if (AssetBundlePath.IsABExitInOutterPath(resname))
        {
            Debug.Log(string.Format("删除包外资源 : {0}", resname));
            File.Delete(resfullpath);
        }
        File.WriteAllBytes(resfullpath, data);

        if(!File.Exists(LocalResourceUpdateListFilePath))
        {
            Debug.Log(string.Format("创建文件 : {0}", LocalResourceUpdateListFilePath));
            using (var fs = File.Create(LocalResourceUpdateListFilePath))
            {
                fs.Close();
            }
        }
        using (var sw = File.AppendText(LocalResourceUpdateListFilePath))
        {
            sw.WriteLine(resname);
            sw.Close();
        }
        Debug.Log(string.Format("写入已更资源 : {0}", resfullpath));
    }
}