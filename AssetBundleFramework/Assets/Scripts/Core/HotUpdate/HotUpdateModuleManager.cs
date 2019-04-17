/*
 * Description:             HotUpdateModuleManager.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/14
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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
    /// 资源热更测试地址
    /// </summary>
    private const string TestResourceURL = "http://192.168.1.4/AssetBundleFramework/";

    /// <summary>
    /// 测试资源热更列表
    /// </summary>
    private List<string> TestHotUpdateResourceList;

    private void Awake()
    {
        HotUpdateSwitch = true;
        TestHotUpdateResourceList = new List<string>();
        TestHotUpdateResourceList.Add("Android");
        TestHotUpdateResourceList.Add("tutorialcellspritesheet");
    }

    /// <summary>
    /// 开启资源热更
    /// </summary>
    public void startResourceHotUpdate()
    {
        if(HotUpdateSwitch)
        {
            AssetBundlePath.CheckOrCreateABOutterPathFolder();
            StartCoroutine(resourcesRequest());
        }
    }

    /// <summary>
    /// 资源请求携程
    /// </summary>
    /// <returns></returns>
    private IEnumerator resourcesRequest()
    {
        foreach(var hotupdateres in TestHotUpdateResourceList)
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
                    var outterabfullpath = AssetBundlePath.GetABOutterPath() + hotupdateres;
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