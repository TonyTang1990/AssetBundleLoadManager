/*
 * Description:             GlobalExceptionHandler.cs
 * Author:                  TONYTANG
 * Create Date:             2026//09/04
 */

using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// GlobalExceptionHandler.cs
/// 全局异常处理器
/// </summary>
public static class GlobalExceptionHandler
{
    /// <summary>
    /// 初始化全局异常处理器
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // 避免关闭Domain Reload时重复注册。
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>
    /// 监听线程中真正没有被捕获的异常。
    /// 通常触发后进程可能即将终止。
    /// </summary>
    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        var exception = args.ExceptionObject as Exception;

        if (exception != null)
        {
            Debug.LogError($"AppDomain未捕获异常:{exception}");
        }
        else
        {
            Debug.LogError($"AppDomain未捕获异常：{args.ExceptionObject}");
        }

        Debug.LogError($"运行时是否正在终止：{args.IsTerminating}");
    }

    /// <summary>
    /// 监听System.Threading.Tasks.Task中未被观察的异常。
    /// </summary>
    private static void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
    {
        if (args.Exception != null)
        {
            Debug.LogError($"未观察的Task异常:{args.Exception.Flatten()}");
        }

        // 告诉TaskScheduler异常已经被观察。
        args.SetObserved();
    }
}