/*
 * Description:             ABDebugWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2018//08/28
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// ABDebugWindow.cs
/// AssetBundle辅助调试工具UI窗口
/// </summary>
public class ABDebugWindow : EditorWindow {

    /// <summary>
    /// AB辅助工具类型
    /// </summary>
    private enum ABDebugToolType
    {
        AB_Display_All_Dep = 1,                     // 展示AB依赖文件信息类型
        AB_Display_AB_ReferenceInfo = 2,            // 展示所有AB引用信息类型
    }

    /// <summary>
    /// Log开关
    /// </summary>
    public static bool LogSwitch = true;

    /// <summary>
    /// 过滤文本
    /// </summary>
    private string mTextFilter = "maincitymain";

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

    [MenuItem("新AB工具/AB加载管理调试/辅助工具")]
    public static void openConvenientUIWindow()
    {
        ABDebugWindow window = EditorWindow.GetWindow<ABDebugWindow>();
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
            LogSwitch = GUILayout.Toggle(LogSwitch, "Unity Log总开关", GUILayout.Width(150.0f));
            if (LogSwitch != Debug.unityLogger.logEnabled)
            {
                Debug.unityLogger.logEnabled = LogSwitch;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("筛选文本(默认不填表示显示所有):", GUILayout.MaxWidth(200.0f), GUILayout.MaxHeight(30.0f));
            mTextFilter = GUILayout.TextField(mTextFilter, GUILayout.MaxWidth(100.0f), GUILayout.MaxHeight(30.0f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("查看AB依赖信息", GUILayout.MaxWidth(200.0f), GUILayout.MaxHeight(30.0f)))
            {
                mCurrentABDebugToolType = ABDebugToolType.AB_Display_All_Dep;
            }
            if (GUILayout.Button("查看AB使用索引信息", GUILayout.MaxWidth(200.0f), GUILayout.MaxHeight(30.0f)))
            {
                mCurrentABDebugToolType = ABDebugToolType.AB_Display_AB_ReferenceInfo;
            }
            if (GUILayout.Button("生成一份txt的AB依赖信息", GUILayout.MaxWidth(200.0f), GUILayout.MaxHeight(30.0f)))
            {
                writeABDepInfoIntoTxt();
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
        GUILayout.BeginVertical();
        var alldepinfo = ModuleManager.Singleton.getModule<ResourceModuleManager>().AssetBundleDpMap;
        if (!mTextFilter.Equals(string.Empty))
        {
            if (alldepinfo.ContainsKey(mTextFilter))
            {
                GUILayout.Label(string.Format("{0} -> {1}", mTextFilter, getDepDes(alldepinfo[mTextFilter])));
            }
            else
            {
                GUILayout.Label(string.Format("找不到资源 : {0}的依赖信息!", mTextFilter));
            }
        }
        else
        {
            foreach (var depinfo in alldepinfo)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("{0} -> {1}", depinfo.Key, getDepDes(depinfo.Value)));
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 显示AB使用索引信息UI
    /// </summary>
    private void displayABReferenceInfoUI()
    {
        EditorGUILayout.BeginVertical();
        var requestabllist = ModuleManager.Singleton.getModule<ResourceModuleManager>().ABRequestTaskList;
        //var loadingabllist = ModuleManager.Singleton.getModule<ResourceModuleManager>().ABLoadingTaskList;
        var normalloadedabinfomap = ModuleManager.Singleton.getModule<ResourceModuleManager>().getSpecificLoadTypeABIMap(ABLoadType.NormalLoad);
        var preloadloadedabinfomap = ModuleManager.Singleton.getModule<ResourceModuleManager>().getSpecificLoadTypeABIMap(ABLoadType.Preload);
        var permanentloadedabinfomap = ModuleManager.Singleton.getModule<ResourceModuleManager>().getSpecificLoadTypeABIMap(ABLoadType.PermanentLoad);
        if (!mTextFilter.Equals(string.Empty))
        {
            if (normalloadedabinfomap.ContainsKey(mTextFilter))
            {
                GUILayout.Label(normalloadedabinfomap[mTextFilter].getAssetBundleInfoDes());
            }
            if (preloadloadedabinfomap.ContainsKey(mTextFilter))
            {
                GUILayout.Label(preloadloadedabinfomap[mTextFilter].getAssetBundleInfoDes());
            }
            else if (permanentloadedabinfomap.ContainsKey(mTextFilter))
            {
                GUILayout.Label(permanentloadedabinfomap[mTextFilter].getAssetBundleInfoDes());
            }
            else
            {
                GUILayout.Label(string.Format("找不到资源 : {0}的索引信息!", mTextFilter));
            }
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("当前FPS : {0}", ModuleManager.Singleton.getModule<ResourceModuleManager>().CurrentFPS));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(string.Format("等待加载AB任务信息 : {0}", requestabllist.Count == 0 ? "无" : string.Empty));
            EditorGUILayout.EndHorizontal();
            foreach (var requestabl in requestabllist)
            {
                displayOneAssetBundleLoaderInfoUI(requestabl);
            }

            //EditorGUILayout.Space();
            //EditorGUILayout.BeginHorizontal();
            //GUILayout.Label(string.Format("加载中AB任务信息 : {0}", loadingabllist.Count == 0 ? "无" : string.Empty));
            //EditorGUILayout.EndHorizontal();
            //foreach (var loadingabl in loadingabllist)
            //{
            //    displayOneAssetBundleLoaderInfoUI(loadingabl);
            //}

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("正常已加载AB数量 : " + normalloadedabinfomap.Count);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("可回收正常已加载非常驻AB数量 : " + ModuleManager.Singleton.getModule<ResourceModuleManager>().getNormalUnsedABNumber());
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
            GUILayout.Label("预加载已加载AB数量 : " + preloadloadedabinfomap.Count);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("可回收预加载已加载非常驻AB数量 : " + ModuleManager.Singleton.getModule<ResourceModuleManager>().getPreloadUnsedABNumber());
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
            GUILayout.Label("已加载常驻AB数量 : " + permanentloadedabinfomap.Count);
            EditorGUILayout.EndHorizontal();
            foreach (var ploadedabi in permanentloadedabinfomap)
            {
                displayOneAssetBundleInfoUI(ploadedabi.Value);
            }
        }
        EditorGUILayout.EndVertical();
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
        EditorGUILayout.LabelField("资源名 : " + abl.ABName, GUILayout.Width(150.0f));
        EditorGUILayout.LabelField("加载状态 : " + abl.LoadState, GUILayout.Width(150.0f));
        EditorGUILayout.LabelField("加载方式 : " + abl.LoadType, GUILayout.Width(100.0f));
        EditorGUILayout.LabelField("依赖资源数量 : " + abl.DepABCount, GUILayout.Width(100.0f));
        EditorGUILayout.LabelField("已加载依赖资源数量 : " + abl.DepAssetBundleInfoList.Count, GUILayout.Width(150.0f));
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
    private void displayOneAssetBundleInfoUI(AssetBundleInfo abi)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("资源名 : " + abi.AssetBundleName, GUILayout.Width(250.0f));
        EditorGUILayout.LabelField("引用计数 : " + abi.RefCount, GUILayout.Width(100.0f));
        EditorGUILayout.LabelField("最近使用时间 : " + abi.LastUsedTime, GUILayout.Width(150.0f));
        EditorGUILayout.LabelField(string.Format("依赖引用对象列表 : {0}", abi.ReferenceOwnerList.Count == 0 ? "无" : string.Empty), GUILayout.Width(150.0f));
        foreach (var refowner in abi.ReferenceOwnerList)
        {
            if(refowner.Target != null)
            {
                //EditorGUILayout.LabelField(refowner.Target.ToString(), GUILayout.Width(400.0f));
                EditorGUILayout.ObjectField((Object)refowner.Target, typeof(Object), true, GUILayout.Width(200.0f));
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 根据AB依赖二进制ab信息写一份txt的AB依赖信息文本
    /// </summary>
    private void writeABDepInfoIntoTxt()
    {
        var alldepinfomap = ModuleManager.Singleton.getModule<ResourceModuleManager>().AssetBundleDpMap;
        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(Application.dataPath + "/alldepab.txt"))
        {
            foreach(var depinfo in alldepinfomap)
            {
                sw.WriteLine(depinfo.Key);
                foreach(var dep in depinfo.Value)
                {
                    sw.WriteLine("\t" + dep);
                }
                sw.WriteLine();
            }
            sw.Close();
            sw.Dispose();
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
}
