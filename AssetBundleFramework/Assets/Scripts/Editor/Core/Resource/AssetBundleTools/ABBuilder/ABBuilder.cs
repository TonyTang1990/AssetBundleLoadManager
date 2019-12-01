/*
 * Description:             AB打包工具
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using System.IO;
using UnityEditor.SceneManagement;

/// <summary>
/// AB打包工具
/// </summary>
public class ABBuilder {

#if AB_PACKAGE_SYSTEM
    [MenuItem("Tools/Assetbundle/打包当前平台选中对象 %#X", false, 100)]
#endif
    public static void packageAssetForCurrentPlatform()
    {
        var objs = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        if(objs == null || objs.Length == 0)
        {
            Debug.LogError("没有有效的选中Asset!");
            return;
        }
        else if(objs.Length > 1)
        {
            Debug.LogError("每次打包暫時只支持選中一個Asset進行打包，不能選中多個!");
            return;
        }

        if (ABHelper.Singleton.isSceneAssetFile(objs[0]))
        {
            Debug.LogError("此功能只支持非场景资源打包，场景打包请使用Tools->Assetbundle->打包选中场景");
            return;
        }

        if(!packageABForSpecificAsset(objs[0]))
        {
            Debug.LogError(string.Format("打包:{0}失败!", objs[0].name));
        }
    }

    #if AB_PACKAGE_SYSTEM
    [MenuItem("Tools/Assetbundle/打包选中场景", false, 101)]
    #endif
    public static void packageSceneAssetForCurrentPlatform()
    {
        var objs = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        if (objs == null || objs.Length == 0)
        {
            Debug.LogError("没有有效的选中Scene Asset!");
            return;
        }
        else if (objs.Length > 1)
        {
            Debug.LogError("每次打包暂时只支持選中一個Asset進行打包，不能選中多個!");
            return;
        }
        
        if(!ABHelper.Singleton.isSceneAssetFile(objs[0]))
        {
            Debug.LogError("此功能只支持打包场景!");
            return;
        }

        var originalsceneassetpath = AssetDatabase.GetAssetPath(objs[0]);
        var scenename = Path.GetFileNameWithoutExtension(originalsceneassetpath);

        //打包场景AB
        var sceneasset = AssetDatabase.LoadAssetAtPath<Object>(originalsceneassetpath);
        if(!packageABForSpecificAsset(sceneasset))
        {
            Debug.LogError(string.Format("打包场景:{0}失败!", scenename));
            return;
        }

        EditorSceneManager.OpenScene(originalsceneassetpath);
    }

    #if AB_PACKAGE_SYSTEM
    [MenuItem("Tools/Assetbundle/批量打包选中对象(仅支持所有都是同类型资源)", false, 102)]
    #endif
    public static void packageMutilpleAssetForCurrentPlatform()
    {
        var objs = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
        if (objs == null || objs.Length == 0)
        {
            Debug.LogError("没有有效的选中Scene Asset!");
            return;
        }

        var assetpath = string.Empty;
        var assetpackagetype = AssetPackageType.E_INVALIDE;
        foreach (var obj in objs)
        {
            assetpath = AssetDatabase.GetAssetPath(obj);
            if(assetpackagetype == AssetPackageType.E_INVALIDE)
            {
                assetpackagetype = ABHelper.Singleton.getAssetPackageType(assetpath);
            }
            else if (assetpackagetype != ABHelper.Singleton.getAssetPackageType(assetpath))
            {
                Debug.LogError(string.Format("Asset Path:{0}不属于资源类型:{1}", assetpath, assetpackagetype));
                return;
            }
            else if(assetpackagetype == AssetPackageType.E_SCENE)
            {
                Debug.LogError("此功能不只支持批量打包场景!");
                return;
            }
        }

        //批量打包
        foreach (var obj in objs)
        {
            if(!packageABForSpecificAsset(obj))
            {
                Debug.LogError(string.Format("打包:{0}失败!", obj.name));
                return;
            }
        }
    }

    #if AB_PACKAGE_SYSTEM
    [MenuItem("Tools/Assetbundle/打包所有Shader %#s", false, 108)]
    #endif
    public static void PackageAllShaders()
    {
        var shaderpath = ABHelper.Singleton.FinalShaderPath;
        ABHelper.Singleton.checkShaderBuildFolder();

        var shaderfilepath = shaderpath + ABHelper.Singleton.ShaderABName + ABHelper.Singleton.ShaderFilePostFix;
        if (!File.Exists(shaderfilepath))
        {
            EditorUtility.DisplayDialog("警告", string.Format("找不到:{0}编译列表文件!打包Shader失败!", shaderfilepath), "确认");
            return;
        }

        StreamReader sr = new StreamReader(shaderfilepath);
        var sdassetfplist = new List<string>();
        var shaderassetfp = string.Empty;
        AssetBundleBuild[] abb = new AssetBundleBuild[] { new AssetBundleBuild() };
        abb[0].assetBundleName = ABHelper.Singleton.ShaderABName + ABHelper.Singleton.CurrentPlatformABPostfix;

        while ((shaderassetfp = sr.ReadLine()) != null)
        {
            Debug.Log(string.Format("需要打包的Shader : {0}", shaderassetfp));
            var sdasset = AssetDatabase.LoadAssetAtPath<Object>(shaderassetfp);
            if (sdasset == null)
            {
                EditorUtility.DisplayDialog("警告", string.Format("找不到需要打包的Shader : {0}，请确认是否Shader被误删。打包Shader失败！", shaderassetfp), "确认");
                continue;
            }
            else
            {
                sdassetfplist.Add(shaderassetfp);
            }
        }

        abb[0].assetNames = sdassetfplist.ToArray();
        BuildPipeline.BuildAssetBundles(ABHelper.Singleton.CurrentPlatformABPath, abb, ABHelper.Singleton.ABBuildOptions, ABHelper.Singleton.getCurrentPlatformABBuildTarget());
    }

    /// <summary>
    /// 打包指定Asset AB接口
    /// </summary>
    /// <param name="asset"></param>
    /// <returns>打包成功与否</returns>
    private static bool packageABForSpecificAsset(Object obj)
    {
        if (obj == null)
        {
            Debug.LogError("无效的Asset，无法打包!");
            return false;
        }

        Utilities.CheckAndCreateSpecificFolder(ABHelper.Singleton.CurrentPlatformABPath);
        
        // Asset打包映射map，Key為Asset相对路径，Value为Asset打包抽象对象
        // 用于整理出Asset的层级关系，确保每一个Asset只对应一个PackageAsset抽象对象
        Dictionary<string, PackageAsset> packageassetmap = new Dictionary<string, PackageAsset>();

        if (!getAllPackageAssets(obj, ref packageassetmap))
        {
            return false;
        }

        foreach (var packageasset in packageassetmap.Values)
        {
            if (!packageasset.checkAssetPackageConditions())
            {
                return false;
            }
        }

        //得出最终打包结论
        //打包映射map，Key为AB名字，Value为AB里Asset列表
        Dictionary<string, List<string>> buildmap = new Dictionary<string, List<string>>();
        //检查是否用到了内置资源
        bool isusingbuildinresource = false;
        foreach (var packageasset in packageassetmap.Values)
        {
            if (ABHelper.Singleton.isUsingBuildInAsset(packageasset.AssetObject))
            {
                isusingbuildinresource = true;
            }
            var abname = packageasset.getPackageAssetABName();
            // 目前看來Unity 5的打包API在不拆分AnimationClip的情況下，暫時不支持把FBX内嵌的AnimationClip指定AB打包
            // 因爲buildlist需要指定AssetPath，但獲取到的AnimationClip的Asset Path始終是FBX Asset的路徑
            // AnimationClip拆分后重新指定預制件上使用，打包時會導致FBX AB裏一份原始的AnimationClip，拆分的新的AnimationClip一個AB
            // Note:
            // 动画单独提取出来打包会导致FBX的AB里会有一份冗余动画资源
            // 交叉使用动画时会导致交叉使用的动画所在的FBX AB整个被加载进来，
            // 当前的方案是通过脚本把动画提取出来后，把FBX设置成不导入动画的形式，然后对提取出来的动画单独打包避免动画冗余
            // TODO:
            // 每次通过脚本提取出来的动画不包含原来添加在提取出来后的动画里的Event等数据，
            // 还需要做额外操作保证更新FBX动画时提取出来后的动画保留原来的部分操作数据
            //if (packageasset.PackageAssetType == AssetPackageType.E_FBX)
            //{
            //    var paobjs = AssetDatabase.LoadAllAssetsAtPath(packageasset.AssetPath);
            //    foreach (var paobj in paobjs)
            //    {
            //        var clip = paobj as AnimationClip;
            //        if (clip != null)
            //        {
            //            var clippath = AssetDatabase.GetAssetPath(clip);
            //            Debug.Log(string.Format("clippath : {0}", clippath));
            //            Debug.Log(string.Format("clipname : {0}", clip.name));
            //        }
            //    }
            //}
            var finalabname = abname + ABHelper.Singleton.CurrentPlatformABPostfix;
            var assetlist = new List<string>();
            //AssetABBuildRule.E_MUTILPLE打包规则的Asse列表不止一个，需要单独判定
            if (packageasset.AssetAssetBundleBuildRule == AssetABBuildRule.E_MUTILPLE)
            {
                var filespathlist = ABHelper.Singleton.getAssetListForMultipleRule(packageasset.AssetPath);
                foreach (var filepath in filespathlist)
                {
                    var relativepath = "Assets" + filepath.Substring(Application.dataPath.Length);
                    assetlist.Add(relativepath);
                }
            }
            else
            {
                assetlist.Add(packageasset.AssetPath);
            }
            if (!buildmap.ContainsKey(finalabname))
            {
                buildmap.Add(finalabname, new List<string>());
            }
            //避免添加重复的Asset(e.g. 当引用多个AnimationClip时，因为Multiple规则导致多次判定动画)
            //如果动画是和FBX打一起的
            //问题：
            //1. 交叉引用动画会把依赖的FBX所有资源都加载进来(已验证)
            //2. 交叉引用动画的FBX打包规则为NormalRule的话，在动画不拆分的情况下，FBX会因为交叉使用被直接打包到引用该动画的AB里(方案:提取动画出来单独打包ab，然后设置fbx import animation为false避免冗余动画打包)
            //3. FBX更新后重新导出AnimationClip如何保留原有AnimationClip上的信息(待解决)
            foreach (var asset in assetlist)
            {
                //Note:
                //Editor资源虽然最终不参与打包AB，但需要通过Editor Only的一些资源得出间接引用的非Editor Only资源(e.g. LightData.asset间接引用的光照贴图)
                if (ABHelper.Singleton.isEditorOnlyAsset(asset))
                {
                    continue;
                }
                else if(ABHelper.Singleton.isSpecialAssetNoNeedConsider(asset))
                {
                    continue;
                }
                else
                {
                    if (!buildmap[finalabname].Contains(asset))
                    {
                        buildmap[finalabname].Add(asset);
                    }
                }
            }
        }

        if (isusingbuildinresource)
        {
            //临时允许使用Build In Resource打包
            //return;
        }

        List<AssetBundleBuild> abblist = new List<AssetBundleBuild>();
        foreach (var build in buildmap)
        {
            var abb = new AssetBundleBuild();
            abb.assetBundleName = build.Key;
            abb.assetNames = build.Value.ToArray();
            abblist.Add(abb);
        }

        //如果包含Shader，判定是否有新的shader需要加入打包列表
        var shaderabname = ABHelper.Singleton.ShaderABName + ABHelper.Singleton.CurrentPlatformABPostfix;
        if (buildmap.ContainsKey(shaderabname))
        {
            recordAllNewShaders(buildmap[shaderabname]);
        }

        // 开始打包AB
        // Note: 这里采用LZ4压缩方式打包AB
        BuildPipeline.BuildAssetBundles(ABHelper.Singleton.CurrentPlatformABPath, abblist.ToArray(), ABHelper.Singleton.ABBuildOptions, ABHelper.Singleton.getCurrentPlatformABBuildTarget());

        recordAllABDpInfo();

        PackagAssetInfoBrowserWindow.showWindow(packageassetmap.Values, abblist);

        return true;
    }

    /// <summary>
    /// 得出所有需要参与打包Asset的PackageAsset抽象
    /// </summary>
    /// <param name="asset">需要参与打包的主Asset</param>
    /// <param name="packageassetmap">Asset打包映射map，Key為Asset相對路徑，Value為Asset打包抽象對象</param>
    /// <returns>Asset打包是否成功</returns>
    private static bool getAllPackageAssets(Object asset, ref Dictionary<string, PackageAsset> packageassetmap)
    {
        //Queue來模拟先进先出的广度优先搜索方式，确保最终整理出的Asset的DependentAsset是最里层的Asset
        //KeyValuePair的Key是入队列需要判定的Asset，Value为需要判定的Asset的Dependent Asset
        //这里按广度优先入队列进行判定，确保最终整理出的Asset的DependentAsset是最里层的Asset
        Queue<KeyValuePair<Object, Object>> assetqueue = new Queue<KeyValuePair<Object, Object>>();
        assetqueue.Enqueue(new KeyValuePair<Object, Object>(asset, null));
        while (assetqueue.Count > 0)
        {
            var obj = assetqueue.Dequeue();
            if (!getPackageAssets(obj.Key, ref packageassetmap, ref assetqueue, obj.Value))
            {
                return false;
            }
        }
        return true;
    }

    #if AB_PACKAGE_SYSTEM
    [MenuItem("Tools/Assetbundle/合并依赖文件", false, 109)]
    #endif
    public static void mergeDependencyFiles()
    {
        MergeABDependence(ABHelper.Singleton.CurrentPlatformABPath, ABHelper.Singleton.CurrentPlatformABPath, ABHelper.DependencyFileName);
    }

    /// <summary>
    /// 合并依赖
    /// </summary>
    private static void MergeABDependence(string inputPath, string outPath, string depName)
    {
        if (!Directory.Exists(outPath))
            Directory.CreateDirectory(outPath);

        string temppath = Application.dataPath + "/" + Path.GetFileNameWithoutExtension(depName) + ".bytes";
        //txt仅用于PC查看
        string temppath2 = Application.dataPath + "/" + Path.GetFileNameWithoutExtension(depName) + ".txt";
        if (File.Exists(temppath))
            File.Delete(temppath);

        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        StreamWriter txtwriter = new StreamWriter(temppath2);
        var files = Directory.GetFiles(inputPath, "*.d", SearchOption.AllDirectories);
        if (files.Length == 0)
            return;

        foreach (var file in files)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            var arr = ResDpManager.Singleton.getAssetBundleDpInfo(name);
            if (arr == null)
                continue;
            writer.Write(name);
            writer.Write((System.Int16)arr.Length);
            txtwriter.WriteLine(name);
            foreach (var s in arr)
            {
                writer.Write(s);
                txtwriter.WriteLine("\t" + s);
            }
            txtwriter.WriteLine();
        }
        File.WriteAllBytes(temppath, stream.ToArray());
        stream.Close();
        writer.Close();
        txtwriter.Close();
        AssetDatabase.Refresh();

        AssetBundleBuild abb = new AssetBundleBuild();
        abb.assetBundleName = depName;
        abb.assetNames = new string[] { temppath.Substring(temppath.IndexOf("Assets/")) };
        BuildPipeline.BuildAssetBundles(outPath, new AssetBundleBuild[] { abb }, ABHelper.Singleton.ABBuildOptions, ABHelper.Singleton.getCurrentPlatformABBuildTarget());
        AssetDatabase.Refresh();

        File.Delete(temppath);
        File.Delete(temppath + ".meta");
        DirectoryInfo info = new DirectoryInfo(outPath);
        File.Delete(outPath + info.Name);
        File.Delete(outPath + info.Name + ".manifest");
        File.Delete(outPath + depName + ".manifest");

        Debug.Log("合并依赖文件完成!");
    }

    /// <summary>
    /// 得出所有需要参与打包Asset的PackageAsset抽象
    /// </summary>
    /// <param name="asset">需要参与打包的主Asset</param>
    /// <param name="packageassetmap">Asset打包映射map，Key為Asset相对路径，Value為Asset打包抽象对象</param>
    /// <param name="assetqueue">需要参与获取PackageAsset的Asset队列</param>
    /// <param name="dependentasset">依赖使用参与打包的主Asset的Asset</param>
    /// <returns>Asset打包是否成功</returns>
    private static bool getPackageAssets(Object asset, ref Dictionary<string, PackageAsset> packageassetmap, ref Queue<KeyValuePair<Object, Object>> assetqueue, Object dependentasset = null)
    {
        if (asset == null)
        {
            return true;
        }
        else
        {
            var assetpath = AssetDatabase.GetAssetPath(asset);
            if (!ABHelper.Singleton.isValideAssetFile(assetpath))
            {
                return true;
            }
            else
            {
                if (packageassetmap.ContainsKey(assetpath))
                {
                    //如果包含了，那么就把对应Asset的Dependent Asset修改为更里层的Dependent Asset
                    //确保每一个Asset对应唯一一个PackageAsset且Dependent Asset信息为最里层的信息
                    //e.g. Scene -> M1 + M2   Scene -> P1  P1 -> F1 -> M1
                    //那么对于材质M1而言，最里层DependentAsset引用是F1而非Scene
                    //对于材质M2而言，最里层DepdentAsset引用是Scene
                    var depdendentassetpath = AssetDatabase.GetAssetPath(dependentasset);
                    //if (packageassetmap[assetpath].AssetAssetBundleBuildRule == AssetABBuildRule.E_NORMAL)
                    //{
                    //    var olddppatype = packageassetmap[assetpath].DependentPackageAsset.PackageAssetType;
                    //    var newdppatype = packageassetmap[depdendentassetpath].PackageAssetType;
                    //    var olddependentpaassetpath = packageassetmap[assetpath].DependentPackageAsset.AssetPath;
                    //    var newdppaassetpath = packageassetmap[depdendentassetpath].AssetPath;
                    //    //Normal Asset被不同资源但类型相同引用存在潜在问题
                    //    //e.g. 
                    //    //打包时如果只有Prefab1 -> M1   Prefab2 -> M1会导致M1只跟其中一个打包在一起其中一个Prefab依赖另一个Prefab的AB
                    //    //出现M1打包规则为NormalRule时被多个Prefab引用时，需要确保打包机制得出的最终结论M1不属于Prefab层，而属于其他层比如FBX(EntireRule)
                    //    //所以如果M1不是作为跟随FBX层打包又被多个Prefab使用的话，建议使用ShareRule打包M1
                    //    if (olddppatype == newdppatype && !olddependentpaassetpath.Equals(newdppaassetpath))
                    //    {
                    //        Debug.LogWarning(string.Format("Asset Path: {0}打包規則為NormalRule，但被多個同類型Asset引用:Dp1:{1} Dp2:{2}！", assetpath, olddependentpaassetpath, newdppaassetpath));
                    //        return false;
                    //    }
                    //}
                    // 依赖当前Asset的Asset信息不为空，且最新的依赖当前Asset的Asset资源类型与当前依赖的Asset资源类型一致
                    // 说明有E_NORMAL打包规则的资源被多个同类型资源引用
                    // 这是不允许的，如果资源被多个资源引用，不允许设定E_NORMAL，因为同一个Asset不能指定多个ABName
                    // 建议设置E_SHARE
                    if (packageassetmap[assetpath].AssetAssetBundleBuildRule == AssetABBuildRule.E_NORMAL
                        && packageassetmap[assetpath].DependentPackageAsset != null
                        && packageassetmap[assetpath].DependentPackageAsset.PackageAssetType == packageassetmap[depdendentassetpath].PackageAssetType)
                    {
                        Debug.Log(string.Format("资源：{0} 打包规则E_NORMAL，被多个同类型资源引用 : 资源1 : {1} 资源2 : {2}，建议设置成E_SHARE打包规则！",
                                  assetpath, packageassetmap[assetpath].DependentPackageAsset.AssetPath, packageassetmap[depdendentassetpath].AssetPath));
                        return false;
                    }
                    else
                    {
                        packageassetmap[assetpath].DependentPackageAsset = packageassetmap[depdendentassetpath];
                    }
                }
                else
                {
                    var packageasset = PackageAsset.createPackageAsset(asset, dependentasset);
                    if (packageasset != null)
                    {
                        packageassetmap.Add(assetpath, packageasset);
                        // 将依赖使用当前Asset的PackageAsset信息指向同一个PackageAsset
                        var dpassetpath = AssetDatabase.GetAssetPath(dependentasset);
                        if (packageassetmap.ContainsKey(dpassetpath))
                        {
                            packageassetmap[assetpath].DependentPackageAsset = packageassetmap[dpassetpath];
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("Asset Path創建PackageAsset失敗!Asset Path: {0}", assetpath));
                        return false;
                    }
                }

                //这里的false参数是为了一层一层取出Asset的依赖关系，然后整理出Asset的层级关系，确保每一个Asset只对应一个最终的PackageAsset
                var dps = AssetDatabase.GetDependencies(assetpath, false);
                foreach (var dp in dps)
                {
                    if (!ABHelper.Singleton.isValideAssetFile(dp))
                    {
                        continue;
                    }
                    var dpasset = AssetDatabase.LoadAssetAtPath<Object>(dp);
                    assetqueue.Enqueue(new KeyValuePair<Object, Object>(dpasset, asset));
                }
                return true;
            }
        }
    }

    /// <summary>
    /// 记录本次打包所有的AB依赖信息
    /// 通过把每一个AB缩写一个同名的*.d文件来记录AB依赖关系
    /// </summary>
    private static void recordAllABDpInfo()
    {
        var manifestfile = AssetBundle.LoadFromFile(ABHelper.Singleton.CurrentPlatformABManifestFilePath);
        AssetBundleManifest manifest = manifestfile.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        if(manifest == null)
        {
            Debug.LogError("加载AssetBundleManifest文件失敗!");
            return;
        }
        else
        {
            var allassetbundle = manifest.GetAllAssetBundles();
            foreach(var assetbundle in allassetbundle)
            {
                var dpassetbundles = manifest.GetDirectDependencies(assetbundle);
                ABHelper.Singleton.writeAssetBundleDpToFile(assetbundle, dpassetbundles);
            }
        }
    }

    /// <summary>
    /// 记录所有新用到的Shader信息
    /// </summary>
    /// <param name="assetslist"></param>
    private static void recordAllNewShaders(List<string> assetslist)
    {
        foreach (var assetpath in assetslist)
        {
            Debug.Log(string.Format("Record Shader : {0}", assetpath));
            ABHelper.Singleton.addShaderToBuildList(assetpath);
        }
    }
}
