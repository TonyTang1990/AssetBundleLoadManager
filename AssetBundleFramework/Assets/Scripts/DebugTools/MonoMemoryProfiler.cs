/*
 * Description:             MemoryProfiler.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/08
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

/// <summary>
/// MemoryProfiler.cs
/// 简陋的内存统计工具(统计托管的Mono内存)
/// </summary>
public class MonoMemoryProfiler : SingletonTemplate<MonoMemoryProfiler> {

    /// <summary>
    /// 内存Profile类型
    /// </summary>
    public enum MemoryProfilerType
    {
        CSharp_GC = 1,          // CS GC统计
        Unity_Profiler = 2      // Unity Profiler接口统计
    }

    /// <summary>
    /// 当前内存统计类型
    /// </summary>
    private MemoryProfilerType mCurrentMemoryProfilerType = MemoryProfilerType.Unity_Profiler;

    /// <summary>
    /// 内存标签名
    /// </summary>
    private string mTagName;
    
    /// <summary>
    /// 开始统计时总共使用的Mono内存
    /// </summary>
    private long mTotalUsedMonoMemory_Begin;

    /// <summary>
    /// 结束统计时总共使用的Mono内存
    /// </summary>
    private long mTotalUsedMonoMemory_End;

    /// <summary>
    /// 设置当前内存统计类型
    /// </summary>
    /// <param name="mpt"></param>
    public void setMemoryProfilerType(MemoryProfilerType mpt)
    {
        mCurrentMemoryProfilerType = mpt;
    }

    /// <summary>
    /// 开启内存统计Tag
    /// </summary>
    /// <param name="tag"></param>
    public void beginMemorySample(string tag)
    {
        if(!tag.IsNullOrEmpty())
        {
            mTagName = tag;
            if (mCurrentMemoryProfilerType == MemoryProfilerType.CSharp_GC)
            {
                // 确保得到正确的起始Heap Memory Size
                mTotalUsedMonoMemory_Begin = GC.GetTotalMemory(true);
            }
            else if (mCurrentMemoryProfilerType == MemoryProfilerType.Unity_Profiler)
            {
                GC.Collect();
                mTotalUsedMonoMemory_Begin = Profiler.GetMonoUsedSizeLong();
            }
        }
        else
        {
            Debug.LogError("MonoMemoryProfiler的Tag不能为空!");
        }
    }

    /// <summary>
    /// 结束内存统计
    /// </summary>
    public void endMemorySample()
    {
        if(!mTagName.IsNullOrEmpty())
        {
            if (mCurrentMemoryProfilerType == MemoryProfilerType.CSharp_GC)
            {
                mTotalUsedMonoMemory_End = GC.GetTotalMemory(false);
            }
            else if (mCurrentMemoryProfilerType == MemoryProfilerType.Unity_Profiler)
            {
                GC.Collect();
                mTotalUsedMonoMemory_End = Profiler.GetMonoUsedSizeLong();
            }

            var heapmemoryoffset = mTotalUsedMonoMemory_End - mTotalUsedMonoMemory_Begin;
            Debug.Log(string.Format("内存统计标签 : {0}", mTagName));
            Debug.Log(string.Format("当前Mono内存大小 = {0} Bytes", mTotalUsedMonoMemory_End));
            Debug.Log(string.Format("之前Mono内存大小 = {0} Bytes", mTotalUsedMonoMemory_Begin));
            Debug.Log(string.Format("总共Mono内存占用 = {0} Bytes == {1} KB == {2} M", heapmemoryoffset, heapmemoryoffset / 1024 , heapmemoryoffset / (1024 * 1024)));
            mTagName = string.Empty;
        }
    }
}