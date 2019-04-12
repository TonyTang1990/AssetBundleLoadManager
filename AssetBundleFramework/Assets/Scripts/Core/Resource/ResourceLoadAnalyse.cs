/*
 * Description:             ResourceLoadAnalyse.cs
 * Author:                  TonyTang
 * Create Date:             2018/12/29
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// ResourceLoadAnalyse.cs
/// 资源加载统计分析工具
/// </summary>
public class ResourceLoadAnalyse : SingletonTemplate<ResourceLoadAnalyse> {

    /// <summary> 资源加载卸载信息抽象 /// </summary>
    public class ResourceLoadInfo
    {
        /// <summary> AB名字 /// </summary>
        public string ABName { get; set; }

        /// <summary> AB加载次数 /// </summary>
        public int ResourceLoadTimeCount { get; set; }

        /// <summary> 资源卸载次数 /// </summary>
        public int ResourceUnloadTimeCount { get; set; }

        /// <summary> 资源第一次被加载的时间 /// </summary>
        public string ResourceFirstLoadTime { get; set; }

        /// <summary> 资源最后一次被卸载的时间 /// </summary>
        public string ResourceLastUnloadTime { get; set; }
    }

    /// <summary> 资源加载统计开关 /// </summary>
    public bool ResourceLoadAnalyseSwitch { get; set; }

    /// <summary> 资源加载分析开始标志 /// </summary>
    public bool ResourceLoadAnalyseStart { get; set; }

    /// <summary>
    /// 资源加载统计分析映射Map
    /// Key为AB名，Value为对应资源加载卸载详细信息
    /// </summary>
    public Dictionary<string, ResourceLoadInfo> ResourceLoadAnalyseMap { get; set; }

    /// <summary>
    /// AB加载详细信息文件输出目录全路径
    /// </summary>
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private string ABLoadedInfoOutputFolderPath = Application.dataPath + "/..";
#elif UNITY_ANDROID
    private string ABLoadedInfoOutputFolderPath = Application.persistentDataPath;
#elif UNITY_IOS
    private string ABLoadedInfoOutputFolderPath = Application.temporaryCachePath;
#endif

    public ResourceLoadAnalyse()
    {
        ResourceLoadAnalyseSwitch = false;
        ResourceLoadAnalyseStart = false;
        ResourceLoadAnalyseMap = new Dictionary<string, ResourceLoadInfo>();
    }

    /// <summary>
    /// 开始资源加载卸载统计
    /// </summary>
    public void startResourceLoadAnalyse()
    {
        if(ResourceLoadAnalyseSwitch)
        {
            ResourceLoadAnalyseStart = true;
            ResourceLoadAnalyseMap.Clear();
            ResourceLogger.log("开启AB加载统计!");
        }
        else
        {
            ResourceLogger.logErr("请先开启AB资源加载分析开关!");
        }
    }

    /// <summary>
    /// 停止资源加载卸载统计并输出
    /// </summary>
    public void endResourceLoadAnalyse()
    {
        if(ResourceLoadAnalyseSwitch)
        {
            if(ResourceLoadAnalyseStart)
            {
                ResourceLoadAnalyseStart = false;
                outputResourceLoadedInfoDetail();
                ResourceLoadAnalyseMap.Clear();
                ResourceLogger.log("结束资源加载统计!");
            }
            else
            {
                ResourceLogger.logErr("请先启动资源加载统计!");
            }
        }
        else
        {
            ResourceLogger.logErr("请先开启资源资源加载分析开关!");
        }
    }
    
    /// <summary>
    /// 增加指定资源加载次数
    /// </summary>
    /// <param name="abname"></param>
    public void addResourceLoadedTime(string abname)
    {
        if(ResourceLoadAnalyseMap.ContainsKey(abname))
        {
            ResourceLoadAnalyseMap[abname].ResourceLoadTimeCount = ResourceLoadAnalyseMap[abname].ResourceLoadTimeCount + 1;
        }
        else
        {
            var abloadinfo = new ResourceLoadInfo();
            abloadinfo.ABName = abname;
            abloadinfo.ResourceLoadTimeCount = 1;
            abloadinfo.ResourceUnloadTimeCount = 0;
            var now = DateTime.Now;
            abloadinfo.ResourceFirstLoadTime = string.Format("{0}-{1}-{2}", now.Hour, now.Minute, now.Second);
            ResourceLoadAnalyseMap.Add(abname, abloadinfo);
        }
    }

    /// <summary>
    /// 增加指定资源卸载次数
    /// </summary>
    /// <param name="abname"></param>
    public void addResourceUnloadedTime(string abname)
    {
        if (ResourceLoadAnalyseMap.ContainsKey(abname))
        {
            ResourceLoadAnalyseMap[abname].ResourceUnloadTimeCount = ResourceLoadAnalyseMap[abname].ResourceUnloadTimeCount + 1;
            var now = DateTime.Now;
            ResourceLoadAnalyseMap[abname].ResourceLastUnloadTime = string.Format("{0}-{1}-{2}", now.Hour, now.Minute, now.Second);
        }
        else
        {
            //有可能开启统计时已经加载过了，所以未被统计到
            //ResourceLogger.instance.logErr(string.Format("AB : {0}未被加载，不允许增加卸载次数！请检查AB加载卸载是否出问题！", abname));
            addResourceLoadedTime(abname);
            addResourceUnloadedTime(abname);
        }
    }

    /// <summary>
    /// 输出资源加载卸载详细信息
    /// </summary>
    private void outputResourceLoadedInfoDetail()
    {
        DateTime now = DateTime.Now;
        string logFile = string.Format("{0}/ABLoadDetail_{1}_{2}_{3}.log", ABLoadedInfoOutputFolderPath,
            now.Hour, now.Minute, now.Second);

        StreamWriter sw;
        if (!File.Exists(logFile))
            sw = File.CreateText(logFile);
        else
            sw = new StreamWriter(logFile, true);

        var loadedabinfolist = new List<ResourceLoadInfo>(ResourceLoadAnalyseMap.Values);
        loadedabinfolist.Sort(sortUnloadTime);

        foreach (var abloadinfo in loadedabinfolist)
        {
            sw.WriteLine("AssetBundleName : " + abloadinfo.ABName);
            sw.WriteLine("\tABLoadTimeCount : " + abloadinfo.ResourceLoadTimeCount);
            sw.WriteLine("\tABUnloadTimeCount : " + abloadinfo.ResourceUnloadTimeCount);
            sw.WriteLine("\tABFirstLoadTime : " + abloadinfo.ResourceFirstLoadTime);
            sw.WriteLine("\tABLastUnloadTime : " + abloadinfo.ResourceLastUnloadTime);
            sw.WriteLine();
        }
        sw.Dispose();
    }

    /// <summary>
    /// 按卸载次数从高到低排序
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    private int sortUnloadTime(ResourceLoadInfo a, ResourceLoadInfo b)
    {
        return b.ResourceUnloadTimeCount.CompareTo(a.ResourceUnloadTimeCount);
    }
}
