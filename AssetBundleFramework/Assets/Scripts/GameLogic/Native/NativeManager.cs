/*
 * Description:             NativeManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/12
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NativeManager.cs
/// 原生接口管理单例类
/// </summary>
public abstract class NativeManager{

    /// <summary>
    /// 原生接口单例对象
    /// </summary>
    public static NativeManager Singleton
    {
        get
        {
            if (mNativeManagerSingleton == null)
            {
#if UNITY_ANDROID
                mNativeManagerSingleton = new AndroidNativeManager();
#elif UNITY_IOS
                mNativeManagerSingleton = new IOSNativeManager();
#elif UNITY_STANDALONE
                mNativeManagerSingleton = new PCNativeManager();
#endif
            }
            return mNativeManagerSingleton;
        }
    }
    private static NativeManager mNativeManagerSingleton;

    /// <summary>
    /// 初始化
    /// </summary>
    public abstract void init();

    /// <summary>
    /// 调用原生方法
    /// </summary>
    public abstract void callNativeMethod();
}