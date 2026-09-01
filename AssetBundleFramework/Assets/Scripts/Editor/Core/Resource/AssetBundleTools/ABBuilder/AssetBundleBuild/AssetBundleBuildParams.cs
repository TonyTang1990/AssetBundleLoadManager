/*
 * Description:             AssetBundleBuildParams.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/11
 */

using UnityEditor;
using UnityEngine;

namespace TResource
{
    /// <summary>
    /// AssetBundleBuildParams.cs
    /// AB打包参数类
    /// </summary>
    public class AssetBundleBuildParams
    {
		/// <summary>
		/// AB打包用途枚举
		/// </summary>
		public AssetBundleBuildPurpose AssetBundleBuildPurpose
		{
			get;
			set;
		} = AssetBundleBuildPurpose.BuildPlayerBaseLine;

		/// <summary>
		/// 构建平台
		/// </summary>
		public BuildTarget BuildTarget
		{
		 	get;
			private set;
		} = BuildTarget.NoTarget;

        /// <summary>
        /// 构建平台目标输出目录
        /// </summary>
        public string BuildTargetOutputFolder
        {
            get
            {
                return AssetBundleBuilderHelper.GetBuildTargetOutputRootPath(BuildTarget);
            }
        }

		/// <summary>
		/// 构建选项
		/// </summary>
		public ABCompressOption CompressOption
        {
            get;
            set;
        } = ABCompressOption.ChunkBasedCompressionLZ4;

		/// <summary>
		/// 是否强制重新打包资源
		/// </summary>
		public bool IsForceRebuild
        {
            get;
            set;
        } = false;

		/// <summary>
		/// 是否添加Hash名字到打包出来的AB文件名里
		/// </summary>
		public bool IsAppendHash
        {
            get;
            set;
        } = false;

		/// <summary>
		/// 是否禁止像AB里添加type信息(启用这个会容易受Unity版本升级等影响)
		/// </summary>
		public bool IsDisableWriteTypeTree
        {
            get;
            set;
        } = false;

		/// <summary>
		/// 是否忽略Type Tree的变化(启用这个在Type Tree变化时不重新打包AB，不推荐使用)
		/// Type Tree指的是一些类型结构变化，比如Class Player新增字段或者改变一个成员类型
		/// Note:
		/// SBP不支持这个
		/// </summary>
		public bool IsIgnoreTypeTreeChanges
        {
            get;
            set;
        } = false;

        /// <summary>
        /// 版本号(仅热更新AB打包时有用)
        /// </summary>
        public double VersionCode
        {
            get;
            set;
        } = 0;

        /// <summary>
        /// 资源版本号(仅热更新AB打包时有用)
        /// </summary>
        public int ResourceVersionCode
        {
            get;
            set;
        } = 0;

        public AssetBundleBuildParams(BuildTarget buildTarget)
        {
            BuildTarget = buildTarget;
        }

        public AssetBundleBuildParams(AssetBundleBuildPurpose assetBundleBuildPurpose, BuildTarget buildTarget,
                                      ABCompressOption compressOption = ABCompressOption.ChunkBasedCompressionLZ4,
                                      bool isForceRebuild = false, bool isAppendHash = false,
                                      bool isDisableWriteTypeTree = false, bool isIgnoreTypeTreeChanges = false,
                                      double versionCode = 0, int resourceVersionCode = 0)
        {
            AssetBundleBuildPurpose = assetBundleBuildPurpose;
            BuildTarget = buildTarget;
            CompressOption = compressOption;
            IsForceRebuild = isForceRebuild;
            IsAppendHash = isAppendHash;
            IsDisableWriteTypeTree = isDisableWriteTypeTree;
            IsIgnoreTypeTreeChanges = isIgnoreTypeTreeChanges;
            VersionCode = versionCode;
            ResourceVersionCode = resourceVersionCode;
        }

        /// <summary>
        /// 打印所有参数
        /// </summary>
        public void PrintAllParams()
        {
            Debug.Log($"AssetBundle打包参数:");
            Debug.Log($"AssetBundleBuildParams:{AssetBundleBuildPurpose}");
            Debug.Log($"BuildTarget:{BuildTarget}");
            Debug.Log($"BuildTargetOutputFolder:{BuildTargetOutputFolder}");
            Debug.Log($"CompressOption:{CompressOption}");
            Debug.Log($"IsForceRebuild:{IsForceRebuild}");
            Debug.Log($"IsAppendHash:{IsAppendHash}");
            Debug.Log($"IsDisableWriteTypeTree:{IsDisableWriteTypeTree}");
            Debug.Log($"IsIgnoreTypeTreeChanges:{IsIgnoreTypeTreeChanges}");
            Debug.Log($"VersionCode:{VersionCode}");
            Debug.Log($"ResourceVersionCode:{ResourceVersionCode}");
        }
    }
}
