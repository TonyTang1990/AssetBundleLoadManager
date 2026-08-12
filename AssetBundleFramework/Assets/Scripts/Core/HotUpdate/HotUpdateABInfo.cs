/*
 * Description:             HotUpdateABInfo.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/09
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// HotUpdateABInfo.cs
    /// 热更新AB信息类(对应AssetBundleInfo.json的数据结构)
    /// </summary>
    [Serializable]
    public class HotUpdateABInfo
    {
        /// <summary>
        /// 热更新AB信息列表
        /// </summary>
        public List<HotUpdateSingleABInfo> HotUpdateSingleABInfoList;

        /// <summary>
        /// 热更新AB信息字典(Key为AB相对路径名(含后缀),Value为HotUpdateSingleABInfo)
        /// </summary>
        private Dictionary<string, HotUpdateSingleABInfo> mHotUpdateSingleABInfoMap;

        public HotUpdateABInfo()
        {
            HotUpdateSingleABInfoList = new List<HotUpdateSingleABInfo>();
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            mHotUpdateSingleABInfoMap = new Dictionary<string, HotUpdateSingleABInfo>();
            foreach (var hotUpdateSingleABInfo in HotUpdateSingleABInfoList)
            {
                if (hotUpdateSingleABInfo == null)
                {
                    Debug.LogError($"热更新AB信息列表中存在null的HotUpdateSingleABInfo!");
                    continue;
                }

                if (mHotUpdateSingleABInfoMap.ContainsKey(hotUpdateSingleABInfo.ABRelativePath))
                {
                    Debug.LogError($"热更新AB信息列表中存在重复的AB相对路径名:{hotUpdateSingleABInfo.ABRelativePath}!");
                    continue;
                }

                mHotUpdateSingleABInfoMap.Add(hotUpdateSingleABInfo.ABRelativePath, hotUpdateSingleABInfo);
            }
        }

        /// <summary>
        /// 添加热更新单个AB信息
        /// </summary>
        /// <param name="hotUpdateSingleABInfo"></param>
        /// <returns></returns>
        public bool AddHotUpdateSingleABInfo(HotUpdateSingleABInfo hotUpdateSingleABInfo)
        {
            if (hotUpdateSingleABInfo == null)
            {
                Debug.LogError($"添加热更新单个AB信息失败,hotUpdateSingleABInfo为null!");
                return false;
            }

            HotUpdateSingleABInfoList.Add(hotUpdateSingleABInfo);
            return true;
        }

        /// <summary>
        /// 移除热更新单个AB信息
        /// </summary>
        /// <param name="hotUpdateSingleABInfo"></param>
        /// <returns></returns>
        public bool RemoveHotUpdateSingleABInfo(HotUpdateSingleABInfo hotUpdateSingleABInfo)
        {
            if (hotUpdateSingleABInfo == null)
            {
                Debug.LogError($"移除热更新单个AB信息失败,hotUpdateSingleABInfo为null!");
                return false;
            }

            HotUpdateSingleABInfoList.Remove(hotUpdateSingleABInfo);
            return true;
        }

        /// <summary>
        /// 获取指定ab相对路径的热更新单个AB信息
        /// </summary>
        /// <param name="abRelativePath"></param>
        /// <returns></returns>
        public HotUpdateSingleABInfo GetABSingleABInfo(string abRelativePath)
        {
            if (mHotUpdateSingleABInfoMap == null)
            {
                Debug.LogError($"热更新AB信息字典未初始化,请先调用Init()方法，获取：{abRelativePath}的SingleABInfo失败!");
                return null;
            }
            if (mHotUpdateSingleABInfoMap.TryGetValue(abRelativePath, out var hotUpdateSingleABInfo))
            {
                return hotUpdateSingleABInfo;
            }
            Debug.LogError($"热更新AB信息字典中不存在AB相对路径名:{abRelativePath}的热更新单个AB信息!");
            return hotUpdateSingleABInfo;
        }

        /// <summary>
        /// 获取指定ab相对路径的真实ab相对路径(带MD5)
        /// </summary>
        /// <param name="abRelativePath"></param>
        /// <returns></returns>
        public string GetABRealRelativePath(string abRelativePath)
        {
            var singleABInfo = GetABSingleABInfo(abRelativePath);
            if (singleABInfo == null)
            {
                Debug.LogError($"热更新AB信息字典中不存在AB相对路径名:{abRelativePath}的热更新单个AB信息，获取AB真实相对路径失败!");
                return string.Empty;
            }
            return singleABInfo.GetABRelativePathWithMD5();
        }
    }
}