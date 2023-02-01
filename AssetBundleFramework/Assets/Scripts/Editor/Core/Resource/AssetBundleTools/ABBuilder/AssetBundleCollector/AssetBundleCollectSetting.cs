/*
 * Description:             AssetBundleCollectSetting.cs
 * Author:                  TonyTang
 * Create Date:             2020//10/25
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace TResource
{
    /// <summary>
    /// 单个搜集打包设定
    /// </summary>
    [Serializable]
    public class Collector
    {
        /// <summary>
        /// 搜集设定相对目录路径
        /// </summary>
        public string CollectFolderPath;

        /// <summary>
        /// 收集规则
        /// </summary>
        public AssetBundleCollectRule CollectRule = AssetBundleCollectRule.Collect;

        /// <summary>
        /// 搜集打包规则
        /// </summary>
        public AssetBundleBuildRule BuildRule;

        /// <summary>
        /// 固定名字(仅当收集打包规则为LoadByConstName时有效)
        /// </summary>
        public string ConstName;

        public Collector()
        {

        }

        public Collector(string collectrelativefolderpath, AssetBundleCollectRule collectrule = AssetBundleCollectRule.Collect, AssetBundleBuildRule buildrule = AssetBundleBuildRule.ByFilePath)
        {
            CollectFolderPath = collectrelativefolderpath;
            CollectRule = collectrule;
            BuildRule = buildrule;
            ConstName = string.Empty;
        }
    }

    /// <summary>
    /// 黑名单信息
    /// </summary>
    [Serializable]
    public class BlackListInfo
    {
        /// <summary>
        /// 后缀名黑名单列表
        /// </summary>
        public List<string> PostFixBlackList;

        /// <summary>
        /// 文件名黑名单列表
        /// </summary>
        public List<string> FileNameBlackList;

        /// <summary>
        /// 后缀名黑名单Map<后缀名, 后缀名>
        /// </summary>
        public Dictionary<string, string> PostFixBlackMap
        {
            get;
            private set;
        }

        /// <summary>
        /// 文件名黑名单Map<文件名, 文件名>
        /// </summary>
        public Dictionary<string, string> FileNameBlackMap
        {
            get;
            private set;
        }

        public BlackListInfo()
        {
            PostFixBlackList = new List<string>();
            FileNameBlackList = new List<string>();
            PostFixBlackMap = new Dictionary<string, string>();
            FileNameBlackMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        public void UpdateData()
        {
            PostFixBlackMap.Clear();
            foreach (var postFixBlack in PostFixBlackList)
            {
                if (PostFixBlackMap.ContainsKey(postFixBlack))
                {
                    Debug.LogError($"有重复的后缀名:{postFixBlack}黑名单配置!");
                    continue;
                }
                PostFixBlackMap.Add(postFixBlack, postFixBlack);
            }
            FileNameBlackMap.Clear();
            foreach (var fileNameBlack in FileNameBlackList)
            {
                if (FileNameBlackMap.ContainsKey(fileNameBlack))
                {
                    Debug.LogError($"有重复的文件名:{fileNameBlack}黑名单配置!");
                    continue;
                }
                FileNameBlackMap.Add(fileNameBlack, fileNameBlack);
            }
        }

        /// <summary>
        /// 指定后缀名是否在黑名单里
        /// </summary>
        /// <param name="postFix"></param>
        /// <returns></returns>
        public bool IsBlackPostFix(string postFix)
        {
            return PostFixBlackMap.ContainsKey(postFix);
        }

        /// <summary>
        /// 指定文件名是否在黑名单里
        /// </summary>
        /// <param name="postFix"></param>
        /// <returns></returns>
        public bool IsBlackFileName(string postFix)
        {
            return FileNameBlackMap.ContainsKey(postFix);
        }
    }

    /// <summary>
    /// AssetBundleCollectSetting.cs
    /// AB打包搜集规则数据序列化类
    /// </summary>
    public class AssetBundleCollectSetting : ScriptableObject
    {
        /// <summary>
        /// 所有的AB搜集信息
        /// </summary>
        public List<Collector> AssetBundleCollectors;

        /// <summary>
        /// 黑名单信息
        /// </summary>
        public BlackListInfo BlackListInfo;

        AssetBundleCollectSetting()
        {
            AssetBundleCollectors = new List<Collector>();
            BlackListInfo = new BlackListInfo();
        }

        /// <summary>
        /// 更新树
        /// </summary>
        public void UpdateData()
        {
            BlackListInfo.UpdateData();
        }

        /// <summary>
        /// 触发搜集器排序
        /// </summary>
        public void SortCollector()
        {
            AssetBundleCollectors.Sort(SortAssetBundleCollector);
        }

        /// <summary>
        /// 收集器排序
        /// </summary>
        /// <param name="collector1"></param>
        /// <param name="colloect2"></param>
        /// <returns></returns>
        private int SortAssetBundleCollector(Collector collector1, Collector colloect2)
        {
            return collector1.CollectFolderPath.CompareTo(colloect2.CollectFolderPath);
        }
    }
}