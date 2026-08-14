using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 只允许挂载在GameLauncher上的单例模板MonoBehaviour类
/// 通过抽象单例模板MonoBehaviour的初始化和释放方法
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonMonoTemplate<T> : MonoBehaviour where T : SingletonMonoTemplate<T>
{
    /// <summary>
    /// 单例对象
    /// </summary>
    private static T mInstance = null;

    /// <summary>
    /// 获取实例对象
    /// </summary>
    /// <returns></returns>
    public static T GetInstance()
    {
        return mInstance;
    }

    /// <summary>
    /// 设置实例对象
    /// </summary>
    /// <param name="t"></param>
    public void SetInstance(T t)
    {
        if(mInstance == null)
        {
            mInstance = t;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public virtual void Init()
    {
        return;
    }

    /// <summary>
    /// 移除释放
    /// </summary>
    public virtual void Release()
    {
        return;
    }
}
