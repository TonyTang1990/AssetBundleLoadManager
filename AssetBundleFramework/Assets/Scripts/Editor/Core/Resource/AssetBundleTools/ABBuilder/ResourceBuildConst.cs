/*
 * Description:             ResourceBuildConst.cs
 * Author:                  TONYTANG
 * Create Date:             2026//09/01
 */


using UnityEngine;

namespace TResource
{
    /// <summary>
    /// ResourceBuildConst.cs
    /// 资源打包常量定义
    /// </summary>
    public static class ResourceBuildConst
    {
        /// <summary>
        /// 项目路径的哈希值
        /// </summary>
        public static readonly int ProjectPathHashValue = Application.dataPath.GetHashCode();

        /// <summary>
        /// 压缩格式设置本地存储Key
        /// </summary>
        public const string ABBuildSettingCompressOptionKey = "ABBuildSettingCompressOption";

        /// <summary>
        /// 是否强制重新打包设置本地存储Key
        /// </summary>
        public const string ABBuildSettingIsForceRebuildKey = "ABBuildSettingIsForceRebuild";

        /// <summary>
        /// 是否AppendHash设置本地存储Key
        /// </summary>
        public const string ABBuildSettingIsAppendHashKey = "ABBuildSettingIsAppendHash";

        /// <summary>
        /// 是否Disable Write Type Tree设置本地存储Key
        /// </summary>
        public const string ABBuildSettingIsDisableWriteTypeTreeKey = "ABBuildSettingIsDisableWriteTypeTree";

        /// <summary>
        /// 是否Ignore Type Tree Change设置本地存储Key
        /// </summary>
        public const string ABBuildSettingIsIgnoreTypeTreeChangesKey = "ABBuildSettingIsIgnoreTypeTreeChanges";

        /// <summary>
        /// 是否受用PlayerSettingVersion设置本地存储Key
        /// </summary>
        public const string ABBuildSettingIsUsePlayerSettingVersionKey = "ABBuildSettingIsUsePlayerSettingVersion";
    }
}