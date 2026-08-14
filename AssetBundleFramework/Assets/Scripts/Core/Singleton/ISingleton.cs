/*
 * Description:             ISingleton.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/14
 */

/// <summary>
/// ISingleton.cs
/// 单例接口定义
/// </summary>
public interface ISingleton
{
    /// <summary>
    /// 初始化
    /// </summary>
    void Initialize();

    /// <summary>
    /// 释放
    /// </summary>
    void Shutdown();
}