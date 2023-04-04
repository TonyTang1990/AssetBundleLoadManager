/*
 * Description:             AssetBundle打包辅助工具
 * Author:                  TonyTang
 * Create Date:             2023/01/23
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace TResource
{
	public static class AssetBundleBuilderHelper
	{
		/// <summary>
		/// 获取默认的导出根路径
		/// </summary>
		public static string GetOutputRootPath()
		{
			string projectPath = PathUtilities.GetProjectFullPath();
			return $"{projectPath}Assets/StreamingAssets";
		}

        /// <summary>
        /// 当前激活平台的获取默认的导出根路径
        /// </summary>
        public static string GetActiveBuildTargetOutputRootPath()
        {
			var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
			var activeBuildTargetDefaultOutputRootPath = GetBuildTargetOutputRootPath(activeBuildTarget);
			return activeBuildTargetDefaultOutputRootPath;
        }

        /// <summary>
        /// 当前激活平台的获取默认的导出根路径
        /// </summary>
		/// <param name="buildTarget">平台</param>
        public static string GetBuildTargetOutputRootPath(BuildTarget buildTarget)
        {
            return $"{GetOutputRootPath()}/{buildTarget}/";
		}

        /// <summary>
        /// 清空流文件夹
        /// </summary>
        public static void ClearStreamingAssetsFolder()
		{
			string streamingPath = Application.dataPath + "/StreamingAssets";
			FolderUtilities.ClearFolder(streamingPath);
		}

		/// <summary>
		/// 删除流文件夹内无关的文件
		/// 删除.manifest文件和.meta文件
		/// </summary>
		public static void DeleteStreamingAssetsIgnoreFiles()
		{
			string streamingPath = Application.dataPath + "/StreamingAssets";
			if (Directory.Exists(streamingPath))
			{
				string[] files = Directory.GetFiles(streamingPath, "*.manifest", SearchOption.AllDirectories);
				foreach (var file in files)
				{
					FileInfo info = new FileInfo(file);
					info.Delete();
				}

				files = Directory.GetFiles(streamingPath, "*.meta", SearchOption.AllDirectories);
				foreach (var item in files)
				{
					FileInfo info = new FileInfo(item);
					info.Delete();
				}
			}
		}
	}
}