/*
 * Description:             BuildCommandConst.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/22
 */

/// <summary>
/// BuildCommandConst.cs
/// 打包命令常量
/// </summary>
public static class BuildCommandConst
{
    #region Unity内置命令
    /// <summary>
    /// 退出Unity编辑器命令
    /// </summary>
    public const string QUIT = "-quit";

    /// <summary>
    /// 后台运行命令
    /// </summary>
    public const string BATCH_MODE = "-batchmode";

    /// <summary>
    /// 项目路径吗命令
    /// </summary>
    public const string PROJECT_PATH = "-projectPath";

    /// <summary>
    /// 日志文件命令
    /// </summary>
    public const string LOG_FILE = "-logFile";

    /// <summary>
    /// 执行方法命令
    /// </summary>
    public const string EXECUTE_METHOD = "-executeMethod";
    #endregion

    #region 自定义打包命令
    /// <summary>
    /// 打包平台命令
    /// </summary>
    public const string BUILD_TARGET = "-buildTarget";

    /// <summary>
    /// 开发模式命令
    /// </summary>
    public const string DEVELOPMENT_MODE = "-developmentMode";

    /// <summary>
    /// 打包渠道命令
    /// </summary>
    public const string CHANNEL = "-channel";

    /// <summary>
    /// 是否开发版本命令
    /// </summary>
    public const string IS_DEVELOPMENT = "-isDevelopment";

    /// <summary>
    /// 版本号命令
    /// </summary>
    public const string VERSION_CODE = "-versionCode";

    /// <summary>
    /// 资源版本号命令
    /// </summary>
    public const string RESOURCE_VERSION_CODE = "-resourceVersionCode";

    /// <summary>
    /// 重打AB命令
    /// </summary>
    public const string FORCE_REBUILD_AB = "-forceRebuildAB";

    /// <summary>
    /// SDK类型命令
    /// </summary>
    public const string SDK_TYPE = "-sdkType";
    #endregion
}