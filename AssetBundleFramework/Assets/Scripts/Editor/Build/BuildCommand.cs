/*
 * Description:             BuildCommand.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/20
 */

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// BuildCommand.cs
/// 打包命令静态类
/// </summary>
public static class BuildCommand
{
    /// <summary>
    /// 解析命令行参数为BuildParameters对象
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static BuildParameters ParseCommandArgsToBuildParameters(string[] args)
    {
        if(args == null || args.Length == 0)
        {
            Debug.LogError("不允许传入空参数，打包参数解析失败！");
            return null;
        }
        var parameterMap = ParseCommandArgsToDictionary(args);
        BuildParameters buildParameters = ConstructBuildParametersByParamsMap(parameterMap);
        return buildParameters;
    }

    /// <summary>
    /// 解析命令行参数为字典
    /// Note:
    /// 1. 统一打包命令格式为:Unity执行程序路径 -命令1 值1(可以没值) -命令2 值2(可以没值) ******
    /// 2. 没有参数值的Value为null
    /// 3. 解析结果不包含Unity执行程序路径
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Dictionary<string, string> ParseCommandArgsToDictionary(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Debug.LogError("不允许传入空参数，打包参数解析失败！");
            return null;
        }
        Dictionary<string, string> parameterMap = new Dictionary<string, string>();
        var argTotalNum = args.Length;
        var lastIndex = Math.Max(argTotalNum - 1, 0);
        // 默认跳过Unity执行路径
        for(int index = 1; index < argTotalNum; index++)
        {
            var isLastIndex = index == lastIndex;
            var arg = args[index];
            // -logFile比较特殊，有可能跟-作为值表示输出到控制台
            // 这里特殊处理-logFile
            if(string.Equals(arg, BuildCommandConst.LOG_FILE))
            {
                if(!isLastIndex)
                {
                    var nextArg = args[index + 1];
                    if(string.Equals(nextArg, "-"))
                    {
                        // 直接赋值并跳过-值往后继续分析
                        parameterMap[arg] = nextArg;
                        index++;
                        continue;
                    }
                }
            }
            if (arg.StartsWith("-"))
            {
                string argValue = null;
                if(!isLastIndex)
                {
                    var valueIndex = index + 1;
                    var nextArg = args[valueIndex];
                    // 下一个参数不是以-开头表示是有值的
                    if(!nextArg.StartsWith("-"))
                    {
                        argValue = nextArg;
                        // 如果有值则直接跳到下一个参数进行解析
                        index++;
                    }
                }
                // 如果命令是最后一个参数则表示没有值
                if(parameterMap.ContainsKey(arg))
                {
                    Debug.LogWarning($"命令行参数中存在重复的命令: {arg}，将覆盖之前的值！");
                    parameterMap[arg] = argValue;
                }
                else
                {
                    parameterMap.Add(arg, argValue);
                }
            }
        }
        return parameterMap;
    }

    /// <summary>
    /// 根据参数Map构建BuildParameters对象
    /// </summary>
    /// <param name="parameterMap"></param>
    /// <returns></returns>
    private static BuildParameters ConstructBuildParametersByParamsMap(Dictionary<string, string> parameterMap)
    {
        if(parameterMap == null || parameterMap.Count == 0)
        {
            Debug.LogError("不允许传入空参数Map，构建BuildParameters失败！");
            return null;
        }
        if(!parameterMap.TryGetValue(BuildCommandConst.BUILD_TARGET, out string buildTargetStr))
        {
            Debug.LogError($"传入打包参数中缺少必要的参数:{BuildCommandConst.BUILD_TARGET}，构建BuildParameters失败！");
            return null;
        }
        BuildTarget buildTarget = BuildTarget.Android;
        var result = Enum.TryParse<BuildTarget>(buildTargetStr, out buildTarget);
        if(!result)
        {
            Debug.LogError($"传入打包参数:{BuildCommandConst.BUILD_TARGET}参数值无效:{buildTargetStr}，构建BuildParameters失败！");
            return null;
        }
        if(!parameterMap.TryGetValue(BuildCommandConst.DEVELOPMENT_MODE, out string developmentModeStr))
        {
            Debug.LogError($"传入打包参数中缺少必要的参数:{BuildCommandConst.DEVELOPMENT_MODE}，构建BuildParameters失败！");
            return null;
        }
        if(!parameterMap.TryGetValue(BuildCommandConst.CHANNEL, out string buildChannelStr))
        {
            Debug.LogError($"传入打包参数中缺少必要的参数:{BuildCommandConst.CHANNEL}，构建BuildParameters失败！");
            return null;
        }
        Channel channel = Channel.None;
        result = Enum.TryParse<Channel>(buildChannelStr, out channel);
        if(!result)
        {
            Debug.LogError($"传入打包参数:{BuildCommandConst.CHANNEL}参数值无效:{buildChannelStr}，构建BuildParameters失败！");
            return null;
        }
        if(!parameterMap.TryGetValue(BuildCommandConst.IS_DEVELOPMENT, out string isDevelopmentStr))
        {
            Debug.LogError($"传入打包参数中缺少必要的参数:{BuildCommandConst.IS_DEVELOPMENT}，构建BuildParameters失败！");
            return null;
        }
        bool isDevelopment = false;
        result = bool.TryParse(isDevelopmentStr, out isDevelopment);
        if(!result)
        {
            Debug.LogError($"传入打包参数:{BuildCommandConst.IS_DEVELOPMENT}参数值无效:{isDevelopmentStr}，构建BuildParameters失败！");
            return null;
        }
        if(!parameterMap.TryGetValue(BuildCommandConst.VERSION_CODE, out string versionCodeStr))
        {
            Debug.LogError($"传入打包参数中缺少必要的参数:{BuildCommandConst.VERSION_CODE}，构建BuildParameters失败！");
            return null;
        }
        double versionCode = 1;
        result = double.TryParse(versionCodeStr, out versionCode);
        if(!result)
        {
            Debug.LogError($"传入打包参数:{BuildCommandConst.VERSION_CODE}参数值无效:{versionCodeStr}，构建BuildParameters失败！");
            return null;
        }
        // 版本号格式限定*.**
        (var versionCodeResult, var finalVersionCode) = VersionEditorUtilities.FormatVersionCode(versionCode);
        if(!versionCodeResult)
        {
            Debug.LogError($"传入打包参数:{BuildCommandConst.VERSION_CODE}参数值无效:{versionCodeStr}，格式要求:*.**，构建BuildParameters失败！");
            return null;
        }
        if(!parameterMap.TryGetValue(BuildCommandConst.RESOURCE_VERSION_CODE, out string resourceVersionCodeStr))
        {
            Debug.LogError($"传入打包参数中缺少必要的参数:{BuildCommandConst.RESOURCE_VERSION_CODE}，构建BuildParameters失败！");
            return null;
        }
        int resourceVersionCode = 1;
        result = int.TryParse(resourceVersionCodeStr, out resourceVersionCode);
        if(!result)
        {
            Debug.LogError($"传入打包参数:{BuildCommandConst.RESOURCE_VERSION_CODE}参数值无效:{resourceVersionCodeStr}，构建BuildParameters失败！");
            return null;
        }
        if(!parameterMap.TryGetValue(BuildCommandConst.FORCE_REBUILD_AB, out string forceRebuildABStr))
        {
            Debug.LogError($"传入打包参数中缺少必要的参数:{BuildCommandConst.FORCE_REBUILD_AB}，构建BuildParameters失败！");
            return null;
        }
        bool forceRebuildAB = false;
        result = bool.TryParse(forceRebuildABStr, out forceRebuildAB);
        if(!result)
        {
            Debug.LogError($"传入打包参数:{BuildCommandConst.FORCE_REBUILD_AB}参数值无效:{forceRebuildABStr}，构建BuildParameters失败！");
            return null;
        }
        BuildParameters buildParameters = new BuildParameters(buildTarget, channel, isDevelopment,
                                                              finalVersionCode, resourceVersionCode,
                                                              forceRebuildAB);
        return buildParameters;
    }
}