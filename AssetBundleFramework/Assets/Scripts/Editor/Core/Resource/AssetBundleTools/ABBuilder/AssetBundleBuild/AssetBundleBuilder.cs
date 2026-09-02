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
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Content;
using UnityEditor.SceneManagement;
using System.Globalization;

// 打包设计简要说明:

// AB打包生成文件说明：
// AB相关资源文件(e.g. AB依赖信息文件，资源AB文件)
// AB打包说明文件(e.g. readme.txt, assetBuildReadme.txt)
// 校验AB资源热更信息文件(VerifyABInfo.json)
// AB资源热更新信息文件(ABInfo.json)

// AB打包类型说明:
// AssetBundleBuildPurpose.BuildPlayerBaseLine(构建母包，相关资源文件会放到Resources和StreammingAssets)
// AssetBundleBuildPurpose.BuildHotUpdate(构建热更包，相关文件会直接输出到热更新目录对应位置)

// AB打包生成位置说明：
// AB打包会统一先生成到项目目录下的BuildCache/ABBuild目录
// 然后清空BuildCache/ABBuildRename目录并将ABBuild目录打包的AB所有资源复制到BuildCache/ABBuildRename目录
// 然后将BuildCache/ABBuildRename目录下的资源根据MD5信息进行改名
// 然后根据BuildCache/ABBuildRename目录下的资源名生成VerifyABInfo.json和ABInfo.json到BuildCache/ResourcesCache/目录下
// 然后根据AB打包类型会决定BuildCache/ABBuildRename和BuildCache/ResourcesCache/目录下的资源拷贝到对应目标为止

namespace TResource
{
	/// <summary>
	/// AB打包工具
	/// </summary>
	public class AssetBundleBuilder
	{
		/// <summary>
		/// AssetBundle打包参数
		/// </summary>
		public AssetBundleBuildParams AssetBundleBuildParams
		{
			get;
			private set;
		}

		/// <summary>
		/// 构建目标平台AB缓存目录路径
		/// </summary>
		public string BuildTargetABTempFolderPath
		{
			get;
			private set;
		}

		/// <summary>
		/// 构建目标平台AB改名缓存目录路径
		/// </summary>
		public string BuildTargetABRenameTempFolderPath
		{
			get;
			private set;
		}

		/// <summary>
		/// 构建Resources缓存目录路径
		/// </summary>
		public string BuildResourcesTempFolderPath
		{
			get;
			private set;
		}

		/// <summary>
        /// 构建平台组
        /// </summary>
		public BuildTargetGroup BuildTargetGroup
		{
	   	    get;
			private set;
		} = BuildTargetGroup.Unknown;

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
			AssetBundleBuildConstData.BuildLogStepFileName,
		};

		/// <summary>
		/// AssetBuilder
		/// </summary>
		/// <param name="buildTarget">构建平台</param>
		/// <param name="assetBundleBuildPurpose">AB打包用途</param>
		public AssetBundleBuilder(AssetBundleBuildParams assetBundleBuildParams)
		{
			var buildTarget = assetBundleBuildParams.BuildTarget;
			AssetBundleBuildParams = assetBundleBuildParams;
			BuildTargetABTempFolderPath = AssetBundleBuilderHelper.GetBuildTargetABTempFolderPath(buildTarget);
			BuildTargetABRenameTempFolderPath = AssetBundleBuilderHelper.GetBuildTargetABRenameTempFolderPath(buildTarget);
			BuildResourcesTempFolderPath = AssetBundleBuilderHelper.GetBuildResourcesTempFolderPath(buildTarget);
			BuildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
		}

		/// <summary>
		/// 准备构建
		/// </summary>
		public bool PreAssetBuild()
		{
			Debug.Log("------------------------------OnPreAssetBuild------------------------------");

			// 检测构建平台是否合法
			var buildTarget = AssetBundleBuildParams.BuildTarget;
			if (buildTarget == BuildTarget.NoTarget)
            {
                Debug.LogError("[BuildPatch] 请选择目标平台");
				return false;
            }

            // 检测构建版本是否合法
            //if (EditorUtilities.IsNumber(BuildVersion.ToString()) == false)
            //	throw new Exception($"[BuildPatch] 版本号格式非法：{BuildVersion}");
            //if (BuildVersion < 0)
            //	throw new Exception("[BuildPatch] 请先设置版本号");

            // 检测补丁包是否已经存在
            //string packageDirectory = GetPackageDirectory();
            //if (Directory.Exists(packageDirectory))
            //	throw new Exception($"[BuildPatch] 补丁包已经存在：{packageDirectory}");

            // 如果是强制重建
			var isForceRebuild = AssetBundleBuildParams.IsForceRebuild;
            if (isForceRebuild)
			{
				// 删除平台总目录
				if (Directory.Exists(BuildTargetABTempFolderPath))
				{
					Log($"删除平台总目录：{BuildTargetABTempFolderPath}");
					Directory.Delete(BuildTargetABTempFolderPath, true);
				}
			}

			// 如果输出目录不存在
			if (!Directory.Exists(BuildTargetABTempFolderPath))
			{
				Directory.CreateDirectory(BuildTargetABTempFolderPath);
				Log($"创建输出目录：{BuildTargetABTempFolderPath}");
			}

			// Asset打包信息输出目录不存在
			var assetbuildinfofolderpath = ResourcePath.GetAssetBuildInfoFolderFullPath();
			Debug.Log($"Asset打包信息输出目录:{assetbuildinfofolderpath}");
			if (!Directory.Exists(assetbuildinfofolderpath))
			{
				Directory.CreateDirectory(assetbuildinfofolderpath);
				Log($"创建打包信息Asset输出目录：{assetbuildinfofolderpath}");
			}
			return true;
		}

		/// <summary>
		/// 执行构建
		/// </summary>
		public bool PostAssetBuild()
		{
			Debug.Log("------------------------------PostAssetBuild------------------------------");
			// 准备工作
			var result = DoAssetBundleBuildPreparation();
			if(!result)
			{
				Debug.LogError($"AB打包准备工作失败，打包终止!");
				return false;
			}
			// 开始构建
			Log($"开始构建......");
			// 避免SBP打包时场景未保存报错
			EditorSceneManager.SaveOpenScenes();
			bool buildSuccess;
#if OLD_ASSET_BUILD_PIPELINE
			DoCustomAssetBundleBuild(BuildTargetABTempFolderPath, out buildSuccess);
#else
			DoSBPAssetBundleBuild(BuildTargetABTempFolderPath, out buildSuccess);
#endif
			if(buildSuccess == false)
            {
				Debug.LogError($"打包AB失败!");
				return false;
            }

			// 视频单独打包
			//PackVideo(buildAssetInfoList);

			// 避免生成ABInfo.json时又重新计算文件相关信息
			Dictionary<string, RenameFileInfo> renameFileInfoMap = new Dictionary<string, RenameFileInfo>();
			var copyAndRenameResult = CopyAndRenameAllABFiles(ref renameFileInfoMap);
			if(!copyAndRenameResult)
			{
				Debug.LogError($"生成复制和改名AB失败，打包终止!");
				return false;
			}

			// 单独生成包内的VerifyABInfo.json和ABInfo.json信息
			var createResult = CreateVerifyAndABInfoFile(ref renameFileInfoMap);
			if(!createResult)
			{
				Debug.LogError($"生成VerifyABInfo.json和ABInfo.json信息失败，打包终止!");
				return false;
			}

			var buildPostProcessResult = DoAssetBundleBuildPostProcess();
			if(!buildPostProcessResult)
			{
				Debug.LogError($"AB打包后续处理失败，打包终止!");
				return false;
			}
			Log("构建完成！");
			return true;
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
			SBPAssetBundleBuilder.BuildAllAssetBundles(this, outputDirectory, AssetBundleBuildParams.BuildTarget,
																	  buildParams, mAllAssetBundleBuildList,
																	  out buildSuccess, out results);
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
			AssetBundleManifest unityManifest = OldAssetBundleBuilder.BuildAllAssetBundles(this, outputDirectory,
																						AssetBundleBuildParams.BuildTarget,
																						options, mAllAssetBundleBuildList,
																						out buildSuccess);
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
			if(!DoAnalyseAssetBundleBuild())
            {
				return false;
            }
            // 创建Asset AB打包详细说明信息文件
			CreateAssetBuildReadmeFile();
			return true;
        }

		/// <summary>
		/// 执行AB打包完成后处理
		/// </summary>
		/// <returns></returns>
		private bool DoAssetBundleBuildPostProcess()
		{
			// 执行根据AB打包用途的后续资源处理
			bool buildPurposeResult = DoAssetBundleBuildPurposePostProcess();
			if(!buildPurposeResult)
			{
				Debug.LogError($"AB打包用途:{AssetBundleBuildParams.AssetBundleBuildPurpose}的后续处理失败!");
				return false;
			}
			return true;
		}

		/// <summary>
		/// 执行根据AB打包用途的后续资源处理
		/// </summary>
		/// <returns></returns>
		private bool DoAssetBundleBuildPurposePostProcess()
		{
			// 强制刷新Asset，避免移动到Assets内的一些文件操作没刷新Meta导致后续打包访问出错
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			AssetDatabase.SaveAssets();

			var finalABBuildTargetOutputFolderPath = GetFinalABBuildTargetOutputFolderPath();
			var finalABBuildResourceOutputFolderPath = GetFinalABBuildResourcesOutputFolderPath();
			// 确保资源打包目录是全新的再操作后续复制文件流程，避免有错误资源进包
			FolderUtilities.RecreateSpecificFolder(finalABBuildTargetOutputFolderPath);
			// 包内Resources还有别的资源不能直接全部删除
			FolderUtilities.CheckAndCreateSpecificFolder(finalABBuildResourceOutputFolderPath);

			// 复制改名后的AB资源目录和AB热更新信息文件(e.g. ABInfo.json, VerifyABInfo.json)到最终输出目录
			FileUtilities.CopyFolderToFolder(BuildTargetABRenameTempFolderPath, finalABBuildTargetOutputFolderPath);

			var verifyABInfoFileBuildTempPath = GetVerifyABInfoFileBuildTempPath();
			var abInfoFileBuildTempPath = GetABInfoFileBuildTempPath();
			string newVerifyABInfoFilePath;
			var copyVerifyABInfoResule = FileUtilities.CopyFileToFolder(verifyABInfoFileBuildTempPath, finalABBuildResourceOutputFolderPath, out newVerifyABInfoFilePath);
			if(!copyVerifyABInfoResule)
			{
				Debug.LogError($"复制包内资源信息文件:{verifyABInfoFileBuildTempPath}到最终输出目录:{finalABBuildResourceOutputFolderPath}失败，请检查是否有文件被占用或其他问题!");
				return false;
			}
			string newABInfoFilePath;
			var copyABInfoResult = FileUtilities.CopyFileToFolder(abInfoFileBuildTempPath, finalABBuildResourceOutputFolderPath, out newABInfoFilePath);
			if(!copyABInfoResult)
			{
				Debug.LogError($"复制包内资源信息文件:{abInfoFileBuildTempPath}到最终输出目录:{finalABBuildResourceOutputFolderPath}失败，请检查是否有文件被占用或其他问题!");
				return false;
			}
			var abBuildPurpose = AssetBundleBuildParams.AssetBundleBuildPurpose;
			if(abBuildPurpose == AssetBundleBuildPurpose.BuildHotUpdate)
			{
				// 热更流程还需要生成热更新版本号到热更目录
				var hotUpdateOutputFolderPath = HotUpdatePath.GetLocalHotUpdateFolderPath(AssetBundleBuildParams.BuildTarget);
				var updateVersionConfigResult = HotUpdateTool.UpdateHotUpdateVersionConfig(hotUpdateOutputFolderPath, AssetBundleBuildParams.VersionCode, AssetBundleBuildParams.ResourceVersionCode);
				if(!updateVersionConfigResult)
				{
					Debug.LogError($"更新热更新版本文件失败！版本号:{AssetBundleBuildParams.VersionCode}，资源版本号:{AssetBundleBuildParams.ResourceVersionCode}到热更目录:{hotUpdateOutputFolderPath}!");
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// 更新AssetBundle打包编译信息Asset
		/// </summary>
		/// <param name="assetBundleBuildInfoList"></param>
		private bool UpdateAssetBundleBuildInfoAsset(List<AssetBundleBuildInfo> assetBundleBuildInfoList)
		{
			// Note: AssetBundle打包信息统一存小写，确保和AB打包那方一致
			var assetBundleBuildInfoAssetRelativePath = ResourcePath.GetAssetBuildInfoFileRelativePath();
			var assetBundleBuildAsset = AssetDatabase.LoadAssetAtPath<AssetBuildInfoAsset>(assetBundleBuildInfoAssetRelativePath);
			if (assetBundleBuildAsset == null)
			{
				assetBundleBuildAsset = ScriptableObject.CreateInstance<AssetBuildInfoAsset>();
				AssetDatabase.CreateAsset(assetBundleBuildAsset, assetBundleBuildInfoAssetRelativePath);
			}
			assetBundleBuildAsset.BuildAssetInfoList.Clear();

			// 重复Asset名字打包检测
			var dumplicatedAssetNameMap = new Dictionary<string, string>();

			// Asset打包信息构建
			foreach (var assetBundleBuildInfo in assetBundleBuildInfoList)
			{
				if(assetBundleBuildInfo.AssetBuildInfoMap == null)
				{
					continue;
				}
				var assetBundleName = assetBundleBuildInfo.AssetBundleName;
				var assetBundleVariant = assetBundleBuildInfo.AssetBundleVariant;
				foreach (var assetBuildInfoData in assetBundleBuildInfo.AssetBuildInfoMap)
				{
					// 仅导出需要从代码主动加载的Asset打包信息，其他Asset通过依赖加载还原即可(优化Asset打包信息Asset数据量)
					var assetBuildInfo = assetBuildInfoData.Value;
					if(!assetBuildInfo.IsAllowLoadFromScript)
					{
						continue;
					}
					// 不剔除后缀，确保AssetDatabase模式可以全路径(带后缀)加载
					var assetPath = assetBuildInfo.AssetPath;
					var buildAssetInfo = new BuildAssetInfo(assetPath, assetBundleName, assetBundleVariant);
					var assetName = buildAssetInfo.AssetName;
					if(dumplicatedAssetNameMap.TryGetValue(assetName, out var preAssetPath))
					{
						Debug.LogError($"重复的Asset名字:{assetName}，Asset路径1:{preAssetPath}和Asset路径2:{assetPath}的Asset名字同名了，请修改资源名避免重名!");
						return false;
					}
					dumplicatedAssetNameMap.Add(assetName, assetPath);
					assetBundleBuildAsset.BuildAssetInfoList.Add(buildAssetInfo);
				}
			}

			EditorUtility.SetDirty(assetBundleBuildAsset);
			AssetDatabase.SaveAssets();
			return true;
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

#region 公共部分
		/// <summary>
		/// 获取正确的版本号(默认保留两位小数)
		/// </summary>
		/// <param name="versionCode"></param>
		/// <returns></returns>
		public static double GetCorrectVersionCode(double versionCode)
		{
			// 版本号目录默认最多保留两位小数
			return Math.Truncate(versionCode * 100) / 100;
		}

		/// <summary>
		/// 获取指定版本号的字符串(默认保留两位小数)
		/// </summary>
		/// <param name="versionCode"></param>
		/// <returns></returns>
		private string GetVersionCodeS(double versionCode)
		{
			// 版本号目录默认最多保留两位小数且不显示最后位的0
			return versionCode.ToString("0.##");
		}

		/// <summary>
		/// 获取指定资源版本号的字符串
		/// </summary>
		/// <param name="resourceVersionCode"></param>
		/// <returns></returns>
		private string GetResourceVersionCodeS(int resourceVersionCode)
		{
			return resourceVersionCode.ToString();
		}

		/// <summary>
		/// 获取校验AB打包信息打包AB临时路径
		/// </summary>
		/// <returns></returns>
		private string GetVerifyABInfoFileBuildTempPath()
		{
			var verifyABInfoResRelativePath = ResourcePath.GetVerifyABInfoResRelativePath();
			return Path.Combine(BuildResourcesTempFolderPath, verifyABInfoResRelativePath);
		}

		/// <summary>
		/// 获取AB打包信息打包AB临时路径
		/// </summary>
		/// <returns></returns>
		private string GetABInfoFileBuildTempPath()
		{
			var abInfoResRelativePath = ResourcePath.GetABInfoResRelativePath();
			return Path.Combine(BuildResourcesTempFolderPath, abInfoResRelativePath);
		}

		/// <summary>
		/// 获取AB打包说明文件打包AB临时路径
		/// </summary>
		/// <returns></returns>
		private string GetBuildReadmeFileBuildTempPath()
		{
			return Path.Combine(BuildTargetABTempFolderPath, AssetBundleBuildConstData.AssetBuildReadmeFileName);
		}

		/// <summary>
		/// 获取最终本地热更新输出目录路径
		/// </summary>
		/// <returns></returns>
		private string GetFinalLocalHotUpdateOutputFolderPath()
		{
			var localHotUpdateOutputFolderPath = HotUpdatePath.GetLocalHotUpdateFolderPath(AssetBundleBuildParams.BuildTarget);
			var versionCode = AssetBundleBuildParams.VersionCode;
			var resourceVersionCode = AssetBundleBuildParams.ResourceVersionCode;
			var versionCodeS = GetVersionCodeS(versionCode);
			var resourceVersionCodeS = GetResourceVersionCodeS(resourceVersionCode);
			var finalLocalHotUpdateOutputFolderPath = Path.Combine(localHotUpdateOutputFolderPath, versionCodeS, resourceVersionCodeS);
			return finalLocalHotUpdateOutputFolderPath;
		}

		/// <summary>
		/// 获取最终平台AB打包输出目录路径
		/// </summary>
		/// <returns></returns>
		private string GetFinalABBuildTargetOutputFolderPath()
		{
			var buildTarget = AssetBundleBuildParams.BuildTarget;
			if(AssetBundleBuildParams.AssetBundleBuildPurpose == AssetBundleBuildPurpose.BuildPlayerBaseLine)
			{
				return AssetBundleBuilderHelper.GetBuildTargetOutputRootPath(buildTarget);
			}
			else if(AssetBundleBuildParams.AssetBundleBuildPurpose == AssetBundleBuildPurpose.BuildHotUpdate)
			{
				return GetFinalLocalHotUpdateOutputFolderPath();
			}
			else
			{
				Debug.LogError($"不支持的AB打包用途:{AssetBundleBuildParams.AssetBundleBuildPurpose}，获取最终AB打包输出目录失败!");
				return string.Empty;
			}
		}

		/// <summary>
		/// 获取最终平台AB打包Resources输出目录路径
		/// </summary>
		/// <returns></returns>
		private string GetFinalABBuildResourcesOutputFolderPath()
		{
			if(AssetBundleBuildParams.AssetBundleBuildPurpose == AssetBundleBuildPurpose.BuildPlayerBaseLine)
			{
				return ResourcePath.GetProjectResourcesFullPath();
			}
			else if(AssetBundleBuildParams.AssetBundleBuildPurpose == AssetBundleBuildPurpose.BuildHotUpdate)
			{
				return GetFinalLocalHotUpdateOutputFolderPath();
			}
			else
			{
				Debug.LogError($"不支持的AB打包用途:{AssetBundleBuildParams.AssetBundleBuildPurpose}，获取最终AB打包输出目录失败!");
				return string.Empty;
			}
		}
#endregion

#region 准备工作
		/// <summary>
		/// 执行AssetBundle打包分析
		/// </summary>
		private bool DoAnalyseAssetBundleBuild()
		{
            // 获取所有的收集路径
            List<string> collectDirectorys = AssetBundleCollectSettingData.GetAllCollectDirectory();
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
			var result = UpdateAssetBundleBuildDatas();
			if(!result)
			{
				Debug.LogError("更新AssetBundle打包数据失败");
				return false;
			}

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
            var assetBuildInfoAssetRelativePath = ResourcePath.GetAssetBuildInfoFileRelativePath();
            var assetBundleName = ResourcePath.GetAssetBuildInfoABName();
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
		private bool UpdateAssetBundleBuildDatas()
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
            var result = UpdateAssetBundleBuildInfoAsset(mAllAssetBundleBuildInfoList);
			return result;
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
			assetBundleName = ResourcePath.GetABPathWithPostFix(assetBundleName);
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
			assetBundleName = GetAssetBundleName(assetPath);
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
		/// 获取临时输出目录下所有的AB文件路径
		/// </summary>
		/// <returns></returns>
		private IEnumerable<string> GetAllABPathUnderTempOutputFolder()
		{
			var abFilesPath = Directory.GetFiles(BuildTargetABTempFolderPath, $"*.*", SearchOption.AllDirectories).Where(
				f => !f.EndsWith(".meta") && !f.EndsWith(".manifest") &&
				!mAllMD5FileNameBlackList.Contains(Path.GetFileName(f)));
			return abFilesPath;
		}

		/// <summary>
		/// 获取临时改名目录下所有的AB文件路径
		/// </summary>
		/// <returns></returns>
		private IEnumerable<string> GetAllABPathUnderRenameTempFolder()
		{
			var abFilesPath = Directory.GetFiles(BuildTargetABRenameTempFolderPath, $"*.*", SearchOption.AllDirectories).Where(
				f => !f.EndsWith(".meta") && !f.EndsWith(".manifest") &&
				!mAllMD5FileNameBlackList.Contains(Path.GetFileName(f)));
			return abFilesPath;
		}

		/// <summary>
		/// 更新AssetBundle文件名带MD5和创建AssetBundleInfo.json信息文件
		/// </summary>
		/// <param name="renameFileInfoMap">AB重命名信息<重命名文件全路径，重命名文件信息></param>
		private bool CopyAndRenameAllABFiles(ref Dictionary<string, RenameFileInfo> renameFileInfoMap)
		{
			renameFileInfoMap.Clear();
			// 将打包输出的AB文件复制到临时的改名目录下
			// 避免直接在打包输出目录改名导致AB无法增量打包问题
			// 避免目录存在旧资源导致操作到错误资源文件问题
			FolderUtilities.MakeSureFolderExistAndClean(BuildTargetABRenameTempFolderPath);

			// 有效排除不必要进包或者进入热更新的临时文件(e.g. readme.txt)
			var abFilesFullPath = GetAllABPathUnderTempOutputFolder();
			var abFileNumber = abFilesFullPath?.Count() ?? 0;
			if(abFileNumber == 0)
			{
				Debug.LogWarning($"临时输出目录:{BuildTargetABTempFolderPath}下没有AB文件，无法复制到改名目录:{BuildTargetABRenameTempFolderPath}!");
				return true;
			}
			string newABFilePath;
			bool copyResult;
			string abFileRelativePath;
			foreach(var abFilePath in abFilesFullPath)
			{
				abFileRelativePath = Path.GetRelativePath(BuildTargetABTempFolderPath, abFilePath);
				newABFilePath = Path.Combine(BuildTargetABRenameTempFolderPath, abFileRelativePath);
				copyResult = FileUtilities.CopyFileToFile(abFilePath, newABFilePath);
				if(!copyResult)
				{
					Debug.LogError($"复制AB文件:{abFilePath}到改名目录:{BuildTargetABRenameTempFolderPath}失败，请检查是否有文件被占用或其他问题!");
					return false;
				}
			}
			var md5 = MD5.Create();
			var sha256 = SHA256.Create();
			var renameABFilesFullPath = GetAllABPathUnderRenameTempFolder();
			foreach(var renameABFilePath in renameABFilesFullPath)
			{
				// 相对于热更新目录的AB路径
				var hotupdateABRelativePath = Path.GetRelativePath(BuildTargetABRenameTempFolderPath, renameABFilePath);
				// 清除最前面遗留的/避免相对路径不对问题
				hotupdateABRelativePath = hotupdateABRelativePath.TrimStart('/', '\\');
				hotupdateABRelativePath = PathUtilities.GetRegularPath(hotupdateABRelativePath);
				var fileMd5 = FileUtilities.GetFileMD5(renameABFilePath, md5);
				var newABRelativePath = PathUtilities.GetFilePathWithMD5(renameABFilePath, fileMd5);
				var newABFileName = Path.GetFileName(newABRelativePath);
				// 修改AB文件名
				var newFilePath = FileUtilities.RenameFile(renameABFilePath, newABFileName);
				if(string.IsNullOrEmpty(newFilePath))
				{
					Debug.LogError($"AB文件名修改带MD5名失败，AB Asset相对路径:{renameABFilePath}，新AB文件名:{newABFileName}，错误信息:{newFilePath}");
					return false;
				}
				var newABFileInfo = new FileInfo(newFilePath);
				if(!newABFileInfo.Exists)
				{
					Debug.LogError($"新AB文件名:{newFilePath}不存在，获取文件信息失败!");
					return false;
				}
				var newFileSha256 = FileUtilities.GetFileSha256(newFilePath, sha256);
				var renameFileInfo = new RenameFileInfo(newFilePath, hotupdateABRelativePath, 
														newABFileInfo.Length, fileMd5, newFileSha256);
				renameFileInfoMap.Add(newFilePath, renameFileInfo);
			}
			Debug.Log($"复制并修改AB文件名带MD5成功，文件改名总数:{renameFileInfoMap.Count}，临时改名目录:{BuildTargetABRenameTempFolderPath}");
			return true;
		}

		/// <summary>
		/// 创建VerifyABInfo.json和ABInfo.json信息文件
		/// </summary>
		/// <param name="renameFileInfoMap"></param>
		/// <returns></returns>
		private bool CreateVerifyAndABInfoFile(ref Dictionary<string, RenameFileInfo> renameFileInfoMap)
		{
			// 避免目录存在旧的文件信息
			FolderUtilities.MakeSureFolderExistAndClean(BuildResourcesTempFolderPath);

			var hotUpdateABInfo = new HotUpdateABInfo();
			foreach(var renameFileInfoPairs in renameFileInfoMap)
			{
				var renameFileInfo = renameFileInfoPairs.Value;
				var hotUpdateSingleABInfo = new HotUpdateSingleABInfo(renameFileInfo.FileRelativePath, renameFileInfo.FileMd5,
																	  renameFileInfo.FileSize, renameFileInfo.FileSha256);
				hotUpdateABInfo.AddHotUpdateSingleABInfo(hotUpdateSingleABInfo);
			}
			var abInfoContent = JsonUtility.ToJson(hotUpdateABInfo, true);
			var outputABInfoFilePath = GetABInfoFileBuildTempPath();
			// 确保创建最新的
			FileUtilities.DeleteFile(outputABInfoFilePath);
			File.WriteAllText(outputABInfoFilePath, abInfoContent, new UTF8Encoding(false));
			Debug.Log($"创建ABInfo.json信息文件成功，文件路径:{outputABInfoFilePath}");

  			var outputABInfoFileInfo = new FileInfo(outputABInfoFilePath);
			if(!outputABInfoFileInfo.Exists)
			{
				Debug.LogError($"ABInfo.json信息文件不存在，无法创建VerifyABInfo.json信息文件，文件路径:{outputABInfoFilePath}");
				return false;
			}
			var abInfoFileSize = outputABInfoFileInfo.Length;
			var abInfoFileSha256Hash = FileUtilities.GetFileSha256(outputABInfoFilePath);
			var outputVerifyABInfoFilePath = GetVerifyABInfoFileBuildTempPath();
			// 确保创建最新的
			FileUtilities.DeleteFile(outputVerifyABInfoFilePath);
			var hotUpdateVerifyABInfo = new HotUpdateVerifyABInfo(abInfoFileSize, abInfoFileSha256Hash);
			var verifyABInfoContent = JsonUtility.ToJson(hotUpdateVerifyABInfo, true);
			File.WriteAllText(outputVerifyABInfoFilePath, verifyABInfoContent, new UTF8Encoding(false));
			Debug.Log($"创建VerifyABInfo.json信息文件成功，文件路径:{outputVerifyABInfoFilePath}");
			return true;
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
            string filePath = GetBuildReadmeFileBuildTempPath();
            if (File.Exists(filePath))
			{
                File.Delete(filePath);
			}

            Log($"创建Asset AB打包详细说明文件：{filePath}");

            StringBuilder content = new StringBuilder();
            AppendData(content, $"构建平台：{AssetBundleBuildParams.BuildTarget}");
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
            AppendData(content, $"构建平台：{AssetBundleBuildParams.BuildTarget}");
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
            AppendData(content, $"CompressOption：{AssetBundleBuildParams.CompressOption}");
            AppendData(content, $"ForceRebuild：{AssetBundleBuildParams.IsForceRebuild}");
            AppendData(content, $"DisableWriteTypeTree：{AssetBundleBuildParams.IsDisableWriteTypeTree}");
            AppendData(content, $"IgnoreTypeTreeChanges：{AssetBundleBuildParams.IsIgnoreTypeTreeChanges}");
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
			var compressOption = AssetBundleBuildParams.CompressOption;
			if (compressOption == ABCompressOption.Uncompressed)
			{
				return UnityEngine.BuildCompression.Uncompressed;
			}
			else if (compressOption == ABCompressOption.ChunkBasedCompressionLZ4)
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
			CustomBuildParameters bundleBuildParameters = new CustomBuildParameters(AssetBundleBuildParams.BuildTarget,
																					BuildTargetGroup,
																					BuildTargetABTempFolderPath);
			//bundleBuildParameters.CacheServerHost = "";
			//bundleBuildParameters.CacheServerPort = ;
            bundleBuildParameters.BundleCompression = GetConfigBuildCompression();
			var isForceRebuild = AssetBundleBuildParams.IsForceRebuild;
            if (isForceRebuild)
            {
                // 是否增量打包
                bundleBuildParameters.UseCache = !isForceRebuild;
            }
            bundleBuildParameters.ContiguousBundles = true;
			var isAppendHash = AssetBundleBuildParams.IsAppendHash;
            if (isAppendHash)
            {
                bundleBuildParameters.AppendHash = isAppendHash;
            }
			var isDisableWriteTypeTree = AssetBundleBuildParams.IsDisableWriteTypeTree;
            if (isDisableWriteTypeTree)
            {
                bundleBuildParameters.ContentBuildFlags |= ContentBuildFlags.DisableWriteTypeTree;
            }
			bundleBuildParameters.ContentBuildFlags |= ContentBuildFlags.StripUnityVersion;
			var isIgnoreTypeTreeChanges = AssetBundleBuildParams.IsIgnoreTypeTreeChanges;
			if (isIgnoreTypeTreeChanges)
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
			var compressOption = AssetBundleBuildParams.CompressOption;
			if (compressOption == ABCompressOption.Uncompressed)
			{
				return BuildAssetBundleOptions.UncompressedAssetBundle;
			}
			else if (compressOption == ABCompressOption.ChunkBasedCompressionLZ4)
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
			if (AssetBundleBuildParams.IsForceRebuild)
			{
				opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle; //Force rebuild the asset bundles
            }
            if (AssetBundleBuildParams.IsAppendHash)
            {
                opt |= BuildAssetBundleOptions.AppendHashToAssetBundleName; //Append the hash to the assetBundle name
            }
            if (AssetBundleBuildParams.IsDisableWriteTypeTree)
            {
                opt |= BuildAssetBundleOptions.DisableWriteTypeTree; //Do not include type information within the asset bundle (don't write type tree).
            }
            if (AssetBundleBuildParams.IsIgnoreTypeTreeChanges)
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