/*
 * Description:             Texture和Atlas资源类型的Asset打包抽象
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

/// <summary>
/// Texture和Atlas资源类型的Asset打包抽象
/// </summary>
public class TexturePackageAsset : PackageAsset {

    /// <summary>
    /// Asset打包构造函数
    /// </summary>
    /// <param name="asset">当前Asset</param>
    /// <param name="dependentasset">依赖使用当前Asset的Asset</param>
    public TexturePackageAsset(Object asset, Object dependentasset = null) : base(asset, dependentasset)
    {
        //mInvalideBuildRuleList.Add(AssetABBuildRule.E_ENTIRE);
    }

    /// <summary>
    /// 检测Asset是否符合打包条件限制
    /// </summary>
    /// <returns></returns>
    protected override bool checkAssetPackageLimit()
    {
        //Normal Rule Texture Asset如果打包结论上层不是Material的话，强制使用Share Rule规则，避免被多个Prefab引用时存在潜在问题
        //e.g. 
        //打包时如果只有Prefab1 -> T1   Prefab2 -> T1会导致T1只跟其中一个打包在一起其中一个Prefab依赖另一个Prefab的AB
        //出现T1打包规则为NormalRule时被多个Prefab引用时，需要确保打包机制得出的最终结论T1不属于Prefab层，而属于其他层比如FBX(EntireRule)
        //所以如果T1不是作为跟随FBX层打包又被多个Prefab使用的话，强烈建议使用ShareRule打包T1
        //if (mAssetAssetBundleBuildRule == AssetABBuildRule.E_NORMAL && mDependentPackageAsset.PackageAssetType == AssetPackageType.E_PREFAB)
        //{
        //    Debug.LogError(string.Format("Asset Path: {0}打包規則為NormalRule，但被最终上层引用是E_PREFAB，请使用ShareRule打包此资源!！", AssetPath));
        //    return false;
        //}

        if(!checkTextureImporter())
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 检查纹理资源导入相关设置
    /// </summary>
    /// <returns></returns>
    private bool checkTextureImporter()
    {
        var textureassetimporter = AssetImporter.GetAtPath(mAssetPath) as TextureImporter;
        var texturecompressionfomart = textureassetimporter.GetAutomaticFormat(ABHelper.Singleton.CurrentPlatformString);

        // 检查纹理压缩格式
#if UNITY_STANDALONE
        Debug.LogError("暂时不支持PC平台打包AB!");
        return false;
#elif UNITY_ANDROID
        // 建议在Android模式下，透贴格式为RGBA16bit或者ETC2 RGBA 8bits，非透贴格式为RGB ETC 4bits
        if (texturecompressionfomart != TextureImporterFormat.RGBA16 && texturecompressionfomart != TextureImporterFormat.ETC_RGB4 && texturecompressionfomart != TextureImporterFormat.ETC2_RGBA8)
        {
            Debug.LogWarning(string.Format("警告Asset Path: {0}，{1} Platform, 纹理格式为:{2}， 除非显示效果要求特别高，建议透贴格式为RGBA16bit，非透贴格式为RGB ETC 4bits。", mAssetPath, ABHelper.Singleton.CurrentPlatformString, texturecompressionfomart));
        }
#elif UNITY_IOS
        // 建议ios模式下，透贴格式为RGBA PVRTC4bits，非透贴格式为 RBG PVRTC 4bits。
        if(texturecompressionfomart != TextureImporterFormat.PVRTC_RGBA4 && texturecompressionfomart != TextureImporterFormat.PVRTC_RGB4)
        {
            Debug.LogWarning(string.Format("警告Asset Path: {0}，{1} Platform, 纹理格式为:{2}， 除非显示效果要求特别高，建议透贴格式为RGBA PVRTC4bits，非透贴格式为RBG PVRTC 4bits。", mAssetPath, ABHelper.Singleton.CurrentPlatformString, texturecompressionfomart));
        }
#endif
        if (textureassetimporter.maxTextureSize > 2048)
        {
            Debug.LogWarning(string.Format("警告Asset Path: {0}，{1} Platform, 纹理maxTextureSize设置:{2}超过了2048，建议最大值设置为2048。", mAssetPath, ABHelper.Singleton.CurrentPlatformString, texturecompressionfomart));
        }
        switch (textureassetimporter.textureType)
        {
            case TextureImporterType.Default:

                break;
            case TextureImporterType.Sprite:
                if (textureassetimporter.mipmapEnabled)
                {
                    Debug.LogError(string.Format("Asset Path: {0}，{1} Platform, UI Sprite不允许开启Mipmap功能，打包失败!", mAssetPath, ABHelper.Singleton.CurrentPlatformString));
                    return false;
                }
                break;
            default:

                break;
        }
        return true;
    }
}
