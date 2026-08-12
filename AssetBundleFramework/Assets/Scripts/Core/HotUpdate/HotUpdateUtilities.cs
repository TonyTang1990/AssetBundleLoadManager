/*
 * Description:             HotUpdateUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/12
 */

using System.IO;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// HotUpdateUtilities.cs
    /// 热更新静态工具类
    /// </summary>
    public static class HotUpdateUtilities
    {
        /// <summary>
        /// 删除所有包外热更新资源目录
        /// </summary>
        public static void DeleteAllOutterHotUpdateResources()
        {
            // 含热更资源，临时资源，包外资源版本文件等所有资源
            if(Directory.Exists(ResourcePath.ABHotUpdatePath))
            {
                Debug.Log(string.Format("删除包外热更新资源目录:{0}!", ResourcePath.ABHotUpdatePath));
                Directory.Delete(ResourcePath.ABHotUpdatePath, true);
            }
            // 热更配置目录不一样，单独删除
            var outterConfigFolderPath = ResourcePath.GetOutterVersionConfigFolderPath();
            if(Directory.Exists(outterConfigFolderPath))
            {
                Debug.Log(string.Format("删除包外热更新配置目录:{0}!", outterConfigFolderPath));
                Directory.Delete(outterConfigFolderPath, true);
            }
            Debug.Log($"删除所有包外热更新资源目录完成!");
        }
    }
}