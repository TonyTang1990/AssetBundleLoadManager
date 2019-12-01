/*
 * Description:             选中Asset操作窗口(用于批量做一些事 e.g. 批量制作预制件)
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 选中Asset操作窗口(用于批量做一些事 e.g. 批量制作预制件)
/// </summary>
public class AssetSelectionsOperationWindow : EditorWindow
{
    /// <summary>
    /// 选中对象列表
    /// </summary>
    private List<GameObject> sltGoList;

    private Vector2 uiScrollPos;

    /// <summary>
    /// 选中对象是否可操作信息列表
    /// True代表操作
    /// </summary>
    private List<bool> sltGoOperationList;
    
    /// <summary>
    /// 可操作的预制件Asset
    /// </summary>
    private Object sltOperatedPrefab;

    /// <summary>
    /// 选中的可操作Asset
    /// </summary>
    private List<Object> sltOperatedAsset;

    /// <summary>
    /// Prefab存储路径，默认以当前打开的场景所在路径为准
    /// </summary>
    private string prefabSavePath;

    private const string PrefabPosfix = ".prefab";

    #if AB_PACKAGE_SYSTEM
    [MenuItem("Tools/Assets/操作选中对象资源窗口", false, 102)]
    #endif
    public static void selectionAssetOperationWindow()
    {
        var sltassetoperationwindow = EditorWindow.GetWindow<AssetSelectionsOperationWindow>();
        sltassetoperationwindow.Show();
    }

    void Awake()
    {
        refreshSelectedData();
    }

    void OnEnable()
    {
        refreshSelectedData();
    }

    private void refreshSelectedData()
    {
        refreshSelectedGoes();
        refreshSelectedPrefabAssets();
    }

    private void refreshSelectedGoes()
    {
        sltGoList = new List<GameObject>();
        sltGoOperationList = new List<bool>();
        var activescene = EditorSceneManager.GetActiveScene();
        prefabSavePath = Path.GetDirectoryName(activescene.path);
        var selections = Selection.transforms;
        foreach (var selection in selections)
        {
            sltGoList.Add(selection.gameObject);
            sltGoOperationList.Add(true);
        }
    }

    private void refreshSelectedPrefabAssets()
    {
        sltOperatedPrefab = null;
        sltOperatedAsset = new List<Object>();
        var selectionassets = Selection.GetFiltered<Object>(SelectionMode.Assets);
        foreach (var selectionasset in selectionassets)
        {
            var prefabtype = PrefabUtility.GetPrefabType(selectionasset);
            if(prefabtype == PrefabType.Prefab && sltOperatedPrefab == null)
            {
                //默认只取第一个预制件作为操作对象
                sltOperatedPrefab = selectionasset;
            }
            sltOperatedAsset.Add(selectionasset);
        }
    }

    public void OnGUI()
    {
        GUILayout.BeginVertical();
        showSelectedAssetsUI();
        showSelectedPrefabUI();
        GUILayout.BeginHorizontal();
        GUILayout.Label("操作存储路径：" + prefabSavePath);
        if (GUILayout.Button("刷新选中数据", GUILayout.MaxWidth(200.0f)))
        {
            refreshSelectedData();
        }
        if (GUILayout.Button("将选中对象Aplly到对应预制件", GUILayout.MaxWidth(200.0f)))
        {
            for (int i = 0; i < sltGoList.Count; i++)
            {
                PrefabUtilitiesExtension.Singleton.applyGameObjectToPrefab(sltGoList[i]);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("将选中对象制成预制件", GUILayout.MaxWidth(200.0f)))
        {
            for(int i = 0; i < sltGoList.Count; i++)
            {
                if(sltGoOperationList[i] == true)
                {
                    var prefabsavefullpath = prefabSavePath + "/" + sltGoList[i].name + PrefabPosfix;
                    Debug.Log("prefabSavePath = " + prefabsavefullpath);
                    PrefabUtility.CreatePrefab(prefabsavefullpath, sltGoList[i], ReplacePrefabOptions.ConnectToPrefab);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("实例化选中预制件替换选中对象(保留transform数据)", GUILayout.MaxWidth(300.0f)))
        {
            if(sltOperatedPrefab == null)
            {
                Debug.Log("没有选中有效的预制件Asset");
            }
            else
            {
                for (int i = 0; i < sltGoList.Count; i++)
                {
                    if (sltGoOperationList[i] == true)
                    {
                        replaceGoWithPrefab(sltOperatedPrefab, sltGoList[i]);
                    }
                }
                refreshSelectedData();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示选中对象UI
    /// </summary>
    private void showSelectedAssetsUI()
    {
        GUILayout.BeginVertical();
        uiScrollPos = GUILayout.BeginScrollView(uiScrollPos);
        GUILayout.Label("选中对象列表:");
        for (int i = 0; i < sltGoList.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(string.Format("第{0}:", i+1), GUILayout.Width(30.0f));
            GUILayout.Label(sltGoList[i].name, GUILayout.Width(120.0f));
            sltGoOperationList[i] = GUILayout.Toggle(sltGoOperationList[i],"是否操作", GUILayout.Width(120.0f));
            var prefabtype = PrefabUtility.GetPrefabType(sltGoList[i]);
            GUILayout.Label(string.Format("Object PrefabType : {0}", prefabtype.ToString()), GUILayout.Width(300.0f));
            if (prefabtype == PrefabType.PrefabInstance)
            {
                var prefabparent = PrefabUtility.FindPrefabRoot(sltGoList[i]);
                GUILayout.Label(string.Format("Prefab name : {0}", prefabparent.name), GUILayout.Width(400.0f));
                //已经被预制件包含的不再制作成预制件
                sltGoOperationList[i] = false;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    /// <summary>
    /// 显示选中预制件UI
    /// </summary>
    private void showSelectedPrefabUI()
    {
        if(sltOperatedPrefab != null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("可操作的预制件:", GUILayout.Width(100.0f));
            GUILayout.Label(sltOperatedPrefab.name, GUILayout.Width(120.0f));
            GUILayout.EndHorizontal();
        }
        if(sltOperatedAsset.Count > 0)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Project选中的Asset类型信息:", GUILayout.Width(200.0f));
            GUILayout.EndHorizontal();
            foreach(var asset in sltOperatedAsset)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("asset name: {0}", asset.name), GUILayout.Width(200.0f));
                var assetpath = AssetDatabase.GetAssetPath(asset);
                GUILayout.Label(string.Format("asset path: {0}", assetpath), GUILayout.Width(500.0f));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label(string.Format("asset type: {0}", asset.GetType()), GUILayout.Width(300.0f));
                GUILayout.Label(string.Format("asset importer type: {0}", AssetImporter.GetAtPath(assetpath).GetType()), GUILayout.Width(300.0f));
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }

    /// <summary>
    /// 使用预制件实例化替换GameObject
    /// </summary>
    /// <param name="prefabasset">预制件Asset</param>
    /// <param name="go">需要替换的Gameobjet</param>
    /// <param name="keeptransformvalue">保留原tranform值</param>
    /// <param name="keephierarchy">保留原有层级关系</param>
    private void replaceGoWithPrefab(Object prefabasset, GameObject go, bool keeptransformvalue = true, bool keephierarchy = true)
    {
        var gotransform = go.transform;
        var goname = go.name;
        var goparent = go.transform.parent;
        var siblingindex = go.transform.GetSiblingIndex();
        var newgo = PrefabUtility.InstantiatePrefab(prefabasset) as GameObject;
        newgo.name = goname;
        if (keephierarchy)
        {
            newgo.transform.SetParent(goparent);
            newgo.transform.SetSiblingIndex(siblingindex);
        }
        if (keeptransformvalue)
        {
            newgo.transform.localPosition = gotransform.localPosition;
            newgo.transform.localRotation = gotransform.localRotation;
            newgo.transform.localScale = gotransform.localScale;
        }
        GameObject.DestroyImmediate(go);
        go = null;
    }
}
