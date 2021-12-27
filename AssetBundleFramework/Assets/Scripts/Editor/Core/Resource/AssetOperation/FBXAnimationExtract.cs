/*
 * Description:             FBX动画提取工具
 * Author:                  tanghuan
 * Create Date:             2018/03/16
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// FBX动画提取工具
/// </summary>
public class FBXAnimationExtract {

    [MenuItem("Tools/Assets/FBX/提取FBX动画(且自动创建带AnimationClip的预制件对象) %&E", false, 100)]
    public static void extractAnimations()
    {
        var objs = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        if (objs == null || objs.Length == 0)
        {
            Debug.LogError("没有有效的选中Asset!");
            return;
        }

        foreach (var obj in objs)
        {
            var objpath = AssetDatabase.GetAssetPath(obj);
            if (objpath.ToLower().EndsWith(".fbx"))
            {
                var animationcliplist = extractClipsFromFBX(objpath);
                changeFBXImportSetting(objpath);
                createFBXPrefabWithAnimations(objpath, animationcliplist);
            }
            else
            {
                continue;
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 从FBX里提取动画(默认提取到同层目录)
    /// </summary>
    /// <param name="objpath"></param>
    /// <returns>拆分后的动画列表</returns>
    private static List<AnimationClip> extractClipsFromFBX(string objpath)
    {
        List<AnimationClip> animationcliplist = new List<AnimationClip>();
        if (objpath.ToLower().EndsWith(".fbx"))
        {
            var fbxfilename = Path.GetFileNameWithoutExtension(objpath);
            var fbxobjs = AssetDatabase.LoadAllAssetsAtPath(objpath);
            var fbxfolderpath = Path.GetDirectoryName(objpath);
            foreach (var fbxobj in fbxobjs)
            {
                if (fbxobj is AnimationClip && !fbxobj.name.Contains("Take 001"))
                {
                    AnimationClip temp = new AnimationClip();
                    EditorUtility.CopySerialized(fbxobj, temp);
                    var animationfolderpath = fbxfolderpath + "/" + fbxfilename + "Animations/";
                    var animationassetname = fbxobj.name + ".anim";
                    var animationassetfullpath = animationfolderpath + animationassetname;
                    FolderUtilities.CheckAndCreateSpecificFolder(animationfolderpath);
                    AssetDatabase.CreateAsset(temp, animationassetfullpath);
                    animationcliplist.Add(temp);
                }
            }
        }
        return animationcliplist;
    }

    /// <summary>
    /// 修改FBX部分导入设置
    /// 1. 移除FBX导入动画设置(一旦拆分动画后不再需要使用原Clip)
    /// 2. 修改Material Search为Project Wide去全局搜索原始material
    /// </summary>
    /// <param name="objpath"></param>
    private static void changeFBXImportSetting(string objpath)
    {
        if (objpath.ToLower().EndsWith(".fbx"))
        {
            var modelassetimporter = AssetImporter.GetAtPath(objpath) as ModelImporter;
            if(modelassetimporter.importAnimation == true)
            {
                modelassetimporter.importAnimation = false;
                modelassetimporter.SaveAndReimport();
            }
        }
        else
        {
            return;
        }
    }

    /// <summary>
    /// 创建FBX对应预制件对象并添加所有拆分的动画
    /// </summary>
    /// <param name="objpath"></param>
    /// <param name="animationcliplist">需要添加的动画clip列表</param>
    private static void createFBXPrefabWithAnimations(string objpath, List<AnimationClip> animationcliplist)
    {
        if (objpath.ToLower().EndsWith(".fbx"))
        {
            var fbxobj = AssetDatabase.LoadAssetAtPath<GameObject>(objpath);
            var fbxfolderpath = Path.GetDirectoryName(objpath);
            var fbxfilename = Path.GetFileNameWithoutExtension(objpath);
            var prefabpath = fbxfolderpath + "/pre_" + fbxfilename + ".prefab";
            var fbxprefab = PrefabUtility.CreatePrefab(prefabpath, fbxobj, ReplacePrefabOptions.ConnectToPrefab) ;
            if(animationcliplist.Count > 0)
            {
                var animationscomponent = fbxprefab.GetComponent<Animation>();
                foreach (var animationclip in animationcliplist)
                {
                    animationscomponent.AddClip(animationclip, animationclip.name);
                }
                animationscomponent.clip = animationcliplist[0];
            }
        }
    }
}
