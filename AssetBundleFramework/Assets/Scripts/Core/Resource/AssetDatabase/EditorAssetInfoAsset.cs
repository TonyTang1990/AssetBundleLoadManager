/*
 * Description:             EditorAssetInfoAsset.cs
 * Author:                  TONYTANG
 * Create Date:             2026//07/13
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// EditorAssetInfoAsset.cs
    /// EditorAssetInfoAsset类,用于存储编辑器Asset信息(Asset名字(含后缀)和Asset路径)
    /// </summary>
    public class EditorAssetInfoAsset : ScriptableObject
    {
        /// <summary>
        /// EditorAssetInfo信息列表
        /// Note:
        /// 仅包含需要支持主动加载的资源Asset信息
        /// </summary>
        [Header("EditorAssetInfo列表")]
        public List<EditorAssetInfo> EditorAssetInfoList;

        /// <summary>
        /// 打包AssetBundle信息信息列表(现有设计采用AB打包出来的Manifest文件访问依赖信息)
        /// </summary>
        //[Header("AB打包信息信息列表")]
        //public List<AssetBundleBuildInfo> AssetBundleBuildInfoList;

        /// <summary>
        /// Asset信息映射Map(Key为Asset路径(含后缀)，Value为对应Asset打包信息)
        /// </summary>
        private Dictionary<string, EditorAssetInfo> mPathAssetInfoMap;

        /// <summary>
        /// Asset信息映射Map(Key为Asset名(含后缀)，Value为对应Asset路径)
        /// </summary>
        private Dictionary<string, string> mAssetNameAndPathMap;
            
        public EditorAssetInfoAsset()
        {
            EditorAssetInfoList = new List<EditorAssetInfo>();
            mPathAssetInfoMap = new Dictionary<string, EditorAssetInfo>();
            mAssetNameAndPathMap = new Dictionary<string, string>();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init()
        {
            mPathAssetInfoMap.Clear();
            mAssetNameAndPathMap.Clear();
            if(EditorAssetInfoList == null)
            {
                return;
            }
            for (int i = 0, length = EditorAssetInfoList.Count; i < length; i++)
            {
                var editorAssetInfo = EditorAssetInfoList[i];
                if(!mPathAssetInfoMap.ContainsKey(editorAssetInfo.AssetPath))
                {
                    mPathAssetInfoMap.Add(editorAssetInfo.AssetPath, editorAssetInfo);
                }
                else
                {
                    Debug.LogError($"EditorAssetInfo信息里有同名Asset路径:{editorAssetInfo.AssetPath}，理论上打包测已经做了检测不应该发生，请检查代码!");
                }
                if(!mAssetNameAndPathMap.TryGetValue(editorAssetInfo.AssetName, out var preAssetPath))
                {
                    mAssetNameAndPathMap.Add(editorAssetInfo.AssetName, editorAssetInfo.AssetPath);
                }
                else
                {
                    Debug.LogError($"EditorAssetInfo信息里有同名Asset名:{editorAssetInfo.AssetPath}和{preAssetPath}，理论上打包测已经做了检测不应该发生，请检查代码!");
                }
            }
        }

        /// <summary>
        /// 添加EditorAssetInfo
        /// </summary>
        /// <param name="editorAssetInfo"></param>
        /// <returns></returns>
        public bool AddEditorAssetInfo(EditorAssetInfo editorAssetInfo)
        {
            if (editorAssetInfo == null)
            {
                Debug.LogError($"EditorAssetInfoAsset.AddEditorAssetInfo()失败，editorAssetInfo为空!");
                return false;
            }
            if (string.IsNullOrEmpty(editorAssetInfo.AssetPath))
            {
                Debug.LogError($"EditorAssetInfoAsset.AddEditorAssetInfo()失败，editorAssetInfo.AssetPath为空!");
                return false;
            }
            EditorAssetInfoList.Add(editorAssetInfo);
            return true;
        }

        /// <summary>
        /// 获取指定Asset名(含后缀)的Asset路径
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public string GetAssetNamePath(string assetName)
        {
            if (mAssetNameAndPathMap.TryGetValue(assetName, out var assetPath))
            {
                return assetPath;
            }
            else
            {
                Debug.LogError($"EditorAssetInfoAsset信息里找不到Asset名:{assetName}的Asset路径信息，请使用Tools->AssetBundle->更新EditorAssetInfoAsset工具更新!");
                return null;
            }
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public void ClearAllDatas()
        {
            EditorAssetInfoList?.Clear();
            mPathAssetInfoMap?.Clear();
            mAssetNameAndPathMap?.Clear();
        }
    }
}