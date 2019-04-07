/*
 * Description:             ResourceModuleEnum.cs
 * Author:                  TONYTANG
 * Create Date:             2019//01/21
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源加载模式
/// </summary>
public enum ResourceLoadMode
{
    AssetBundle = 0,            // AssetBundle模式
    AssetDatabase = 1           // 编辑器AssetDatabase模式
}

/// <summary>
/// 资源加载方式
/// </summary>
public enum ResourceLoadMethod
{
    Sync = 1,          // 同步
    Async = 2          // 异步
}

/// <summary>
/// 资源加载类型
/// Note:
/// 已加载的资源加载类型允许更改，但只允许从低往高变(NormalLoad -> Preload -> PermanentLoad)，不允许从高往低(PermanentLoad -> Preload -> NormalLoad)
/// </summary>
public enum ResourceLoadType
{
    NormalLoad = 1,         // 正常加载(可通过Tick检测判定正常卸载)
    Preload = 2,            // 预加载(切场景才会卸载)
    PermanentLoad = 3,      // 永久加载(常驻内存永不卸载)
}

/// <summary>
/// 重写ResourceLoadType比较相关接口函数，避免ResourceLoadType作为Dictionary Key时，
/// 底层调用默认Equals(object obj)和DefaultCompare.GetHashCode()导致额外的堆内存分配
/// 参考:
/// http://gad.qq.com/program/translateview/7194373
/// </summary>
public class ResourceLoadTypeComparer : IEqualityComparer<ResourceLoadType>
{
    public bool Equals(ResourceLoadType x, ResourceLoadType y)
    {
        return x == y;
    }

    public int GetHashCode(ResourceLoadType x)
    {
        return (int)x;
    }
}

/// <summary>
/// 资源加载任务状态
/// </summary>
public enum ResourceLoadState
{
    None = 1,             // 未加载状态
    Waiting = 2,          // 等待加载状态
    Loading = 3,          // 加载中状态
    SelfComplete = 4,     // 自身加载完成状态
    AllComplete = 5,      // 自身以及依赖AB加载完成状态
    Error = 6             // 出错状态
}
