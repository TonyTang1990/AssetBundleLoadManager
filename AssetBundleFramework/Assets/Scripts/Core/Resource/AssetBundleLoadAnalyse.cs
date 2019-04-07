/*
 * Description:             AssetBundleLoadAnalyse.cs
 * Author:                  TonyTang
 * Create Date:             2018/12/29
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// AssetBundleLoadAnalyse.cs
/// AB资源加载统计分析工具
/// </summary>
public class AssetBundleLoadAnalyse : SingletonTemplate<AssetBundleLoadAnalyse> {

    /// <summary> AB加载卸载信息抽象 /// </summary>
    public class ABLoadInfo
    {
        /// <summary> AB名字 /// </summary>
        public string ABName { get; set; }

        /// <summary> AB加载次数 /// </summary>
        public int ABLoadTimeCount { get; set; }

        /// <summary> AB卸载次数 /// </summary>
        public int ABUnloadTimeCount { get; set; }

        /// <summary> AB第一次被加载的时间 /// </summary>
        public string ABFirstLoadTime { get; set; }

        /// <summary> AB最后一次被卸载的时间 /// </summary>
        public string ABLastUnloadTime { get; set; }
    }

    /// <summary> AB资源加载统计开关 /// </summary>
    public bool ABLoadAnalyseSwitch { get; set; }

    /// <summary> AB加载分析开始标志 /// </summary>
    public bool ABLoadAnalyseStart { get; set; }

    /// <summary>
    /// AB加载统计分析映射Map
    /// Key为AB名，Value为对应AB加载卸载详细信息
    /// </summary>
    public Dictionary<string, ABLoadInfo> ABLoadAnalyseMap { get; set; }

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

    public AssetBundleLoadAnalyse()
    {
        ABLoadAnalyseSwitch = false;
        ABLoadAnalyseStart = false;
        ABLoadAnalyseMap = new Dictionary<string, ABLoadInfo>();
    }

    /// <summary>
    /// 开始AB加载卸载统计
    /// </summary>
    public void startABLoadAnalyse()
    {
        if(ABLoadAnalyseSwitch)
        {
            ABLoadAnalyseStart = true;
            ABLoadAnalyseMap.Clear();
            ResourceLogger.log("开启AB加载统计!");
        }
        else
        {
            ResourceLogger.logErr("请先开启AB资源加载分析开关!");
        }
    }

    /// <summary>
    /// 停止AB加载卸载统计并输出
    /// </summary>
    public void endABLoadAnalyse()
    {
        if(ABLoadAnalyseSwitch)
        {
            if(ABLoadAnalyseStart)
            {
                ABLoadAnalyseStart = false;
                outputABLoadedInfoDetail();
                ABLoadAnalyseMap.Clear();
                ResourceLogger.log("结束AB加载统计!");
            }
            else
            {
                ResourceLogger.logErr("请先启动AB加载统计!");
            }
        }
        else
        {
            ResourceLogger.logErr("请先开启AB资源加载分析开关!");
        }
    }
    
    /// <summary>
    /// 增加指定AB加载次数
    /// </summary>
    /// <param name="abname"></param>
    public void addABLoadedTime(string abname)
    {
        if(ABLoadAnalyseMap.ContainsKey(abname))
        {
            ABLoadAnalyseMap[abname].ABLoadTimeCount = ABLoadAnalyseMap[abname].ABLoadTimeCount + 1;
        }
        else
        {
            var abloadinfo = new ABLoadInfo();
            abloadinfo.ABName = abname;
            abloadinfo.ABLoadTimeCount = 1;
            abloadinfo.ABUnloadTimeCount = 0;
            var now = DateTime.Now;
            abloadinfo.ABFirstLoadTime = string.Format("{0}-{1}-{2}", now.Hour, now.Minute, now.Second);
            ABLoadAnalyseMap.Add(abname, abloadinfo);
        }
    }

    /// <summary>
    /// 增加指定AB卸载次数
    /// </summary>
    /// <param name="abname"></param>
    public void addABUnloadedTime(string abname)
    {
        if (ABLoadAnalyseMap.ContainsKey(abname))
        {
            ABLoadAnalyseMap[abname].ABUnloadTimeCount = ABLoadAnalyseMap[abname].ABUnloadTimeCount + 1;
            var now = DateTime.Now;
            ABLoadAnalyseMap[abname].ABLastUnloadTime = string.Format("{0}-{1}-{2}", now.Hour, now.Minute, now.Second);
        }
        else
        {
            //有可能开启统计时已经加载过了，所以未被统计到
            //ResourceLogger.instance.logErr(string.Format("AB : {0}未被加载，不允许增加卸载次数！请检查AB加载卸载是否出问题！", abname));
            addABLoadedTime(abname);
            addABUnloadedTime(abname);
        }
    }

    /// <summary>
    /// 输出AB加载卸载详细信息
    /// </summary>
    private void outputABLoadedInfoDetail()
    {
        DateTime now = DateTime.Now;
        string logFile = string.Format("{0}/ABLoadDetail_{1}_{2}_{3}.log", ABLoadedInfoOutputFolderPath,
            now.Hour, now.Minute, now.Second);

        StreamWriter sw;
        if (!File.Exists(logFile))
            sw = File.CreateText(logFile);
        else
            sw = new StreamWriter(logFile, true);

        var loadedabinfolist = new List<ABLoadInfo>(ABLoadAnalyseMap.Values);
        loadedabinfolist.Sort(sortUnloadTime);

        foreach (var abloadinfo in loadedabinfolist)
        {
            sw.WriteLine("AssetBundleName : " + abloadinfo.ABName);
            sw.WriteLine("\tABLoadTimeCount : " + abloadinfo.ABLoadTimeCount);
            sw.WriteLine("\tABUnloadTimeCount : " + abloadinfo.ABUnloadTimeCount);
            sw.WriteLine("\tABFirstLoadTime : " + abloadinfo.ABFirstLoadTime);
            sw.WriteLine("\tABLastUnloadTime : " + abloadinfo.ABLastUnloadTime);
            sw.WriteLine();
        }
        sw.Dispose();
    }

    /// <summary>
    /// 按卸载次数从高到低排序
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    private int sortUnloadTime(ABLoadInfo a, ABLoadInfo b)
    {
        return b.ABUnloadTimeCount.CompareTo(a.ABUnloadTimeCount);
    }
}
