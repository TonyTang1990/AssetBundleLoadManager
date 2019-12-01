/*
 * Description:             Asset打包抽象
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.IO;

/// <summary>
/// Asset打包抽象
/// </summary>
public class PackageAsset{

    /// <summary>
    /// Asset对象
    /// </summary>
    public Object AssetObject
    {
        get
        {
            return mAssetObject;
        }
    }
    protected Object mAssetObject;

    /// <summary>
    /// 当前Asset的资源类型
    /// </summary>
    public AssetPackageType PackageAssetType
    {
        get
        {
            return mPackageAssetType;
        }
    }
    protected AssetPackageType mPackageAssetType;

    /// <summary>
    /// Asset打包规则
    /// </summary>
    public AssetABBuildRule AssetAssetBundleBuildRule
    {
        get
        {
            return mAssetAssetBundleBuildRule;
        }
    }
    protected AssetABBuildRule mAssetAssetBundleBuildRule;

    /// <summary>
    /// Asset相对路径
    /// </summary>
    public string AssetPath
    {
        get
        {
            return mAssetPath;
        }
    }
    protected string mAssetPath;

    /// <summary>
    /// 依赖使用当前Asset的Asset
    /// 用于抽象出Asset之间引用层级关系(辅助AB名字打包结论)
    /// </summary>
    public PackageAsset DependentPackageAsset
    {
        get
        {
            return mDependentPackageAsset;
        }
        set
        {
            mDependentPackageAsset = value;
        }
    }
    protected PackageAsset mDependentPackageAsset;

    /// <summary>
    /// 无效的打包规则列表
    /// </summary>
    public List<AssetABBuildRule> InvalideBuildRuleList
    {
        get
        {
            return mInvalideBuildRuleList;
        }
    }
    protected List<AssetABBuildRule> mInvalideBuildRuleList;
    
    private PackageAsset()
    {
        mAssetObject = null;
        mPackageAssetType = AssetPackageType.E_INVALIDE;
        mAssetAssetBundleBuildRule = AssetABBuildRule.E_INVALIDE;
        mAssetPath = string.Empty;
        mDependentPackageAsset = null;
        mInvalideBuildRuleList = null;
    }

    /// <summary>
    /// Asset打包构造函数
    /// </summary>
    /// <param name="asset">当前Asset</param>
    /// <param name="dependentasset">依赖使用当前Asset的Asset</param>
    public PackageAsset(Object asset, Object dependentasset = null)
    {
        mAssetObject = asset;
        if (mAssetObject == null)
        {
            mPackageAssetType = AssetPackageType.E_INVALIDE;
            mAssetAssetBundleBuildRule = AssetABBuildRule.E_INVALIDE;
            mAssetPath = string.Empty;
            mDependentPackageAsset = null;
            mInvalideBuildRuleList = null;
        }
        else
        {
            mAssetPath = AssetDatabase.GetAssetPath(mAssetObject);
            mPackageAssetType = ABHelper.Singleton.getAssetPackageType(mAssetPath);
            mAssetAssetBundleBuildRule = ABHelper.Singleton.getAssetABBuildRule(mAssetPath);
            if(dependentasset != null)
            {
                mDependentPackageAsset = new PackageAsset(dependentasset);
            }
            else
            {
                mDependentPackageAsset = null;
            }
            mInvalideBuildRuleList = new List<AssetABBuildRule>();
            mInvalideBuildRuleList.Add(AssetABBuildRule.E_INVALIDE);
        }
    }

    /// <summary>
    /// 检查Asset打包条件
    /// </summary>
    /// <returns>是否更新成功</returns>
    public bool checkAssetPackageConditions()
    {
        if(!checkAssetInfo() || !checkSupportedBuildRuleLimit() || !checkAssetPackageLimit())
        {
            return false;
        }
        return true;
    }

    /// <summary>
    /// 根据打包规则获取PackageAsset的最终打包的AB名字
    /// </summary>
    /// <returns></returns>
    public string getPackageAssetABName()
    {
        if (mAssetObject != null)
        {
            var abname = string.Empty;
            // Shader默认都打包到同一个AB里，这里给个默认的打包名是为了避免Shader被打包到其他AB里
            // 真正的Shader AB是根据shaderlist.txt里记录的信息打包得到的
            if(mPackageAssetType == AssetPackageType.E_SHADER)
            {
                abname = ABHelper.Singleton.ShaderABName;
            }
            else
            {
                switch (mAssetAssetBundleBuildRule)
                {
                    case AssetABBuildRule.E_SHARE:
                    case AssetABBuildRule.E_ENTIRE:
                        abname = mAssetObject.name;
                        break;
                    case AssetABBuildRule.E_MUTILPLE:
                        abname = Utilities.GetFileFolderName(mAssetPath);
                        break;
                    case AssetABBuildRule.E_NORMAL:
                        // 递归判定出最底层Asset最终Normal规则打包的AB名字
                        abname = mDependentPackageAsset.getPackageAssetABName();
                        break;
                    default:
                        break;
                }
            }
            return abname;
        }
        else
        {
            return string.Empty;
        }
    }
    
    /// <summary>
    /// 检测Asset是否符合打包条件限制(不同的Asset类型资源有不同的限制，继承重写各类型资源的资源限制)
    /// </summary>
    /// <returns></returns>
    protected virtual bool checkAssetPackageLimit()
    {
        return true;
    }

    /// <summary>
    /// 檢查打包規則支持限制
    /// </summary>
    /// <returns></returns>
    private bool checkSupportedBuildRuleLimit()
    {
        if(mInvalideBuildRuleList != null)
        {
            foreach(var invalidebuildrule in mInvalideBuildRuleList)
            {
                if(mAssetAssetBundleBuildRule == invalidebuildrule)
                {
                    Debug.LogError(string.Format("Asset Path: {0} {1}類型文件不支持{2}打包規則，请修改文件打包规则设定!", mAssetPath, mPackageAssetType, mAssetAssetBundleBuildRule));
                    return false;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// 检查Asset相关信息
    /// </summary>
    private bool checkAssetInfo()
    {
        if (mAssetObject == null)
        {
            Debug.Log("Asset不能为空！");
            return false;
        }
        else
        {
            // Shader打包默认都打包到一个shaderlist ab里(临时用，避免shader被重复打包到其他AB里)
            // 真正的Shader AB最终根据shaderlist.txt里的数据打包出一个最终AB
            if (mPackageAssetType == AssetPackageType.E_SHADER && mAssetAssetBundleBuildRule == AssetABBuildRule.E_INVALIDE)
            {
                return true;
            }
            else if (mPackageAssetType == AssetPackageType.E_SHADER && mAssetAssetBundleBuildRule != AssetABBuildRule.E_INVALIDE)
            {
                Debug.LogError(string.Format("Asset Path: {0} Shader打包无需设置规则!", mAssetPath));
                return false;
            }
            else if (mPackageAssetType == AssetPackageType.E_INVALIDE || mAssetAssetBundleBuildRule == AssetABBuildRule.E_INVALIDE)
            {
                Debug.LogError(string.Format("Asset Path: {0}打包规则或者资源类型无效，请检查Asset目录以及Asset资源格式支持!", mAssetPath));
                return false;
            }
            else if(mAssetAssetBundleBuildRule == AssetABBuildRule.E_NORMAL && mDependentPackageAsset == null)
            {
                Debug.LogError(string.Format("Asset Path: {0}打包规则为Normal却找不到依赖使用他的Asset，不能單獨打包!", mAssetPath));
                return false;
            }
            else
            {
                return true;
            }
        }
    }
    
    /// <summary>
    /// 根据Asset资源类型创建对应的Package Asset对象(创建PackageAsset统一入口)
    /// </summary>
    /// <param name="asset">当前Asset</param>
    /// <param name="dependentasset">依赖当前Asset的Asset</param>
    /// <returns></returns>
    public static PackageAsset createPackageAsset(Object asset, Object dependentasset = null)
    {
        if (asset != null)
        {
            var assetpath = AssetDatabase.GetAssetPath(asset);
            var assettype = ABHelper.Singleton.getAssetPackageType(assetpath);
            PackageAsset pa = null;
            switch (assettype)
            {
                case AssetPackageType.E_SCENE:
                    pa = new ScenePackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_PREFAB:
                    pa = new PrefabPackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_FBX:
                    pa = new FBXPackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_ANIMATIONCLIP:
                    pa = new AnimationClipPackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_ANICONTROLLER:
                    pa = new AnimationControllerPackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_MATERIAL:
                    pa = new MaterialPackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_TEXTURE:
                    pa = new TexturePackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_AUDIOS:
                    pa = new AudiosPackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_NAVMESH:
                    pa = new ScenePackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_SHADER:
                    pa = new ShaderPackageAsset(asset, dependentasset);
                    break;
                case AssetPackageType.E_EDITOR_ASSET:
                    pa = new EditorAssetPackageAsset(asset, dependentasset);
                    break;
                default:
                    break;
            }
            return pa;
        }
        else
        {
            return null;
        }
    }
}

