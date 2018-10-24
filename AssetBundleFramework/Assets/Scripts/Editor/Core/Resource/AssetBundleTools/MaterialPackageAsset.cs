/*
 * Description:             Material资源类型的Asset打包抽象
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Material资源类型的Asset打包抽象
/// </summary>
public class MaterialPackageAsset : PackageAsset {

    /// <summary>
    /// Asset打包构造函数
    /// </summary>
    /// <param name="asset">当前Asset</param>
    /// <param name="dependentasset">依赖使用当前Asset的Asset</param>
    public MaterialPackageAsset(Object asset, Object dependentasset = null) : base(asset, dependentasset)
    {
        mInvalideBuildRuleList.Add(AssetABBuildRule.E_ENTIRE);
        mInvalideBuildRuleList.Add(AssetABBuildRule.E_MUTILPLE);
    }

    /// <summary>
    /// 检测Asset是否符合打包条件限制
    /// </summary>
    /// <returns></returns>
    protected override bool checkAssetPackageLimit()
    {
        //Normal Rule Material Asset如果打包结论上层不是FBX的话，强制使用Share Rule规则，避免被多个Prefab引用时存在潜在问题
        //e.g. 
        //打包时如果只有Prefab1 -> M1   Prefab2 -> M1会导致M1只跟其中一个打包在一起其中一个Prefab依赖另一个Prefab的AB
        //出现M1打包规则为NormalRule时被多个Prefab引用时，需要确保打包机制得出的最终结论M1不属于Prefab层，而属于其他层比如FBX(EntireRule)
        //所以如果M1不是作为跟随FBX层打包又被多个Prefab使用的话，强烈建议使用ShareRule打包M1
        //if(mAssetAssetBundleBuildRule == AssetABBuildRule.E_NORMAL && mDependentPackageAsset.PackageAssetType == AssetPackageType.E_PREFAB)
        //{
        //    Debug.LogError(string.Format("Asset Path: {0}打包規則為NormalRule，但被最终上层引用是E_PREFAB，请使用ShareRule打包此资源!！", AssetPath));
        //    return false;
        //}
        return true;
    }
}
