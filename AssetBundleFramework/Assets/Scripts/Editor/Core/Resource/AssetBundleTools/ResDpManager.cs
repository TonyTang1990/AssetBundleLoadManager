/*
 * Description:             资源依赖信息管理者
 * Author:                  tanghuan
 * Create Date:             2018/03/12
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

/// <summary>
/// 资源依赖信息管理者
/// Note:
/// 仅限编辑器模式下使用
/// </summary>
public class ResDpManager : SingletonTemplate<ResDpManager> {

    /// <summary>
    /// AB依赖信息映射map
    /// Key为AB名字，Value为该AB依赖的AB信息
    /// </summary>
    public Dictionary<string, string[]> AssetBundleDpMap
    {
        get
        {
            return mAssetBundleDpMap;
        }
        private set
        {
            mAssetBundleDpMap = value;
        }
    }
    private Dictionary<string, string[]> mAssetBundleDpMap = new Dictionary<string, string[]>();

    /// <summary>
    /// 加载所有AB依赖信息
    /// </summary>
    /// <returns></returns>
    public void loadAllDpInfo()
    {
        var dpfiles = Directory.GetFiles(AssetBundlePath.ABBuildinPath, "*.d");
        var dplist = new List<string>();
        foreach (var dpfile in dpfiles)
        {
            dplist.Clear();
            var abname = Path.GetFileNameWithoutExtension(dpfile);
            using (StreamReader sr = new StreamReader(dpfile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    dplist.Add(line);
                }
                mAssetBundleDpMap.Add(abname, dplist.ToArray());
            }
        }
        return;
    }

    /// <summary>
    /// 获取AssetBundle锁依赖的AB信息
    /// </summary>
    /// <param name="abname"></param>
    /// <returns></returns>
    public string[] getAssetBundleDpInfo(string abname)
    {
        if(mAssetBundleDpMap.Count == 0)
        {
            loadAllDpInfo();
        }

        if (mAssetBundleDpMap.ContainsKey(abname))
        {
            return mAssetBundleDpMap[abname];
        }
        else
        {
            Debug.LogError(string.Format("找不到AB名字为:{0}的ab依赖信息!", abname));
            return null;
        }
    }

    /// <summary>
    /// 打印所有AB依赖信息
    /// </summary>
    public void printAllABDpInfo()
    {
        foreach(var abinfo in mAssetBundleDpMap)
        {
            Debug.Log(string.Format("AB Name:{0}", abinfo.Key));
            foreach(var dpfile in abinfo.Value)
            {
                Debug.Log(string.Format("       DP AB Name:{0}", dpfile));
            }
        }
    }
}