using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TResource;

public static class ShaderVariantCollector
{
    private enum ESteps
    {
        None,
        Prepare,
        CollectAllMaterial,
        CollectVariants,
        CollectSleeping,
        WaitingDone,
    }

    private const float WaitMilliseconds = 3000f;
    private const float SleepMilliseconds = 3000f;
    private static string _savePath;
    private static int _processMaxNum;
    private static Action _completedCallback;

    private static ESteps _steps = ESteps.None;
    private static System.Diagnostics.Stopwatch _elapsedTime;
    private static List<string> _allMaterials;
    private static List<GameObject> _allSpheres = new List<GameObject>(1000);

    /// <summary>
    /// 材质类型信息
    /// </summary>
    private static readonly Type MaterialType = typeof(Material);

    /// <summary>
    /// Asset和依赖Asset路径数组缓存Map
    /// </summary>
    private static Dictionary<string, string[]> AssetDependencyAssetsCacheMap = new Dictionary<string, string[]>();

    /// <summary>
    /// Asset和Asset类型缓存Map
    /// </summary>
    private static Dictionary<string, Type> AssetTypeCacheMap = new Dictionary<string, Type>();

    /// <summary>
    /// 开始收集
    /// </summary>
    public static void Run(string savePath, int processMaxNum, Action completedCallback)
    {
        if (_steps != ESteps.None)
            return;

        if (Path.HasExtension(savePath) == false)
            savePath = $"{savePath}.shadervariants";
        if (Path.GetExtension(savePath) != ".shadervariants")
            throw new System.Exception("Shader variant file extension is invalid.");

        // 注意：先删除再保存，否则ShaderVariantCollection内容将无法及时刷新
        AssetDatabase.DeleteAsset(savePath);
        FolderUtilities.CreateFileDirectory(savePath);
        _savePath = savePath;
        _processMaxNum = processMaxNum;
        _completedCallback = completedCallback;

        // 聚焦到游戏窗口
        EditorUtilities.FocusUnityGameWindow();

        // 创建临时测试场景
        CreateTempScene();

        _steps = ESteps.Prepare;
        EditorApplication.update += EditorUpdate;
    }

    private static void EditorUpdate()
    {
        if (_steps == ESteps.None)
            return;

        if (_steps == ESteps.Prepare)
        {
            ShaderVariantCollectionHelper.ClearCurrentShaderVariantCollection();
            _steps = ESteps.CollectAllMaterial;
            return; //等待一帧
        }

        if (_steps == ESteps.CollectAllMaterial)
        {
            _allMaterials = GetAllMaterials();
            _steps = ESteps.CollectVariants;
            return; //等待一帧
        }

        if (_steps == ESteps.CollectVariants)
        {
            int count = Mathf.Min(_processMaxNum, _allMaterials.Count);
            List<string> range = _allMaterials.GetRange(0, count);
            _allMaterials.RemoveRange(0, count);
            CollectVariants(range);

            if (_allMaterials.Count > 0)
            {
                _elapsedTime = System.Diagnostics.Stopwatch.StartNew();
                _steps = ESteps.CollectSleeping;
            }
            else
            {
                _elapsedTime = System.Diagnostics.Stopwatch.StartNew();
                _steps = ESteps.WaitingDone;
            }
        }

        if (_steps == ESteps.CollectSleeping)
        {
            if (ShaderUtil.anythingCompiling)
                return;

            if (_elapsedTime.ElapsedMilliseconds > SleepMilliseconds)
            {
                DestroyAllSpheres();
                _elapsedTime.Stop();
                _steps = ESteps.CollectVariants;
            }
        }

        if (_steps == ESteps.WaitingDone)
        {
            // 注意：一定要延迟保存才会起效
            if (_elapsedTime.ElapsedMilliseconds > WaitMilliseconds)
            {
                _elapsedTime.Stop();
                _steps = ESteps.None;

                // 保存结果并创建清单
                ShaderVariantCollectionHelper.SaveCurrentShaderVariantCollection(_savePath);
                //CreateManifest();

                //UnityEngine.Debug.Log($"搜集SVC完毕！");
                Debug.Log($"Shader变体搜集完成！");
                EditorApplication.update -= EditorUpdate;
                _completedCallback?.Invoke();
            }
        }
    }
    private static void CreateTempScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
    }

    /// <summary>
    /// 获取所有打包材质路径列表
    /// </summary>
    /// <returns></returns>
    private static List<string> GetAllMaterials()
    {
        // 获取所有的收集路径
        List<string> collectDirectorys = AssetBundleCollectSettingData.GetAllCollectDirectory();
        var totalCollectNum = collectDirectorys != null ? collectDirectorys.Count : 0;
        if (totalCollectNum == 0)
        {
            Debug.LogWarning("[BuildPatch] 配置的资源收集路径为空");
            return null;
        }
        ClearAssetDependencyAssetsCacheMap();
        ClearAssetTypeCacheMap();
        // Note: 搜集策略存在相互包含的情况，所以可能出现重复分析的Asset
        // 搜集所有材质球
        // 流程如下:
        // 1. 搜索所有配置搜集目录的所有搜集Asset信息(含GUID, AssetPath, DependencyAssetPaths)
        // 2. 遍历所有搜集到的搜集Asset信息和他的所有依赖Asset信息，筛选出所有的材质球Asset类型添加到待搜集HashSet里
        // 3. 针对搜集到的待搜集材质球HashSet遍历进行新场景Camera照射
        int progressValue = 0;
        HashSet<string> result = new HashSet<string>();
        Dictionary<string, CollectAssetInfo> collectAssetInfoMap = new Dictionary<string, CollectAssetInfo>();
        foreach(var collectDirectory in collectDirectorys)
        {
            // 获取所有资源
            string[] guids = AssetDatabase.FindAssets(string.Empty, collectDirectorys.ToArray());
            foreach (string guid in guids)
            {
                string mainAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                // 没计算过的Asset
                if(!collectAssetInfoMap.ContainsKey(mainAssetPath))
                {
                    // 要考虑没包含在搜集策略里的依赖资源
                    string[] dependencyAssetPaths;
                    if(HasAssetDependencyAssetsCache(mainAssetPath))
                    {
                        dependencyAssetPaths = GetAssetDependencyAssetsCache(mainAssetPath);
                    }
                    else
                    {
                        dependencyAssetPaths = AssetDatabase.GetDependencies(mainAssetPath, true);
                        AddAssetDependencyAssetsCache(mainAssetPath, dependencyAssetPaths);
                    }
                    var assetType = AssetDatabase.GetMainAssetTypeAtPath(mainAssetPath);
                    var collectAssetInfo = new CollectAssetInfo(guid, mainAssetPath, assetType, dependencyAssetPaths);
                    collectAssetInfoMap.Add(mainAssetPath, collectAssetInfo);
                    if(!HasAssetTypeCache(mainAssetPath))
                    {
                        AddAssetTypeCache(mainAssetPath, assetType);
                    }
                }
                EditorUtilities.DisplayProgressBar($"搜集目录:{collectDirectory}的所有材质球", ++progressValue, totalCollectNum);
            }
        }

        foreach(var collectAssetInfos in collectAssetInfoMap)
        {
            var collectAssetInfo = collectAssetInfos.Value;
            if (collectAssetInfo.AssetType == MaterialType)
            {
                string assetPath = collectAssetInfo.AssetPath;
                if (!result.Contains(assetPath))
                {
                    result.Add(assetPath);                    
                }
            }
            // 要考虑没包含在搜集策略里的依赖资源
            var dependAssetPaths = collectAssetInfo.DependencyAssetPaths;
            if(dependAssetPaths == null)
            {
                continue;
            }
            foreach(var dependAssetPath in dependAssetPaths)
            {
                // 可能出现重复判定的相同材质Asset
                if (result.Contains(dependAssetPath))
                {
                    continue;
                }
                Type dependAssetType = GetAssetTypeCache(dependAssetPath);
                if(dependAssetType == null)
                {
                    dependAssetType = AssetDatabase.GetMainAssetTypeAtPath(dependAssetPath);
                    AddAssetTypeCache(dependAssetPath, dependAssetType);
                }
                if (dependAssetType == MaterialType)
                {
                    result.Add(dependAssetPath);
                }
            }
        }
        EditorUtilities.ClearProgressBar();
        ClearAssetDependencyAssetsCacheMap();
        ClearAssetTypeCacheMap();
        Debug.Log($"一共搜集到材质球数量:{result.Count}");
        // foreach(var mat in result)
        // {
        //     Debug.Log($"材质球路径:{mat}");
        // }

        // 返回结果
        return result.ToList();
    }
    private static void CollectVariants(List<string> materials)
    {
        Camera camera = Camera.main;
        if (camera == null)
            throw new System.Exception("Not found main camera.");

        // 设置主相机
        float aspect = camera.aspect;
        int totalMaterials = materials.Count;
        float height = Mathf.Sqrt(totalMaterials / aspect) + 1;
        float width = Mathf.Sqrt(totalMaterials / aspect) * aspect + 1;
        float halfHeight = Mathf.CeilToInt(height / 2f);
        float halfWidth = Mathf.CeilToInt(width / 2f);
        camera.orthographic = true;
        camera.orthographicSize = halfHeight;
        camera.transform.position = new Vector3(0f, 0f, -10f);

        // 创建测试球体
        int xMax = (int)(width - 1);
        int x = 0, y = 0;
        int progressValue = 0;
        for (int i = 0; i < materials.Count; i++)
        {
            var material = materials[i];
            var position = new Vector3(x - halfWidth + 1f, y - halfHeight + 1f, 0f);
            var go = CreateSphere(material, position, i);
            if (go != null)
                _allSpheres.Add(go);
            if (x == xMax)
            {
                x = 0;
                y++;
            }
            else
            {
                x++;
            }
            EditorUtilities.DisplayProgressBar("照射所有材质球", ++progressValue, materials.Count);
        }
        EditorUtilities.ClearProgressBar();
    }
    private static GameObject CreateSphere(string assetPath, Vector3 position, int index)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        var shader = material.shader;
        if (shader == null)
            return null;

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.GetComponent<Renderer>().sharedMaterial = material;
        go.transform.position = position;
        go.name = $"Sphere_{index} | {material.name}";
        return go;
    }
    private static void DestroyAllSpheres()
    {
        foreach (var go in _allSpheres)
        {
            GameObject.DestroyImmediate(go);
        }
        _allSpheres.Clear();

        // 尝试释放编辑器加载的资源
        EditorUtility.UnloadUnusedAssetsImmediate(true);
    }
    private static void CreateManifest()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        ShaderVariantCollection svc = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(_savePath);
        if (svc != null)
        {
            var wrapper = ShaderVariantCollectionManifest.Extract(svc);
            string jsonData = JsonUtility.ToJson(wrapper, true);
            string savePath = _savePath.Replace(".shadervariants", ".json");
            File.WriteAllText(savePath, jsonData);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    /// <summary>
    /// 清理Asset和依赖Asset路径数组缓存Map
    /// </summary>
    private static void ClearAssetDependencyAssetsCacheMap()
    {
        AssetDependencyAssetsCacheMap.Clear();
    }

    /// <summary>
    /// 添加Asset和依赖Asset路径数组缓存
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="dependencyAssetPaths"></param>
    /// <returns></returns>
    private static bool AddAssetDependencyAssetsCache(string assetPath, string[] dependencyAssetPaths)
    {
        if (HasAssetDependencyAssetsCache(assetPath))
        {
            Debug.LogError($"不应该重复添加Asset:{assetPath}的依赖Asset路径数组缓存，请检查代码！");
            return false;
        }
        AssetDependencyAssetsCacheMap.Add(assetPath, dependencyAssetPaths);
        return true;
    }
    
    /// <summary>
    /// 获取指定Asset路径的依赖Asset路径数组缓存
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private static string[] GetAssetDependencyAssetsCache(string assetPath)
    {
        string[] dependencyAssetPaths;
        if (AssetDependencyAssetsCacheMap.TryGetValue(assetPath, out dependencyAssetPaths))
        {
            return dependencyAssetPaths;
        }
        return null;
    }

    /// <summary>
    /// 检查指定Asset路径是否存在依赖Asset路径数组缓存
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private static bool HasAssetDependencyAssetsCache(string assetPath)
    {
        return AssetDependencyAssetsCacheMap.ContainsKey(assetPath);
    }

    /// <summary>
    /// 清理Asset类型缓存Map
    /// </summary>
    private static void ClearAssetTypeCacheMap()
    {
        AssetTypeCacheMap.Clear();
    }

    /// <summary>
    /// 添加Asset类型缓存
    /// </summary>
    /// <param name="assetPath"></param>
    /// <param name="assetType"></param>
    /// <returns></returns>
    private static bool AddAssetTypeCache(string assetPath, Type assetType)
    {
        if (HasAssetTypeCache(assetPath))
        {
            Debug.LogError($"不应该重复添加Asset:{assetPath}的Asset类型缓存，请检查代码！");
            return false;
        }
        AssetTypeCacheMap.Add(assetPath, assetType);
        return true;
    }

    /// <summary>
    /// 获取指定Asset路径的Asset类型缓存
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private static Type GetAssetTypeCache(string assetPath)
    {
        Type assetType;
        if (AssetTypeCacheMap.TryGetValue(assetPath, out assetType))
        {
            return assetType;
        }
        return null;
    }

    /// <summary>
    /// 检查指定Asset路径是否存在Asset类型缓存
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    private static bool HasAssetTypeCache(string assetPath)
    {
        return AssetTypeCacheMap.ContainsKey(assetPath);
    }
}