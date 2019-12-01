/*
 * Description:             AB打包辅助类
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System.IO;

/// <summary>
/// Asset打包规则
/// </summary>
public enum AssetABBuildRule
{
    E_INVALIDE = 0,             // 无效的打包规则
    E_SHARE = 1,                // 作为共享资源，单独打成AB
    E_ENTIRE = 2,               // 作为一个整体，所有Asset用到的资源打包成一个AB
    E_MUTILPLE = 3,             // 作为一个集合，同目录下同类型Asset打包成一个AB
    E_NORMAL = 4                // 作为一个普通资源，谁依赖使用它就把它打包到使用它的Asset打包规则的AB里
}

/// <summary>
/// Asset资源类型
/// </summary>
public enum AssetPackageType
{
    E_INVALIDE = 0,                 // 无效类型
    E_SCENE = 1,                    // 场景
    E_PREFAB = 2,                   // 预制件
    E_FBX = 3,                      // FBX模型
    E_ANIMATIONCLIP = 4,            // 动画Clip
    E_ANICONTROLLER = 5,
    E_MATERIAL = 6,                 // 材质
    E_TEXTURE = 7,                  // 图集和纹理暂时不区分，默认优先级相等都是E_TEXTURE
    E_AUDIOS = 8,                   // 音效
    E_NAVMESH = 9,                  // 寻路数据
    E_SHADER = 10,                  // Shader
    E_EDITOR_ASSET = 11             // Editor Only的Asset
}

/// <summary>
/// AB打包辅助类
/// </summary>
public class ABHelper : SingletonTemplate<ABHelper> {

    /// <summary>
    /// AB打包规则对应的名字
    /// </summary>
    public const string AB_SHARE_RULE = "sharerule";
    public const string AB_NORMAL_RULE = "normalrule";
    public const string AB_MULTIPLE_RULE = "multiplerule";
    public const string AB_ENTIRE_RULE = "entirerule";

    /// <summary>
    /// 当前平台AB路径
    /// </summary>
    public string CurrentPlatformABPath
    {
        get
        {
            string platformdir = string.Empty;
#if UNITY_STANDALONE
            platformdir = "/PC/";
#elif UNITY_ANDROID
            platformdir = "/Android/";
#elif UNITY_IOS
            platformdir = "/IOS/";
#endif
            return Application.streamingAssetsPath + platformdir;
        }
    }

    /// <summary>
    /// 当前平台后缀
    /// </summary>
    public string CurrentPlatformABPostfix
    {
        get
        {
            return string.Empty;
            string platform = string.Empty;
#if UNITY_STANDALONE
            platform = ".p";
#elif UNITY_ANDROID
            platform = ".a";
#elif UNITY_IOS
            platform = ".i";
#endif
            return platform;
        }
    }

    /// <summary>
    /// 当前平台AB打包Manifest文件路径
    /// </summary>
    public string CurrentPlatformABManifestFilePath
    {
        get
        {
            string platformdir = string.Empty;
#if UNITY_STANDALONE
            platformdir = "PC";
#elif UNITY_ANDROID
            platformdir = "Android";
#elif UNITY_IOS
            platformdir = "IOS";
#endif
            return CurrentPlatformABPath + platformdir;
        }
    }

    /// <summary>
    /// 当前平台对应的字符串(暂时用于获取资源特定平台格式)
    /// </summary>
    public string CurrentPlatformString
    {
        get
        {
            string platformdir = string.Empty;
#if UNITY_STANDALONE
            platformdir = "Standalone";
#elif UNITY_ANDROID
            platformdir = "Android";
#elif UNITY_IOS
            platformdir = "iPhone";
#endif
            return platformdir;
        }
    }

    /// <summary>
    /// 記錄依賴文件信息文件後綴
    /// </summary>
    public string AssetBundleDpInfoFilePostFix
    {
        get
        {         
            return ".d";
        }
    }

    /// <summary>
    /// 最终需要打包的Shader信息的文件夹路径
    /// </summary>
    public string FinalShaderPath
    {
        get
        {
            return mFinalShaderPath;
        }
    }
    private string mFinalShaderPath = Application.dataPath + "/../ShaderBuild/";

    /// <summary>
    /// 最终需要打包的Shader文件的后缀
    /// </summary>
    public string ShaderFilePostFix
    {
        get
        {
            return mShaderFilePostfix;
        }
    }
    private string mShaderFilePostfix = ".txt";

    /// <summary>
    /// 最终打包的Shader AB名字(同时也是存储Shader打包信息的文件名)
    /// </summary>
    public string ShaderABName
    {
        get
        {
            return mShaderABName;
        }
    }
    private string mShaderABName = "shaderlist";

    /// <summary>
    /// 打包依赖信息存储路径
    /// </summary>
    public string PackageAssetInfoBrowerFilePath
    {
        get
        {
            return mPackageAssetInfoBrowerFilePath;
        }
    }
    private string mPackageAssetInfoBrowerFilePath = Application.dataPath + "/../AssetBundleTemp/";

    /// <summary>
    /// 打包依赖信息文件名
    /// </summary>
    public string PackageAssetInfoBrowerFileName
    {
        get
        {
            return mPackageAssetInfoBrowerFileName;
        }
    }
    private string mPackageAssetInfoBrowerFileName = "packageassetinfo.bytes";

    /// <summary>
    /// 打包信息文件名
    /// </summary>
    public string AssetBundleBuildInfoBrowerFileName
    {
        get
        {
            return mAssetBundleBuildInfoBrowerFileName;
        }
    }
    private string mAssetBundleBuildInfoBrowerFileName = "assetbundleinfo.bytes";


    /// <summary>
    /// AB打包模式
    /// </summary>
    public BuildAssetBundleOptions ABBuildOptions
    {
        get
        {
            return BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.DisableWriteTypeTree;
        }
    }

    /// <summary>
    /// 文件格式后缀与资源格式映射Map
    /// Key为文件格式后缀(e.g. .unity)，Value为对应资源格式
    /// </summary>
    private Dictionary<string, AssetPackageType> mFileAndABBuildRuleMap;

    /// <summary>
    /// 依赖配置文件名
    /// </summary>
    public const string DependencyFileName = "allabdep";

    /// <summary>
    /// 无效的Asset后缀名
    /// </summary>
    private static string[] mInvalideAssetPostFix = { ".cs", "dll" };

    public ABHelper()
    {
        mFileAndABBuildRuleMap = new Dictionary<string, AssetPackageType>();

        mFileAndABBuildRuleMap.Add(".unity", AssetPackageType.E_SCENE);

        mFileAndABBuildRuleMap.Add(".prefab", AssetPackageType.E_PREFAB);

        mFileAndABBuildRuleMap.Add(".fbx", AssetPackageType.E_FBX);

        mFileAndABBuildRuleMap.Add(".anim", AssetPackageType.E_ANIMATIONCLIP);

        mFileAndABBuildRuleMap.Add(".controller", AssetPackageType.E_ANICONTROLLER);

        mFileAndABBuildRuleMap.Add(".mat", AssetPackageType.E_MATERIAL);

        mFileAndABBuildRuleMap.Add(".png", AssetPackageType.E_TEXTURE);
        mFileAndABBuildRuleMap.Add(".tif", AssetPackageType.E_TEXTURE);
        mFileAndABBuildRuleMap.Add(".jpg", AssetPackageType.E_TEXTURE);
        mFileAndABBuildRuleMap.Add(".psd", AssetPackageType.E_TEXTURE);
        mFileAndABBuildRuleMap.Add(".exr", AssetPackageType.E_TEXTURE);             // 光照烘焙图

        mFileAndABBuildRuleMap.Add(".mp3", AssetPackageType.E_AUDIOS);
        mFileAndABBuildRuleMap.Add(".wav", AssetPackageType.E_AUDIOS);

        mFileAndABBuildRuleMap.Add(".nav", AssetPackageType.E_NAVMESH);

        mFileAndABBuildRuleMap.Add(".shader", AssetPackageType.E_SHADER);

        mFileAndABBuildRuleMap.Add(".asset", AssetPackageType.E_EDITOR_ASSET);
    }

    ~ABHelper()
    {
        mFileAndABBuildRuleMap = null;
    }

    /// <summary>
    /// 获取当前AB打包平台
    /// </summary>
    /// <returns></returns>
    public BuildTarget getCurrentPlatformABBuildTarget()
    {
#if UNITY_STANDALONE
        return BuildTarget.StandaloneWindows;
#elif UNITY_ANDROID
        return BuildTarget.Android;
#elif UNITY_IOS
        return BuildTarget.iOS;
#endif
    }

    /// <summary>
    /// 获取Asset的AB打包规则
    /// </summary>
    /// <param name="assetpath">Asset资源相对路径</param>
    /// <returns></returns>
    public AssetABBuildRule getAssetABBuildRule(string assetpath)
    {
        //换成根据Asset的AB名字设置决定规则
        var assetimporter = AssetImporter.GetAtPath(assetpath);
        var abname = assetimporter.assetBundleName;
        if (abname.Equals(AB_SHARE_RULE))
        {
            return AssetABBuildRule.E_SHARE;
        }
        else if (abname.Equals(AB_NORMAL_RULE))
        {
            return AssetABBuildRule.E_NORMAL;
        }
        else if (abname.Equals(AB_MULTIPLE_RULE))
        {
            return AssetABBuildRule.E_MUTILPLE;
        }
        else if (abname.Equals(AB_ENTIRE_RULE))
        {
            return AssetABBuildRule.E_ENTIRE;
        }
        else
        {
            return AssetABBuildRule.E_INVALIDE;
        }
    }

    /// <summary>
    /// 获取Asset的资源类型
    /// </summary>
    /// <param name="assetpath">Asset资源相对路径</param>
    /// <returns></returns>
    public AssetPackageType getAssetPackageType(string assetpath)
    {
        var lowerfileextension = Path.GetExtension(assetpath).ToLower();
        if (mFileAndABBuildRuleMap.ContainsKey(lowerfileextension))
        {
            return mFileAndABBuildRuleMap[lowerfileextension];
        }
        else
        {
            return AssetPackageType.E_INVALIDE;
        }
    }

    /// <summary>
    /// 以Multiple打包规则获取指定asset的asset列表
    /// 非Multiple规则asset返回null
    /// </summary>
    /// <param name="assetpath"></param>
    /// <returns></returns>
    public List<string> getAssetListForMultipleRule(string assetpath)
    {
        var assetbuildrule = getAssetABBuildRule(assetpath);
        if(assetbuildrule != AssetABBuildRule.E_MUTILPLE)
        {
            Debug.LogError(string.Format("assetpath:{0}不是放在MultipleRule目录下的资源，无法以Multiple规则获取Asset列表!", assetpath));
            return null;
        }
        else
        {
            var assettype = getAssetPackageType(assetpath);
            var assetfullpath = Path.GetFullPath(assetpath);
            var assetfolderpath = Path.GetDirectoryName(assetfullpath);
            var filespathlist = Directory.GetFiles(assetfolderpath);
            var validefilepathlist = new List<string>();
            foreach(var filepath in filespathlist)
            {
                var fileassettype = getAssetPackageType(filepath);
                if(fileassettype == assettype)
                {
                    validefilepathlist.Add(filepath);
                }
            }
            return validefilepathlist;
        }
    }

    /// <summary>
    /// 检查Asset是否使用了内置资源
    /// </summary>
    /// <param name="asset">Asset资源</param>
    /// <returns></returns>
    public bool isUsingBuildInAsset(Object asset)
    {
        if (asset != null)
        {
            var assetpath = AssetDatabase.GetAssetPath(asset);
            var collectdps = EditorUtility.CollectDependencies(new Object[] { asset });
            foreach (var cltdp in collectdps)
            {
                var cltappath = AssetDatabase.GetAssetPath(cltdp);
                if (cltappath.ToLower().Contains("unity_builtin_extra"))
                {
                    Debug.LogError(string.Format("Asset Path: {0} 使用了内置资源 {1}!不允许使用内置资源打包，请替换成自己的资源重新打包!", assetpath, cltdp.name));
                    return true;
                }
            }            
        }
        return false;
    }

    /// <summary>
    /// 是否是Editor Only的资源
    /// </summary>
    /// <param name="assetpath"></param>
    /// <returns></returns>
    public bool isEditorOnlyAsset(string assetpath)
    {
        if (!string.IsNullOrEmpty(assetpath))
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetpath);
            if(asset != null)
            {
                //Note:
                //场景文件比较特别，需要参与打包，但FullName却属于UnityEditor
                if (asset.GetType().FullName.Contains("UnityEditor") && !assetpath.EndsWith(".unity"))
                {
                    Debug.LogError(string.Format("Asset Path: {0} 是Editor Only资源!", assetpath));
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 是否是特殊Asset，不需要考虑打包规则和结论(e.g. NavMesh.asset，寻路Asset跟场景打包一起，不考虑打包规则)
    /// </summary>
    /// <param name="assetpath"></param>
    /// <returns></returns>
    public bool isSpecialAssetNoNeedConsider(string assetpath)
    {
        if (!string.IsNullOrEmpty(assetpath))
        {
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetpath);
            if (asset != null)
            {
                //Note:
                //NavMesh.asset寻路数据比较特别，需要和场景文件打包一起，无视打包规则
                if (asset.GetType().FullName.Contains("UnityEngine.AI.NavMeshData"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 將AB依赖信息写入文件中
    /// </summary>
    /// <param name="assetbundlename"></param>
    /// <param name="dpssetbundles"></param>
    public void writeAssetBundleDpToFile(string assetbundlename, string[] dpssetbundles)
    {
        using (StreamWriter sw = new StreamWriter(CurrentPlatformABPath + assetbundlename + AssetBundleDpInfoFilePostFix))
        {
            foreach(var dpab in dpssetbundles)
            {
                sw.WriteLine(dpab);
            }
        }
    }

    /// <summary>
    /// 检查Asset文件是否是符合条件的文件(判断后缀名)
    /// </summary>
    /// <param name="assetpath"></param>
    /// <returns></returns>
    public bool isValideAssetFile(string assetpath)
    {
        foreach (var invalidepostfix in mInvalideAssetPostFix)
        {
            if (assetpath.EndsWith(invalidepostfix))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 是否是场景文件对象
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public bool isSceneAssetFile(Object obj)
    {
        if(obj != null)
        {
            var assetpath = AssetDatabase.GetAssetPath(obj);
            if (assetpath.EndsWith(".unity"))
            {
                return true;
            }
        }
        return false;
    }


    #region Shader打包部分
    /// <summary>
    /// 检查Shader打包信息目录
    /// </summary>
    public void checkShaderBuildFolder()
    {
        Utilities.CheckAndCreateSpecificFolder(mFinalShaderPath);
    }

    /// <summary>
    /// 添加Shader到编译列表
    /// </summary>
    /// <param name="sdarelativepath"></param>
    public void addShaderToBuildList(string sdarelativepath)
    {
        if (sdarelativepath == null || sdarelativepath == string.Empty)
        {
            EditorUtility.DisplayDialog("警告", "不能添加空的Shader到编译列表!", "确认");
            return;
        }
        else if (AssetDatabase.LoadAssetAtPath<Object>(sdarelativepath) == null)
        {
            Debug.LogError(string.Format("找不到要添加的Shader:{0}资源，请确认工程里有再打包添加!", sdarelativepath));
            EditorUtility.DisplayDialog("警告", string.Format("找不到要添加的Shader:{0}资源，请确认工程里有再打包添加!", sdarelativepath), "确认");
            return;
        }
        else if (isShaderExsitInBuildList(sdarelativepath))
        {
            return;
        }

        var shaderfilepath = mFinalShaderPath + ShaderABName + ShaderFilePostFix;
        using (FileStream fs = new FileStream(shaderfilepath, FileMode.Append, FileAccess.Write))
        using (StreamWriter sw = new StreamWriter(fs))
        {
            sw.WriteLine(sdarelativepath);
        }
        EditorUtility.DisplayDialog("提示", string.Format("添加Shader:{0}到编译列表成功!请记得提交最新的shaderlist.txt文件!", sdarelativepath), "确认");
    }

    /// <summary>
    /// 检查Shader编译文件目录以及文件是否存在，不存在就创建一份
    /// </summary>
    private void checkOrCreateShaderBuildFolderAndFile()
    {
        checkShaderBuildFolder();
        var shaderfilepath = mFinalShaderPath + ShaderABName + ShaderFilePostFix;
        if (!File.Exists(shaderfilepath))
        {
            File.Create(shaderfilepath).Close();
        }
    }
    
    /// <summary>
    /// 判定指定路径Shader是否已经在编译列表里
    /// </summary>
    /// <param name="sdassetpath"></param>
    /// <returns></returns>
    public bool isShaderExsitInBuildList(string sdassetpath)
    {
        checkOrCreateShaderBuildFolderAndFile();
        var shaderfilepath = mFinalShaderPath + ShaderABName + ShaderFilePostFix;
        StreamReader sr = new StreamReader(shaderfilepath);
        Dictionary<string, string> sdatmap = new Dictionary<string, string>();
        string sdatpath;
        while ((sdatpath = sr.ReadLine()) != null)
        {
            if (sdatmap.ContainsKey(sdatpath))
            {
                continue;
            }
            else
            {
                sdatmap.Add(sdatpath, sdatpath);
            }
        }
        sr.Close();
        return sdatmap.ContainsKey(sdassetpath);
    }
    #endregion
}
