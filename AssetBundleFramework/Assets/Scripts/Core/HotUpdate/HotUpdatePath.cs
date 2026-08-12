/*
 * Description:             HotUpdatePath.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/11
 */

using UnityEditor;

namespace TResource
{
    /// <summary>
    /// HotUpdatePath.cs
    /// 热更新路径类
    /// </summary>
    public static class HotUpdatePath
    {
    #if UNITY_EDITOR
        /// <summary>
        /// 获取当前激活平台的热更新输出目录
        /// </summary>
        /// <param name="buildTareget"></param>
        /// <returns></returns>
        public static string GetLocalHotUpdateFolderPath()
        {
            return GetLocalHotUpdateFolderPath(EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>
        /// 获取本地指定平台的热更新输出目录
        /// </summary>
        /// <param name="buildTareget"></param>
        /// <returns></returns>
        public static string GetLocalHotUpdateFolderPath(BuildTarget buildTareget)
        {
            var projectFolderPath = PathUtilities.GetProjectFullPath();
            var localHotUpdateFolderPath = $"{projectFolderPath}../HotUpdate/Preparation/{buildTareget.ToString()}";
            return PathUtilities.GetRegularPath(localHotUpdateFolderPath);
        }
    #endif
    }
}