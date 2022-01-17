/*
 * Description:             ResourceDebugWindow.cs
 * Author:                  TONYTANG
 * Create Date:             2021//11/11
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// NewResourceDebugWindow.cs
    /// 资源调试窗口
    /// </summary>
    public class ResourceDebugWindow : BaseEditorWindow
    {
        /// <summary>
        /// AB辅助工具类型
        /// </summary>
        private enum ABDebugToolType
        {
            AB_Display_All_Dep = 1,                     // 展示AB依赖文件信息类型
            AB_Display_ReferenceInfo = 2,               // 展示所有引用信息类型
            AB_Display_Wait_Loaded_LoaderInfo = 3,      // 展示等待加载加载器信息类型
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
        private ABDebugToolType mCurrentABDebugToolType = ABDebugToolType.AB_Display_ReferenceInfo;

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

        #region AssetBundle模式展示数据
        /// <summary>
        /// 符合展示索引信息筛选条件的AB名字列表
        /// </summary>
        private List<string> mValideDepABPathList = new List<string>();

        /// <summary>
        /// 默认不筛选需要展示的最大索引信息的AB最大数量(避免不筛选是过多导致过卡)
        /// </summary>
        private const int MaxDepABInfoNumber = 100;

        /// <summary>
        /// 符合展示引用信息筛选条件的AB加载信息列表
        /// </summary>
        private List<AssetBundleInfo> mValideReferenceABInfoList = new List<AssetBundleInfo>();

        /// <summary>
        /// 所有等待加载的AssetBundleLoader
        /// </summary>
        private List<BundleLoader> mAllAssetBundleLoader = new List<BundleLoader>();

        /// <summary>
        /// 所有等待加载的AssetLoader
        /// </summary>
        private List<AssetLoader> mAllAssetLoader = new List<AssetLoader>();

        /// <summary>
        /// AssetBundle的Asset信息折叠控制Map<AssetBundle路径, AssetBundle的Asset信息是否折叠显示>
        /// </summary>
        private Dictionary<string, bool> mAssetBundleAssetInfoFoldMap = new Dictionary<string, bool>();

        /// <summary>
        /// AssetBundle加载器依赖信息折叠控制Map<AssetBundle路径, AssetBundle加载器依赖信息是否折叠显示>
        /// </summary>
        private Dictionary<string, bool> mAssetBundleLoaderDepFoldMap = new Dictionary<string, bool>();
        #endregion

        #region AssetDatabase模式展示数据
        /// <summary>
        /// 符合展示引用信息筛选条件的Asset加载信息列表
        /// </summary>
        private List<AssetInfo> mValideReferenceAssetInfoList = new List<AssetInfo>();

        /// <summary>
        /// 所有等待加载的AssetDatabaseLoader
        /// </summary>
        private List<AssetLoader> mAllAssetDatabaseLoader = new List<AssetLoader>();
        #endregion

        [MenuItem("Tools/AssetBundle/Debug/资源调试工具", false, 103)]
        public static void openResourceDebugWindow()
        {
            ResourceDebugWindow window = EditorWindow.GetWindow<ResourceDebugWindow>(false, "资源调试工具");
            window.Show();
        }
        private void OnInspectorUpdate()
        {
            Repaint();
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
                if (ResourceModuleManager.Singleton.CurrentResourceModule != null)
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
                    if (preresourceloadmodeindex != newresourceloadmodeindex)
                    {
                        ResourceLoadModeIndex = newresourceloadmodeindex;
                    }
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
                    if (GUILayout.Button("查看AB依赖信息", GUILayout.MaxWidth(150.0f), GUILayout.MaxHeight(30.0f)))
                    {
                        mCurrentABDebugToolType = ABDebugToolType.AB_Display_All_Dep;
                        mFilterTextChanged = true;
                    }
                    if (GUILayout.Button("查看资源使用索引信息", GUILayout.MaxWidth(150.0f), GUILayout.MaxHeight(30.0f)))
                    {
                        mCurrentABDebugToolType = ABDebugToolType.AB_Display_ReferenceInfo;
                        mFilterTextChanged = true;
                    }
                    if (GUILayout.Button("查看加载器信息", GUILayout.MaxWidth(150.0f), GUILayout.MaxHeight(30.0f)))
                    {
                        mCurrentABDebugToolType = ABDebugToolType.AB_Display_Wait_Loaded_LoaderInfo;
                        mFilterTextChanged = true;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("强制卸载指定AB", GUILayout.MaxWidth(150.0f), GUILayout.MaxHeight(30.0f)))
                    {
                        forceUnloadSpecificAB(mTextFilter);
                    }
                    if (GUILayout.Button("开启资源加载统计", GUILayout.MaxWidth(150.0f), GUILayout.MaxHeight(30.0f)))
                    {
                        ResourceLoadAnalyse.Singleton.ResourceLoadAnalyseSwitch = true;
                        ResourceLoadAnalyse.Singleton.startResourceLoadAnalyse();
                    }
                    if (GUILayout.Button("结束资源加载统计", GUILayout.MaxWidth(150.0f), GUILayout.MaxHeight(30.0f)))
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
                        else if (mCurrentABDebugToolType == ABDebugToolType.AB_Display_ReferenceInfo)
                        {
                            displayReferenceInfoUI();
                        }
                        else if (mCurrentABDebugToolType == ABDebugToolType.AB_Display_Wait_Loaded_LoaderInfo)
                        {
                            displayLoaderInfoUI();
                        }
                    }
                    GUILayout.EndScrollView();
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    GUILayout.Label("还未开启新版资源加载!");
                }
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
                var alldepinfo = assetbundleresourcemodule.AssetBuildInfo.ABPathDepMap;
                if (!mTextFilter.Equals(string.Empty))
                {
                    if (mFilterTextChanged)
                    {
                        mValideDepABPathList.Clear();
                        foreach (var depinfo in alldepinfo)
                        {
                            if (depinfo.Key.Contains(mTextFilter))
                            {
                                mValideDepABPathList.Add(depinfo.Key);
                            }
                        }
                        if (mValideDepABPathList.Count > 0)
                        {
                            foreach (var assetBundlePath in mValideDepABPathList)
                            {
                                displayOneAssetBundleDepInfoUI(assetBundlePath, alldepinfo[assetBundlePath]);
                            }
                        }
                        else
                        {
                            GUILayout.Label(string.Format("找不到资源以 : {0}开头的依赖信息!", mTextFilter));
                        }
                    }
                    else
                    {
                        if (mValideDepABPathList.Count > 0)
                        {
                            foreach (var assetBundlePath in mValideDepABPathList)
                            {
                                displayOneAssetBundleDepInfoUI(assetBundlePath, alldepinfo[assetBundlePath]);
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
                            displayOneAssetBundleDepInfoUI(depinfo.Key, depinfo.Value);
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
        /// 显示一个AB
        /// </summary>
        /// <param name="assetBundlePath"></param>
        /// <param name="depAssetBundlePaths"></param>
        private void displayOneAssetBundleDepInfoUI(string assetBundlePath, string[] depAssetBundlePaths)
        {
            EditorGUILayout.BeginHorizontal();
            displayOneAssetBundleButton(assetBundlePath);
            EditorGUILayout.LabelField("-->", GUILayout.Width(20f), GUILayout.Height(20f));
            for (int i = 0, length = depAssetBundlePaths.Length; i < length; i++)
            {
                displayOneAssetBundleButton(depAssetBundlePaths[i]);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示一个AssetBundle路径的按钮
        /// </summary>
        /// <param name="assetBundlePath"></param>
        private void displayOneAssetBundleButton(string assetBundlePath)
        {
            if (GUILayout.Button($"{assetBundlePath}", GUILayout.Width(400f), GUILayout.Height(20f)))
            {
                EditorGUIUtility.systemCopyBuffer = assetBundlePath;
                Debug.Log($"复制AssetBundle路径:{assetBundlePath}!");
            }
        }

        /// <summary>
        /// 显示资源使用索引信息UI
        /// </summary>
        private void displayReferenceInfoUI()
        {
            var assetBundleResourceModule = ResourceModuleManager.Singleton.CurrentResourceModule;
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"当前FPS : {assetBundleResourceModule.CurrentFPS}", GUILayout.Width(200f), GUILayout.Height(20f));
            GUILayout.Label($"当前时间 : {Time.time}", GUILayout.Width(200f), GUILayout.Height(20f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"资源回收检测时间间隔 : {assetBundleResourceModule.CheckUnsedResourceTimeInterval}s", GUILayout.Width(200f), GUILayout.Height(20f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (isAssetBundleModule())
            {
                displayAssetBundleReferenceInfoUI();
            }
            else if (isAssetDatabaseModule())
            {
                displayAssetDatabaseReferenceInfoUI();
            }
            else
            {
                GUILayout.Label($"不支持的资源模式:{(ResourceLoadMode)ResourceLoadModeIndex}的索引信息展示!");
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 显示AssetBundle模式的索引信息UI
        /// </summary>
        private void displayAssetBundleReferenceInfoUI()
        {
            var assetBundleResourceModule = ResourceModuleManager.Singleton.CurrentResourceModule;
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"资源最短生存时长 : {assetBundleResourceModule.ResourceMinimumLifeTime}s", GUILayout.Width(200f), GUILayout.Height(20f));
            GUILayout.Label($"单帧最大卸载AB数量 : {assetBundleResourceModule.MaxUnloadABNumberPerFrame}", GUILayout.Width(200f), GUILayout.Height(20f));
            GUILayout.Label($"AssetBundle资源回收FPS门槛 : {assetBundleResourceModule.ResourceRecycleFPSThreshold}", GUILayout.Width(200f), GUILayout.Height(20f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            var normalLoadedAssetBundleInfoMap = assetBundleResourceModule.getSpecificLoadTypeAssetBundleInfoMap(ResourceLoadType.NormalLoad);
            var permanentLoadedAssetBundleInfoMap = assetBundleResourceModule.getSpecificLoadTypeAssetBundleInfoMap(ResourceLoadType.PermanentLoad);
            if (!mTextFilter.Equals(string.Empty))
            {
                if (mFilterTextChanged)
                {
                    mValideReferenceABInfoList.Clear();
                    foreach (var normalLoadedAssetBundleInfo in normalLoadedAssetBundleInfoMap)
                    {
                        if (normalLoadedAssetBundleInfo.Key.Contains(mTextFilter))
                        {
                            mValideReferenceABInfoList.Add(normalLoadedAssetBundleInfo.Value);
                        }
                    }

                    foreach (var permanentLoadedAssetBundleInfo in permanentLoadedAssetBundleInfoMap)
                    {
                        if (permanentLoadedAssetBundleInfo.Key.Contains(mTextFilter))
                        {
                            mValideReferenceABInfoList.Add(permanentLoadedAssetBundleInfo.Value);
                        }
                    }

                    if (mValideReferenceABInfoList.Count > 0)
                    {
                        foreach (var valideReferenceAssetBundleInfo in mValideReferenceABInfoList)
                        {
                            displayOneAssetBundleInfoUI(valideReferenceAssetBundleInfo);
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
                        foreach (var valideReferenceAssetBundleInfo in mValideReferenceABInfoList)
                        {
                            displayOneAssetBundleInfoUI(valideReferenceAssetBundleInfo);
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
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("正常已加载资源信息:");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("正常已加载AssetBundle数量 : {0}", normalLoadedAssetBundleInfoMap.Count));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("可回收正常已加载非常驻AssetBundle数量 : {0}", assetBundleResourceModule.getNormalUnsedABNumber()));
                EditorGUILayout.EndHorizontal();
                foreach (var loadedAssetBundleInfo in normalLoadedAssetBundleInfoMap)
                {
                    displayOneAssetBundleInfoUI(loadedAssetBundleInfo.Value);
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("常驻已加载AssetBundle信息:");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("已加载常驻AssetBundle数量 : {0}", permanentLoadedAssetBundleInfoMap.Count));
                EditorGUILayout.EndHorizontal();
                foreach (var permanentLoadedAssetBundleInfo in permanentLoadedAssetBundleInfoMap)
                {
                    displayOneAssetBundleInfoUI(permanentLoadedAssetBundleInfo.Value);
                }
            }
        }

        /// <summary>
        /// 显示一个AssetBundleInfo的信息
        /// </summary>
        /// <param name="assetBundleInfo"></param>
        private void displayOneAssetBundleInfoUI(AssetBundleInfo assetBundleInfo)
        {
            Color preColor = GUI.color;
            if (assetBundleInfo.IsUnsed)
            {
                GUI.color = Color.yellow;
            }
            if (!mAssetBundleAssetInfoFoldMap.ContainsKey(assetBundleInfo.ResourcePath))
            {
                mAssetBundleAssetInfoFoldMap.Add(assetBundleInfo.ResourcePath, true);
            }
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("AssetBundle路径 : {0}", assetBundleInfo.ResourcePath), GUILayout.Width(600.0f));
            EditorGUILayout.LabelField(string.Format("是否就绪 : {0}", assetBundleInfo.IsReady), GUILayout.Width(100.0f));
            EditorGUILayout.LabelField(string.Format("引用计数 : {0}", assetBundleInfo.RefCount), GUILayout.Width(100.0f));
            EditorGUILayout.LabelField(string.Format("最近使用时间 : {0}", assetBundleInfo.LastUsedTime), GUILayout.Width(150.0f));
            EditorGUILayout.LabelField(string.Format("依赖引用对象列表 : {0}", assetBundleInfo.ReferenceOwnerList.Count == 0 ? "无" : string.Empty), GUILayout.Width(150.0f));
            EditorGUILayout.EndHorizontal();
            foreach (var refowner in assetBundleInfo.ReferenceOwnerList)
            {
                if (refowner.Target != null)
                {
                    EditorGUILayout.ObjectField((UnityEngine.Object)refowner.Target, typeof(UnityEngine.Object), true, GUILayout.Width(200.0f));
                }
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.EndHorizontal();
            if (assetBundleInfo.AllLoadedAssetInfoMap.Count > 0)
            {
                mAssetBundleAssetInfoFoldMap[assetBundleInfo.ResourcePath] = EditorGUILayout.Foldout(mAssetBundleAssetInfoFoldMap[assetBundleInfo.ResourcePath], $"Asset引用信息列表 : {assetBundleInfo.AllLoadedAssetInfoMap.Count}");
                if (mAssetBundleAssetInfoFoldMap[assetBundleInfo.ResourcePath])
                {
                    if (assetBundleInfo.AllLoadedAssetInfoMap.Count > 0)
                    {
                        foreach (var assetInfo in assetBundleInfo.AllLoadedAssetInfoMap)
                        {
                            displayOneAssetInfoUI(assetInfo.Value);
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField($"Asset引用信息列表 : 无", GUILayout.Width(200.0f));
            }
            EditorGUILayout.EndVertical();
            GUI.color = preColor;
        }

        /// <summary>
        /// 显示一个AssetInfo的信息
        /// </summary>
        /// <param name="assetInfo"></param>
        private void displayOneAssetInfoUI(AssetInfo assetInfo)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("Asset路径 : {0}", assetInfo.ResourcePath), GUILayout.Width(600.0f));
            EditorGUILayout.LabelField(string.Format("是否就绪 : {0}", assetInfo.IsReady), GUILayout.Width(100.0f));
            EditorGUILayout.LabelField(string.Format("引用计数 : {0}", assetInfo.RefCount), GUILayout.Width(100.0f));
            EditorGUILayout.LabelField(string.Format("最近使用时间 : {0}", assetInfo.LastUsedTime), GUILayout.Width(150.0f));
            EditorGUILayout.LabelField(string.Format("依赖引用对象列表 : {0}", assetInfo.ReferenceOwnerList.Count == 0 ? "无" : string.Empty), GUILayout.Width(150.0f));
            foreach (var refowner in assetInfo.ReferenceOwnerList)
            {
                if (refowner.Target != null)
                {
                    EditorGUILayout.ObjectField((UnityEngine.Object)refowner.Target, typeof(UnityEngine.Object), true, GUILayout.Width(200.0f));
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 显示加载器信息UI
        /// </summary>
        private void displayLoaderInfoUI()
        {
            if (isAssetBundleModule())
            {
                displayAssetBundleLoaderInfoUI();
            }
            else if (isAssetDatabaseModule())
            {
                displayAssetDatabaseLoaderInfoUI();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"不支持资源加载模式:{(ResourceLoadMode)ResourceLoadModeIndex},无法查看加载器!");
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 显示AssetBundle模式加载器信息UI
        /// </summary>
        private void displayAssetBundleLoaderInfoUI()
        {
            EditorGUILayout.BeginVertical();
            LoaderManager.Singleton.getAllAssetBundleLoader(ref mAllAssetBundleLoader);
            GUILayout.Label(string.Format("AssetBundle加载器信息 : {0}", mAllAssetBundleLoader.Count == 0 ? "无" : string.Empty));
            foreach (var assetBundleLoader in mAllAssetBundleLoader)
            {
                displayOneAssetBundleLoaderInfoUI(assetBundleLoader);
            }
            LoaderManager.Singleton.getAllAssetLoader(ref mAllAssetLoader);
            GUILayout.Label(string.Format("Asset加载器信息 : {0}", mAllAssetLoader.Count == 0 ? "无" : string.Empty));
            foreach (var assetLoader in mAllAssetLoader)
            {
                displayOneAssetLoaderInfoUI(assetLoader);
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 显示一个AssetBundleLoader的加载器信息
        /// </summary>
        /// <param name="abi"></param>
        private void displayOneAssetBundleLoaderInfoUI(BundleLoader bundleLoader)
        {
            var preColor = GUI.color;
            if (bundleLoader.IsUnsed)
            {
                GUI.color = Color.yellow;
            }
            else if (bundleLoader.IsLoading)
            {
                GUI.color = Color.red;
            }
            else if (bundleLoader.IsWaiting)
            {
                GUI.color = Color.gray;
            }
            if (!mAssetBundleLoaderDepFoldMap.ContainsKey(bundleLoader.ResourcePath))
            {
                mAssetBundleLoaderDepFoldMap.Add(bundleLoader.ResourcePath, true);
            }
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("AssetBundle路径 : {0}", bundleLoader.ResourcePath), GUILayout.Width(600.0f));
            EditorGUILayout.LabelField(string.Format("加载状态 : {0}", bundleLoader.LoadState), GUILayout.Width(150.0f));
            EditorGUILayout.LabelField(string.Format("加载方式 : {0}", bundleLoader.LoadMethod), GUILayout.Width(150.0f));
            EditorGUILayout.LabelField(string.Format("加载类型 : {0}", bundleLoader.LoadType), GUILayout.Width(150.0f));
            EditorGUILayout.LabelField(string.Format("依赖资源数量 : {0}", bundleLoader.DepABPaths?.Length), GUILayout.Width(150.0f));
            EditorGUILayout.LabelField(string.Format("已加载AssetBundle资源数量 : {0}", bundleLoader.LoadCompletedAssetBundleNumer - 1), GUILayout.Width(200.0f));
            EditorGUILayout.EndHorizontal();
            var depCount = bundleLoader.DepAssetBundleInfoList.Count;
            if (depCount > 0)
            {
                mAssetBundleLoaderDepFoldMap[bundleLoader.ResourcePath] = EditorGUILayout.Foldout(mAssetBundleLoaderDepFoldMap[bundleLoader.ResourcePath], $"依赖资源加载信息列表 : {depCount}");
                if (mAssetBundleLoaderDepFoldMap[bundleLoader.ResourcePath])
                {
                    if (depCount > 0)
                    {
                        for (int i = 0, length = depCount; i < length; i++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            var depabi = bundleLoader.DepAssetBundleInfoList[i];
                            EditorGUILayout.LabelField($"\t{i}. {depabi.ResourcePath}", GUILayout.Width(600.0f));
                            EditorGUILayout.LabelField($"是否加载完成:{depabi.IsReady}", GUILayout.Width(150f));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField($"依赖资源加载信息列表 : 无", GUILayout.Width(200.0f));
            }
            EditorGUILayout.EndVertical();
            GUI.color = preColor;
        }

        /// <summary>
        /// 显示AssetDatabase模式加载器信息UI
        /// </summary>
        private void displayAssetDatabaseLoaderInfoUI()
        {
            EditorGUILayout.BeginVertical();
            LoaderManager.Singleton.getAllAssetLoader(ref mAllAssetDatabaseLoader);
            GUILayout.Label(string.Format("AssetDatabase加载器信息 : {0}", mAllAssetDatabaseLoader.Count == 0 ? "无" : string.Empty));
            foreach (var waitLoadedAssetLoader in mAllAssetDatabaseLoader)
            {
                displayOneAssetLoaderInfoUI(waitLoadedAssetLoader);
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 显示一个AssetLoader的加载器信息
        /// </summary>
        /// <param name="abi"></param>
        private void displayOneAssetLoaderInfoUI(AssetLoader assetLoader)
        {
            var preColor = GUI.color;
            if(assetLoader.IsUnsed)
            {
                GUI.color = Color.yellow;
            }
            else if (assetLoader.IsLoading)
            {
                GUI.color = Color.red;
            }
            else if(assetLoader.IsWaiting)
            {
                GUI.color = Color.gray;
            }
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("Asset路径 : {0}", assetLoader.ResourcePath), GUILayout.Width(600.0f));
            EditorGUILayout.LabelField(string.Format("加载状态 : {0}", assetLoader.LoadState), GUILayout.Width(150.0f));
            EditorGUILayout.LabelField(string.Format("加载方式 : {0}", assetLoader.LoadMethod), GUILayout.Width(150.0f));
            EditorGUILayout.LabelField(string.Format("加载类型 : {0}", assetLoader.LoadType), GUILayout.Width(150.0f));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            GUI.color = preColor;
        }

        /// <summary>
        /// 显示AssetDatabase模式的索引信息UI
        /// </summary>
        private void displayAssetDatabaseReferenceInfoUI()
        {
            var assetBundleResourceModule = ResourceModuleManager.Singleton.CurrentResourceModule;
            var normalLoadedAssetInfoMap = assetBundleResourceModule.getSpecificLoadTypeAssetInfoMap(ResourceLoadType.NormalLoad);
            var permanentLoadedAssetInfoMap = assetBundleResourceModule.getSpecificLoadTypeAssetInfoMap(ResourceLoadType.PermanentLoad);
            if (!mTextFilter.Equals(string.Empty))
            {
                if (mFilterTextChanged)
                {
                    mValideReferenceAssetInfoList.Clear();
                    foreach (var normalLoadedAssetInfo in normalLoadedAssetInfoMap)
                    {
                        if (normalLoadedAssetInfo.Key.Contains(mTextFilter))
                        {
                            mValideReferenceAssetInfoList.Add(normalLoadedAssetInfo.Value);
                        }
                    }

                    foreach (var permanentLoadedAssetInfo in permanentLoadedAssetInfoMap)
                    {
                        if (permanentLoadedAssetInfo.Key.Contains(mTextFilter))
                        {
                            mValideReferenceAssetInfoList.Add(permanentLoadedAssetInfo.Value);
                        }
                    }

                    if (mValideReferenceAssetInfoList.Count > 0)
                    {
                        foreach (var valideReferenceAssetInfo in mValideReferenceAssetInfoList)
                        {
                            displayOneAssetInfoUI(valideReferenceAssetInfo);
                        }
                    }
                    else
                    {
                        GUILayout.Label(string.Format("找不到资源 : {0}的索引信息!", mTextFilter));
                    }
                }
                else
                {
                    if (mValideReferenceAssetInfoList.Count > 0)
                    {
                        foreach (var valideReferenceAssetInfo in mValideReferenceAssetInfoList)
                        {
                            displayOneAssetInfoUI(valideReferenceAssetInfo);
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
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("正常已加载资源信息:");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("正常已加载Asset数量 : {0}", normalLoadedAssetInfoMap.Count));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("可回收正常已加载非常驻Asset数量 : {0}", assetBundleResourceModule.getNormalUnsedAssetNumber()));
                EditorGUILayout.EndHorizontal();
                foreach (var loadedAssetInfo in normalLoadedAssetInfoMap)
                {
                    displayOneAssetInfoUI(loadedAssetInfo.Value);
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("常驻已加载Asset信息:");
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("已加载常驻Asset数量 : {0}", permanentLoadedAssetInfoMap.Count));
                EditorGUILayout.EndHorizontal();
                foreach (var ploadedAssetInfo in permanentLoadedAssetInfoMap)
                {
                    displayOneAssetInfoUI(ploadedAssetInfo.Value);
                }
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
        /// <param name="abnPath"></param>
        private void forceUnloadSpecificAB(string abnPath)
        {
            if (isAssetBundleModule())
            {
                var assetbundleresourcemodule = ResourceModuleManager.Singleton.CurrentResourceModule as AssetBundleModule;
                assetbundleresourcemodule.forceUnloadSpecificAssetBundle(abnPath);
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

        /// <summary>
        /// 是否是AssetDatabase模式
        /// </summary>
        /// <returns></returns>
        private bool isAssetDatabaseModule()
        {
            return ResourceModuleManager.Singleton.CurrentResourceModule.ResLoadMode == ResourceLoadMode.AssetDatabase;
        }
    }
}
