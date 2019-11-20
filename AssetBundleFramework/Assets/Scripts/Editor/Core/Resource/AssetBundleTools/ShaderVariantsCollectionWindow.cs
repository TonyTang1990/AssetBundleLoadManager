/*
 * Description:             ShaderVariantsCollectionWindow.cs
 * Author:                  TANGHUAN
 * Create Date:             2019/11/19
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Shader变体搜集窗口
/// </summary>
public class ShaderVariantsCollectionWindow : EditorWindow
{
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
    /// 是否完成Shader变体收集
    /// </summary>
    private bool mCompleteSVC;

    /// <summary>
    /// 是否完成动态Cube创建
    /// </summary>
    private bool mCompleteDynamicCubeInstantiate;

    /// <summary>
    /// 动态创建Cube的生存时长
    /// </summary>
    private const float mDynamicCubeLifeTime = 2.0f;

    /// <summary>
    /// 经历的时间
    /// </summary>
    private float mTimePassed;

    [MenuItem("Tools/Assetbundle/Shader变体搜集窗口", false)]
    public static void shaderVaraintsCollectWindow()
    {
        var shadervariantscolectionwindow = EditorWindow.GetWindow<ShaderVariantsCollectionWindow>();
        shadervariantscolectionwindow.Show();
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("搜集Shader变体", GUILayout.MaxWidth(150.0f)))
        {
            collectAllShaderVariants();
        }
        if (GUILayout.Button("切换到变体搜集场景", GUILayout.MaxWidth(150.0f)))
        {
            openShaderVariantsCollectScene();
        }
        if (GUILayout.Button("清理变体", GUILayout.MaxWidth(150.0f)))
        {
            clearAllShaderVariants();
        }
        if (GUILayout.Button("创建Cube使用有效材质", GUILayout.MaxWidth(150.0f)))
        {
            createAllValideMaterialCude();
        }
        if (GUILayout.Button("触发变体搜集", GUILayout.MaxWidth(150.0f)))
        {
            doShaderVariantsCollect();
        }
        if (GUILayout.Button("测试Async Await", GUILayout.MaxWidth(150.0f)))
        {
            Debug.Log("Before Call TestAsync()");
            var task = TestAsync1();
            task.Wait();
            Debug.Log("After Call TestAsync()");
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    void Update()
    {
        //if (mCompleteDynamicCubeInstantiate)
        //{
        //    mTimePassed += Time.deltaTime;

        //    if (mTimePassed >= mDynamicCubeLifeTime)
        //    {
        //        mCompleteDynamicCubeInstantiate = false;
        //        mTimePassed = 0f;
        //        doShaderVariantsCollect();
        //    }
        //}
    }

    /// <summary>
    /// 搜集所有的Shader变体
    /// </summary>
    private void collectAllShaderVariants()
    {
        ShaderCollectRootFolderPath = Application.dataPath;
        ShaderVariantOuputFolderPath = Application.dataPath + "/Res/shadervariants";
        mCompleteSVC = false;
        mCompleteDynamicCubeInstantiate = false;
        // Shader变体搜集流程
        // 1. 打开Shader变体搜集场景
        // 2. 清除Shader变体搜集数据
        // 3. 并排创建使用每一个有效材质的Cube渲染一帧
        // 4. 触发变体搜集并保存变体搜集文件
        openShaderVariantsCollectScene();//.Wait();
        clearAllShaderVariants();//.Wait();
        createAllValideMaterialCude();//.Wait();
        doShaderVariantsCollect();//.Wait();

        //ShaderCollection.GenShaderVariant(ShaderCollectRootFolderPath, ShaderVariantOuputFolderPath);
    }

    /// <summary>
    /// 清除Shader变体数据
    /// </summary>
    private void clearAllShaderVariants()
    {
        Debug.Log("clearAllShaderVariants()");
        MethodInfo clearcurrentsvc = typeof(ShaderUtil).GetMethod("ClearCurrentShaderVariantCollection", BindingFlags.NonPublic | BindingFlags.Static);
        clearcurrentsvc.Invoke(null, null);
        Debug.Log("清除Shader变体数据!");
        //await Task.Delay(1000);
    }

    /// <summary>
    /// 打开Shader变体搜集场景
    /// </summary>
    private void openShaderVariantsCollectScene()
    {
        Debug.Log("openShaderVariantsCollectScene()");
        EditorSceneManager.OpenScene("Assets/Res/scenes/ShaderVariantsCollectScene.unity");
        Debug.Log("打开Shader变体收集场景!");
        //await Task.Delay(1000);
    }

    /// <summary>
    /// 创建所有有效材质的对应Cube
    /// </summary>
    private void createAllValideMaterialCude()
    {
        Debug.Log("createAllValideMaterialCude()");
        SVCCubeParentGo = new GameObject("SVCCubeParentGo");
        SVCCubeParentGo.transform.position = Vector3.zero;
        var posoffset = new Vector3(0.05f, 0f, 0f);
        var cubeworldpos = Vector3.zero;
        var svccubetemplate = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Res/prefabs/pre_SVCCube.prefab");
        var allmatassets = getAllValideMaterial();
        for (int i = 0, length = allmatassets.Count; i < length; i++)
        {
            var cube = GameObject.Instantiate<GameObject>(svccubetemplate);
            cube.name = allmatassets[i].name + "Cube";
            cube.transform.position = posoffset * i;
            cube.GetComponent<MeshRenderer>().material = allmatassets[i];
            cube.transform.SetParent(SVCCubeParentGo.transform);
        }
        EditorSceneManager.SaveOpenScenes();
        //mCompleteDynamicCubeInstantiate = true;
        //延时等待一会，确保变体数据更新
        Debug.Log("创建完Cube，开始等待两秒!");
        //await Task.Delay(2000);
        Debug.Log("创建完Cube，等待两秒完成!");
    }

    /// <summary>
    /// 执行变体搜集
    /// </summary>
    private void doShaderVariantsCollect()
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
        //await Task.Delay(1000);
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

    #region Async Await
    /// <summary>
    /// 测试关键字Async Await的异步用法
    /// </summary>
    /// <returns></returns>
    private static async Task TestAsync1()
    {
        Debug.Log("TestAsync1()");
        var result = await TestAsync2();
        Debug.Log("Get result from TestAsync2()");
    }

    /// <summary>
    /// 测试关键字Async Await的异步用法
    /// </summary>
    /// <returns></returns>
    private static async Task<bool> TestAsync2()
    {
        Debug.Log("TestAsync2()");
        var result = await Task.Run<bool>(() => { return true; });
        Debug.Log("result1 = " + result);
        result = await Task.Run<bool>(() => { return false; });
        Debug.Log("result2 = " + result);
        result = await Task.Run<bool>(() => { return true; });
        Debug.Log("result3 = " + result);
        return result;
    }
    #endregion
}