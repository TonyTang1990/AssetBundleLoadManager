/*
 * Description:             SingletonBase.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/14
 */

/// <summary>
/// SingletonBase.cs
/// 单例基类抽象
/// </summary>
public abstract class SingletonBase<T> : ISingleton where T : class, ISingleton
{
    /// <summary>
    /// 单例对象
    /// </summary>
    public static T Singleton => SingletonManager.Get<T>();

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// 释放
    /// </summary>
    public virtual void Shutdown()
    {
    }
}