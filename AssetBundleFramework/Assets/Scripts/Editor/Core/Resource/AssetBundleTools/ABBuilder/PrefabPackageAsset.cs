/*
 * Description:             预制件资源类型的Asset打包抽象
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 预制件资源类型的Asset打包抽象
/// </summary>
public class PrefabPackageAsset : PackageAsset {

    /// <summary>
    /// Asset打包构造函数
    /// </summary>
    /// <param name="asset">当前Asset</param>
    /// <param name="dependentasset">依赖使用当前Asset的Asset</param>
    public PrefabPackageAsset(Object asset, Object dependentasset = null) : base(asset, dependentasset)
    {
        mInvalideBuildRuleList.Add(AssetABBuildRule.E_MUTILPLE);
    }

    /// <summary>
    /// 检测Asset是否符合打包条件限制
    /// </summary>
    /// <returns></returns>
    protected override bool checkAssetPackageLimit()
    {
        return true;
    }
}
