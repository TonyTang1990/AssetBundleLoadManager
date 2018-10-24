/*
 * Description:             PackageAsset资源打包依赖层级查看窗口
 * Author:                  tanghuan
 * Create Date:             2018/03/11
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.IO;
using System;

/// <summary>
/// PackageAsset资源打包依赖层级查看窗口
/// </summary>
public class PackagAssetInfoBrowserWindow : EditorWindow
{
    /// <summary>
    /// 需要展示的PackageAsset打包列表
    /// </summary>
    public static List<PackageAsset> mPackageAssetList = new List<PackageAsset>();

    /// <summary>
    /// AB打包列表信息
    /// </summary>
    public static List<AssetBundleBuild> mAssetBundleBuildList = new List<AssetBundleBuild>();

    /// <summary>
    /// UI滚动位置
    /// </summary>
    private Vector2 mUiScrollPos;

    /// <summary>
    /// 当前窗口实例对象
    /// </summary>
    private static PackagAssetInfoBrowserWindow mPackageAssetInfoBrowserWindow;

    /// <summary>
    /// 显示PackageAsset资源打包依赖层级查看窗口
    /// </summary>
    /// <returns></returns>
    public static void showWindow(Dictionary<string, PackageAsset>.ValueCollection palist, List<AssetBundleBuild> abblist)
    {
        Debug.Log("PackagAssetInfoBrowserWindow:showWindow()");
        mPackageAssetList.Clear();
        mPackageAssetList.AddRange(palist);
        mAssetBundleBuildList = abblist;
        mPackageAssetInfoBrowserWindow = (PackagAssetInfoBrowserWindow)EditorWindow.GetWindow(typeof(PackagAssetInfoBrowserWindow));
        mPackageAssetInfoBrowserWindow.Show();
    }

    private void OnDestroy()
    {
        Debug.Log("PackageAssetInfoBrowserWindow::OnDestroy()");
    }

    public void OnGUI()
    {
        if (mPackageAssetList.Count > 0)
        {
            GUILayout.BeginVertical(GUILayout.MaxWidth(position.width), GUILayout.MaxHeight(position.height));
            mUiScrollPos = GUILayout.BeginScrollView(mUiScrollPos);
            foreach (var packageasset in mPackageAssetList)
            {
                showAssetDpUI(packageasset);
            }
            GUILayout.Space(20.0f);
            foreach (var packageasset in mPackageAssetList)
            {
                showAssetABNameUI(packageasset);
            }
            GUILayout.Space(20.0f);
            foreach (var abb in mAssetBundleBuildList)
            {
                showAssetBundleBuildUI(abb);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }
        else
        {
            return;
        }        
    }

    /// <summary>
    /// 显示依赖资源信息UI
    /// </summary>
    /// <param name="assetpath"></param>
    /// <param name="dpassetpath"></param>
    private void showAssetDpUI(PackageAsset pa)
    {
        GUILayout.BeginHorizontal();
        var temppa = pa;
        GUILayout.Label(string.Format("{0}", Path.GetFileName(temppa.AssetPath)), GUILayout.Width(300.0f));
        while (temppa.DependentPackageAsset != null)
        {
            GUILayout.Label(" <- ", GUILayout.Width(30.0f));
            GUILayout.Label(string.Format("{0}", Path.GetFileName(temppa.DependentPackageAsset.AssetPath)), GUILayout.Width(300.0f));
            temppa = temppa.DependentPackageAsset;
        }
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 显示资源的最终AB名字结论
    /// </summary>
    /// <param name="pa"></param>
    private void showAssetABNameUI(PackageAsset pa)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("Asset path : {0}", pa.AssetPath), GUILayout.Width(700.0f));
        GUILayout.Label(string.Format("Asset BuildRule : {0}", pa.AssetAssetBundleBuildRule), GUILayout.Width(200.0f));
        GUILayout.Label(string.Format("ABName : {0}", pa.getPackageAssetABName()), GUILayout.Width(400.0f));
        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// 显示资源的最终AB打包信息
    /// </summary>
    /// <param name="pa"></param>
    private void showAssetBundleBuildUI(AssetBundleBuild abb)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(string.Format("ABName : {0}", abb.assetBundleName), GUILayout.Width(400.0f));
        GUILayout.BeginVertical();
        foreach(var asset  in abb.assetNames)
        {
            GUILayout.Label(string.Format("Asset Path : {0}", asset), GUILayout.Width(700.0f));
        }
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
}
