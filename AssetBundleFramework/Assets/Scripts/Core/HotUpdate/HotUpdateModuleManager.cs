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
// 1. 检测是否需要版本强更
// 2. 需要版本强更 -> 4
// 3. 不需要版本强更 -> 6
// 4. 版本强更，清空包外信息版本信息以及热更资源以及热更资源列表信息
// 5. 重启游戏
// 6. 检测是否需要资源热更
// 7. 拉去服务器资源热更列表信息与本地资源版本号以及已经热更到的资源信息做比较，得出需要热更的资源列表
// 8. 开始资源热更，存储热更资源到包外目录
// 9. 资源热更完成，删除本地资源热更列表信息记录文件
// 10. 使用最新资源进入游戏

/// <summary>
/// HotUpdateModuleManager.cs
/// 热更模块(含资源和版本更新)
/// Note:
/// 服务器那一块待开发，所以先默认有资源可以更新，暂时只做资源热更下载这一块
/// </summary>
public class HotUpdateModuleManager : SingletonMonoBehaviourTemplate<HotUpdateModuleManager>, IModuleInterface
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
    /// </summary>
    private const double HotUpdateVersionCode = 2.0;

    /// <summary>
    /// 测试热更资源版本号
    /// </summary>
    private const int HotUpdateResourceCode = 2;

    /// <summary>
    /// 资源热更测试地址
    /// </summary>
    private const string TestResourceURL = "http://192.168.1.4/AssetBundleFramework/";

    /// <summary>
    /// 测试资源热更列表
    /// </summary>
    private List<string> mTestHotUpdateResourceList;

    /// <summary>
    /// 资源热更进度
    /// </summary>
    private float mHotUpdateResourceProgress;

    /// <summary>
    /// 资源热更是否完成
    /// </summary>
    private bool mIsHotUpdateResourceComplete;

    /// <summary>
    /// 资源热更结果映射Map
    /// Key为资源名，Value为资源热更结果
    /// </summary>
    private Dictionary<string, bool> mHotUpdateResourceResultMap;

    /// <summary>
    /// 版本更新完成回调
    /// </summary>
    private Action<double, bool> mVersionHotUpdateCompleteCB;

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
    /// 本地资源热更记录文件路径
    /// Note:
    /// 所有资源热更完成后内容置空
    /// 没有热更完的话会记录已经更到的资源列表
    /// </summary>
    private readonly string LocalResourceUpdateListFilePath = Application.persistentDataPath + ResourceUpdateListFileName;

    /// <summary>
    /// 热更模块初始化
    /// </summary>
    public void init()
    {
        HotUpdateSwitch = true;
        mTestHotUpdateResourceList = new List<string>();
        mTestHotUpdateResourceList.Add("Android");
        mTestHotUpdateResourceList.Add("tutorialcellspritesheet");
        mHotUpdateResourceProgress = 0.0f;
        mIsHotUpdateResourceComplete = false;
        mHotUpdateResourceResultMap = new Dictionary<string, bool>();
        mVersionHotUpdateCompleteCB = null;
        mResourceHotUpdateCompleteCB = null;
        mResourceUpdateListMap = new Dictionary<int, List<string>>();
        mLocalResourceUpdateList = new List<string>();

        initLocalResourceUpdateList();
    }

    /// <summary>
    /// 检查版本强更
    /// </summary>
    /// <param name="completecallback"></param>
    public void checkVersionHotUpdate(Action<double, bool> completecallback)
    {
        if (HotUpdateSwitch)
        {
            mVersionHotUpdateCompleteCB = completecallback;
            if (VersionConfigManager.Singleton.needVersionHotUpdate(HotUpdateVersionCode))
            {
                DIYLog.Log(string.Format("服务器版本号 : {0}高于本地版本号 : {1}，需要强更！", HotUpdateVersionCode, VersionConfigManager.Singleton.GameVersionConfig.VersionCode));
                //TODO:强更代码
                mVersionHotUpdateCompleteCB(HotUpdateVersionCode, true);
            }
            else
            {
                DIYLog.Log(string.Format("服务器版本号 : {0}小于或等于本地版本号 : {1}，不需要强更！", HotUpdateVersionCode, VersionConfigManager.Singleton.GameVersionConfig.VersionCode));
                mVersionHotUpdateCompleteCB(VersionConfigManager.Singleton.GameVersionConfig.VersionCode, true);
            }
            mVersionHotUpdateCompleteCB = null;
        }
    }

    /// <summary>
    /// 检查资源热更
    /// </summary>
    /// <param name="completecallback">资源热更完成回调</param>
    public void checkResourceHotUpdate(Action<int, bool> completecallback)
    {
        if(HotUpdateSwitch)
        {
            mResourceHotUpdateCompleteCB = completecallback;
            if (VersionConfigManager.Singleton.needResourceHotUpdate(HotUpdateResourceCode))
            {
                //拉取服务器热更资源信息与本地资源热更信息进行比较
                TWebRequest twr = new TWebRequest();
                var url = TestResourceURL + ResourceUpdateListFileName;
                twr.enqueue(url, resourceListHotUpdateCompleteCB);
                twr.startRequest();
            }
            else
            {
                DIYLog.Log(string.Format("服务器资源版本号 : {0}小于或等于本地资源版本号 : {1}，不需要资源热更！", HotUpdateResourceCode, VersionConfigManager.Singleton.GameVersionConfig.ResourceVersionCode));
                mResourceHotUpdateCompleteCB(VersionConfigManager.Singleton.GameVersionConfig.ResourceVersionCode, true);
            }
        }
    }

    /// <summary>
    /// 初始化本地资源热更列表
    /// </summary>
    private void initLocalResourceUpdateList()
    {
        DIYLog.Log("初始化本地资源热更列表!");
        if(File.Exists(LocalResourceUpdateListFilePath))
        {
            var resourceupdateinfo = File.ReadAllText(LocalResourceUpdateListFilePath);
            var resources = resourceupdateinfo.Split('+');
            mLocalResourceUpdateList.AddRange(resources);
            foreach(var updatedresource in mLocalResourceUpdateList)
            {
                DIYLog.Log(string.Format("已经更新到的资源 : {0}", updatedresource));
            }
        }
        else
        {
            DIYLog.Log(string.Format("{0}文件不存在，未更新任何资源!", LocalResourceUpdateListFilePath));
        }
    }

    /// <summary>
    /// 热更资源列表下载完成回调
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requeststatus"></param>
    private void resourceListHotUpdateCompleteCB(string url, TWebRequest.WebRequestStatus requeststatus)
    {
        //TODO:
        if (mTestHotUpdateResourceList.Count > 0)
        {
            AssetBundlePath.CheckAndCreateABOutterPathFolder();
            //StartCoroutine(resourcesRequest());
            TWebRequest twr = new TWebRequest();
            foreach (var res in mTestHotUpdateResourceList)
            {
                var finalurl = TestResourceURL + res;
                twr.enqueue(finalurl, singleResourceHotUpdateCompleteCB);
            }
            twr.startRequest();
        }
        else
        {
            DIYLog.Log("没有热更资源需要下载！资源热更完成！");
            mResourceHotUpdateCompleteCB(HotUpdateResourceCode, true);
        }
    }


    /// <summary>
    /// 单个资源热更下载完成回调
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requeststatus"></param>
    private void singleResourceHotUpdateCompleteCB(string url, TWebRequest.WebRequestStatus requeststatus)
    {
        //mHotUpdateResourceProgress = 
    }

    /// <summary>
    /// 资源请求携程
    /// </summary>
    /// <returns></returns>
    private IEnumerator resourcesRequest()
    {
        foreach(var hotupdateres in mTestHotUpdateResourceList)
        {
            var resurl = TestResourceURL + hotupdateres;
            ResourceLogger.log(string.Format("下载资源 : {0}", resurl));
            var webrequest = UnityWebRequest.Get(resurl);
            yield return webrequest.SendWebRequest();
            if(webrequest.isNetworkError)
            {
                ResourceLogger.logErr(string.Format("{0}资源下载出错!", hotupdateres));
                ResourceLogger.logErr(webrequest.error);
            }
            else
            {
                if(webrequest.isDone)
                {
                    ResourceLogger.log(string.Format("{0}资源下载完成!", hotupdateres));
                    var data = webrequest.downloadHandler.data;
                    //检查包外是否存在同名资源，存在的话需要先删除再存储最新到包外
                    var outterabfullpath = AssetBundlePath.ABHotUpdatePath + hotupdateres;
                    if (AssetBundlePath.IsABExitInOutterPath(hotupdateres))
                    {
                        ResourceLogger.log(string.Format("删除包外资源 : {0}", hotupdateres));
                        File.Delete(outterabfullpath);
                    }
                    using (var fs = File.Create(outterabfullpath))
                    {
                        fs.Write(data, 0, data.Length);
                        fs.Flush();
                        fs.Close();
                        ResourceLogger.log(string.Format("包外资源 : {0}写入完成!", hotupdateres));
                    }
                }
            }
        }
    }
}