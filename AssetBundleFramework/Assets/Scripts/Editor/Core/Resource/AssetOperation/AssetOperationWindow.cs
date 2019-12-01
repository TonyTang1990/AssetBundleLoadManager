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
public class AssetOperationWindow : EditorWindow
{
    /// <summary>
    /// Asset处理类型
    /// </summary>
    public enum EAssetOperationType
    {
        Invalide = 1,                               // 无效类型
        AssetDependencyBrowser,                     // Asset依赖文件查看类型
        AssetBuildInResourceRefAnalyze,             // Asset内置资源引用统计类型
        AssetBuildInResourceRefExtraction,          // Asset内置资源引用提取类型
        ShaderVariantsCollection,                   // Shader变体搜集窗口
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
        var assetoperationwindow = EditorWindow.GetWindow<AssetOperationWindow>();
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
        GUILayout.Space(10);
        if (GUILayout.Button("搜集Shader变体", GUILayout.MaxWidth(150.0f)))
        {
            mAssetOperationType = EAssetOperationType.ShaderVariantsCollection;
            collectAllShaderVariants();
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
            case EAssetOperationType.AssetDependencyBrowser:
                displayAssetDependencyBrowserResult();
                break;
            case EAssetOperationType.AssetBuildInResourceRefAnalyze:
                displayAssetBuildInResourceRefAnalyze();
                break;
            case EAssetOperationType.AssetBuildInResourceRefExtraction:
                displayAssetBuildInResourceRefExtractionResult();
                break;
            case EAssetOperationType.ShaderVariantsCollection:
                displayShaderVariantsCollectionResult();
                break;
            default:
                displayInvalideResult();
                break;
        }
    }

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
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos, GUILayout.MaxWidth(2000.0f), GUILayout.MaxHeight(800.0f));
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
        GUILayout.EndScrollView();
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
        Utilities.CheckAndCreateSpecificFolder(materialextractoutputfolderpath);
        Utilities.CheckAndCreateSpecificFolder(textureextractoutputfolderpath);
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

    #region Shader变体搜集
    /// <summary>
    /// Shader变体搜集跟目录
    /// </summary>
    private string ShaderCollectRootFolderPath;

    /// <summary>
    /// Shader变体搜集文件输出目录
    /// </summary>
    private string ShaderVariantOuputFolderPath;

    /// <summary>
    /// 变体搜集Cube父节点(方便统一删除)
    /// </summary>
    private GameObject SVCCubeParentGo;

    /// <summary>
    /// Shader变体搜集结果
    /// </summary>
    private bool mShaderVariantCollectionResult;

    /// <summary>
    /// 需要搜集的材质Asset路径列表
    /// </summary>
    private List<string> mMaterialNeedCollectedList = new List<string>();

    /// <summary>
    /// 显示变体搜集结果
    /// </summary>
    private void displayShaderVariantsCollectionResult()
    {
        GUILayout.BeginVertical();
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos);
        GUILayout.Label(string.Format("Shader变体搜集{0}!", mShaderVariantCollectionResult == false ? "进行中" : "完成"));
        GUILayout.Label("需要搜集的材质资源路径:");
        foreach (var matcollectedpath in mMaterialNeedCollectedList)
        {
            GUILayout.Label(matcollectedpath);
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 搜集所有的Shader变体
    /// </summary>
    private async void collectAllShaderVariants()
    {
        mShaderVariantCollectionResult = false;
        ShaderCollectRootFolderPath = Application.dataPath;
        ShaderVariantOuputFolderPath = Application.dataPath + "/Res/shadervariants";
        var preactivescene = EditorSceneManager.GetActiveScene();
        var preactivescenepath = preactivescene.path;
        Debug.Log(string.Format("之前打开的场景资源路径:{0}", preactivescenepath));
        // Shader变体搜集流程
        // 1. 打开Shader变体搜集场景
        // 2. 清除Shader变体搜集数据
        // 3. 并排创建使用每一个有效材质的Cube渲染一帧
        // 4. 触发变体搜集并保存变体搜集文件
        Debug.Log("开始搜集Shader变体!");
        EditorUtility.DisplayProgressBar("Shader变体收集", "打开Shader变体收集场景!", 0.0f);
        await openShaderVariantsCollectSceneAsync();
        EditorUtility.DisplayProgressBar("Shader变体收集", "清除Shader变体数据!", 0.2f);
        await clearAllShaderVariantsAsync();
        EditorUtility.DisplayProgressBar("Shader变体收集", "创建Shader变体收集所需Cube!", 0.3f);
        await createAllValideMaterialCudeAsync();
        EditorUtility.DisplayProgressBar("Shader变体收集", "执行Shader变体收集!", 0.5f);
        await doShaderVariantsCollectAsync();
        EditorUtility.DisplayProgressBar("Shader变体收集", "完成Shader变体收集!", 1.0f);
        Debug.Log("结束搜集Shader变体!");
        // 打开之前的场景
        EditorSceneManager.OpenScene(preactivescenepath);
        EditorUtility.ClearProgressBar();
        mShaderVariantCollectionResult = true;
    }

    /// <summary>
    /// 打开Shader变体搜集场景
    /// </summary>
    private async Task openShaderVariantsCollectSceneAsync()
    {
        Debug.Log("openShaderVariantsCollectScene()");
        EditorSceneManager.OpenScene("Assets/Res/scenes/ShaderVariantsCollectScene.unity");
        Debug.Log("打开Shader变体收集场景!");
        await Task.Delay(1000);
    }

    /// <summary>
    /// 清除Shader变体数据
    /// </summary>
    private async Task clearAllShaderVariantsAsync()
    {
        Debug.Log("clearAllShaderVariants()");
        MethodInfo clearcurrentsvc = typeof(ShaderUtil).GetMethod("ClearCurrentShaderVariantCollection", BindingFlags.NonPublic | BindingFlags.Static);
        clearcurrentsvc.Invoke(null, null);
        Debug.Log("清除Shader变体数据!");
        await Task.Delay(1000);
    }

    /// <summary>
    /// 创建所有有效材质的对应Cube
    /// </summary>
    private async Task createAllValideMaterialCudeAsync()
    {
        Debug.Log("createAllValideMaterialCude()");
        SVCCubeParentGo = new GameObject("SVCCubeParentGo");
        SVCCubeParentGo.transform.position = Vector3.zero;
        var posoffset = new Vector3(0.05f, 0f, 0f);
        var cubeworldpos = Vector3.zero;
        var svccubetemplate = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/prefabs/pre_SVCCube.prefab");
        var allmatassets = getAllValideMaterial();
        mMaterialNeedCollectedList.Clear();
        Debug.Log(string.Format("需要搜集的材质数量:{0}", allmatassets));
        for (int i = 0, length = allmatassets.Count; i < length; i++)
        {
            var matassetpath = AssetDatabase.GetAssetPath(allmatassets[i]);
            mMaterialNeedCollectedList.Add(matassetpath);
            var cube = GameObject.Instantiate<GameObject>(svccubetemplate);
            cube.name = allmatassets[i].name + "Cube";
            cube.transform.position = posoffset * i;
            cube.GetComponent<MeshRenderer>().material = allmatassets[i];
            cube.transform.SetParent(SVCCubeParentGo.transform);
        }
        EditorSceneManager.SaveOpenScenes();
        //延时等待一会，确保变体数据更新
        Debug.Log("创建完Cube，开始等待5秒!");
        await Task.Delay(5000);
        Debug.Log("创建完Cube，等待5秒完成!");
    }

    /// <summary>
    /// 执行变体搜集
    /// </summary>
    private async Task doShaderVariantsCollectAsync()
    {
        Debug.Log("doShaderVariantsCollect()");
        ShaderVariantOuputFolderPath = Application.dataPath + "/Res/shadervariants/";
        var outputassetsindex = ShaderVariantOuputFolderPath.IndexOf("Assets");
        var outputrelativepath = ShaderVariantOuputFolderPath.Substring(outputassetsindex, ShaderVariantOuputFolderPath.Length - outputassetsindex);
        var svcoutputfilepath = outputrelativepath + "DIYShaderVariantsCollection.shadervariants";
        Debug.Log(string.Format("Shader变体文件输出目录:{0}", ShaderVariantOuputFolderPath));
        Debug.Log(string.Format("Shader变体文件输出相对路径:{0}", svcoutputfilepath));
        if (!Directory.Exists(ShaderVariantOuputFolderPath))
        {
            Debug.Log(string.Format("Shader变体文件输出目录:{0}不存在，重新创建一个!", ShaderVariantOuputFolderPath));
            Directory.CreateDirectory(ShaderVariantOuputFolderPath);
        }
        EditorSceneManager.SaveOpenScenes();
        MethodInfo savecurrentsvc = typeof(ShaderUtil).GetMethod("SaveCurrentShaderVariantCollection", BindingFlags.NonPublic | BindingFlags.Static);
        savecurrentsvc.Invoke(null, new object[] { svcoutputfilepath });
        // 直接设置AB名字和Shader打包到一起
        var svcassetimporter = AssetImporter.GetAtPath(svcoutputfilepath);
        if (svcassetimporter != null)
        {
            svcassetimporter.assetBundleName = "shaderlist";
            DIYLog.Log(string.Format("设置资源:{0}的AB名字为:shaderlist", svcoutputfilepath));
            AssetDatabase.SaveAssets();
        }
        GameObject.DestroyImmediate(SVCCubeParentGo);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("保存完Shader变体文件!");
        await Task.Delay(1000);
    }

    /// <summary>
    /// 获取所有有效材质(有效是指有使用Shader)
    /// </summary>
    /// <returns></returns>
    private List<Material> getAllValideMaterial()
    {
        ShaderCollectRootFolderPath = Application.dataPath;
        var assetsindex = ShaderCollectRootFolderPath.IndexOf("Assets");
        var collectrelativepath = ShaderCollectRootFolderPath.Substring(assetsindex, ShaderCollectRootFolderPath.Length - assetsindex);
        var assets = AssetDatabase.FindAssets("t:Prefab", new string[] { collectrelativepath }).ToList();
        var assets2 = AssetDatabase.FindAssets("t:Material", new string[] { collectrelativepath });
        assets.AddRange(assets2);

        List<Material> allmatassets = new List<Material>();
        List<string> allmats = new List<string>();

        //GUID to assetPath
        for (int i = 0; i < assets.Count; i++)
        {
            var p = AssetDatabase.GUIDToAssetPath(assets[i]);
            //获取依赖中的mat
            var dependenciesPath = AssetDatabase.GetDependencies(p, true);

            var mats = dependenciesPath.ToList().FindAll((dp) => dp.EndsWith(".mat"));
            allmats.AddRange(mats);
        }

        //处理所有的 material
        allmats = allmats.Distinct().ToList();

        foreach (var mat in allmats)
        {
            var obj = AssetDatabase.LoadMainAssetAtPath(mat);
            if (obj is Material)
            {
                allmatassets.Add(obj as Material);
            }
        }
        return allmatassets;
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