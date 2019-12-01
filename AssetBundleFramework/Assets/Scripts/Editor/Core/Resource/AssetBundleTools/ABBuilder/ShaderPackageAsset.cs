/*
 * Description:             Shader资源类型的Asset打包抽象
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shader资源类型的Asset打包抽象
/// </summary>
public class ShaderPackageAsset : PackageAsset {

    /// <summary>
    /// Asset打包构造函数
    /// </summary>
    /// <param name="asset">当前Asset</param>
    /// <param name="dependentasset">依赖使用当前Asset的Asset</param>
    public ShaderPackageAsset(Object asset, Object dependentasset = null) : base(asset, dependentasset)
    {
        //shader打包无视现有打包规则，最终根据记录在shaderlist.txt里的数据来打一个最终shader ab
        //shader无需设置规则，允许默认INVALIDE有效
        mInvalideBuildRuleList.Remove(AssetABBuildRule.E_INVALIDE);
        mInvalideBuildRuleList.Add(AssetABBuildRule.E_ENTIRE);
        mInvalideBuildRuleList.Add(AssetABBuildRule.E_NORMAL);
        mInvalideBuildRuleList.Add(AssetABBuildRule.E_SHARE);
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
