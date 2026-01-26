/*
 * Description:             AssetBundle打包工具
 * Author:                  TonyTang
 * Create Date:             2023/01/23
 */
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using TResource;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Utilities;
using UnityEditor.Build.Content;
using UnityEditor.SceneManagement;

namespace TResource
{
	/// <summary>
	/// AB打包工具
	/// </summary>
	public class AssetBundleBuilder
	{
		/// <summary>
		/// AssetBundle压缩选项
		/// </summary>
		public enum ECompressOption
		{
			Uncompressed = 0,
			StandardCompressionLZMA,
			ChunkBasedCompressionLZ4,
		}

		/// <summary>
		/// 输出的根目录
		/// </summary>
		private readonly string _outputRoot;

		/// <summary>
		/// 构建平台
		/// </summary>
		public BuildTarget BuildTarget { private set; get; } = BuildTarget.NoTarget;

		/// <summary>
        /// 构建平台组
        /// </summary>
		public BuildTargetGroup BuildTargetGroup { private set; get; } = BuildTargetGroup.Unknown;

		/// <summary>
		/// 输出目录
		/// </summary>
		public string OutputDirectory { private set; get; } = string.Empty;

		/// <summary>
		/// 构建选项
		/// </summary>
		public ECompressOption CompressOption = ECompressOption.Uncompressed;

		/// <summary>
		/// 是否强制重新打包资源
		/// </summary>
		public bool IsForceRebuild = false;

		/// <summary>
		/// 
		/// </summary>
		public bool IsAppendHash = false;

		/// <summary>
		/// 
		/// </summary>
		public bool IsDisableWriteTypeTree = false;

		/// <summary>
		/// 
		/// </summary>
		public bool IsIgnoreTypeTreeChanges = false;


		/// <summary>
		/// 所有AssetBundle打包信息Map<AssetBundle名, <AssetBundle变体名, AssetBundle打包信息>>(避免相同AssetBundle打包信息重复New AssetBundleBuildInfo)
        /// Note:
        /// 1. 仅包含参打包的AssetBundle信息(视频单独打包不包含在内)
		/// </summary>
		private Dictionary<string, Dictionary<string, AssetBundleBuildInfo>> mAssetBundleBuildInfoMap = new Dictionary<string, Dictionary<string, AssetBundleBuildInfo>>();

		/// <summary>
		/// 所有Asset打包信息Map<Asset路径, Asset信息>(避免相同Asset重复New AssetBuildInfo)
        /// Note:
        /// 1. 仅包含参与打包的Asset打包信息
		/// </summary>
		private Dictionary<string, AssetBuildInfo> mAllAssetBuildInfoMap = new Dictionary<string, AssetBuildInfo>();

		/// <summary>
        /// 所有Asset的AB名信息缓存Map<Asset路径, AB名>
        /// </summary>
		private Dictionary<string, string> mAllAssetBundleNameCacheMap = new Dictionary<string, string>();

		/// <summary>
		/// 所有Asset的AB变体名信息缓存Map<Asset路径, AB变体名>
		/// </summary>
		private Dictionary<string, string> mAllAssetBundleVariantNameCacheMap = new Dictionary<string, string>();

		/// <summary>
		/// 所有AssetBundle打包列表
		/// </summary>
		private List<AssetBundleBuild> mAllAssetBundleBuildList = new List<AssetBundleBuild>();

		/// <summary>
		/// 所有AssetBundle打包信息列表
		/// </summary>
		private List<AssetBundleBuildInfo> mAllAssetBundleBuildInfoList = new List<AssetBundleBuildInfo>();

		/// <summary>
		/// AB MD5的文件名黑名单
		/// </summary>
		private List<string> mAllMD5FileNameBlackList = new List<string>()
		{
			AssetBundleBuildConstData.ReadmeFileName,
			AssetBundleBuildConstData.AssetBuildReadmeFileName,
		};

		/// <summary>
		/// AssetBuilder
		/// </summary>
		/// <param name="buildTarget">构建平台</param>
		public AssetBundleBuilder(BuildTarget buildTarget)
		{
			_outputRoot = AssetBundleBuilderHelper.GetOutputRootPath();
			BuildTarget = buildTarget;
			BuildTargetGroup = BuildPipeline.GetBuildTargetGroup(BuildTarget);
			OutputDirectory = AssetBundleBuilderHelper.GetBuildTargetOutputRootPath(BuildTarget);
		}

		/// <summary>
		/// 准备构建
		/// </summary>
		public void PreAssetBuild()
		{
			Debug.Log("------------------------------OnPreAssetBuild------------------------------");

			// 检测构建平台是否合法
			if (BuildTarget == BuildTarget.NoTarget)
            {
                throw new Exception("[BuildPatch] 请选择目标平台");
            }

            // 检测构建版本是否合法
            //if (EditorUtilities.IsNumber(BuildVersion.ToString()) == false)
            //	throw new Exception($"[BuildPatch] 版本号格式非法：{BuildVersion}");
            //if (BuildVersion < 0)
            //	throw new Exception("[BuildPatch] 请先设置版本号");

            // 检测输出目录是否为空
            if (string.IsNullOrEmpty(OutputDirectory))
            {
                throw new Exception("[BuildPatch] 输出目录不能为空");
            }

            // 检测补丁包是否已经存在
            //string packageDirectory = GetPackageDirectory();
            //if (Directory.Exists(packageDirectory))
            //	throw new Exception($"[BuildPatch] 补丁包已经存在：{packageDirectory}");

            // 如果是强制重建
            if (IsForceRebuild)
			{
				// 删除平台总目录
				string platformDirectory = $"{_outputRoot}/{BuildTarget}";
				if (Directory.Exists(platformDirectory))
				{
					Directory.Delete(platformDirectory, true);
					Log($"删除平台总目录：{platformDirectory}");
				}
			}

			// 如果输出目录不存在
			if (Directory.Exists(OutputDirectory) == false)
			{
				Directory.CreateDirectory(OutputDirectory);
				Log($"创建输出目录：{OutputDirectory}");
			}

			// Asset打包信息输出目录不存在
			var assetbuildinfofolderpath = AssetBundlePath.GetAssetBuildInfoFolderFullPath();
			Debug.Log($"Asset打包信息输出目录:{assetbuildinfofolderpath}");
			if (!Directory.Exists(assetbuildinfofolderpath))
			{
				Directory.CreateDirectory(assetbuildinfofolderpath);
				Log($"创建打包信息Asset输出目录：{assetbuildinfofolderpath}");
			}
		}

		/// <summary>
		/// 执行构建
		/// </summary>
		public void PostAssetBuild()
		{
			Debug.Log("------------------------------OnPostAssetBuild------------------------------");
			// 准备工作
			DoAssetBundleBuildPreparation();
			// 开始构建
			Log($"开始构建......");
			bool buildSuccess;
#if OLD_ASSET_BUILD_PIPELINE
			DoCustomAssetBundleBuild(OutputDirectory, out buildSuccess);
#else
			DoSBPAssetBundleBuild(OutputDirectory, out buildSuccess);
#endif
			if(buildSuccess == false)
            {
				Debug.LogError($"打包AB失败!");
				return;
            }

			// 视频单独打包
			//PackVideo(buildAssetInfoList);
			// 单独生成包内的AssetBundle的MD5信息(用于热更新判定)
			CreateAssetBundleMd5InfoFile();

			Log("构建完成！");
		}

		/// <summary>
		/// 执行新版Scriptable Build Pipeline AB打包
		/// </summary>
		/// <param name="outputDirectory"></param>
		/// <param name="buildSuccess"></param>
		private void DoSBPAssetBundleBuild(string outputDirectory, out bool buildSuccess)
        {
            var buildParams = MakeBuildParameters();
			IBundleBuildResults results;
			SBPAssetBundleBuilder.BuildAllAssetBundles(this, outputDirectory, BuildTarget, buildParams, mAllAssetBundleBuildList, out buildSuccess, out results);
			CreateSBPReadmeFile(outputDirectory, results);
		}

        /// <summary>
        /// 执行老版自定义AB打包
        /// </summary>
        /// <param name="outputDirectory"></param>
        /// <param name="buildSuccess"></param>
        private void DoCustomAssetBundleBuild(string outputDirectory, out bool buildSuccess)
        {
			BuildAssetBundleOptions options = MakeBuildOptions();
			AssetBundleManifest unityManifest = OldAssetBundleBuilder.BuildAllAssetBundles(this, outputDirectory, BuildTarget, options, mAllAssetBundleBuildList, out buildSuccess);
			// 创建说明文件
			CreateReadmeFile(outputDirectory, unityManifest);
		}

		/// <summary>
		/// 执行AssetBundle打包准备工作
		/// </summary>
		private bool DoAssetBundleBuildPreparation()
        {
            ResetBuildDatas();
			AssetBundleCollectSettingData.ClearLoadSettingData();
			AssetBundleCollectSettingData.LoadSettingData();
			if (!AssetBundleCollectSettingData.CheckCollectorSettingValidation())
            {
                return false;
            }
			EditorSceneManager.SaveOpenScenes();
			if(!DoAnalyseAssetBundleBuild())
            {
				return false;
            }
            // 创建Asset AB打包详细说明信息文件
			CreateAssetBuildReadmeFile();
			return true;
        }

		/// <summary>
		/// 更新AssetBundle打包编译信息Asset
		/// </summary>
		/// <param name="assetBundleBuildList"></param>
		private void UpdateAssetBundleBuildInfoAsset(List<AssetBundleBuild> assetBundleBuildList)
		{
			// Note: AssetBundle打包信息统一存小写，确保和AB打包那方一致
			var assetbundlebuildinfoassetrelativepath = AssetBundlePath.GetAssetBuildInfoFileRelativePath();
			var assetbundlebuildasset = AssetDatabase.LoadAssetAtPath<AssetBuildInfoAsset>(assetbundlebuildinfoassetrelativepath);
			if (assetbundlebuildasset == null)
			{
				assetbundlebuildasset = new AssetBuildInfoAsset();
				AssetDatabase.CreateAsset(assetbundlebuildasset, assetbundlebuildinfoassetrelativepath);
			}
			assetbundlebuildasset.BuildAssetInfoList.Clear();

			// Asset打包信息构建
			foreach (var bi in assetBundleBuildList)
			{
				foreach (var assetName in bi.assetNames)
				{
					// 不剔除后缀，确保AssetDatabase模式可以全路径(带后缀)加载
					var assetPath = assetName;
					// Note:
					// 1. 记录的Asset对应AB路径要不带AB后缀，AB后缀在运行加载时根据平台自动添加
					var assetBundleNameNoPostFix = PathUtilities.GetPathWithoutPostFix(bi.assetBundleName);
					var buildAssetInfo = new BuildAssetInfo(assetPath, assetBundleNameNoPostFix, bi.assetBundleVariant);
					assetbundlebuildasset.BuildAssetInfoList.Add(buildAssetInfo);
				}
			}

			EditorUtility.SetDirty(assetbundlebuildasset);
			AssetDatabase.SaveAssets();
		}

		private void Log(string log)
		{
			Debug.Log($"[BuildPatch] {log}");
		}

		/// <summary>
		/// 获取指定AssetBundle名和变体名的AssetBundle打包信息
		/// </summary>
		/// <param name="assetBundleLable"></param>
		/// <returns></returns>
		private AssetBundleBuildInfo GetAssetBundleBuildInfo(string assetBundleLable, string assetBundleVariant)
		{
			Dictionary<string, AssetBundleBuildInfo> assetBundleVariantInfoMap;
			if (!mAssetBundleBuildInfoMap.TryGetValue(assetBundleLable, out assetBundleVariantInfoMap))
			{
				return null;
			}
			AssetBundleBuildInfo asestBundleBuildInfo; 
			if (!assetBundleVariantInfoMap.TryGetValue(assetBundleVariant, out asestBundleBuildInfo))
			{
				return null;
			}
			return asestBundleBuildInfo;
		}

		/// <summary>
		/// 添加AssetBundle打包信息
		/// </summary>
		/// <param name="assetBundleBuildInfo"></param>
		/// <returns></returns>
		private bool AddAssetBundleBuildInfo(AssetBundleBuildInfo assetBundleBuildInfo)
		{
			Dictionary<string, AssetBundleBuildInfo> assetBundleVariantMap;
			if (!mAssetBundleBuildInfoMap.TryGetValue(assetBundleBuildInfo.AssetBundleName, out assetBundleVariantMap))
			{
				assetBundleVariantMap = new Dictionary<string, AssetBundleBuildInfo>();
				mAssetBundleBuildInfoMap.Add(assetBundleBuildInfo.AssetBundleName, assetBundleVariantMap);
			}
			if(assetBundleVariantMap.ContainsKey(assetBundleBuildInfo.AssetBundleVariant))
            {
				Debug.LogError($"重复添加AssetBundle:{assetBundleBuildInfo.AssetBundleName} AssetBundleVariant:{assetBundleBuildInfo.AssetBundleVariant}的AssetBundle打包信息，添加失败，请检查代码!");
				return false;
			}
			assetBundleVariantMap.Add(assetBundleBuildInfo.AssetBundleVariant, assetBundleBuildInfo);
			return true;
		}

		/// <summary>
		/// 获取指定Asset路径的Asset打包信息
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		private AssetBuildInfo GetAssetBuildInfo(string assetPath)
		{
			AssetBuildInfo assetBuildInfo;
			if (!mAllAssetBuildInfoMap.TryGetValue(assetPath, out assetBuildInfo))
			{
				return null;
			}
			return assetBuildInfo;
		}

		/// <summary>
		/// 添加指定Asset打包信息
		/// </summary>
		/// <param name="assetBuildInfo"></param>
		/// <returns></returns>
		private bool AddAssetBuildInfo(AssetBuildInfo assetBuildInfo)
		{
			if (mAllAssetBuildInfoMap.ContainsKey(assetBuildInfo.AssetPath))
			{
				Debug.LogError($"重复添加Asset路径:{assetBuildInfo.AssetPath}的Asset打包信息，添加失败，请检查代码!");
				return false;
			}
			mAllAssetBuildInfoMap.Add(assetBuildInfo.AssetPath, assetBuildInfo);
			return true;
		}

#region 准备工作
		/// <summary>
		/// 执行AssetBundle打包分析
		/// </summary>
		private bool DoAnalyseAssetBundleBuild()
		{
            // 获取所有的收集路径
            List<string> collectDirectorys = AssetBundleCollectSettingData.GetAllCollectDirectory();
            if (collectDirectorys.Count == 0)
            {
                Debug.LogWarning("[BuildPatch] 配置的资源收集路径为空");
				return false;
            }
            int progressBarCount = 0;
            // 获取所有资源
            string[] guids = AssetDatabase.FindAssets(string.Empty, collectDirectorys.ToArray());
            foreach (string guid in guids)
            {
                string mainAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                string regularMainAssetPath = PathUtilities.GetRegularPath(mainAssetPath);
                UpdateAssetAllAssetInfo(regularMainAssetPath);
                // 进度条
                progressBarCount++;
                EditorUtility.DisplayProgressBar("进度", $"依赖文件分析：{progressBarCount}/{guids.Length}", (float)progressBarCount / guids.Length);
            }
            EditorUtility.ClearProgressBar();

			UpdateAssetBundleBuildInfoAssetDatas();
			UpdateAssetBundleBuildDatas();

            int totalAssetBuildNum = 0;
			foreach (var assetBundleBuildInfo in mAllAssetBundleBuildInfoList)
			{
				totalAssetBuildNum += assetBundleBuildInfo.GetTotalAssetBuildNum();
			}
			Log($"构建列表里总共有{mAllAssetBundleBuildInfoList.Count}个AB需要打包，总共有:{totalAssetBuildNum}个Asset资源要参与打包");
			return true;
		}

		/// <summary>
		/// 更新AssetBundle打包信息Asset数据
		/// </summary>
		private void UpdateAssetBundleBuildInfoAssetDatas()
        {
            // AssetBuildInfoAsset打包信息单独打包
            var assetBuildInfoAssetRelativePath = AssetBundlePath.GetAssetBuildInfoFileRelativePath();
            var assetBundleName = GetAssetBuildInfoAssetBundleName();
            var assetBundleVariant = GetAssetBuildBundleVariant(assetBuildInfoAssetRelativePath);
			var assetBuildCompression = GetAssetBuildCompression(assetBuildInfoAssetRelativePath);
            var assetBundleBuildInfo = new AssetBundleBuildInfo(assetBundleName, assetBundleVariant, assetBuildCompression);
            var addresableName = GetAssetAddresableName(assetBuildInfoAssetRelativePath);
			var assetBuildInfo = new AssetBuildInfo(assetBuildInfoAssetRelativePath, addresableName);
			AddAssetBuildInfo(assetBuildInfo);
			assetBundleBuildInfo.AddAssetBuildInfo(assetBuildInfo);
			AddAssetBundleBuildInfo(assetBundleBuildInfo);
        }

		/// <summary>
		/// 更新AssetBundle打包数据
		/// </summary>
		private void UpdateAssetBundleBuildDatas()
        {
			foreach(var assetBundleBuildInfos in mAssetBundleBuildInfoMap)
            {
				foreach(var assetBundleBuildInfo in assetBundleBuildInfos.Value)
                {
                    mAllAssetBundleBuildInfoList.Add(assetBundleBuildInfo.Value);
                }
            }

			foreach(var assetBundleBuildInfo in mAllAssetBundleBuildInfoList)
            {
				var assetBundleBuild = new AssetBundleBuild();
				assetBundleBuild.assetBundleName = assetBundleBuildInfo.AssetBundleName;
				assetBundleBuild.assetBundleVariant = assetBundleBuildInfo.AssetBundleVariant;
				// AssetBundle打包限制必须Asset全路径
				assetBundleBuild.assetNames = assetBundleBuildInfo.GetAllAssetPaths();
				assetBundleBuild.addressableNames = assetBundleBuildInfo.GetAllAddresableNames();
				mAllAssetBundleBuildList.Add(assetBundleBuild);
			}

            // 更新AB打包信息Asset(e.g.比如Asset打包信息)
            UpdateAssetBundleBuildInfoAsset(mAllAssetBundleBuildList);
        }

        /// <summary>
        /// 重置打包数据
        /// </summary>
        private void ResetBuildDatas()
        {
			mAllAssetBundleNameCacheMap.Clear();
			mAllAssetBundleVariantNameCacheMap.Clear();
			mAssetBundleBuildInfoMap.Clear();
			mAllAssetBuildInfoMap.Clear();
			mAllAssetBundleBuildList.Clear();
			mAllAssetBundleBuildInfoList.Clear();
		}

		/// <summary>
		/// 更新指定资源的所有Asset信息
		/// </summary>
		private void UpdateAssetAllAssetInfo(string assetPath)
		{
			var regularAssetPath = PathUtilities.GetRegularPath(assetPath);
			// Note:
            // 1. 视频参与一起打包，未来有需求或问题再拆分打包
			if (IsValidateCollectAsset(regularAssetPath))
			{
                var assetInfo = GetAssetBuildInfo(regularAssetPath);
                if (assetInfo == null)
                {
					var assetAddreableName = GetAssetAddresableName(regularAssetPath);
                    assetInfo = new AssetBuildInfo(regularAssetPath, assetAddreableName);
                    AddAssetBuildInfo(assetInfo);
                }
                var assetBundleName = GetAssetBundleNameByCache(regularAssetPath);
				var assetBundleVariant = GetAssetBuildBundleVariant(regularAssetPath);
				var assetBundleBuildInfo = GetAssetBundleBuildInfo(assetBundleName, assetBundleVariant);
				if(assetBundleBuildInfo == null)
				{
					var compression = GetAssetBuildCompression(regularAssetPath);
					assetBundleBuildInfo = new AssetBundleBuildInfo(assetBundleName, assetBundleVariant, compression);
					AddAssetBundleBuildInfo(assetBundleBuildInfo);
				}
				assetBundleBuildInfo.AddAssetBuildInfo(assetInfo);
			}
		}

		/// <summary>
		/// 获取指定Asset路径在AB里的访问名
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		private string GetAssetAddresableName(string assetPath)
        {
            // TODO: 未来支持不同策略Asset AB名策略配置
            /*
            var assetCollector = AssetBundleCollectSettingData.GetCollectorByAssetPath(assetPath);
			if(assetCollector.AddresableNameType == ?)
			{
				return ?;
			}
			*/
#if !OLD_ASSET_BUILD_PIPELINE
			// 新版支持准确的大小写AddreableName
            return assetPath;
#else
            // 老版BuildPipeline.BuildAssetBundles打包指定AssetBundleBuild.assetNames为含大写
            // 但不知道为什么打包出来的AB里面的加载路径依然是全小写,所以这里强制老版AB打包AddresableName全小写
            return assetPath.ToLower();
#endif
		}

		/// <summary>
		/// 获取AB打包后缀名
		/// </summary>
		/// <returns></returns>
		private string GetBuildAssetBundlePostFix()
		{
			if (BuildTarget == BuildTarget.StandaloneWindows || BuildTarget == BuildTarget.StandaloneWindows64)
			{
				return AssetBundlePath.WindowAssetBundlePostFix;
			}
			if (BuildTarget == BuildTarget.Android)
			{
				return AssetBundlePath.AndroidAssetBundlePostFix;
			}
			if (BuildTarget == BuildTarget.iOS)
			{
				return AssetBundlePath.IOSAssetBundlePostFix;
			}
			else
			{
				Debug.LogError($"不支持的打包平台:{BuildTarget},获取AB后缀名失败!");
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取Asset打包信息文件AB名
		/// </summary>
		private string GetAssetBuildInfoAssetBundleName()
		{
			var assetBuildInfoRelativePath = AssetBundlePath.GetAssetBuildInfoFileRelativePath();
			var assetBuildInfoABPath = AssetBundlePath.ChangeAssetPathToABPath(assetBuildInfoRelativePath);
			assetBuildInfoABPath = AssetBundlePath.GetABPathWithPostFix(assetBuildInfoABPath);
			return PathUtilities.GetRegularPath(assetBuildInfoABPath.ToLower());
		}

		/// <summary>
        /// 指定Asset路径是否是有效可搜集资源
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
		private bool IsValidateCollectAsset(string assetPath)
        {
			if(!IsValidateAsset(assetPath))
            {
				return false;
            }
			if (!AssetBundleCollectSettingData.IsCollectAsset(assetPath))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// 检测资源是否有效
		/// </summary>
		private bool IsValidateAsset(string assetPath)
		{
			if (!assetPath.StartsWith("Assets/"))
			{
				return false;
			}
			if (AssetDatabase.IsValidFolder(assetPath))
			{
				return false;
			}
			string ext = System.IO.Path.GetExtension(assetPath);
			if (AssetBundleCollectSettingData.Setting.BlackListInfo.IsBlackPostFix(ext))
			{
				return false;
			}
			string fileName = Path.GetFileName(assetPath);
			if (AssetBundleCollectSettingData.Setting.BlackListInfo.IsBlackFileName(fileName))
			{
				return false;
			}
			return true;
		}

		/// <summary>
		/// 获取指定Asset路径的AB名(不带缓存)
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		public static string GetAssetBundleName(string assetPath)
		{
			string assetBundleName = AssetBundleCollectSettingData.GetAssetBundleName(assetPath);
			assetBundleName = AssetBundlePath.GetABPathWithPostFix(assetBundleName);
			return assetBundleName;
		}

		/// <summary>
		/// 获取指定Asset路径的AB名(带缓存)
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		private string GetAssetBundleNameByCache(string assetPath)
		{
			string assetBundleName;
			if (mAllAssetBundleNameCacheMap.TryGetValue(assetPath, out assetBundleName))
			{
				return assetBundleName;
			}
			assetBundleName = AssetBundleCollectSettingData.GetAssetBundleName(assetPath);
			assetBundleName = AssetBundlePath.GetABPathWithPostFix(assetBundleName);
			mAllAssetBundleNameCacheMap.Add(assetPath, assetBundleName);
			return assetBundleName;
		}

        /// <summary>
        /// 获取指定Asset路径的AB变体名
        /// Note:
        /// 1. 因为Scriptable Build Pipeline不支持变体功能，所以这里统一不启用变体功能，改为AB名自带后缀的方式
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public string GetAssetBuildBundleVariant(string assetPath)
        {
			string asestBundleVariantName;
			if (mAllAssetBundleVariantNameCacheMap.TryGetValue(assetPath, out asestBundleVariantName))
			{
				return asestBundleVariantName;
			}
			asestBundleVariantName = string.Empty;
			mAllAssetBundleVariantNameCacheMap.Add(assetPath, asestBundleVariantName);
			return asestBundleVariantName;
        }

		/// <summary>
		/// 获取指定Asset路径的压缩格式
		/// </summary>
		/// <param name="assetPath"></param>
		/// <returns></returns>
		private UnityEngine.BuildCompression GetAssetBuildCompression(string assetPath)
        {
			var collector = AssetBundleCollectSettingData.GetCollectorByAssetPath(assetPath);
			if(collector.BuildRule != AssetBundleBuildRule.Ignore)
			{
				if(collector.Compression == UnityEngine.CompressionType.Lz4 || collector.Compression == UnityEngine.CompressionType.Lz4HC)
	            {
					return UnityEngine.BuildCompression.LZ4;
	            }
				else if(collector.Compression == UnityEngine.CompressionType.Lzma)
	            {
					return UnityEngine.BuildCompression.LZMA;
	            }
				else if(collector.Compression == UnityEngine.CompressionType.None)
	            {
					return UnityEngine.BuildCompression.Uncompressed;
	            }
	            else
	            {
					Debug.LogError($"不支持的压缩格式设置:{collector.Compression}，返回默认LZ4压缩格式!");
					return UnityEngine.BuildCompression.LZ4;
				}
			}
			return GetConfigBuildCompression();
		}
		#endregion

		#region AssetBundle资源热更新相关
		/// <summary>
		/// 创建AssetBundle的MD5信息文件
		/// </summary>
		private void CreateAssetBundleMd5InfoFile()
		{
			var assetBundleMd5FilePath = AssetBundlePath.GetInnerAssetBundleMd5FilePath();
			// 确保创建最新的
			FileUtilities.DeleteFile(assetBundleMd5FilePath);
			VersionConfigModuleManager.Singleton.initVerisonConfigData();
			var resourceversion = VersionConfigModuleManager.Singleton.InnerGameVersionConfig.ResourceVersionCode;
			// Note: 
			// 这里如果直接指定Encoding.UTF8会出现BOM文件(默认选择了带BOM的方式)
			// 最终导致热更新文件读取比较后的路径信息带了BOM导致https识别时报错导致下载不到正确的资源
			using (var md5SW = new StreamWriter(assetBundleMd5FilePath, false, new UTF8Encoding(false)))
			{
				var abFilesFullPath = Directory.GetFiles(OutputDirectory, "*.*", SearchOption.AllDirectories).Where(f =>
					!f.EndsWith(".meta") && !f.EndsWith(".manifest"));
				var md5hash = MD5.Create();
				// 格式:AB全路径+":"+MD5值
				foreach (var abFilePath in abFilesFullPath)
				{
					var abFileName = Path.GetFileName(abFilePath);
					if(mAllMD5FileNameBlackList.Contains(abFileName))
                    {
						continue;
                    }
					var abRelativePath = abFilePath.Remove(0, OutputDirectory.Length);
					abRelativePath = PathUtilities.GetRegularPath(abRelativePath);
					var fileMd5 = FileUtilities.GetFileMD5(abFilePath, md5hash);
					md5SW.WriteLine($"{abRelativePath}{ResourceConstData.AssetBundlleInfoSeparater}{fileMd5}");
				}
				Debug.Log($"AssetBundle的包内MD5信息计算完毕!");
			}
		}
#endregion

#region 视频相关
		/// <summary>
        /// 视频单独打包
        /// </summary>
        /// <param name="assetBuildInfoList"></param>
		private void PackVideo(List<AssetBuildInfo> assetBuildInfoList)
		{
			// 未来有单独打包视频需求再说
		}
        #endregion

        #region 文件加密

        #endregion

        #region 文件相关
        /// <summary>
        /// 创建Asset AB打包详细Readme文件到输出目录
        /// </summary>
        /// <param name="assetBundleBuildList">Asset AB打包信息列表</param>
        private void CreateAssetBuildReadmeFile()
        {
            // 删除旧文件
            string filePath = $"{OutputDirectory}/{AssetBundleBuildConstData.AssetBuildReadmeFileName}";
            if (File.Exists(filePath))
                File.Delete(filePath);

            Log($"创建Asset AB打包详细说明文件：{filePath}");

            StringBuilder content = new StringBuilder();
            AppendData(content, $"构建平台：{BuildTarget}");
            AppendData(content, $"构建时间：{DateTime.Now}");

            AppendData(content, "");
            AppendData(content, $"--Asset AB打包信息--");
            for (int i = 0, length = mAllAssetBundleBuildList.Count; i < length; i++)
            {
                var assetBundleBuild = mAllAssetBundleBuildList[i];
				var assetBundleBuildInfo = mAllAssetBundleBuildInfoList[i];
				AppendData(content, $"AssetBundleName:{assetBundleBuild.assetBundleName} AssetBundleVariant:{assetBundleBuild.assetBundleVariant} BuildCompression:{assetBundleBuildInfo.Compression.compression}");
                foreach (var assetPath in assetBundleBuild.assetNames)
                {
                    AppendData(content, $"\tAssetPath: {assetPath}");
                }
                AppendData(content, "");
            }
            // 创建新文件
            File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
        }

		/// <summary>
		/// 添加打包平台和时间内容
		/// </summary>
		/// <param name="content"></param>
		private void AppendBuildTargetAndTimeContent(StringBuilder content)
        {
            AppendData(content, $"构建平台：{BuildTarget}");
            AppendData(content, $"构建时间：{DateTime.Now}");
            AppendData(content, "");
        }

		/// <summary>
		/// 添加收集器配置内容
		/// </summary>
		/// <param name="content"></param>
		private void AppendCollectorContent(StringBuilder content)
        {
            AppendData(content, $"--配置信息--");
            for (int i = 0; i < AssetBundleCollectSettingData.Setting.AssetBundleCollectors.Count; i++)
            {
                Collector wrapper = AssetBundleCollectSettingData.Setting.AssetBundleCollectors[i];
                if (wrapper.BuildRule != AssetBundleBuildRule.ByConstName)
                {
                    AppendData(content, $"Directory : {wrapper.CollectFolderPath} || CollectRule : {wrapper.CollectRule} || BuildRule : {wrapper.BuildRule}");
                }
                else
                {
                    AppendData(content, $"Directory : {wrapper.CollectFolderPath} || CollectRule : {wrapper.CollectRule} || BuildRule : {wrapper.BuildRule} || ConstName : {wrapper.ConstName}");
                }
            }
            AppendData(content, "");
        }

		/// <summary>
		/// 添加打包参数配置内容
		/// </summary>
		/// <param name="content"></param>
		private void AppendBuildParametersContent(StringBuilder content)
        {
            AppendData(content, $"--构建参数--");
            AppendData(content, $"CompressOption：{CompressOption}");
            AppendData(content, $"ForceRebuild：{IsForceRebuild}");
            AppendData(content, $"DisableWriteTypeTree：{IsDisableWriteTypeTree}");
            AppendData(content, $"IgnoreTypeTreeChanges：{IsIgnoreTypeTreeChanges}");
            AppendData(content, "");
        }
		#endregion

		#region 新版SBP打包相关
		/// <summary>
		/// 获取配置的压缩格式
		/// </summary>
		/// <returns></returns>
		private UnityEngine.BuildCompression GetConfigBuildCompression()
        {
			if (CompressOption == ECompressOption.Uncompressed)
			{
				return UnityEngine.BuildCompression.Uncompressed;
			}
			else if (CompressOption == ECompressOption.ChunkBasedCompressionLZ4)
			{
				return UnityEngine.BuildCompression.LZ4;
			}
			else
			{
				return UnityEngine.BuildCompression.LZMA;
			}
		}

		/// <summary>
		/// 获取构建参数
		/// </summary>
		private CustomBuildParameters MakeBuildParameters()
        {
			CustomBuildParameters bundleBuildParameters = new CustomBuildParameters(BuildTarget, BuildTargetGroup, OutputDirectory);
			//bundleBuildParameters.CacheServerHost = "";
			//bundleBuildParameters.CacheServerPort = ;
            bundleBuildParameters.BundleCompression = GetConfigBuildCompression();
            if (IsForceRebuild)
            {
                // 是否增量打包
                bundleBuildParameters.UseCache = !IsForceRebuild;
            }
            bundleBuildParameters.ContiguousBundles = true;
            if (IsAppendHash)
            {
                bundleBuildParameters.AppendHash = IsAppendHash;
            }
            if (IsDisableWriteTypeTree)
            {
                bundleBuildParameters.ContentBuildFlags |= ContentBuildFlags.DisableWriteTypeTree;
            }
			bundleBuildParameters.ContentBuildFlags |= ContentBuildFlags.StripUnityVersion;
			if (IsIgnoreTypeTreeChanges)
            {
                // SBP不支持BuildAssetBundleOptions.IgnoreTypeTreeChanges
            }
			// 添加自定义AB压缩格式设置
			foreach(var assetBundleBuildInfo in mAllAssetBundleBuildInfoList)
            {
				bundleBuildParameters.AddAssetBundleCompression(assetBundleBuildInfo.AssetBundleName, assetBundleBuildInfo.Compression);
			}
            return bundleBuildParameters;
        }

		/// <summary>
		/// 创建ScriptableBuildPipeline Readme文件到输出目录
		/// </summary>
		/// <param name="outputDirectory">输出目录</param>
		/// <param name="bundleBuildResults">打包结果</param>
		private void CreateSBPReadmeFile(string outputDirectory, IBundleBuildResults bundleBuildResults)
		{
			// 删除旧文件
			string filePath = $"{outputDirectory}/{AssetBundleBuildConstData.ReadmeFileName}";
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}

			Debug.Log($"创建说明文件：{filePath}");

			StringBuilder content = new StringBuilder();
			AppendBuildTargetAndTimeContent(content);
			AppendCollectorContent(content);
			AppendBuildParametersContent(content);
			AppendData(content, $"--构建清单--");
			foreach (var bundleBuildInfos in bundleBuildResults.BundleInfos)
			{
				var bundleBuildInfo = bundleBuildInfos.Value;
				AppendData(content, bundleBuildInfo.FileName);
			}

			// 创建新文件
			File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
		}
		#endregion

		#region 老版自定义打包相关
		/// <summary>
		/// 获取配置的AB压缩格式
		/// </summary>
		/// <returns></returns>
		private BuildAssetBundleOptions GetConfigBuildCompressionOption()
        {
			if (CompressOption == ECompressOption.Uncompressed)
			{
				return BuildAssetBundleOptions.UncompressedAssetBundle;
			}
			else if (CompressOption == ECompressOption.ChunkBasedCompressionLZ4)
			{
				return BuildAssetBundleOptions.ChunkBasedCompression;
			}
            else
            {
				// 默认LZMA
				return BuildAssetBundleOptions.None;

			}
		}

        /// <summary>
        /// 获取构建选项
        /// </summary>
        private BuildAssetBundleOptions MakeBuildOptions()
        {
            // For the new build system, unity always need BuildAssetBundleOptions.CollectDependencies and BuildAssetBundleOptions.DeterministicAssetBundle
            // 除非设置ForceRebuildAssetBundle标记，否则会进行增量打包
            BuildAssetBundleOptions opt = BuildAssetBundleOptions.None;
            opt |= BuildAssetBundleOptions.StrictMode; //Do not allow the build to succeed if any errors are reporting during it.
			opt |= GetConfigBuildCompressionOption();
			if (IsForceRebuild)
			{
				opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
            }
            if (IsAppendHash)
            {
                opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName; //Append the hash to the assetBundle name
            }
            if (IsDisableWriteTypeTree)
            {
                opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
            }
            if (IsIgnoreTypeTreeChanges)
            {
                opt |= BuildAssetBundleOptions.IgnoreTypeTreeChanges; //Ignore the type tree changes when doing the incremental build check.
            }
            return opt;
        }

		/// <summary>
		/// 创建Readme文件到输出目录
		/// </summary>
		/// <param name="outputDirectory">输出目录</param>
		private void CreateReadmeFile(string outputDirecotry, AssetBundleManifest unityManifest)
		{
			string[] allAssetBundles = unityManifest.GetAllAssetBundles();

			// 删除旧文件
			string filePath = $"{outputDirecotry}/{AssetBundleBuildConstData.ReadmeFileName}";
			if (File.Exists(filePath))
				File.Delete(filePath);

			Debug.Log($"创建说明文件：{filePath}");

			StringBuilder content = new StringBuilder();
			AppendBuildTargetAndTimeContent(content);
			AppendCollectorContent(content);
			AppendBuildParametersContent(content);
			AppendData(content, $"--构建清单--");
			for (int i = 0; i < allAssetBundles.Length; i++)
			{
				AppendData(content, allAssetBundles[i]);
			}

			// 创建新文件
			File.WriteAllText(filePath, content.ToString(), Encoding.UTF8);
		}
		#endregion

		/// <summary>
		/// 往StringBuilder添加数据
		/// </summary>
		/// <param name="sb"></param>
		/// <param name="data"></param>
		private void AppendData(StringBuilder sb, string data)
		{
			sb.Append(data);
			sb.Append("\r\n");
		}
	}
}