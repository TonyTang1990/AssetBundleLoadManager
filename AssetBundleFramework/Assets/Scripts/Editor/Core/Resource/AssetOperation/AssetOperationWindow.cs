/*
 * Description:             AssetOperationWindow.cs
 * Author:                  TANGHUAN
 * Create Date:             2019//11/21
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Asset处理窗口
/// </summary>
public class AssetOperationWindow : BaseEditorWindow
{
    /// <summary>
    /// Asset处理类型
    /// </summary>
    public enum EAssetOperationType
    {
        Invalide = 1,                               // 无效类型
        FastSetAssetABName,                         // 快速设置选中Asset指定AB名字
        AssetDependencyBrowser,                     // Asset依赖文件查看类型
        AssetBuildInResourceRefAnalyze,             // Asset内置资源引用统计类型
        AssetBuildInResourceRefExtraction,          // Asset内置资源引用提取类型
    }

    /// <summary>
    /// Asset处理类型
    /// </summary>
    private EAssetOperationType mAssetOperationType = EAssetOperationType.Invalide;

    /// <summary>
    /// 滚动位置
    /// </summary>
    private Vector2 uiScrollPos;

    [MenuItem("Tools/Assets/Asset相关处理工具", false, 101)]
    public static void dpAssetBrowser()
    {
        var assetoperationwindow = EditorWindow.GetWindow<AssetOperationWindow>(false, "Asset处理工具");
        assetoperationwindow.Show();
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        includeIndirectDp = GUILayout.Toggle(includeIndirectDp, "是否包含间接引用", GUILayout.Width(120.0f));
        GUILayout.Label("资源后缀过滤:", GUILayout.Width(80.0f));
        postFixFilter = GUILayout.TextField(postFixFilter, GUILayout.MaxWidth(150.0f));
        if (GUILayout.Button("查看选中Asset依赖", GUILayout.MaxWidth(150.0f)))
        {
            mAssetOperationType = EAssetOperationType.AssetDependencyBrowser;
            refreshAssetDepBrowserSelections();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label("AB名字:", GUILayout.Width(50.0f));
        fastSetABName = GUILayout.TextField(fastSetABName, GUILayout.MaxWidth(150.0f));
        if (GUILayout.Button("设置选中Asset AB名字", GUILayout.MaxWidth(150.0f)))
        {
            mAssetOperationType = EAssetOperationType.FastSetAssetABName;
            doSetAssetABName();
        }
        GUILayout.Label("AB名字为空表示以Asset自身名字小写为AB名", GUILayout.Width(300.0f));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(10);
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("统计内置资源引用Asset", GUILayout.MaxWidth(150.0f)))
        {
            mAssetOperationType = EAssetOperationType.AssetBuildInResourceRefAnalyze;
            analyzeBuildInResourceReferenceAsset();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("提取选中对象内置资源", GUILayout.MaxWidth(150.0f)))
        {
            mAssetOperationType = EAssetOperationType.AssetBuildInResourceRefExtraction;
            var selectiongo = Selection.activeGameObject;
            if (selectiongo != null)
            {
                extractBuildInResource(selectiongo);
            }
            else
            {
                Debug.Log("请先选中有效提取对象,只支持GameObject!");
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.Space(10);
        displayAssetOperationResult();
    }

    /// <summary>
    /// 显示内置资源引用结果
    /// </summary>
    private void displayAssetOperationResult()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label("Asset处理类型:" + mAssetOperationType);
        GUILayout.EndHorizontal();
        switch (mAssetOperationType)
        {
            case EAssetOperationType.FastSetAssetABName:
                displayFastSetABNameResult();
                break;
            case EAssetOperationType.AssetDependencyBrowser:
                displayAssetDependencyBrowserResult();
                break;
            case EAssetOperationType.AssetBuildInResourceRefAnalyze:
                displayAssetBuildInResourceRefAnalyze();
                break;
            case EAssetOperationType.AssetBuildInResourceRefExtraction:
                displayAssetBuildInResourceRefExtractionResult();
                break;
            default:
                displayInvalideResult();
                break;
        }
    }

    #region 快速设置Asset AB名字
    /// <summary>
    /// 快速设置的AB名字
    /// </summary>
    private string fastSetABName = string.Empty;

    /// <summary>
    /// 设置选中Asset AB名字
    /// </summary>
    private void doSetAssetABName()
    {
        for (int i = 0, length = Selection.objects.Length; i < length; i++)
        {
            var obj = Selection.objects[i];
            EditorUtility.DisplayProgressBar("设置选中Asset AB名字", $"当前处理Asset:{obj.name}!", (i + 1) / length);
            AssetBundleNameSetting.AutoSetAssetBundleName(obj, fastSetABName);
        }
        EditorUtility.ClearProgressBar();
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 显示快速设置Asset AB名字结果
    /// </summary>
    private void displayFastSetABNameResult()
    {
        GUILayout.BeginVertical();
        GUILayout.Label($"快速设置AB名字为:{fastSetABName}!", GUILayout.Width(200.0f));
        GUILayout.EndVertical();
    }
    #endregion

    #region Asset依赖文件查看
    /// <summary>
    /// 依赖资源映射map
    /// Key为选中Asset的路径，Value依赖Asset的路径列表
    /// </summary>
    private Dictionary<string, List<string>> dpAssetInfoMap = new Dictionary<string, List<string>>();

    /// <summary>
    /// 是否包含间接引用
    /// </summary>
    private bool includeIndirectDp = false;

    /// <summary>
    /// 资源后缀过滤
    /// </summary>
    private string postFixFilter = string.Empty;

    /// <summary>
    /// 显示Asset依赖资源浏览结果
    /// </summary>
    private void displayAssetDependencyBrowserResult()
    {
        GUILayout.BeginVertical();
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos);
        foreach (var dpassetinfo in dpAssetInfoMap)
        {
            showAssetDpUI(dpassetinfo.Key, dpassetinfo.Value);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示依赖资源信息UI
    /// </summary>
    /// <param name="assetpath"></param>
    /// <param name="dpassetpath"></param>
    private void showAssetDpUI(string assetpath, List<string> dpassetpathlist)
    {
        GUILayout.BeginVertical();
        GUILayout.Label("主Asset路径:");
        GUILayout.Label(assetpath);
        GUILayout.Label("依赖Asset路径:");
        foreach (var dpassetpath in dpassetpathlist)
        {
            if (postFixFilter.Equals(string.Empty))
            {
                GUILayout.Label(dpassetpath);
            }
            else
            {
                if (dpassetpath.EndsWith(postFixFilter))
                {
                    GUILayout.Label(dpassetpath);
                }
            }
        }
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 刷新选中Asset依赖数据浏览
    /// </summary>
    private void refreshAssetDepBrowserSelections()
    {
        dpAssetInfoMap.Clear();
        var selections = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
        foreach (var selection in selections)
        {
            var selectionassetpath = AssetDatabase.GetAssetPath(selection);
            var dpassets = AssetDatabase.GetDependencies(selectionassetpath, includeIndirectDp);
            if (!dpAssetInfoMap.ContainsKey(selectionassetpath))
            {
                dpAssetInfoMap.Add(selectionassetpath, new List<string>());
            }
            foreach (var dpasset in dpassets)
            {
                dpAssetInfoMap[selectionassetpath].Add(dpasset);
            }
        }
    }
    #endregion

    #region Asset内置资源引用统计
    /// <summary>
    /// 依赖内置资源的Asset映射map
    /// Key为选中Asset的路径，Value为Asset使用了内置资源的节点名和资源名的列表
    /// </summary>
    private Dictionary<string, List<KeyValuePair<string, string>>> mReferenceBuildInResourceAssetMap = new Dictionary<string, List<KeyValuePair<string, string>>>();

    /// <summary>
    /// 显示内置资源引用统计结果
    /// </summary>
    private void displayAssetBuildInResourceRefAnalyze()
    {
        GUILayout.BeginVertical();
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos);
        foreach (var referenceasset in mReferenceBuildInResourceAssetMap)
        {
            showBIResourceReferenceAssetUI(referenceasset.Key, referenceasset.Value);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示使用了内置资源信息UI
    /// </summary>
    /// <param name="assetpath"></param>
    /// <param name="biassetinfolist"></param>
    private void showBIResourceReferenceAssetUI(string assetpath, List<KeyValuePair<string, string>> biassetinfolist)
    {
        GUILayout.Label("Asset路径:");
        GUILayout.Label(assetpath);
        if (biassetinfolist.Count > 0)
        {
            GUILayout.Label("内置资源使用信息:");
            foreach (var biassetinfo in biassetinfolist)
            {
                GUILayout.Label(string.Format("节点名:{0}, 资源名:{1}", biassetinfo.Key, biassetinfo.Value));
            }
        }
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
    }

    /// <summary>
    /// 统计内置资源引用Asset
    /// </summary>
    private void analyzeBuildInResourceReferenceAsset()
    {
        //  主要检查以下几个方面：
        //  1. Graphic的Texture和Matetial
        //  2. MeshRenderer的Material
        //  3. ParticleSystem的Material
        //  4. Material的Shader
        mReferenceBuildInResourceAssetMap.Clear();
        var allmatfiles = Directory.GetFiles("Assets/", "*.mat", SearchOption.AllDirectories);
        var assetinfolist = new List<KeyValuePair<string, string>>();
        foreach (var matfile in allmatfiles)
        {
            var matasset = AssetDatabase.LoadAssetAtPath<Material>(matfile);
            if (isUsingBuildInResource<Material>(matasset, ref assetinfolist))
            {
                tryAddBuildInShaderReferenceAsset(matfile, assetinfolist);
            }
        }
        var allprefabfiles = Directory.GetFiles("Assets/", "*.prefab", SearchOption.AllDirectories);
        foreach (var prefabfile in allprefabfiles)
        {
            var prefabasset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabfile);
            if (isUsingBuildInResource<GameObject>(prefabasset, ref assetinfolist))
            {
                tryAddBuildInShaderReferenceAsset(prefabfile, assetinfolist);
            }
        }
    }

    /// <summary>
    /// 指定材质是否使用了内置资源
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="assetinfolist"></param>
    /// <returns></returns>
    private bool isUsingBuildInResource<T>(T asset, ref List<KeyValuePair<string, string>> assetinfolist) where T : UnityEngine.Object
    {
        assetinfolist.Clear();
        if (typeof(T) == typeof(Material))
        {
            var mat = asset as Material;
            if (mat.shader != null && EditorResourceUtilities.isBuildInResource(mat.shader))
            {
                assetinfolist.Add(new KeyValuePair<string, string>("无", mat.shader.name));
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (typeof(T) == typeof(GameObject))
        {
            var prefab = asset as GameObject;
            // 人物模型和粒子特效
            var allrenders = prefab.GetComponentsInChildren<Renderer>();
            var isusingbuildinresource = false;
            foreach (var render in allrenders)
            {
                //用到的内置asset名(采用拼凑的方式 e.g. Default.mat Sprite.png)
                var usedassetname = string.Empty;
                if (render.sharedMaterial != null)
                {
                    if (EditorResourceUtilities.isBuildInResource(render.sharedMaterial))
                    {
                        usedassetname += render.sharedMaterial.name;
                        isusingbuildinresource = true;
                    }
                    if (render.sharedMaterial.shader != null)
                    {
                        if (EditorResourceUtilities.isBuildInResource(render.sharedMaterial.shader))
                        {
                            usedassetname += " " + render.sharedMaterial.shader.name;
                            isusingbuildinresource = true;
                        }
                    }
                    if (!usedassetname.IsNullOrEmpty())
                    {
                        assetinfolist.Add(new KeyValuePair<string, string>(render.transform.name, usedassetname));
                    }
                }
            }
            // UI组件
            var allgraphics = prefab.GetComponentsInChildren<Graphic>();
            foreach (var graphic in allgraphics)
            {
                //用到的内置asset名(采用拼凑的方式 e.g. Default.mat Sprite.png)
                var usedassetname = string.Empty;
                if (graphic.material != null)
                {
                    if (EditorResourceUtilities.isBuildInResource(graphic.material))
                    {
                        usedassetname += graphic.material.name;
                        isusingbuildinresource = true;
                    }
                    if (graphic.material.shader != null)
                    {
                        if (EditorResourceUtilities.isBuildInResource(graphic.material.shader))
                        {
                            usedassetname += " " + graphic.material.shader.name;
                            isusingbuildinresource = true;
                        }
                    }
                }
                if (graphic.mainTexture != null)
                {
                    if (EditorResourceUtilities.isBuildInResource(graphic.mainTexture))
                    {
                        usedassetname += " " + graphic.mainTexture.name;
                        isusingbuildinresource = true;
                    }
                }
                if (!usedassetname.IsNullOrEmpty())
                {
                    assetinfolist.Add(new KeyValuePair<string, string>(graphic.transform.name, usedassetname));
                }
            }
            return isusingbuildinresource;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 尝试添加Asset的内置Shader的引用信息
    /// </summary>
    /// <param name="assetpath"></param>
    /// <param name="assetinfolist"></param>
    /// <returns></returns>
    private void tryAddBuildInShaderReferenceAsset(string assetpath, List<KeyValuePair<string, string>> assetinfolist = null)
    {
        if (!mReferenceBuildInResourceAssetMap.ContainsKey(assetpath))
        {
            mReferenceBuildInResourceAssetMap.Add(assetpath, new List<KeyValuePair<string, string>>());
        }
        foreach (var assetinfo in assetinfolist)
        {
            mReferenceBuildInResourceAssetMap[assetpath].Add(assetinfo);
        }
    }
    #endregion

    #region Asset内置资源引用提取
    /// <summary>
    /// 内置材质提取相对输出目录
    /// </summary>
    private const string mBuildInMaterialExtractRelativeOutputFolderPath = "/Res/buildinresources/buildinmaterials/";

    /// <summary>
    /// 内置材质提取相对输出目录
    /// </summary>
    private const string mBuildInTextureExtractRelativeOutputFolderPath = "/Res/buildinresources/buildintextures/";

    /// <summary>
    /// 内置资源提取结果
    /// </summary>
    private bool mBuildInResourceExtractionResult;

    /// <summary>
    /// 内置资源提取结果列表
    /// </summary>
    private List<string> mBuildInResourceExtractedList = new List<string>();

    /// <summary>
    /// Asset内置资源提取结果
    /// </summary>
    private void displayAssetBuildInResourceRefExtractionResult()
    {
        GUILayout.BeginVertical();
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos);
        GUILayout.Label(string.Format("内置资源提取{0}!", mBuildInResourceExtractionResult == false ? "进行中" : "完成"));
        GUILayout.Label("内置资源提取结果列表:");
        foreach (var extractedres in mBuildInResourceExtractedList)
        {
            GUILayout.Label(extractedres);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 提取内置资源
    /// </summary>
    /// <param name="asset"></param>
    private void extractBuildInResource(UnityEngine.GameObject asset)
    {
        mBuildInResourceExtractionResult = false;
        var materialextractoutputfolderpath = Application.dataPath + mBuildInMaterialExtractRelativeOutputFolderPath;
        var textureextractoutputfolderpath = Application.dataPath + mBuildInTextureExtractRelativeOutputFolderPath;
        Debug.Log("materialextractoutputfolderpath = " + materialextractoutputfolderpath);
        Debug.Log("textureextractoutputfolderpath = " + textureextractoutputfolderpath);
        FolderUtilities.CheckAndCreateSpecificFolder(materialextractoutputfolderpath);
        FolderUtilities.CheckAndCreateSpecificFolder(textureextractoutputfolderpath);
        var referencebuildinobjectlist = getReferenceBuildInResourceExcludeShader(asset);
        referencebuildinobjectlist = referencebuildinobjectlist.Distinct().ToList();
        Debug.Log(string.Format("引用内置资源数量:{0}", referencebuildinobjectlist.Count));
        mBuildInResourceExtractedList.Clear();
        foreach (var buildinobject in referencebuildinobjectlist)
        {
            if (buildinobject is Material mt)
            {
                Material mat = GameObject.Instantiate<Material>(mt);
                var outputfolderpath = "Assets" + mBuildInMaterialExtractRelativeOutputFolderPath + buildinobject.name + ".mat";
                Debug.Log(string.Format("材质输出相对路径:{0}", outputfolderpath));
                AssetDatabase.CreateAsset(mat, outputfolderpath);
                mBuildInResourceExtractedList.Add(outputfolderpath);
            }
            else if (buildinobject is Texture)
            {
                var texturepreview = AssetPreview.GetAssetPreview(buildinobject);
                var outputfolderpath = "Assets" + mBuildInTextureExtractRelativeOutputFolderPath + buildinobject.name + ".png";
                Debug.Log(string.Format("纹理输出相对路径:{0}", outputfolderpath));
                File.WriteAllBytes(outputfolderpath, texturepreview.EncodeToPNG());
                mBuildInResourceExtractedList.Add(outputfolderpath);
            }
            else
            {
                Debug.LogError(string.Format("不支持导出的资源类型:{0}", buildinobject.GetType()));
            }
        }
        mBuildInResourceExtractionResult = true;
    }

    /// <summary>
    /// 获取非Shader的内置资源引用
    /// </summary>
    /// <param name="asset"></param>
    /// <returns></returns>
    private List<UnityEngine.Object> getReferenceBuildInResourceExcludeShader(UnityEngine.GameObject asset)
    {
        // 主要提起以下几种资源:
        // 1. 内置Texture
        // 2. 内置材质
        // 人物模型和粒子特效
        var assetlist = new List<UnityEngine.Object>();
        var allrenders = asset.GetComponentsInChildren<Renderer>();
        foreach (var render in allrenders)
        {
            if (render.sharedMaterial != null && EditorResourceUtilities.isBuildInResource(render.sharedMaterial))
            {
                assetlist.Add(render.sharedMaterial);
            }
        }
        // UI组件
        var allgraphics = asset.GetComponentsInChildren<Graphic>();
        foreach (var graphic in allgraphics)
        {
            if (graphic.material != null && EditorResourceUtilities.isBuildInResource(graphic.material))
            {
                assetlist.Add(graphic.material);
            }
            if (graphic.mainTexture != null && EditorResourceUtilities.isBuildInResource(graphic.mainTexture))
            {
                assetlist.Add(graphic.mainTexture);
            }
        }
        return assetlist;
    }
    #endregion

    #region 默认无效类型
    /// <summary>
    /// 显示无效类型结果
    /// </summary>
    private void displayInvalideResult()
    {
        GUILayout.BeginVertical();
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos);
        GUILayout.Label("没有有效操作!");
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }
    #endregion
}