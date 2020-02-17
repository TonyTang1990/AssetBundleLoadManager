using UnityEngine;
using System.Collections;

/// <summary>
/// 模板单例
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonTemplate<T> where T : class, new()
{
    public static T Singleton
    {
        get
        {
            if(mSingleton == null)
            {
                mSingleton = new T();
            }
            return mSingleton;
        }
    }
    protected static T mSingleton = null;

    protected SingletonTemplate()
    {

    }

    /// <summary>
    /// 提供一个方法触发第一次构造函数调用
    /// </summary>
    public void startUp()
    {

    }
}
