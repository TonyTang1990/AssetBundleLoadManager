/*
 * Description:             AssetInfoBuilder.cs
 * Author:                  TONYTANG
 * Create Date:             2026//07/13
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetInfoBuilder.cs
    /// Asset信息构建器类,用于构建Asset的名字(含后缀)和Asset路径信息
    /// </summary>
    public class EditorAssetInfoBuilder
    {
        /// <summary>
		/// 所有EditorAssetInfoMap<Asset名(含后缀), Asset信息>(避免相同Asset重复New EditorAssetInfo)
        /// Note:
        /// 1. 仅包含参与打包的Asset打包信息
		/// </summary>
		private Dictionary<string, EditorAssetInfo> mAllEditorAssetInfoMap = new Dictionary<string, EditorAssetInfo>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public EditorAssetInfoBuilder()
        {
        }

        /// <summary>
        /// 执行搜集Editor Asset信息
        /// </summary>
        public bool DoCollectEditorAssetInfo()
        {
            MakeSureEditorAssetInfoFolderExist();
            var result = StartAnalyseEditorAssetInfo();
            if(!result)
            {
                Debug.LogError("搜集EditorAssetInfoAsset信息失败，请检查配置!");
            }
            return result;
        }

        /// <summary>
        /// 确保EditorAssetInfo Asset所在目录存在
        /// </summary>
        private void MakeSureEditorAssetInfoFolderExist()
        {
            var editorAssetInfoAssetFolderFulPath = EditorAssetInfoPath.GetEditorAssetInfoFolderFullPath();
            FolderUtilities.CheckAndCreateSpecificFolder(editorAssetInfoAssetFolderFulPath);
        }

        /// <summary>
        /// 开始分析Editor Asset信息
        /// </summary>
        private bool StartAnalyseEditorAssetInfo()
        {
            mAllEditorAssetInfoMap.Clear();
            // 获取所有的收集路径
            List<string> collectDirectorys = AssetBundleCollectSettingData.GetAllCollectDirectory();
            int progressBarCount = 0;
            // 获取所有资源
            string[] guids = AssetDatabase.FindAssets(string.Empty, collectDirectorys.ToArray());
            foreach (string guid in guids)
            {
                // 进度条
                progressBarCount++;
                string mainAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                string regularMainAssetPath = PathUtilities.GetRegularPath(mainAssetPath);
                EditorUtility.DisplayProgressBar("进度", $"打包文件分析：{progressBarCount}/{guids.Length}", (float)progressBarCount / guids.Length);
                if (!IsValidateCollectAsset(regularMainAssetPath))
                {
                    continue;
                }
                var collector = AssetBundleCollectSettingData.GetCollectorByAssetPath(regularMainAssetPath);
                if(collector == null)
                {
                    Debug.LogError($"未找到Asset路径:{regularMainAssetPath}对应的收集器信息，请检查配置!");
                    EditorUtility.ClearProgressBar();
                    return false;
                }
                if(!collector.AllowLoadFromScript)
                {
                    continue;
                }
                var regularMainAssetName = Path.GetFileName(regularMainAssetPath);
                var editorAssetInfo = GetEditorAssetInfo(regularMainAssetName);
                if(editorAssetInfo != null)
                {
                    Debug.LogError($"EditorAssetInfo信息里有同名Asset:{regularMainAssetName}，Asset1:{editorAssetInfo.AssetPath}和Asset2:{regularMainAssetPath}，请修复同名文件后重新生成!");
                    EditorUtility.ClearProgressBar();
                    return false;
                }
                editorAssetInfo = new EditorAssetInfo(regularMainAssetPath);
                mAllEditorAssetInfoMap.Add(editorAssetInfo.AssetName, editorAssetInfo);
            }
            EditorUtility.ClearProgressBar();

			var result = UpdateEditorAssetInfoDatas();
			if(!result)
			{
				Debug.LogError("更新AssetBundle打包数据失败");
				return false;
			}

			Debug.Log($"构建列表里总共有{mAllEditorAssetInfoMap.Count}个Asset信息需要搜集，更新AssetBundle打包数据成功！");
            return true;
        }

		/// <summary>
		/// 获取指定Asset名的EditorAssetInfo信息
		/// </summary>
		/// <param name="assetName">Asset名(含后缀)</param>
		/// <returns></returns>
		private EditorAssetInfo GetEditorAssetInfo(string assetName)
		{
			EditorAssetInfo editorAssetInfo;
			if (!mAllEditorAssetInfoMap.TryGetValue(assetName, out editorAssetInfo))
			{
				return null;
			}
			return editorAssetInfo;
		}

		/// <summary>
		/// 更新EditorAssetInfo数据
		/// </summary>
		private bool UpdateEditorAssetInfoDatas()
        {
			var editorAssetInfoAssetRelativePath = EditorAssetInfoPath.GetEditorAssetInfoAssetFileRelativePath();
			var editorAssetInfoAsset = AssetDatabase.LoadAssetAtPath<EditorAssetInfoAsset>(editorAssetInfoAssetRelativePath);
			if (editorAssetInfoAsset == null)
			{
				editorAssetInfoAsset = ScriptableObject.CreateInstance<EditorAssetInfoAsset>();
				AssetDatabase.CreateAsset(editorAssetInfoAsset, editorAssetInfoAssetRelativePath);
			}
            editorAssetInfoAsset.ClearAllDatas();
            // 更新EditorAssetInfo信息Asset
            if(mAllEditorAssetInfoMap == null || mAllEditorAssetInfoMap.Count == 0)
            {
                return true;
            }
            foreach(var editorAssetInfo in mAllEditorAssetInfoMap)
            {
                editorAssetInfoAsset.AddEditorAssetInfo(editorAssetInfo.Value);
            }
			return true;
        }

        /// <summary>
        /// 指定Asset路径是否是有效可搜集资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        private bool IsValidateCollectAsset(string assetPath)
        {
            if(!IsValidateAsset(assetPath))
            {
                return false;
            }
            if (!AssetBundleCollectSettingData.IsCollectAsset(assetPath))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检测资源是否有效
        /// </summary>
        private bool IsValidateAsset(string assetPath)
        {
            if (!assetPath.StartsWith("Assets/"))
            {
                return false;
            }
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return false;
            }
            string ext = System.IO.Path.GetExtension(assetPath);
            if (AssetBundleCollectSettingData.Setting.BlackListInfo.IsBlackPostFix(ext))
            {
                return false;
            }
            string fileName = Path.GetFileName(assetPath);
            if (AssetBundleCollectSettingData.Setting.BlackListInfo.IsBlackFileName(fileName))
            {
                return false;
            }
            return true;
        }
    }
}