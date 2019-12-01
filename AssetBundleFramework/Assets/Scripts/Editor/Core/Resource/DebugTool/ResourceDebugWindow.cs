/*
 * Description:             ResourceDebugWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2018//08/28
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ResourceDebugWindow.cs
/// 资源(AssetBundle和AssetDatabase两种模式都支持)辅助调试工具UI窗口
/// </summary>
public class ResourceDebugWindow : EditorWindow
{
    /// <summary>
    /// AB辅助工具类型
    /// </summary>
    private enum ABDebugToolType
    {
        AB_Display_All_Dep = 1,                     // 展示AB依赖文件信息类型
        AB_Display_AB_ReferenceInfo = 2,            // 展示所有AB引用信息类型
        AB_Display_Async_QueueInfo = 3,             // 展示AB异步加载队列信息类型
    }

    /// <summary>
    /// 是否开启Logger
    /// </summary>
    private bool mLoggerSwitch = true;

    /// <summary>
    /// 当前资源加载模式
    /// </summary>
    private int ResourceLoadModeIndex
    {
        get
        {
            return (int)ResourceModuleManager.Singleton.ResLoadMode;
        }
        set
        {
            ResourceModuleManager.Singleton.ResLoadMode = (ResourceLoadMode)value;
        }
    }

    /// <summary>
    /// 资源模式选项列表
    /// </summary>
    private string[] ResourceLoadModeChoices = Enum.GetNames(typeof(ResourceLoadMode));

    /// <summary>
    /// 过滤文本
    /// </summary>
    private string mTextFilter = "";

    /// <summary>
    /// 当前AB辅助工具类型
    /// </summary>
    private ABDebugToolType mCurrentABDebugToolType = ABDebugToolType.AB_Display_AB_ReferenceInfo;

    /// <summary>
    /// UI滚动位置
    /// </summary>
    private Vector2 mUiScrollPos;

    /// <summary>
    /// 详细信息是否折叠
    /// </summary>
    private bool mDetailFoldOut = true;

    /// <summary>
    /// 过滤文本是否变化(减少显示更新频率)
    /// </summary>
    private bool mFilterTextChanged = true;

    /// <summary>
    /// 符合展示索引信息筛选条件的AB名字列表
    /// </summary>
    private List<string> mValideDepABNameList = new List<string>();

    /// <summary>
    /// 默认不筛选需要展示的最大索引信息的AB最大数量(避免不筛选是过多导致过卡)
    /// </summary>
    private const int MaxDepABInfoNumber = 100;

    /// <summary>
    /// 符合展示引用信息筛选条件的AB加载信息列表
    /// </summary>
    private List<AbstractResourceInfo> mValideReferenceABInfoList = new List<AbstractResourceInfo>();

    [MenuItem("Tools/Debug/资源调试工具", false, 103)]
    public static void openConvenientUIWindow()
    {
        ResourceDebugWindow window = EditorWindow.GetWindow<ResourceDebugWindow>();
        window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        if (!EditorApplication.isPlaying)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("仅在运行模式下可用!");
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            mLoggerSwitch = GUILayout.Toggle(mLoggerSwitch, "Unity Log总开关", GUILayout.Width(150.0f));
            if (mLoggerSwitch != Debug.unityLogger.logEnabled)
            {
                Debug.unityLogger.logEnabled = mLoggerSwitch;
            }
            ResourceLogger.LogSwitch = GUILayout.Toggle(ResourceLogger.LogSwitch, "是否开启资源Log", GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f));
            GUILayout.Label("资源回收开关:" + ResourceModuleManager.Singleton.CurrentResourceModule.EnableResourceRecyclingUnloadUnsed, GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f));
            var preresourceloadmodeindex = ResourceLoadModeIndex;
            var newresourceloadmodeindex = EditorGUILayout.Popup(preresourceloadmodeindex, ResourceLoadModeChoices, GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f));
            if(preresourceloadmodeindex != newresourceloadmodeindex)
            {
                ResourceLoadModeIndex = newresourceloadmodeindex;
            }
            GUILayout.Label("资源回收开关:" + ResourceModuleManager.Singleton.CurrentResourceModule.EnableResourceRecyclingUnloadUnsed, GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("筛选文本(默认不填表示显示所有):", GUILayout.MaxWidth(200.0f), GUILayout.MaxHeight(30.0f));
            var oldtextfilter = mTextFilter;
            mTextFilter = EditorGUILayout.TextField(mTextFilter, GUILayout.MaxWidth(100.0f), GUILayout.MaxHeight(30.0f));
            if (!oldtextfilter.Equals(mTextFilter))
            {
                mFilterTextChanged = true;
            }
            else
            {
                mFilterTextChanged = false;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("查看AB依赖信息", GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f)))
            {
                mCurrentABDebugToolType = ABDebugToolType.AB_Display_All_Dep;
                mFilterTextChanged = true;
            }
            if (GUILayout.Button("查看资源使用索引信息", GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f)))
            {
                mCurrentABDebugToolType = ABDebugToolType.AB_Display_AB_ReferenceInfo;
                mFilterTextChanged = true;
            }
            if (GUILayout.Button("查看AB异步加载信息", GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f)))
            {
                mCurrentABDebugToolType = ABDebugToolType.AB_Display_Async_QueueInfo;
                mFilterTextChanged = true;
            }
            if (GUILayout.Button("生成一份txt的AB依赖信息", GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f)))
            {
                writeABDepInfoIntoTxt();
            }
            if (GUILayout.Button("强制卸载指定AB", GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f)))
            {
                forceUnloadSpecificAB(mTextFilter);
            }
            if (GUILayout.Button("开启资源加载统计", GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f)))
            {
                ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch = true;
                ResourceLoadAnalyse.Singleton.startResourceLoadAnalyse();
            }
            if (GUILayout.Button("结束资源加载统计", GUILayout.MaxWidth(120.0f), GUILayout.MaxHeight(30.0f)))
            {
                ResourceLoadAnalyse.Singleton.endResourceLoadAnalyse();
                ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch = false;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.BeginVertical(GUILayout.MaxWidth(position.width), GUILayout.MaxHeight(position.height));
            mUiScrollPos = GUILayout.BeginScrollView(mUiScrollPos);
            mDetailFoldOut = EditorGUILayout.Foldout(mDetailFoldOut, "详细数据展示区域:");
            if (mDetailFoldOut)
            {
                if (mCurrentABDebugToolType == ABDebugToolType.AB_Display_All_Dep)
                {
                    displayABAllDepInfoUI();
                }
                else if (mCurrentABDebugToolType == ABDebugToolType.AB_Display_AB_ReferenceInfo)
                {
                    displayABReferenceInfoUI();
                }
                else if (mCurrentABDebugToolType == ABDebugToolType.AB_Display_Async_QueueInfo)
                {
                    displayABAysncQueueInfoUI();
                }
            }
            GUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 显示AB依赖信息UI
    /// </summary>
    private void displayABAllDepInfoUI()
    {
        if (isAssetBundleModule())
        {
            var assetbundleresourcemodule = ResourceModuleManager.Singleton.CurrentResourceModule as AssetBundleModule;
            GUILayout.BeginVertical();
            var alldepinfo = assetbundleresourcemodule.AssetBundleDpMap;
            if (!mTextFilter.Equals(string.Empty))
            {
                if (mFilterTextChanged)
                {
                    mValideDepABNameList.Clear();
                    foreach (var depinfo in alldepinfo)
                    {
                        if (depinfo.Key.StartsWith(mTextFilter))
                        {
                            mValideDepABNameList.Add(depinfo.Key);
                        }
                    }
                    if (mValideDepABNameList.Count > 0)
                    {
                        foreach (var abname in mValideDepABNameList)
                        {
                            GUILayout.Label(string.Format("{0} -> {1}", abname, getDepDes(alldepinfo[abname])));
                        }
                    }
                    else
                    {
                        GUILayout.Label(string.Format("找不到资源以 : {0}开头的依赖信息!", mTextFilter));
                    }
                }
                else
                {
                    if (mValideDepABNameList.Count > 0)
                    {
                        foreach (var abname in mValideDepABNameList)
                        {
                            GUILayout.Label(string.Format("{0} -> {1}", abname, getDepDes(alldepinfo[abname])));
                        }
                    }
                    else
                    {
                        GUILayout.Label(string.Format("找不到资源以 : {0}开头的依赖信息!", mTextFilter));
                    }
                }
            }
            else
            {
                int num = 0;
                foreach (var depinfo in alldepinfo)
                {
                    num++;
                    if (num < MaxDepABInfoNumber)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label(string.Format("{0} -> {1}", depinfo.Key, getDepDes(depinfo.Value)));
                        EditorGUILayout.EndHorizontal();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            GUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("AB依赖信息只在AssetBundle模式下可用！");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }        
    }

    /// <summary>
    /// 显示资源使用索引信息UI
    /// </summary>
    private void displayABReferenceInfoUI()
    {
        var assetbundleresourcemodule = ResourceModuleManager.Singleton.CurrentResourceModule;
        EditorGUILayout.BeginVertical();
        var normalloadedabinfomap = assetbundleresourcemodule.getSpecificLoadTypeARIMap(ResourceLoadType.NormalLoad);
        var preloadloadedabinfomap = assetbundleresourcemodule.getSpecificLoadTypeARIMap(ResourceLoadType.Preload);
        var permanentloadedabinfomap = assetbundleresourcemodule.getSpecificLoadTypeARIMap(ResourceLoadType.PermanentLoad);
        if (!mTextFilter.Equals(string.Empty))
        {
            if (mFilterTextChanged)
            {
                mValideReferenceABInfoList.Clear();
                foreach (var normalloadedabinfo in normalloadedabinfomap)
                {
                    if (normalloadedabinfo.Key.StartsWith(mTextFilter))
                    {
                        mValideReferenceABInfoList.Add(normalloadedabinfo.Value);
                    }
                }

                foreach (var preloadloadedabinfo in preloadloadedabinfomap)
                {
                    if (preloadloadedabinfo.Key.StartsWith(mTextFilter))
                    {
                        mValideReferenceABInfoList.Add(preloadloadedabinfo.Value);
                    }
                }

                foreach (var permanentloadedabinfo in permanentloadedabinfomap)
                {
                    if (permanentloadedabinfo.Key.StartsWith(mTextFilter))
                    {
                        mValideReferenceABInfoList.Add(permanentloadedabinfo.Value);
                    }
                }

                if (mValideReferenceABInfoList.Count > 0)
                {
                    foreach (var validereferenceabinfo in mValideReferenceABInfoList)
                    {
                        displayOneAssetBundleInfoUI(validereferenceabinfo);
                    }
                }
                else
                {
                    GUILayout.Label(string.Format("找不到资源 : {0}的索引信息!", mTextFilter));
                }
            }
            else
            {
                if (mValideReferenceABInfoList.Count > 0)
                {
                    foreach (var validereferenceabinfo in mValideReferenceABInfoList)
                    {
                        displayOneAssetBundleInfoUI(validereferenceabinfo);
                    }
                }
                else
                {
                    GUILayout.Label(string.Format("找不到资源 : {0}的索引信息!", mTextFilter));
                }
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("当前FPS : {0}", assetbundleresourcemodule.CurrentFPS));
            EditorGUILayout.EndHorizontal();
            if(assetbundleresourcemodule.ResLoadMode == ResourceLoadMode.AssetBundle)
            {
                var resourcemodule = assetbundleresourcemodule as AssetBundleModule;
                if (resourcemodule != null)
                {
                    var requestinfomap = resourcemodule.ABRequestTaskMap;
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(string.Format("加载队列信息 : {0}", requestinfomap.Count == 0 ? "无" : string.Empty));
                    EditorGUILayout.EndHorizontal();
                    foreach (var requestabl in requestinfomap)
                    {
                        displayOneAssetBundleLoaderInfoUI(requestabl.Value);
                    }
                }
            }            
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("资源加载队列信息只在AssetBundle模式下可用!");
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("正常已加载资源信息:");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("正常已加载资源数量 : {0}", normalloadedabinfomap.Count));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("可回收正常已加载非常驻资源数量 : {0}", assetbundleresourcemodule.getNormalUnsedABNumber()));
            EditorGUILayout.EndHorizontal();
            foreach (var loadedabi in normalloadedabinfomap)
            {
                displayOneAssetBundleInfoUI(loadedabi.Value);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("预加载已加载资源信息:");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("预加载已加载资源数量 : {0}", preloadloadedabinfomap.Count));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("可回收预加载已加载非常驻资源数量 : {0}", assetbundleresourcemodule.getPreloadUnsedABNumber()));
            EditorGUILayout.EndHorizontal();
            foreach (var loadedabi in preloadloadedabinfomap)
            {
                displayOneAssetBundleInfoUI(loadedabi.Value);
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("常驻已加载资源信息:");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("已加载常驻资源数量 : {0}", permanentloadedabinfomap.Count));
            EditorGUILayout.EndHorizontal();
            foreach (var ploadedabi in permanentloadedabinfomap)
            {
                displayOneAssetBundleInfoUI(ploadedabi.Value);
            }
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 显示AB异步加载队列信息UI
    /// </summary>
    private void displayABAysncQueueInfoUI()
    {
        if (isAssetBundleModule())
        {
            var assetbundleresourcemodule = ResourceModuleManager.Singleton.CurrentResourceModule as AssetBundleModule;
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("当前AB异步加载队列信息 :");
            EditorGUILayout.EndHorizontal();
            var abasyncqueue = AssetBundleAsyncQueue.ABAsyncQueue;
            if (abasyncqueue.Count > 0)
            {
                foreach (var abasync in abasyncqueue)
                {
                    displayOneAssetBundleLoaderInfoUI(abasync);
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("无", GUILayout.Width(250.0f));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("当前AB异步加载携程总数量 : {0}", assetbundleresourcemodule.MaxMaximumAsyncCoroutine));
            EditorGUILayout.EndHorizontal();
            var abasyncqueuelist = assetbundleresourcemodule.AssetBundleAsyncQueueList;
            for (int i = 0; i < abasyncqueuelist.Count; i++)
            {
                displayOneAssetBundleAsyncQueueInfoUI(abasyncqueuelist[i], i);
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            GUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("AB异步加载队列信息只在AssetBundle模式下可用！");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>
    /// 显示一个AssetBundleLoader的信息
    /// </summary>
    /// <param name="abi"></param>
    private void displayOneAssetBundleLoaderInfoUI(AssetBundleLoader abl)
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("资源名 : {0}", abl.AssetBundleName), GUILayout.Width(150.0f));
        EditorGUILayout.LabelField(string.Format("加载状态 : {0}", abl.LoadState), GUILayout.Width(150.0f));
        EditorGUILayout.LabelField(string.Format("加载方式 : {0}", abl.LoadMethod), GUILayout.Width(150.0f));
        EditorGUILayout.LabelField(string.Format("加载类型 : {0}", abl.LoadType), GUILayout.Width(150.0f));        
        EditorGUILayout.LabelField(string.Format("依赖资源数量 : {0}", abl.DepABCount), GUILayout.Width(150.0f));
        EditorGUILayout.LabelField(string.Format("已加载依赖资源数量 : {0}", abl.DepAssetBundleInfoList.Count), GUILayout.Width(150.0f));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("已加载完成的依赖资源列表 :", GUILayout.Width(200.0f));
        EditorGUILayout.BeginHorizontal();
        for (int i = 0, length = abl.DepAssetBundleInfoList.Count; i < length; i++)
        {
            var depabi = abl.DepAssetBundleInfoList[i];
            EditorGUILayout.LabelField(i + ". " + depabi.AssetBundleName, GUILayout.Width(150.0f));
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField("未加载完成的依赖资源列表 :", GUILayout.Width(200.0f));
        EditorGUILayout.BeginHorizontal();
        for (int j = 0, length = abl.UnloadedAssetBundleName.Count; j < length; j++)
        {
            var uldepabi = abl.UnloadedAssetBundleName[j];
            EditorGUILayout.LabelField(j + ". " + uldepabi, GUILayout.Width(150.0f));
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 显示一个AssetBundleInfo的信息
    /// </summary>
    /// <param name="abi"></param>
    private void displayOneAssetBundleInfoUI(AbstractResourceInfo abi)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("资源名 : {0}", abi.AssetBundleName), GUILayout.Width(250.0f));
        EditorGUILayout.LabelField(string.Format("是否就绪 : {0}", abi.mIsReady), GUILayout.Width(100.0f));
        EditorGUILayout.LabelField(string.Format("引用计数 : {0}", abi.RefCount), GUILayout.Width(100.0f));
        EditorGUILayout.LabelField(string.Format("最近使用时间 : {0}", abi.LastUsedTime), GUILayout.Width(150.0f));
        EditorGUILayout.LabelField(string.Format("依赖引用对象列表 : {0}", abi.ReferenceOwnerList.Count == 0 ? "无" : string.Empty), GUILayout.Width(150.0f));
        foreach (var refowner in abi.ReferenceOwnerList)
        {
            if (refowner.Target != null)
            {
                EditorGUILayout.ObjectField((UnityEngine.Object)refowner.Target, typeof(UnityEngine.Object), true, GUILayout.Width(200.0f));
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 显示一个AB异步加载队列信息
    /// </summary>
    /// <param name="abaq">队列信息</param>
    /// <param name="queueindex">队列索引</param>
    private void displayOneAssetBundleAsyncQueueInfoUI(AssetBundleAsyncQueue abaq, int queueindex)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(string.Format("AB异步携程索引号 : {0}", queueindex), GUILayout.Width(250.0f));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("当前AB异步加载信息 : ", GUILayout.Width(250.0f));
        EditorGUILayout.EndHorizontal();
        if(abaq.CurrentLoadingAssetBundleLoader != null)
        {
            displayOneAssetBundleLoaderInfoUI(abaq.CurrentLoadingAssetBundleLoader);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("无", GUILayout.Width(250.0f));
            EditorGUILayout.EndHorizontal();
        }
    }

    /// <summary>
    /// 根据AB依赖二进制ab信息写一份txt的AB依赖信息文本
    /// </summary>
    private void writeABDepInfoIntoTxt()
    {
        if(isAssetBundleModule())
        {
            var assetbundleresourcemodule = ResourceModuleManager.Singleton.CurrentResourceModule as AssetBundleModule;
            var alldepinfomap = assetbundleresourcemodule.AssetBundleDpMap;
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Application.dataPath + "/../alldepab.txt"))
            {
                foreach (var depinfo in alldepinfomap)
                {
                    sw.WriteLine(depinfo.Key);
                    foreach (var dep in depinfo.Value)
                    {
                        sw.WriteLine("\t" + dep);
                    }
                    sw.WriteLine();
                }
                sw.Close();
                sw.Dispose();
            }
        }
        else
        {
            Debug.LogError("非AssetBundle模式下不支持输出AB依赖信息文件！");
        }
    }

    /// <summary>
    /// 获取依赖AB信息描述字符串
    /// </summary>
    /// <param name="deps"></param>
    /// <returns></returns>
    private string getDepDes(string[] deps)
    {
        var depdes = string.Empty;
        for (int i = 0, length = deps.Length; i < length; i++)
        {
            depdes += deps[i];
            if (i != length - 1)
            {
                depdes += " & ";
            }
        }
        return depdes;
    }

    /// <summary>
    /// 强制卸载指定AB
    /// </summary>
    /// <param name="abname"></param>
    private void forceUnloadSpecificAB(string abname)
    {
        if(isAssetBundleModule())
        {
            var assetbundleresourcemodule = ResourceModuleManager.Singleton.CurrentResourceModule as AssetBundleModule;
            assetbundleresourcemodule.forceUnloadSpecificResource(abname);
        }
        else
        {
            Debug.LogError("强制卸载指定AB功能只在AssetBundle模式下可用!");
        }
    }

    /// <summary>
    /// 是否是AssetBundle模式
    /// </summary>
    /// <returns></returns>
    private bool isAssetBundleModule()
    {
        return ResourceModuleManager.Singleton.CurrentResourceModule.ResLoadMode == ResourceLoadMode.AssetBundle;
    }
}
