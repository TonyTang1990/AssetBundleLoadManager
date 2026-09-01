/*
 * Description:             BuildResult.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/25
 */

/// <summary>
/// BuildResult.cs
/// 打包结果枚举
/// Note:
/// 打包枚举int值当做打包结果返回给进程退出，所以0必须是打包成功
/// </summary>
public enum BuildResult
{
    /// <summary>
    /// 打包成功
    /// </summary>
    Success = 0,
    /// <summary>
    /// 无打包参数
    /// </summary>
    No_Build_Parameters,
    /// <summary>
    /// 无效打包参数
    /// </summary>
    Invalide_Build_Parameters,
    /// <summary>
    /// 无效输出路径
    /// </summary>
    Invalide_Output_Path,
    /// <summary>
    /// 不支持的平台
    /// </summary>
    Not_Supported_Build_Target,
    /// <summary>
    /// 不支持的打包分组
    /// </summary>
    Not_Supported_Build_Target_Group,
    /// <summary>
    /// 无内部版本配置文件
    /// </summary>
    No_Inner_Version_Config,
    /// <summary>
    /// 无平台渠道配置文件
    /// </summary>
    No_Platform_Channel_Config,
    /// <summary>
    /// 不正确的平台渠道配置
    /// </summary>
    Incorrect_Platform_Channel_Config,
    /// <summary>
    /// 打包通用前处理失败
    /// </summary>
    Build_Common_PreProcess_Failed,
    /// <summary>
    /// 打包资源失败
    /// </summary>
    Build_Resource_Failed,
    /// <summary>
    /// 打包目标通用前处理失败
    /// </summary>
    Build_Target_Common_PreProcess_Failed,
    /// <summary>
    /// 打包预处理失败
    /// </summary>
    Build_Target_PreProcess_Failed,
    /// <summary>
    /// 打包抛异常
    /// </summary>
    Build_Exception,
    /// <summary>
    /// 打包失败
    /// </summary>
    Build_Failed,
    /// <summary>
    /// 打包目标后处理失败
    /// </summary>
    Build_Target_PostProcess_Failed,
}