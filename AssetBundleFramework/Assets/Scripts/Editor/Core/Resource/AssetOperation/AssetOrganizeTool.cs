/*
 * File Name:               AssetOrganizeTool.cs
 *
 * Description:             资源整理工具(用于整理从Unity 4.7导出的资源)
 * Author:                  tanghuan <435853363@qq.com>
 * Create Date:             2018/02/24
 */

using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

/// <summary>
/// 资源整理工具(用于整理从Unity 4.7导出的资源)
/// 整理规则:
/// 1. 以当前选中需要整理的资源路径作为基础目录
/// 2. 删除FBX导入自动创建的Material资源(原始资源已经被改名，自动导入得是重复资源)
/// 3. 同一个预制件引用的资源放到同一个目录
/// 4. 所有Shader放同一个目录
/// 5. 没有预制件只有FBX时，FBX以及引用的资源放同一个目录(后期为了重用需要做成prefab)
/// 6. 其他找不到引用关系的资源单独放一个目录(Other)
/// Note：
/// 1. 光照图升级Untiy后需要重新打，没有被拷贝过来(光照图和场景预制件AB打一起)
/// 2. 导航寻路暂时不确定是否需要重新Bake，暂时没有拷贝过来(寻路和场景预制件AB打一起)
/// </summary>
public class AssetOrganizeTool {

#if AB_PACKAGE_SYSTEM
    [MenuItem("Tools/Assets/整理选中Asset及其依赖资源", false, 101)]
#endif
    public static void assetOrganize()
    {
        var selections = Selection.GetFiltered(typeof(object), SelectionMode.Assets);

        if (selections == null)
        {
            Debug.LogError("未选中任何有效对象！");
            return;
        }
        else if (selections.Length > 1)
        {
            Debug.LogError("暂时只允许选中一个有效对象！");
            return;
        }


        var asset = selections[0];
        Debug.Log("asset.name = " + asset.name);

        var assetpath = AssetDatabase.GetAssetPath(asset);
        var assetfolderpath = Path.GetDirectoryName(assetpath);
        var assetfilenamenoex = Path.GetFileNameWithoutExtension(assetpath);
        var baseassetfolder = assetfolderpath + "/" + assetfilenamenoex;
        var shaderpath = assetfolderpath + "/" + "Shaders";
        Debug.Log(string.Format("shaderpath : {0}", shaderpath));
        organizeAsset(asset, baseassetfolder, shaderpath);

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 整理Asset以及依赖资源
    /// </summary>
    /// <param name="asset">需要整理的Asset</param>
    /// <param name="basefolderpath">当前整理Asset整理后的基准目录(Asset同层同名文件夹)</param>
    /// <param name="shaderfolderpath">Shader所放目录</param>
    private static void organizeAsset(Object asset, string basefolderpath, string shaderfolderpath)
    {
        if(asset == null)
        {
            return;
        }

        var assetpath = AssetDatabase.GetAssetPath(asset);
        //Debug.Log(string.Format("organizeAsset() assetpath : {0}", assetpath));

        if (asset is Shader)
        {
            // Shader统一放到一个目录不进行细分
            AssetOrganizeHelper.moveFileAndMeta(assetpath, shaderfolderpath);
            return;
        }
        
        var assetprefabtype = PrefabUtility.GetPrefabType(asset);
        Debug.Log(string.Format("{0} prefabtype: {1}", asset, assetprefabtype.ToString()));
        //修改FBX的导入设置，避免每次都因为原材质改名生成新的Material
        //同时将FBX和上层引用他的资源放在一起，不再追溯其依赖资源，并删除其导入生成的依赖Material
        if(assetprefabtype == PrefabType.ModelPrefab)
        {
            deleteDpMaterialAsset(assetpath);
            var modelassetomporter = ModelImporter.GetAtPath(assetpath) as ModelImporter;
            modelassetomporter.importMaterials = false;
            modelassetomporter.SaveAndReimport();
            AssetOrganizeHelper.moveFileAndMeta(assetpath, basefolderpath);
            return;
        }

        var dpassets = AssetDatabase.GetDependencies(assetpath, false);
        Debug.Log(string.Format("assetpath = {0}", assetpath));
        Debug.Log(string.Format("dpassets.Length = {0}", dpassets.Length));

        //在遍历依赖之前先复制源文件，确保递归判定时文件复制优先级
        AssetOrganizeHelper.moveFileAndMeta(assetpath, basefolderpath);

        //优先预制件及其依赖资源，确保预制件和其依赖资源放在同层目录
        var nonprefablist = new List<string>();

        for (int i = 0; i < dpassets.Length; i++)
        {
            Debug.Log(string.Format("direct dp asset[{0}] : {1}", i, dpassets[i]));
            var dpasset = AssetDatabase.LoadAssetAtPath(dpassets[i], typeof(object));
            var prefabtype = PrefabUtility.GetPrefabType(dpasset);
            Debug.Log(string.Format("{0} prefabtype: {1}", dpassets[i], prefabtype.ToString()));
            if(prefabtype != PrefabType.Prefab)
            {
                nonprefablist.Add(dpassets[i]);
                continue;
            }

            var dpassetpath = AssetDatabase.GetAssetPath(dpasset);
            var dpassetfilenamenoex = Path.GetFileNameWithoutExtension(dpassetpath);
            var dpbaseassetfolder = basefolderpath + "/" + dpassetfilenamenoex;
            organizeAsset(dpasset, dpbaseassetfolder, shaderfolderpath);
        }

        for (int i = 0; i < nonprefablist.Count; i++)
        {
            Debug.Log(string.Format("direct dp asset[{0}] : {1}", i, nonprefablist[i]));
            var nonprefabdpasset = AssetDatabase.LoadAssetAtPath(nonprefablist[i], typeof(object));            
            var dpassetpath = AssetDatabase.GetAssetPath(nonprefabdpasset);
            var dpassetfilenamenoex = Path.GetFileNameWithoutExtension(dpassetpath);
            var dpbaseassetfolder = basefolderpath + "/" + dpassetfilenamenoex;
            organizeAsset(nonprefabdpasset, dpbaseassetfolder, shaderfolderpath);
        }
    }

    /// <summary>
    /// 删除引用材质资源
    /// </summary>
    /// <param name="assetpath"></param>
    private static void deleteDpMaterialAsset(string assetpath)
    {
        var dpassets = AssetDatabase.GetDependencies(assetpath, false);
        for (int i = 0; i < dpassets.Length; i++)
        {
            var dpmatasset = AssetDatabase.LoadAssetAtPath(dpassets[i], typeof(Material));
            if (dpmatasset != null)
            {
                AssetOrganizeHelper.deleteFileAndMeta(dpassets[i]);
            }
        }
    }
}
