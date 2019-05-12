/*
 * Description:             IOSNativeManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/10
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_IOS
/// <summary>
/// IOSNativeManager.cs
/// IOS原生管理类
/// </summary>
public class IOSNativeManager : NativeManager {

    /// <summary>
    /// 初始化
    /// </summary>
    public override void init()
    {
        Debug.Log("IOSNativeManager:init()");    
    }

    /// <summary>
    /// 调用原生方法
    /// </summary>
    public override void callNativeMethod()
    {
        Debug.Log("IOSNativeManager:init()");
    }
}
#endif